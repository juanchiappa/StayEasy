using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace StayEasy.DAL.Infraestructura
{
    public abstract class RepositorioBase : IDisposable
    {
        protected SqlConnection _conexion;

        protected RepositorioBase()
        {
            // Instancia la conexión leyendo el App.config de la UI
            string cadena = ConfigurationManager.ConnectionStrings["StayEasyDB"].ConnectionString;
            _conexion = new SqlConnection(cadena);
        }

        public void Dispose()
        {
            if (_conexion != null)
            {
                if (_conexion.State == System.Data.ConnectionState.Open)
                    _conexion.Close();

                _conexion.Dispose();
            }
        }
    }
}
