using StayEasy.BLL;
using StayEasy.Seguridad;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace StayEasy.UI
{
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
            SetupEventHandlers();
        }

        UsuarioSeguridadBLL usuario = new UsuarioSeguridadBLL();

        private void SetupEventHandlers()
        {
            // Arrastre desde el Grid principal
            MainGrid.MouseLeftButtonDown += MainGrid_MouseLeftButtonDown;

            Txt_Usuario.TextChanged += (s, e) =>
                Ph_Usuario.Visibility = string.IsNullOrEmpty(Txt_Usuario.Text)
                    ? Visibility.Visible : Visibility.Collapsed;

            Btn_IniciarSesion.Click += Btn_IniciarSesion_Click;
            Btn_Registrarse.Click += Btn_Registrarse_Click_1;
        }

        private void Panel_Arrastre_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState != MouseButtonState.Pressed)
                return;

            if (e.OriginalSource is FrameworkElement element)
            {
                if (element is Button ||
                    element is TextBox ||
                    element is PasswordBox ||
                    element is CheckBox ||
                    element is RadioButton ||
                    element is ToggleButton)
                {
                    return;
                }
            }

            try { this.DragMove(); } catch { }
        }

        private void MainGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState != MouseButtonState.Pressed)
                return;

            if (e.OriginalSource is FrameworkElement element)
            {
                if (element is Button ||
                    element is TextBox ||
                    element is PasswordBox ||
                    element is CheckBox ||
                    element is RadioButton ||
                    element is ToggleButton)
                {
                    return;
                }
            }

            try { this.DragMove(); } catch { }
        }







        private void Btn_IniciarSesion_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Txt_Usuario.Text) || string.IsNullOrWhiteSpace(CampoClave.Password))
                    throw new ArgumentException("Por favor, ingresá tu usuario y contraseña.");

                usuario.Login(Txt_Usuario.Text, CampoClave.Password);
                if (GestorSesion.Instancia.UsuarioLogueado != null)
                {
                    Pnl_Error.Visibility = Visibility.Collapsed;
                    var dashboard = new Dashboard();
                    dashboard.Show();
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                Pnl_Error.Visibility = Visibility.Visible;
                Lbl_Error.Text = ex.Message;
            }
        }

        private void Btn_Registrarse_Click_1(object sender, RoutedEventArgs e)
        {
            var registro = new Registro();
            registro.Show();
            this.Close();
        }

        private void Btn_OlvidePassword_Click(object sender, RoutedEventArgs e)
        {
            var rp = new ResetPassword();
            rp.Show();
        }
    }
}
