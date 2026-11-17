using System;
using System.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;

namespace StayEasy.DAL.Infraestructura
{
    /// <summary>
    /// Acceso a datos para operaciones de Backup y Restore de la base de datos SQL Server.
    /// Conecta con los Stored Procedures <see cref="SP_BACKUP"/> (<c>sp_BackupBaseDatos</c>)
    /// y <see cref="SP_RESTORE"/> (<c>usp_Restore_StayEasyDB</c> / <c>sp_RestoreBaseDatos</c>).
    /// </summary>
    public class BackupRestoreDAL
    {
        public const string SP_BACKUP = "sp_BackupBaseDatos";
        public const string SP_RESTORE = "usp_Restore_StayEasyDB";
        public const string SP_RESTORE_SCRIPT_ACTUAL = "sp_RestoreBaseDatos";
        public const string PARAM_RUTA_ARCHIVO = "@RutaArchivo";

        private readonly string _connectionString =
            ConfigurationManager.ConnectionStrings["StayEasyDB"]?.ConnectionString
            ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'StayEasyDB' en App.config.");

        /// <summary>
        /// Ejecuta el backup completo de StayEasyDB hacia el archivo .bak indicado
        /// invocando el Stored Procedure <c>sp_BackupBaseDatos</c>.
        /// </summary>
        public void RealizarBackup(string rutaArchivo)
        {
            if (string.IsNullOrWhiteSpace(rutaArchivo))
                throw new ArgumentException("La ruta del archivo de backup es obligatoria.", nameof(rutaArchivo));

            try
            {
                using var conexion = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand(SP_BACKUP, conexion)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = 300
                };

                cmd.Parameters.Add(new SqlParameter(PARAM_RUTA_ARCHIVO, SqlDbType.VarChar, 260)
                {
                    Value = rutaArchivo
                });

                conexion.Open();
                cmd.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                throw new DataAccessException(
                    $"Error de SQL Server al ejecutar {SP_BACKUP}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Ejecuta la restauración de StayEasyDB desde el archivo .bak indicado.
        /// Intenta invocar primero <c>usp_Restore_StayEasyDB</c>; si no existe en el servidor (error 2812),
        /// invoca <c>sp_RestoreBaseDatos</c> definido en <c>StayEasyDB_Script.sql</c>.
        /// Si SQL Server rechaza el RESTORE por estar la sesión activa en StayEasyDB (errores 3101/3102),
        /// ejecuta la operación desde el catálogo <c>master</c>.
        /// </summary>
        /// <returns>Nombre del procedimiento o mecanismo efectivamente ejecutado.</returns>
        public string RealizarRestore(string rutaArchivo)
        {
            if (string.IsNullOrWhiteSpace(rutaArchivo))
                throw new ArgumentException("La ruta del archivo de restore es obligatoria.", nameof(rutaArchivo));

            // Liberar todas las conexiones del pool para no bloquear SET SINGLE_USER
            SqlConnection.ClearAllPools();

            try
            {
                EjecutarSpRestore(SP_RESTORE, rutaArchivo);
                return SP_RESTORE;
            }
            catch (SqlException ex) when (ex.Number == 2812) // Could not find stored procedure 'usp_Restore_StayEasyDB'
            {
                try
                {
                    EjecutarSpRestore(SP_RESTORE_SCRIPT_ACTUAL, rutaArchivo);
                    return SP_RESTORE_SCRIPT_ACTUAL;
                }
                catch (SqlException exInner) when (exInner.Number is 3101 or 3102 or 5069)
                {
                    EjecutarRestoreDesdeMaster(rutaArchivo);
                    return $"{SP_RESTORE_SCRIPT_ACTUAL} (vía master)";
                }
                catch (SqlException exInner)
                {
                    throw new DataAccessException(
                        $"Error de SQL Server al ejecutar {SP_RESTORE_SCRIPT_ACTUAL}: {exInner.Message}", exInner);
                }
            }
            catch (SqlException ex) when (ex.Number is 3101 or 3102 or 5069)
            {
                // Error 3101/3102: RESTORE no puede procesar StayEasyDB porque está en uso por esta misma sesión.
                EjecutarRestoreDesdeMaster(rutaArchivo);
                return $"{SP_RESTORE} (vía master)";
            }
            catch (SqlException ex)
            {
                throw new DataAccessException(
                    $"Error de SQL Server al restaurar la base de datos: {ex.Message}", ex);
            }
        }

        private void EjecutarSpRestore(string nombreSp, string rutaArchivo)
        {
            using var conexion = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(nombreSp, conexion)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 300
            };

            cmd.Parameters.Add(new SqlParameter(PARAM_RUTA_ARCHIVO, SqlDbType.VarChar, 260)
            {
                Value = rutaArchivo
            });

            conexion.Open();
            cmd.ExecuteNonQuery();
        }

        private void EjecutarRestoreDesdeMaster(string rutaArchivo)
        {
            SqlConnection.ClearAllPools();

            var builder = new SqlConnectionStringBuilder(_connectionString)
            {
                InitialCatalog = "master",
                Pooling = false
            };

            const string sqlRestore = @"
                ALTER DATABASE [StayEasyDB] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                RESTORE DATABASE [StayEasyDB] FROM DISK = @RutaArchivo WITH REPLACE;
                ALTER DATABASE [StayEasyDB] SET MULTI_USER;";

            using var conexionMaster = new SqlConnection(builder.ConnectionString);
            using var cmd = new SqlCommand(sqlRestore, conexionMaster)
            {
                CommandType = CommandType.Text,
                CommandTimeout = 300
            };

            cmd.Parameters.Add(new SqlParameter(PARAM_RUTA_ARCHIVO, SqlDbType.VarChar, 260)
            {
                Value = rutaArchivo
            });

            conexionMaster.Open();
            cmd.ExecuteNonQuery();
        }
    }
}
