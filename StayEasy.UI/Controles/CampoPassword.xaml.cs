using System;
using System.Windows;
using System.Windows.Controls;

namespace StayEasy.UI.Controles;

public partial class CampoPassword : UserControl
{
    private bool _sincronizando;
    public event EventHandler? PasswordChanged;
    public string Password => EntradaOculta.Password;

    public static readonly DependencyProperty PlaceholderProperty = DependencyProperty.Register(
        nameof(Placeholder), typeof(string), typeof(CampoPassword),
        new PropertyMetadata("Ingresá tu contraseña", (d, e) =>
        {
            var campo = (CampoPassword)d;
            if (campo.MarcaAgua is not null) campo.MarcaAgua.Text = (string)e.NewValue;
        }));

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public CampoPassword()
    {
        InitializeComponent();
        MarcaAgua.Text = Placeholder;
        EntradaOculta.PasswordChanged += (_, _) =>
        {
            if (_sincronizando) return;
            Sincronizar(() =>
            {
                if (BotonMostrar.IsChecked == true) EntradaVisible.Text = Password;
            });
            NotificarCambio();
        };
        EntradaVisible.TextChanged += (_, _) =>
        {
            if (_sincronizando || BotonMostrar.IsChecked != true) return;
            Sincronizar(() => EntradaOculta.Password = EntradaVisible.Text);
            NotificarCambio();
        };
        BotonMostrar.Checked += (_, _) => CambiarVisibilidad(true);
        BotonMostrar.Unchecked += (_, _) => CambiarVisibilidad(false);
    }

    private void Sincronizar(Action accion)
    {
        _sincronizando = true;
        try { accion(); }
        finally { _sincronizando = false; }
    }

    private void CambiarVisibilidad(bool mostrar)
    {
        Sincronizar(() =>
        {
            EntradaVisible.Text = mostrar ? Password : string.Empty;
            EntradaVisible.Visibility = mostrar ? Visibility.Visible : Visibility.Collapsed;
            EntradaOculta.Visibility = mostrar ? Visibility.Collapsed : Visibility.Visible;
        });
        BotonMostrar.Content = mostrar ? "Ocultar" : "Mostrar";
        BotonMostrar.ToolTip = mostrar ? "Ocultar contraseña" : "Mostrar contraseña";
        if (mostrar)
        {
            EntradaVisible.Focus();
            EntradaVisible.CaretIndex = EntradaVisible.Text.Length;
        }
        else EntradaOculta.Focus();
    }

    private void NotificarCambio()
    {
        MarcaAgua.Visibility = Password.Length == 0 ? Visibility.Visible : Visibility.Collapsed;
        PasswordChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Limpiar()
    {
        Sincronizar(() => { EntradaOculta.Clear(); EntradaVisible.Clear(); });
        BotonMostrar.IsChecked = false;
        NotificarCambio();
    }
}
