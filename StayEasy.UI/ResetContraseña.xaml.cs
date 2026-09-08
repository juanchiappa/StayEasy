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
            Btn_SolicitarToken.Content = "Confirmar token";
            lbl_Email.Visibility = Visibility.Collapsed;
            Ph_Usuario.Visibility = Visibility.Collapsed;

            lbl_Token.Visibility = Visibility.Visible;
            Ph_Token.Visibility = Visibility.Visible;


            /*if(Ph_Token.Visibility == Visibility.Visible)
            {
                var nc = new NuevaContraseña();
                nc.Show();
                this.Close();
            }*/

        }
    }
}
