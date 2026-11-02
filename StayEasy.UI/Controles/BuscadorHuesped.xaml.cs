using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using StayEasy.BE;

namespace StayEasy.UI.Controles;

public partial class BuscadorHuesped : UserControl
{
    private readonly DispatcherTimer _temporizador;
    private bool _actualizando;
    public Func<string, List<Huesped>>? Buscar { get; set; }
    public Huesped? Seleccionado { get; private set; }
    public event EventHandler? SeleccionChanged;

    public BuscadorHuesped()
    {
        InitializeComponent();
        _temporizador = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(350) };
        _temporizador.Tick += (_, _) => { _temporizador.Stop(); EjecutarBusqueda(false); };
        Entrada.TextChanged += (_, _) =>
        {
            if (_actualizando || Seleccionado is not null) return;
            _temporizador.Stop();
            Resultados.ItemsSource = null;
            Resultados.Visibility = Visibility.Collapsed;
            Mensaje.Visibility = Visibility.Collapsed;
            if (Entrada.Text.Trim().Length >= 2) _temporizador.Start();
        };
        Entrada.KeyDown += (_, e) =>
        {
            if (Seleccionado is not null) return;
            if (e.Key == Key.Enter)
            {
                _temporizador.Stop();
                EjecutarBusqueda(true);
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                _temporizador.Stop();
                Resultados.Visibility = Visibility.Collapsed;
                e.Handled = true;
            }
        };
        Resultados.SelectionChanged += (_, _) =>
        {
            if (!_actualizando && Resultados.SelectedItem is Huesped huesped) Elegir(huesped);
        };
        BotonLimpiar.Click += (_, _) => { Limpiar(); Entrada.Focus(); };
        Unloaded += (_, _) => _temporizador.Stop();
    }

    private void EjecutarBusqueda(bool seleccionarSiUnico)
    {
        string texto = Entrada.Text.Trim();
        if (texto.Length < 2 || Seleccionado is not null) return;
        if (Buscar is null) { MostrarError("No se configuró la función de búsqueda."); return; }
        try
        {
            List<Huesped> encontrados = Buscar(texto);
            if (encontrados.Count == 0)
            {
                Resultados.Visibility = Visibility.Collapsed;
                MostrarError("No se encontraron huéspedes.");
                return;
            }
            Mensaje.Visibility = Visibility.Collapsed;
            if (seleccionarSiUnico && encontrados.Count == 1) { Elegir(encontrados[0]); return; }
            _actualizando = true;
            try
            {
                Resultados.ItemsSource = encontrados;
                Resultados.SelectedIndex = -1;
                Resultados.Visibility = Visibility.Visible;
            }
            finally { _actualizando = false; }
        }
        catch (Exception ex)
        {
            Resultados.Visibility = Visibility.Collapsed;
            MostrarError(ex.Message);
        }
    }

    private void Elegir(Huesped huesped)
    {
        _temporizador.Stop();
        Seleccionado = huesped;
        _actualizando = true;
        try
        {
            Entrada.Text = huesped.NombreCompleto;
            Entrada.IsReadOnly = true;
            Resultados.ItemsSource = null;
            Resultados.Visibility = Visibility.Collapsed;
            NombreSeleccionado.Text = huesped.NombreCompleto;
            DetalleSeleccionado.Text = string.Join(" · ",
                new[] { $"DNI {huesped.DNI}", huesped.Email, huesped.Telefono }
                    .Where(s => !string.IsNullOrWhiteSpace(s)));
            Ficha.Visibility = Visibility.Visible;
            BotonLimpiar.Visibility = Visibility.Visible;
            Mensaje.Visibility = Visibility.Collapsed;
        }
        finally { _actualizando = false; }
        SeleccionChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Limpiar()
    {
        _temporizador.Stop();
        Seleccionado = null;
        _actualizando = true;
        try
        {
            Entrada.Clear();
            Entrada.IsReadOnly = false;
            Resultados.ItemsSource = null;
            Resultados.Visibility = Visibility.Collapsed;
            Ficha.Visibility = Visibility.Collapsed;
            BotonLimpiar.Visibility = Visibility.Collapsed;
            Mensaje.Visibility = Visibility.Collapsed;
        }
        finally { _actualizando = false; }
        SeleccionChanged?.Invoke(this, EventArgs.Empty);
    }

    private void MostrarError(string mensaje)
    {
        Mensaje.Text = mensaje;
        Mensaje.Visibility = Visibility.Visible;
    }
}
