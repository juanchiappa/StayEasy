using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Microsoft.Data.SqlClient;
using StayEasy.BE;
using StayEasy.MPP;
using StayEasy.DAL.Infraestructura;

namespace StayEasy.DAL.Seguridad
{
    public class UsuarioDAL : RepositorioBase
    {
        public Usuario Login(string nombreUsuario, byte[] passwordHash, string direccionIP)
        {
            Usuario usuarioLogueado = null;

            using (SqlCommand comando = new SqlCommand("sp_Login", _conexion))
            {
                comando.CommandType = CommandType.StoredProcedure;
                comando.Parameters.AddWithValue("@NombreUsuario", nombreUsuario);
                comando.Parameters.AddWithValue("@PasswordHash", passwordHash);
                comando.Parameters.AddWithValue("@DireccionIP", string.IsNullOrEmpty(direccionIP) ? (object)DBNull.Value : direccionIP);

                try
                {
                    _conexion.Open();
                    using (SqlDataReader reader = comando.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            usuarioLogueado = UsuarioMapper.MapearDesdeReader(reader);
                        }
                    }
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 52000)
                    {
                        throw new DataAccessException("Credenciales incorrectas o usuario inactivo.", ex);
                    }
                    throw new DataAccessException("Error crítico de base de datos durante el inicio de sesión.", ex);
                }
                finally
                {
                    _conexion.Close();
                }
            }
            return usuarioLogueado; 
        }

        public int RegistrarUsuario(Usuario nuevoUsuario, int usuarioCreadorID)
        {
            int nuevoId = 0;

            using (SqlCommand comando = new SqlCommand("sp_CrearUsuario", _conexion)) 
            {
                comando.CommandType = CommandType.StoredProcedure;
                comando.Parameters.AddRange(UsuarioMapper.MapearParametrosRegistro(nuevoUsuario, usuarioCreadorID));
                try
                {
                    _conexion.Open();
                    object resultado = comando.ExecuteScalar();

                    if (resultado != null)
                    {
                        nuevoId = Convert.ToInt32(resultado);
                    }
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 52002)
                    {
                        throw new DataAccessException("El nombre de usuario ingresado ya existe en el sistema.", ex);
                    }
                    throw new DataAccessException("Error al intentar registrar el nuevo usuario.", ex);
                }
                finally
                {
                    _conexion.Close();
                }
            }
            return nuevoId;
        }
    }
}
