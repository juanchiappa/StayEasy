using StayEasy.BLL;
using System.Windows;
using StayEasy.Seguridad;
using System.Windows.Controls;

namespace StayEasy.UI
{
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
            Txt_Usuario.TextChanged += (s, e) =>
                Ph_Usuario.Visibility = string.IsNullOrEmpty(Txt_Usuario.Text)
                    ? Visibility.Visible : Visibility.Collapsed;
            Pwd_Password.PasswordChanged += Pwd_Password_PasswordChanged;
            Txt_PasswordVisible.TextChanged += Txt_PasswordVisible_TextChanged;
            Btn_VerPassword.Checked += (s, e) => MostrarPassword(true);
            Btn_VerPassword.Unchecked += (s, e) => MostrarPassword(false);

            Btn_IniciarSesion.Click += Btn_IniciarSesion_Click;
            Btn_Registrarse.Click += Btn_Registrarse_Click_1;
        }
        UsuarioSeguridadBLL usuario = new UsuarioSeguridadBLL();

        private void Pwd_Password_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (Btn_VerPassword.IsChecked == false)
            {
                Ph_Password.Visibility = string.IsNullOrEmpty(Pwd_Password.Password)
                    ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void Txt_PasswordVisible_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Btn_VerPassword.IsChecked == true)
            {
                Pwd_Password.Password = Txt_PasswordVisible.Text;
                Ph_Password.Visibility = string.IsNullOrEmpty(Txt_PasswordVisible.Text)
                    ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void MostrarPassword(bool mostrar)
        {
            if (mostrar)
            {
                Txt_PasswordVisible.Text = Pwd_Password.Password;
                Txt_PasswordVisible.Visibility = Visibility.Visible;
                Pwd_Password.Visibility = Visibility.Collapsed;
                Ph_Password.Visibility = string.IsNullOrEmpty(Txt_PasswordVisible.Text)
                    ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                Pwd_Password.Password = Txt_PasswordVisible.Text;
                Pwd_Password.Visibility = Visibility.Visible;
                Txt_PasswordVisible.Visibility = Visibility.Collapsed;
                Ph_Password.Visibility = string.IsNullOrEmpty(Pwd_Password.Password)
                    ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void Btn_IniciarSesion_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Txt_Usuario.Text) || string.IsNullOrWhiteSpace(Pwd_Password.Password))
            {
                Pnl_Error.Visibility = Visibility.Visible;
                Lbl_Error.Text = "Por favor, ingresá tu usuario y contraseña.";
                return;
            }
            usuario.Login(Txt_Usuario.Text, Pwd_Password.Password);
            if(GestorSesion.Instancia.UsuarioLogueado != null)
            {
                var Dashboard = new Dashboard();
                Dashboard.Show();
                this.Close();
            }

            Pnl_Error.Visibility = Visibility.Collapsed;
        }


        private void Btn_Registrarse_Click_1(object sender, RoutedEventArgs e)
        {
            var registro = new Registro();
            registro.Show();
            this.Close();
        }
    }
}