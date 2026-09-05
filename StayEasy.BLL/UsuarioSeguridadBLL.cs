using StayEasy.Seguridad.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StayEasy.Seguridad; 
using StayEasy.MPP;       

namespace StayEasy.BLL
{
    public class UsuarioSeguridadBLL
    {
        private readonly UsuarioSeguridadDAL _usuarioDAL = new UsuarioSeguridadDAL();
        private readonly UsuarioMPP _usuarioMPP = new UsuarioMPP();

        public void Login(string nombreUsuario, string passwordPlana)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario) || string.IsNullOrWhiteSpace(passwordPlana))
                throw new ArgumentException("El usuario y la contraseña son campos obligatorios.");

            byte[] hash = Criptografia.HashearPassword(passwordPlana);

            Usuario usuarioValido = _usuarioDAL.Login(nombreUsuario, hash);

            if (usuarioValido != null)
            {
                GestorSesion.Instancia.Iniciar(usuarioValido);
            }
            else
            {
                throw new UnauthorizedAccessException("Credenciales incorrectas o usuario inexistente.");
            }
        }

        public void Registrar(string nombreUsuario, string passwordPlana, string nombreCompleto, string email)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario) || string.IsNullOrWhiteSpace(passwordPlana))
                throw new ArgumentException("Faltan campos obligatorios para el registro.");

            byte[] hash = Criptografia.HashearPassword(passwordPlana);

            Usuario nuevoUsuario = new Usuario(0, nombreUsuario, hash, nombreCompleto, email);

            _usuarioMPP.RegistrarUsuario(nuevoUsuario);
        }

        public void Logout()
        {
            if (GestorSesion.Instancia.UsuarioLogueado != null)
            {
                // Limpia el Singleton, cerrando efectivamente la sesión
                GestorSesion.Instancia.Cerrar();
            }
        }
    }
}
