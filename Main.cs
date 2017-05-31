/*
 * 
 * LICENCE : 2017 
 * THIS SCRIPT CAN ONLY BE DOWNLOADED AND EDITED AT https://github.com/winject/NoCarJack 
 * 
 */

using System;
using System.Drawing;
using CitizenFX;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading.Tasks;
using NoCarJack;
using Screen = CitizenFX.Core.UI.Screen;

namespace NoCarJack
{
    public class NoCarJack : BaseScript
    {
        /*const string FILENAME = "NoCarJack.net.dll";
        const string VERSION = "1.2.1";
        const string AUTHOR = "winject";
        const string CONFIG_PATH = "NoCarJack.ini";*/

        bool SKIP = false;
        bool LOCK = false;
        Timer unlockTimer = new Timer(0);
        Timer skipTimer = new Timer(1);
        Timer updateTimer = new Timer(2);

        const int maxVehicles = 10;
        List<int> vehicleHistory = new List<int>(maxVehicles);
        Vehicle lastVeh = null;
        Vehicle targetVeh = null;

        public NoCarJack()
        {
            EventHandlers["nocarjack:skipThisFrame"] += new Action<int, int>(Skip);
            EventHandlers["nocarjack:addOwnedVehicle"] += new Action<int, int>(Add);
            EventHandlers["nocarjack:removeOwnedVehicle"] += new Action<int, int>(Remove);
            base.Tick += OnTick;
        }

        /// <summary>
        /// Disable the vehicle check temporarily for this player.
        /// Allows the player to get in any vehicle, must be triggered server-side!
        /// </summary>
        /// <param name="playerId">Targetted player ID</param>
        /// <param name="time">Time left before vehicle checking continues again </param>
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
        /// Add an owned vehicle to the vehicle history list which basically allows the player to get in this networked car
        /// </summary>
        /// <param name="playerId">Targetted player ID</param>
        /// <param name="networkVehicleID">Must be retrieved with NETWORK_GET_NETWORK_ID_FROM_ENTITY, works only with MISSION_ENTITY</param>
        private void Add(int playerId, int vehNetworkID)
        {
            int id = Game.Player.ServerId;
            if(playerId == id)
            {
                if(!vehicleHistory.Contains(vehNetworkID) && vehNetworkID != 0 && vehicleHistory.Count < maxVehicles)
                {
                    vehicleHistory.Add(vehNetworkID);
                }
            }
        }

        /// <summary>
        /// Removes the selected vehicle from the vehicle history list
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="networkVehicleID"></param>
        private void Remove(int playerId, int vehNetworkID)
        {
            int id = Game.Player.ServerId;
            if (playerId == id)
            {
                if (vehicleHistory.Contains(vehNetworkID) && vehNetworkID != 0)
                {
                    vehicleHistory.Remove(vehNetworkID);
                }
            }
        }

        /// <summary>
        /// Remove detroyed vehicles since storing them doesn't make any sense
        /// </summary>
        /// <param name="refreshTime">Let the CPU breathe</param>
        private void Update(int refreshTime = 6000)
        {
            if (updateTimer.Expired)
            {
                if (vehicleHistory.Count > 0)
                {
                    for (int i = vehicleHistory.Count - 1; i >= 0; i--)
                    {
                        Vehicle tempVeh = new Vehicle(Function.Call<int>(Hash.NETWORK_GET_ENTITY_FROM_NETWORK_ID, vehicleHistory[i]));
                        if (tempVeh.Exists() && tempVeh.IsDead)
                        {
                            tempVeh.IsPersistent = false;
                            vehicleHistory.Remove(vehicleHistory[i]);
                        }
                    }
                    updateTimer.Limit = refreshTime;
                }
            }
        }

        private void DebugData()
        {
            Debug.Write("--------------------------\n");
            for(int e = 0; e < vehicleHistory.Count; e++)
            {
                Debug.Write(string.Format("Element : {0} \n ID : {1} \n", e, vehicleHistory[e]));
            }
            Debug.Write(string.Format(" Proceeded count : {0} \n --------------------------\n", vehicleHistory.Count));
        }

        /// <summary>
        /// Main loop
        /// </summary>
        /// <returns></returns>
        private async Task OnTick()
        {
            Update();

            if(SKIP && skipTimer.Expired)
            {
                SKIP = false;
            }


            if(Game.PlayerPed.Exists() && Game.PlayerPed.IsAlive && Game.PlayerPed.CurrentVehicle != null)
            {
                if(Game.PlayerPed.CurrentVehicle.Driver != null)
                {
                    if (Game.PlayerPed.CurrentVehicle.Driver.Handle == Game.PlayerPed.Handle)
                    {
                        lastVeh = Game.PlayerPed.CurrentVehicle;
                        Add(Game.Player.ServerId, lastVeh.GetNetworkID());
                    }
                }
            }

            if (Game.PlayerPed.VehicleTryingToEnter != null && Game.PlayerPed.VehicleTryingToEnter.Exists() && !LOCK && !SKIP)
            {
                //Check if vehicle is unlocked and free to be entered
                if (Function.Call<int>(Hash.GET_VEHICLE_DOOR_LOCK_STATUS, Game.PlayerPed.VehicleTryingToEnter) != 2 && Function.Call<int>(Hash.GET_VEHICLE_DOOR_LOCK_STATUS, Game.PlayerPed.VehicleTryingToEnter) != 10)
                {
                    targetVeh = Game.PlayerPed.VehicleTryingToEnter;
                    if(IsVehicleInHistory(targetVeh.GetNetworkID()))
                    {
                        return;
                    }
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
                                if (targetVeh.IsPersistent) targetVeh.IsPersistent = false;
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
                                        if (targetVeh.IsPersistent) targetVeh.IsPersistent = false;
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
                                    if (targetVeh.IsPersistent) targetVeh.IsPersistent = false;
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

        /// <summary>
        /// Checks if a vehicle already exists as a newtork registered vehicle
        /// </summary>
        /// <param name="veh"></param>
        /// <returns></returns>
        private bool IsVehicleInHistory(int vehNetworkID)
        {
            for(int i = vehicleHistory.Count - 1; i >= 0; i--) // Allows the list to be looped even if it gets changed at runtime
            {
                    if (vehicleHistory[i] == vehNetworkID)
                    {
                        return true;
                    }
            }
            return false;
        }
    }
}    
