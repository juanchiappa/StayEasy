using StayEasy.Seguridad.Entidades;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using System.Configuration;

namespace StayEasy.Seguridad
{
    internal class UsuarioSeguridadDAL
    {
        private readonly string _cadenaConexion = ConfigurationManager.ConnectionStrings["StayEasyDB"].ConnectionString;

        public Usuario Login(string nombreUsuario, byte[] passwordHash)
        {
            Usuario usuarioLogueado = null;

            using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
            {
                using (SqlCommand comando = new SqlCommand("sp_Login", conexion))
                {
                    comando.CommandType = CommandType.StoredProcedure;
                    comando.Parameters.AddWithValue("@NombreUsuario", nombreUsuario);
                    comando.Parameters.AddWithValue("@PasswordHash", passwordHash);

                    try
                    {
                        conexion.Open();
                        using (SqlDataReader reader = comando.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                usuarioLogueado = new Usuario(
                                    Convert.ToInt32(reader["UsuarioID"]),
                                    reader["NombreUsuario"].ToString(),
                                    new byte[0],
                                    reader["NombreCompleto"].ToString(),
                                    reader["Email"].ToString()
                                );
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        throw new Exception("Error al intentar conectar con la base de datos de seguridad.", ex);
                    }
                }
            }
            return usuarioLogueado;
        }

        public int RegistrarUsuario(Usuario nuevoUsuario)
        {
            int nuevoId = 0;

            using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
            {
                using (SqlCommand comando = new SqlCommand("sp_CrearUsuario", conexion))
                {
                    comando.CommandType = CommandType.StoredProcedure;
                    comando.Parameters.AddWithValue("@NombreUsuario", nuevoUsuario.NombreUsuario);
                    comando.Parameters.AddWithValue("@PasswordHash", nuevoUsuario.PasswordHash);
                    comando.Parameters.AddWithValue("@NombreCompleto", nuevoUsuario.NombreCompleto);
                    comando.Parameters.AddWithValue("@Email", nuevoUsuario.Email);

                    try
                    {
                        conexion.Open();
                        nuevoId = Convert.ToInt32(comando.ExecuteScalar());
                    }
                    catch (SqlException ex)
                    {
                        throw new Exception("Error al registrar el usuario en la base de datos.", ex);
                    }
                }
            }
            return nuevoId;
        }
    }
}
