using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection.Emit;
using Lidgren.Network;
using HarmonyLib;
using SFD;
using CConst = SFDCT.Misc.Globals;

namespace SFDCT.OnlineServices;

[HarmonyPatch]
internal static class Slots
{
    // Use our own arrays for slot states
    // slot states say if the slot is open/closed/bot
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Constants), nameof(Constants.GET_HOST_GAME_SLOT_STATE))]
    private static bool ConstantsGetHostGameSlotState(ref byte __result, int index)
    {
        if (CConst.HOST_GAME_EXTENDED_SLOTS)
        {
            __result = CConst.HOST_GAME_SLOT_STATES[index];
            return false;
        }
        return true;
    }
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Constants), nameof(Constants.SET_HOST_GAME_SLOT_STATE))]
    private static bool ConstantsSetHostGameSlotState(int index, byte value)
    {
        if (CConst.HOST_GAME_EXTENDED_SLOTS)
        {
            CConst.HOST_GAME_SLOT_STATES[index] = value;
            return false;
        }
        return true;
    }

    // Use different arrays for slot teams
    // Those arrays will have different sizes than 8
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Constants), nameof(Constants.GET_HOST_GAME_SLOT_TEAM))]
    private static bool ConstantsGetHostGameSlotTeam(ref Team __result, int index)
    {
        if (CConst.HOST_GAME_EXTENDED_SLOTS)
        {
            __result = CConst.HOST_GAME_SLOT_TEAMS[index];
            return false;
        }
        return true;
    }
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Constants), nameof(Constants.SET_HOST_GAME_SLOT_TEAM))]
    private static bool ConstantsSetHostGameSlotTeam(int index, Team value)
    {
        if (CConst.HOST_GAME_EXTENDED_SLOTS)
        {
            CConst.HOST_GAME_SLOT_TEAMS[index] = value;
            return false;
        }
        return true;
    }

    // Increase GameInfo.GameSlots array size for out slot count
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(GameInfo), MethodType.Constructor, new Type[] { typeof(GameOwnerEnum) })]
    private static IEnumerable<CodeInstruction> GameInfoConstructor(IEnumerable<CodeInstruction> instructions)
    {
        instructions.ElementAt(94).opcode = OpCodes.Ldc_I4_S;
        instructions.ElementAt(94).operand = CConst.SLOTCOUNT;
        return instructions;
    }

    // Increase maximum connections for our slot count
    // SFD uses 12 connections, 8 players and 4 spectators
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(Server), nameof(Server.Start))]
    private static IEnumerable<CodeInstruction> ServerStart(IEnumerable<CodeInstruction> instructions)
    {
        instructions.ElementAt(26).opcode = OpCodes.Ldc_I4_S;
        instructions.ElementAt(26).operand = CConst.SLOTCOUNT + 4;
        return instructions;
    }

    // Change the number of times the for loop
    // that initializes GameSlots runs for our slot count
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(GameInfo), nameof(GameInfo.InitOpenGameSlots))]
    private static IEnumerable<CodeInstruction> GameInfoInitOpenGameSlots(IEnumerable<CodeInstruction> instructions)
    {
        instructions.ElementAt(54).opcode = OpCodes.Ldc_I4_S;
        instructions.ElementAt(54).operand = CConst.SLOTCOUNT;
        return instructions;
    }


    // The following patches are to fix errors caused
    // by the client only expecting information about 8 GameSlots

    // GameSlot Changes
    [HarmonyPrefix]
    [HarmonyPatch(typeof(NetMessage.GameInfo.GameSlotChange), nameof(NetMessage.GameInfo.GameSlotChange.Read))]
    private static bool GameInfoGameSlotChangeRead(ref NetMessage.GameInfo.GameSlotChange.Data __result, NetIncomingMessage netIncomingMessage)
    {
        if (CConst.HOST_GAME_EXTENDED_SLOTS)
        {
            NetMessage.GameInfo.GameSlotChange.Data result;
            result.SlotIndex = netIncomingMessage.ReadRangedInteger(0, CConst.SLOTCOUNT - 1);
            result.DataToChange = (NetMessage.GameInfo.GameSlotChange.DataChangeType)netIncomingMessage.ReadRangedInteger(0, 3);
            result.NewValue = netIncomingMessage.ReadInt32();
            __result = result;

            return false;
        }
        return true;
    }
    [HarmonyPrefix]
    [HarmonyPatch(typeof(NetMessage.GameInfo.GameSlotChange), nameof(NetMessage.GameInfo.GameSlotChange.Write))]
    private static bool GameInfoGameSlotChangeWrite(ref NetOutgoingMessage __result, ref NetMessage.GameInfo.GameSlotChange.Data dataToWrite, NetOutgoingMessage netOutgoingMessage)
    {
        if (CConst.HOST_GAME_EXTENDED_SLOTS)
        {
            NetMessage.WriteDataType(MessageType.GameInfo_GameSlotChange, netOutgoingMessage);
            netOutgoingMessage.WriteRangedInteger(0, CConst.SLOTCOUNT - 1, dataToWrite.SlotIndex);
            netOutgoingMessage.WriteRangedInteger(0, 3, (int)dataToWrite.DataToChange);
            netOutgoingMessage.Write(dataToWrite.NewValue);
            __result = netOutgoingMessage;

            return false;
        }
        return true;
    }

    // GameSlot Updates
    [HarmonyPrefix]
    [HarmonyPatch(typeof(NetMessage.GameInfo.GameSlotUpdate), nameof(NetMessage.GameInfo.GameSlotUpdate.Read))]
    private static bool GameInfoGameSlotUpdateRead(GameSlot[] connectionSlotsDestination, NetIncomingMessage netIncomingMessage)
    {
        if (CConst.HOST_GAME_EXTENDED_SLOTS)
        {
            int num = netIncomingMessage.ReadRangedInteger(0, CConst.SLOTCOUNT - 1);
            GameSlot gameSlot = connectionSlotsDestination[num];
            gameSlot.CurrentState = (GameSlot.State)netIncomingMessage.ReadRangedInteger(0, 7);
            gameSlot.CurrentTeam = (Team)netIncomingMessage.ReadRangedInteger(0, 7);
            gameSlot.NextTeam = (Team)netIncomingMessage.ReadRangedInteger(0, 7);

            return false;
        }
        return true;
    }
    [HarmonyPrefix]
    [HarmonyPatch(typeof(NetMessage.GameInfo.GameSlotUpdate), nameof(NetMessage.GameInfo.GameSlotUpdate.Write))]
    public static bool GameInfoGameSlotUpdateWrite(ref NetOutgoingMessage __result, ref NetMessage.GameInfo.GameSlotUpdate.DataWrite dataToWrite, NetOutgoingMessage netOutgoingMessage)
    {
        if (CConst.HOST_GAME_EXTENDED_SLOTS)
        {
            NetMessage.WriteDataType(MessageType.GameInfo_GameSlotUpdate, netOutgoingMessage);
            GameSlot gameSlot = dataToWrite.ConnectionSlots[dataToWrite.ConnectionSlotIndex];
            netOutgoingMessage.WriteRangedInteger(0, CConst.SLOTCOUNT - 1, dataToWrite.ConnectionSlotIndex);
            netOutgoingMessage.WriteRangedInteger(0, 7, (int)gameSlot.CurrentState);
            netOutgoingMessage.WriteRangedInteger(0, 7, (int)gameSlot.CurrentTeam);
            netOutgoingMessage.WriteRangedInteger(0, 7, (int)gameSlot.NextTeam);
            __result = netOutgoingMessage;

            return false;
        }
        return true;
    }

    // GameUser Updates
    [HarmonyPrefix]
    [HarmonyPatch(typeof(NetMessage.GameInfo.GameUserUpdate), nameof(NetMessage.GameInfo.GameUserUpdate.Read))]
    private static bool GameInfoGameUserUpdateRead(ref NetMessage.GameInfo.GameUserUpdate.Data __result, NetIncomingMessage netIncomingMessage, Profile.ValidateProfileType validateProfileType)
    {
        if (CConst.HOST_GAME_EXTENDED_SLOTS)
        {
            NetMessage.GameInfo.GameUserUpdate.Data result = default(NetMessage.GameInfo.GameUserUpdate.Data);
            netIncomingMessage.ReadBoolean();
            result.StartIndex = netIncomingMessage.ReadRangedInteger(0, 31);
            result.EndIndex = netIncomingMessage.ReadRangedInteger(0, 31);
            NetMessage.GameInfo.GameUserUpdate.GameUserNetData[] array = new NetMessage.GameInfo.GameUserUpdate.GameUserNetData[netIncomingMessage.ReadRangedInteger(0, 31)];
            for (int i = 0; i < array.Length; i++)
            {
                int userIdentifier = netIncomingMessage.ReadInt32();
                int localUserIndex = netIncomingMessage.ReadRangedInteger(0, 3);
                GameUser gameUser = new GameUser(localUserIndex, userIdentifier);
                int gameSlotIndex = -1;
                if (result.StartIndex <= i && i < result.EndIndex)
                {
                    gameUser.Account = netIncomingMessage.ReadString();
                    gameUser.AccountName = netIncomingMessage.ReadString();
                    gameUser.UserType = (GameUser.Type)netIncomingMessage.ReadRangedInteger(0, 3);
                    if (gameUser.UserType == GameUser.Type.Bot)
                    {
                        switch (netIncomingMessage.ReadRangedInteger(0, 4))
                        {
                            case 1:
                                gameUser.BotPredefinedAIType = SFDGameScriptInterface.PredefinedAIType.BotA;
                                break;
                            case 2:
                                gameUser.BotPredefinedAIType = SFDGameScriptInterface.PredefinedAIType.BotB;
                                break;
                            case 3:
                                gameUser.BotPredefinedAIType = SFDGameScriptInterface.PredefinedAIType.BotC;
                                break;
                            case 4:
                                gameUser.BotPredefinedAIType = SFDGameScriptInterface.PredefinedAIType.BotD;
                                break;
                            default:
                                gameUser.BotPredefinedAIType = SFDGameScriptInterface.PredefinedAIType.BotA;
                                break;
                        }
                    }
                    gameUser.Score.TotalGames = (int)netIncomingMessage.ReadUInt16();
                    gameUser.Score.TotalWins = (int)netIncomingMessage.ReadUInt16();
                    gameUser.Score.TotalLosses = (int)netIncomingMessage.ReadUInt16();
                    gameUser.SpectatingWhileWaitingToPlay = netIncomingMessage.ReadBoolean();
                    gameUser.JoinedAsSpectator = netIncomingMessage.ReadBoolean();
                    gameUser.IsHost = netIncomingMessage.ReadBoolean();
                    gameUser.IsModerator = netIncomingMessage.ReadBoolean();
                    gameUser.RequestServerMovement = netIncomingMessage.ReadBoolean();
                    gameSlotIndex = netIncomingMessage.ReadRangedInteger(-1, CConst.SLOTCOUNT);
                    if (netIncomingMessage.ReadBoolean())
                    {
                        gameUser.Profile = NetMessage.PlayerProfileMessage.Read(netIncomingMessage, validateProfileType);
                    }
                }
                NetMessage.GameInfo.GameUserUpdate.GameUserNetData gameUserNetData = new NetMessage.GameInfo.GameUserUpdate.GameUserNetData(gameUser, gameSlotIndex);
                array[i] = gameUserNetData;
            }
            result.GUNDs = array;
            __result = result;

            return false;
        }
        return true;
    }
    [HarmonyPrefix]
    [HarmonyPatch(typeof(NetMessage.GameInfo.GameUserUpdate), nameof(NetMessage.GameInfo.GameUserUpdate.Write))]
    private static bool GameInfoGameUserUpdateWrite(ref NetOutgoingMessage __result, ref NetMessage.GameInfo.GameUserUpdate.Data dataToWrite, NetOutgoingMessage netOutgoingMessage)
    {
        if (CConst.HOST_GAME_EXTENDED_SLOTS)
        {
            NetMessage.WriteDataType(MessageType.GameInfo_GameUserUpdate, netOutgoingMessage);
            netOutgoingMessage.Write(dataToWrite.MultiMode);
            netOutgoingMessage.WriteRangedInteger(0, 31, dataToWrite.StartIndex);
            netOutgoingMessage.WriteRangedInteger(0, 31, dataToWrite.EndIndex);
            netOutgoingMessage.WriteRangedInteger(0, 31, dataToWrite.GUNDs.Length);
            for (int i = 0; i < dataToWrite.GUNDs.Length; i++)
            {
                NetMessage.GameInfo.GameUserUpdate.GameUserNetData gameUserNetData = dataToWrite.GUNDs[i];
                netOutgoingMessage.Write(gameUserNetData.GameUser.UserIdentifier);
                netOutgoingMessage.WriteRangedInteger(0, 3, gameUserNetData.GameUser.LocalUserIndex);
                if (dataToWrite.StartIndex <= i && i < dataToWrite.EndIndex)
                {
                    netOutgoingMessage.Write(gameUserNetData.GameUser.Account);
                    netOutgoingMessage.Write(gameUserNetData.GameUser.AccountName);
                    netOutgoingMessage.WriteRangedInteger(0, 3, (int)gameUserNetData.GameUser.UserType);
                    if (gameUserNetData.GameUser.UserType == GameUser.Type.Bot)
                    {
                        byte value = 0;
                        SFDGameScriptInterface.PredefinedAIType botPredefinedAIType = gameUserNetData.GameUser.BotPredefinedAIType;
                        if (botPredefinedAIType != SFDGameScriptInterface.PredefinedAIType.BotA)
                        {
                            switch (botPredefinedAIType)
                            {
                                case SFDGameScriptInterface.PredefinedAIType.BotB:
                                    value = 2;
                                    break;
                                case SFDGameScriptInterface.PredefinedAIType.BotC:
                                    value = 3;
                                    break;
                                case SFDGameScriptInterface.PredefinedAIType.BotD:
                                    value = 4;
                                    break;
                            }
                        }
                        else
                        {
                            value = 1;
                        }
                        netOutgoingMessage.WriteRangedInteger(0, 4, (int)value);
                    }
                    GameUserScore score = gameUserNetData.GameUser.Score;
                    netOutgoingMessage.Write((ushort)((score != null) ? score.TotalGames : 0));
                    netOutgoingMessage.Write((ushort)((score != null) ? score.TotalWins : 0));
                    netOutgoingMessage.Write((ushort)((score != null) ? score.TotalLosses : 0));
                    netOutgoingMessage.Write(gameUserNetData.GameUser.SpectatingWhileWaitingToPlay);
                    netOutgoingMessage.Write(gameUserNetData.GameUser.JoinedAsSpectator);
                    netOutgoingMessage.Write(gameUserNetData.GameUser.IsHost);
                    netOutgoingMessage.Write(gameUserNetData.GameUser.IsModerator);
                    netOutgoingMessage.Write(gameUserNetData.GameUser.RequestServerMovement);
                    netOutgoingMessage.WriteRangedInteger(-1, CConst.SLOTCOUNT, gameUserNetData.GameSlotIndex);
                    Profile profile = gameUserNetData.GameUser.Profile;
                    if (profile == null)
                    {
                        netOutgoingMessage.Write(false);
                    }
                    else
                    {
                        netOutgoingMessage.Write(true);
                        NetMessage.PlayerProfileMessage.Write(profile, netOutgoingMessage);
                    }
                }
            }
            __result = netOutgoingMessage;
            return false;
        }
        return true;
    }
}
