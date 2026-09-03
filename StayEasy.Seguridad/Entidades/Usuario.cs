using System;
using System.Collections.Generic;
using System.Text;

namespace StayEasy.Seguridad.Entidades
{
    public class Usuario
    {
        public int UsuarioID { get; set; }
        public string NombreUsuario { get; set; }
        public byte[] PasswordHash { get; set; }
        public string NombreCompleto { get; set; }
        public string Email { get; set; }
        public string IdiomaPreferido { get; set; }
        public PermisoBase Permisos { get; set; } // Enlace con el Composite

        public Usuario(int id, string usuario, byte[] hash, string nombre, string email)
        {
            UsuarioID = id;
            NombreUsuario = usuario;
            PasswordHash = hash;
            NombreCompleto = nombre;
            Email = email;
        }
    }
}
