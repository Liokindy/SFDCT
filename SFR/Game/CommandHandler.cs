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

            // Add commands to Moderator Commands
            // /MODERATOR_COMMANDS_ADD [...]
            if (args.IsCommand("MODERATOR_COMMANDS_ADD") && args.Parameters.Count >= 1)
            {
                for(int i = 0; i < args.Parameters.Count; i++)
                {
                    string param = args.Parameters[i].ToUpper();
                    if (string.IsNullOrEmpty(param) || param == "ALL") { continue; }

                    if (!Constants.MODDERATOR_COMMANDS.Contains(param))
                    {
                        Constants.MODDERATOR_COMMANDS.Add(param);

                        string mess = $"Added '/{param}' to moderator commands.";
                        args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, mess, CConst.Colors.Staff_Chat_Message, args.SenderGameUser));
                    }
                }
                if (args.Parameters.Count > 1)
                {
                    args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, $"Added {args.Parameters.Count} moderator command(s)", CConst.Colors.Staff_Chat_Name, args.SenderGameUser));
                }

                SFDConfig.SaveConfig(SFDConfigSaveMode.HostGameOptions);
            }

            // Remove commands from Moderator Commands
            // /MODERATOR_COMMANDS_REMOVE [...]
            if (args.IsCommand("MODERATOR_COMMANDS_REMOVE") && args.Parameters.Count >= 1)
            {
                for (int i = 0; i < args.Parameters.Count; i++)
                {
                    string param = args.Parameters[i].ToUpper();
                    if (string.IsNullOrEmpty(param)) { continue; }

                    if (param == "ALL")
                    {
                        Constants.MODDERATOR_COMMANDS.Clear();

                        string mess = "Removed all commands from moderator commands, moderators can now use all commands.";
                        args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, mess, CConst.Colors.Staff_Chat_Tag, args.SenderGameUser));
                        break;
                    }

                    if (Constants.MODDERATOR_COMMANDS.Contains(param))
                    {
                        Constants.MODDERATOR_COMMANDS.Remove(param);

                        string mess = $"Removed '/{param}' from moderator commands.";
                        args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, mess, CConst.Colors.Staff_Chat_Message, args.SenderGameUser));
                    }
                }

                if (args.Parameters.Count > 1)
                {
                    args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, $"Removed {args.Parameters.Count} moderator command(s)", CConst.Colors.Staff_Chat_Name, args.SenderGameUser));
                }

                SFDConfig.SaveConfig(SFDConfigSaveMode.HostGameOptions);
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

                            if (args.HostPrivileges && param.Equals("HostGun", StringComparison.OrdinalIgnoreCase))
                            {
                                WeaponItem wpnBazooka = new WeaponItem(WeaponItemType.Rifle, new WpnBazooka());
                                wpnBazooka.RWeaponData.Properties.MaxMagsInWeapon = 1;
                                wpnBazooka.RWeaponData.Properties.MaxRoundsInMag = 10;
                                wpnBazooka.RWeaponData.Properties.MaxCarriedSpareMags = 0;
                                wpnBazooka.RWeaponData.Properties.StartMags = 1;
                                wpnBazooka.RWeaponData.Properties.CooldownAfterPostAction = 100;
                                wpnBazooka.RWeaponData.Properties.ExtraAutomaticCooldown = 50;
                                wpnBazooka.RWeaponData.Properties.ProjectilesEachBlast = 3;
                                wpnBazooka.RWeaponData.Properties.SpecialAmmoBulletsRefill = 10;
                                wpnBazooka.RWeaponData.LazerUpgrade = 1;

                                wpnBazooka.RWeaponData.Properties.Projectile = new ProjectileBazooka();
                                wpnBazooka.RWeaponData.Properties.Projectile.Properties.InitialSpeed = 300; // 490
                                wpnBazooka.RWeaponData.Properties.Projectile.Properties.DodgeChance = 0.9f;
                                wpnBazooka.RWeaponData.Properties.Projectile.Properties.CanBeAbsorbedOrBlocked = true;

                                __instance.GameWorld.SlowmotionHandler.AddSlowmotion(new Slowmotion(100f, 250f, 1000f, 0.1f, player.ObjectID));
                                SFD.Effects.EffectHandler.PlayEffect("CAM_S", player.Position, __instance.GameWorld, 3.25f, 100f, false);
                                SFD.Sounds.SoundHandler.PlaySound("LogoSlam", player.Position, 1f, __instance.GameWorld);

                                player.GrabWeaponItem(wpnBazooka);
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
                
                // Manually create a vote, the results are sent to the owner user,
                // optionally, results can be shown to all players.
                // /DOVOTE [OVER-HALF?] [TEXT] [A] [B] [C?] [D?]
                if (!__instance.GameWorld.GameOverData.IsOver && !__instance.GameWorld.m_restartInstant &&
                    args.Parameters.Count >= 3 && args.IsCommand("DOVOTE") && args.CanUseModeratorCommand("DOVOTE")
                )
                {
                    if (__instance.VoteInfo.ActiveVotes.Count >= 1)
                    {
                        args.Feedback.Add(new(args.SenderGameUser, "There is already a vote in progress.", Color.Red, args.SenderGameUser));
                        return false;
                    }

                    bool requiresOverHalf = true;
                    if (args.Parameters[0] == "0" || args.Parameters[0].ToUpper() == "FALSE")
                    {
                        requiresOverHalf = false;
                    }

                    string description = "";
                    List<string> alternatives = [];

                    bool isDescription = true;
                    string temp = "";
                    for (int i = 0; i < args.Parameters.Count; i++)
                    {
                        // First argument was FALSE or 0
                        if (i == 0 && !requiresOverHalf) { continue; }

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
                        string mess = string.Format("Creating vote ({0})...", requiresOverHalf ? "over-half" : "all");
                        args.Feedback.Add(new(args.SenderGameUser, mess, Color.ForestGreen, args.SenderGameUser));

                        GameVote vote = new GameVoteManual(GameVote.GetNextVoteID(), args.SenderGameUser, description, alternatives.ToArray(), true, requiresOverHalf);
                        
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
                    return true;
                }
            
                // Manually start sudden-death.
                // /FORCESD [TIME?]
                if (args.IsCommand("FORCESUDDENDEATH", "FORCESD") && args.CanUseModeratorCommand("FORCESUDDENDEATH", "FORCESD"))
                {
                    int sdTime = 45;
                    if (args.Parameters.Count > 0)
                    {
                        if (!int.TryParse(args.Parameters[0], out sdTime))
                        {
                            sdTime = 45;
                        }
                    }
                    if (sdTime > 45) { sdTime = 45; }
                    if (sdTime < 5) { sdTime = 5; }

                    __instance.GameWorld.StartSuddenDeathForced(sdTime);
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

                // Staff messages
                if (args.IsCommand("STAFF", "S") && args.Parameters.Count > 0)
                {
                    if (args.SenderGameUser == null)
                    {
                        return false;
                    }
                    if (!server.Running)
                    {
                        return false;
                    }

                    string profName = args.SenderGameUser.GetProfileName();
                    string mess = UI.ChatTweaks.ConstructMetaTextToStaffChatMessage(args.SourceParameters, profName, args.SenderGameUser.TeamIcon);
                    NetMessage.ChatMessage.Data messageData = new(mess, CConst.Colors.Staff_Chat_Message, profName, true, args.SenderGameUser.UserIdentifier);

                    ConsoleOutput.ShowMessage(ConsoleOutputType.PlayerAction, $"Server: Sending staff message from '{profName}' ({args.SenderGameUser.AccountName})");
                    // Loop through slots and find moderators or the host
                    for (int i = 0; i < __instance.GameSlots.Length; i++)
                    {
                        GameSlot gameSlot = __instance.GameSlots[i];
                        GameUser slotUser = gameSlot.GameUser;
                        if (gameSlot.CurrentState == GameSlot.State.Occupied && slotUser != null && (slotUser.IsHost || slotUser.IsModerator))
                        {
                            GameConnectionTag gameConnectionTag = slotUser.GetGameConnectionTag();
                            NetConnection netConnection = gameConnectionTag?.NetConnection;

                            if (netConnection != null)
                            {
                                NetOutgoingMessage msg = NetMessage.ChatMessage.Write(ref messageData, server.m_server.CreateMessage());
                                server.m_server.SendMessage(msg, netConnection, NetDeliveryMethod.ReliableOrdered, 1);
                            }
                        }
                    }

                    // Leave a notification in DS
                    if (SFD.Program.IsServer)
                    {
                        string msg2 = messageData.Message;
                        if (messageData.IsMetaText)
                        {
                            msg2 = TextMeta.ToPlain(messageData.Message);
                        }

                        DSInfoNotification.Notify(new DSInfoNotification.ChatMessage(args.SenderGameUser.UserIdentifier, msg2, messageData.Color.ToWinDrawColor()));
                    }

                    return true;
                }

                // List commands in Moderator Commands
                // /MODERATOR_COMMANDS_GET [...]
                if (args.IsCommand("MHELP", "MODHELP"))
                {
                    if (Constants.MODDERATOR_COMMANDS.Count == 0)
                    {
                        string mess = "Moderators have access to all commands.";
                        args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, mess, CConst.Colors.Staff_Chat_Message, args.SenderGameUser));

                        return true;
                    }

                    args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, $"Listing all moderator commands...", CConst.Colors.Staff_Chat_Name, args.SenderGameUser));
                    foreach (string str in Constants.MODDERATOR_COMMANDS)
                    {
                        args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, $"'/{str}'", CConst.Colors.Staff_Chat_Message, args.SenderGameUser));
                    }
                    args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, $"Moderators have access to {Constants.MODDERATOR_COMMANDS.Count} command(s).", CConst.Colors.Staff_Chat_Name, args.SenderGameUser));

                    return true;
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

        // Public commands, only available whne using extended slots
        if (CConst.HOST_GAME_EXTENDED_SLOTS)
        {
            // Players will only see "?0" when listing players,
            // so we send them a server-sided list instead.
            // /SCOREBOARD
            if (args.IsCommand(
                "SCOREBOARD"
            ))
            {
                string header = "- SCOREBOARD -";
                args.Feedback.Add(new(args.SenderGameUser, header, Color.ForestGreen, args.SenderGameUser));

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

                    args.Feedback.Add(new(args.SenderGameUser, mess1, messCol, args.SenderGameUser));
                }
                return true;
            }
        }
        
        // Custom-Help command
        if (args.IsCommand("CTHELP"))
        {
            Dictionary<string[], string> hostCommands = new()
            {
                { ["MOUSE", "M"], "[1/0]" },
                { ["MODERATOR_COMMANDS_ADD"], "[...]" },
                { ["MODERATOR_COMMANDS_REMOVE"], "[...]" },
            };
            Dictionary<string[], string> moderatorCommands = new()
            {
                { ["GIVE"], "[USER] [ITEMS...]" },
                { ["DOVOTE"], "[OVER-HALF?] [TEXT] [A] [B] [C?] [D?]" },
                { ["FORCESERVERMOVEMENT", "FORCESVMOV"], "[USER] [1/0/NULL]" },
                { ["STAFF", "S"], "[MESSAGE]" },
                { ["FORCESUDDENDEATH", "FORCESD"], "[TIME?]" },
                { ["MHELP, MODHELP"], "" },

                // Extended-slots
                // { ["SLOTS"], "" },
                // { ["SETSLOT", "SSLOT"], "[INDEX] [0/1/2/4/5/6] [0/1/2/3/4]" }
            };
            Dictionary<string[], string> publicCommands = new()
            {
                // Extended-slots
                // { ["SCOREBOARD"], "" }
            };

            Color cSep = Color.LightBlue;
            Color cPublic = new(255, 181, 26);
            Color cMod = new(159, 255, 64);
            Color cHost = Color.Yellow;

            if (args.HostPrivileges)
            {
                args.Feedback.Add(new(args.SenderGameUser, "- HOST COMMANDS", cSep, args.SenderGameUser));
                foreach (KeyValuePair<string[], string> kvp in hostCommands)
                {
                    args.Feedback.Add(new(args.SenderGameUser, $"/{kvp.Key.First()} {kvp.Value}", cHost, args.SenderGameUser));
                }
            }
            if (args.ModeratorPrivileges)
            {
                args.Feedback.Add(new(args.SenderGameUser, "- MODERATOR COMMANDS", cSep, args.SenderGameUser));
                foreach (KeyValuePair<string[], string> kvp in moderatorCommands)
                {
                    if (args.CanUseModeratorCommand(kvp.Key))
                    {
                        args.Feedback.Add(new(args.SenderGameUser, $"/{kvp.Key.First()} {kvp.Value}", cMod, args.SenderGameUser));
                    }
                }
            }
            args.Feedback.Add(new(args.SenderGameUser, "- PUBLIC COMMANDS", cSep, args.SenderGameUser));
            foreach (KeyValuePair<string[], string> kvp in publicCommands)
            {
                args.Feedback.Add(new(args.SenderGameUser, $"/{kvp.Key.First()} {kvp.Value}", cPublic, args.SenderGameUser));
            }

            return true;
        }

        return false;
    }
}