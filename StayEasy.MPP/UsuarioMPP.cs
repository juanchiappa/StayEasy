using StayEasy.DAL;
using StayEasy.DAL.Registro;
using StayEasy.Seguridad.Entidades;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks.Dataflow;

namespace StayEasy.MPP
{
    public class UsuarioMPP
    {
        public void RegistrarUsuario(Usuario nuevoUsuario)
        {
            AccesoDatos dal = new AccesoDatos();

            Hashtable parametros = new Hashtable();
            parametros.Add("@NombreUsuario", nuevoUsuario.NombreUsuario);
            parametros.Add("@PasswordHash", nuevoUsuario.PasswordHash);
            parametros.Add("@NombreCompleto", nuevoUsuario.NombreCompleto);
            parametros.Add("@Email", nuevoUsuario.Email);
            parametros.Add("@IdiomaPreferido", nuevoUsuario.IdiomaPreferido ?? "ES");

            dal.Escribir("sp_RegistrarUsuario", parametros);
        }
        public bool ValidarEmailExistente(string email)
        {
            AccesoDatos dal = new AccesoDatos();
            Hashtable parametros = new Hashtable();

            parametros.Add("@Email", email);

            // Supongo que tienes un método 'Leer' que devuelve un DataTable.
            // Si devuelve un DataSet, sería dt.Tables[0].Rows.Count
            DataTable dt = dal.Leer("sp_ValidarEmail", parametros);

            // Si la tabla tiene al menos una fila, significa que el correo existe en la base de datos
            if (dt != null && dt.Rows.Count > 0)
            {
                return true;
            }

            return false;
        }
    }
}
