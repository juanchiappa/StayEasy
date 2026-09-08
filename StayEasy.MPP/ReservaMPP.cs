using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using StayEasy.BE;
using StayEasy.DAL;
using StayEasy.DAL.Registro;

namespace StayEasy.MPP
{
    public class ReservaMPP
    {
        public int RegistrarReserva(Reserva nuevaReserva, int idUsuarioAccion)
        {
            AccesoDatos dal = new AccesoDatos();
            Hashtable parametros = new Hashtable();

            parametros.Add("@HuespedID", nuevaReserva.ID_Huesped.HuespedID);
            parametros.Add("@HabitacionID", nuevaReserva.Habitacion.ID_habitacion);
            parametros.Add("@FechaCheckIn", nuevaReserva.FechaCheckIn);
            parametros.Add("@FechaCheckOut", nuevaReserva.FechaChekOut);
            parametros.Add("@Total", nuevaReserva.Total);

            //bitacora en sql
            parametros.Add("@UsuarioAccionID", idUsuarioAccion);

            return dal.EscribirEscalar("sp_RegistrarReserva", parametros);
        }
    }
}
