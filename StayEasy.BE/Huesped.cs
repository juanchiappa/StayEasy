using System;
using System.Collections.Generic;
using System.Text;

namespace StayEasy.BE
{
    public class Huesped 
    {
        public Huesped(int huespedID, string nombre, string apellido, int dNI, string email, string telefono)
        {
            HuespedID = huespedID;
            Nombre = nombre;
            Apellido = apellido;
            DNI = dNI;
            Email = email;
            Telefono = telefono;
        }

        public int HuespedID { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public int DNI { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }

    }
}
