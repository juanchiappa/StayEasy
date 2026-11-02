using System;
using System.Collections.Generic;
using System.Text;
using StayEasy.MPP;
using StayEasy.Seguridad.Entidades;
using StayEasy.Seguridad.Utilidades;
using StayEasy.Seguridad;
using StayEasy.Seguridad.Validaciones;

namespace StayEasy.BLL
{
    public class RecuperacionClaveBLL
    {
        private readonly TokenRecuperacionMPP _tokenMPP; 
        public RecuperacionClaveBLL()
        {
            _tokenMPP = new TokenRecuperacionMPP();
        }

        public void ManejarSolicitud(SolicitarRecuperacionEvento ev)
        {
            string email = ev.Email?.Trim() ?? string.Empty;
            ValidadorDatos.ValidarEmail(email);

            string codigoCorto = Guid.NewGuid().ToString().Substring(0, 6).ToUpper();
            Token nuevoToken = new Token(
                0,
                email,
                codigoCorto,
                DateTime.Now.AddMinutes(15)
                );
            _tokenMPP.GuardarToken(nuevoToken);

            // Disparamos evento para que la clase EnviarToken haga su trabajo
            BusEventosNativo.Instancia.Publicar(new EnviarCorreoEvento
            {
                Email = nuevoToken.Email,
                Token = nuevoToken.token
            });
        }

        public void ManejarEjecucion(EjecutarRecuperacionEvento ev)
        {
            string token = ev.Token?.Trim().ToUpperInvariant() ?? string.Empty;
            if (!ValidadorDatos.EsTokenValido(token))
                throw new ArgumentException("El código debe contener exactamente 6 caracteres entre 0-9 y A-F.");
            ValidadorDatos.ValidarPassword(ev.NuevaClave);
            var tokenValido = _tokenMPP.ObtenerTokenValido(token);

            if (tokenValido == null)
                throw new Exception("El código es inválido o ha expirado.");

            byte[] hashBytes= Criptografia.HashearPassword(ev.NuevaClave);
            _tokenMPP.ActualizarContraseña(tokenValido.Email, hashBytes);
            _tokenMPP.EliminarToken(token);
        }
    }
}
