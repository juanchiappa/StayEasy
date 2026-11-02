using System;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace StayEasy.Seguridad.Validaciones
{
    public static class ValidadorDatos
    {
        public const string DominioCorporativo = "stayeasy.enterprise.com.ar";
        private static readonly TimeSpan TiempoLimite = TimeSpan.FromMilliseconds(250);
        private static readonly Regex Usuario = CrearRegex(@"\A[A-Za-z0-9._-]{3,50}\z");
        private static readonly Regex Email = CrearRegex(@"\A[^\s@]+@[^\s@]+\.[^\s@]+\z");
        private static readonly Regex Nombre = CrearRegex(@"\A\p{L}[\p{L}\p{M}]*(?:[ '\u2019-]\p{L}[\p{L}\p{M}]*)*\z");
        private static readonly Regex Dni = CrearRegex(@"\A[1-9][0-9]{6,7}\z");
        private static readonly Regex Token = CrearRegex(@"\A[0-9A-Fa-f]{6}\z");
        private static readonly Regex Telefono = CrearRegex(@"\A\+?[0-9 ()-]+\z");
        private static readonly Regex Mayuscula = CrearRegex("[A-Z]");
        private static readonly Regex Numero = CrearRegex("[0-9]");
        private static readonly Regex Importe = CrearRegex(@"\A[+-]?[0-9]+(?:,[0-9]{1,2})?\z");

        private static Regex CrearRegex(string patron) =>
            new Regex(patron, RegexOptions.CultureInvariant, TiempoLimite);

        private static bool Coincide(Regex regex, string? texto)
        {
            if (texto is null) return false;
            try { return regex.IsMatch(texto); }
            catch (RegexMatchTimeoutException) { return false; }
        }

        public static bool EsUsuarioValido(string? usuario) => Coincide(Usuario, usuario);

        public static bool EsEmailValido(string? email) =>
            !string.IsNullOrWhiteSpace(email) && email.Length <= 150
            && Coincide(Email, email)
            && MailAddress.TryCreate(email, out var direccion)
            && string.Equals(direccion.Address, email, StringComparison.OrdinalIgnoreCase);

        public static bool EsCorreoCorporativo(string? email)
        {
            string correo = email?.Trim() ?? string.Empty;
            return EsEmailValido(correo)
                && correo.EndsWith("@" + DominioCorporativo, StringComparison.OrdinalIgnoreCase);
        }

        public static bool EsNombreValido(string? nombre) =>
            !string.IsNullOrWhiteSpace(nombre) && nombre.Length <= 100 && Coincide(Nombre, nombre);

        public static bool EsDniValido(string? dni) =>
            Coincide(Dni, dni) && int.TryParse(dni, out int numero) && numero > 0;

        public static bool EsTokenValido(string? token) => Coincide(Token, token);

        public static bool EsTelefonoValido(string? telefono)
        {
            if (string.IsNullOrWhiteSpace(telefono)) return true;
            if (telefono.Length > 50 || !Coincide(Telefono, telefono)) return false;
            int digitos = telefono.Count(c => c >= '0' && c <= '9');
            return digitos >= 6 && digitos <= 15;
        }

        public static bool TieneMayuscula(string? password) => Coincide(Mayuscula, password);
        public static bool TieneNumero(string? password) => Coincide(Numero, password);

        public static bool CumplePoliticaPassword(string? password) =>
            !string.IsNullOrWhiteSpace(password) && password.Length >= 8
            && TieneMayuscula(password) && TieneNumero(password);

        public static void ValidarEmail(string? email)
        {
            if (!EsEmailValido(email))
                throw new ArgumentException("Ingresá un correo válido de hasta 150 caracteres.");
        }

        public static void ValidarPassword(string? password)
        {
            if (!CumplePoliticaPassword(password))
                throw new ArgumentException("La contraseña debe tener al menos 8 caracteres, una mayúscula y un número.");
        }

        public static void ValidarCuenta(string usuario, string password, string nombreCompleto, string email)
        {
            if (!EsUsuarioValido(usuario))
                throw new ArgumentException("El usuario debe tener entre 3 y 50 caracteres: letras sin tildes, números, punto, guion o guion bajo.");
            ValidarPassword(password);
            ValidarEmail(email);
            if (string.IsNullOrWhiteSpace(nombreCompleto) || nombreCompleto.Length > 150 || !Coincide(Nombre, nombreCompleto))
                throw new ArgumentException("Ingresá un nombre completo válido de hasta 150 caracteres.");
        }

        public static void ValidarDatosHuesped(string nombre, string apellido, string dni, string email, string? telefono)
        {
            if (!EsNombreValido(nombre))
                throw new ArgumentException("Ingresá un nombre válido de hasta 100 caracteres.");
            if (!EsNombreValido(apellido))
                throw new ArgumentException("Ingresá un apellido válido de hasta 100 caracteres.");
            if (!EsDniValido(dni))
                throw new ArgumentException("El DNI debe contener 7 u 8 dígitos, sin cero inicial, puntos ni espacios.");
            ValidarEmail(email);
            if (!EsTelefonoValido(telefono))
                throw new ArgumentException("El teléfono debe contener entre 6 y 15 dígitos; puede incluir + al comienzo, espacios, guiones y paréntesis.");
        }

        /// <summary>Importe completo en formato es-AR, sin separadores de miles.</summary>
        public static bool TryValidarSenia(string? texto, decimal total, out decimal senia, out string error)
        {
            error = string.Empty;
            senia = 0m;
            if (!Coincide(Importe, texto?.Trim())
                || !decimal.TryParse(texto?.Trim(), NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign,
                CultureInfo.GetCultureInfo("es-AR"), out senia))
                error = "Ingresá un importe válido con hasta dos decimales, sin separadores de miles.";
            else if (senia < 0m)
                error = "La seña no puede ser negativa.";
            else if (senia != decimal.Round(senia, 2))
                error = "El importe admite hasta dos decimales.";
            else if (senia > total)
                error = "La seña no puede superar el total.";
            return error.Length == 0;
        }
    }
}
