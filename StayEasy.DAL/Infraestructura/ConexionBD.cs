using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using Microsoft.Data.SqlClient;

namespace StayEasy.DAL.Infraestructura
{
    internal static class ConexionBD
    {
        private static string _cadenaConexion => ConfigurationManager.ConnectionStrings["StayEasyDB"].ConnectionString;

        public static SqlConnection ObtenerConexion()
        {
            return new SqlConnection(_cadenaConexion);
        }
    }
}
