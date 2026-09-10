using System;
using System.Collections.Generic;
using System.Text;
using StayEasy.MPP;
using StayEasy.Seguridad.Entidades;
using StayEasy.Seguridad.Utilidades;
using StayEasy.Seguridad;

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
            // Opcional: Validar con UsuarioMPP si el correo de verdad existe en el sistema

            // Generamos un token alfanumérico corto (6 caracteres) ideal para copiar/pegar en XAML
            string codigoCorto = Guid.NewGuid().ToString().Substring(0, 6).ToUpper();
            Token nuevoToken = new Token(
                0,
                ev.Email,
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
            var tokenValido = _tokenMPP.ObtenerTokenValido(ev.Token);

            if (tokenValido == null)
                throw new Exception("El código es inválido o ha expirado.");

            byte[] hashBytes= Criptografia.HashearPassword(ev.NuevaClave);
            string hashNuevo = Convert.ToBase64String(hashBytes);
            _tokenMPP.ActualizarContraseña(tokenValido.Email, hashBytes);
            _tokenMPP.EliminarToken(ev.Token);
        }
    }
}
