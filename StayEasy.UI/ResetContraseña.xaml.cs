using System;
using System.Windows;
using StayEasy.Seguridad.Entidades;
using StayEasy.Seguridad.Utilidades;
using StayEasy.Seguridad.Validaciones;

namespace StayEasy.UI
{
    public partial class ResetPassword : Window
    {
        public ResetPassword()
        {
            InitializeComponent();
            Txt_Mail.TextChanged += (_, _) => Ph_Mail.Visibility =
                Txt_Mail.Text.Length == 0 ? Visibility.Visible : Visibility.Collapsed;
            Txt_Token.TextChanged += (_, _) => Ph_Token.Visibility =
                Txt_Token.Text.Length == 0 ? Visibility.Visible : Visibility.Collapsed;
            CampoClave.PasswordChanged += (_, _) => IndicadorClave.Actualizar(CampoClave.Password);
        }

        private void Btn_SolicitarToken_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string email = Txt_Mail.Text.Trim();
                ValidadorDatos.ValidarEmail(email);
                BusEventosNativo.Instancia.Publicar(new SolicitarRecuperacionEvento { Email = email });
                Pnl_Error.Visibility = Visibility.Collapsed;
                PanelPaso1.Visibility = Visibility.Collapsed;
                PanelPaso2.Visibility = Visibility.Visible;
                Txt_Token.Focus();
            }
            catch (Exception ex) { MostrarError(ex.Message); }
        }

        private void Btn_RestablecerToken_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string token = Txt_Token.Text.Trim().ToUpperInvariant();
                if (!ValidadorDatos.EsTokenValido(token))
                    throw new ArgumentException("El código debe contener exactamente 6 caracteres entre 0-9 y A-F.");
                ValidadorDatos.ValidarPassword(CampoClave.Password);
                BusEventosNativo.Instancia.Publicar(new EjecutarRecuperacionEvento
                {
                    Token = token,
                    NuevaClave = CampoClave.Password
                });
                CampoClave.Limpiar();
                MessageBox.Show("Tu contraseña fue actualizada. Ya podés iniciar sesión.",
                    "StayEasy", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
            catch (Exception ex) { MostrarError(ex.Message); }
        }

        private void MostrarError(string mensaje)
        {
            Lbl_Error.Text = mensaje;
            Pnl_Error.Visibility = Visibility.Visible;
        }
    }
}
