using System;
using StayEasy.BE;

namespace StayEasy.BLL
{
    public class GestorSesion
    {
        // Variable estática privada que guarda la única instancia
        private static GestorSesion _instancia;
        // Objeto de bloqueo para garantizar seguridad en entornos multihilo (Thread-safe)
        private static readonly object _lock = new object();

        // Propiedades de la sesión
        public Usuario UsuarioLogueado { get; private set; }
        public DateTime FechaInicioSesion { get; private set; }

        // Constructor privado para evitar que alguien haga "new GestorSesion()"
        private GestorSesion() { }

        // Propiedad pública para acceder a la instancia
        public static GestorSesion Instancia
        {
            get
            {
                lock (_lock)
                {
                    if (_instancia == null)
                    {
                        _instancia = new GestorSesion();
                    }
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