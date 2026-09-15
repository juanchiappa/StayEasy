using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using StayEasy.BE;
using StayEasy.DAL.Registro;

namespace StayEasy.MPP
{
    /// <summary>
    /// Mapeo entre las filas de Habitacion y la jerarquia BE.Habitacion.
    /// </summary>
    public class HabitacionMPP
    {
        private readonly AccesoDatos _dal = new AccesoDatos();

        /// <summary>
        /// Habitaciones vendibles para el rango [checkIn, checkOut).
        /// La disponibilidad la resuelve la base por solapamiento de fechas,
        /// no por el estado fisico de la habitacion.
        /// </summary>
        public List<HabitacionDisponible> ListarDisponibles(DateTime checkIn, DateTime checkOut,
                                                            char? tipoHabitacion = null)
        {
            if (checkOut.Date <= checkIn.Date)
                throw new ArgumentException("La fecha de check-out debe ser posterior a la de check-in.");

            var parametros = new Hashtable
            {
                { "@FechaCheckIn",   checkIn.Date  },
                { "@FechaCheckOut",  checkOut.Date },
                { "@TipoHabitacion", Mapeo.Parametro(tipoHabitacion) }
            };

            DataTable tabla = _dal.Leer("sp_ListarHabitacionesDisponibles", parametros);

            var disponibles = new List<HabitacionDisponible>();
            foreach (DataRow fila in tabla.Rows)
            {
                Habitacion habitacion = MapearHabitacion(fila);

                disponibles.Add(new HabitacionDisponible(
                    habitacion,
                    checkIn.Date,
                    checkOut.Date,
                    Mapeo.Entero(fila, "Noches"),
                    Mapeo.Decimal(fila, "SubtotalAlojamiento")));
            }

            return disponibles;
        }

        /// <summary>
        /// Construye la subclase correcta a partir de la fila. La columna
        /// EstadoFisico solo existe en el SP de disponibilidad; en el resto
        /// de las consultas la columna se llama Estado.
        /// </summary>
        internal static Habitacion MapearHabitacion(DataRow fila)
        {
            string columnaEstado = fila.Table.Columns.Contains("EstadoFisico") ? "EstadoFisico" : "Estado";

            return Habitacion.Crear(
                Mapeo.Caracter(fila, "TipoHabitacion",  Habitacion.TIPO_ESTANDAR),
                Mapeo.Entero  (fila, "ID_habitacion"),
                Mapeo.Entero  (fila, "Numero"),
                Mapeo.Decimal (fila, "PrecioBase"),
                Mapeo.Caracter(fila, "NivelDeServicio", Habitacion.NIVEL_BASICO),
                Mapeo.Caracter(fila, columnaEstado,     'D'));
        }
    }
}
