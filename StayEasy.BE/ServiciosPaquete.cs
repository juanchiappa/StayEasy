using System;
using System.Collections.Generic;
using System.Text;

namespace StayEasy.BE
{
    public class ServiciosPaquete
    {
        public ServiciosPaquete() { }

        public ServiciosPaquete(int idServicio, string nombre, decimal precio, bool esCombo)
        {
            ID_Servicio = idServicio;
            NombreTexto = nombre;
            Nombre = nombre.Length > 0 ? nombre[0] : ' ';
            Precio = precio;
            EsCombo = esCombo;
        }

        public ServiciosPaquete(int iD_Servicio, char nombre, decimal precio, bool esCombo)
        {
            ID_Servicio = iD_Servicio;
            Nombre = nombre;
            NombreTexto = nombre.ToString();
            Precio = precio;
            EsCombo = esCombo;
        }

        public int ID_Servicio { get; set; }
        public char Nombre { get; set; }
        public string NombreTexto { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public bool EsCombo { get; set; }
    }

    // Alias de compatibilidad hacia atrás con el nombre en minúscula
    public class serviciosPaquete : ServiciosPaquete
    {
        public serviciosPaquete() { }

        public serviciosPaquete(int iD_Servicio, char nombre, decimal precio, bool esCombo)
            : base(iD_Servicio, nombre, precio, esCombo) { }
    }
}
