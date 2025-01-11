using System.Linq;
using SFD;
using SFD.Voting;
using Microsoft.Xna.Framework;
using Lidgren.Network;

namespace SFDCT.Game.Voting;

internal class GameVoteManual : GameVote
{
    public GameUser VoteOwner;
    public long VoteOwnerID;
    public bool VoteHandled = false;
    public bool Public = true;
    public bool OverHalfEndsVote = true;

    // <Text id="vote.nextmap.alternative.official">{0}</Text>
    private const string EmptyTextID = "vote.nextmap.alternative.official";
    private const int TextMaxLength = 64;

    public GameVoteManual(int ID, GameUser ownerUser, string descriptionText, string[] alternativeTexts, bool publicResults = true, bool requireHalf = true) : base(ID, GameVote.Type.KickVote)
    {
        this.DescriptionTextID = EmptyTextID;

        if (descriptionText.Length > TextMaxLength)
        {
            descriptionText = descriptionText.Substring(0, TextMaxLength - 3) + "...";
        }
        this.DescriptionParameters = [descriptionText];

        this.TotalVoteTime = 25 * 1000;

        this.VoteOwner = ownerUser;
        this.VoteOwnerID = ownerUser.UserIdentifier;

        this.Public = publicResults;
        this.OverHalfEndsVote = requireHalf;


        sbyte aID = 0;
        foreach(string alternative in alternativeTexts)
        {
            if (aID >= 4) { break; }

            this.Alternatives.Add(new GameVoteAlternative(aID, EmptyTextID, alternative));
            aID++;
        }
    }

    public override void AnswersUpdated(GameInfo gameInfo)
    {
        // Logger.LogInfo($"VOTING: {this.VoteID} answer updated.");
        if (this.VotedRemoteUniqueIdentifiers.Count >= this.ValidRemoteUniqueIdentifiers.Count)
        {
            // Logger.LogInfo($"VOTING: {this.VoteID} all voted.");
            this.VoteTimeout(gameInfo);
            this.Remove();
            return;
        }

        if (OverHalfEndsVote)
        {
            int halfCount = this.ValidRemoteUniqueIdentifiers.Count / 2 + 1;
            if (this.VotedRemoteUniqueIdentifiers.Count >= halfCount)
            {
                GameVoteAlternative highestVoteCount = this.GetHighestVoteCount(out int voteCount);
                if (highestVoteCount != null && voteCount >= halfCount)
                {
                    // Logger.LogInfo($"VOTING: {this.VoteID} over half voted.");

                    this.VoteTimeout(gameInfo);
                    this.Remove();
                }
            }
        }
    }
    public override void VoteTimeout(GameInfo gameInfo)
    {
        // Logger.LogInfo($"VOTING: {this.VoteID} time out.");
        if (this.VoteHandled)
        {
            return;
        }
        this.VoteHandled = true;

        Server sv = GameSFD.Handle.Server;
        
        int votedCount = this.VotedRemoteUniqueIdentifiers.Count;
        int validCount = this.ValidRemoteUniqueIdentifiers.Count;

        Color resultsHeaderCol = Color.LightBlue;
        Color resultsAlterColor = resultsHeaderCol * 0.5f;
        Color resultsChoosenColor = resultsHeaderCol * 0.9f;

        if (!this.Public)
        {
            float mult = 0.7f;
            resultsHeaderCol *= mult;
            resultsAlterColor *= mult;
            resultsChoosenColor *= mult;
        }

        sbyte? highestAlternativeID = this.GetHighestVoteCount(out _)?.Index;
        NetConnection singleConn = this.Public ? null : this.VoteOwner.GetGameConnectionTag()?.NetConnection;

        // Header. "Vote (X/X): X"
        string headerText = $"Vote ({votedCount}/{validCount}): \"{this.DescriptionParameters[0]}\"";
        if (sv != null)
        {
            sv.SendMessage(MessageType.ChatMessage, new NetMessage.ChatMessage.Data(headerText, resultsHeaderCol), null, singleConn);
        }
        else
        {
            ChatMessage.Show(headerText, resultsHeaderCol);
        }
            
        // Results. "X - X"
        for(int i = 0; i < this.Alternatives.Count; i++)
        {
            GameVoteAlternative gvAlt = this.Alternatives[i];
            if (gvAlt == null) { continue; }

            bool isHighestAlternative = highestAlternativeID != null && highestAlternativeID == gvAlt.Index;
            Color resultCol = isHighestAlternative ? resultsChoosenColor : resultsAlterColor;

            int awnserVoteCount = this.Answers.Where(gvA => gvA != null && gvA.SelectedAlternativeIndex == gvAlt.Index).Count();
            string resultText = $"{awnserVoteCount} - \"{gvAlt.DescriptionParameters[0]}\"";
            if (sv != null)
            {
                sv.SendMessage(MessageType.ChatMessage, new NetMessage.ChatMessage.Data(resultText, resultCol), null, singleConn);
            }
            else
            {
                ChatMessage.Show(resultText, resultCol);
            }
        }

        // Vote (7/8): "Header?"
        // 2 - "Yes"
        // 1 - "No"
        // 4 - "Yes (Extra)"
    }
}
