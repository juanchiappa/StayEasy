using System;
using System.Collections.Generic;
using System.Text;

namespace StayEasy.BE
{
    public class Paquete
    {
        public Paquete(int iD_Paquete, int iD_Servicio,int cantidad,decimal preciounitario)
        {
            ID_Paquete = iD_Paquete;
            ID_Servicio = iD_Servicio; 
            Cantidad = cantidad;
            PrecioUnitario = preciounitario;
            
        }

        public Paquete() { }
        public int ID_Paquete { get; set; }
        public int ID_Servicio { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }

        public decimal Subtotal => Cantidad * PrecioUnitario;

    }
}
