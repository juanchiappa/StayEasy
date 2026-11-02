using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace StayEasy.UI.Controles;

public partial class SelectorFechasEstadia : UserControl
{
    private static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("es-AR");
    private bool _notificacionPendiente;
    public int MaximoNoches { get; set; } = 90;
    public event EventHandler? FechasChanged;

    public SelectorFechasEstadia()
    {
        InitializeComponent();
        Entrada.DisplayDateStart = DateTime.Today;
        Entrada.SelectedDate = DateTime.Today;
        Salida.SelectedDate = DateTime.Today.AddDays(1);
        Entrada.SelectedDateChanged += (_, _) => ProgramarNotificacion();
        Salida.SelectedDateChanged += (_, _) => ProgramarNotificacion();
        Entrada.DateValidationError += FechaInvalida;
        Salida.DateValidationError += FechaInvalida;
        // Detecta texto inválido incluso antes de que DatePicker lo confirme.
        AddHandler(TextBoxBase.TextChangedEvent, new TextChangedEventHandler((_, _) => ProgramarNotificacion()));
        TryObtenerRango(out _, out _);
    }

    private void FechaInvalida(object? sender, DatePickerDateValidationErrorEventArgs e)
    {
        e.ThrowException = false;
        MostrarError("Ingresá una fecha válida.");
        ProgramarNotificacion();
    }

    private void ProgramarNotificacion()
    {
        if (_notificacionPendiente) return;
        _notificacionPendiente = true;
        Dispatcher.BeginInvoke(new Action(() =>
        {
            _notificacionPendiente = false;
            TryObtenerRango(out _, out _);
            FechasChanged?.Invoke(this, EventArgs.Empty);
        }));
    }

    private static bool LeerFecha(DatePicker control, out DateTime fecha)
    {
        fecha = default;
        string texto = (control.Template?.FindName("PART_TextBox", control) as TextBox)?.Text ?? control.Text;
        if (!control.SelectedDate.HasValue
            || !DateTime.TryParse(texto, Cultura, DateTimeStyles.None, out DateTime escrita)) return false;
        fecha = control.SelectedDate.Value.Date;
        return escrita.Date == fecha;
    }

    public bool TryObtenerRango(out DateTime checkIn, out DateTime checkOut)
    {
        checkIn = default;
        checkOut = default;
        if (!LeerFecha(Entrada, out checkIn) || !LeerFecha(Salida, out checkOut))
        {
            MostrarError("Elegí fechas válidas de ingreso y salida.");
            return false;
        }
        if (checkIn < DateTime.Today)
        {
            MostrarError("El ingreso no puede ser anterior a hoy.");
            return false;
        }
        int noches = (checkOut - checkIn).Days;
        if (noches <= 0)
        {
            MostrarError("La salida debe ser posterior al ingreso.");
            return false;
        }
        if (noches > MaximoNoches)
        {
            MostrarError($"La estadía no puede superar las {MaximoNoches} noches.");
            return false;
        }
        CantidadNoches.Text = noches == 1 ? "1 noche" : $"{noches} noches";
        Mensaje.Visibility = Visibility.Collapsed;
        return true;
    }

    public void MostrarError(string mensaje)
    {
        CantidadNoches.Text = string.Empty;
        Mensaje.Text = mensaje;
        Mensaje.Visibility = Visibility.Visible;
    }
}
