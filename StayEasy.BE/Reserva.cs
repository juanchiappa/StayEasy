using System;
using System.Collections.Generic;
using System.Text;

namespace StayEasy.BE
{
    public class Reserva
    {
        public Reserva(int iD_Reserva, Huesped iD_Huesped, Habitacion habitacion, DateTime fechaCheckIn, DateTime fechaChekOut, EstadoReserva estado, decimal total)
        {
            ID_Reserva = iD_Reserva;
            ID_Huesped = iD_Huesped;
            Habitacion = habitacion;
            FechaCheckIn = fechaCheckIn;
            FechaChekOut = fechaChekOut;
            Estado = estado;
            Total = total;
        }


        public int ID_Reserva { get; set; }
        public Huesped  ID_Huesped { get; set; }
        public Habitacion Habitacion { get; set; }
        public DateTime FechaCheckIn { get; set; }
        public DateTime FechaChekOut { get; set; }
        public EstadoReserva Estado { get; set; }
        public decimal Total { get; set; }

    }
}
