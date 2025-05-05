//using SFD;
//using SFDCT.Helper;
//using Lidgren.Network;
//using HarmonyLib;
//using SFDCT.Sync.Custom;
//using SFD.Weapons;
//using System.Collections.Generic;
//using System;

//namespace SFDCT.Sync
//{
//    [HarmonyPatch]
//    internal static class NetMessagesHandler
//    {
//        public const int CTDataReservedServerUPS = -51;

//        [HarmonyPostfix]
//        [HarmonyPatch(typeof(Server), nameof(Server.SyncAllGameInfo))]
//        private static void ServerSyncAllGameInfo(Server __instance, NetConnection singleTarget = null)
//        {
//            SyncCTWeaponsInfo(__instance, singleTarget);
//        }
//        public static void SyncCTWeaponsInfo(Server __instance, NetConnection singleTarget = null)
//        {
//            if (!__instance.Running)
//            {
//                return;
//            }

//            Logger.LogWarn("SyncCTWeaponsInfo()");
//            foreach (KeyValuePair<short, WeaponItem> ctWpn in Weapons.Database.m_customWeaponsDic)
//            {
//                NetCTData.Data ctData = new NetCTData.Data();

//                NetCTWeapon.Data ctWpnData = new NetCTWeapon.Data();
//                ctWpnData.Weapon = ctWpn.Value;

//                ctData.DataType = NetCTDataType.CTWeapon;
//                ctData.DataObject = ctWpnData;
//                SendNetCTData(ctData, singleTarget);
//            }
//        }

//        [HarmonyPrefix]
//        [HarmonyPatch(typeof(NetMessage.ServerPerformance), nameof(NetMessage.ServerPerformance.Read))]
//        public static bool Read(ref NetMessage.ServerPerformance.Data __result, NetIncomingMessage netIncomingMessage)
//        {
//            __result = new NetMessage.ServerPerformance.Data();
//            __result.ServerUPS = 100;

//            int serverUPS = netIncomingMessage.ReadInt32();

//            // Logger.LogWarn($"NetMessage.ServerPerformance Read: {serverUPS}");

//            if (serverUPS == CTDataReservedServerUPS)
//            {
//                NetCTData.Data ctData;
//                try
//                {
//                    ctData = NetCTData.Read(netIncomingMessage);
//                }
//                catch(Exception ex)
//                {
//                    Logger.LogError(ex);
//                    return false;
//                }
                
//                Logger.LogWarn($"Read NetCTData: {ctData.DataType}");
//                switch (ctData.DataType)
//                {
//                    case NetCTDataType.CTWeapon:
//                        NetCTWeapon.Data ctWpnData = (NetCTWeapon.Data)ctData.DataObject;

//                        Logger.LogWarn($"- '{ctWpnData.Weapon.BaseProperties.WeaponNameID}' ({ctWpnData.Weapon.BaseProperties.WeaponID})");
//                        try
//                        {
//                            if (Weapons.Database.m_customWeaponsDic.ContainsKey(ctWpnData.Weapon.BaseProperties.WeaponID))
//                            {
//                                Logger.LogWarn($"- m_customWeaponsDic already contains '{ctWpnData.Weapon.BaseProperties.WeaponNameID}' ({ctWpnData.Weapon.BaseProperties.WeaponID}), removing...");
//                                Weapons.Database.m_customWeaponsDic.Remove(ctWpnData.Weapon.BaseProperties.WeaponID);
//                            }

//                            Logger.LogWarn($"- Added '{ctWpnData.Weapon.BaseProperties.WeaponNameID}' ({ctWpnData.Weapon.BaseProperties.WeaponID}) to m_customWeaponsDic");
//                            Weapons.Database.m_customWeaponsDic.Add(ctWpnData.Weapon.BaseProperties.WeaponID, ctWpnData.Weapon);
//                        }
//                        catch (Exception ex)
//                        {
//                            Logger.LogError(ex);
//                        }
//                        break;
//                }

//                return false;
//            }
//            else
//            {
//                __result.ServerUPS = serverUPS;
//            }

//            return false;
//        }

//        public static void SendNetCTData(NetCTData.Data dataToSend, NetConnection singleTarget = null)
//        {
//            if (GameSFD.Handle.Server is { NetServer: { } })
//            {
//                NetOutgoingMessage netOutgoingMessage = NetCTData.Write(ref dataToSend, GameSFD.Handle.Server.NetServer.CreateMessage());
//                Logger.LogWarn("Sending NetCTData...");
//                Logger.LogWarn($"- {dataToSend.DataType}");

//                if (singleTarget != null)
//                {
//                    singleTarget.SendMessage(netOutgoingMessage, NetMessage.ServerPerformance.Delivery.Method, NetMessage.ServerPerformance.Delivery.Channel);
//                }
//                else
//                {
//                    GameSFD.Handle.Server.NetServer.SendToAll(netOutgoingMessage, null, NetMessage.ServerPerformance.Delivery.Method, NetMessage.ServerPerformance.Delivery.Channel);
//                }
//            }
//        }
//    }
//}
