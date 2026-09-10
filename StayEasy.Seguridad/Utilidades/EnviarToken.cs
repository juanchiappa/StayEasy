using Microsoft.Data.SqlClient;
using StayEasy.Seguridad.Entidades;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;


namespace StayEasy.Seguridad.Utilidades
{
    public class EnviarToken
    {
        public void ManejarEnvioCorreo(EnviarCorreoEvento ev)
        {
            try
            {
                string correoOrigen = ConfigurationManager.AppSettings["CorreoOrigen"]
                    ?? throw new Exception("Falta configurar 'CorreoOrigen' en los secretos.");

                string claveOrigen = ConfigurationManager.AppSettings["ClaveOrigen"]
                    ?? throw new Exception("Falta configurar 'ClaveOrigen' en los secretos.");

                using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.Credentials = new NetworkCredential(correoOrigen, claveOrigen);
                    smtp.EnableSsl = true;

                    MailMessage mensaje = new MailMessage
                    {
                        From = new MailAddress(correoOrigen),
                        Subject = "StayEasy - Recuperación de Acceso",
                        Body = $"Tu código de verificación es: {ev.Token}\n\nCópialo e ingrésalo en la aplicación para crear tu nueva contraseña."
                    };
                    mensaje.To.Add(ev.Email);

                    smtp.Send(mensaje);
                }
            }
            catch (Exception ex)
            {
                // Manejar error (ej. bitácora)
                throw new Exception("Error al despachar el correo: " + ex.Message);
            }
        }
    }
}
