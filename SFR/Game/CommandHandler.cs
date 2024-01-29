using HarmonyLib;
using System;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using SFD;
using SFD.Core;
using SFD.States;
using SFD.Voting;
using SFD.ManageLists;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using CConst = SFDCT.Misc.Constants;

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
    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameInfo), nameof(GameInfo.HandleCommand), typeof(ProcessCommandArgs))]
    private static void HandleCommands(ProcessCommandArgs args, GameInfo __instance)
    {
        // Server commands.
        if (__instance.GameOwner == GameOwnerEnum.Client)
        {
            return;
        }

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
                return;
            }

            // List slot states
            if (CConst.HOST_GAME_SLOT_COUNT > 8 && args.IsCommand("SLOTS"))
            {
                for (int i = 0; i < CConst.HOST_GAME_SLOT_COUNT; i++)
                {
                    string mess = "- " + GetSlotState(__instance.GetGameSlotByIndex(i).GameSlotIndex, CConst.HOST_GAME_SLOT_STATES[i], (int)CConst.HOST_GAME_SLOT_TEAMS[i], __instance.GetGameSlotByIndex(i).IsOccupiedByUser);
                    args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, mess, Color.Yellow));
                }
                return;
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
                return;
            }
        }

        if (args.IsCommand("VOTEKICK", "VK") && args.Parameters.Count > 0)
        {
            if (!Settings.Values.GetBool("VOTE_KICKING_ENABLED"))
            {
                return;
            }
            if (GameSFD.Handle.CurrentState != State.Game || GameSFD.Handle.CurrentState == State.GameOffline)
            {
                return;
            }
            if (!Voting.GameVoteKick.CanBeCalled())
            {
                args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "Vote-kicking is in cooldown.", Color.Red, args.SenderGameUser, null));
                return;
            }
            if (__instance.VoteInfo.ActiveVotes.Count > 0)
            {
                args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "There is a vote already in progress.", Color.Red, args.SenderGameUser, null));
                return;
            }
            if (!(args.SenderGameUser.IsHost || args.SenderGameUser.IsModerator) && (DateTime.Now.Hour <= args.SenderGameUser.JoinTime.Hour && (DateTime.Now.Minute - args.SenderGameUser.JoinTime.Minute) <= 2))
            {
                args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "You must be atleast 2 minutes in the server before initiating a vote-kick.", Color.Red, args.SenderGameUser, null));
                return;
            }

            GameUser userTokick = __instance.GetGameUserByStringInput(args.Parameters[0], args.SenderGameUser);
            string voteReason = (args.Parameters.Count > 1 ? args.Parameters[1] : "");

            if (userTokick == null)
            {
                args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "Couldn't find user.", Color.Red, args.SenderGameUser, null));
                return;
            }
            if (userTokick.UserIdentifier == args.SenderGameUserIdentifier)
            {
                args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "You can't initiate a vote-kick against yourself.", Color.Red, args.SenderGameUser, null));
                return;
            }
            if (userTokick.IsBot)
            {
                args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "You can't initiate a vote-kick against bots.", Color.Red, args.SenderGameUser, null));
                return;
            }
            if (userTokick.IsHost || userTokick.IsModerator)
            {
                args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "You can't initiate a vote-kick against that user.", Color.Red, args.SenderGameUser, null));
                return;
            }

            Server server = GameSFD.Handle.Server;
            if (server != null)
            {
                GameVote gameVoteKick = new Voting.GameVoteKick(GameVote.GetNextVoteID(), voteReason, userTokick, args.SenderGameUser);
                gameVoteKick.ValidRemoteUniqueIdentifiers.AddRange(server.GetConnectedUniqueIdentifiers((NetConnection x) => x.GameConnectionTag() != null && x.GameConnectionTag().FirstGameUser != null && x.GameConnectionTag().FirstGameUser.CanVote));

                __instance.VoteInfo.AddVote(gameVoteKick);
                server.SendMessage(MessageType.GameVote, new Pair<GameVote, bool>(gameVoteKick, false));
            }
            return;
        }
    }
}