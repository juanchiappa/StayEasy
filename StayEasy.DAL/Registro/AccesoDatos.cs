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
                        if (ex.Number == 52002)
                        {
                            throw new Exception(ex.Message);
                        }
                        throw new Exception($"Error de base de datos: {ex.Message}", ex);
                    }
                }
            }
        }
    }
}
