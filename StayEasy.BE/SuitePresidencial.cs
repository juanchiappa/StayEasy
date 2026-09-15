namespace StayEasy.BE
{
    /// <summary>
    /// Suite presidencial. Ademas del nivel de servicio aplica un recargo
    /// propio del tipo de habitacion.
    /// </summary>
    public class SuitePresidencial : Habitacion
    {
        /// <summary>Recargo fijo de la suite sobre el precio base.</summary>
        public const decimal RECARGO_SUITE = 1.35m;

        public SuitePresidencial() { }

        public SuitePresidencial(int idHabitacion, int numero, decimal precioBase,
                                 char nivelDeServicio, char estado)
            : base(idHabitacion, numero, precioBase, nivelDeServicio, estado) { }

        public override char TipoHabitacion => TIPO_SUITE;

        public override string Descripcion => "Suite presidencial";

        public override decimal CalcularPrecioNoche()
            => decimal.Round(PrecioBase * FactorNivelServicio * RECARGO_SUITE, 2);
    }
}
