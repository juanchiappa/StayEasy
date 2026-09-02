using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayEasy.Seguridad.Entidades
{
    public abstract class PermisoBase
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public abstract bool TienePermiso(string permisoBuscado);
    }
}
