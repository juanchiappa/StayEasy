using System;

namespace StayEasy.BE
{
    /// <summary>
    /// Modelo de lectura que devuelve sp_ListarHabitacionesDisponibles.
    /// No es una entidad persistida: es una habitacion mas el resultado de
    /// evaluarla contra un rango de fechas concreto. Alimenta la grilla de
    /// habitaciones de la pantalla de reservas.
    /// </summary>
    public class HabitacionDisponible
    {
        public HabitacionDisponible(Habitacion habitacion, DateTime fechaCheckIn,
                                    DateTime fechaCheckOut, int noches, decimal subtotalBase)
        {
            Habitacion     = habitacion;
            FechaCheckIn   = fechaCheckIn;
            FechaCheckOut  = fechaCheckOut;
            Noches         = noches;
            SubtotalBase   = subtotalBase;
        }

        public Habitacion Habitacion   { get; set; }
        public DateTime   FechaCheckIn { get; set; }
        public DateTime   FechaCheckOut{ get; set; }
        public int        Noches       { get; set; }

        /// <summary>
        /// PrecioBase * noches, tal cual lo calculo el SP. Sirve como referencia
        /// rapida; el precio que se cobra es <see cref="Subtotal"/>.
        /// </summary>
        public decimal SubtotalBase { get; set; }

        /// <summary>
        /// Subtotal real de alojamiento: sale del calculo polimorfico de la
        /// habitacion (tipo + nivel de servicio). Sin servicios adicionales.
        /// </summary>
        public decimal Subtotal => Habitacion.CalcularEstadia(Noches);

        public int    NumeroHabitacion => Habitacion.Numero;
        public string DescripcionTipo  => Habitacion.Descripcion;
        public decimal PrecioNoche     => Habitacion.CalcularPrecioNoche();

        public override string ToString()
            => $"{NumeroHabitacion} - {DescripcionTipo} - {PrecioNoche:C} / noche";
    }
}
