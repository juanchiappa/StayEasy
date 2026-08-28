using System;
using System.Collections.Generic;
using System.Text;

namespace StayEasy.BE
{
    public class ServicioLimpieza
    {
        public int AlertaID { get; set; }
        public int HabitacionID { get; set; }
        public DateTime FechaHora { get; set; }
        public string Prioridad { get; set; } // "Normal", "Urgente"
        public bool Atendida { get; set; }
        public int? UsuarioAtendioID { get; set; }

     

        public ServicioLimpieza(int alertaID, int habitacionID, DateTime fechaHora, string prioridad, bool atendida, int? usuarioAtendioID)
        {
            AlertaID = alertaID;
            HabitacionID = habitacionID;
            FechaHora = fechaHora;
            Prioridad = prioridad;
            Atendida = atendida;
            UsuarioAtendioID = usuarioAtendioID;
        }
    }
}
