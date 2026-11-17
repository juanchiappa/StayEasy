using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using StayEasy.BE;
using StayEasy.DAL.Infraestructura;
using StayEasy.MPP;
using StayEasy.Seguridad;

namespace StayEasy.BLL
{
    /// <summary>
    /// Lógica de negocio para la administración de Copias de Seguridad (Backup / Restore)
    /// y Persistencia XML (<see cref="XmlHelper{T}"/>).
    /// </summary>
    public class BackupRestoreBLL
    {
        public const int MAX_LONGITUD_RUTA_SP = 260;

        private readonly BackupRestoreDAL _backupRestoreDAL = new BackupRestoreDAL();
        private readonly ReservaMPP _reservaMPP = new ReservaMPP();

        /// <summary>
        /// Ruta por defecto donde se persiste el catálogo XML de historial de backups y restores.
        /// </summary>
        public static string RutaCatalogoHistorialXml =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DatosXml", "HistorialBackupRestore.xml");

        /// <summary>
        /// Carpeta sugerida por defecto para almacenar archivos .bak y exportaciones .xml.
        /// </summary>
        public static string CarpetaBackupsPorDefecto
        {
            get
            {
                string carpeta = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backups");
                if (!Directory.Exists(carpeta))
                    Directory.CreateDirectory(carpeta);
                return carpeta;
            }
        }

        /// <summary>
        /// Ejecuta el backup de la base de datos mediante <c>sp_BackupBaseDatos</c>
        /// y registra la operación en el catálogo XML local usando <see cref="XmlHelper{T}"/>.
        /// </summary>
        public RegistroBackupXml EjecutarBackup(string rutaArchivoBak)
        {
            ValidarRutaBak(rutaArchivoBak, exigirExistencia: false);

            string? directorio = Path.GetDirectoryName(Path.GetFullPath(rutaArchivoBak));
            if (!string.IsNullOrWhiteSpace(directorio) && !Directory.Exists(directorio))
                Directory.CreateDirectory(directorio);

            _backupRestoreDAL.RealizarBackup(rutaArchivoBak);

            long tamano = File.Exists(rutaArchivoBak) ? new FileInfo(rutaArchivoBak).Length : 0L;

            var registro = new RegistroBackupXml(
                tipoOperacion: "BACKUP",
                rutaArchivoBak: Path.GetFullPath(rutaArchivoBak),
                nombreArchivo: Path.GetFileName(rutaArchivoBak),
                fechaHora: DateTime.Now,
                usuarioEjecutor: ObtenerNombreUsuarioSesion(),
                tamanoBytes: tamano,
                procedimientoEjecutado: BackupRestoreDAL.SP_BACKUP,
                estado: "Completado");

            RegistrarEnHistorialXml(registro);
            return registro;
        }

        /// <summary>
        /// Ejecuta la restauración de la base de datos mediante <c>usp_Restore_StayEasyDB</c>
        /// (o <c>sp_RestoreBaseDatos</c>) y persiste el evento en el catálogo XML.
        /// </summary>
        public RegistroBackupXml EjecutarRestore(string rutaArchivoBak)
        {
            ValidarRutaBak(rutaArchivoBak, exigirExistencia: true);

            long tamano = new FileInfo(rutaArchivoBak).Length;
            string spEjecutado = _backupRestoreDAL.RealizarRestore(rutaArchivoBak);

            var registro = new RegistroBackupXml(
                tipoOperacion: "RESTORE",
                rutaArchivoBak: Path.GetFullPath(rutaArchivoBak),
                nombreArchivo: Path.GetFileName(rutaArchivoBak),
                fechaHora: DateTime.Now,
                usuarioEjecutor: ObtenerNombreUsuarioSesion(),
                tamanoBytes: tamano,
                procedimientoEjecutado: spEjecutado,
                estado: "Restaurado");

            RegistrarEnHistorialXml(registro);
            return registro;
        }

        /// <summary>
        /// Obtiene el historial de operaciones de Backup y Restore persistido en XML.
        /// </summary>
        public List<RegistroBackupXml> ObtenerHistorialXml(string? rutaXml = null)
        {
            string ruta = string.IsNullOrWhiteSpace(rutaXml) ? RutaCatalogoHistorialXml : rutaXml;
            return XmlHelper<RegistroBackupXml>.LeerLista(ruta)
                .OrderByDescending(r => r.FechaHora)
                .ToList();
        }

        /// <summary>
        /// Exporta el listado actual de reservas de la base de datos a un archivo XML
        /// utilizando <see cref="XmlHelper{T}"/>.
        /// </summary>
        public int ExportarReservasAXml(string rutaArchivoXml)
        {
            if (string.IsNullOrWhiteSpace(rutaArchivoXml))
                throw new ArgumentException("Seleccioná una ruta válida para exportar el archivo XML.", nameof(rutaArchivoXml));

            List<Reserva> reservas = _reservaMPP.Listar();
            XmlHelper<Reserva>.GuardarLista(reservas, rutaArchivoXml);
            return reservas.Count;
        }

        /// <summary>
        /// Importa/lee un listado de reservas desde un archivo XML utilizando <see cref="XmlHelper{T}"/>.
        /// </summary>
        public List<Reserva> ImportarReservasDesdeXml(string rutaArchivoXml)
        {
            if (string.IsNullOrWhiteSpace(rutaArchivoXml))
                throw new ArgumentException("Seleccioná un archivo XML válido para importar.", nameof(rutaArchivoXml));

            return XmlHelper<Reserva>.LeerLista(rutaArchivoXml);
        }

        /// <summary>
        /// Genera la representación XML en texto del historial actual para previsualización.
        /// </summary>
        public string ObtenerVistaPreviaXmlHistorial()
        {
            List<RegistroBackupXml> historial = ObtenerHistorialXml();
            return XmlHelper<List<RegistroBackupXml>>.SerializarAXml(historial);
        }

        private void RegistrarEnHistorialXml(RegistroBackupXml nuevoRegistro)
        {
            List<RegistroBackupXml> historial = XmlHelper<RegistroBackupXml>.LeerLista(RutaCatalogoHistorialXml);
            historial.Insert(0, nuevoRegistro);
            XmlHelper<RegistroBackupXml>.GuardarLista(historial, RutaCatalogoHistorialXml);
        }

        private static void ValidarRutaBak(string rutaArchivo, bool exigirExistencia)
        {
            if (string.IsNullOrWhiteSpace(rutaArchivo))
                throw new ArgumentException("Debés seleccionar una ruta de archivo .bak.");

            string rutaCompleta = Path.GetFullPath(rutaArchivo.Trim());

            if (rutaCompleta.Length > MAX_LONGITUD_RUTA_SP)
                throw new ArgumentException(
                    $"La ruta supera los {MAX_LONGITUD_RUTA_SP} caracteres admitidos por el parámetro @RutaArchivo VARCHAR(260) del Stored Procedure.");

            if (!rutaCompleta.EndsWith(".bak", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("El archivo de copia de seguridad debe tener extensión .bak.");

            if (exigirExistencia && !File.Exists(rutaCompleta))
                throw new FileNotFoundException("El archivo de backup (.bak) seleccionado no existe en el disco.", rutaCompleta);
        }

        private static string ObtenerNombreUsuarioSesion()
        {
            var usuario = GestorSesion.Instancia.UsuarioLogueado;
            return usuario?.NombreUsuario ?? "admin_local";
        }
    }
}
