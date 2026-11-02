using System.Globalization;
using System.Windows.Controls;

namespace StayEasy.UI.Controles;

public partial class ResumenReserva : UserControl
{
    private static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("es-AR");

    public ResumenReserva()
    {
        InitializeComponent();
        MostrarId(null);
        Actualizar("Sin habitación", 0, 0m, 0m, 0m, 0m);
    }

    public void MostrarId(int? id) => Identificador.Text = id.HasValue ? $"Reserva #{id.Value}" : "Nueva reserva";
    public void MostrarEstado(string mensaje) => Estado.Text = mensaje;

    public void Actualizar(string habitacion, int noches, decimal alojamiento,
        decimal servicios, decimal total, decimal senia, string etiquetaPago = "Pago previsto")
    {
        Habitacion.Text = habitacion;
        Noches.Text = noches == 1 ? "1 noche" : $"{noches} noches";
        Alojamiento.Text = $"Alojamiento: {alojamiento.ToString("C", Cultura)}";
        Servicios.Text = $"Servicios: {servicios.ToString("C", Cultura)}";
        Total.Text = total.ToString("C", Cultura);
        Senia.Text = $"{etiquetaPago}: {senia.ToString("C", Cultura)}";
        Saldo.Text = $"Saldo: {(total - senia).ToString("C", Cultura)}";
    }
}
