using System;
using StayEasy.BE;
using StayEasy.MPP;
using StayEasy.Seguridad;
using StayEasy.Seguridad.Validaciones;

namespace StayEasy.BLL
{
    public class HuespedBLL
    {
        private readonly HuespedMPP _huespedMPP = new HuespedMPP();
        public int RegistrarHuesped(string nombre, string apellido, int dni, string email,
                                     string? telefono, string nombreUsuario, string passwordPlana)
        {
            nombre = nombre?.Trim() ?? string.Empty;
            apellido = apellido?.Trim() ?? string.Empty;
            email = email?.Trim() ?? string.Empty;
            nombreUsuario = nombreUsuario?.Trim() ?? string.Empty;
            telefono = string.IsNullOrWhiteSpace(telefono) ? null : telefono.Trim();

            ValidadorDatos.ValidarDatosHuesped(nombre, apellido,
                dni.ToString(System.Globalization.CultureInfo.InvariantCulture), email, telefono);
            ValidadorDatos.ValidarCuenta(nombreUsuario, passwordPlana, $"{nombre} {apellido}", email);

            if (ValidadorDatos.EsCorreoCorporativo(email))
                throw new ArgumentException("Los correos corporativos deben registrarse como personal interno, no como huésped.");

            byte[] hash = Criptografia.HashearPassword(passwordPlana);

            // ID en 0: lo asigna la base de datos (IDENTITY) dentro del SP.
            Huesped nuevoHuesped = new Huesped(0, nombre, apellido, dni, email, telefono);

            return _huespedMPP.RegistrarUsuarioHuesped(nuevoHuesped, nombreUsuario, hash);
        }
    }
}
