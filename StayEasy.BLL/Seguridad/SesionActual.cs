
using StayEasy.BE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoInsumos.BLL.Seguridad
{
    public class SesionActual
    {
        private static readonly Lazy<SesionActual> _instancia = new Lazy<SesionActual>(() => new SesionActual());

        private SesionActual() { }

        public static SesionActual Instancia => _instancia.Value;

        public Usuario UsuarioLogueado { get; set; }

        public void CerrarSesion()
        {
            UsuarioLogueado = null;
        }
    }
}