using System;
using System.Collections;
using StayEasy.BE;
using StayEasy.DAL.Registro;

namespace StayEasy.MPP
{
    public class HuespedMPP
    {
        public int RegistrarUsuarioHuesped(Huesped huesped, string nombreUsuario, byte[] passwordHash)
        {
            AccesoDatos dal = new AccesoDatos();
            Hashtable parametros = new Hashtable();

            parametros.Add("@NombreUsuario", nombreUsuario);
            parametros.Add("@PasswordHash", passwordHash);
            parametros.Add("@Nombre", huesped.Nombre);
            parametros.Add("@Apellido", huesped.Apellido);
            parametros.Add("@DNI", huesped.DNI);
            parametros.Add("@Email", huesped.Email);
            parametros.Add("@Telefono", (object)huesped.Telefono ?? DBNull.Value);

            return dal.EscribirEscalar("sp_RegistrarUsuarioHuesped", parametros);
        }
    }
}