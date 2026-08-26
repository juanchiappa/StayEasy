using System;
using System.Collections.Generic;
using System.Text;

namespace StayEasy.BE
{
    public class Huesped
    {
        public Huesped(int iD_Huesped, char nombre, int dNI)
        {
            ID_Huesped = iD_Huesped;
            Nombre = nombre;
            DNI = dNI;
        }

        public int ID_Huesped { get; set; }
        public char Nombre { get; set; }
        public int DNI { get; set; }

    }
}
