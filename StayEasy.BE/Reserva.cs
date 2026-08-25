using System;
using System.Collections.Generic;
using System.Text;

namespace StayEasy.BE
{
    internal class Reserva
    {
        public Reserva(int iD_Reserva, Huesped iD_Huesped, Habitacion habitacion, Datetime fechaCheckIn, Datetime fechaChekOut, decimal total)
        {
            ID_Reserva = iD_Reserva;
            ID_Huesped = iD_Huesped;
            Habitacion = habitacion;
            FechaCheckIn = fechaCheckIn;
            FechaChekOut = fechaChekOut;
            Total = total;
        }

        public int ID_Reserva { get; set; }
        public Huesped  ID_Huesped { get; set; }
        public Habitacion Habitacion { get; set; }
        public Datetime FechaCheckIn { get; set; }
        public Datetime FechaChekOut { get; set; }
        public decimal Total { get; set; }


    }
}
