using System;
using System.Drawing;
using CitizenFX;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading.Tasks;
using NoCarJack;

namespace NoCarJack
{
    public class NoCarJack : BaseScript
    {
        const string FILENAME = "NoCarJack.net.dll";
        const string VERSION = "1.0.0";
        const string AUTHOR = "winject";
        const string CONFIG_PATH = "NoCarJack.ini";

        bool SKIP = false;
        bool LOCK = false;
        Timer unlockTimer = new Timer(0);
        Timer skipTimer = new Timer(0);

        Vehicle lastVeh = null;
        Vehicle targetVeh = null;

        public NoCarJack()
        {
            EventHandlers["nocarjack:skipThisFrame"] += new Action<int, int>(Skip);
            base.Tick += OnTick;
        }

        /// <summary>
        /// Disable the vehicle check temporarily for this player.
        /// Allows the player to get in any vehicle, must be triggered server-side!
        /// </summary>
        /// <param name="playerId">Targetted player ID retrieved with NETWORK_PLAYER_GET_USERID</param>
        /// <param name="time">Time before the granted access expires </param>
        private void Skip(int playerId, int time)
        {
            int id = Game.Player.ServerId;
            if(playerId == id)
            {
                SKIP = true;
                skipTimer.Limit = time;
            }
        }

        /// <summary>
        /// Main loop
        /// </summary>
        /// <returns></returns>
        private async Task OnTick()
        {
            if(SKIP && skipTimer.Expired)
            {
                SKIP = false;
            }

            if(Game.PlayerPed.Exists() && Game.PlayerPed.IsAlive && Game.PlayerPed.CurrentVehicle != null)
            {
                if(Game.PlayerPed.CurrentVehicle.Driver != null)
                {
                    if(Game.PlayerPed.CurrentVehicle.Driver.Handle == Game.PlayerPed.Handle)
                    {
                        lastVeh = Game.PlayerPed.CurrentVehicle;
                    }
                }
            }

            if (Game.PlayerPed.VehicleTryingToEnter != null && Game.PlayerPed.VehicleTryingToEnter.Exists() && !LOCK && !SKIP)
            {
                //Check if vehicle is unlocked and free to be entered
                if (Function.Call<int>(Hash.GET_VEHICLE_DOOR_LOCK_STATUS, Game.PlayerPed.VehicleTryingToEnter) != 2 && Function.Call<int>(Hash.GET_VEHICLE_DOOR_LOCK_STATUS, Game.PlayerPed.VehicleTryingToEnter) != 10)
                {
                    targetVeh = Game.PlayerPed.VehicleTryingToEnter;

                    if (!Game.PlayerPed.IsLucky(90))
                    {
                        if (targetVeh.HasDriver())
                        {
                            if (!IsPedInPlayerList(targetVeh.Driver)) //avoid disabling another's player vehicle
                            {
                                Game.PlayerPed.Task.ClearAll();
                                targetVeh.LockStatus = VehicleLockStatus.CannotBeTriedToEnter;
                                Function.Call(Hash.SET_VEHICLE_UNDRIVEABLE, targetVeh, true);
                                targetVeh.IsEngineRunning = true;
                            }
                            else
                            {

                            }
                        }
                        else
                        {
                            if (!IsVehiclePlayerListLastVehicle(targetVeh))
                            {
                                if(lastVeh != null)
                                {
                                    if(lastVeh.Handle != targetVeh.Handle)
                                    {
                                        Game.PlayerPed.Task.ClearAll();
                                        targetVeh.LockStatus = VehicleLockStatus.CannotBeTriedToEnter;
                                        Function.Call(Hash.SET_VEHICLE_UNDRIVEABLE, targetVeh, true);
                                    }
                                    else
                                    {

                                    }
                                } 
                                else
                                {
                                    Game.PlayerPed.Task.ClearAll();
                                    targetVeh.LockStatus = VehicleLockStatus.CannotBeTriedToEnter;
                                    Function.Call(Hash.SET_VEHICLE_UNDRIVEABLE, targetVeh, true);
                                }                        
                            }
                            else
                            {

                            }
                        }
                    }
                    else
                    {
                        
                    }
                }
                unlockTimer.Limit = 500;
                LOCK = true;
            }

            if(unlockTimer.Expired && LOCK)
            {
                if(Game.PlayerPed.VehicleTryingToEnter != null)
                {
                    if(targetVeh.Handle == Game.PlayerPed.VehicleTryingToEnter.Handle)
                    {
                        unlockTimer.Limit = 1000;
                    }
                    else 
                    {
                        LOCK = !LOCK;
                    }
                }
                else 
                {
                    LOCK = !LOCK;
                }
            }
            await Task.FromResult(0);
        }
       
        /// <summary>
        /// Checks whether the specified ped belongs to a player's ped
        /// </summary>
        /// <param name="ped"></param>
        /// <returns></returns>
        private bool IsPedInPlayerList(Ped ped)
        {
            PlayerList list = base.Players;
            foreach (Player p in list)
            {
                if(p.Character == ped)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks whether the specified vehicle belongs to another player
        /// </summary>
        /// <param name="veh"></param>
        /// <returns></returns>
        private bool IsVehiclePlayerListLastVehicle(Vehicle veh)
        {
            PlayerList list = base.Players;
            foreach (Player p in list)
            {
                if (p.Handle != Game.PlayerPed.Handle)
                {
                    if (p.Character.LastVehicle != null)
                    {
                        if (p.Character.LastVehicle.Handle == veh.Handle)
                        {
                            //CitizenFX.Core.UI.Screen.ShowNotification("~r~PRIVATE VEHICLE");
                            return true;
                        }
                    }
                }
            }
            //CitizenFX.Core.UI.Screen.ShowNotification("~g~PUBLIC VEHICLE");
            return false;
        }
    }
}    
