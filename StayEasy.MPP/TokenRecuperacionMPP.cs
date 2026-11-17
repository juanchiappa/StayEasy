using Microsoft.Data.SqlClient;
using StayEasy.DAL.Registro;
using StayEasy.Seguridad.Entidades;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace StayEasy.MPP
{
    public class TokenRecuperacionMPP
    {
        private readonly string _conexion = ConfigurationManager.ConnectionStrings["StayEasyDB"].ConnectionString;

        public void GuardarToken(Token token)
        {
            AccesoDatos dal = new AccesoDatos();
            Hashtable parametros = new Hashtable
            {
                { "@Email", token.Email },
                { "@Token", token.token },
                { "@Exp",   token.TiempoDeExpiracion }
            };

            token.ID = dal.EscribirEscalar("sp_GuardarToken", parametros);
        }

        public Token ObtenerTokenValido(string token)
        {
            Token entidad = null;
            using (SqlConnection conn = new SqlConnection(_conexion))
            {
                // Es recomendable traer también el Id en el SELECT
                string query = "SELECT Id, Email, Token, FechaExpiracion FROM TokensRecuperacion WHERE Token = @Token AND FechaExpiracion > @Now";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Token", token);
                    cmd.Parameters.AddWithValue("@Now", DateTime.Now);

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            entidad = new Token(
                                Convert.ToInt32(reader["Id"]),
                                reader["Email"].ToString()!,
                                reader["Token"].ToString()!,
                                Convert.ToDateTime(reader["FechaExpiracion"])
                            );
                        }
                    }
                }
            }
            return entidad!;
        }

        public void EliminarToken(string token)
        {
            using (SqlConnection conn = new SqlConnection(_conexion))
            {
                string query = "DELETE FROM TokensRecuperacion WHERE Token = @Token";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Token", token);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    
                }
            }
        }
        public void ActualizarContraseña(string email, byte[] nuevoPasswordHash)
        {
            AccesoDatos dal = new AccesoDatos();

            Hashtable parametros = new Hashtable();
            // Pasamos el identificador del usuario (Email) y el nuevo hash
            parametros.Add("@Email", email);
            parametros.Add("@PasswordHash", nuevoPasswordHash);

            // Llamamos al Stored Procedure correspondiente en la base de datos
            dal.Escribir("sp_ActualizarContrasena", parametros);
        }

    }
}
