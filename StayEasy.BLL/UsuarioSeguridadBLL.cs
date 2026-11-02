using StayEasy.MPP;       
using StayEasy.Seguridad; 
using StayEasy.Seguridad.Entidades;
using StayEasy.Seguridad.Validaciones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            nombreUsuario = nombreUsuario?.Trim() ?? string.Empty;
            nombreCompleto = nombreCompleto?.Trim() ?? string.Empty;
            email = email?.Trim() ?? string.Empty;

            ValidadorDatos.ValidarCuenta(nombreUsuario, passwordPlana, nombreCompleto, email);
            if (!ValidadorDatos.EsCorreoCorporativo(email))
                throw new ArgumentException("El personal interno debe utilizar un correo corporativo.");

            byte[] hash = Criptografia.HashearPassword(passwordPlana);

            Usuario nuevoUsuario = new Usuario(0, nombreUsuario, hash, nombreCompleto, email);

            _usuarioDAL.RegistrarUsuario(nuevoUsuario);
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
