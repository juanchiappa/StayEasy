using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using StayEasy.BE;
using StayEasy.DAL.Registro;

namespace StayEasy.MPP
{
    /// <summary>
    /// Lectura de los reportes del Dashboard (ocupacion e ingresos).
    /// Solo lectura: no hay stored procedures de escritura asociados.
    /// </summary>
    public class ReporteMPP
    {
        private readonly AccesoDatos _dal = new AccesoDatos();

        public List<OcupacionDiaria> ListarOcupacionDiaria(DateTime desde, DateTime hasta)
        {
            DataTable tabla = _dal.Leer("sp_ReporteOcupacionDiaria", new Hashtable
            {
                { "@Desde", desde.Date },
                { "@Hasta", hasta.Date }
            });

            var resultado = new List<OcupacionDiaria>();
            foreach (DataRow fila in tabla.Rows)
            {
                resultado.Add(new OcupacionDiaria(
                    Mapeo.Fecha(fila, "Fecha"),
                    Mapeo.Entero(fila, "HabitacionesOcupadas"),
                    Mapeo.Entero(fila, "HabitacionesTotales")));
            }
            return resultado;
        }

        public IngresosMes ObtenerIngresosMes(int anio, int mes)
        {
            DataTable tabla = _dal.Leer("sp_ReporteIngresosMes", new Hashtable
            {
                { "@Anio", anio },
                { "@Mes",  mes }
            });

            if (tabla.Rows.Count == 0)
                return new IngresosMes(anio, mes, 0m, 0m);

            DataRow fila = tabla.Rows[0];
            return new IngresosMes(anio, mes,
                Mapeo.Decimal(fila, "Alojamiento"),
                Mapeo.Decimal(fila, "Consumos"));
        }
    }
}
