using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace StayEasy.UI.Controles;

/// <summary>
/// Botón reutilizable que cambia de verde a rojo y se agranda 100 px al
/// pasar el cursor, y expone un evento <see cref="Click"/> enrutado.
/// </summary>
public partial class BotonUsuarioAnimado : UserControl
{
    public const double CRECIMIENTO = 100;

    private static readonly Color ColorReposo = (Color)ColorConverter.ConvertFromString("#2E9E5B");
    private static readonly Color ColorHover  = (Color)ColorConverter.ConvertFromString("#D64545");
    private static readonly Duration DuracionAnimacion = new(TimeSpan.FromMilliseconds(250));

    public static readonly DependencyProperty TextoProperty = DependencyProperty.Register(
        nameof(Texto), typeof(string), typeof(BotonUsuarioAnimado), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty AnchoBaseProperty = DependencyProperty.Register(
        nameof(AnchoBase), typeof(double), typeof(BotonUsuarioAnimado),
        new PropertyMetadata(160d, (d, _) => ((BotonUsuarioAnimado)d).AplicarAnchoBase()));

    public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent(
        nameof(Click), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(BotonUsuarioAnimado));

    public BotonUsuarioAnimado()
    {
        InitializeComponent();
        AplicarAnchoBase();
        MouseEnter += (_, _) => Animar(ColorHover, AnchoBase + CRECIMIENTO);
        MouseLeave += (_, _) => Animar(ColorReposo, AnchoBase);
    }

    public string Texto
    {
        get => (string)GetValue(TextoProperty);
        set => SetValue(TextoProperty, value);
    }

    /// <summary>Ancho en reposo; con el cursor encima mide AnchoBase + 100.</summary>
    public double AnchoBase
    {
        get => (double)GetValue(AnchoBaseProperty);
        set => SetValue(AnchoBaseProperty, value);
    }

    public event RoutedEventHandler Click
    {
        add => AddHandler(ClickEvent, value);
        remove => RemoveHandler(ClickEvent, value);
    }

    private void AplicarAnchoBase()
    {
        // Sin animación activa, el ancho local es el de reposo.
        BeginAnimation(WidthProperty, null);
        Width = AnchoBase;
    }

    private void Animar(Color color, double ancho)
    {
        var easing = new CubicEase { EasingMode = EasingMode.EaseOut };

        Pincel.BeginAnimation(SolidColorBrush.ColorProperty,
            new ColorAnimation(color, DuracionAnimacion) { EasingFunction = easing });

        // Sin From: arranca desde el valor actual, así una salida a mitad de
        // la animación de entrada vuelve suave sin saltos.
        BeginAnimation(WidthProperty,
            new DoubleAnimation(ancho, DuracionAnimacion) { EasingFunction = easing });
    }

    private void Btn_Click(object sender, RoutedEventArgs e)
    {
        RaiseEvent(new RoutedEventArgs(ClickEvent, this));
    }
}
