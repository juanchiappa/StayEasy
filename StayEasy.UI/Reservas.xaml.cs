using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using StayEasy.BE;
using StayEasy.BLL;
using StayEasy.Seguridad.Validaciones;

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

        public Reservas()
        {
            InitializeComponent();
            SelectorHuesped.Buscar = _reservaBLL.BuscarHuesped;
            SelectorHuesped.SeleccionChanged += (_, _) =>
            {
                _huespedElegido = SelectorHuesped.Seleccionado;
                OcultarError(Er_Huesped);
                RecalcularResumen();
            };
            SelectorFechas.MaximoNoches = ReservaBLL.MAXIMO_NOCHES;
            SelectorFechas.FechasChanged += (_, _) =>
            {
                if (!_inicializando) CargarDisponibilidad();
            };
            EngancharEventos();
            Cb_Estado.IsEnabled = false;
            _inicializando = false;
            CargarDisponibilidad();
        }

        // ==================================================================
        // Cableado
        // ==================================================================

        private void EngancharEventos()
        {
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

        /// <summary>
        /// Repuebla la grilla con lo que devuelve la base para el rango actual.
        /// Reemplaza por completo las tarjetas hardcodeadas del XAML.
        /// </summary>
        private void CargarDisponibilidad()
        {
            Grid_Habitaciones.Children.Clear();
            _disponibles.Clear();
            OcultarError(Er_Habitacion);

            if (!SelectorFechas.TryObtenerRango(out DateTime checkIn, out DateTime checkOut))
            {
                RecalcularResumen();
                return;
            }

            try
            {
                _disponibles = _reservaBLL.BuscarDisponibilidad(checkIn, checkOut);
            }
            catch (ArgumentException ex)          // validaciones de la BLL
            {
                MostrarError(Er_Habitacion, ex.Message);
                RecalcularResumen();
                return;
            }
            catch (Exception ex)                  // la base no respondio
            {
                MostrarError(Er_Habitacion, ex.Message);
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
            var servicios = ServiciosElegidos();
            int noches = habitacion?.Noches ?? 0;
            decimal alojamiento = habitacion?.Subtotal ?? 0m;
            decimal totalServicios = servicios.Sum(s => s.Precio);
            decimal total = habitacion is null ? 0m
                : _reservaBLL.CalcularTotal(habitacion, servicios.Select(s => s.Precio));
            bool seniaValida = TryObtenerSenia(total, out decimal senia);
            bool fechasValidas = SelectorFechas.TryObtenerRango(out DateTime entrada, out DateTime salida);
            bool habitacionVigente = fechasValidas && habitacion is not null
                && habitacion.FechaCheckIn.Date == entrada && habitacion.FechaCheckOut.Date == salida;
            Tx_ServiciosElegidos.Text = $"{servicios.Count} seleccionados";
            string etiquetaPago = Rb_PagoTotal.IsChecked == true ? "Pago total previsto"
                : Rb_AlCheckIn.IsChecked == true ? "A pagar en el check-in" : "Seña prevista";
            Resumen.Actualizar(habitacion is null ? "Sin habitación seleccionada"
                : $"Habitación {habitacion.NumeroHabitacion} · {habitacion.DescripcionTipo}",
                noches, alojamiento, totalServicios, total, seniaValida ? senia : 0m, etiquetaPago);
            Btn_Confirmar.IsEnabled = habitacionVigente && _huespedElegido is not null && seniaValida;
            Resumen.MostrarEstado(Btn_Confirmar.IsEnabled ? "Listo para confirmar."
                : "Revisá las fechas, la habitación, el huésped y el importe.");
        }

        /// <summary>
        /// Solo para mostrar el saldo en pantalla: la base todavia no tiene
        /// donde guardar senia ni medio de pago (falta la tabla de pagos).
        /// </summary>
        private bool TryObtenerSenia(decimal total, out decimal senia)
        {
            senia = 0m;
            if (Rb_PagoTotal.IsChecked == true)
            {
                senia = total;
                OcultarError(Er_Senia);
                return true;
            }
            if (Rb_AlCheckIn.IsChecked == true)
            {
                OcultarError(Er_Senia);
                return true;
            }
            if (!ValidadorDatos.TryValidarSenia(Tb_Senia.Text, total, out senia, out string error))
            {
                MostrarError(Er_Senia, error);
                return false;
            }
            OcultarError(Er_Senia);
            return true;
        }

        // ==================================================================
        // Confirmar
        // ==================================================================

        private void Btn_Confirmar_Click(object sender, RoutedEventArgs e)
        {
            RecalcularResumen();
            if (!Btn_Confirmar.IsEnabled) return;
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

                decimal total = _reservaBLL.CalcularTotal(habitacion, servicios);
                if (!TryObtenerSenia(total, out _)) return;

                int idReserva = _reservaBLL.RegistrarReserva(_huespedElegido, habitacion, servicios);

                Resumen.MostrarId(idReserva);
                Resumen.MostrarEstado("Reserva confirmada.");

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
                RecalcularResumen();
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
