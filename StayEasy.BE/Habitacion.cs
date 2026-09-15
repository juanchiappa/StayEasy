using System;

namespace StayEasy.BE
{
    /// <summary>
    /// Clase abstracta base de la jerarquia de habitaciones.
    /// Cada subclase decide como se cotiza una noche (polimorfismo) y que
    /// discriminador viaja a la columna Habitacion.TipoHabitacion.
    /// Se instancia siempre con la factory <see cref="Crear"/>.
    /// </summary>
    public abstract class Habitacion
    {
        // Discriminadores de la columna TipoHabitacion (char(1)).
        public const char TIPO_ESTANDAR = 'E';
        public const char TIPO_SUITE    = 'S';

        // Niveles de servicio de la columna NivelDeServicio (char(1)).
        public const char NIVEL_BASICO  = 'B';
        public const char NIVEL_PREMIUM = 'P';
        public const char NIVEL_LUJO    = 'L';

        protected Habitacion() { }

        protected Habitacion(int idHabitacion, int numero, decimal precioBase,
                             char nivelDeServicio, char estado)
        {
            ID_habitacion   = idHabitacion;
            Numero          = numero;
            PrecioBase      = precioBase;
            NivelDeServicio = nivelDeServicio;
            Estado          = estado;
        }

        public int     ID_habitacion   { get; set; }
        public int     Numero          { get; set; }
        public decimal PrecioBase      { get; set; }
        public char    NivelDeServicio { get; set; }

        /// <summary>
        /// Estado FISICO de hoy: 'D' disponible, 'O' ocupada, 'L' en limpieza,
        /// 'F' fuera de servicio. No indica si se puede reservar a futuro:
        /// eso lo resuelve la consulta de disponibilidad por rango de fechas.
        /// </summary>
        public char Estado { get; set; }

        /// <summary>Discriminador que se persiste en TipoHabitacion.</summary>
        public abstract char TipoHabitacion { get; }

        /// <summary>Texto legible para la grilla de la UI.</summary>
        public abstract string Descripcion { get; }

        /// <summary>
        /// Precio de una noche. Cada subclase aplica su propio criterio.
        /// </summary>
        public abstract decimal CalcularPrecioNoche();

        /// <summary>Precio de la estadia completa, sin servicios adicionales.</summary>
        public decimal CalcularEstadia(int noches)
        {
            if (noches <= 0)
                throw new ArgumentException("La cantidad de noches debe ser mayor a cero.", nameof(noches));

            return CalcularPrecioNoche() * noches;
        }

        /// <summary>Multiplicador segun el nivel de servicio contratado.</summary>
        protected decimal FactorNivelServicio => NivelDeServicio switch
        {
            NIVEL_PREMIUM => 1.15m,
            NIVEL_LUJO    => 1.30m,
            _             => 1.00m
        };

        public bool EstaLibreAhora     => Estado == 'D';
        public bool EstaFueraDeServicio => Estado == 'F';

        /// <summary>
        /// Factory: construye la subclase correcta a partir del discriminador
        /// que viene de la base. Es el unico lugar donde se decide el tipo.
        /// </summary>
        public static Habitacion Crear(char tipoHabitacion, int idHabitacion, int numero,
                                       decimal precioBase, char nivelDeServicio, char estado)
        {
            return char.ToUpperInvariant(tipoHabitacion) switch
            {
                TIPO_ESTANDAR => new HabitacionEstandar(idHabitacion, numero, precioBase, nivelDeServicio, estado),
                TIPO_SUITE    => new SuitePresidencial(idHabitacion, numero, precioBase, nivelDeServicio, estado),
                _ => throw new ArgumentOutOfRangeException(
                         nameof(tipoHabitacion),
                         $"Tipo de habitacion desconocido '{tipoHabitacion}' en la habitacion numero {numero}. " +
                         $"Los valores validos son '{TIPO_ESTANDAR}' (estandar) y '{TIPO_SUITE}' (suite).")
            };
        }

        public override string ToString() => $"{Numero} - {Descripcion}";
    }
}
