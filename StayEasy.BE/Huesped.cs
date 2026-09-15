using System;

namespace StayEasy.BE
{
    public class Huesped
    {
        public Huesped() { }

        public Huesped(int huespedID, string nombre, string apellido, int dNI,
                       string? email, string? telefono)
        {
            HuespedID = huespedID;
            Nombre    = nombre;
            Apellido  = apellido;
            DNI       = dNI;
            Email     = email;
            Telefono  = telefono;
        }

        public int     HuespedID { get; set; }
        public string  Nombre    { get; set; } = string.Empty;
        public string  Apellido  { get; set; } = string.Empty;
        public int     DNI       { get; set; }
        public string? Email     { get; set; }
        public string? Telefono  { get; set; }

        /// <summary>Usuario de seguridad enlazado, si el huesped se autorregistro.</summary>
        public int? UsuarioID { get; set; }

        public string NombreCompleto => $"{Nombre} {Apellido}".Trim();

        /// <summary>Iniciales para el avatar de la UI.</summary>
        public string Iniciales
        {
            get
            {
                char inicialNombre   = Nombre.Length   > 0 ? char.ToUpperInvariant(Nombre[0])   : ' ';
                char inicialApellido = Apellido.Length > 0 ? char.ToUpperInvariant(Apellido[0]) : ' ';
                return $"{inicialNombre}{inicialApellido}".Trim();
            }
        }

        public override string ToString() => $"{NombreCompleto} (DNI {DNI})";
    }
}
