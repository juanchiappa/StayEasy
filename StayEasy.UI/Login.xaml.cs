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
using Wpf.Ui.Controls;
using StayEasy.Seguridad;
//Esto es un reemplazo para evitar conflictos con System.Windows.MessageBox, pero no deberia estar
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;

namespace StayEasy.UI
{
    public partial class Login : Window
    {
        private readonly UsuarioSeguridadBLL _seguridadBLL = new UsuarioSeguridadBLL();

        public Login()
        {
            InitializeComponent();
        }

        private void Btn_IniciarSesion_Click(object sender, RoutedEventArgs e)
        {
            // 1. Capturamos los datos
            string usuario = Txt_Usuario.Text;
            string password = Txt_PasswordVisible.Text; 

            try
            {
                // 2. Le pasamos la pelota a la BLL de Seguridad
                _seguridadBLL.Login(usuario, password);

                // 3. Si llega a esta línea, es porque el login fue exitoso (no saltó al catch).
                // Acá leemos el Singleton para darle la bienvenida personalizada (opcional)
                var usuarioLogueado = GestorSesion.Instancia.UsuarioLogueado;
                MessageBox.Show($"¡Bienvenido de nuevo, {usuarioLogueado.NombreCompleto}!",
                                "Login Exitoso", MessageBoxButton.OK, MessageBoxImage.Information);

                // 4. Abrimos la ventana principal del hotel y cerramos el Login
                MainWindow ventanaPrincipal = new MainWindow();
                ventanaPrincipal.Show();
                this.Close();
            }
            catch (UnauthorizedAccessException ex)
            {
                // Atrapamos la excepción específica de credenciales incorrectas
                MessageBox.Show(ex.Message, "Error de Acceso", MessageBoxButton.OK, MessageBoxImage.Warning);
                Txt_PasswordVisible.Clear(); // Limpiamos la clave para que vuelva a intentar
            }
            catch (ArgumentException ex)
            {
                // Atrapamos el error de campos vacíos
                MessageBox.Show(ex.Message, "Faltan datos", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                // Atrapamos cualquier error grave (ej: la base de datos está caída)
                MessageBox.Show($"Ocurrió un error inesperado:\n{ex.Message}", "Error Crítico", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Btn_Registrarse_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Pantalla de registro en construcción...", "Próximamente", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
