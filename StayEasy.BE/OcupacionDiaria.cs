using System;

namespace StayEasy.BE
{
    /// <summary>
    /// Modelo de lectura de sp_ReporteOcupacionDiaria: habitaciones ocupadas
    /// sobre habitaciones vendibles para un dia. Alimenta el grafico de
    /// ocupacion del Dashboard.
    /// </summary>
    public class OcupacionDiaria
    {
        public OcupacionDiaria() { }

        public OcupacionDiaria(DateTime fecha, int ocupadas, int totales)
        {
            Fecha    = fecha;
            Ocupadas = ocupadas;
            Totales  = totales;
        }

        public DateTime Fecha    { get; set; }
        public int      Ocupadas { get; set; }
        public int      Totales  { get; set; }

        /// <summary>Porcentaje 0-100 (0 si no hay habitaciones vendibles).</summary>
        public double Porcentaje => Totales == 0 ? 0 : Math.Round(Ocupadas * 100.0 / Totales, 1);
    }
}
