using Microsoft.Data.SqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Configuration;

namespace StayEasy.DAL.Registro
{
    public class AccesoDatos
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["StayEasyDB"].ConnectionString;

        public void Escribir(string storedProcedure, Hashtable parametros)
        {
            using (SqlConnection conexion = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(storedProcedure, conexion))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    if (parametros != null)
                    {
                        foreach (DictionaryEntry param in parametros)
                        {
                            cmd.Parameters.AddWithValue(param.Key.ToString(), param.Value);
                        }
                    }

                    try
                    {
                        conexion.Open();
                        cmd.ExecuteNonQuery();
                    }
                    catch (SqlException ex)
                    {
                        if (ex.Number >= 52000 && ex.Number <= 52999)
                        {
                            throw new Exception(ex.Message, ex);
                        }
                        throw new Exception($"Error de base de datos: {ex.Message}", ex);
                    }
                }
            }
        }

        public int EscribirEscalar(string storedProcedure, Hashtable parametros)
        {
            using (SqlConnection conexion = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(storedProcedure, conexion))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    if (parametros != null)
                    {
                        foreach (DictionaryEntry param in parametros)
                        {
                            cmd.Parameters.AddWithValue(param.Key.ToString(), param.Value);
                        }
                    }

                    try
                    {
                        conexion.Open();
                        object resultado = cmd.ExecuteScalar();
                        return Convert.ToInt32(resultado);
                    }
                    catch (SqlException ex)
                    {
                        if (ex.Number >= 52000 && ex.Number <= 52999)
                        {
                            throw new Exception(ex.Message, ex);
                        }
                        throw new Exception($"Error de base de datos: {ex.Message}", ex);
                    }
                }
            }
        }
        public DataTable Leer(string storedProcedure, Hashtable parametros)
        {
            using (SqlConnection conexion = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(storedProcedure, conexion))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Si hay parámetros, los agregamos al comando
                    if (parametros != null)
                    {
                        foreach (DictionaryEntry param in parametros)
                        {
                            cmd.Parameters.AddWithValue(param.Key.ToString(), param.Value);
                        }
                    }

                    try
                    {
                        // Usamos SqlDataAdapter para llenar el DataTable automáticamente
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        return dt;
                    }
                    catch (SqlException ex)
                    {
                        // Puedes adaptar el manejo de errores según tus códigos específicos
                        throw new Exception($"Error de base de datos en lectura: {ex.Message}", ex);
                    }
                }
            }
        }
    }
}
