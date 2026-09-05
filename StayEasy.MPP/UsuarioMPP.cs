using StayEasy.DAL;
using StayEasy.DAL.Registro;
using StayEasy.Seguridad.Entidades;
using System;
using System.Collections;
using System.Collections.Generic;
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
    }
}
