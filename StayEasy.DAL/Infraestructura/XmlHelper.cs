using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace StayEasy.DAL.Infraestructura
{
    /// <summary>
    /// Clase genérica de persistencia XML basada en <see cref="XmlSerializer"/>.
    /// Permite serializar y deserializar cualquier entidad o colección de tipo <typeparamref name="T"/>
    /// hacia y desde archivos físicos XML o cadenas en memoria.
    /// </summary>
    /// <typeparam name="T">Tipo de entidad a persistir (debe ser público y poseer constructor sin parámetros).</typeparam>
    public class XmlHelper<T>
    {
        private static readonly XmlWriterSettings ConfiguracionEscritura = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "  ",
            Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            OmitXmlDeclaration = false
        };

        private static readonly XmlReaderSettings ConfiguracionLectura = new XmlReaderSettings
        {
            IgnoreComments = true,
            IgnoreWhitespace = true,
            DtdProcessing = DtdProcessing.Prohibit
        };

        /// <summary>
        /// Serializa un objeto de tipo <typeparamref name="T"/> en la ruta de archivo XML indicada.
        /// Si el directorio destino no existe, lo crea automáticamente.
        /// </summary>
        public static void Guardar(T datos, string rutaArchivo)
        {
            ArgumentNullException.ThrowIfNull(datos);
            ValidarRuta(rutaArchivo);

            try
            {
                AsegurarDirectorio(rutaArchivo);

                var serializer = new XmlSerializer(typeof(T));
                using var stream = new FileStream(rutaArchivo, FileMode.Create, FileAccess.Write, FileShare.None);
                using var writer = XmlWriter.Create(stream, ConfiguracionEscritura);
                serializer.Serialize(writer, datos);
            }
            catch (Exception ex) when (ex is not DataAccessException)
            {
                throw new DataAccessException(
                    $"Error al guardar el archivo XML en '{rutaArchivo}': {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Deserializa y reconstruye una instancia de tipo <typeparamref name="T"/> desde un archivo XML.
        /// </summary>
        public static T Leer(string rutaArchivo)
        {
            ValidarRuta(rutaArchivo);

            if (!File.Exists(rutaArchivo))
                throw new FileNotFoundException($"No se encontró el archivo XML especificado: '{rutaArchivo}'.", rutaArchivo);

            try
            {
                var serializer = new XmlSerializer(typeof(T));
                using var stream = new FileStream(rutaArchivo, FileMode.Open, FileAccess.Read, FileShare.Read);
                using var reader = XmlReader.Create(stream, ConfiguracionLectura);

                object? resultado = serializer.Deserialize(reader);
                if (resultado is T entidad)
                    return entidad;

                throw new DataAccessException(
                    $"El contenido del archivo '{rutaArchivo}' no pudo deserializarse al tipo '{typeof(T).Name}'.");
            }
            catch (Exception ex) when (ex is not DataAccessException and not FileNotFoundException)
            {
                throw new DataAccessException(
                    $"Error al leer el archivo XML '{rutaArchivo}': {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Serializa una lista de elementos <typeparamref name="T"/> en un archivo XML.
        /// </summary>
        public static void GuardarLista(List<T> lista, string rutaArchivo)
        {
            ArgumentNullException.ThrowIfNull(lista);
            XmlHelper<List<T>>.Guardar(lista, rutaArchivo);
        }

        /// <summary>
        /// Deserializa una lista de elementos <typeparamref name="T"/> desde un archivo XML.
        /// Si el archivo no existe, devuelve una lista vacía.
        /// </summary>
        public static List<T> LeerLista(string rutaArchivo)
        {
            ValidarRuta(rutaArchivo);

            if (!File.Exists(rutaArchivo))
                return new List<T>();

            return XmlHelper<List<T>>.Leer(rutaArchivo);
        }

        /// <summary>
        /// Serializa un objeto <typeparamref name="T"/> y devuelve su representación XML en texto (UTF-8).
        /// </summary>
        public static string SerializarAXml(T datos)
        {
            ArgumentNullException.ThrowIfNull(datos);

            var serializer = new XmlSerializer(typeof(T));
            using var memoryStream = new MemoryStream();
            using (var writer = XmlWriter.Create(memoryStream, ConfiguracionEscritura))
            {
                serializer.Serialize(writer, datos);
            }

            return Encoding.UTF8.GetString(memoryStream.ToArray());
        }

        /// <summary>
        /// Deserializa una cadena XML en memoria hacia un objeto <typeparamref name="T"/>.
        /// </summary>
        public static T DeserializarDesdeXml(string contenidoXml)
        {
            if (string.IsNullOrWhiteSpace(contenidoXml))
                throw new ArgumentException("El contenido XML no puede estar vacío.", nameof(contenidoXml));

            var serializer = new XmlSerializer(typeof(T));
            using var stringReader = new StringReader(contenidoXml);
            using var xmlReader = XmlReader.Create(stringReader, ConfiguracionLectura);

            object? resultado = serializer.Deserialize(xmlReader);
            if (resultado is T entidad)
                return entidad;

            throw new DataAccessException(
                $"No fue posible deserializar el texto XML al tipo '{typeof(T).Name}'.");
        }

        // Alias de instancia por compatibilidad si se instancia XmlHelper<T>
        public void Serializar(T datos, string rutaArchivo) => Guardar(datos, rutaArchivo);
        public T Deserializar(string rutaArchivo) => Leer(rutaArchivo);

        private static void ValidarRuta(string rutaArchivo)
        {
            if (string.IsNullOrWhiteSpace(rutaArchivo))
                throw new ArgumentException("La ruta del archivo XML es obligatoria.", nameof(rutaArchivo));
        }

        private static void AsegurarDirectorio(string rutaArchivo)
        {
            string? directorio = Path.GetDirectoryName(Path.GetFullPath(rutaArchivo));
            if (!string.IsNullOrWhiteSpace(directorio) && !Directory.Exists(directorio))
            {
                Directory.CreateDirectory(directorio);
            }
        }
    }
}
