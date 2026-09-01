using Microsoft.Data.SqlClient;
using StayEasy.BE;
using System;
using System.Collections.Generic;
using System.Text;

namespace StayEasy.MPP
{
    public static class UsuarioMapper
    {
        public static Usuario MapearDesdeReader(SqlDataReader reader)
        {
            Usuario usuario = new Usuario(
                Convert.ToInt32(reader["UsuarioID"]),
                reader["NombreUsuario"].ToString(),
                new byte[0],
                reader["NombreCompleto"].ToString(),
                string.Empty
                );

            if (reader["IdiomaPreferido"] != DBNull.Value)
            {
                usuario.IdiomaPreferido = reader["IdiomaPreferido"].ToString();
            }
            return usuario;
        }

        public static SqlParameter[] MapearParametrosRegistro(Usuario usuario, int usuarioCreadorID)
        {
            return new SqlParameter[]
            {
                new SqlParameter("@NombreUsuario", usuario.NombreUsuario),
                new SqlParameter("@PasswordHash", usuario.PasswordHash),
                new SqlParameter("@NombreCompleto", usuario.NombreCompleto),
                new SqlParameter("@Email", usuario.Email),
                new SqlParameter("@UsuarioCreadorID", usuarioCreadorID)
            };
        }
    }
}
