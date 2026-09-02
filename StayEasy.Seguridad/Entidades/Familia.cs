using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayEasy.Seguridad.Entidades
{
    public class Familia : PermisoBase
    {
        public List<PermisoBase> Hijos { get; set; } = new List<PermisoBase>();

        public override bool TienePermiso(string permisoBuscado)
        {
            if (this.Nombre == permisoBuscado) return true;
            foreach (var hijo in Hijos)
            {
                if (hijo.TienePermiso(permisoBuscado)) return true;
            }
            return false;
        }
    }
}
