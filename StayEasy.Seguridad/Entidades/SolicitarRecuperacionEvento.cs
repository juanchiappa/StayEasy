using System;
using System.Collections.Generic;
using System.Text;

namespace StayEasy.Seguridad.Entidades
{
    public class SolicitarRecuperacionEvento
    {
        public string Email { get; set; }
    }

    public class EnviarCorreoEvento
    {
        public string Email { get; set; }
        public string Token { get; set; }
    }
    public class EjecutarRecuperacionEvento
    {
        public string Token { get; set; }
        public string NuevaClave { get; set; }
    }
}
