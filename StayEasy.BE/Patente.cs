using System;
using System.Collections.Generic;
using System.Text;

namespace StayEasy.BE
{
    public class Patente
    {
        public Patente(int patenteID, string nombre, string descripcion, bool esFamilia)
        {
            PatenteID = patenteID;
            Nombre = nombre;
            Descripcion = descripcion;
            EsFamilia = esFamilia;
        }

        public int PatenteID { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public bool EsFamilia { get; private set; }

    }
}
