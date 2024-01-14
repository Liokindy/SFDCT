using System;
using System.Collections.Generic;
using System.Linq;
using SFD.ManageLists;
using SFD.Voting;
using SFD;
using Microsoft.Xna.Framework;

namespace SFDCT.Game.Voting;

/// <summary>
///     Vote-kick a user in-game with a reason
///     if the majority of the server agrees, or
///     the user leaves early.
/// </summary>
public class GameVoteKick : SFD.Voting.GameVote
{
    private GameUser m_userToKick;
    private GameUser m_votekickOriginUser;
    private string m_voteReason;
    private bool m_voteHandled;

    public static DateTime m_lastVoteKickTime;
    public static bool CanBeCalled()
    {
        if ((DateTime.Now - m_lastVoteKickTime).TotalMinutes < Settings.Values.GetInt("VOTE_KICKING_COOLDOWN_MINUTES"))
        {
            return false;
        }
        return true;
    }
    public GameVoteKick(int voteID, string votekickReason, GameUser gameUserToKick, GameUser votekickOwner) : base(voteID, Type.KickVote)
    {
        m_userToKick = gameUserToKick;
        m_votekickOriginUser = votekickOwner;

        m_voteReason = votekickReason;
        if (m_voteReason.Length > 24)
        {
            m_voteReason = m_voteReason.Substring(0, 24) + "...";
        }

        if (string.IsNullOrEmpty(m_voteReason))
        {
            m_voteReason = "No reason";
        }

        GameVoteKick.m_lastVoteKickTime = DateTime.Now;
        this.TotalVoteTime = Settings.Values.GetInt("VOTE_KICKING_DURATION_SECONDS") * 1000;
        this.DescriptionTextID = "vote.nextmap.alternative.official";
        this.DescriptionParameters = new string[]
        {
            $"Vote-kicking \"{gameUserToKick.GetProfileName()}\" ({gameUserToKick.AccountName}) - Reason: {m_voteReason}"
        };

        this.Alternatives.Add(new GameVoteAlternative(0, "general.yes", new string[0]));
        this.Alternatives.Add(new GameVoteAlternative(1, "general.no", new string[0]));

        // gameInfo.ShowChatMessage(new($"\"{m_votekickOriginUser.GetProfileName()}\" ({m_votekickOriginUser.AccountName}) has initiated a vote-kick against \"{m_userToKick.GetProfileName()}\" ({m_userToKick.AccountName}).", Color.Yellow));
    }
    private void VoteKickUserToKick()
    {
        if (m_userToKick == null)
        {
            return;
        }

        GameConnectionTag connTag = m_userToKick.GetGameConnectionTag();
        if (connTag != null)
        {
            if (connTag.NetConnection != null)
            {
                KickList.Add(connTag.NetConnection, connTag.GetFirstGameUserProfileName(), SFD.Constants.HOST_GAME_DEFAULT_KICK_DURATION_MINUTES);
                connTag.NetConnection.Disconnect("connection.failed.kicked");
            }
        }
    }
    private void CheckUserToKickDisconnected(GameInfo gameInfo)
    {
        if (gameInfo.GetGameUserByIP(m_userToKick.GetNetIP()) == null)
        {
            gameInfo.ShowChatMessage(new($"\"{m_userToKick.GetProfileName()} ({m_userToKick.AccountName})\" left before the vote-kick ended.", Color.Yellow));
            VoteKickUserToKick(); // Add to the kick list

            m_voteHandled = true;
            this.VoteTimeout(gameInfo);
            this.Remove();
        }
    }
    private int GetAgreeCount()
    {
        return this.Answers.Where(vote => vote.UserID != m_userToKick.GetGameConnectionTag().RemoteUniqueIdentifier && (int)vote.SelectedAlternativeIndex == 0).Count();
    }
    private int GetVotedCount()
    {
        return this.VotedRemoteUniqueIdentifiers.Where(id => id != m_userToKick.GetGameConnectionTag().RemoteUniqueIdentifier).Count();
    }
    private int GetValidCount()
    {
        return this.ValidRemoteUniqueIdentifiers.Where(id => id != m_userToKick.GetGameConnectionTag().RemoteUniqueIdentifier).Count();
    }
    public override void AnswersUpdated(GameInfo gameInfo)
    {
        CheckUserToKickDisconnected(gameInfo);

        // All players in the server voted.
        if (GetValidCount() == GetVotedCount() || GetVotedCount() >= GetValidCount() / 2 + 1)
        {
            this.VoteTimeout(gameInfo);
            this.Remove();
            return;
        }
    }
    public override void VoteTimeout(GameInfo gameInfo)
    {
        if (this.m_voteHandled)
        {
            return;
        }
        this.m_voteHandled = true;

        if (GetVotedCount() == 0)
        {
            gameInfo.ShowChatMessage(new($"\"{m_userToKick.GetProfileName()}\" ({m_userToKick.AccountName}) was not kicked. (Nobody voted)", Color.Red));
            return;
        }

        // Kick the player if more than half the server agrees
        if (GetAgreeCount() >= (GetValidCount() / 2) + 1)
        {
            gameInfo.ShowChatMessage(new($"Kicking \"{m_userToKick.GetProfileName()} ({m_userToKick.AccountName})\". Reason: {m_voteReason}. (Majority agreed)", Color.ForestGreen));
            VoteKickUserToKick();
        }
        else
        {
            gameInfo.ShowChatMessage(new($"\"{m_userToKick.GetProfileName()}\" ({m_userToKick.AccountName}) was not kicked. (Not enough votes in favor)", Color.Red));
        }

        this.Remove();
    }
}