using System;
using System.Collections.Generic;
using System.Linq;
using SFD;
using SFD.Core;
using SFD.States;
using SFD.Voting;
using SFD.ManageLists;
using SFD.Objects;
using SFD.Weapons;
using SFD.Projectiles;
using SFD.GUI.Text;
using Microsoft.Xna.Framework;
using Lidgren.Network;
using SFDCT.Game.Voting;
using SFDCT.Helper;
using CConst = SFDCT.Misc.Constants;
using HarmonyLib;

namespace SFDCT.Game;

[HarmonyPatch]
internal static class CommandHandler
{   
    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameInfo), nameof(GameInfo.HandleCommand), typeof(ProcessCommandArgs))]
    private static bool GameInfo_HandleCommands(ref bool __result, ProcessCommandArgs args, GameInfo __instance)
    {
        bool ranCustomCommand = false;

        // We check to see if we handle custom commands before vanilla ones,
        // this also allows us to replace them.
        if (__instance.GameOwner == GameOwnerEnum.Client || __instance.GameOwner == GameOwnerEnum.Local)
        {
            // Client
            ranCustomCommand = ClientCommands(args, __instance);
        }
        if (!ranCustomCommand && (__instance.GameOwner == GameOwnerEnum.Server || __instance.GameOwner == GameOwnerEnum.Local))
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

                string slotIndex, profileName, accountName, account, powerStatus;
                slotIndex = gameUser.GameSlotIndex.ToString();

                // Modified clients can illegaly modify these, bypassing their maximum lengths.
                profileName = gameUser.GetProfileName().Substring(0, Math.Min(gameUser.GetProfileName().Length, 32));
                accountName = gameUser.AccountName.Substring(0, Math.Min(gameUser.AccountName.Length, 32));
                account = gameUser.Account.Substring(0, Math.Min(gameUser.Account.Length, 32));

                // Check IsHost first, hosts are counted as moderators
                powerStatus = gameUser.IsHost ? "- HOST" : (gameUser.IsModerator ? "- MOD" : "");

                string mess = "?";
                if (gameUser.IsBot)
                {
                    // Bots don't have ping, or an account. Less clutter
                    mess = $"{slotIndex}: {profileName} - BOT";
                    bots++;
                }
                else
                {
                    mess = $"{slotIndex}: {profileName} ({accountName}:{account}) {powerStatus}";
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
        Server server = GameSFD.Handle.Server;
        if (server == null && __instance.GameOwner == GameOwnerEnum.Server)
        {
            return false;
        }

        // Host-only commands
        if (args.HostPrivileges)
        {
            // Makes the host able to use the mouse to drag around dynamic
            // objects and players. Looks somewhat stuttery outside the editor.
            // /MOUSE [1/0]
            if (args.IsCommand("MOUSE", "M") && args.Parameters.Count > 0)
            {
                if (args.Parameters[0] == "1" || args.Parameters[0].ToUpper() == "TRUE")
                {
                    Commands.DebugMouse.IsEnabled = true;
                }
                if (args.Parameters[0] == "0" || args.Parameters[0].ToUpper() == "FALSE")
                {
                    Commands.DebugMouse.IsEnabled = false;
                }

                string mess = "Mouse dragging is now " + (Commands.DebugMouse.IsEnabled ? "enabled" : "disabled");
                args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, mess, args.SenderGameUser));
                return true;
            }

            // Manually create a game-vote, like a map-vote
            // /DOVOTE [TEXT] [A] [B] [C?] [D?]
            // /DOVOTEHIDDEN [TEXT] [A] [B] [C?] [D?]
            if (!__instance.GameWorld.GameOverData.IsOver && !__instance.GameWorld.m_restartInstant &&
                args.Parameters.Count >= 3 && args.IsCommand("DOVOTE", "DOVOTEHIDDEN")
            )
            {
                if (__instance.VoteInfo.ActiveVotes.Count > 0)
                {
                    args.Feedback.Add(new(args.SenderGameUser, "There is already a vote in progress.", Color.Red, args.SenderGameUser));
                    return false;
                }

                string description = "";
                List<string> alternatives = [];

                bool isPublic = !args.IsCommand("DOVOTEHIDDEN");

                bool isDescription = true;
                string temp = "";
                for (int i = 0; i < args.Parameters.Count; i++)
                {
                    string parameter = args.Parameters[i];
                    temp += parameter.Replace("\"", string.Empty) + " ";

                    if (parameter.EndsWith("\""))
                    {
                        temp = temp.Substring(0, temp.Length - 1);
                        if (isDescription)
                        {
                            description = temp;
                            isDescription = false;
                            temp = "";
                            continue;
                        }

                        alternatives.Add(temp);
                        temp = "";

                        // Will cause an out-of-index crash on vanilla clients.
                        // SFD only assigns 4 keys for voting in an array, F1-4.
                        if (alternatives.Count >= 4)
                        {
                            break;
                        }
                    }
                }

                if (isDescription || string.IsNullOrEmpty(description) || alternatives.Count <= 1)
                {
                    args.Feedback.Add(new(args.SenderGameUser, "Error parsing Vote-Syntax.", Color.Red, args.SenderGameUser));
                }
                else
                {
                    string mess = isPublic ? "Creating public-vote..." : "Creating private-vote...";
                    args.Feedback.Add(new(args.SenderGameUser, mess, Color.ForestGreen, args.SenderGameUser));

                    GameVote vote = new GameVoteManual(GameVote.GetNextVoteID(), args.SenderGameUser, description, alternatives.ToArray(), isPublic, false);

                    if (__instance.GameOwner == GameOwnerEnum.Server)
                    {
                        vote.ValidRemoteUniqueIdentifiers.AddRange(server.GetConnectedUniqueIdentifiers((NetConnection x) => x.GameConnectionTag() != null && x.GameConnectionTag().FirstGameUser != null && x.GameConnectionTag().FirstGameUser.CanVote));
                        __instance.VoteInfo.AddVote(vote);
                        server.SendMessage(MessageType.GameVote, new Pair<GameVote, bool>(vote, false));
                    }
                    else
                    {
                        vote.ValidRemoteUniqueIdentifiers.Add(1L);
                        __instance.VoteInfo.AddVote(vote);
                    }
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
                // Better /GIVE command, allows for multiple weapons in a single command
                // and use ALL as user (uses all users in the lobby)
                // /GIVE [USER/PLAYER/ALL] [ITEMS...]
                if (args.IsCommand("GIVE") && args.CanUseModeratorCommand("GIVE") && args.Parameters.Count >= 2)
                {
                    Color feedbackColor = Color.ForestGreen;
                    Color errorColor = Color.Red;
                    List<Player> targetPlayers = [];
                    bool isGiveAll = args.Parameters[0].Equals("All", StringComparison.OrdinalIgnoreCase);

                    // "All" targets all players in the lobby
                    if (isGiveAll)
                    {
                        targetPlayers = __instance.GetGameUsers().Select((GameUser user) =>
                        {
                            if (user != null && !user.IsDisposed)
                            {
                                Player userPlayer = __instance.GameWorld.GetPlayerByUserIdentifier(user.UserIdentifier);
                                if (userPlayer != null && !userPlayer.IsDisposed && !userPlayer.IsRemoved)
                                {
                                    return userPlayer;
                                }
                            }

                            return null;
                        }).ToList();
                    }
                    else
                    {
                        // Try to get user by string input
                        GameUser targetUser = __instance.GetGameUserByStringInput(args.Parameters[0], args.SenderGameUser);
                        if (targetUser == null || targetUser.IsDisposed)
                        {
                            args.Feedback.Add(new(args.SenderGameUser, $"User '{args.Parameters[0]}' not found.", errorColor, args.SenderGameUser));
                            return true;
                        }
                        Player targetPlayer = __instance.GameWorld.GetPlayerByUserIdentifier(targetUser.UserIdentifier);
                        if (targetPlayer == null || targetPlayer.IsDisposed || targetPlayer.IsRemoved)
                        {
                            return true;
                        }

                        targetPlayers = [targetPlayer];
                    }


                    List<string> receivedTexts = [];

                    // Loop through players
                    foreach(Player player in targetPlayers)
                    {
                        if (player == null) { continue; }

                        // Loop through parameters
                        for(int i = 1; i < args.Parameters.Count; i++)
                        {
                            string param = args.Parameters[i];
                            if (string.IsNullOrEmpty(param)) { continue; }

                            // Ammo for Rifle and Handgun
                            if (param.Equals("Ammo", StringComparison.OrdinalIgnoreCase))
                            {
                                bool filledAmmo = false;
                                if (player.CurrentHandgunWeapon != null)
                                {
                                    player.CurrentHandgunWeapon.CurrentSpareMags = player.CurrentHandgunWeapon.Properties.MaxCarriedSpareMags;
                                    filledAmmo = true;
                                    if (__instance.GameOwner == GameOwnerEnum.Server)
                                    {
                                        NetMessage.PlayerReceiveItem.Data data = new(player.ObjectID, player.CurrentHandgunWeapon, NetMessage.PlayerReceiveItem.ReceiveSourceType.GrabWeaponAmmo);
                                        server.SendMessage(MessageType.PlayerReceiveItem, data);
                                    }
                                }
                                if (player.CurrentRifleWeapon != null)
                                {
                                    player.CurrentRifleWeapon.CurrentSpareMags = player.CurrentRifleWeapon.Properties.MaxCarriedSpareMags;
                                    filledAmmo = true;
                                    if (__instance.GameOwner == GameOwnerEnum.Server)
                                    {
                                        NetMessage.PlayerReceiveItem.Data data2 = new(player.ObjectID, player.CurrentRifleWeapon, NetMessage.PlayerReceiveItem.ReceiveSourceType.GrabWeaponAmmo);
                                        server.SendMessage(MessageType.PlayerReceiveItem, data2);
                                    }
                                }

                                if (filledAmmo)
                                {
                                    receivedTexts.Add("Ammo");
                                }
                            }

                            // Streetsweeper and StreetsweeperCrate
                            if (param.Equals("SW", StringComparison.OrdinalIgnoreCase) ||
                                param.Equals("STREETSWEEPER", StringComparison.OrdinalIgnoreCase) ||
                                param.Equals("SWC", StringComparison.OrdinalIgnoreCase) ||
                                param.Equals("STREETSWEEPERCRATE", StringComparison.OrdinalIgnoreCase))
                            {
                                string objectName = "STREETSWEEPERCRATE";
                                if (param.Equals("SW", StringComparison.OrdinalIgnoreCase) || param.Equals("STREETSWEEPER", StringComparison.OrdinalIgnoreCase))
                                {
                                    objectName = "STREETSWEEPER";
                                }

                                Vector2 swSpawnOffset = new(-player.LastDirectionX * 2f, 8f);

                                SpawnObjectInformation spawnObject = new SpawnObjectInformation(__instance.GameWorld.CreateObjectData(objectName), player.PreWorld2DPosition + swSpawnOffset, 0f, 1, Vector2.Zero, 0f);
                                ObjectData objectData = ObjectData.Read(__instance.GameWorld.CreateTile(spawnObject));
                                if (objectData is ObjectStreetsweeper)
                                {
                                    ((ObjectStreetsweeper)objectData).SetOwnerPlayer(player);
                                    ((ObjectStreetsweeper)objectData).SetOwnerTeam(player.CurrentTeam, false);
                                }

                                receivedTexts.Add(objectName.ToLowerInvariant());
                            }

                            // Weapon ID or Name
                            try
                            {
                                WeaponItem wpn = WeaponDatabase.GetWeapon(param);
                                if (wpn == null)
                                {
                                    short weaponID = 0;
                                    bool parsedID = short.TryParse(param, out weaponID);
                                    wpn = parsedID ? WeaponDatabase.GetWeapon(weaponID) : null;
                            
                                    if (!parsedID || wpn == null)
                                    {
                                        args.Feedback.Add(new(args.SenderGameUser, $"Weapon '{param}' not found.", errorColor, args.SenderGameUser));
                                        continue;
                                    }
                                }

                                if (!wpn.BaseProperties.WeaponCanBeEquipped) { continue; }
                                receivedTexts.Add($"{wpn.BaseProperties.VisualText} ({wpn.BaseProperties.WeaponID})");
                                player.GrabWeaponItem(wpn);
                            }
                            catch
                            {
                                continue;
                            }
                        }
                    }

                    if (receivedTexts.Count == 0)
                    {
                        return true;
                    }

                    // Create feedback text
                    string mess = "";
                    if (isGiveAll)
                    {
                        mess = "Everyone received ";
                    }
                    else
                    {
                        mess = $"{targetPlayers[0].Name} received ";
                    }

                    if (receivedTexts.Count == 1)
                    {
                        mess += receivedTexts[0];
                    }
                    else
                    {
                        mess += string.Join(", ", receivedTexts);
                    }

                    args.Feedback.Add(new(args.SenderGameUser, mess, feedbackColor, null));
                    return true;
                }
            }

            if (__instance.GameOwner == GameOwnerEnum.Server)
            {
                // Forcefully enables/disables server-movement on a user, regardless of the
                // automatic server-movement setting.
                // /FORCESVMOV [USER] [1/0/NULL]
                if (args.Parameters.Count > 1 && args.IsCommand("FORCESERVERMOVEMENT", "FORCESVMOV") && args.CanUseModeratorCommand("FORCESERVERMOVEMENT", "FORCESVMOV"))
                {
                    GameUser gameUser = __instance.GetGameUserByStringInput(args.Parameters[0], args.SenderGameUser);
                    GameConnectionTag gameUserTag = gameUser?.GetGameConnectionTag();
                
                    if (gameUser != null && gameUserTag != null)
                    {
                        if (args.Parameters[1].ToUpper() == "DEFAULT" || args.Parameters[1].ToUpper() == "NULL")
                        {
                            args.Feedback.Add(new(args.SenderGameUser, $"\"{gameUser.GetProfileName()}\" ({gameUser.AccountName}) forced server-movement was reset.", Color.LightBlue, args.SenderGameUser));
                            bool oldVal = (gameUserTag.Ping > Constants.HOST_GAME_FORCED_SERVER_MOVEMENT_PING || Constants.HOST_GAME_FORCED_SERVER_MOVEMENT_PING == 0) && Constants.HOST_GAME_FORCED_SERVER_MOVEMENT_CHECK;
                        
                            gameUserTag.ForceServerMovement = oldVal;
                            gameUserTag.ForcedServerMovementToggleTime = Constants.HOST_GAME_FORCED_SERVER_MOVEMENT_TOGGLE_TIME_MS + 100f;
                            if (gameUserTag.GameUsers != null)
                            {
                                for (int j = 0; j < gameUserTag.GameUsers.Length; j++)
                                {
                                    GameUser gu = gameUserTag.GameUsers[j];
                                    if (gu != null)
                                    {
                                        gu.ForceServerMovement = oldVal;
                                        Player playerByUserID = __instance.GameWorld?.GetPlayerByUserIdentifier(gu.UserIdentifier);
                                        playerByUserID?.UpdateCanDoPlayerAction();
                                    }
                                }
                            }
                            return true;
                        }

                        bool val = (args.Parameters[1] == "1" || args.Parameters[1].ToUpper() == "TRUE");
                        args.Feedback.Add(new(args.SenderGameUser, $"\"{gameUser.GetProfileName()}\" ({gameUser.AccountName}) set forced server-movement to {val}.", Color.LightBlue, args.SenderGameUser));
                        gameUserTag.ForceServerMovement = val;
                        gameUserTag.ForcedServerMovementToggleTime = -1f;
                        if (gameUserTag.GameUsers != null)
                        {
                            for (int j = 0; j < gameUserTag.GameUsers.Length; j++)
                            {
                                GameUser gu = gameUserTag.GameUsers[j];
                                if (gu != null)
                                {
                                    gu.ForceServerMovement = val;
                                    Player playerByUserID = __instance.GameWorld?.GetPlayerByUserIdentifier(gu.UserIdentifier);
                                    playerByUserID?.UpdateCanDoPlayerAction();
                                }
                            }
                        }
                        return true;
                    }
                }
            }

            // Host/Mod commands, only available when using extended slots
            if (CConst.HOST_GAME_EXTENDED_SLOTS)
            {
                // Provides a readable list of the extended slots states
                // /SLOTS
                if (args.IsCommand("SLOTS") && args.CanUseModeratorCommand("SLOTS"))
                {
                    args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "- Listing all slots...", Color.Yellow, args.SenderGameUser));
                    for (int i = 0; i < CConst.HOST_GAME_SLOT_COUNT; i++)
                    {
                        GameSlot slot = __instance.GetGameSlotByIndex(i);
                        Color messCol = Color.Orange * (i % 2 == 0 ? 1f : 0.9f);
                        if (!slot.IsOccupied)
                        {
                            messCol *= 0.5f;
                        }

                        string mess = string.Format("{0}: {1}", i, Commands.ExtendedSlots.SlotToString(slot));

                        args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, mess, messCol, args.SenderGameUser));
                    }
                    return true;
                }

                // Manually set a slot state and team
                // /SSLOT [INDEX] [0/1/2/4/5/6] [0/1/2/3/4]
                if (args.IsCommand("SETSLOT", "SSLOT") && args.CanUseModeratorCommand("SLOTS") && args.Parameters.Count >= 2)
                {
                    if (int.TryParse(args.Parameters[0], out int slotIndex))
                    {
                        slotIndex = Math.Min(Math.Max(slotIndex, 0), CConst.HOST_GAME_SLOT_COUNT - 1);
                        int slotState = Commands.ExtendedSlots.GetSlotStateByStringInput(args.Parameters[1]);
                        int slotTeam = (int)(args.Parameters.Count >= 3 ? Commands.ExtendedSlots.GetSlotTeamByStringInput(args.Parameters[2]) : Constants.GET_HOST_GAME_SLOT_TEAM(slotIndex));

                        string messSlotBefore = Commands.ExtendedSlots.SlotToString(__instance.GetGameSlotByIndex(slotIndex));

                        LobbyCommandHandler.LobbyStatus_LobbySlotValueChanged(null, slotIndex, slotState);
                        LobbyCommandHandler.LobbyTeam_LobbySlotValueChanged(null, slotIndex, slotTeam);

                        string messSlotAfter = Commands.ExtendedSlots.SlotToString(__instance.GetGameSlotByIndex(slotIndex));
                        args.Feedback.Add(new(args.SenderGameUser, messSlotBefore + "->" + messSlotAfter, Color.ForestGreen, args.SenderGameUser));

                        GameSFD.Handle.Server.SyncGameSlotsInfo();
                    }
                    return true;
                }
            }
        }
        
        return false;
    }
}