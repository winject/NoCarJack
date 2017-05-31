
using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoCarJack
{
    public static class Extension
    {
        static Random rnd = new Random();

        /// <summary>
        /// Is vehicle empty, parked or in motion?
        /// </summary>
        /// <param name="veh"></param>
        /// <returns></returns>
        public static bool HasDriver(this Vehicle veh)
        {
            bool hasDriver = false;
            if(veh.Exists() && veh.IsAlive)
            {
                if(veh.Driver != null && veh.Driver.Exists() && veh.Driver.IsAlive)
                {
                    hasDriver = true;
                }
            }
            return hasDriver;
        }

        /// <summary>
        /// Defines whether the player can actually get inside the vehicle
        /// </summary>
        /// <returns></returns>
        public static bool IsLucky(this Ped ped, int value)
        {
            int i = rnd.Next(0, 101);
            return i >= value; //luck factor
        }

        public static bool IsValid(this Vehicle veh)
        {
            return veh != null && veh.Exists() && veh.IsAlive && veh.Speed < 5.0f;
        }

        public static bool IsConnected(this Player plyr)
        {
            return Function.Call<bool>(Hash.NETWORK_IS_PLAYER_CONNECTED, plyr);
        }

        public static Player GetPlayerIndex(this Ped ped)
        {
            return Function.Call<Player>(Hash._NETWORK_GET_PED_PLAYER, ped);
        }

        /// <summary>
        /// Returns the real ID on the server and makes it persistent
        /// </summary>
        /// <param name="veh"></param>
        /// <returns></returns>
        public static int GetNetworkID(this Vehicle veh)
        {
            if(!veh.IsPersistent)
            {
                veh.IsPersistent = true;
            }
            return Function.Call<int>(Hash.NETWORK_GET_NETWORK_ID_FROM_ENTITY, veh.Handle);
        }

     }
}
