using System;
using System.Collections.Generic;
using StayEasy.BE;
using StayEasy.MPP;

namespace StayEasy.BLL
{
    /// <summary>
    /// Estadisticas del Dashboard: ocupacion de los ultimos 7 dias contra los
    /// 7 anteriores, e ingresos del mes en curso.
    /// </summary>
    public class ReporteBLL
    {
        private readonly ReporteMPP _reporteMPP = new ReporteMPP();

        public const int DIAS_SEMANA = 7;

        /// <summary>
        /// Devuelve dos series de 7 dias alineadas por posicion: la semana que
        /// termina en <paramref name="hoy"/> y la semana inmediatamente anterior.
        /// </summary>
        public (List<OcupacionDiaria> Actual, List<OcupacionDiaria> Anterior) ObtenerOcupacionSemanal(DateTime hoy)
        {
            DateTime finActual    = hoy.Date;
            DateTime inicioActual = finActual.AddDays(-(DIAS_SEMANA - 1));
            DateTime inicioPrevio = inicioActual.AddDays(-DIAS_SEMANA);

            // Una sola consulta para los 14 dias y se parte en memoria.
            List<OcupacionDiaria> dias = _reporteMPP.ListarOcupacionDiaria(inicioPrevio, finActual);

            var anterior = dias.FindAll(d => d.Fecha < inicioActual);
            var actual   = dias.FindAll(d => d.Fecha >= inicioActual);
            return (actual, anterior);
        }

        public IngresosMes ObtenerIngresosMes(DateTime hoy)
        {
            return _reporteMPP.ObtenerIngresosMes(hoy.Year, hoy.Month);
        }
    }
}
