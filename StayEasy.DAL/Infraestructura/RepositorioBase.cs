using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace StayEasy.DAL.Infraestructura
{
    public abstract class RepositorioBase : IDisposable
    {
        protected SqlConnection _conexion;

        protected RepositorioBase()
        {
            _conexion = ConexionBD.ObtenerConexion();
        }

        public void Dispose()
        {
            if (_conexion != null)
            {
                if (_conexion.State == System.Data.ConnectionState.Open)
                {
                    _conexion.Close();
                }
                _conexion.Dispose();
            }
        }
    }
}
