using System;
using System.Collections.Generic;
using System.Text;

namespace StayEasy.BE
{
    public class Paquete
    {
        public Paquete(global::System.Int32 iD_Paquete, global::System.Int32 iD_Servicio)
        {
            ID_Paquete = iD_Paquete;
            ID_Servicio = iD_Servicio;
        }

        public int ID_Paquete { get; set; }
        public int ID_Servicio { get; set; }

    }
}
