using StayEasy.Seguridad.Entidades;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using System.Configuration;

namespace StayEasy.MPP
{
    public class UsuarioSeguridadDAL
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["StayEasyDB"].ConnectionString;

        public Usuario Login(string nombreUsuario, byte[] passwordHash)
        {
            Usuario usuarioLogueado = null;

            using (SqlConnection conexion = new SqlConnection(_connectionString))
            {
                using (SqlCommand comando = new SqlCommand("sp_Login", conexion))
                {
                    comando.CommandType = CommandType.StoredProcedure;
                    comando.Parameters.AddWithValue("@NombreUsuario", nombreUsuario);
                    comando.Parameters.AddWithValue("@PasswordHash", passwordHash);
                    comando.Parameters.AddWithValue("@DireccionIP", "127.0.0.1");

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
                                    passwordHash,
                                    reader["NombreCompleto"].ToString(),
                                    "" 
                                );

                                usuarioLogueado.IdiomaPreferido = reader["IdiomaPreferido"].ToString();
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        if (ex.Number == 52000)
                        {
                            throw new UnauthorizedAccessException("Usuario o contraseña incorrectos.");
                        }

                        throw new Exception($"Error de base de datos: {ex.Message}", ex);
                    }
                }
            }
            return usuarioLogueado;
        }

        public void RegistrarUsuario(Usuario nuevoUsuario)
        {
            using (SqlConnection conexion = new SqlConnection(_connectionString))
            {
                using (SqlCommand comando = new SqlCommand("sp_RegistrarUsuario", conexion))
                {
                    comando.CommandType = CommandType.StoredProcedure;
                    comando.Parameters.AddWithValue("@NombreUsuario", nuevoUsuario.NombreUsuario);
                    comando.Parameters.AddWithValue("@PasswordHash", nuevoUsuario.PasswordHash);
                    comando.Parameters.AddWithValue("@NombreCompleto", nuevoUsuario.NombreCompleto);
                    comando.Parameters.AddWithValue("@Email", nuevoUsuario.Email);
                    comando.Parameters.AddWithValue("@IdiomaPreferido", nuevoUsuario.IdiomaPreferido ?? "ES");

                    try
                    {
                        conexion.Open();
                        comando.ExecuteNonQuery();
                    }
                    catch (SqlException ex)
                    {
                        if (ex.Number == 52002)
                        {
                            throw new Exception(ex.Message);
                        }
                        throw new Exception("Error al registrar el usuario en la base de datos.", ex);
                    }
                }
            }
        }
    }
}