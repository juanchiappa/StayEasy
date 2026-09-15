using System;
using System.Data;
using StayEasy.BE;

namespace StayEasy.MPP
{
    /// <summary>
    /// Helpers para leer celdas de un DataRow sin repetir el chequeo de DBNull
    /// en cada mapper. Uso interno de la capa MPP.
    /// </summary>
    internal static class Mapeo
    {
        public static string Texto(DataRow fila, string columna)
            => fila.IsNull(columna) ? string.Empty : Convert.ToString(fila[columna]) ?? string.Empty;

        public static string? TextoNulo(DataRow fila, string columna)
            => fila.IsNull(columna) ? null : Convert.ToString(fila[columna]);

        public static int Entero(DataRow fila, string columna)
            => fila.IsNull(columna) ? 0 : Convert.ToInt32(fila[columna]);

        public static int? EnteroNulo(DataRow fila, string columna)
            => fila.IsNull(columna) ? null : Convert.ToInt32(fila[columna]);

        public static decimal Decimal(DataRow fila, string columna)
            => fila.IsNull(columna) ? 0m : Convert.ToDecimal(fila[columna]);

        public static DateTime Fecha(DataRow fila, string columna)
            => fila.IsNull(columna) ? default : Convert.ToDateTime(fila[columna]);

        /// <summary>
        /// Las columnas char(1) llegan como string en el DataTable.
        /// </summary>
        public static char Caracter(DataRow fila, string columna, char porDefecto = ' ')
        {
            if (fila.IsNull(columna)) return porDefecto;
            string valor = Convert.ToString(fila[columna]) ?? string.Empty;
            return valor.Length > 0 ? valor[0] : porDefecto;
        }

        /// <summary>
        /// Reserva.Estado se guarda como varchar y se modela como enum. Los
        /// nombres del enum coinciden exactamente con el CHECK CK_Reserva_Estado,
        /// asi que el parseo es directo. Si aparece un valor desconocido es que
        /// alguien inserto a mano saltando el constraint: falla ruidosamente.
        /// </summary>
        public static EstadoReserva Estado(DataRow fila, string columna)
        {
            string valor = Texto(fila, columna);

            if (Enum.TryParse(valor, ignoreCase: true, out EstadoReserva estado)
                && Enum.IsDefined(estado))
            {
                return estado;
            }

            throw new InvalidOperationException(
                $"Estado de reserva desconocido en la base: '{valor}'.");
        }

        /// <summary>
        /// SqlClient no sabe mapear System.Char: todo char que viaje como
        /// parametro tiene que convertirse a string primero.
        /// </summary>
        public static object Parametro(char? valor)
            => valor.HasValue ? valor.Value.ToString() : DBNull.Value;

        public static object Parametro(string? valor)
            => string.IsNullOrWhiteSpace(valor) ? DBNull.Value : valor;

        public static object Parametro(int? valor)
            => valor.HasValue ? valor.Value : DBNull.Value;

        public static object Parametro(DateTime? valor)
            => valor.HasValue ? valor.Value.Date : DBNull.Value;

        /// <summary>El enum viaja a la base como el texto del CHECK.</summary>
        public static object Parametro(EstadoReserva? valor)
            => valor.HasValue ? valor.Value.ToString() : DBNull.Value;
    }
}
