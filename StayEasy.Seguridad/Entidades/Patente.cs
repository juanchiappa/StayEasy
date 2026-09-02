using System;
using System.Collections.Generic;
using System.Text;

namespace StayEasy.Seguridad.Entidades
{
    public class Patente : PermisoBase
    {
        public override bool TienePermiso(string permisoBuscado)
        {
            return this.Nombre == permisoBuscado;
        }
    }
}
