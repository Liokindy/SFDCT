using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using SFD;
using SFD.GUI.Text;
using SFD.ManageLists;
using SFDCT.Helper;
using CGlobals = SFDCT.Misc.Globals;
using CSettings = SFDCT.Settings.Values;

namespace SFDCT.OnlineServices;

[HarmonyPatch]
internal static class Host
{
    //     Make GameUsers that have 'ForcedServerMovementToggleTime' set to -1
    //     have forced server movement regardless of their latency, makes it
    //     possible to set their ForcedServerMovement elsewhere
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Server), nameof(Server.updateForcedServerMovement))]
    private static bool ServerUpdateForcedServerMovement(Server __instance, float time)
    {
        __instance.m_updateForcedServerMovementTime -= time;
        if (__instance.m_updateForcedServerMovementTime > 0)
        {
            return false;
        }
        __instance.m_updateForcedServerMovementTime = 100f;

        int svMovCount = 0;
        bool svMovActive = Constants.HOST_GAME_FORCED_SERVER_MOVEMENT_CHECK;
        bool svMovForced = Constants.HOST_GAME_FORCED_SERVER_MOVEMENT_PING == 0;
        float svMovPing = Constants.HOST_GAME_FORCED_SERVER_MOVEMENT_PING * 0.001f;
        float svMovToggleTime = Constants.HOST_GAME_FORCED_SERVER_MOVEMENT_TOGGLE_TIME_MS;

        lock (Server.ServerUpdateLockObject)
        {
            List<NetConnection> netConns = __instance.m_server.Connections;
            for(int i = netConns.Count - 1; i >= 0; i--)
            {
                NetConnection netConn = netConns[i];
                GameConnectionTag gameConnTag = netConn.GameConnectionTag();
                if (gameConnTag == null)
                {
                    continue;
                }

                bool SvMovWasSet = (gameConnTag.ForcedServerMovementToggleTime == -1f);
                bool doSvMov = svMovForced || (svMovActive && gameConnTag.Ping > svMovPing);
                if (gameConnTag.ForceServerMovement != doSvMov && !SvMovWasSet)
                {
                    gameConnTag.ForcedServerMovementToggleTime += 100f;
                    if (svMovForced || gameConnTag.ForcedServerMovementToggleTime > svMovToggleTime)
                    {
                        gameConnTag.ForceServerMovement = doSvMov;
                        if (gameConnTag.GameUsers != null)
                        {
                            for(int j = 0; j < gameConnTag.GameUsers.Length; j++)
                            {
                                GameUser gu = gameConnTag.GameUsers[j];
                                gu.ForceServerMovement = doSvMov;

                                Player playerByUserID = __instance.GameInfo.GameWorld.GetPlayerByUserIdentifier(gu.UserIdentifier);
                                playerByUserID?.UpdateCanDoPlayerAction();
                            }
                        }
                    }
                }
                else
                {
                    if (!SvMovWasSet)
                    {
                        gameConnTag.ForcedServerMovementToggleTime = 0f;
                    }
                }

                if (gameConnTag.ForceServerMovement)
                {
                    svMovCount++;
                }
            }
        }
        __instance.m_forcedServerMovementConnectionCount = svMovCount;
        
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Server), nameof(Server.DoReadRun))]
    private static bool ServerDoReadRun(Server __instance, NetIncomingMessage firstMessage, bool sendMultiPacket)
    {
        DoReadRun(__instance, firstMessage, sendMultiPacket);
        return false;
    }
    private static void DoReadRun(Server __instance, NetIncomingMessage firstMessage, bool sendMultiPacket)
    {
        int readMessageCount = 1000; //num
        NetIncomingMessage netIncomingMessage = firstMessage;

        int maximumSpectatorCount = CSettings.Get<int>(CSettings.GetKey(CSettings.SettingKey.AllowSpectatorsCount)); // doesnt affect host
        bool onlyLocalHostAsSpectator = !CSettings.Get<bool>(CSettings.GetKey(CSettings.SettingKey.AllowSpectators));
        bool onlyModeratorsAsSpectators = CSettings.Get<bool>(CSettings.GetKey(CSettings.SettingKey.AllowSpectatorsOnlyModerators));
        
        bool showDSPreviewJoinMessage = false;
        
        while (readMessageCount-- > 0 && (firstMessage != null || (netIncomingMessage = __instance.m_server.ReadMessage()) != null))
        {
            firstMessage = null;
            try
            {
                NetIncomingMessageType messageType = netIncomingMessage.MessageType;
                if (messageType <= NetIncomingMessageType.VerboseDebugMessage)
                {
                    if (messageType <= NetIncomingMessageType.Data)
                    {
                        switch (messageType)
                        {
                            case NetIncomingMessageType.StatusChanged:
                                byte messageByte = 0; //b
                                if (!netIncomingMessage.ReadByte(out messageByte)) { continue; }

                                NetConnectionStatus netConnectionStatus = (NetConnectionStatus)messageByte;
                                NetConnection senderConnection = netIncomingMessage.SenderConnection;

                                if (netConnectionStatus == NetConnectionStatus.Connected)
                                {
                                    if (senderConnection.GameConnectionTag() != null) { continue; }

                                    NetMessage.Connection.ConnectRequest.Data connectRequestData = default; // connectData 
                                    connectRequestData.PClientInstance = Guid.Empty;

                                    try
                                    {
                                        if (senderConnection.RemoteHailMessage == null)
                                        {
                                            connectRequestData.ConnectingUserCount = 0;
                                        }
                                        else
                                        {
                                            connectRequestData = NetMessage.Connection.ConnectRequest.Read(senderConnection.RemoteHailMessage, Profile.ValidateProfileType.CanEquip);
                                        }
                                    }
                                    catch
                                    {
                                        connectRequestData.ConnectingUserCount = 0;
                                    }

                                    if (connectRequestData.ConnectingUserCount <= 0)
                                    {
                                        senderConnection.Disconnect(Constants.NET.SERVER_BYE_MESSAGE);
                                        continue;
                                    }

                                    Server.ClientRequestData clientRequestData = __instance.GetClientRequestData(connectRequestData.PClientInstance, false);
                                    if (clientRequestData == null || (clientRequestData.ServerResponse != ServerResponse.AllowConnect && clientRequestData.ServerResponse != ServerResponse.SpectatorMode))
                                    {
                                        senderConnection.Disconnect(Constants.NET.SERVER_BYE_MESSAGE_NOT_NEGOTIATED);
                                        continue;
                                    }

                                    if (__instance.CurrentState == ServerClientState.Loading)
                                    {
                                        senderConnection.Disconnect(Constants.NET.SERVER_BYE_MESSAGE_CURRENTLY_LOADING);
                                        continue;
                                    }

                                    bool senderConnectionIsLocalHost = false; //flag
                                    if (SFD.Program.IsGame)
                                    {
                                        senderConnectionIsLocalHost = (senderConnection.IsLocalHost() && __instance.GameInfo.TotalGameUserCount == 0);
                                    }
                                    else if (SFD.Program.IsServer)
                                    {
                                        senderConnectionIsLocalHost = senderConnection.IsLocalHost();
                                    }
                                    bool senderConnectionIsHostAsSpectator = senderConnectionIsLocalHost && connectRequestData.AsSpectators; //flag2
                                    bool senderConnectionIsDedicatedPreview = false;

                                    if (__instance.GameInfo.GetAvailableGameSlotsCount() < connectRequestData.ConnectingUserCount)
                                    {
                                        if (senderConnectionIsLocalHost || (!senderConnectionIsLocalHost && !onlyLocalHostAsSpectator))
                                        {
                                            if (!onlyModeratorsAsSpectators)
                                            {
                                                Logger.LogInfo("connectRequestData.AsSpectators = true;");
                                                connectRequestData.AsSpectators = true;
                                            }
                                            else
                                            {
                                                string senderNetIP = netIncomingMessage.SenderConnection.RemoteEndPoint?.Address?.ToReadableString();
                                                RemoteUserItem item = RemoteUserList.ModeratorList.GetEntryByIP(senderNetIP);
                                                if (item != null && string.IsNullOrEmpty(item.Password))
                                                {
                                                    Logger.LogInfo("connectRequestData.AsSpectators = true;");
                                                    connectRequestData.AsSpectators = true;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            connectRequestData.AsSpectators = false;
                                        }
                                    }

                                    if (!senderConnectionIsLocalHost && __instance.GameInfo.SpectatorGameUserCount >= maximumSpectatorCount)
                                    {
                                        senderConnection.Disconnect(Constants.NET.SERVER_BYE_MESSAGE);
                                        continue;
                                    }

                                    List<GameSlot> gameSlotsList = null; //list
                                    if (!connectRequestData.AsSpectators)
                                    {
                                        GameInfo.DropInType dropInType = __instance.GameInfo.DropInMode;
                                        if (SFD.Program.IsGame && senderConnectionIsLocalHost && __instance.GameInfo.DropInMode == GameInfo.DropInType.OpenSlotsOnly)
                                        {
                                            dropInType = GameInfo.DropInType.OpenSlotsThenReplaceBots;
                                        }

                                        gameSlotsList = __instance.FindOpenGameSlots(dropInType, connectRequestData.ConnectingUserCount, __instance.GameInfo.EvenTeams);
                                    }

                                    if ((!connectRequestData.AsSpectators && (gameSlotsList.Count == 0 || gameSlotsList.Count < connectRequestData.ConnectingUserCount)) || (!senderConnectionIsLocalHost && !__instance.AllowJoin))
                                    {
                                        senderConnection.Disconnect(Constants.NET.SERVER_BYE_MESSAGE);
                                        continue;
                                    }

                                    bool validAccountData = true; //flag3
                                    string accountName = "";
                                    string account = ""; //text
                                    if (clientRequestData != null)
                                    {
                                        string key = clientRequestData.CryptoPhraseB + clientRequestData.Account;
                                        if (!Constants.Account.ReadAccountData(connectRequestData.AccountData, key, out accountName, out account))
                                        {
                                            validAccountData = false;
                                            if (senderConnectionIsHostAsSpectator)
                                            {
                                                validAccountData = true;
                                                accountName = "SERVER";
                                                senderConnectionIsDedicatedPreview = true;
                                            }
                                        }
                                    }
                                    if (!validAccountData || account != clientRequestData.Account)
                                    {
                                        senderConnection.Disconnect(Constants.NET.SERVER_BYE_MESSAGE_NOT_NEGOTIATED);
                                        continue;
                                    }

                                    if (!senderConnectionIsDedicatedPreview || (senderConnectionIsDedicatedPreview && showDSPreviewJoinMessage))
                                    {
                                        string joinSoundName = "PlayerJoin";
                                        NetMessage.Sound.Data joinSoundData = new NetMessage.Sound.Data(joinSoundName, true, Vector2.Zero, 1f);
                                        __instance.SendMessage(MessageType.Sound, joinSoundData, senderConnection);
                                    }

                                    Server.ServerPassRequests.RemovePassPair(netIncomingMessage.SenderEndPoint);
                                    bool flag4 = false; //flag4
                                    if (!connectRequestData.AsSpectators)
                                    {
                                        if (__instance.GameWorld == null)
                                        {
                                            flag4 = true;
                                        }
                                        else
                                        {
                                            int playingGameUserCount = __instance.GameInfo.PlayingGameUserCount;
                                            if (playingGameUserCount == 0)
                                            {
                                                flag4 = true;
                                            }
                                            else if (playingGameUserCount == 1)
                                            {
                                                flag4 = (__instance.GameWorld.MapType == SFDGameScriptInterface.MapType.Versus && __instance.GameWorld.PlayingUsersVersusMode != GameWorld.PlayingUserMode.Multi);
                                            }
                                        }
                                    }

                                    GameUser[] gameUsersArray = new GameUser[connectRequestData.ConnectingUserCount];
                                    GameConnectionTag gameConnectionTag = new GameConnectionTag(gameUsersArray, senderConnection, connectRequestData);
                                    senderConnection.Tag = gameConnectionTag;
                                    bool isNotWaitingInLobby = !__instance.WaitingInLobby; //flag5
                                    bool isModerator = false; //flag6
                                    int[] userIdentifiersArray = new int[connectRequestData.ConnectingUserCount]; //array2
                                    for (int i = 0; i < connectRequestData.ConnectingUserCount; i++)
                                    {
                                        if (gameSlotsList != null)
                                        {
                                            __instance.GameInfo.DisposeGameUser(gameSlotsList[i].GameUser, __instance);
                                        }

                                        int localUserIndex = (int)connectRequestData.LocalUserIndex[i];
                                        GameSlot gameSlot = (!connectRequestData.AsSpectators) ? gameSlotsList[i] : null;
                                        GameUser gameUser = new GameUser(localUserIndex, GameUser.GetNextUserIdentifier());
                                        gameUser.RequestServerMovement = connectRequestData.RequestServerMovement;
                                        gameUser.GameSlot = gameSlot;
                                        if (connectRequestData.UserProfiles != null && i < connectRequestData.UserProfiles.Length)
                                        {
                                            gameUser.Profile = connectRequestData.UserProfiles[i];
                                        }
                                        else
                                        {
                                            gameUser.Profile = new Profile();
                                        }
                                        gameUser.Profile.Name = __instance.GameInfo.GetUniqueNameForGameUser(gameUser);
                                        gameUser.Profile.Updated = true;
                                        if (onlyLocalHostAsSpectator && !senderConnectionIsLocalHost)
                                        {
                                            if (SFD.Program.IsGame)
                                            {
                                                gameUser.JoinedAsSpectator = false;
                                            }
                                            else if (SFD.Program.IsServer)
                                            {
                                                gameUser.JoinedAsSpectator = (senderConnectionIsLocalHost && connectRequestData.AsSpectators);
                                            }
                                        }
                                        else
                                        {
                                            gameUser.JoinedAsSpectator = connectRequestData.AsSpectators;
                                        }
                                        gameUser.Account = accountName;
                                        gameUser.AccountName = accountName;
                                        gameUser.SpectatingWhileWaitingToPlay = isNotWaitingInLobby;
                                        gameUser.IsHost = senderConnectionIsLocalHost;
                                        isModerator = senderConnectionIsLocalHost;
                                        gameUser.IsModerator = isModerator;
                                        gameUser.SetGameConnectionTag(gameConnectionTag);

                                        if (gameSlot != null)
                                        {
                                            gameSlot.CurrentState = GameSlot.State.Occupied;
                                            gameSlot.GameUser = gameUser;
                                        }

                                        gameUsersArray[i] = gameUser;
                                        __instance.GameInfo.AddGameUser(gameUser);
                                        userIdentifiersArray[i] = gameUser.UserIdentifier;

                                        if (!isModerator)
                                        {
                                            RemoteUserItem modItemByGameUser = __instance.GetModItemByGameUser(gameUser);
                                            if (modItemByGameUser != null && string.IsNullOrEmpty(modItemByGameUser.Password))
                                            {
                                                isModerator = true;
                                                gameUser.IsModerator = true;
                                            }
                                        }
                                    }

                                    __instance.m_activeGameConnectionTags.Add(gameConnectionTag);
                                    if (SFD.Program.IsGame && senderConnectionIsLocalHost)
                                    {
                                        ChatMessage.Show(LanguageHelper.GetText("menu.lobby.connected"), Color.White, "", false);
                                        ChatMessage.Show(LanguageHelper.GetText("menu.lobby.helpText"), Color.Yellow, "", false);
                                        if (__instance.GameInfo.PublicGame)
                                        {
                                            __instance.UpdateGameServerSettings();
                                        }
                                    }
                                    else if (__instance.GameInfo.PublicGame && __instance.AllowJoin)
                                    {
                                        __instance.PerformNextGameServerSettings(500f);
                                    }

                                    __instance.SendMessage(MessageType.ClientUserIdentifiers, userIdentifiersArray, null, senderConnection);
                                    foreach (GameUser gameUser2 in gameUsersArray)
                                    {
                                        string item = gameUser2.GetProfileName() + (gameUser2.JoinedAsSpectator ? " (Spectator)" : "");
                                        __instance.SyncGameUserInfo(gameUser2, null);
                                        GameSlot gameSlot2 = gameUser2.GameSlot;
                                        if (gameSlot2 != null)
                                        {
                                            __instance.SyncGameSlotInfo(gameSlot2, null);
                                        }
                                        string text2 = "menu.lobby.newPlayerJoined"; //text2
                                        List<string> list2 = new List<string>(); //list2
                                        list2.Add(item);
                                        if (gameSlot2 != null && gameSlot2.CurrentTeam != Team.Independent)
                                        {
                                            text2 = "menu.lobby.newPlayerJoinedTeam";
                                            List<string> list3 = list2; //list3
                                            int currentTeam = (int)gameSlot2.CurrentTeam;
                                            list3.Add(currentTeam.ToString());
                                        }
                                        string textId = text2 + ".toHost";

                                        List<string> list4 = new List<string>();
                                        list4.AddRange(list2);
                                        list4.Add(senderConnection.RemoteEndPoint.Address.ToReadableString());

                                        if (SFD.Program.IsServer)
                                        {
                                            string text3 = LanguageHelper.GetText(textId, list4.ToArray());
                                            DSInfoNotification.Notify(new DSInfoNotification.MessageLog(text3, System.Drawing.Color.Green));
                                            DSInfoNotification.Notify(new DSInfoNotification.ChatMessage(0, text3, Constants.COLORS.PLAYER_CONNECTED.ToWinDrawColor()));
                                        }

                                        if (!gameUser2.IsDedicatedPreview || (gameUser2.IsDedicatedPreview && showDSPreviewJoinMessage))
                                        {
                                            Color c = gameUser2.IsDedicatedPreview ? Constants.COLORS.SERVER_PREVIEW_CONNECTED : Constants.COLORS.PLAYER_CONNECTED;

                                            foreach (NetConnection netConnection in __instance.m_server.Connections)
                                            {
                                                if (netConnection.RemoteUniqueIdentifier != senderConnection.RemoteUniqueIdentifier)
                                                {
                                                    GameConnectionTag gameConnectionTag2 = netConnection.GameConnectionTag();
                                                    GameUser gameUser3 = (gameConnectionTag2 != null) ? gameConnectionTag2.FirstGameUser : null;

                                                    if (gameUser3 != null)
                                                    {
                                                        if (gameUser3 != null && gameUser3.IsHost)
                                                        {
                                                            __instance.SendMessage(MessageType.ChatMessageSuppressDSForm, new NetMessage.ChatMessage.Data(textId, c, list4.ToArray()), null, netConnection);
                                                        }
                                                        else
                                                        {
                                                            __instance.SendMessage(MessageType.ChatMessageSuppressDSForm, new NetMessage.ChatMessage.Data(text2, c, list2.ToArray()), null, netConnection);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        __instance.SyncAllGameInfo(senderConnection);

                                        List<NetMessage.ChatMessage.Data> workingCopyScriptsOrderedAsChatMessages = __instance.GameInfo.GetWorkingCopyScriptsOrderedAsChatMessages();
                                        if (workingCopyScriptsOrderedAsChatMessages.Count > 0)
                                        {
                                            foreach (NetMessage.ChatMessage.Data data in workingCopyScriptsOrderedAsChatMessages)
                                            {
                                                __instance.SendMessage(MessageType.ChatMessage, data, null, senderConnection);
                                            }
                                        }
                                        if (!string.IsNullOrEmpty(__instance.GameInfo.Description))
                                        {
                                            string msg = string.Format("[#FF0]Server description:[#] {0}", TextMeta.EscapeText(__instance.GameInfo.Description));
                                            __instance.SendMessage(MessageType.ChatMessage, new NetMessage.ChatMessage.Data(msg, Constants.COLORS.LIGHT_GRAY, "", true, 1), null, senderConnection);
                                        }
                                        if (Constants.HOST_GAME_KICK_MAX_PING_ENABLED)
                                        {
                                            __instance.SendMessage(MessageType.ChatMessage, new NetMessage.ChatMessage.Data(string.Format("Max ping is set to {0}", Constants.HOST_GAME_KICK_MAX_PING_VALUE), Constants.COLORS.PLAYER_CONNECTED), null, senderConnection);
                                        }
                                        if (!senderConnectionIsLocalHost && isModerator)
                                        {
                                            __instance.SendMessage(MessageType.ChatMessage, new NetMessage.ChatMessage.Data(string.Format("You're registered as a moderator. Type /help for available commands.", new object[0]), Constants.COLORS.MODERATOR_MESSAGE), null, senderConnection);
                                        }
                                        if (isNotWaitingInLobby)
                                        {
                                            __instance.SendMessage(MessageType.Signal, new NetMessage.Signal.Data(NetMessage.Signal.Type.LoadSignalSpectatorMode, __instance.GameInfo.MapInfo.LastSaveGuid), null, senderConnection);
                                            __instance.SendMessage(MessageType.GameWorldInformation, __instance.GameWorld, null, senderConnection);
                                            __instance.GameWorld.NewObjectsCollection.ResetPropertyUpdateReuse();
                                            gameConnectionTag.LoadingObjectSyncCount = __instance.GameWorld.NewObjectsCollection.LoadSyncCount;
                                            gameConnectionTag.LoadingProgress = 0f;
                                            gameConnectionTag.LoadingNODPackagesReceived.Clear();
                                            gameConnectionTag.LastLoadingMessageReceived = DateTime.Now;
                                            gameConnectionTag.LoadingDisconnectAccumulatedTime = 0f;
                                            gameConnectionTag.LoadingDisconnectTimestamp = DateTime.MinValue;
                                            __instance.SendMessage(MessageType.Signal, new NetMessage.Signal.Data(NetMessage.Signal.Type.LoadBeginFetchingDataSignal, gameConnectionTag.LoadingObjectSyncCount), null, senderConnection);
                                            __instance.m_server.FlushSendQueue();
                                            __instance.SendLoadStatus(true);
                                        }
                                        if (flag4 && __instance.GameWorld != null)
                                        {
                                            __instance.GameWorld.SetGameOver(GameWorld.GameOverReason.Default);
                                            continue;
                                        }
                                        continue;
                                    }
                                    continue;
                                }
                                else
                                {
                                    if (netConnectionStatus != NetConnectionStatus.Disconnected)
                                    {
                                        continue;
                                    }

                                    __instance.RemoveOldClientRequestData();
                                    GameConnectionTag gameConnectionTag3 = senderConnection.GameConnectionTag();
                                    if (gameConnectionTag3 == null)
                                    {
                                        __instance.ResyncConnections();
                                    }
                                    else
                                    {
                                        GameUser firstGameUser = gameConnectionTag3.FirstGameUser;
                                        bool isFirstGameUserDSPreview = senderConnection.IsLocalHost() && firstGameUser != null && firstGameUser.JoinedAsSpectator; //flag7
                                        foreach (GameUser gameUser4 in gameConnectionTag3.GameUsers)
                                        {
                                            __instance.ClientMovementMasterCounts.RemoveClientMovementMasterCount(gameUser4.UserIdentifier);
                                        }
                                        __instance.RemoveUserFromVotes(senderConnection.RemoteUniqueIdentifier);
                                        if (__instance.WaitingInLobby)
                                        {
                                            __instance.SendMessage(MessageType.Sound, new NetMessage.Sound.Data("PlayerLeave", true, Vector2.Zero, 1f), senderConnection);
                                        }
                                        else if (__instance.CurrentState == ServerClientState.Game || __instance.CurrentState == ServerClientState.Loading)
                                        {
                                            GameWorld gameWorld = __instance.GameWorld;
                                            if (gameWorld != null && !gameWorld.IsDisposed)
                                            {
                                                foreach (GameUser gameUser5 in gameConnectionTag3.GameUsers.ToList<GameUser>())
                                                {
                                                    __instance.GameInfo.StorePlayerStats(gameUser5);
                                                    Player playerByUserIdentifier = __instance.GameWorld.GetPlayerByUserIdentifier(gameUser5.UserIdentifier);
                                                    if (playerByUserIdentifier != null && !playerByUserIdentifier.IsDisposed && !playerByUserIdentifier.IsRemoved)
                                                    {
                                                        if (__instance.CurrentState == ServerClientState.Game)
                                                        {
                                                            playerByUserIdentifier.Kill(false, true);
                                                        }
                                                        else
                                                        {
                                                            playerByUserIdentifier.Remove(false);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        Color color = Constants.COLORS.PLAYER_DISCONNECTED;
                                        int num2 = 0;
                                        string a = "";
                                        if (netIncomingMessage.ReadString(out a) && a == "cbyeF")
                                        {
                                            num2 = 1;
                                            color = Constants.COLORS.PLAYER_LEFT_GAME;
                                        }
                                        ConsoleOutput.ShowMessage(ConsoleOutputType.Information, string.Format("Server: Client disconnected type: {0})", num2));
                                        if (gameConnectionTag3 == null)
                                        {
                                            ConsoleOutput.ShowMessage(ConsoleOutputType.Information, string.Format("Server: Unknown player disconnected type: {0}, resyncing", num2));
                                            __instance.ResyncConnections();
                                        }
                                        else
                                        {
                                            foreach (GameUser gameUser6 in gameConnectionTag3.GameUsers)
                                            {
                                                string item2 = gameUser6.GetProfileName() + (gameUser6.JoinedAsSpectator ? " (Spectator)" : "");
                                                string text4 = (num2 == 0) ? "menu.lobby.playerConnectionLost" : "menu.lobby.playerLeftGame";
                                                List<string> list5 = new List<string>();
                                                list5.Add(item2);
                                                string textId2 = text4 + ".toHost";
                                                List<string> list6 = new List<string>();
                                                list6.AddRange(list5);
                                                list6.Add(senderConnection.RemoteEndPoint.Address.ToReadableString());
                                                if (SFD.Program.IsServer)
                                                {
                                                    string text5 = LanguageHelper.GetText(textId2, list6.ToArray());
                                                    DSInfoNotification.Notify(new DSInfoNotification.MessageLog(text5, color.ToWinDrawColor()));
                                                    DSInfoNotification.Notify(new DSInfoNotification.ChatMessage(0, text5, color.ToWinDrawColor()));
                                                }

                                                if (!isFirstGameUserDSPreview)
                                                {
                                                    foreach (NetConnection netConnection2 in __instance.m_server.Connections)
                                                    {
                                                        if (netConnection2.RemoteUniqueIdentifier != senderConnection.RemoteUniqueIdentifier)
                                                        {
                                                            GameConnectionTag gameConnectionTag4 = netConnection2.GameConnectionTag();
                                                            GameUser gameUser7 = (gameConnectionTag4 != null) ? gameConnectionTag4.FirstGameUser : null;
                                                            if (gameUser7 != null)
                                                            {
                                                                if (gameUser7 != null && gameUser7.IsHost)
                                                                {
                                                                    __instance.SendMessage(MessageType.ChatMessageSuppressDSForm, new NetMessage.ChatMessage.Data(textId2, color, list6.ToArray()), null, netConnection2);
                                                                }
                                                                else
                                                                {
                                                                    __instance.SendMessage(MessageType.ChatMessageSuppressDSForm, new NetMessage.ChatMessage.Data(text4, color, list5.ToArray()), null, netConnection2);
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            __instance.DisposeGameConnection(gameConnectionTag3);
                                            if (__instance.GameInfo.PublicGame)
                                            {
                                                __instance.PerformNextGameServerSettings(500f);
                                            }
                                        }
                                    }
                                    __instance.RedistributeBots();
                                    if (__instance.CurrentState == ServerClientState.Game)
                                    {
                                        __instance.UpdateGameOverStatus();
                                        continue;
                                    }
                                    continue;
                                }
                            case NetIncomingMessageType.UnconnectedData:
                                try
                                {
                                    int num3 = __instance.GreetValid(netIncomingMessage);
                                    string text6;
                                    if (num3 == 0 && netIncomingMessage.ReadString(out text6) && text6 != null && (text6.StartsWith("SFDPING:") || text6 == "GREET") && text6.Length < 16)
                                    {
                                        bool flag8 = true;
                                        int num4;
                                        if (text6.StartsWith("SFDPING:") && !int.TryParse(text6.Substring(8), out num4))
                                        {
                                            flag8 = false;
                                        }
                                        if (flag8)
                                        {
                                            NetOutgoingMessage msg2 = __instance.m_server.CreateMessage(text6);
                                            __instance.m_server.SendUnconnectedMessage(msg2, netIncomingMessage.SenderEndPoint);
                                        }
                                    }
                                    if (num3 >= 3)
                                    {
                                        __instance.Block(netIncomingMessage);
                                    }
                                    continue;
                                }
                                catch
                                {
                                    continue;
                                }
                            default:
                                if (messageType != NetIncomingMessageType.Data)
                                {
                                    continue;
                                }
                                break;
                        }

                        if (netIncomingMessage.SenderConnection.Status == NetConnectionStatus.Connected)
                        {
                            NetMessage.MessageData messageData = NetMessage.ReadDataType(netIncomingMessage);
                            if (messageData.GameNumber != __instance.m_server.GameNumber)
                            {
                                if (!messageData.IsValid(__instance.m_server.GameNumber))
                                {
                                    goto IGNORE_PACKET;
                                }
                            }
                            try
                            {
                                __instance.HandleDataMessage(messageData, netIncomingMessage);
                                continue;
                            }
                            catch (Exception ex)
                            {
                                __instance.MarkAsTrash(netIncomingMessage, "Server ReadRun: " + ex.ToString(), Server.TrashMode.TrackGarbage);
                                continue;
                            }
                        IGNORE_PACKET:
                            ConsoleOutput.ShowMessage(ConsoleOutputType.Information, string.Format("Server: Ignoring invalid data packet from client - {0}", messageData.MessageType.ToString()));
                        }
                    }
                    else if (messageType != NetIncomingMessageType.DiscoveryRequest)
                    {
                        if (messageType != NetIncomingMessageType.VerboseDebugMessage)
                        {
                        }
                    }
                    else
                    {
                        bool flag9 = false;
                        if (!__instance.CheckSenderConnected(netIncomingMessage.SenderConnection))
                        {
                            string cryptPhraseA = "";
                            ServerResponse serverResponse = ServerResponse.RefuseConnect;
                            NetMessage.Connection.DiscoveryRequest.Data data2 = default(NetMessage.Connection.DiscoveryRequest.Data);
                            try
                            {
                                data2 = NetMessage.Connection.DiscoveryRequest.Read(netIncomingMessage);
                            }
                            catch (Exception)
                            {
                                data2.ConnectingUserCount = 0;
                            }

                            if (data2.ConnectingUserCount > 0)
                            {
                                if (data2.ConnectingUserCount < 1 || data2.ConnectingUserCount > 2)
                                {
                                    serverResponse = ServerResponse.RefuseConnectVersionDiffer;
                                    if (SFD.Program.IsServer)
                                    {
                                        string msg3 = string.Format("Invalid request ({0}) trying to connect (probably old SFD version) - refusing", netIncomingMessage.SenderEndPoint.Address.ToReadableString());
                                        if (__instance._limitedLogs.Add(msg3))
                                        {
                                            DSInfoNotification.Notify(new DSInfoNotification.MessageLog(msg3, System.Drawing.Color.OrangeRed));
                                        }
                                    }
                                }
                                else if (RemoteUserList.BanList.IsIPIncluded(netIncomingMessage.SenderEndPoint.Address.ToReadableString()))
                                {
                                    serverResponse = ServerResponse.RefuseConnect;
                                    if (SFD.Program.IsServer)
                                    {
                                        string msg4 = string.Format("Banned IP ({0}) trying to connect - refusing", netIncomingMessage.SenderEndPoint.Address.ToReadableString());
                                        if (__instance._limitedLogs.Add(msg4))
                                        {
                                            DSInfoNotification.Notify(new DSInfoNotification.MessageLog(msg4, System.Drawing.Color.OrangeRed));
                                        }
                                    }
                                }
                                else if (KickList.OnTimeout(netIncomingMessage.SenderEndPoint.Address.ToReadableString()))
                                {
                                    serverResponse = ServerResponse.RefuseConnectKicked;
                                    if (SFD.Program.IsServer)
                                    {
                                        string msg5 = string.Format("Kicked IP ({0}) trying to connect - refusing", netIncomingMessage.SenderEndPoint.Address.ToReadableString());
                                        if (__instance._limitedLogs.Add(msg5))
                                        {
                                            DSInfoNotification.Notify(new DSInfoNotification.MessageLog(msg5, System.Drawing.Color.OrangeRed));
                                        }
                                    }
                                }
                                else if (!string.IsNullOrWhiteSpace(data2.Version) && data2.Version != CGlobals.Version.SFD)
                                {
                                    serverResponse = ServerResponse.RefuseConnectVersionDiffer;
                                    if (SFD.Program.IsServer)
                                    {
                                        string msg6 = string.Format("Invalid request ({0}) trying to connect using another version of the game - refusing", netIncomingMessage.SenderEndPoint.Address.ToReadableString());
                                        if (__instance._limitedLogs.Add(msg6))
                                        {
                                            DSInfoNotification.Notify(new DSInfoNotification.MessageLog(msg6, System.Drawing.Color.OrangeRed));
                                        }
                                    }
                                }
                                else if (__instance.AllowJoin || (SFD.Program.IsServer && data2.AsSpectators && netIncomingMessage.IsLocalHost()))
                                {
                                    if (__instance.CurrentState == ServerClientState.Loading)
                                    {
                                        serverResponse = ServerResponse.CurrentlyLoading;
                                    }
                                    else
                                    {
                                        if (__instance.GameInfo.GetAvailableGameSlotsCount() < data2.ConnectingUserCount && __instance.GameInfo.SpectatorGameUserCount < maximumSpectatorCount)
                                        {
                                            if (!(onlyLocalHostAsSpectator && !netIncomingMessage.SenderConnection.IsLocalHost()))
                                            {
                                                if (!onlyModeratorsAsSpectators)
                                                {
                                                    Logger.LogInfo("data2.AsSpectators = true;");
                                                    data2.AsSpectators = true;
                                                }
                                                else
                                                {
                                                    string senderNetIP = netIncomingMessage.SenderConnection.RemoteEndPoint?.Address?.ToReadableString();
                                                    RemoteUserItem item = RemoteUserList.ModeratorList.GetEntryByIP(senderNetIP);
                                                    if (item != null && string.IsNullOrEmpty(item.Password))
                                                    {
                                                        Logger.LogInfo("data2.AsSpectators = true;");
                                                        data2.AsSpectators = true;
                                                    }
                                                }
                                            }
                                        }

                                        if (SFD.Program.IsGame && __instance.GameInfo.TotalGameUserCount == 0)
                                        {
                                            if (netIncomingMessage.IsLocalHost())
                                            {
                                                serverResponse = ServerResponse.AllowConnect;
                                            }
                                        }
                                        else if (data2.AsSpectators || __instance.GameInfo.GetAvailableGameSlotsCount() >= data2.ConnectingUserCount)
                                        {
                                            if (SFD.Program.IsServer && data2.AsSpectators && netIncomingMessage.IsLocalHost())
                                            {
                                                serverResponse = ServerResponse.AllowConnect;
                                            }
                                            else
                                            {
                                                bool hasPassword = Server.HasPassword;
                                                bool flag10 = !string.IsNullOrEmpty(data2.Passphrase);
                                                if (hasPassword && !flag10)
                                                {
                                                    Server.PassPair passPair = Server.ServerPassRequests.CreateSenderPassPair(netIncomingMessage.SenderEndPoint, Server.Password);
                                                    cryptPhraseA = passPair.CryptoPhrase;
                                                    serverResponse = ServerResponse.CheckingPassword;
                                                }
                                                else if (hasPassword && !Server.ServerPassRequests.ConsumeAndCheckSenderPass(netIncomingMessage.SenderEndPoint, data2.Passphrase, false))
                                                {
                                                    serverResponse = ServerResponse.RefuseConnectInvalidPassword;
                                                }
                                                else
                                                {
                                                    serverResponse = ServerResponse.AllowConnect;
                                                    if (RemoteUserList.BanList.IsAccountIncluded(data2.Account))
                                                    {
                                                        serverResponse = ServerResponse.RefuseConnect;
                                                    }
                                                    else
                                                    {
                                                        int num5 = __instance.GreetValid(netIncomingMessage);
                                                        if (num5 >= 1)
                                                        {
                                                            if (netIncomingMessage.SenderConnection != null)
                                                            {
                                                                netIncomingMessage.SenderConnection.Deny();
                                                            }
                                                            serverResponse = ServerResponse.RefuseConnect;
                                                        }
                                                        if (num5 >= 2)
                                                        {
                                                            flag9 = true;
                                                        }
                                                        if (num5 >= 3)
                                                        {
                                                            __instance.Block(netIncomingMessage);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            serverResponse = ServerResponse.RefuseConnectFullLobby;
                                            if (SFD.Program.IsServer)
                                            {
                                                string msg7 = string.Format("Connection refused for IP ({0}). Not enough open game slots available.", netIncomingMessage.SenderEndPoint.Address.ToString());
                                                if (__instance._limitedLogs.Add(msg7))
                                                {
                                                    DSInfoNotification.Notify(new DSInfoNotification.MessageLog(msg7, System.Drawing.Color.OrangeRed));
                                                }
                                            }
                                        }

                                        if (!__instance.WaitingInLobby && serverResponse == ServerResponse.AllowConnect)
                                        {
                                            serverResponse = ServerResponse.SpectatorMode;
                                        }
                                    }
                                }
                            }

                            if (!flag9)
                            {
                                Server.ClientRequestData clientRequestData2 = __instance.GetClientRequestData(data2.PClientInstance, true);
                                clientRequestData2.ServerResponse = serverResponse;
                                clientRequestData2.Account = data2.Account;
                                string cryptoPhraseB = clientRequestData2.CryptoPhraseB;
                                NetMessage.Connection.DiscoveryResponse.Data data3 = new NetMessage.Connection.DiscoveryResponse.Data(serverResponse, CGlobals.Version.SFD, cryptPhraseA, cryptoPhraseB, Constants.PApplicationInstance);
                                __instance.m_server.SendDiscoveryResponse(NetMessage.Connection.DiscoveryResponse.Write(ref data3, __instance.m_server.CreateMessage()), netIncomingMessage.SenderEndPoint);
                            }
                        }
                    }
                }
                else if (messageType <= NetIncomingMessageType.WarningMessage)
                {
                    if (messageType != NetIncomingMessageType.DebugMessage && messageType != NetIncomingMessageType.WarningMessage)
                    {

                    }
                }
                else if (messageType != NetIncomingMessageType.ErrorMessage && messageType != NetIncomingMessageType.ConnectionLatencyUpdated)
                {

                }
            }
            catch(NullReferenceException ex)
            {
                SFD.Program.ShowError(ex, "Error: Bad read for server. " + ((netIncomingMessage == null) ? "null msg" : netIncomingMessage.MessageType.ToString()), false);
                return;
            }
        }

        if (__instance.GameWorld != null && sendMultiPacket)
        {
            __instance.GameWorld.SendMultiPacket();
            return;
        }
    }

    //     Make the dedicated server preview bypass the ReadAccountData check to
    //     join the server while having an invalid AccountName
    //[HarmonyTranspiler]
    //[HarmonyPatch(typeof(Server), nameof(Server.DoReadRun))]
    private static IEnumerable<CodeInstruction> ServerDoReadRunDSPreviewFix(IEnumerable<CodeInstruction> instructions, ILGenerator il)
    {
        // The "flag" variable stores if the sender is the DS preview.
        //
        // We add CodeInstructions that check for this variable (ldloc_S 20) and branches
        // after "flag3" is supposed to be set to false.
        //
        // We do this because "flag3" is only set to false after "ReadAccountData" returns false.
        // (Which it will, because the AccountName provided is empty.)

        // In-game code
        /*
        bool flag = false;
        .
        .
        else if (SFD.Program.IsServer)
        {
            flag = senderConnection.IsLocalHost();
        }
        .
        .
        if (!Constants.Account.ReadAccountData(connectData.AccountData, key, out accountName, out text))
        {
            flag3 = false;
        }
        */

        // Define a label in the instructions after "flag = false;"
        Label returnLabel = il.DefineLabel();
        List<CodeInstruction> code = new List<CodeInstruction>(instructions);

        code.ElementAt(552).labels.Add(returnLabel);

        // Add the code instructions to branch if "flag" is true
        code.Insert(550, new(OpCodes.Ldloc_S, 20));
        code.Insert(551, new(OpCodes.Brtrue_S, returnLabel));

        return code;
    }
    
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Server), nameof(Server.SendClientTeamMessage))]
    private static bool ServerSendClientTeamMessage(Server __instance, GameUser senderGameUser, NetMessage.ChatMessage.Data dataToSend)
    {
        if (!__instance.Running)
        {
            return false;
        }

        foreach(GameUser gameUser in __instance.GameInfo.GetGameUsers())
        {
            if (gameUser != null)
            {
                if (gameUser.GameSlotTeam == senderGameUser.GameSlotTeam)
                {
                    GameConnectionTag gameConnectionTag = gameUser.GetGameConnectionTag();
                    NetConnection netConnection = (gameConnectionTag != null) ? gameConnectionTag.NetConnection : null;
                    if (netConnection != null)
                    {
                        NetOutgoingMessage msg = NetMessage.ChatMessage.Write(ref dataToSend, __instance.m_server.CreateMessage());
                        __instance.m_server.SendMessage(msg, netConnection, NetDeliveryMethod.ReliableOrdered, 1);
                    }
                }
            }
        }

        if (SFD.Program.IsServer)
        {
            string msg2 = dataToSend.Message;
            if (dataToSend.IsMetaText)
            {
                msg2 = TextMeta.ToPlain(dataToSend.Message);
            }
            DSInfoNotification.Notify(new DSInfoNotification.ChatMessage((senderGameUser != null) ? senderGameUser.UserIdentifier : -1, msg2, dataToSend.Color.ToWinDrawColor()));
        }
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameUser), nameof(GameUser.IsDedicatedPreview), MethodType.Getter)]
    private static bool GameUserIsDedicatedPreview(ref bool __result, GameUser __instance)
    {
        __result = __instance.IsHost && __instance.JoinedAsSpectator && __instance.AccountName == "SERVER";
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameInfo), nameof(GameInfo.TotalGameUserCount), MethodType.Getter)]
    private static bool GameInfoTotalGameUserCount(ref int __result, GameInfo __instance)
    {
        __result = __instance.GetGameUsers().Count((GameUser gameUser) => gameUser.IsUser && !gameUser.JoinedAsSpectator);
        return false;
    }
}