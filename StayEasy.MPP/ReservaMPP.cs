using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using StayEasy.BE;
using StayEasy.DAL.Registro;

namespace StayEasy.MPP
{
    /// <summary>
    /// Persistencia de reservas. Todas las operaciones que cambian estado van
    /// por stored procedures transaccionales: la validacion de disponibilidad,
    /// el cambio de estado de la habitacion y la bitacora viajan juntos.
    /// </summary>
    public class ReservaMPP
    {
        private readonly AccesoDatos _dal = new AccesoDatos();

        // ------------------------------------------------------------------
        // Escritura
        // ------------------------------------------------------------------

        /// <summary>
        /// Alta de reserva. La base valida que no haya solapamiento de fechas
        /// (error 52010) y devuelve el ID_Reserva nuevo.
        /// La habitacion NO queda ocupada aca: eso pasa en el check-in.
        /// </summary>
        public int RegistrarReserva(Reserva nuevaReserva, int idUsuarioAccion)
        {
            var parametros = new Hashtable
            {
                { "@HuespedID",       nuevaReserva.ID_Huesped.HuespedID },
                { "@HabitacionID",    nuevaReserva.Habitacion.ID_habitacion },
                { "@FechaCheckIn",    nuevaReserva.FechaCheckIn.Date },
                { "@FechaCheckOut",   nuevaReserva.FechaCheckOut.Date },
                { "@Total",           nuevaReserva.Total },
                { "@UsuarioAccionID", idUsuarioAccion }
            };

            return _dal.EscribirEscalar("sp_RegistrarReserva", parametros);
        }

        /// <summary>
        /// Confirmada -> EnCurso. Marca la habitacion como ocupada ('O').
        /// </summary>
        public void RegistrarCheckIn(int idReserva, int idUsuarioAccion)
        {
            _dal.Escribir("sp_RegistrarCheckIn", new Hashtable
            {
                { "@ReservaID",       idReserva },
                { "@UsuarioAccionID", idUsuarioAccion }
            });
        }

        /// <summary>
        /// EnCurso -> Finalizada. Deja la habitacion en limpieza ('L') y
        /// genera la alerta al personal (Observer).
        /// </summary>
        public void RegistrarCheckOut(int idReserva, int idUsuarioAccion)
        {
            _dal.Escribir("sp_RegistrarCheckOut", new Hashtable
            {
                { "@ReservaID",       idReserva },
                { "@UsuarioAccionID", idUsuarioAccion }
            });
        }

        /// <summary>
        /// Confirmada -> Cancelada. Al cancelar, el rango de fechas queda
        /// liberado automaticamente para otra reserva.
        /// </summary>
        public void Cancelar(int idReserva, int idUsuarioAccion, string? motivo = null)
        {
            _dal.Escribir("sp_CancelarReserva", new Hashtable
            {
                { "@ReservaID",       idReserva },
                { "@UsuarioAccionID", idUsuarioAccion },
                { "@Motivo",          Mapeo.Parametro(motivo) }
            });
        }

        // ------------------------------------------------------------------
        // Lectura
        // ------------------------------------------------------------------

        /// <summary>
        /// Reservas que tocan la ventana [desde, hasta). Todos los filtros son
        /// opcionales: null significa "no filtrar por eso".
        /// El SP trae huesped y habitacion en el mismo SELECT, asi que no hay
        /// una consulta extra por fila.
        /// </summary>
        public List<Reserva> Listar(DateTime? desde = null, DateTime? hasta = null,
                                    EstadoReserva? estado = null,
                                    int? idHuesped = null, int? idHabitacion = null)
        {
            if (desde.HasValue && hasta.HasValue && hasta.Value.Date <= desde.Value.Date)
                throw new ArgumentException("Rango de fechas invalido: 'hasta' debe ser posterior a 'desde'.");

            var parametros = new Hashtable
            {
                { "@ReservaID",    DBNull.Value },
                { "@Desde",        Mapeo.Parametro(desde) },
                { "@Hasta",        Mapeo.Parametro(hasta) },
                { "@Estado",       Mapeo.Parametro(estado) },
                { "@HuespedID",    Mapeo.Parametro(idHuesped) },
                { "@HabitacionID", Mapeo.Parametro(idHabitacion) }
            };

            DataTable tabla = _dal.Leer("sp_ListarReservas", parametros);

            var reservas = new List<Reserva>();
            foreach (DataRow fila in tabla.Rows)
                reservas.Add(MapearReserva(fila));

            return reservas;
        }

        /// <summary>
        /// Devuelve la reserva con su huesped y su habitacion, o null si no existe.
        /// </summary>
        public Reserva? Obtener(int idReserva)
        {
            var parametros = new Hashtable { { "@ReservaID", idReserva } };

            DataTable tabla = _dal.Leer("sp_ObtenerReserva", parametros);

            return tabla.Rows.Count == 0 ? null : MapearReserva(tabla.Rows[0]);
        }

        /// <summary>
        /// Arma el grafo completo reutilizando los mappers de las otras
        /// entidades. Depende de los alias del SP: EstadoReserva para el
        /// estado de la reserva y EstadoFisico para el de la habitacion.
        /// </summary>
        internal static Reserva MapearReserva(DataRow fila)
        {
            return new Reserva
            {
                ID_Reserva    = Mapeo.Entero (fila, "ID_Reserva"),
                ID_Huesped    = HuespedMPP.MapearHuesped(fila),
                Habitacion    = HabitacionMPP.MapearHabitacion(fila),
                FechaCheckIn  = Mapeo.Fecha  (fila, "FechaCheckIn"),
                FechaCheckOut = Mapeo.Fecha  (fila, "FechaCheckOut"),
                Estado        = Mapeo.Estado (fila, "EstadoReserva"),
                Total         = Mapeo.Decimal(fila, "Total")
            };
        }
    }
}
