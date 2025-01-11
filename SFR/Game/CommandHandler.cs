using System;
using System.Collections.Generic;
using System.Linq;
using SFD;
using SFD.Core;
using SFD.States;
using SFD.Voting;
using SFD.Objects;
using SFD.Weapons;
using Microsoft.Xna.Framework;
using Lidgren.Network;
using SFDCT.Game.Voting;
using CGlobals = SFDCT.Misc.Globals;
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
        if (args.IsCommand("PLAYERS","LISTPLAYERS","SHOWPLAYERS","USERS","LISTUSERS","SHOWUSERS"))
        {
            int players = 0;
            int bots = 0;

            string header = "- Listing all users in the lobby...";
            args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, header, Color.ForestGreen, args.SenderGameUser));

            foreach (GameUser gameUser in __instance.GetGameUsers().OrderBy(gu => gu.GameSlotIndex))
            {
                if (gameUser.IsDedicatedPreview && !args.SenderGameUser.IsHost) { continue; } // Vanilla hides the dedicated preview from the list

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

        // Host commands
        if (args.HostPrivileges)
        {
            // Makes the host able to use the mouse to drag around dynamic
            // objects and players. Looks somewhat stuttery outside the editor
            // because in online mode, clients use physics predictions
            // /MOUSE
            // /MOUSE [1/0]
            if (args.IsCommand("MOUSE", "M"))
            {
                bool enabled = !Commands.DebugMouse.IsEnabled;
                if (args.Parameters.Count >= 1)
                {
                    if (args.Parameters[0] == "1" || args.Parameters[0].Equals("TRUE", StringComparison.OrdinalIgnoreCase))
                    {
                        enabled = true;
                    }
                    if (args.Parameters[0] == "0" || args.Parameters[0].Equals("FALSE", StringComparison.OrdinalIgnoreCase))
                    {
                        enabled = false;
                    }
                }

                Commands.DebugMouse.IsEnabled = enabled;
                args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, $"Mouse dragging is set to {enabled}", args.SenderGameUser));
                return true;
            }
        }
        
        // Host/Moderator commands
        if (args.HostPrivileges || args.ModeratorPrivileges)
        {
            // Commands that interact with the gameworld
            if (__instance.GameWorld != null)
            {
                // Manually create a vote like a map-vote
                // Syntax is: /DOVOTE "The vote's description" "First option" "Second option" "Third option" "Fourth option"
                // /DOVOTE [TEXT] [A] [B] [C?] [D?]
                // /DOVOTEHIDDEN [TEXT] [A] [B] [C?] [D?]
                if (args.IsCommand("DOVOTE", "DOVOTEHIDDEN") && args.CanUseModeratorCommand("DOVOTE", "DOVOTEHIDDEN") && !__instance.GameWorld.GameOverData.IsOver && !__instance.GameWorld.m_restartInstant)
                {
                    if (__instance.VoteInfo.ActiveVotes.Count > 0)
                    {
                        args.Feedback.Add(new(args.SenderGameUser, "There is already a vote in progress", Color.Red, args.SenderGameUser));
                        return false;
                    }

                    // Will cause an out-of-index crash on vanilla clients.
                    // SFD only assigns 4 keys for voting in an array, F1-4.
                    string description = null;
                    List<string> alternatives = [];
                    bool isPublic = !args.IsCommand("DOVOTEHIDDEN");

                    string[] parsedParams = args.SourceParameters.Split('\"');
                    if (parsedParams.Length >= 4)
                    {
                        description = parsedParams[1];
                        alternatives.Add(parsedParams[3]);

                        if (parsedParams.Length >= 6)
                        {
                            alternatives.Add(parsedParams[5]);
                        }
                        if (parsedParams.Length >= 8)
                        {
                            alternatives.Add(parsedParams[7]);
                        }
                        if (parsedParams.Length >= 10)
                        {
                            alternatives.Add(parsedParams[9]);
                        }
                    }

                    if (string.IsNullOrEmpty(description) || alternatives.Count <= 0)
                    {
                        args.Feedback.Add(new(args.SenderGameUser, "Error parsing vote-syntax...", Color.Red, args.SenderGameUser));
                        return true;
                    }
                    else
                    {
                        string mess = $"Creating vote: '{description}' :";
                        foreach(string alt in alternatives)
                        {
                            mess += $" '{alt}' ";
                        }

                        args.Feedback.Add(new(args.SenderGameUser, mess, isPublic ? Color.LightBlue : Color.LightBlue * 0.7f, args.SenderGameUser));

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
                        return true;
                    }
                }

                // Better /GIVE command, allows for multiple weapons in a single command
                // and use ALL as user (uses all users in the lobby)
                // /GIVE [PLAYER/ALL/ANY] [ITEMS...]
                if (args.IsCommand("GIVE") && args.CanUseModeratorCommand("GIVE") && args.Parameters.Count >= 2)
                {
                    List<Player> targetPlayers = [];
                    bool isGiveAll = args.Parameters[0].Equals("All", StringComparison.OrdinalIgnoreCase);
                    bool isGiveRandom = args.Parameters[0].Equals("Any", StringComparison.OrdinalIgnoreCase) || args.Parameters[0].Equals("Random", StringComparison.OrdinalIgnoreCase);

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
                    else if (isGiveRandom)
                    {
                        IEnumerable<Player> validPlayers = __instance.GetGameUsers().Select((GameUser user) =>
                        {
                            if (user != null && !user.IsDisposed)
                            {
                                Player userPlayer = __instance.GameWorld.GetPlayerByUserIdentifier(user.UserIdentifier);
                                if (userPlayer != null && !userPlayer.IsDisposed && !userPlayer.IsRemoved && !userPlayer.IsDead)
                                {
                                    return userPlayer;
                                }
                            }

                            return null;
                        });

                        if (validPlayers.Count() > 0)
                        {
                            Random RND = new Random();
                            targetPlayers.Add(validPlayers.ElementAt(RND.Next(0, validPlayers.Count())));
                        }
                    }
                    else
                    {
                        // Try to get user by string input
                        GameUser targetUser = __instance.GetGameUserByStringInput(args.Parameters[0], args.SenderGameUser); ;

                        if (targetUser == null || targetUser.IsDisposed)
                        {    
                            args.Feedback.Add(new(args.SenderGameUser, $"User '{args.Parameters[0]}' not found", Color.Red, args.SenderGameUser));
                            return true;
                        }

                        Player targetPlayer = __instance.GameWorld.GetPlayerByUserIdentifier(targetUser.UserIdentifier);
                        if (targetPlayer == null || targetPlayer.IsDisposed || targetPlayer.IsRemoved)
                        {
                            args.Feedback.Add(new(args.SenderGameUser, $"User '{args.Parameters[0]}' has no player", Color.Red, args.SenderGameUser));
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
                                continue;
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
                                continue;
                            }

                            if (param.Equals("LONG_ASS_WHIP", StringComparison.OrdinalIgnoreCase))
                            {
                                WeaponItem wpn = WeaponDatabase.GetWeapon("Whip");
                                wpn.MWeaponData.Properties.Range = 100.0f;
                                wpn.MWeaponData.Properties.DamageObjects = 50f;
                                wpn.MWeaponData.Properties.DeflectionDuringBlock.DeflectType = DeflectBulletType.Deflect;
                                wpn.MWeaponData.Properties.DeflectionDuringBlock.DurabilityLoss = 1.0f;
                                wpn.MWeaponData.Properties.DeflectionDuringBlock.DeflectCone = SFDMath.DegToRad(3f);
                                wpn.MWeaponData.Properties.DeflectionOnAttack.DeflectType = DeflectBulletType.Deflect;
                                wpn.MWeaponData.Properties.DeflectionOnAttack.DurabilityLoss = 0.0f;
                                wpn.MWeaponData.Properties.DeflectionOnAttack.DeflectCone = SFDMath.DegToRad(0f);
                                wpn.BaseProperties.VisualText = "Long Ass Whip";
                                player.GrabWeaponItem(wpn);

                                receivedTexts.Add($"{wpn.BaseProperties.VisualText} ({wpn.BaseProperties.WeaponID})");
                                continue;
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
                                        args.Feedback.Add(new(args.SenderGameUser, $"Weapon '{param}' not found.", Color.Red, args.SenderGameUser));
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
                    else if (isGiveRandom)
                    {
                        mess = $"{targetPlayers[0].Name} randomly received ";
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

                    args.Feedback.Add(new(args.SenderGameUser, mess));
                    return true;
                }
            }

            if (__instance.GameOwner == GameOwnerEnum.Server)
            {
                // Forcefully enables/disables server-movement on a user, regardless of the
                // automatic server-movement setting.
                // /FORCESERVERMOVEMENT [USER] [1/0/NULL]
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
                // Host/Mod commands, only available when using extended slots
                if (CGlobals.HOST_GAME_EXTENDED_SLOTS)
                {
                    // Provides a readable list of the extended slots states
                    // /SLOTS
                    if (args.IsCommand("SLOTS") && args.CanUseModeratorCommand("SLOTS"))
                    {
                        args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "- Listing all slots...", Color.Yellow, args.SenderGameUser));
                        for (int i = 0; i < CGlobals.SLOTCOUNT; i++)
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
                    if (args.IsCommand("SETSLOT", "SSLOT") && args.CanUseModeratorCommand("SETSLOT", "SSLOT") && args.Parameters.Count >= 2)
                    {
                        if (int.TryParse(args.Parameters[0], out int slotIndex))
                        {
                            slotIndex = Math.Min(Math.Max(slotIndex, 0), CGlobals.SLOTCOUNT - 1);
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
        }

        // Public commands
        if (args.IsCommand("HELP"))
        {
            Color yellow = Color.Yellow;
            Color c = new(255, 181, 26);
            Color c2 = new(159, 255, 64);
            Color c3 = new(255, 91, 51);
            Color c4 = new(63, 255, 155);

            if (__instance.GameOwner != GameOwnerEnum.Server)
            {
                // Client/Local
                args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "'/PLAYERS' to list all players.", yellow, args.SenderGameUser, null));
                args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "'/MUTE [PLAYER]' to mute a player's chat messages.", yellow, args.SenderGameUser, null));
                args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "'/UNMUTE [PLAYER]' to unmute a muted player's chat messages.", yellow, args.SenderGameUser, null));
                args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "'/SCRIPTS' to list all current scripts.", yellow, args.SenderGameUser, null));
                args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "'/SHOWDIFFICULTY' to show current difficulty for campaign maps.", yellow, args.SenderGameUser, null));
                
                if (__instance.GameOwner == GameOwnerEnum.Client)
                {
                    // Client
                    args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "'/PING' to show your ping to the server.", yellow, args.SenderGameUser, null));
                    args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "'/W [PLAYER] [TEXT]' to whisper a player.", yellow, args.SenderGameUser, null));
                    args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "'/T [TEXT]' to send a team message.", yellow, args.SenderGameUser, null));
                }
            }

            if (args.SenderGameUser.IsModerator)
            {
                if (__instance.GameOwner == GameOwnerEnum.Client) { return false; }

                if (args.CanUseModeratorCommand("SLOMO","SLOWMOTION")) { args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "'/SLOMO [1/0]'", c2, args.SenderGameUser, null)); }
                if (args.CanUseModeratorCommand("SETTIME")) { args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "'/SETTIME [0.1-2.0]'", c2, args.SenderGameUser, null)); }
                if (args.CanUseModeratorCommand("INFINITE_AMMO","IA")) { args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "'/INFINITE_AMMO [1/0]'", c2, args.SenderGameUser, null)); }
                if (args.CanUseModeratorCommand("INFINITE_LIFE","INFINITE_HEALTH","IL","IH")) { args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "'/INFINITE_LIFE [1/0]'", c2, args.SenderGameUser, null)); }
                if (args.CanUseModeratorCommand("INFINITE_ENERGY","IE")) { args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "'/INFINITE_ENERGY [1/0]'", c2, args.SenderGameUser, null)); }
                if (args.CanUseModeratorCommand("GIVE")) { args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "'/GIVE [PLAYER/ANY/ALL] [ITEM] [ITEM] [...]'", c2, args.SenderGameUser, null)); }
                if (args.CanUseModeratorCommand("REMOVE")) { args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "'/REMOVE [PLAYER] [ITEM/SLOT]'", c2, args.SenderGameUser, null)); }
                
                args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "'/ITEMS' to list all available items in the game.", c2, args.SenderGameUser, null));
                
                if (args.CanUseModeratorCommand("SETSTARTHEALTH","SETSTARTLIFE","STARTHEALTH","STARTLIFE")) { args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "'/STARTLIFE [1-100]'", c2, args.SenderGameUser, null)); }
                if (args.CanUseModeratorCommand("STARTITEMS","STARTITEM","SETSTARTITEMS","SETSTARTITEM","SETSTARTUPITEMS","SETSTARTUPITEM")) { args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "'/SETSTARTITEMS ID ID ID ...' to set start items.", c2, args.SenderGameUser, null)); }
                if (args.CanUseModeratorCommand("CLEAR","RESET")) { args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "'/CLEAR' to reset cheats.", c2, args.SenderGameUser, null)); }
                
                if (args.SenderGameUser.IsHost && __instance.GameOwner != GameOwnerEnum.Local)
                {
                    args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "'/CHAT [1/0]' to enable/disable global chat.", c3, args.SenderGameUser, null));
                    args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "'/MODERATORS' to list all moderators with index.", c3, args.SenderGameUser, null));
                    args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "'/ADDMODERATOR [PLAYER]' to add someone to the moderator list.", c3, args.SenderGameUser, null));
                    args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "'/REMOVEMODERATOR [INDEX|PLAYER]' to remove from the moderator list.", c3, args.SenderGameUser, null));
                    args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "'/SETMODPASS [INDEX|PLAYER] [PASS]' to set mod password.", c3, args.SenderGameUser, null));
                }

                if (args.CanUseModeratorCommand("MSG","MESSAGE")) { args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "'/MSG [TEXT]' to show a message to everyone.", c, args.SenderGameUser, null)); }
                if (args.CanUseModeratorCommand("SERVERDESCRIPTION")) { args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "'/SERVERDESCRIPTION' to show the server description as a reminder.", c, args.SenderGameUser, null)); }
                if (args.CanUseModeratorCommand("GAMEOVER")) { args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "'/GAMEOVER' to restart the game.", c, args.SenderGameUser, null)); }
                
                if (GameSFD.Handle.CurrentState == State.EditorTestRun)
                {
                    args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "'/R' to restart instant.", c, args.SenderGameUser, null));
                }

                if (args.CanUseModeratorCommand("SCRIPTS")) { args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "'/SCRIPTS' to list all available scripts.", c, args.SenderGameUser, null)); }
                if (args.CanUseModeratorCommand("LOADSCRIPT","STARTSCRIPT")) { args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "'/STARTSCRIPT X' to start script X.", c, args.SenderGameUser, null)); }
                if (args.CanUseModeratorCommand("UNLOADSCRIPT","STOPSCRIPT")) { args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "'/STOPSCRIPT X' to stop script X.", c, args.SenderGameUser, null)); }
                if (args.SenderGameUser != null && args.SenderGameUser.IsHost)
                {
                    args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "'/RELOADSCRIPTS' to reload scripts from disk.", c, args.SenderGameUser, null));
                }

                if (args.CanUseModeratorCommand("MAPS","LISTMAPS","SHOWMAPS")) { args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "'/MAPS' to list all maps.", c, args.SenderGameUser, null)); }
                if (args.CanUseModeratorCommand("MAPS","LISTMAPS","SHOWMAPS")) { args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "'/MAPS [CATEGORY]' to list all maps in category X.", c, args.SenderGameUser, null)); }
                if (args.CanUseModeratorCommand("LISTMAPCATEGORIES","LISTMAPCAT","SHOWMAPCATEGORIES","SHOWMAPCAT","LISTMC","SHOWMC","MAPCATEGORIES")) { args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "'/MAPCATEGORIES' to list all map categories.", c, args.SenderGameUser, null)); }
                if (args.CanUseModeratorCommand("CHANGEMAPCATEGORY","CHANGEMAPCAT","CHANGEMC")) { args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "'/CHANGEMAPCATEGORY [CATEGORY]' to change the map category.", c, args.SenderGameUser, null)); }
                if (args.CanUseModeratorCommand("MAP","CHANGEMAP")) { args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "'/CHANGEMAP [MAP]' to change the map next fight.", c, args.SenderGameUser, null)); }
                if (args.CanUseModeratorCommand("NEXTMAP")) { args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "'/NEXTMAP' to change map in the current map rotation to the next map.", c, args.SenderGameUser, null)); }
                if (args.CanUseModeratorCommand("MAPPARTS","SHOWMAPPARTS","LISTMAPPARTS","CHAPTERS","LISTCHAPTERS")) { args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "'/CHAPTERS' to list available chapters for the current map.", c, args.SenderGameUser, null)); }
                if (args.CanUseModeratorCommand("SETMAPPART","CHANGEMAPPART","SMP","CMP","SETCHAPTER")) { args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "'/SETCHAPTER [X]' to change to chapter X.", c, args.SenderGameUser, null)); }
                if (args.CanUseModeratorCommand("NEXTMAPPART","NEXTCHAPTER")) { args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "'/NEXTCHAPTER' to change to the next chapter.", c, args.SenderGameUser, null)); }
                if (args.CanUseModeratorCommand("MAPROTATION","MR"))
                {
                    args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "'/MAPROTATION [X]' to enable map rotation every X fights.", c, args.SenderGameUser, null));
                    args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "'/MAPROTATION [M]' to change map rotation mode where M is A, B, C or D.", c, args.SenderGameUser, null));
                    args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "'/MAPROTATION [M] [X]' to change map rotation mode and interval.", c, args.SenderGameUser, null));
                }
                if (args.CanUseModeratorCommand("SETDIFFICULTY")) { args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "'/SETDIFFICULTY [1/2/3/4/EASY/NORMAL/HARD/EXPERT]' to change the difficulty for campaign maps.", c, args.SenderGameUser, null)); }

                if (__instance.GameOwner != GameOwnerEnum.Local)
                {
                    if (args.CanUseModeratorCommand("BAN","BAN_USER","BAN_USER_BY_IP"))
                    {
                        args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "'/BAN [PLAYER]' to ban a player by name or index.", c, args.SenderGameUser, null));
                    }
                    if (args.CanUseModeratorCommand("KICK","KICK_USER"))
                    {
                        args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "'/KICK [PLAYER]' to kick player by name or index.", c, args.SenderGameUser, null));
                        args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "'/KICK [X] [PLAYER]' to kick a player by name or index for X minutes (max 60 minutes).", c, args.SenderGameUser, null));
                    }
                    if (args.CanUseModeratorCommand("MAXPING","MAX_PING"))
                    {
                        args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "'/MAXPING [X]' to set a maximum ping to X (range 50-500). 0 to disable.", c, args.SenderGameUser, null));
                    }
                    if (args.CanUseModeratorCommand("AUTO_KICK_AFK","AUTOKICKAFK","KICK_AFK","KICKAFK","AUTO_KICK_IDLE","AUTOKICKIDLE","KICK_IDLE","KICKIDLE"))
                    {
                        args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "'/KICKIDLE [X]' to set a maximum idle time to X seconds (range 30-600). 0 to disable.", c, args.SenderGameUser, null));
                    }
                }

                if (args.CanUseModeratorCommand("TIMELIMIT","TL")) { args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "'/TIMELIMIT [X]' to set time limit to X seconds (range 30-600). 0=disable.", c, args.SenderGameUser, null)); }
                if (args.CanUseModeratorCommand("SUDDENDEATH","SD")) { args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "'/SUDDENDEATH [1/0]' to set sudden death on/off.", c, args.SenderGameUser, null)); }
                if (args.CanUseModeratorCommand("SHUFFLETEAMS", "ST")) { args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "'/SHUFFLETEAMS' to shuffle the teams next fight.", c, args.SenderGameUser, null));}
                if (args.CanUseModeratorCommand("SHUFFLETEAMS", "ST")) { args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "'/SHUFFLETEAMS [X]' to shuffle the teams each X fights.", c, args.SenderGameUser, null)); }
                if (args.CanUseModeratorCommand("SETTEAMS")) { args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "'/SETTEAMS 00000000' to set new teams next fight. 0=independent, 1=team1...", c, args.SenderGameUser, null)); }

                // SFDCT
                if (args.CanUseModeratorCommand("FORCESERVERMOVEMENT", "FORCESVMOV")) { args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "'/FORCESERVERMOVEMENT [PLAYER] [1/0/DEFAULT]' to manually set the server-movement state of a player.", c4, args.SenderGameUser, null)); }
                if (args.CanUseModeratorCommand("DOVOTE", "DOVOTEHIDDEN")) { args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "'/DOVOTE [DESCRIPTION] \"[OPTION F1]\" \"[OPTION F2]\" \"[OPTION F3]?\" \"[OPTION F4]?\"' to create a public vote.", c4, args.SenderGameUser, null)); }
                
                if (args.SenderGameUser.IsHost)
                {
                    args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "'/MOUSE [1/0]' to enable or disable debug mouse dragging.", c4, args.SenderGameUser, null));
                }

                if (CGlobals.HOST_GAME_EXTENDED_SLOTS)
                {
                    if (args.CanUseModeratorCommand("SETSLOT", "SSLOT")) { args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "'/SSLOT [INDEX] [STATE] [TEAM]?' to set a slot status.", c4, args.SenderGameUser, null)); }
                    if (args.CanUseModeratorCommand("SLOTS")) { args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "'/SLOTS' to see all slots status.", c4, args.SenderGameUser, null)); }
                }

                args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "----", Microsoft.Xna.Framework.Color.LightBlue, args.SenderGameUser, null));
                args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "Scroll the chat using the scroll-wheel to see all commands.", Microsoft.Xna.Framework.Color.LightBlue, args.SenderGameUser, null));
            }
            return true;
        }
        return false;
    }
}