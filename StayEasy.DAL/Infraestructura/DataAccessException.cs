using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace StayEasy.DAL.Infraestructura
{
    public class DataAccessException : Exception
    {
        public DataAccessException() { }
        public DataAccessException(string message) : base(message) { }
        public DataAccessException(string message, Exception innerException) : base(message, innerException) { }
    }
}
