using System;
using System.Collections.Generic;
using System.Text;

namespace StayEasy.Seguridad.Entidades
{
    public class Token
    {
        public Token(int iD, string email, string token, DateTime tiempoDeExpiracion)
        {
            ID = iD;
            Email = email;
            this.token = token;
            TiempoDeExpiracion = tiempoDeExpiracion;
        }

        public int ID { get; set; }
        public string Email { get; set; }
        public string token { get; set; }
        public DateTime TiempoDeExpiracion { get; set; }

    }
}
