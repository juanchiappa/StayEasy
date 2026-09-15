using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using StayEasy.BE;
using StayEasy.DAL.Registro;

namespace StayEasy.MPP
{
    public class HuespedMPP
    {
        private readonly AccesoDatos _dal = new AccesoDatos();

        /// <summary>
        /// Autorregistro: crea Usuario + Patente + Huesped en una sola
        /// transaccion (sp_RegistrarUsuarioHuesped) y devuelve el HuespedID.
        /// </summary>
        public int RegistrarUsuarioHuesped(Huesped huesped, string nombreUsuario, byte[] passwordHash)
        {
            var parametros = new Hashtable
            {
                { "@NombreUsuario", nombreUsuario },
                { "@PasswordHash",  passwordHash },
                { "@Nombre",        huesped.Nombre },
                { "@Apellido",      huesped.Apellido },
                { "@DNI",           huesped.DNI },
                { "@Email",         Mapeo.Parametro(huesped.Email) },
                { "@Telefono",      Mapeo.Parametro(huesped.Telefono) }
            };

            return _dal.EscribirEscalar("sp_RegistrarUsuarioHuesped", parametros);
        }

        /// <summary>
        /// Busca por nombre, apellido, email o DNI. Alimenta el buscador de
        /// huesped de la pantalla de reservas.
        /// </summary>
        public List<Huesped> Buscar(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                throw new ArgumentException("Ingresa un texto para buscar.", nameof(texto));

            var parametros = new Hashtable { { "@Texto", texto.Trim() } };

            DataTable tabla = _dal.Leer("sp_BuscarHuesped", parametros);

            var huespedes = new List<Huesped>();
            foreach (DataRow fila in tabla.Rows)
                huespedes.Add(MapearHuesped(fila));

            return huespedes;
        }

        internal static Huesped MapearHuesped(DataRow fila)
        {
            return new Huesped(
                Mapeo.Entero   (fila, "HuespedID"),
                Mapeo.Texto    (fila, "Nombre"),
                Mapeo.Texto    (fila, "Apellido"),
                Mapeo.Entero   (fila, "DNI"),
                Mapeo.TextoNulo(fila, "Email"),
                Mapeo.TextoNulo(fila, "Telefono"))
            {
                UsuarioID = fila.Table.Columns.Contains("UsuarioID")
                            ? Mapeo.EnteroNulo(fila, "UsuarioID")
                            : null
            };
        }
    }
}
