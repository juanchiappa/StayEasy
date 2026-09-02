using System;
using StayEasy.Seguridad.Entidades;

namespace StayEasy.Seguridad
{
    public class GestorSesion
    {
        private static GestorSesion _instancia;
        private static readonly object _lock = new object();

        public Usuario UsuarioLogueado { get; private set; }
        public DateTime FechaInicioSesion { get; private set; }

        private GestorSesion() { }

        public static GestorSesion Instancia
        {
            get
            {
                lock (_lock)
                {
                    if (_instancia == null) _instancia = new GestorSesion();
                    return _instancia;
                }
            }
        }

        public void Iniciar(Usuario usuario)
        {
            UsuarioLogueado = usuario;
            FechaInicioSesion = DateTime.Now;
        }

        public void Cerrar()
        {
            UsuarioLogueado = null;
        }
    }
}