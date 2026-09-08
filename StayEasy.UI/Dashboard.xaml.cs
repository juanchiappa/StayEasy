using StayEasy.BLL;
using StayEasy.Seguridad;
using System.Windows;
using System.Windows.Controls;

namespace StayEasy.UI
{
    public partial class Dashboard : Window
    {
        public Dashboard()
        {
            InitializeComponent();
            Lbl_Titulo.Text = GestorSesion.Instancia.UsuarioLogueado.NombreUsuario;
            Lbl_UsuarioNombre.Text = GestorSesion.Instancia.UsuarioLogueado.NombreUsuario;
            string[] NombreCompleto = GestorSesion.Instancia.UsuarioLogueado.NombreCompleto.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            string iniciales = "";

            // Verificar que haya al menos dos palabras
            if (NombreCompleto.Length >= 2)
            {
                // Tomar la primera letra de la primera palabra y la primera de la segunda
                iniciales = $"{NombreCompleto[0][0]}{NombreCompleto[1][0]}".ToUpper();
            }
            Lbl_iniciales.Text = iniciales;
        }
    }
}
