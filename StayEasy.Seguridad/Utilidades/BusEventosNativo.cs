using StayEasy.Seguridad.Entidades;
using System;
using System.Collections.Generic;
using System.Text;

namespace StayEasy.Seguridad.Utilidades
{
    public class BusEventosNativo
    {
        private static BusEventosNativo _instancia;
        public static BusEventosNativo Instancia => _instancia ?? (_instancia = new BusEventosNativo());

        private BusEventosNativo() { }

        // Delegados de los eventos
        public event Action<SolicitarRecuperacionEvento> OnSolicitarRecuperacion;
        public event Action<EnviarCorreoEvento> OnEnviarCorreo;
        public event Action<EjecutarRecuperacionEvento> OnEjecutarRecuperacion;

        // Faltaba este método para que el compilador acepte tu código:
        public void Publicar(EnviarCorreoEvento ev) => OnEnviarCorreo?.Invoke(ev);

        // Y asegúrate de tener también los otros dos:
        public void Publicar(SolicitarRecuperacionEvento ev) => OnSolicitarRecuperacion?.Invoke(ev);
        public void Publicar(EjecutarRecuperacionEvento ev) => OnEjecutarRecuperacion?.Invoke(ev);
    }
}
