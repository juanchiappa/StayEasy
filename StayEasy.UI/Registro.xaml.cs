using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace StayEasy.UI
{
    public partial class Registro : Window
    {
        public Registro()
        {
            InitializeComponent();
            SetupEventHandlers();
        }

        private void SetupEventHandlers()
        {
            Txt_Nombre.TextChanged += (s, e) =>
                Ph_Nombre.Visibility = string.IsNullOrEmpty(Txt_Nombre.Text)
                    ? Visibility.Visible : Visibility.Collapsed;

            Txt_Apellido.TextChanged += (s, e) =>
                Ph_Apellido.Visibility = string.IsNullOrEmpty(Txt_Apellido.Text)
                    ? Visibility.Visible : Visibility.Collapsed;

            Txt_Correo.TextChanged += (s, e) =>
                Ph_Correo.Visibility = string.IsNullOrEmpty(Txt_Correo.Text)
                    ? Visibility.Visible : Visibility.Collapsed;

            Txt_Usuario.TextChanged += (s, e) =>
            {
                Ph_Usuario.Visibility = string.IsNullOrEmpty(Txt_Usuario.Text)
                    ? Visibility.Visible : Visibility.Collapsed;
                ValidateUsername();
            };

            Pwd_Password.PasswordChanged += Pwd_Password_PasswordChanged;
            Txt_PasswordVisible.TextChanged += Txt_PasswordVisible_TextChanged;

            Btn_VerPassword.Checked += (s, e) => MostrarPassword(true);
            Btn_VerPassword.Unchecked += (s, e) => MostrarPassword(false);

            Pwd_Password2.PasswordChanged += (s, e) =>
            {
                Ph_Password2.Visibility = string.IsNullOrEmpty(Pwd_Password2.Password)
                    ? Visibility.Visible : Visibility.Collapsed;
                CheckPasswordMatch();
            };
            Txt_Password2Visible.TextChanged += (s, e) =>
            {
                if (Btn_VerPassword.IsChecked == true)
                {
                    Pwd_Password2.Password = Txt_Password2Visible.Text;
                    Ph_Password2.Visibility = string.IsNullOrEmpty(Txt_Password2Visible.Text)
                        ? Visibility.Visible : Visibility.Collapsed;
                    CheckPasswordMatch();
                }
            };

            Chk_Terminos.Checked += (s, e) => ValidateForm();
            Chk_Terminos.Unchecked += (s, e) => ValidateForm();

            Rol_Recepcion.Checked += (s, e) => ValidateForm();
            Rol_Limpieza.Checked += (s, e) => ValidateForm();
            Rol_Administracion.Checked += (s, e) => ValidateForm();

            Btn_CrearCuenta.Click += Btn_CrearCuenta_Click;
            Btn_IniciarSesion.Click += Btn_IniciarSesion_Click;
        }

        private void Pwd_Password_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (Btn_VerPassword.IsChecked == false)
            {
                Ph_Password.Visibility = string.IsNullOrEmpty(Pwd_Password.Password)
                    ? Visibility.Visible : Visibility.Collapsed;
            }
            OnPasswordChanged();
        }

        private void Txt_PasswordVisible_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Btn_VerPassword.IsChecked == true)
            {
                Pwd_Password.Password = Txt_PasswordVisible.Text;
                Ph_Password.Visibility = string.IsNullOrEmpty(Txt_PasswordVisible.Text)
                    ? Visibility.Visible : Visibility.Collapsed;
                OnPasswordChanged();
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

                Txt_Password2Visible.Text = Pwd_Password2.Password;
                Txt_Password2Visible.Visibility = Visibility.Visible;
                Pwd_Password2.Visibility = Visibility.Collapsed;
                Ph_Password2.Visibility = string.IsNullOrEmpty(Txt_Password2Visible.Text)
                    ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                Pwd_Password.Password = Txt_PasswordVisible.Text;
                Pwd_Password.Visibility = Visibility.Visible;
                Txt_PasswordVisible.Visibility = Visibility.Collapsed;
                Ph_Password.Visibility = string.IsNullOrEmpty(Pwd_Password.Password)
                    ? Visibility.Visible : Visibility.Collapsed;

                Pwd_Password2.Password = Txt_Password2Visible.Text;
                Pwd_Password2.Visibility = Visibility.Visible;
                Txt_Password2Visible.Visibility = Visibility.Collapsed;
                Ph_Password2.Visibility = string.IsNullOrEmpty(Pwd_Password2.Password)
                    ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private string GetPassword()
        {
            if (Pwd_Password.Visibility == Visibility.Visible)
                return Pwd_Password.Password;
            else
                return Txt_PasswordVisible.Text;
        }

        private string GetPassword2()
        {
            if (Pwd_Password2.Visibility == Visibility.Visible)
                return Pwd_Password2.Password;
            else
                return Txt_Password2Visible.Text;
        }

        private void ValidateUsername()
        {
            string username = Txt_Usuario.Text;
            bool isValid = username.Length >= 3;

            if (isValid)
            {
                Chip_Usuario.Background = (Brush)FindResource("Rg.Dark");
                Lbl_ChipUsuario.Text = "Disponible";
                Lbl_ChipUsuario.Foreground = (Brush)FindResource("Rg.OnDark");
            }
            else
            {
                Chip_Usuario.Background = (Brush)FindResource("Rg.Disabled");
                Lbl_ChipUsuario.Text = "Muy corto";
                Lbl_ChipUsuario.Foreground = (Brush)FindResource("Rg.TextFaint");
            }

            ValidateForm();
        }

        private void OnPasswordChanged()
        {
            string password = GetPassword();
            UpdatePasswordStrength(password);
            UpdateRequirements(password);
            CheckPasswordMatch();
            ValidateForm();
        }

        private void UpdatePasswordStrength(string password)
        {
            int score = CalculatePasswordStrength(password);
            string strengthText;
            SolidColorBrush[] segmentColors = new SolidColorBrush[4];
            SolidColorBrush defaultSegment = (SolidColorBrush)FindResource("Rg.Track");

            for (int i = 0; i < 4; i++)
            {
                segmentColors[i] = defaultSegment;
            }

            if (string.IsNullOrEmpty(password))
            {
                strengthText = "Muy débil";
                Lbl_Fuerza.Foreground = (Brush)FindResource("Rg.Label");
            }
            else if (score <= 1)
            {
                strengthText = "Muy débil";
                segmentColors[0] = new SolidColorBrush(Color.FromRgb(255, 77, 77));
                Lbl_Fuerza.Foreground = new SolidColorBrush(Color.FromRgb(255, 77, 77));
            }
            else if (score == 2)
            {
                strengthText = "Débil";
                segmentColors[0] = segmentColors[1] = new SolidColorBrush(Color.FromRgb(255, 165, 0));
                Lbl_Fuerza.Foreground = new SolidColorBrush(Color.FromRgb(255, 165, 0));
            }
            else if (score == 3)
            {
                strengthText = "Media";
                segmentColors[0] = segmentColors[1] = segmentColors[2] = new SolidColorBrush(Color.FromRgb(255, 205, 0));
                Lbl_Fuerza.Foreground = new SolidColorBrush(Color.FromRgb(255, 205, 0));
            }
            else if (score == 4)
            {
                strengthText = "Fuerte";
                segmentColors[0] = segmentColors[1] = segmentColors[2] = segmentColors[3] = new SolidColorBrush(Color.FromRgb(76, 175, 80));
                Lbl_Fuerza.Foreground = new SolidColorBrush(Color.FromRgb(76, 175, 80));
            }
            else
            {
                strengthText = "Muy fuerte";
                segmentColors[0] = segmentColors[1] = segmentColors[2] = segmentColors[3] = new SolidColorBrush(Color.FromRgb(0, 150, 136));
                Lbl_Fuerza.Foreground = new SolidColorBrush(Color.FromRgb(0, 150, 136));
            }

            Lbl_Fuerza.Text = strengthText;

            Seg_1.Background = segmentColors[0];
            Seg_2.Background = segmentColors[1];
            Seg_3.Background = segmentColors[2];
            Seg_4.Background = segmentColors[3];
        }

        private int CalculatePasswordStrength(string password)
        {
            if (string.IsNullOrEmpty(password)) return 0;

            int score = 0;

            if (password.Length >= 8) score++;
            if (password.Length >= 12) score++;
            if (Regex.IsMatch(password, "[A-Z]")) score++;
            if (Regex.IsMatch(password, "[a-z]")) score++;
            if (Regex.IsMatch(password, "[0-9]")) score++;
            if (Regex.IsMatch(password, "[^a-zA-Z0-9]")) score++;

            return Math.Min(score, 5);
        }

        private void UpdateRequirements(string password)
        {
            bool req1 = password.Length >= 8;
            UpdateRequirement(Dot_Req1, Lbl_Req1, req1);

            bool req2 = Regex.IsMatch(password, "[A-Z]");
            UpdateRequirement(Dot_Req2, Lbl_Req2, req2);

            bool req3 = Regex.IsMatch(password, "[0-9]");
            UpdateRequirement(Dot_Req3, Lbl_Req3, req3);
        }

        private void UpdateRequirement(Ellipse dot, TextBlock label, bool isMet)
        {
            if (isMet)
            {
                dot.Fill = new SolidColorBrush(Color.FromRgb(76, 175, 80));
                dot.Stroke = new SolidColorBrush(Color.FromRgb(76, 175, 80));
                label.Foreground = (Brush)FindResource("Rg.TextBody");
            }
            else
            {
                dot.Fill = new SolidColorBrush(Colors.Transparent);
                dot.Stroke = new SolidColorBrush(Color.FromRgb(212, 212, 212));
                label.Foreground = (Brush)FindResource("Rg.TextFaint");
            }
        }

        private void CheckPasswordMatch()
        {
            string password1 = GetPassword();
            string password2 = GetPassword2();

            if (string.IsNullOrEmpty(password1) || string.IsNullOrEmpty(password2))
            {
                Lbl_Coincidencia.Visibility = Visibility.Collapsed;
                Shell_Password2.BorderBrush = (Brush)FindResource("Rg.FieldBorder");
                return;
            }

            if (password1 == password2)
            {
                Lbl_Coincidencia.Text = "✓ Las contraseñas coinciden";
                Lbl_Coincidencia.Foreground = new SolidColorBrush(Color.FromRgb(76, 175, 80));
                Lbl_Coincidencia.Visibility = Visibility.Visible;
                Shell_Password2.BorderBrush = new SolidColorBrush(Color.FromRgb(76, 175, 80));
            }
            else
            {
                Lbl_Coincidencia.Text = "✗ Las contraseñas no coinciden";
                Lbl_Coincidencia.Foreground = new SolidColorBrush(Color.FromRgb(255, 77, 77));
                Lbl_Coincidencia.Visibility = Visibility.Visible;
                Shell_Password2.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 77, 77));
            }

            ValidateForm();
        }

        private void ValidateForm()
        {
            bool isValid = true;

            if (Txt_Usuario.Text.Length < 3)
                isValid = false;

            string password = GetPassword();
            if (password.Length < 8 ||
                !Regex.IsMatch(password, "[A-Z]") ||
                !Regex.IsMatch(password, "[0-9]"))
                isValid = false;

            string password2 = GetPassword2();
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(password2) || password != password2)
                isValid = false;

            if (Chk_Terminos.IsChecked != true)
                isValid = false;

            Btn_CrearCuenta.IsEnabled = isValid;
        }

        private void Btn_CrearCuenta_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Cuenta creada exitosamente. Esperá la confirmación del administrador.",
                "Registro exitoso",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            var login = new Login();
            login.Show();
            this.Close();
        }

        private void Btn_IniciarSesion_Click(object sender, RoutedEventArgs e)
        {
            var login = new Login();
            login.Show();
            this.Close();
        }
    }
}
