using System;
using System.Collections.Generic;
using System.Text;

namespace StayEasy.BE
{
    public class serviciosPaquete 
    {
        public serviciosPaquete(int iD_Servicio, char nombre, decimal precio, bool esCombo)
        {
            ID_Servicio = iD_Servicio;
            Nombre = nombre;
            Precio = precio;
            EsCombo = esCombo;
        }

        public int ID_Servicio { get; set; }
        public char Nombre { get; set; }
        public decimal Precio { get; set; }
        public bool EsCombo { get; set; }

    }
}
