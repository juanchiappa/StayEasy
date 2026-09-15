namespace StayEasy.BE
{
    /// <summary>
    /// Habitacion comun. Cotiza el precio base ajustado por el nivel de servicio.
    /// </summary>
    public class HabitacionEstandar : Habitacion
    {
        public HabitacionEstandar() { }

        public HabitacionEstandar(int idHabitacion, int numero, decimal precioBase,
                                  char nivelDeServicio, char estado)
            : base(idHabitacion, numero, precioBase, nivelDeServicio, estado) { }

        public override char TipoHabitacion => TIPO_ESTANDAR;

        public override string Descripcion => NivelDeServicio switch
        {
            NIVEL_PREMIUM => "Estandar premium",
            NIVEL_LUJO    => "Estandar de lujo",
            _             => "Estandar"
        };

        public override decimal CalcularPrecioNoche()
            => decimal.Round(PrecioBase * FactorNivelServicio, 2);
    }
}
