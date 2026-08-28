using System;
using System.Collections.Generic;
using System.Text;

namespace StayEasy.BE
{
    public class Usuario
    {
        public int UsuarioID { get; set; }
        public string NombreUsuario { get; set; }
        public byte[] PasswordHash { get; set; } // SHA-256, nunca texto plano (RNF-03)
        public string NombreCompleto { get; set; }
        public string Email { get; set; }
        public string IdiomaPreferido { get; set; } = "ES";
        public bool Activo { get; set; } = true;
        public DateTime FechaCreacion { get; set; }
        public DateTime? UltimoLogin { get; set; }

        public Usuario(int usuarioID, string nombreUsuario, byte[] passwordHash, string nombreCompleto, string email)
        {
            UsuarioID = usuarioID;
            NombreUsuario = nombreUsuario;
            PasswordHash = passwordHash;
            NombreCompleto = nombreCompleto;
            Email = email;
            FechaCreacion = DateTime.Now;
        }
    }
}
