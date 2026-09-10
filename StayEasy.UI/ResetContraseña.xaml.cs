using StayEasy.Seguridad.Entidades;
using StayEasy.Seguridad.Utilidades;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace StayEasy.UI
{
    /// <summary>
    /// Interaction logic for ResetContraseña.xaml
    /// </summary>
    public partial class ResetPassword : Window
    {
        public ResetPassword()
        {
            InitializeComponent();
        }

        private void Btn_SolicitarToken_Click(object sender, RoutedEventArgs e)
        {
            string email = Txt_Mail.Text.Trim();

            if (string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show("Por favor, ingresá tu correo electrónico.");
                return;
            }

            try
            {
                // 1. Publicar el evento para que la BLL genere el token y envíe el correo
                var eventoSolicitud = new SolicitarRecuperacionEvento { Email = email };
                BusEventosNativo.Instancia.Publicar(eventoSolicitud);

                // 2. Transición visual al Paso 2
                PanelPaso1.Visibility = Visibility.Collapsed;
                PanelPaso2.Visibility = Visibility.Visible;

                // Opcional: Poner el cursor automáticamente en el campo del Token
                Txt_Token.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void Btn_RestablecerToken_Click(object sender, RoutedEventArgs e)
        {
            string token = Txt_Token.Text.Trim();
            string nuevaClave = Txt_NuevaClave.Password; // PasswordBox no usa .Text

            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(nuevaClave))
            {
                MessageBox.Show("Por favor, completá el token y tu nueva contraseña.");
                return;
            }

            try
            {
                // 1. Publicar el evento para que la BLL valide el token y actualice la base de datos
                var eventoEjecucion = new EjecutarRecuperacionEvento
                {
                    Token = token,
                    NuevaClave = nuevaClave
                };

                BusEventosNativo.Instancia.Publicar(eventoEjecucion);

                // 2. Mensaje de éxito y cierre de ventana
                MessageBox.Show("Tu contraseña ha sido actualizada con éxito. Ya podés iniciar sesión.", "StayEasy", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
