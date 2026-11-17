using System;
using System.Xml.Serialization;

namespace StayEasy.BE
{
    /// <summary>
    /// Entidad serializable en XML para auditar y persistir localmente el historial
    /// de copias de seguridad (Backup) y restauraciones (Restore) ejecutadas en el sistema.
    /// </summary>
    [Serializable]
    [XmlRoot("RegistroBackup")]
    public class RegistroBackupXml
    {
        public RegistroBackupXml() { }

        public RegistroBackupXml(string tipoOperacion, string rutaArchivoBak, string nombreArchivo,
                                 DateTime fechaHora, string usuarioEjecutor, long tamanoBytes,
                                 string procedimientoEjecutado, string estado)
        {
            Id = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
            TipoOperacion = tipoOperacion;
            RutaArchivoBak = rutaArchivoBak;
            NombreArchivo = nombreArchivo;
            FechaHora = fechaHora;
            UsuarioEjecutor = usuarioEjecutor;
            TamanoBytes = tamanoBytes;
            ProcedimientoEjecutado = procedimientoEjecutado;
            Estado = estado;
        }

        [XmlAttribute("Id")]
        public string Id { get; set; } = string.Empty;

        [XmlElement("TipoOperacion")]
        public string TipoOperacion { get; set; } = string.Empty; // "BACKUP" | "RESTORE"

        [XmlElement("NombreArchivo")]
        public string NombreArchivo { get; set; } = string.Empty;

        [XmlElement("RutaArchivoBak")]
        public string RutaArchivoBak { get; set; } = string.Empty;

        [XmlElement("FechaHora")]
        public DateTime FechaHora { get; set; }

        [XmlElement("UsuarioEjecutor")]
        public string UsuarioEjecutor { get; set; } = string.Empty;

        [XmlElement("TamanoBytes")]
        public long TamanoBytes { get; set; }

        [XmlElement("ProcedimientoEjecutado")]
        public string ProcedimientoEjecutado { get; set; } = string.Empty;

        [XmlElement("Estado")]
        public string Estado { get; set; } = string.Empty;

        [XmlIgnore]
        public string TamanoFormateado =>
            TamanoBytes <= 0 ? "—"
          : TamanoBytes >= 1_048_576 ? $"{TamanoBytes / 1_048_576.0:0.00} MB"
          : $"{TamanoBytes / 1024.0:0.0} KB";

        [XmlIgnore]
        public string FechaFormateada => FechaHora.ToString("dd/MM/yyyy HH:mm:ss");
    }
}
