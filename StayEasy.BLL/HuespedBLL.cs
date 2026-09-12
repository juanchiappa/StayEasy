using System;
using StayEasy.BE;
using StayEasy.MPP;
using StayEasy.Seguridad;

namespace StayEasy.BLL
{
    public class HuespedBLL
    {
        private readonly HuespedMPP _huespedMPP = new HuespedMPP();
        public int RegistrarHuesped(string nombre, string apellido, int dni, string email,
                                     string telefono, string nombreUsuario, string passwordPlana)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario) || string.IsNullOrWhiteSpace(passwordPlana))
                throw new ArgumentException("El usuario y la contraseña son campos obligatorios.");

            if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(apellido))
                throw new ArgumentException("Nombre y apellido son obligatorios.");

            if (dni <= 0)
                throw new ArgumentException("El DNI ingresado no es válido.");

            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
                throw new ArgumentException("El email ingresado no es válido.");

            // Misma regla que ya aplica Registro.xaml.cs: los correos corporativos
            // se registran como personal interno, no como huésped autogestionado.
            if (email.EndsWith("@stayeasy.enterprise.com.ar", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Los correos corporativos deben registrarse como personal interno, no como huésped.");

            byte[] hash = Criptografia.HashearPassword(passwordPlana);

            // ID en 0: lo asigna la base de datos (IDENTITY) dentro del SP.
            Huesped nuevoHuesped = new Huesped(0, nombre, apellido, dni, email, telefono);

            return _huespedMPP.RegistrarUsuarioHuesped(nuevoHuesped, nombreUsuario, hash);
        }
    }
}