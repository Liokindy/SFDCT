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
using SFDCT.Helper;

namespace SFDCT.Game;

[HarmonyPatch]
internal static class CommandHandler
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameInfo), nameof(GameInfo.HandleCommand), typeof(ProcessCommandArgs))]
    private static void HandleCommands(ProcessCommandArgs args, GameInfo __instance)
    {
        // Only server or local can handle commands.
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
                args.Feedback.Add( new ProcessCommandMessage(args.SenderGameUser, mess) );
                return;
            }

            // Enable/disable vote-kicking
            if (args.IsCommand("VOTEKICKING") && args.Parameters.Count > 0)
            {
                bool value = Settings.Values.GetBool("VOTE_KICKING_ENABLED");
                if (args.Parameters[0] == "1" || args.Parameters[0].ToUpperInvariant() == "TRUE")
                {
                    value = true;
                }
                if (args.Parameters[0] == "0" || args.Parameters[0].ToUpperInvariant() == "FALSE")
                {
                    value = false;
                }

                if (value != Settings.Values.GetBool("VOTE_KICKING_ENABLED"))
                {
                    Settings.Values.SetSetting("VOTE_KICKING_ENABLED", value);

                    string mess = "Vote-kicking is now " + (value ? "enabled" : "disabled");
                    args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, mess));
                }
                return;
            }
        }

        if (args.IsCommand("VOTEKICK", "VK") && args.Parameters.Count > 0)
        {
            if (GameSFD.Handle.CurrentState != State.Game || GameSFD.Handle.CurrentState == State.GameOffline)
            {
                args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "You can't start a vote-kick right now.", Color.Red, args.SenderGameUser, null));
                return;
            }
            if ( !Settings.Values.GetBool("VOTE_KICKING_ENABLED") )
            {
                args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "Vote-kicks are disabled.", Color.Red, args.SenderGameUser, null));
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