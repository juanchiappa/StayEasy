using System;
using System.Collections.Generic;
using System.Text;

namespace StayEasy.BE
{
    internal class Habitacion
    {
        public Habitacion(int iD_habitacion, int numero, char tipoHabitacion, decimal precioBase, char nivelDeServicio, char estado)
        {
            ID_habitacion = iD_habitacion;
            Numero = numero;
            TipoHabitacion = tipoHabitacion;
            PrecioBase = precioBase;
            NivelDeServicio = nivelDeServicio;
            Estado = estado;
        }

        public int ID_habitacion { get; set; }
        public int Numero { get; set; }
        public char TipoHabitacion { get; set; }
        public decimal PrecioBase { get; set; }
        public char NivelDeServicio { get; set; }
        public char Estado{ get; set; }
    }
}
