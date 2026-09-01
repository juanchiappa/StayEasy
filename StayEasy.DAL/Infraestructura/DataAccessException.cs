using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace StayEasy.DAL.Infraestructura
{
    internal class DataAccessException : Exception
    {
        public DataAccessException()
        {

        }

        public DataAccessException(string mensaje) : base(mensaje)
        {

        }

        public DataAccessException(string mensaje, Exception innerException) : base(mensaje, innerException)
        {
        }
    }
}
