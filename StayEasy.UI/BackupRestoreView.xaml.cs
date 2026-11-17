using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using StayEasy.BE;
using StayEasy.BLL;
using StayEasy.DAL.Infraestructura;
using StayEasy.Seguridad;

namespace StayEasy.UI
{
    /// <summary>
    /// Ventana MDI en WPF para gestionar Copias de Seguridad (sp_BackupBaseDatos),
    /// Restauración de Base de Datos (usp_Restore_StayEasyDB / sp_RestoreBaseDatos)
    /// y Persistencia XML mediante <see cref="XmlHelper{T}"/>.
    /// </summary>
    public partial class BackupRestoreView : Window
    {
        private readonly BackupRestoreBLL _backupRestoreBLL = new BackupRestoreBLL();

        public BackupRestoreView()
        {
            InitializeComponent();
            InicializarVista();
        }

        /// <summary>
        /// Permite alojar el cuerpo de la vista directamente dentro del contenedor MDI
        /// (<c>Host_Contenido</c>) del <see cref="Dashboard"/> sin abrir una ventana externa.
        /// </summary>
        public UIElement ExtraerContenidoMdi()
        {
            if (Pnl_ContenidoMdi.Parent is Panel panelPadre)
            {
                panelPadre.Children.Remove(Pnl_ContenidoMdi);
            }
            Pnl_ContenidoMdi.Margin = new Thickness(0);
            return Pnl_ContenidoMdi;
        }

        private void InicializarVista()
        {
            var usuario = GestorSesion.Instancia.UsuarioLogueado;
            if (usuario is not null)
            {
                Lbl_UsuarioActivo.Text = $"{usuario.NombreUsuario} · StayEasyDB";
            }

            string nombreSugerido = $"StayEasyDB_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
            Txt_RutaBackup.Text = Path.Combine(BackupRestoreBLL.CarpetaBackupsPorDefecto, nombreSugerido);

            RecargarHistorialXml();
        }

        private void RecargarHistorialXml()
        {
            try
            {
                List<RegistroBackupXml> historial = _backupRestoreBLL.ObtenerHistorialXml();
                Lst_HistorialXml.ItemsSource = historial;
                Lbl_ContadorHistorial.Text = historial.Count == 1 ? "1 registro" : $"{historial.Count} registros";
                Txt_PreviewXml.Text = _backupRestoreBLL.ObtenerVistaPreviaXmlHistorial();

                if ( string.IsNullOrWhiteSpace(Txt_RutaRestore.Text) && historial.Count > 0)
                {
                    Txt_RutaRestore.Text = historial[0].RutaArchivoBak;
                }
            }
            catch (Exception ex)
            {
                MostrarEstado($"No se pudo leer el catálogo XML: {ex.Message}", esError: true);
            }
        }

        // ==================================================================
        // 01 · Backup (sp_BackupBaseDatos)
        // ==================================================================

        private void Btn_ExaminarBackup_Click(object sender, RoutedEventArgs e)
        {
            var dialogo = new SaveFileDialog
            {
                Title = "Seleccionar destino para el Backup de StayEasyDB",
                Filter = "Copia de seguridad SQL Server (*.bak)|*.bak",
                DefaultExt = ".bak",
                InitialDirectory = BackupRestoreBLL.CarpetaBackupsPorDefecto,
                FileName = $"StayEasyDB_{DateTime.Now:yyyyMMdd_HHmmss}.bak"
            };

            if (dialogo.ShowDialog() == true)
            {
                Txt_RutaBackup.Text = dialogo.FileName;
            }
        }

        private void Btn_EjecutarBackup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Btn_EjecutarBackup.IsEnabled = false;
                string ruta = Txt_RutaBackup.Text.Trim();

                RegistroBackupXml registro = _backupRestoreBLL.EjecutarBackup(ruta);
                Txt_RutaRestore.Text = registro.RutaArchivoBak;
                RecargarHistorialXml();

                MostrarEstado($"Backup completado exitosamente en '{registro.NombreArchivo}' ({registro.TamanoFormateado}).", esError: false);
                MessageBox.Show(
                    $"Copia de seguridad generada correctamente mediante {registro.ProcedimientoEjecutado}.\n\n" +
                    $"Archivo: {registro.RutaArchivoBak}\n" +
                    $"Tamaño: {registro.TamanoFormateado}\n" +
                    $"Auditoría XML actualizada en: {BackupRestoreBLL.RutaCatalogoHistorialXml}",
                    "StayEasy · Backup Exitoso", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MostrarEstado(ex.Message, esError: true);
                MessageBox.Show(ex.Message, "Error al generar Backup", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Btn_EjecutarBackup.IsEnabled = true;
            }
        }

        // ==================================================================
        // 02 · Restore (usp_Restore_StayEasyDB / sp_RestoreBaseDatos)
        // ==================================================================

        private void Btn_ExaminarRestore_Click(object sender, RoutedEventArgs e)
        {
            var dialogo = new OpenFileDialog
            {
                Title = "Seleccionar archivo .bak para restaurar StayEasyDB",
                Filter = "Copia de seguridad SQL Server (*.bak)|*.bak",
                DefaultExt = ".bak",
                InitialDirectory = BackupRestoreBLL.CarpetaBackupsPorDefecto,
                CheckFileExists = true
            };

            if (dialogo.ShowDialog() == true)
            {
                Txt_RutaRestore.Text = dialogo.FileName;
            }
        }

        private void Btn_EjecutarRestore_Click(object sender, RoutedEventArgs e)
        {
            string ruta = Txt_RutaRestore.Text.Trim();
            if (string.IsNullOrWhiteSpace(ruta))
            {
                MostrarEstado("Seleccioná un archivo .bak antes de iniciar la restauración.", esError: true);
                return;
            }

            MessageBoxResult confirmacion = MessageBox.Show(
                $"¿Confirmás la restauración completa de la base de datos StayEasyDB desde el archivo seleccionado?\n\n" +
                $"Origen: {ruta}\n\n" +
                $"Advertencia: Esta operación cerrará las conexiones activas y reemplazará los datos actuales.",
                "Confirmar Restauración (Restore)",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirmacion != MessageBoxResult.Yes) return;

            try
            {
                Btn_EjecutarRestore.IsEnabled = false;
                RegistroBackupXml registro = _backupRestoreBLL.EjecutarRestore(ruta);
                RecargarHistorialXml();

                MostrarEstado($"Base de datos restaurada desde '{registro.NombreArchivo}' ({registro.ProcedimientoEjecutado}).", esError: false);
                MessageBox.Show(
                    $"La base de datos StayEasyDB fue restaurada exitosamente.\n\n" +
                    $"Procedimiento ejecutado: {registro.ProcedimientoEjecutado}\n" +
                    $"Archivo origen: {registro.RutaArchivoBak}",
                    "StayEasy · Restore Completado", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MostrarEstado(ex.Message, esError: true);
                MessageBox.Show(ex.Message, "Error al restaurar la base de datos", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Btn_EjecutarRestore.IsEnabled = true;
            }
        }

        // ==================================================================
        // 03 · Persistencia XML (XmlHelper<T>)
        // ==================================================================

        private void Btn_ExportarReservasXml_Click(object sender, RoutedEventArgs e)
        {
            var dialogo = new SaveFileDialog
            {
                Title = "Exportar Reservas a XML (XmlHelper<Reserva>)",
                Filter = "Documento XML (*.xml)|*.xml",
                DefaultExt = ".xml",
                InitialDirectory = BackupRestoreBLL.CarpetaBackupsPorDefecto,
                FileName = $"Reservas_Export_{DateTime.Now:yyyyMMdd_HHmmss}.xml"
            };

            if (dialogo.ShowDialog() != true) return;

            try
            {
                int cantidad = _backupRestoreBLL.ExportarReservasAXml(dialogo.FileName);
                Txt_PreviewXml.Text = File.ReadAllText(dialogo.FileName);
                MostrarEstado($"Se exportaron {cantidad} reserva(s) a XML usando XmlHelper<Reserva>.", esError: false);
            }
            catch (Exception ex)
            {
                MostrarEstado(ex.Message, esError: true);
                MessageBox.Show(ex.Message, "Error en exportación XML", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Btn_ImportarReservasXml_Click(object sender, RoutedEventArgs e)
        {
            var dialogo = new OpenFileDialog
            {
                Title = "Leer Reservas desde archivo XML (XmlHelper<Reserva>)",
                Filter = "Documento XML (*.xml)|*.xml",
                DefaultExt = ".xml",
                InitialDirectory = BackupRestoreBLL.CarpetaBackupsPorDefecto,
                CheckFileExists = true
            };

            if (dialogo.ShowDialog() != true) return;

            try
            {
                List<Reserva> reservas = _backupRestoreBLL.ImportarReservasDesdeXml(dialogo.FileName);
                Txt_PreviewXml.Text = XmlHelper<List<Reserva>>.SerializarAXml(reservas);
                MostrarEstado($"Se deserializaron {reservas.Count} reserva(s) correctamente con XmlHelper<Reserva>.", esError: false);
            }
            catch (Exception ex)
            {
                MostrarEstado(ex.Message, esError: true);
                MessageBox.Show(ex.Message, "Error en lectura XML", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Btn_VerXmlHistorial_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Txt_PreviewXml.Text = _backupRestoreBLL.ObtenerVistaPreviaXmlHistorial();
                MostrarEstado($"Mostrando XML de '{BackupRestoreBLL.RutaCatalogoHistorialXml}'.", esError: false);
            }
            catch (Exception ex)
            {
                MostrarEstado(ex.Message, esError: true);
            }
        }

        private void Lst_HistorialXml_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Lst_HistorialXml.SelectedItem is RegistroBackupXml seleccionado)
            {
                Txt_RutaRestore.Text = seleccionado.RutaArchivoBak;
                Txt_PreviewXml.Text = XmlHelper<RegistroBackupXml>.SerializarAXml(seleccionado);
            }
        }

        // ==================================================================
        // Navegación y ventana
        // ==================================================================

        private void BarraEncabezado_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                try { DragMove(); } catch { }
            }
        }

        private void Btn_Cerrar_Click(object sender, RoutedEventArgs e)
        {
            if (Owner is null)
            {
                var dashboard = new Dashboard();
                dashboard.Show();
            }
            Close();
        }

        private void MostrarEstado(string mensaje, bool esError)
        {
            Lbl_EstadoOperacion.Text = mensaje;
            Dot_Estado.Fill = (Brush)FindResource(esError ? "Danger" : "Success");
        }
    }
}
