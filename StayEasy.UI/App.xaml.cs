using StayEasy.BLL;
using StayEasy.Seguridad.Utilidades;
using System.Configuration;
using System.Data;
using System.Windows;

namespace StayEasy.UI;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // 1. Instanciamos las clases que contienen la lógica
        var recuperacionBLL = new RecuperacionClaveBLL();
        var enviarToken = new EnviarToken();

        // 2. SUSCRIBIMOS LAS FUNCIONES A LOS EVENTOS (El paso que falta)
        // Cuando alguien publique "Solicitar", ejecuta "ManejarSolicitud"
        BusEventosNativo.Instancia.OnSolicitarRecuperacion += recuperacionBLL.ManejarSolicitud;

        // Cuando la BLL publique "EnviarCorreo", ejecuta "ManejarEnvioCorreo"
        BusEventosNativo.Instancia.OnEnviarCorreo += enviarToken.ManejarEnvioCorreo;

        // Cuando alguien publique "Ejecutar", ejecuta "ManejarEjecucion"
        BusEventosNativo.Instancia.OnEjecutarRecuperacion += recuperacionBLL.ManejarEjecucion;
    }
}

