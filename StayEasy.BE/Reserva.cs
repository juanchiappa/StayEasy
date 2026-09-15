using System;

namespace StayEasy.BE
{
    public class Reserva
    {
        public Reserva() { }

        public Reserva(int iD_Reserva, Huesped iD_Huesped, Habitacion habitacion,
                       DateTime fechaCheckIn, DateTime fechaCheckOut,
                       EstadoReserva estado, decimal total)
        {
            ID_Reserva    = iD_Reserva;
            ID_Huesped    = iD_Huesped;
            Habitacion    = habitacion;
            FechaCheckIn  = fechaCheckIn;
            FechaCheckOut = fechaCheckOut;
            Estado        = estado;
            Total         = total;
        }

        public int        ID_Reserva { get; set; }
        public Huesped    ID_Huesped { get; set; } = null!;
        public Habitacion Habitacion { get; set; } = null!;

        public DateTime FechaCheckIn  { get; set; }
        public DateTime FechaCheckOut { get; set; }

        /// <summary>
        /// Alias de compatibilidad con el nombre anterior (mal escrito).
        /// Se mantiene para no romper codigo de las otras ramas; migrar a
        /// FechaCheckOut y despues borrar esta propiedad.
        /// </summary>
        [Obsolete("Usar FechaCheckOut. Este alias se elimina cuando todas las ramas esten migradas.")]
        public DateTime FechaChekOut
        {
            get => FechaCheckOut;
            set => FechaCheckOut = value;
        }

        public EstadoReserva Estado { get; set; } = EstadoReserva.Confirmada;
        public decimal       Total  { get; set; }

        /// <summary>Noches facturables del rango [CheckIn, CheckOut).</summary>
        public int Noches => (int)(FechaCheckOut.Date - FechaCheckIn.Date).TotalDays;

        // --- Reglas de la maquina de estados (espejo de los SP) ---
        public bool SePuedeCancelar    => Estado == EstadoReserva.Confirmada;
        public bool SePuedeHacerCheckIn  => Estado == EstadoReserva.Confirmada
                                            && FechaCheckIn.Date <= DateTime.Today;
        public bool SePuedeHacerCheckOut => Estado == EstadoReserva.EnCurso;
        public bool EstaViva => Estado == EstadoReserva.Confirmada || Estado == EstadoReserva.EnCurso;

        /// <summary>
        /// Solapamiento de rangos semiabiertos, misma regla que
        /// fn_ReservasEnConflicto en la base. Sirve para validar en memoria
        /// antes de ir a la BD.
        /// </summary>
        public bool SeSolapaCon(DateTime checkIn, DateTime checkOut)
            => FechaCheckIn.Date < checkOut.Date && FechaCheckOut.Date > checkIn.Date;

        public override string ToString()
            => $"Reserva #{ID_Reserva} - {Estado} - {FechaCheckIn:dd/MM/yyyy} al {FechaCheckOut:dd/MM/yyyy}";
    }
}
