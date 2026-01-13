using SFD;
using SFD.Voting;

namespace SFDCT.Voting;

internal class GameVoteYesNo : GameVote
{
    private bool m_resolved;

    internal GameVoteYesNo(int voteID, params string[] description) : base(voteID, (GameVote.Type)2)
    {
        DescriptionTextID = "button.vote";
        DescriptionParameters = description;

        Alternatives.Add(new(0, "general.yes"));
        Alternatives.Add(new(1, "general.no"));
    }

    internal static bool CanStartVote(GameInfo gameInfo)
    {
        if (gameInfo.InLobby) return false;
        if (gameInfo.VoteInfo == null) return false;
        if (gameInfo.VoteInfo.ActiveVotes.Count > 0) return false;

        return true;
    }

    private GameVoteAlternative GetWinningAlternative()
    {
        int highestVoteCount;
        int overHalfCount = ValidRemoteUniqueIdentifiers.Count / 2 + 1;
        GameVoteAlternative highestVoted = GetHighestVoteCount(out highestVoteCount);

        if (highestVoted != null && highestVoteCount >= overHalfCount)
        {
            return highestVoted;
        }

        return null;
    }

    public override void AnswersUpdated(GameInfo gameInfo)
    {
        if (ValidRemoteUniqueIdentifiers.Count == VotedRemoteUniqueIdentifiers.Count)
        {
            VoteTimeout(gameInfo);
            Remove();
            return;
        }

        GameVoteAlternative winnerAlternative = GetWinningAlternative();
        if (winnerAlternative != null)
        {
            VoteTimeout(gameInfo);
            Remove();
        }
    }

    public override void VoteTimeout(GameInfo gameInfo)
    {
        if (m_resolved) return;
        m_resolved = true;

        GameVoteAlternative winnerAlternative = GetWinningAlternative();
        if (winnerAlternative != null)
        {
            if (winnerAlternative.Index == 0)
            {
                OnYes(gameInfo);
            }
            else
            {
                OnNo(gameInfo);
            }
        }
        else
        {
            OnTie(gameInfo);
        }
    }

    public virtual void OnTie(GameInfo gameInfo) { }
    public virtual void OnNo(GameInfo gameInfo) { }
    public virtual void OnYes(GameInfo gameInfo) { }
}
