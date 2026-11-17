using System.Globalization;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using SkiaSharp;
using StayEasy.BE;
using StayEasy.BLL;
using StayEasy.Seguridad;
using StayEasy.UI.Controles;
using System.Windows;
using System.Windows.Controls;

namespace StayEasy.UI
{
    public partial class Dashboard : Window
    {
        // Paleta monocroma del Dashboard (misma que Db.* en Window.Resources).
        private static readonly SKColor ColorDark     = SKColor.Parse("#1C1C1C");
        private static readonly SKColor ColorAnterior = SKColor.Parse("#C9C9C9");
        private static readonly SKColor ColorMid      = SKColor.Parse("#8F8F8F");
        private static readonly SKColor ColorEje      = SKColor.Parse("#B0B0B0");
        private static readonly SKColor ColorGrilla   = SKColor.Parse("#F0F0F0");

        private static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("es-AR");

        private readonly ReporteBLL _reporteBLL = new ReporteBLL();
        private object? _contenidoInicio;

        public ISeries[] SeriesEstadisticas { get; set; }

        public Dashboard()
        {
            InitializeComponent();
            _contenidoInicio = Host_Contenido.Content;

            SeriesEstadisticas = new ISeries[]
            {
                new LineSeries<double>
                {
                    Values = new double[] { 40, 65, 80, 70, 90, 100 },
                    Name = "Ocupación (%)"
                },
                new ColumnSeries<double>
                {
                    Values = new double[] { 12000, 25000, 28000, 21000, 32000, 45000 },
                    Name = "Ingresos ($)"
                }
            };
            this.DataContext = this;

            var usuarioSesion = GestorSesion.Instancia.UsuarioLogueado;
            if (usuarioSesion != null)
            {
                Lbl_Titulo.Text = usuarioSesion.NombreUsuario;
                Lbl_UsuarioNombre.Text = usuarioSesion.NombreUsuario;
                string[] NombreCompleto = (usuarioSesion.NombreCompleto ?? string.Empty)
                    .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                string iniciales = "";
                if (NombreCompleto.Length >= 2)
                {
                    iniciales = $"{NombreCompleto[0][0]}{NombreCompleto[1][0]}".ToUpper();
                }
                Lbl_iniciales.Text = iniciales;
            }

            CargarEstadisticas();
        }

        // ------------------------------------------------------------------
        // Estadísticas (LiveCharts2)
        // ------------------------------------------------------------------

        private void CargarEstadisticas()
        {
            DateTime hoy = DateTime.Today;
            try
            {
                var (actual, anterior) = _reporteBLL.ObtenerOcupacionSemanal(hoy);
                MostrarOcupacion(actual, anterior);
                MostrarIngresos(_reporteBLL.ObtenerIngresosMes(hoy));
            }
            catch (Exception ex)
            {
                // Sin BD el Dashboard tiene que abrir igual: gráficos vacíos y aviso.
                Chart_Ocupacion.Series = Array.Empty<ISeries>();
                Chart_Ingresos.Series = Array.Empty<ISeries>();
                Lbl_DeltaSemana.Text = "—";
                Lbl_Facturado.Text = "Sin datos";
                MessageBox.Show($"No se pudieron cargar las estadísticas.\n\n{ex.Message}",
                                "StayEasy", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void MostrarOcupacion(List<OcupacionDiaria> actual, List<OcupacionDiaria> anterior)
        {
            Chart_Ocupacion.Series = new ISeries[]
            {
                new LineSeries<double>
                {
                    Name = "Anterior",
                    Values = anterior.Select(d => d.Porcentaje).ToArray(),
                    Stroke = new SolidColorPaint(ColorAnterior, 2.2f) { PathEffect = new DashEffect(new[] { 6f, 6f }) },
                    Fill = null,
                    GeometrySize = 0,
                    LineSmoothness = 0.6,
                    YToolTipLabelFormatter = p => FormatearPunto(anterior, p.Index)
                },
                new LineSeries<double>
                {
                    Name = "Esta semana",
                    Values = actual.Select(d => d.Porcentaje).ToArray(),
                    Stroke = new SolidColorPaint(ColorDark, 2.4f),
                    Fill = null,
                    GeometrySize = 8,
                    GeometryStroke = new SolidColorPaint(ColorDark, 2.4f),
                    GeometryFill = new SolidColorPaint(SKColors.White),
                    LineSmoothness = 0.6,
                    YToolTipLabelFormatter = p => FormatearPunto(actual, p.Index)
                }
            };

            Chart_Ocupacion.XAxes = new[]
            {
                new Axis
                {
                    Labels = actual.Select(d => InicialDia(d.Fecha)).ToArray(),
                    LabelsPaint = new SolidColorPaint(ColorEje),
                    TextSize = 11,
                    SeparatorsPaint = null
                }
            };
            Chart_Ocupacion.YAxes = new[]
            {
                new Axis
                {
                    MinLimit = 0,
                    MaxLimit = 100,
                    MinStep = 25,
                    Labeler = v => $"{v:0}%",
                    LabelsPaint = new SolidColorPaint(ColorEje),
                    TextSize = 11,
                    SeparatorsPaint = new SolidColorPaint(ColorGrilla, 1)
                }
            };

            // Promedio semanal vs. semana anterior (en puntos porcentuales).
            double promActual   = actual.Count   > 0 ? actual.Average(d => d.Porcentaje)   : 0;
            double promAnterior = anterior.Count > 0 ? anterior.Average(d => d.Porcentaje) : 0;
            Lbl_DeltaSemana.Text = ConSigno(promActual - promAnterior) + "%";

            // Tarjeta oscura: ocupación de hoy (último día de la serie actual).
            if (actual.Count > 0)
            {
                OcupacionDiaria deHoy = actual[^1];
                Lbl_Ocupacion.Text = $"{deHoy.Porcentaje:0}%";
                Lbl_OcupacionDetalle.Text = $"{deHoy.Ocupadas} de {deHoy.Totales} habitaciones";
                if (actual.Count > 1)
                    Lbl_OcupacionDelta.Text = $"{ConSigno(deHoy.Ocupadas - actual[^2].Ocupadas)} vs. ayer";

                if (Bar_Ocupacion.Parent is Grid barra)
                {
                    double pct = Math.Clamp(deHoy.Porcentaje, 0, 100);
                    barra.ColumnDefinitions[0].Width = new GridLength(pct, GridUnitType.Star);
                    barra.ColumnDefinitions[1].Width = new GridLength(100 - pct, GridUnitType.Star);
                }
            }
        }

        private void MostrarIngresos(IngresosMes ingresos)
        {
            Chart_Ingresos.Series = new ISeries[]
            {
                CrearPorcion("Alojamiento", ingresos.Alojamiento, ColorDark),
                CrearPorcion("Consumos",    ingresos.Consumos,    ColorMid)
            };

            string mes = Cultura.TextInfo.ToTitleCase(Cultura.DateTimeFormat.GetMonthName(ingresos.Mes));
            Lbl_IngresosPeriodo.Text = $"{mes} · a la fecha";
            Lbl_Facturado.Text   = ingresos.Total.ToString("C0", Cultura);
            Lbl_Alojamiento.Text = Abreviar(ingresos.Alojamiento);
            Lbl_Consumos.Text    = Abreviar(ingresos.Consumos);
            Lbl_Objetivo.Text    = ingresos.Total == 0
                ? "0%"
                : $"{ingresos.Alojamiento * 100 / ingresos.Total:0}%";
        }

        private static PieSeries<double> CrearPorcion(string nombre, decimal monto, SKColor color)
        {
            return new PieSeries<double>
            {
                Name = nombre,
                Values = new[] { (double)monto },
                Fill = new SolidColorPaint(color),
                InnerRadius = 38,
                HoverPushout = 6,
                ToolTipLabelFormatter = p => ((decimal)p.Coordinate.PrimaryValue).ToString("C0", Cultura)
            };
        }

        private static string FormatearPunto(List<OcupacionDiaria> serie, int indice)
        {
            if (indice < 0 || indice >= serie.Count) return string.Empty;
            OcupacionDiaria d = serie[indice];
            return $"{d.Fecha.ToString("ddd d", Cultura)}: {d.Porcentaje:0}% ({d.Ocupadas}/{d.Totales})";
        }

        private static string InicialDia(DateTime fecha) =>
            Cultura.DateTimeFormat.GetShortestDayName(fecha.DayOfWeek)[..1].ToUpper(Cultura);

        private static string ConSigno(double valor) =>
            (valor >= 0 ? "+" : "") + valor.ToString("0", Cultura);

        private static string Abreviar(decimal monto) =>
            monto >= 1_000_000 ? $"${monto / 1_000_000:0.0}M"
          : monto >= 1_000     ? $"${monto / 1_000:0.0}K"
          : monto.ToString("C0", Cultura);

        // ------------------------------------------------------------------
        // Eventos
        // ------------------------------------------------------------------

        private void BtnUsuario_Click(object sender, RoutedEventArgs e)
        {
            if (sender is BotonUsuarioAnimado boton)
            {
                MessageBox.Show($"Mensaje de prueba: se hizo clic en \"{boton.Texto}\".",
                                "StayEasy", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BotonPrueba_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Mensaje de prueba: El botón animado funciona correctamente.", "Prueba Evento Click", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Btn_NuevaReserva_Click(object sender, RoutedEventArgs e)
        {
            var reserva = new Reservas();
            reserva.Show();
            this.Close();
        }

        private void Nav_Inicio_Checked(object sender, RoutedEventArgs e)
        {
            if (Host_Contenido is null || _contenidoInicio is null) return;
            Host_Contenido.Content = _contenidoInicio;
        }

        private void Nav_BackupRestore_Checked(object sender, RoutedEventArgs e)
        {
            if (Host_Contenido is null) return;
            var vista = new BackupRestoreView();
            Host_Contenido.Content = vista.ExtraerContenidoMdi();
        }
    }
}
