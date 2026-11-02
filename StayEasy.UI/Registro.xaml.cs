using StayEasy.BLL;
using System;
using StayEasy.Seguridad.Validaciones;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace StayEasy.UI
{
    public partial class Registro : Window
    {

        public Registro()
        {
            InitializeComponent();
            SetupEventHandlers();
            ActualizarTipoCuenta();
            ValidateUsername();
        }

        // Personal interno (Administración / Recepción / Limpieza)
        UsuarioSeguridadBLL usuario = new UsuarioSeguridadBLL();

        // Huésped autogestionado (correo fuera del dominio corporativo)
        HuespedBLL huespedBLL = new HuespedBLL();

        private void SetupEventHandlers()
        {
            Panel_Arrastre.MouseLeftButtonDown += Panel_Arrastre_MouseLeftButtonDown;
            MainGrid.MouseLeftButtonDown += MainGrid_MouseLeftButtonDown;

            void Vincular(TextBox entrada, TextBlock placeholder)
            {
                entrada.TextChanged += (_, _) =>
                {
                    placeholder.Visibility = entrada.Text.Length == 0 ? Visibility.Visible : Visibility.Collapsed;
                    ValidateForm();
                };
            }

            Vincular(Txt_Nombre, Ph_Nombre);
            Vincular(Txt_Apellido, Ph_Apellido);
            Vincular(Txt_Correo, Ph_Correo);
            Vincular(Txt_Usuario, Ph_Usuario);
            Vincular(Txt_DNI, Ph_DNI);
            Txt_Usuario.TextChanged += (_, _) => ValidateUsername();
            Txt_Correo.TextChanged += Txt_Correo_TextChanged;
            CampoClave.PasswordChanged += (_, _) =>
            {
                IndicadorClave.Actualizar(GetPassword());
                CheckPasswordMatch();
            };
            CampoConfirmacion.PasswordChanged += (_, _) => CheckPasswordMatch();
            Chk_Terminos.Checked += (_, _) => ValidateForm();
            Chk_Terminos.Unchecked += (_, _) => ValidateForm();
            Btn_CrearCuenta.Click += Btn_CrearCuenta_Click;
            Btn_IniciarSesion.Click += Btn_IniciarSesion_Click;
        }

        private void Panel_Arrastre_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState != MouseButtonState.Pressed)
                return;

            // Verificar que no se hizo clic en un elemento interactivo dentro del panel oscuro
            if (e.OriginalSource is FrameworkElement element)
            {
                // Si el elemento es interactivo, no arrastrar
                if (element is Button ||
                    element is TextBox ||
                    element is PasswordBox ||
                    element is CheckBox ||
                    element is RadioButton ||
                    element is ToggleButton ||
                    element is ComboBox ||
                    element is ListBox ||
                    element is ScrollViewer)
                {
                    return;
                }

                // Si el elemento es un TextBlock (etiquetas de texto), permitir arrastre
                if (element is TextBlock)
                {
                    try { this.DragMove(); } catch { }
                    return;
                }
            }

            try
            {
                this.DragMove();
            }
            catch (InvalidOperationException)
            {
                // Si falla, ignorar
            }
        }

        private void MainGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Verificar que el botón izquierdo del mouse está presionado
            if (e.ButtonState != MouseButtonState.Pressed)
                return;

            // Verificar que no se hizo clic en un elemento interactivo
            if (e.OriginalSource is FrameworkElement element)
            {
                // Si el elemento es un control interactivo, NO arrastrar
                if (element is Button ||
                    element is TextBox ||
                    element is PasswordBox ||
                    element is CheckBox ||
                    element is RadioButton ||
                    element is ToggleButton ||
                    element is ComboBox ||
                    element is ListBox ||
                    element is ScrollViewer)
                {
                    return;
                }

                // Si el elemento es un Border, StackPanel o Grid que contiene controles, NO arrastrar
                if (element is Border || element is StackPanel || element is Grid)
                {
                    // Permitir arrastre solo si es el panel oscuro o el fondo
                    if (element.Name != "Panel_Arrastre" && element.Name != "MainGrid" && element.Name != "Panel_Formulario")
                    {
                        return;
                    }
                }
            }

            // Si no se hizo clic en un control interactivo, arrastrar la ventana
            try
            {
                this.DragMove();
            }
            catch (InvalidOperationException)
            {
                // Si falla, ignorar
            }
        }

        private string GetPassword() { return CampoClave.Password; }

        private string GetPassword2() { return CampoConfirmacion.Password; }

        private void ValidateUsername()
        {
            bool valido = ValidadorDatos.EsUsuarioValido(Txt_Usuario.Text.Trim());
            Chip_Usuario.Background = (Brush)FindResource(valido ? "Rg.Dark" : "Rg.Disabled");
            Lbl_ChipUsuario.Text = valido ? "Formato válido" : "Formato inválido";
            Lbl_ChipUsuario.Foreground = (Brush)FindResource(valido ? "Rg.OnDark" : "Rg.TextFaint");
            ValidateForm();
        }

        private void CheckPasswordMatch()
        {
            string password = GetPassword();
            string confirmacion = GetPassword2();
            bool mostrar = password.Length > 0 && confirmacion.Length > 0;
            bool coinciden = password == confirmacion;
            Lbl_Coincidencia.Visibility = mostrar ? Visibility.Visible : Visibility.Collapsed;
            Lbl_Coincidencia.Text = coinciden ? "✓ Las contraseñas coinciden" : "✗ Las contraseñas no coinciden";
            Lbl_Coincidencia.Foreground = coinciden ? Brushes.SeaGreen : Brushes.Firebrick;
            ValidateForm();
        }

        /// <summary>
        /// Es la misma condición que decide qué rama del formulario mostrar
        /// (roles internos vs. DNI de huésped): correo del dominio corporativo.
        /// </summary>
        private bool EsCorreoCorporativo()
        {
            return ValidadorDatos.EsCorreoCorporativo(Txt_Correo.Text);
        }

        private void ValidateForm()
        {
            Btn_CrearCuenta.IsEnabled = TryValidarFormulario(out string error);
            bool formularioIniciado = Txt_Nombre.Text.Length > 0 || Txt_Apellido.Text.Length > 0
                || Txt_Correo.Text.Length > 0 || Txt_Usuario.Text.Length > 0
                || GetPassword().Length > 0 || GetPassword2().Length > 0 || Txt_DNI.Text.Length > 0;
            Lbl_ErrorValidacion.Text = error;
            Lbl_ErrorValidacion.Visibility = formularioIniciado && error.Length > 0
                ? Visibility.Visible : Visibility.Collapsed;
        }

        private bool TryValidarFormulario(out string error)
        {
            error = string.Empty;
            try
            {
                string nombre = Txt_Nombre.Text.Trim();
                string apellido = Txt_Apellido.Text.Trim();
                if (!ValidadorDatos.EsNombreValido(nombre) || !ValidadorDatos.EsNombreValido(apellido))
                    throw new ArgumentException("Ingresá nombre y apellido válidos de hasta 100 caracteres cada uno.");
                ValidadorDatos.ValidarCuenta(Txt_Usuario.Text.Trim(), GetPassword(),
                    $"{nombre} {apellido}", Txt_Correo.Text.Trim());
                if (GetPassword() != GetPassword2())
                    throw new ArgumentException("Las contraseñas deben coincidir.");
                if (!EsCorreoCorporativo())
                    ValidadorDatos.ValidarDatosHuesped(nombre, apellido, Txt_DNI.Text.Trim(), Txt_Correo.Text.Trim(), null);
                if (Chk_Terminos.IsChecked != true)
                    throw new ArgumentException("Aceptá las condiciones de uso para continuar.");
                return true;
            }
            catch (ArgumentException ex)
            {
                error = ex.Message;
                return false;
            }
        }

        private void Btn_CrearCuenta_Click(object sender, RoutedEventArgs e)
        {
            if (!TryValidarFormulario(out string error))
            {
                MessageBox.Show(error, "Revisá los datos", MessageBoxButton.OK, MessageBoxImage.Warning);
                ValidateForm();
                return;
            }
            try
            {
                if (EsCorreoCorporativo())
                {
                    // Alta de personal interno (Administración / Recepción / Limpieza)
                    string nombreCompleto = $"{Txt_Nombre.Text.Trim()} {Txt_Apellido.Text.Trim()}";
                    usuario.Registrar(Txt_Usuario.Text, GetPassword(), nombreCompleto, Txt_Correo.Text);

                    MessageBox.Show(
                        "Cuenta creada exitosamente. Esperá la confirmación del administrador.",
                        "Registro exitoso",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                else
                {
                    // Autorregistro de Huésped
                    int dni = int.Parse(Txt_DNI.Text);

                    huespedBLL.RegistrarHuesped(
                        Txt_Nombre.Text,
                        Txt_Apellido.Text,
                        dni,
                        Txt_Correo.Text,
                        telefono: null,
                        nombreUsuario: Txt_Usuario.Text,
                        passwordPlana: GetPassword());

                    MessageBox.Show(
                        "¡Cuenta creada! Ya podés iniciar sesión y gestionar tu estadía.",
                        "Registro exitoso",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }

                var login = new Login();
                login.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "No se pudo crear la cuenta", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Btn_IniciarSesion_Click(object sender, RoutedEventArgs e)
        {
            var login = new Login();
            login.Show();
            this.Close();
        }

        private void Rol_Recepcion_Checked(object sender, RoutedEventArgs e)
        {

        }
        public void Txt_Correo_TextChanged(object sender, TextChangedEventArgs e)
        {
            ActualizarTipoCuenta();
        }

        private void ActualizarTipoCuenta()
        {
            bool corporativo = EsCorreoCorporativo();
            Visibility personal = corporativo ? Visibility.Visible : Visibility.Collapsed;
            Visibility huesped = corporativo ? Visibility.Collapsed : Visibility.Visible;
            txt_rol.Visibility = personal;
            Rol_Administracion.Visibility = personal;
            Rol_Limpieza.Visibility = personal;
            Rol_Recepcion.Visibility = personal;
            txt_dni.Visibility = huesped;
            Shell_DNI.Visibility = huesped;
            ValidateForm();
        }
    }
}
