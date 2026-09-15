using System;
using System.Collections.Generic;
using System.Linq;
using StayEasy.BE;
using StayEasy.MPP;
using StayEasy.Seguridad;

namespace StayEasy.BLL
{
    /// <summary>
    /// Reglas de negocio del ciclo de vida de una reserva.
    ///
    /// Division de responsabilidades:
    ///   - Esta capa valida lo que se puede validar en memoria y arma el precio.
    ///   - La base valida lo que depende de datos concurrentes (solapamiento de
    ///     fechas, estado real de la reserva) dentro de la transaccion.
    /// Las dos validaciones son necesarias: la de aca da mensajes inmediatos y
    /// evita viajes inutiles, la de la base es la que realmente garantiza la
    /// consistencia si hay dos recepcionistas trabajando a la vez.
    /// </summary>
    public class ReservaBLL
    {
        private readonly ReservaMPP    _reservaMPP    = new ReservaMPP();
        private readonly HabitacionMPP _habitacionMPP = new HabitacionMPP();
        private readonly HuespedMPP    _huespedMPP    = new HuespedMPP();

        /// <summary>Tope de estadia, para atajar errores de tipeo en las fechas.</summary>
        public const int MAXIMO_NOCHES = 90;

        // ------------------------------------------------------------------
        // Busqueda
        // ------------------------------------------------------------------

        /// <summary>
        /// Habitaciones vendibles para el rango pedido. Alimenta la grilla de
        /// la pantalla de reservas.
        /// </summary>
        public List<HabitacionDisponible> BuscarDisponibilidad(DateTime checkIn, DateTime checkOut,
                                                               char? tipoHabitacion = null)
        {
            ValidarRango(checkIn, checkOut);
            return _habitacionMPP.ListarDisponibles(checkIn, checkOut, tipoHabitacion);
        }

        /// <summary>Buscador de huesped por nombre, apellido, email o DNI.</summary>
        public List<Huesped> BuscarHuesped(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto) || texto.Trim().Length < 2)
                throw new ArgumentException("Ingresa al menos dos caracteres para buscar.");

            return _huespedMPP.Buscar(texto.Trim());
        }

        // ------------------------------------------------------------------
        // Precio
        // ------------------------------------------------------------------

        /// <summary>
        /// Total de la reserva: alojamiento (calculo polimorfico de la
        /// habitacion segun tipo y nivel de servicio) mas los servicios
        /// adicionales elegidos.
        ///
        /// Punto de extension: cuando entre el Composite de servicios, este
        /// parametro pasa a ser una coleccion de ServicioHotel y aca se llama
        /// a CalcularPrecio() de cada uno.
        /// </summary>
        public decimal CalcularTotal(HabitacionDisponible seleccion,
                                     IEnumerable<decimal>? serviciosAdicionales = null)
        {
            ArgumentNullException.ThrowIfNull(seleccion);

            decimal alojamiento = seleccion.Subtotal;
            decimal servicios   = serviciosAdicionales?.Sum() ?? 0m;

            if (servicios < 0m)
                throw new ArgumentException("Los servicios adicionales no pueden ser negativos.");

            return decimal.Round(alojamiento + servicios, 2);
        }

        // ------------------------------------------------------------------
        // Alta
        // ------------------------------------------------------------------

        /// <summary>
        /// Registra la reserva y devuelve el ID nuevo.
        /// Si otra persona tomo la habitacion entre la busqueda y la
        /// confirmacion, la base rechaza con el error 52010 y ese mensaje es el
        /// que hay que mostrarle al usuario.
        /// </summary>
        public int RegistrarReserva(Huesped huesped, HabitacionDisponible seleccion,
                                    IEnumerable<decimal>? serviciosAdicionales = null)
        {
            ArgumentNullException.ThrowIfNull(huesped);
            ArgumentNullException.ThrowIfNull(seleccion);

            if (huesped.HuespedID <= 0)
                throw new ArgumentException("Elegi un huesped valido antes de confirmar la reserva.");

            ValidarRango(seleccion.FechaCheckIn, seleccion.FechaCheckOut);

            decimal total = CalcularTotal(seleccion, serviciosAdicionales);

            var nuevaReserva = new Reserva
            {
                ID_Huesped    = huesped,
                Habitacion    = seleccion.Habitacion,
                FechaCheckIn  = seleccion.FechaCheckIn,
                FechaCheckOut = seleccion.FechaCheckOut,
                Estado        = EstadoReserva.Confirmada,
                Total         = total
            };

            return _reservaMPP.RegistrarReserva(nuevaReserva, UsuarioDeLaSesion());
        }

        // ------------------------------------------------------------------
        // Maquina de estados
        // ------------------------------------------------------------------

        /// <summary>Confirmada -> EnCurso. La habitacion pasa a ocupada.</summary>
        public void CheckIn(int idReserva)
        {
            Reserva reserva = ObtenerReserva(idReserva);

            if (reserva.Estado != EstadoReserva.Confirmada)
                throw new InvalidOperationException(
                    $"Solo se puede hacer check-in de una reserva confirmada. Esta reserva esta {reserva.Estado}.");

            if (reserva.FechaCheckIn.Date > DateTime.Today)
                throw new InvalidOperationException(
                    $"El ingreso de esta reserva es el {reserva.FechaCheckIn:dd/MM/yyyy}.");

            _reservaMPP.RegistrarCheckIn(idReserva, UsuarioDeLaSesion());
        }

        /// <summary>
        /// EnCurso -> Finalizada. La habitacion queda en limpieza y se genera
        /// la alerta al personal.
        /// </summary>
        public void CheckOut(int idReserva)
        {
            Reserva reserva = ObtenerReserva(idReserva);

            if (reserva.Estado != EstadoReserva.EnCurso)
                throw new InvalidOperationException(
                    $"Solo se puede hacer check-out de una reserva en curso. Esta reserva esta {reserva.Estado}.");

            _reservaMPP.RegistrarCheckOut(idReserva, UsuarioDeLaSesion());
        }

        /// <summary>Confirmada -> Cancelada. Libera el rango de fechas.</summary>
        public void Cancelar(int idReserva, string? motivo = null)
        {
            Reserva reserva = ObtenerReserva(idReserva);

            if (!reserva.SePuedeCancelar)
                throw new InvalidOperationException(
                    $"No se puede cancelar una reserva {reserva.Estado}.");

            _reservaMPP.Cancelar(idReserva, UsuarioDeLaSesion(), motivo);
        }

        // ------------------------------------------------------------------
        // Consultas
        // ------------------------------------------------------------------

        public List<Reserva> Listar(DateTime? desde = null, DateTime? hasta = null,
                                    EstadoReserva? estado = null,
                                    int? idHuesped = null, int? idHabitacion = null)
            => _reservaMPP.Listar(desde, hasta, estado, idHuesped, idHabitacion);

        /// <summary>Devuelve la reserva o lanza si no existe.</summary>
        public Reserva ObtenerReserva(int idReserva)
        {
            if (idReserva <= 0)
                throw new ArgumentException("Identificador de reserva invalido.", nameof(idReserva));

            return _reservaMPP.Obtener(idReserva)
                   ?? throw new InvalidOperationException($"No existe la reserva #{idReserva}.");
        }

        /// <summary>Check-ins pendientes de hoy. Para el tablero de recepcion.</summary>
        public List<Reserva> LlegadasDeHoy()
            => Listar(estado: EstadoReserva.Confirmada)
               .Where(r => r.FechaCheckIn.Date == DateTime.Today)
               .ToList();

        /// <summary>Check-outs previstos para hoy.</summary>
        public List<Reserva> SalidasDeHoy()
            => Listar(estado: EstadoReserva.EnCurso)
               .Where(r => r.FechaCheckOut.Date == DateTime.Today)
               .ToList();

        // ------------------------------------------------------------------
        // Internos
        // ------------------------------------------------------------------

        private static void ValidarRango(DateTime checkIn, DateTime checkOut)
        {
            if (checkOut.Date <= checkIn.Date)
                throw new ArgumentException("La fecha de check-out debe ser posterior a la de check-in.");

            if (checkIn.Date < DateTime.Today)
                throw new ArgumentException("No se pueden registrar reservas con fecha de ingreso pasada.");

            int noches = (int)(checkOut.Date - checkIn.Date).TotalDays;
            if (noches > MAXIMO_NOCHES)
                throw new ArgumentException($"La estadia no puede superar las {MAXIMO_NOCHES} noches.");
        }

        /// <summary>
        /// Usuario que queda registrado en la bitacora de cada operacion.
        /// Sale del Singleton de sesion.
        /// </summary>
        private static int UsuarioDeLaSesion()
        {
            var usuario = GestorSesion.Instancia.UsuarioLogueado;

            if (usuario is null)
                throw new InvalidOperationException("No hay una sesion activa. Volve a iniciar sesion.");

            return usuario.UsuarioID;
        }
    }
}
