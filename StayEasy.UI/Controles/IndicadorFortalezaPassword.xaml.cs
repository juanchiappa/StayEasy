using System.Windows.Controls;
using System.Windows.Media;
using StayEasy.Seguridad.Validaciones;

namespace StayEasy.UI.Controles;

public partial class IndicadorFortalezaPassword : UserControl
{
    public bool CumpleRequisitos { get; private set; }

    public IndicadorFortalezaPassword()
    {
        InitializeComponent();
        Actualizar(string.Empty);
    }

    public void Actualizar(string? password)
    {
        password ??= string.Empty;
        bool longitud = password.Length >= 8;
        bool mayuscula = ValidadorDatos.TieneMayuscula(password);
        bool numero = ValidadorDatos.TieneNumero(password);
        CumpleRequisitos = ValidadorDatos.CumplePoliticaPassword(password);
        int puntaje = (longitud ? 1 : 0) + (mayuscula ? 1 : 0) + (numero ? 1 : 0);
        if (CumpleRequisitos && password.Length >= 12) puntaje++;
        Barra.Value = puntaje;
        Descripcion.Text = password.Length == 0 ? "Ingresá una contraseña" : puntaje switch
        {
            0 or 1 => "Débil",
            2 => "Media",
            3 => "Cumple los requisitos",
            _ => "Cumple los requisitos y tiene 12 caracteres o más"
        };
        Barra.Foreground = CumpleRequisitos ? Brushes.SeaGreen : Brushes.DarkOrange;
        MostrarRequisito(RequisitoLongitud, longitud, "8 caracteres");
        MostrarRequisito(RequisitoMayuscula, mayuscula, "Una mayúscula");
        MostrarRequisito(RequisitoNumero, numero, "Un número");
    }

    private static void MostrarRequisito(TextBlock destino, bool cumplido, string texto)
    {
        destino.Text = $"{(cumplido ? "✓" : "○")} {texto}";
        destino.Foreground = cumplido ? Brushes.SeaGreen : Brushes.Gray;
    }
}
