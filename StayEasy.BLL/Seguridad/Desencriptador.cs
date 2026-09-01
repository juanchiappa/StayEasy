using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoInsumos.BLL.Seguridad
{
    public class Desencriptador
    {
        private readonly Encriptador _encriptador = new Encriptador();

        public bool VerificarContrasena(string contrasenaIngresada, string hashGuardado)
        {
            string hashIngresado = _encriptador.HashearContrasena(contrasenaIngresada);
            return string.Equals(hashIngresado, hashGuardado, StringComparison.OrdinalIgnoreCase);
        }
    }
}