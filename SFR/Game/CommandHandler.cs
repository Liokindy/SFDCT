using System;
using System.Linq;
using Microsoft.Xna.Framework;
using SFD;
using SFD.Core;
using SFD.States;
using SFD.Voting;
using SFD.ManageLists;
using SFD.Objects;
using SFD.Weapons;
using Lidgren.Network;
using HarmonyLib;
using CConst = SFDCT.Misc.Constants;
using System.Security.AccessControl;

namespace SFDCT.Game;

[HarmonyPatch]
internal static class CommandHandler
{
    private static string GetSlotState(int slotIndex, int slotState, int slotTeam, bool occupiedByHuman = false)
    {
        string state = slotState == 0 ? "Opened" : "Closed";
        Team team = (Team)Math.Max(Math.Min(slotTeam, 4), 0);

        if (slotState >= 2)
        {
            string diff = "Expert";
            switch(slotState)
            {
                case 2:
                    diff = "Easy";
                    break;
                case 4:
                    diff = "Normal";
                    break;
                case 5:
                    diff = "Hard";
                    break;
            }
            state = $"Bot ({diff})";
        }
        if (occupiedByHuman)
        {
            state = "Player";
        }

        return $"({slotIndex}) {state} - {team}";
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameInfo), nameof(GameInfo.HandleCommand), typeof(ProcessCommandArgs))]
    private static bool GameInfo_HandleCommands(ref bool __result, ProcessCommandArgs args, GameInfo __instance)
    {
        bool ranCustomCommand = false;

        // We check to see if we handle custom commands before vanilla ones,
        // this also allows us to replace them.
        if (__instance.GameOwner != GameOwnerEnum.Server || __instance.GameOwner == GameOwnerEnum.Local)
        {
            // Client
            ranCustomCommand = ClientCommands(args, __instance);
        }
        if (__instance.GameOwner == GameOwnerEnum.Server || __instance.GameOwner == GameOwnerEnum.Local)
        {
            // Server
            ranCustomCommand = ServerCommands(args, __instance);
        }

        // Don't execute the original method if a custom command was run.
        __result = ranCustomCommand;
        return !ranCustomCommand;
    }
    private static bool ClientCommands(ProcessCommandArgs args, GameInfo __instance)
    {
        if (args.IsCommand(
            "PLAYERS",
            "LISTPLAYERS",
            "SHOWPLAYERS",
            "USERS",
            "LISTUSERS",
            "SHOWUSERS"
        ))
        {
            int players = 0;
            int bots = 0;

            string header = "- Listing all users in the lobby...";
            args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, header, Color.ForestGreen, args.SenderGameUser));

            foreach (GameUser gameUser in __instance.GetGameUsers().OrderBy(gu => gu.GameSlotIndex))
            {
                if (gameUser.IsDedicatedPreview) { continue; } // Vanilla hides the dedicated preview from the list

                string slotIndex, latency, profileName, accountName, powerStatus;
                slotIndex = gameUser.GameSlotIndex.ToString();
                latency = Math.Min(gameUser.Ping, 9999) + "ms";

                // Modified clients can illegaly modify these, bypassing their maximum lengths.
                profileName = gameUser.GetProfileName().Substring(0, Math.Min(gameUser.GetProfileName().Length, 32));
                accountName = gameUser.AccountName.Substring(0, Math.Min(gameUser.AccountName.Length, 32));
            
                // Check IsHost first, hosts are counted as moderators
                powerStatus = gameUser.IsHost ? "- HOST" : (gameUser.IsModerator ? "- MOD" : "");

                string mess = "?";
                if (gameUser.IsBot)
                {
                    // Bots don't have ping, or an account. Less clutter
                    mess = $"{slotIndex}: \"{profileName}\" - BOT";
                    bots++;
                }
                else
                {
                    mess = $"{slotIndex} - {latency}: \"{profileName}\" ({accountName}) {powerStatus}";
                    players++;
                }
                args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, mess, Color.LightBlue, args.SenderGameUser));
            }

            string info = $"- {players} player(s)" + (bots > 0 ? $" and {bots} bot(s)" : "");
            args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, info, Color.ForestGreen, args.SenderGameUser));

            return true;
        }
        return false;
    }
    private static bool ServerCommands(ProcessCommandArgs args, GameInfo __instance)
    {
        // Host-only commands
        if (args.HostPrivileges)
        {
            // Debug mouse
            if (args.IsCommand("MOUSE", "M") && args.Parameters.Count > 0)
            {
                if (args.Parameters[0] == "1" || args.Parameters[0].ToUpperInvariant() == "TRUE")
                {
                    Commands.DebugMouse.IsEnabled = true;
                }
                if (args.Parameters[0] == "0" || args.Parameters[0].ToUpperInvariant() == "FALSE")
                {
                    Commands.DebugMouse.IsEnabled = false;
                }

                string mess = "Mouse control " + (Commands.DebugMouse.IsEnabled ? "enabled" : "disabled");
                args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, mess));
                return true;
            }

            // List slot states
            if (CConst.HOST_GAME_SLOT_COUNT > 8 && args.IsCommand("SLOTS"))
            {
                for (int i = 0; i < CConst.HOST_GAME_SLOT_COUNT; i++)
                {
                    string mess = "- " + GetSlotState(__instance.GetGameSlotByIndex(i).GameSlotIndex, CConst.HOST_GAME_SLOT_STATES[i], (int)CConst.HOST_GAME_SLOT_TEAMS[i], __instance.GetGameSlotByIndex(i).IsOccupiedByUser);
                    args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, mess, Color.Yellow));
                }
                return true;
            }

            // Manually set a slot state and team
            if (CConst.HOST_GAME_SLOT_COUNT > 8 && args.IsCommand("SETSLOT", "SSLOT") && args.Parameters.Count >= 2)
            {
                if (int.TryParse(args.Parameters[0], out int slotIndex))
                {
                    slotIndex = Math.Min(Math.Max(slotIndex, 0), CConst.HOST_GAME_SLOT_COUNT - 1);
                    int slotState = 0;
                    int slotTeam = (int)Constants.GET_HOST_GAME_SLOT_TEAM(slotIndex);

                    if (args.Parameters[1].ToUpper() == "OPENED" || args.Parameters[1].ToUpper() == "0")
                    {
                        slotState = 0;
                    }
                    else if (args.Parameters[1].ToUpper() == "CLOSED" || args.Parameters[1].ToUpper() == "1")
                    {
                        slotState = 1;
                    }
                    else if (args.Parameters[1].ToUpper() == "EASY" || args.Parameters[1].ToUpper() == "2")
                    {
                        slotState = 2;
                    }
                    else if (args.Parameters[1].ToUpper() == "NORMAL" || args.Parameters[1].ToUpper() == "4")
                    {
                        slotState = 3;
                    }
                    else if (args.Parameters[1].ToUpper() == "HARD" || args.Parameters[1].ToUpper() == "5")
                    {
                        slotState = 5;
                    }
                    else if (args.Parameters[1].ToUpper() == "EXPERT" || args.Parameters[1].ToUpper() == "6")
                    {
                        slotState = 6;
                    }

                    if (args.Parameters.Count >= 3)
                    {
                        if (args.Parameters[2].ToUpper() == "INDEPENDENT" || args.Parameters[2].ToUpper() == "0")
                        {
                            slotTeam = 0;
                        }
                        else if (args.Parameters[2].ToUpper() == "TEAM1" || args.Parameters[2].ToUpper() == "1")
                        {
                            slotTeam = 1;
                        }
                        else if (args.Parameters[2].ToUpper() == "TEAM2" || args.Parameters[2].ToUpper() == "2")
                        {
                            slotTeam = 2;
                        }
                        else if (args.Parameters[2].ToUpper() == "TEAM3" || args.Parameters[2].ToUpper() == "3")
                        {
                            slotTeam = 3;
                        }
                        else if (args.Parameters[2].ToUpper() == "TEAM4" || args.Parameters[2].ToUpper() == "4")
                        {
                            slotTeam = 4;
                        }
                    }

                    string messSlotBefore = GetSlotState(slotIndex, Constants.GET_HOST_GAME_SLOT_STATE(slotIndex), (int)Constants.GET_HOST_GAME_SLOT_TEAM(slotIndex));

                    __instance.GameSlots[slotIndex].CurrentTeam = (Team)slotTeam;
                    __instance.GameSlots[slotIndex].NextTeam = (Team)slotTeam;
                    Constants.SET_HOST_GAME_SLOT_TEAM(slotIndex, (Team)slotTeam);
                    LobbyCommandHandler.LobbyStatus_LobbySlotValueChanged(__instance, slotIndex, slotState);

                    string messSlotAfter = GetSlotState(slotIndex, slotState, slotTeam);
                    string mess = messSlotBefore + " -> " + messSlotAfter;
                    args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, mess, Color.ForestGreen));

                    GameSFD.Handle.Server.SyncGameSlotsInfo();
                }
                return true;
            }
        }
        
        // Moderator commands
        if (args.HostPrivileges || args.ModeratorPrivileges)
        {
            // Commands that interact with the gameworld, such as GIVE
            if (__instance.GameWorld != null)
            {
                // Better /GIVE command, allows for multiple weapons in a single command.
                // i.e /GIVE ME KNIFE MAGNUM 21 SNIPER_RIFLE BOUNCING_AMMO GRENADES
                if (args.IsCommand("GIVE") && args.CanUseModeratorCommand("GIVE") && args.Parameters.Count > 1)
                {
                    Color fbColor = Color.ForestGreen;

                    GameUser selectedUser = __instance.GetGameUserByStringInput(args.Parameters[0], args.SenderGameUser);
                    Player worldPlayer;
                
                    if (selectedUser == null)
                    {
                        return false;
                    }

                    worldPlayer = __instance.GameWorld.GetPlayerByUserIdentifier(selectedUser.UserIdentifier);
                    if (worldPlayer == null || worldPlayer.IsDisposed || worldPlayer.IsRemoved)
                    {
                        return false;
                    }

                    // Start at 1 so we don't use the user parameter
                    for(int i = 1; i < args.Parameters.Count; i++)
                    {
                        string parameter = args.Parameters[i].ToUpper();
                        if (string.IsNullOrEmpty(parameter))
                        {
                            continue;
                        }

                        // Heal the player to full health
                        if (parameter == "LIFE" || parameter == "HEAL" || parameter == "HEALTH")
                        {
                            args.Feedback.Add(new(args.SenderGameUser, $"{worldPlayer.Name} was healed.", fbColor));
                            worldPlayer.HealAmount(worldPlayer.Health.MaxValue);

                            // Visual and audio feedback for the player
                            worldPlayer.GrabWeaponItem(WeaponDatabase.GetWeapon("+50"));
                            continue;
                        }

                        // Give player ammo for his handgun and rifle
                        if (parameter == "AMMO")
                        {
                            bool resupplyAmmo = false;

                            if (worldPlayer.CurrentHandgunWeapon != null)
                            {
                                worldPlayer.CurrentHandgunWeapon.FillAmmoMax();
                                resupplyAmmo = true;
                            }
                            if (worldPlayer.CurrentRifleWeapon != null)
                            {
                                worldPlayer.CurrentRifleWeapon.FillAmmoMax();
                                resupplyAmmo = true;
                            }

                            if (resupplyAmmo)
                            {
                                args.Feedback.Add(new(args.SenderGameUser, $"{worldPlayer.Name} was supplied with ammo.", fbColor));
                            }
                            continue;
                        }

                        // Funny
                        if (args.HostPrivileges && parameter == "ALL")
                        {
                            short[] WeaponIDs = [
                                // 22, 7, 68
                                24,1,28,2,17,6,5,19,26,
                                3,4,31,8,11,41,10,12,18,
                                13,14,15,16,20,25,27,29,9,23,
                                42,43,44,45,21,30,32,33,58,34,
                                35,36,37,38,39,40,49,47,48,46,
                                50,51,52,53,55,54,57,56,59,62,
                                61,63,64,65,66,67
                            ];

                            foreach(short id in WeaponIDs)
                            {
                                worldPlayer.GrabWeaponItem(WeaponDatabase.GetWeapon(id));
                            }

                            args.Feedback.Add(new(args.SenderGameUser, $"{worldPlayer.Name} received a lot of weapons.", fbColor));
                            break;
                        }

                        // Spawn a streetsweeper or streetsweepercrate
                        if (parameter == "SW" || parameter == "SWC" || parameter == "STREETSWEEPER" || parameter == "STREETSWEEPERCRATE")
                        {
                            string objectName = (parameter == "SW" ? "STREETSWEEPER" : (parameter == "SWC" ? "STREETSWEEPERCRATE" : parameter));
                            args.Feedback.Add(new(args.SenderGameUser, $"{worldPlayer.Name} was delivered a {objectName}.", fbColor));

                            SpawnObjectInformation spawnObject = new(__instance.GameWorld.CreateObjectData(objectName), worldPlayer.PreWorld2DPosition + new Vector2((float)(-(float)worldPlayer.LastDirectionX) * 2f, 8f), 0f, 1, Vector2.Zero, 0f);
                            ObjectData objectData = ObjectData.Read(__instance.GameWorld.CreateTile(spawnObject));

                            // Make the streetsweeper an ally, if spawned without a crate
                            if (objectData is ObjectStreetsweeper objStreetsweeper)
                            {
                                objStreetsweeper.SetOwnerPlayer(worldPlayer);
                                objStreetsweeper.SetOwnerTeam(worldPlayer.CurrentTeam, false);
                            }
                            continue;
                        }

                        // Get a weapon with it's name
                        WeaponItem wpn = WeaponDatabase.GetWeapon(parameter);
                        short weaponID = 0;
                        if (wpn == null)
                        {
                            if (!short.TryParse(parameter, out weaponID))
                            {
                                args.Feedback.Add(new(args.SenderGameUser, $"Could not find weapon with name \"{parameter}\"", Color.Red, args.SenderGameUser));
                                continue;
                            }
                            else
                            {
                                // Try using the parsed ID instead
                                wpn = WeaponDatabase.GetWeapon(weaponID);
                        
                                if (wpn == null)
                                {
                                    args.Feedback.Add(new(args.SenderGameUser, $"Could not find weapon with ID \"{parameter}\"", Color.Red, args.SenderGameUser));
                                    continue;
                                }
                            }
                        }

                        if (!wpn.BaseProperties.WeaponCanBeEquipped)
                        {
                            continue;
                        }

                        worldPlayer.GrabWeaponItem(wpn);
                        args.Feedback.Add(new(args.SenderGameUser, $"{worldPlayer.Name} received \"{wpn.BaseProperties.WeaponNameID}\" ({wpn.BaseProperties.WeaponID})", fbColor));
                    }

                    return true;

                }
                
                // Create a slowmotion with an owner
                // (In slowmotions, the owner and his projectiles are slightly faster)
                if (args.IsCommand("SLOWMOTION", "SM") && args.CanUseModeratorCommand("SLOWMOTION", "SM") && args.Parameters.Count > 0)
                {
                    // /SLOWMOTION [INTENSITY] NONE "INFINITE" 100 100
                    // /SLOWMOTION [INTENSITY] [OWNER] [DURATION] [FADEIN] [FADEOUT]
                    if (float.TryParse(args.Parameters[0], out float smIntensity))
                    {
                        int smOwnerPlayerID = -1;
                        float smDuration = 86400f * 1000f;
                        float smFadeIn = 100f;
                        float smFadeOut = 100f;

                        if (args.Parameters.Count > 1)
                        {
                            GameUser selectedGameUser = __instance.GetGameUserByStringInput(args.Parameters[1], args.SenderGameUser);
                            if (selectedGameUser != null)
                            {
                                Player selectedPlayer = __instance.GameWorld.GetPlayerByUserIdentifier(selectedGameUser.UserIdentifier);
                                if (selectedPlayer != null)
                                {
                                    smOwnerPlayerID = selectedPlayer.ObjectID;
                                }
                            }
                        }
                        float.TryParse(args.Parameters.ElementAtOrDefault(2), out smDuration);
                        float.TryParse(args.Parameters.ElementAtOrDefault(4), out smFadeOut);
                        float.TryParse(args.Parameters.ElementAtOrDefault(3), out smFadeIn);

                        __instance.GameWorld.SlowmotionHandler.AddSlowmotion(new(
                            smFadeIn, smDuration, smFadeOut, smIntensity, smOwnerPlayerID    
                        ));

                        string mess = "Slowmotion added";
                        args.Feedback.Add(new(args.SenderGameUser, mess, Color.ForestGreen));
                        return true;
                    }
                }
            }
        }

        if (CConst.SlotCount != 8)
        {
            // Players will only see "?0" when listing players,
            // so we send them a server-sided list instead.

            // (This is taken from ClientCommands!)
            if (args.IsCommand(
                "SCOREBOARD"
            ))
            {
                string header = "- SCOREBOARD -";
                args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, header, Color.ForestGreen, args.SenderGameUser, null));

                int num = 0;
                foreach (GameUser gameUser in __instance.GetGameUsers().OrderBy(gu => gu.GameSlotIndex))
                {
                    if (gameUser.IsDedicatedPreview) { continue; }

                    string slotIndex = gameUser.GameSlotIndex.ToString();
                    string profName = gameUser.GetProfileName();
                    string accName = gameUser.AccountName;
                    string ping = gameUser.Ping.ToString();
                    string tWins = gameUser.Score.TotalWins.ToString();
                    string tLoss = gameUser.Score.TotalLosses.ToString();
                    string tGames = gameUser.Score.TotalGames.ToString();

                    string team = "Team?";
                    if (gameUser.GameSlot != null)
                    {
                        team = gameUser.GameSlot.CurrentTeam.ToString();  
                    }

                    Color messCol = Color.LightBlue * (num % 2 == 0 ? 1f : 0.625f);
                    messCol.A = 255;
                    num++;
                    
                    string mess1;
                    
                    if (gameUser.IsBot)
                    {
                        mess1 = $"{slotIndex}: \"{profName}\" (BOT) - {team}";
                    }
                    else
                    {
                        mess1 = $"{slotIndex}: {accName} - {team} | {tWins}/{tLoss} ({tGames})";
                    }

                    args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, mess1, messCol, args.SenderGameUser, null));
                }
                return true;
            }
        }
        return false;
    }
}