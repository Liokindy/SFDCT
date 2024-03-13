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
        if (server == null)
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
                if (args.Parameters[0] == "1" || args.Parameters[0].ToUpperInvariant() == "TRUE")
                {
                    Commands.DebugMouse.IsEnabled = true;
                }
                if (args.Parameters[0] == "0" || args.Parameters[0].ToUpperInvariant() == "FALSE")
                {
                    Commands.DebugMouse.IsEnabled = false;
                }

                string mess = "Mouse dragging is now " + (Commands.DebugMouse.IsEnabled ? "enabled" : "disabled");
                args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, mess));
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
                if (args.IsCommand("GIVE") && args.CanUseModeratorCommand("GIVE") && args.Parameters.Count > 1)
                {
                    Color fbColor = Color.ForestGreen;

                    bool giveAll = args.Parameters[0].ToUpper() == "ALL";
                    GameUser selectedUser = __instance.GetGameUserByStringInput(args.Parameters[0], args.SenderGameUser);
                    Player selectedPlayer = selectedUser == null ? null : __instance.GameWorld.GetPlayerByUserIdentifier(selectedUser.UserIdentifier);

                    List<Player> playersToGive = [];
                    if (giveAll)
                    {
                        foreach(GameUser gu in __instance.GetGameUsers())
                        {
                            if (gu == null || gu.IsDisposed)
                            {
                                continue;
                            }

                            Player guPlayer = __instance.GameWorld.GetPlayerByUserIdentifier(gu.UserIdentifier);
                            playersToGive.Add(guPlayer);    
                        }
                    }
                    else
                    {
                        playersToGive.Add(selectedPlayer);
                    }

                    List<string> errorMessages = [];
                    string giveAllMessage = "";
                    foreach(Player worldPlayer in playersToGive)
                    {
                        if (worldPlayer == null || worldPlayer.IsDisposed || worldPlayer.IsRemoved || worldPlayer.IsDead) { continue; }

                        // Start at 1 so we don't use the user parameter
                        List<string> receivedStuff = [];
                        for (int i = 1; i < args.Parameters.Count; i++)
                        {
                            string parameter = args.Parameters[i].ToUpper();
                            if (string.IsNullOrEmpty(parameter))
                            {
                                continue;
                            }

                            // Heal the player to full health
                            if (parameter == "LIFE" || parameter == "HEAL" || parameter == "HEALTH")
                            {
                                // args.Feedback.Add(new(args.SenderGameUser, $"{worldPlayer.Name} was healed.", fbColor));
                                receivedStuff.Add("Heal");
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

                                    NetMessage.PlayerReceiveItem.Data data = new(worldPlayer.ObjectID, worldPlayer.CurrentHandgunWeapon, NetMessage.PlayerReceiveItem.ReceiveSourceType.GrabWeaponAmmo);
                                    server.SendMessage(MessageType.PlayerReceiveItem, data);
                                }
                                if (worldPlayer.CurrentRifleWeapon != null)
                                {
                                    worldPlayer.CurrentRifleWeapon.FillAmmoMax();
                                    resupplyAmmo = true;

                                    NetMessage.PlayerReceiveItem.Data data = new(worldPlayer.ObjectID, worldPlayer.CurrentRifleWeapon, NetMessage.PlayerReceiveItem.ReceiveSourceType.GrabWeaponAmmo);
                                    server.SendMessage(MessageType.PlayerReceiveItem, data);
                                }

                                if (resupplyAmmo)
                                {
                                    receivedStuff.Add("Ammo");
                                    // args.Feedback.Add(new(args.SenderGameUser, $"{worldPlayer.Name} was supplied with ammo.", fbColor));
                                }
                                continue;
                            }

                            // All weapons in the game
                            // (This is host-only, so it's his fault if he blows his computer up)
                            if (args.HostPrivileges && parameter == "ALL")
                            {
                                short[] WeaponIDs = [
                                    // 22, 7, 68
                                    24,01,28,02,17,06,05,19,26,03,
                                    04,31,08,11,41,10,12,18,13,14,
                                    15,16,20,25,27,29,09,23,42,43,
                                    44,45,21,30,32,33,58,34,35,36,
                                    37,38,39,40,49,47,48,46,50,51,
                                    52,53,55,54,57,56,59,62,61,63,
                                    64,65,66,67
                                ];

                                foreach (short id in WeaponIDs)
                                {
                                    worldPlayer.GrabWeaponItem(WeaponDatabase.GetWeapon(id));
                                }

                                receivedStuff.Add("All");
                                //args.Feedback.Add(new(args.SenderGameUser, $"{worldPlayer.Name} received a LOT of weapons.", fbColor));
                                break;
                            }

                            // Spawn a streetsweeper or streetsweepercrate
                            if (parameter == "SW" || parameter == "SWC" || parameter == "STREETSWEEPER" || parameter == "STREETSWEEPERCRATE")
                            {
                                string objectName = (parameter == "SW" ? "STREETSWEEPER" : (parameter == "SWC" ? "STREETSWEEPERCRATE" : parameter));
                                //args.Feedback.Add(new(args.SenderGameUser, $"{worldPlayer.Name} was delivered a {objectName}.", fbColor));
                                receivedStuff.Add(objectName);

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


                            // "Custom" weapons - vanilla weapons with modified base-properties
                            if (parameter.ToUpper() == "HOSTGUN" && args.HostPrivileges)
                            {
                                WeaponItem wpnBazooka = new WeaponItem(WeaponItemType.Rifle, new WpnBazooka());
                                wpnBazooka.RWeaponData.Properties.MaxMagsInWeapon = 1;
                                wpnBazooka.RWeaponData.Properties.MaxRoundsInMag = 10;
                                wpnBazooka.RWeaponData.Properties.MaxCarriedSpareMags = 0;
                                wpnBazooka.RWeaponData.Properties.StartMags = 1;
                                wpnBazooka.RWeaponData.Properties.CooldownAfterPostAction = 100;
                                wpnBazooka.RWeaponData.Properties.ExtraAutomaticCooldown = 50;
                                wpnBazooka.RWeaponData.Properties.ProjectilesEachBlast = 2;
                                wpnBazooka.RWeaponData.Properties.SpecialAmmoBulletsRefill = 10;
                                wpnBazooka.RWeaponData.LazerUpgrade = 1;
                                wpnBazooka.RWeaponData.PowerupFireRounds = 30;

                                wpnBazooka.RWeaponData.Properties.Projectile = new ProjectileBazooka();
                                wpnBazooka.RWeaponData.Properties.Projectile.Properties.InitialSpeed = 100; // 490
                                wpnBazooka.RWeaponData.Properties.Projectile.Properties.DodgeChance = 0.9f; // 0.9
                                wpnBazooka.RWeaponData.Properties.Projectile.Properties.CanBeAbsorbedOrBlocked = true;


                                __instance.GameWorld.SlowmotionHandler.AddSlowmotion(new Slowmotion(1f, 1f, 1250f, 0.01f, worldPlayer.ObjectID));
                                SFD.Effects.EffectHandler.PlayEffect("CAM_S", worldPlayer.Position, __instance.GameWorld, 3.25f, 100f, false);
                                SFD.Sounds.SoundHandler.PlaySound("LogoSlam", worldPlayer.Position, 1f, __instance.GameWorld);

                                worldPlayer.GrabWeaponItem(wpnBazooka);
                                args.Feedback.Add(new(args.SenderGameUser, $"{worldPlayer.Name} has received the \"HOST GUN\"", Color.DarkRed));
                                continue;
                            }


                            // Get a weapon with it's name
                            WeaponItem wpn = WeaponDatabase.GetWeapon(parameter);
                            short weaponID = 0;
                            if (wpn == null)
                            {
                                if (!short.TryParse(parameter, out weaponID))
                                {
                                    string mess = $"Could not find weapon with name \"{parameter}\"";
                                    if (!errorMessages.Contains(mess)) { errorMessages.Add(mess); }
                                    // args.Feedback.Add(new(args.SenderGameUser, $"Could not find weapon with name \"{parameter}\"", Color.Red, args.SenderGameUser));
                                    continue;
                                }
                                else
                                {
                                    // Try using the parsed ID instead
                                    wpn = WeaponDatabase.GetWeapon(weaponID);

                                    if (wpn == null)
                                    {
                                        string mess = $"Could not find weapon with ID \"{parameter}\"";
                                        if (!errorMessages.Contains(mess)) { errorMessages.Add(mess); }
                                        // args.Feedback.Add(new(args.SenderGameUser, $"Could not find weapon with ID \"{parameter}\"", Color.Red, args.SenderGameUser));
                                        continue;
                                    }
                                }
                            }

                            if (!wpn.BaseProperties.WeaponCanBeEquipped)
                            {
                                continue;
                            }

                            worldPlayer.GrabWeaponItem(wpn);
                            receivedStuff.Add($"\"{wpn.BaseProperties.WeaponNameID}\" ({wpn.BaseProperties.WeaponID})");
                            // args.Feedback.Add(new(args.SenderGameUser, $"{worldPlayer.Name} received \"{wpn.BaseProperties.WeaponNameID}\" ({wpn.BaseProperties.WeaponID})", fbColor));
                        }

                        if (receivedStuff.Count == 0)
                        {
                            continue;
                        }

                        string itemList = string.Join(", ", receivedStuff);
                        if (!giveAll)
                        {
                            args.Feedback.Add(new(args.SenderGameUser, $"{worldPlayer.Name} received: {itemList}.", Color.ForestGreen, args.SenderGameUser));
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(giveAllMessage))
                            {
                                giveAllMessage = itemList;
                            }
                        }
                    }

                    if (giveAll && !string.IsNullOrEmpty(giveAllMessage))
                    {
                        args.Feedback.Add(new(args.SenderGameUser, $"Everyone received: {giveAllMessage}", Color.ForestGreen, args.SenderGameUser));
                    }

                    return true;

                }
                
                // Manually create a vote, the results are sent to the owner user,
                // optionally, results can be shown to all players.
                // /DOVOTE [OVER-HALF?] [TEXT] [A] [B] [C?] [D?]
                if (!__instance.GameWorld.GameOverData.IsOver && !__instance.GameWorld.m_restartInstant &&
                    args.Parameters.Count > 2 && args.IsCommand("DOVOTE") && args.CanUseModeratorCommand("DOVOTE")
                )
                {
                    if (__instance.VoteInfo.ActiveVotes.Count >= 1)
                    {
                        args.Feedback.Add(new(args.SenderGameUser, "There is already a vote in progress.", Color.Red, args.SenderGameUser));
                        return true;
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
                        vote.ValidRemoteUniqueIdentifiers.AddRange(server.GetConnectedUniqueIdentifiers((NetConnection x) => x.GameConnectionTag() != null && x.GameConnectionTag().FirstGameUser != null && x.GameConnectionTag().FirstGameUser.CanVote));
                        __instance.VoteInfo.AddVote(vote);
                        server.SendMessage(MessageType.GameVote, new Pair<GameVote, bool>(vote, false));
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
            };
            Dictionary<string[], string> moderatorCommands = new()
            {
                { ["GIVE"], "[USER] [ITEMS...]" },
                { ["DOVOTE"], "[OVER-HALF?] [TEXT] [A] [B] [C?] [D?]" },
                { ["FORCESERVERMOVEMENT", "FORCESVMOV"], "[USER] [1/0/NULL]" },
                { ["STAFF", "S"], "[MESSAGE]" },
                { ["FORCESUDDENDEATH", "FORCESD"], "[TIME?]" },

                // Extended-slots
                { ["SLOTS"], "" },
                { ["SETSLOT", "SSLOT"], "[INDEX] [0/1/2/4/5/6] [0/1/2/3/4]" }
            };
            Dictionary<string[], string> publicCommands = new()
            {
                // Extended-slots
                { ["SCOREBOARD"], "" }
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