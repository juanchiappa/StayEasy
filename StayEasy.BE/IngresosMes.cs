namespace StayEasy.BE
{
    /// <summary>
    /// Modelo de lectura de sp_ReporteIngresosMes: lo facturado en el mes,
    /// separado en alojamiento y consumos.
    /// </summary>
    public class IngresosMes
    {
        public IngresosMes(int anio, int mes, decimal alojamiento, decimal consumos)
        {
            Anio        = anio;
            Mes         = mes;
            Alojamiento = alojamiento;
            Consumos    = consumos;
        }

        public int     Anio        { get; set; }
        public int     Mes         { get; set; }
        public decimal Alojamiento { get; set; }
        public decimal Consumos    { get; set; }

        public decimal Total => Alojamiento + Consumos;
    }
}
