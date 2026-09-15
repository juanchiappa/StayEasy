using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using StayEasy.BE;
using StayEasy.BLL;

namespace StayEasy.UI
{
    /// <summary>
    /// Alta de reserva. La pantalla se arma en tres pasos encadenados:
    ///   fechas -> habitaciones libres en ese rango -> huesped -> confirmar.
    /// Cada cambio de fecha vuelve a consultar la disponibilidad, porque la
    /// grilla de habitaciones depende del rango y no de un estado fijo.
    /// </summary>
    public partial class Reservas : Window
    {
        private static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("es-AR");

        private readonly ReservaBLL _reservaBLL = new ReservaBLL();

        private Huesped? _huespedElegido;
        private List<HabitacionDisponible> _disponibles = new List<HabitacionDisponible>();

        /// <summary>Evita recalcular mientras se estan seteando los valores iniciales.</summary>
        private bool _inicializando = true;

        // Buscador de huesped: se arma por codigo porque el XAML no tiene
        // contenedor de resultados.
        private Popup? _popupResultados;
        private ListBox? _listaResultados;

        /// <summary>Debounce del buscador: no consulta en cada tecla.</summary>
        private DispatcherTimer? _temporizadorBusqueda;

        public Reservas()
        {
            InitializeComponent();

            ArmarBuscadorDeHuesped();
            EngancharEventos();

            // Rango por defecto: una noche a partir de hoy.
            Dp_CheckIn.SelectedDate  = DateTime.Today;
            Dp_CheckOut.SelectedDate = DateTime.Today.AddDays(1);
            Dp_CheckIn.DisplayDateStart  = DateTime.Today;
            Dp_CheckOut.DisplayDateStart = DateTime.Today.AddDays(1);

            LimpiarHuesped();

            // El estado de una reserva nueva siempre es Confirmada: lo decide
            // el stored procedure, no la UI.
            Cb_Estado.IsEnabled = false;

            Tx_IdReserva.Text = "—";

            _inicializando = false;

            CargarDisponibilidad();
        }

        // ==================================================================
        // Cableado
        // ==================================================================

        private void EngancharEventos()
        {
            Dp_CheckIn.SelectedDateChanged  += (_, _) => AlCambiarFechas();
            Dp_CheckOut.SelectedDateChanged += (_, _) => AlCambiarFechas();

            Tb_BuscarHuesped.KeyDown     += Tb_BuscarHuesped_KeyDown;
            Tb_BuscarHuesped.TextChanged += Tb_BuscarHuesped_TextChanged;
            Btn_CambiarHuesped.Click     += (_, _) => LimpiarHuesped();
            Btn_NuevoHuesped.Click       += Btn_NuevoHuesped_Click;

            foreach (CheckBox servicio in ServiciosDeLaLista())
            {
                servicio.Checked   += (_, _) => RecalcularResumen();
                servicio.Unchecked += (_, _) => RecalcularResumen();
            }

            Tb_Senia.TextChanged   += (_, _) => RecalcularResumen();
            Rb_Senia.Checked       += (_, _) => RecalcularResumen();
            Rb_PagoTotal.Checked   += (_, _) => RecalcularResumen();
            Rb_AlCheckIn.Checked   += (_, _) => RecalcularResumen();

            Btn_Confirmar.Click += Btn_Confirmar_Click;
            Btn_Cancelar.Click  += (_, _) => VolverAlDashboard();
            Btn_Cerrar.Click    += (_, _) => VolverAlDashboard();
            Btn_Borrador.Click  += Btn_Borrador_Click;
        }

        // ==================================================================
        // Paso 1 y 2: fechas -> disponibilidad
        // ==================================================================

        private void AlCambiarFechas()
        {
            if (_inicializando) return;

            // El check-out siempre tiene que ir despues del check-in.
            if (Dp_CheckIn.SelectedDate is DateTime entrada)
            {
                Dp_CheckOut.DisplayDateStart = entrada.AddDays(1);

                if (Dp_CheckOut.SelectedDate is DateTime salida && salida.Date <= entrada.Date)
                {
                    _inicializando = true;
                    Dp_CheckOut.SelectedDate = entrada.AddDays(1);
                    _inicializando = false;
                }
            }

            CargarDisponibilidad();
        }

        /// <summary>
        /// Repuebla la grilla con lo que devuelve la base para el rango actual.
        /// Reemplaza por completo las tarjetas hardcodeadas del XAML.
        /// </summary>
        private void CargarDisponibilidad()
        {
            Grid_Habitaciones.Children.Clear();
            _disponibles.Clear();

            if (Dp_CheckIn.SelectedDate is not DateTime checkIn ||
                Dp_CheckOut.SelectedDate is not DateTime checkOut)
            {
                MostrarError(Er_Fechas, "Elegí las fechas de ingreso y salida.");
                RecalcularResumen();
                return;
            }

            try
            {
                _disponibles = _reservaBLL.BuscarDisponibilidad(checkIn, checkOut);
                OcultarError(Er_Fechas);
            }
            catch (ArgumentException ex)          // validaciones de la BLL
            {
                MostrarError(Er_Fechas, ex.Message);
                RecalcularResumen();
                return;
            }
            catch (Exception ex)                  // la base no respondio
            {
                MostrarError(Er_Fechas, ex.Message);
                RecalcularResumen();
                return;
            }

            foreach (HabitacionDisponible disponible in _disponibles)
                Grid_Habitaciones.Children.Add(CrearTarjetaDeHabitacion(disponible));

            AcomodarGrilla(_disponibles.Count);

            if (_disponibles.Count == 0)
                MostrarError(Er_Habitacion, "Ninguna habitación está libre en ese rango. Cambiá las fechas.");
            else
                OcultarError(Er_Habitacion);

            RecalcularResumen();
        }

        /// <summary>
        /// Recrea por codigo la misma tarjeta que el XAML tenia hardcodeada,
        /// reutilizando el estilo RoomCard. En el Tag viaja la entidad completa,
        /// no un string con separadores.
        /// </summary>
        private RadioButton CrearTarjetaDeHabitacion(HabitacionDisponible disponible)
        {
            var encabezado = new Grid();
            encabezado.Children.Add(new TextBlock
            {
                Text = disponible.NumeroHabitacion.ToString(Cultura),
                FontSize = 19,
                FontWeight = FontWeights.SemiBold
            });
            encabezado.Children.Add(new Ellipse
            {
                Width = 8,
                Height = 8,
                Fill = Pincel("Muted", Brushes.Silver),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center
            });

            var contenido = new StackPanel();
            contenido.Children.Add(encabezado);
            contenido.Children.Add(new TextBlock
            {
                Text = disponible.DescripcionTipo,
                FontSize = 12,
                Foreground = Pincel("Muted", Brushes.Gray),
                Margin = new Thickness(0, 2, 0, 0)
            });
            contenido.Children.Add(new TextBlock
            {
                Text = $"{disponible.PrecioNoche.ToString("C", Cultura)} / noche",
                FontSize = 12.5,
                FontWeight = FontWeights.Medium,
                Margin = new Thickness(0, 10, 0, 0)
            });

            var tarjeta = new RadioButton
            {
                GroupName = "Habitacion",
                Content   = contenido,
                Tag       = disponible,
                Margin    = new Thickness(0, 0, 10, 10),
                ToolTip   = $"{disponible.Noches} noche(s) · {disponible.Subtotal.ToString("C", Cultura)}"
            };

            if (TryFindResource("RoomCard") is Style estilo)
                tarjeta.Style = estilo;

            tarjeta.Checked += (_, _) => RecalcularResumen();

            return tarjeta;
        }

        private void AcomodarGrilla(int cantidad)
        {
            Grid_Habitaciones.Columns = 3;
            Grid_Habitaciones.Rows    = Math.Max(1, (int)Math.Ceiling(cantidad / 3d));
        }

        private HabitacionDisponible? HabitacionElegida =>
            Grid_Habitaciones.Children
                .OfType<RadioButton>()
                .FirstOrDefault(r => r.IsChecked == true)?.Tag as HabitacionDisponible;

        // ==================================================================
        // Paso 3: huesped
        // ==================================================================

        private void ArmarBuscadorDeHuesped()
        {
            // Busca sola mientras se escribe, pero espera 350 ms de silencio
            // para no mandar una consulta por tecla.
            _temporizadorBusqueda = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(350) };
            _temporizadorBusqueda.Tick += (_, _) =>
            {
                _temporizadorBusqueda!.Stop();
                BuscarHuespedes(seleccionarSiUnico: false);
            };

            _listaResultados = new ListBox { MaxHeight = 220 };
            _listaResultados.SelectionChanged += Lista_SelectionChanged;

            _popupResultados = new Popup
            {
                PlacementTarget = Tb_BuscarHuesped,
                Placement       = PlacementMode.Bottom,
                StaysOpen       = false,
                AllowsTransparency = true,
                Child = new Border
                {
                    Background      = Pincel("Paper", Brushes.White),
                    BorderBrush     = Pincel("Muted", Brushes.LightGray),
                    BorderThickness = new Thickness(1),
                    CornerRadius    = new CornerRadius(10),
                    Padding         = new Thickness(4),
                    Child           = _listaResultados
                }
            };
        }

        private void Tb_BuscarHuesped_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_inicializando) return;

            // Con un huesped ya elegido el campo no vuelve a buscar solo:
            // primero hay que tocar "Cambiar".
            if (_huespedElegido is not null) return;

            _temporizadorBusqueda!.Stop();

            if (Tb_BuscarHuesped.Text.Trim().Length < 2)
            {
                _popupResultados!.IsOpen = false;
                OcultarError(Er_Huesped);
                return;
            }

            _temporizadorBusqueda.Start();
        }

        private void Tb_BuscarHuesped_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                _temporizadorBusqueda!.Stop();
                BuscarHuespedes(seleccionarSiUnico: true);   // Enter con un solo match lo elige
            }
            else if (e.Key == Key.Escape)
            {
                _popupResultados!.IsOpen = false;
            }
        }

        private void BuscarHuespedes(bool seleccionarSiUnico)
        {
            string texto = Tb_BuscarHuesped.Text.Trim();
            if (texto.Length < 2) return;

            try
            {
                List<Huesped> encontrados = _reservaBLL.BuscarHuesped(texto);

                if (encontrados.Count == 0)
                {
                    _listaResultados!.ItemsSource = null;
                    _popupResultados!.IsOpen = false;
                    MostrarError(Er_Huesped, $"Ningún huésped coincide con \"{texto}\".");
                    return;
                }

                OcultarError(Er_Huesped);

                if (seleccionarSiUnico && encontrados.Count == 1)
                {
                    ElegirHuesped(encontrados[0]);
                    return;
                }

                _listaResultados!.ItemsSource  = encontrados;
                _listaResultados.SelectedIndex = -1;
                _popupResultados!.Width        = Tb_BuscarHuesped.ActualWidth;
                _popupResultados.IsOpen        = true;
            }
            catch (Exception ex)
            {
                MostrarError(Er_Huesped, ex.Message);
            }
        }

        private void Lista_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_listaResultados?.SelectedItem is Huesped elegido)
            {
                _popupResultados!.IsOpen = false;
                ElegirHuesped(elegido);
            }
        }

        private void ElegirHuesped(Huesped huesped)
        {
            _huespedElegido = huesped;

            Tx_HuespedIniciales.Text = huesped.Iniciales;
            Tx_HuespedNombre.Text    = huesped.NombreCompleto;

            var detalle = new StringBuilder($"DNI {huesped.DNI.ToString("N0", Cultura)}");
            if (!string.IsNullOrWhiteSpace(huesped.Email))    detalle.Append(" · ").Append(huesped.Email);
            if (!string.IsNullOrWhiteSpace(huesped.Telefono)) detalle.Append(" · ").Append(huesped.Telefono);
            Tx_HuespedDetalle.Text = detalle.ToString();

            Pn_HuespedElegido.Visibility = Visibility.Visible;
            OcultarError(Er_Huesped);
            RecalcularResumen();
        }

        private void LimpiarHuesped()
        {
            _huespedElegido = null;
            Pn_HuespedElegido.Visibility = Visibility.Collapsed;
            Tb_BuscarHuesped.Text = string.Empty;
            Tb_BuscarHuesped.Focus();
            RecalcularResumen();
        }

        private void Btn_NuevoHuesped_Click(object sender, RoutedEventArgs e)
        {
            // Pendiente: hoy el unico alta de huesped es el autorregistro
            // (sp_RegistrarUsuarioHuesped), que exige usuario y contrasena.
            // Falta un alta de huesped sin cuenta para uso de recepcion.
            MessageBox.Show(
                "El alta rápida de huésped desde recepción todavía no está implementada. " +
                "Por ahora el huésped se crea desde la pantalla de registro.",
                "StayEasy", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // ==================================================================
        // Servicios y resumen
        // ==================================================================

        private IEnumerable<CheckBox> ServiciosDeLaLista() => Pn_Servicios.Children.OfType<CheckBox>();

        /// <summary>
        /// Los servicios siguen viniendo del Tag del XAML ("Nombre|Precio|...")
        /// hasta que exista el ABM de ServiciosPaquete en la base.
        /// </summary>
        private List<(string Nombre, decimal Precio)> ServiciosElegidos()
        {
            var elegidos = new List<(string, decimal)>();

            foreach (CheckBox servicio in ServiciosDeLaLista())
            {
                if (servicio.IsChecked != true) continue;

                string[] partes = (servicio.Tag?.ToString() ?? string.Empty).Split('|');
                if (partes.Length < 2) continue;

                if (decimal.TryParse(partes[1], NumberStyles.Any, Cultura, out decimal precio))
                    elegidos.Add((partes[0], precio));
            }

            return elegidos;
        }

        private void RecalcularResumen()
        {
            if (_inicializando) return;

            HabitacionDisponible? habitacion = HabitacionElegida;
            List<(string Nombre, decimal Precio)> servicios = ServiciosElegidos();

            Tx_ServiciosElegidos.Text = $"{servicios.Count} seleccionados";

            int     noches      = habitacion?.Noches   ?? 0;
            decimal alojamiento = habitacion?.Subtotal ?? 0m;
            decimal totalServicios = servicios.Sum(s => s.Precio);
            decimal total = alojamiento + totalServicios;

            Tx_Noches.Text = noches == 1 ? "1 noche" : $"{noches} noches";

            Tx_ResHabLabel.Text  = habitacion is null
                                   ? "Habitación"
                                   : $"Habitación {habitacion.NumeroHabitacion} · {habitacion.DescripcionTipo}";
            Tx_ResHabValor.Text  = alojamiento.ToString("C", Cultura);

            Tx_ResNochesLabel.Text = noches == 1 ? "1 noche de estadía" : $"{noches} noches de estadía";
            Tx_ResNochesValor.Text = (habitacion?.PrecioNoche ?? 0m).ToString("C", Cultura);

            Tx_ResServiciosLabel.Text = servicios.Count == 0
                                        ? "Sin servicios adicionales"
                                        : string.Join(" · ", servicios.Select(s => s.Nombre));
            Tx_ResServiciosValor.Text = totalServicios.ToString("C", Cultura);

            // El descuento por combo entra cuando se implemente el Composite
            // de servicios; por ahora siempre es cero.
            Tx_ResDescuentoValor.Text = 0m.ToString("C", Cultura);

            Tx_Total.Text = total.ToString("C", Cultura);

            decimal senia = SeniaIngresada(total);
            Tx_SeniaLabel.Text = Rb_PagoTotal.IsChecked == true ? "Pago total" :
                                 Rb_AlCheckIn.IsChecked  == true ? "A pagar en el check-in" : "Seña";
            Tx_SeniaMonto.Text = senia.ToString("C", Cultura);
            Tx_Saldo.Text      = (total - senia).ToString("C", Cultura);

            Btn_Confirmar.IsEnabled = habitacion is not null && _huespedElegido is not null;

            Tx_NotaEstado.Text = Btn_Confirmar.IsEnabled
                                 ? "Listo para confirmar."
                                 : "Elegí fechas, habitación y huésped para confirmar.";
        }

        /// <summary>
        /// Solo para mostrar el saldo en pantalla: la base todavia no tiene
        /// donde guardar senia ni medio de pago (falta la tabla de pagos).
        /// </summary>
        private decimal SeniaIngresada(decimal total)
        {
            if (Rb_PagoTotal.IsChecked == true) return total;
            if (Rb_AlCheckIn.IsChecked == true) return 0m;

            string texto = new string((Tb_Senia.Text ?? string.Empty)
                                      .Where(c => char.IsDigit(c) || c == ',' || c == '.').ToArray());

            if (!decimal.TryParse(texto, NumberStyles.Any, Cultura, out decimal senia) || senia < 0m)
            {
                MostrarError(Er_Senia, "El monto de la seña no es válido.");
                return 0m;
            }

            if (senia > total)
            {
                MostrarError(Er_Senia, "La seña no puede superar el total.");
                return total;
            }

            OcultarError(Er_Senia);
            return senia;
        }

        // ==================================================================
        // Confirmar
        // ==================================================================

        private void Btn_Confirmar_Click(object sender, RoutedEventArgs e)
        {
            HabitacionDisponible? habitacion = HabitacionElegida;

            if (_huespedElegido is null)
            {
                MostrarError(Er_Huesped, "Elegí el huésped titular de la reserva.");
                return;
            }

            if (habitacion is null)
            {
                MostrarError(Er_Habitacion, "Elegí una habitación.");
                return;
            }

            Btn_Confirmar.IsEnabled = false;

            try
            {
                IEnumerable<decimal> servicios = ServiciosElegidos().Select(s => s.Precio);

                int idReserva = _reservaBLL.RegistrarReserva(_huespedElegido, habitacion, servicios);

                Tx_IdReserva.Text  = $"#{idReserva}";
                Tx_NotaEstado.Text = "Reserva confirmada.";

                MessageBox.Show(
                    $"Reserva #{idReserva} confirmada para {_huespedElegido.NombreCompleto}, " +
                    $"habitación {habitacion.NumeroHabitacion}, del " +
                    $"{habitacion.FechaCheckIn:dd/MM/yyyy} al {habitacion.FechaCheckOut:dd/MM/yyyy}.",
                    "StayEasy", MessageBoxButton.OK, MessageBoxImage.Information);

                VolverAlDashboard();
            }
            catch (Exception ex)
            {
                // Incluye el caso en que otra persona tomo la habitacion entre
                // la busqueda y la confirmacion (error 52010 de la base).
                MostrarError(Er_Habitacion, ex.Message);
                CargarDisponibilidad();
            }
            finally
            {
                Btn_Confirmar.IsEnabled = HabitacionElegida is not null && _huespedElegido is not null;
            }
        }

        private void Btn_Borrador_Click(object sender, RoutedEventArgs e)
        {
            // Pendiente: Reserva.Estado solo admite Confirmada / EnCurso /
            // Finalizada / Cancelada (CK_Reserva_Estado). Guardar un borrador
            // exige agregar ese estado al CHECK y al enum EstadoReserva.
            MessageBox.Show(
                "Guardar como borrador todavía no está implementado: falta el estado 'Borrador' en la base.",
                "StayEasy", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // ==================================================================
        // Auxiliares
        // ==================================================================

        private void VolverAlDashboard()
        {
            var dashboard = new Dashboard();
            dashboard.Show();
            this.Close();
        }

        private static void MostrarError(TextBlock destino, string mensaje)
        {
            destino.Text       = mensaje;
            destino.Visibility = Visibility.Visible;
        }

        private static void OcultarError(TextBlock destino)
            => destino.Visibility = Visibility.Collapsed;

        private Brush Pincel(string clave, Brush porDefecto)
            => TryFindResource(clave) as Brush ?? porDefecto;
    }
}
