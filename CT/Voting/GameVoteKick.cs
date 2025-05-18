using Microsoft.Xna.Framework;
using Lidgren.Network;
using SFDCT.Configuration;
using SFD;
using SFD.ManageLists;
using SFD.Voting;

namespace SFDCT.Voting;

internal class GameVoteKick : GameVote
{
    public GameVoteKick(int voteID, GameUser gameUserToKick) : base(voteID, GameVote.Type.KickVote)
    {
        this.m_userAddressToKick = gameUserToKick.GetNetIP();
        this.m_userAccountNameToKick = gameUserToKick.AccountName;
        this.m_userProfileNameToKick = gameUserToKick.GetProfileName();

        this.DescriptionTextID = "button.vote";
        this.DescriptionParameters = new string[1];
        this.DescriptionParameters[0] = string.Format("'{0}' ({1})", gameUserToKick.GetProfileName(), gameUserToKick.AccountName);

        this.Alternatives.Add(new GameVoteAlternative(0, "general.yes"));
        this.Alternatives.Add(new GameVoteAlternative(1, "general.no"));
        m_alternativeYesOption = 0;
    }

    public override void AnswersUpdated(GameInfo gameInfo)
    {
        if (this.ValidRemoteUniqueIdentifiers.Count == this.VotedRemoteUniqueIdentifiers.Count)
        {
            this.VoteTimeout(gameInfo);
            this.Remove();
            return;
        }
    }

    public override void VoteTimeout(GameInfo gameInfo)
    {
        if (this.m_voteHandled) return; 
        this.m_voteHandled = true;
        
        string messKick = "- The majority has voted 'yes', kicking '{0}' ({1})...";
        string messNoVotes = "- Not enough votes, '{0}' ({1}) will not be kicked";
        string messNoKick = "- The majority has voted 'no', '{0}' ({1}) will not be kicked";

        gameInfo.ShowChatMessage(new NetMessage.ChatMessage.Data("VOTE-KICK ENDED", GameVoteKick.PRIMARY_MESSAGE_COLOR));

        int num = (int)(this.ValidRemoteUniqueIdentifiers.Count * VOTE_MINIMUM_PERCENTAGE) + 1;
        if (this.VotedRemoteUniqueIdentifiers.Count >= num)
        {
            int num2 = 2;
            GameVoteAlternative highestAlternative = this.GetHighestVoteCount(out num2);
            if (highestAlternative != null && num >= num2)
            {
                if (highestAlternative.Index == m_alternativeYesOption)
                {
                    gameInfo.ShowChatMessage(new NetMessage.ChatMessage.Data(string.Format(messKick, m_userProfileNameToKick, m_userAccountNameToKick), GameVoteKick.SECONDARY_MESSAGE_COLOR));
                    m_nextAvailableVoteKickTimeStamp = NetTime.Now + Settings.Get<int>(SettingKey.VoteKickSuccessCooldown);

                    KickList.Add(m_userAddressToKick, m_userProfileNameToKick, Constants.HOST_GAME_DEFAULT_KICK_DURATION_MINUTES);
                    GameUser userToKick = gameInfo.GetGameUserByIP(m_userAddressToKick);

                    if (userToKick != null && !userToKick.IsDisposed)
                    {
                        gameInfo.RunServerCommand("/KICK " + userToKick.UserIdentifier.ToString());
                    }
                }
                else
                {
                    m_nextAvailableVoteKickTimeStamp = NetTime.Now + Settings.Get<int>(SettingKey.VoteKickFailCooldown);
                    gameInfo.ShowChatMessage(new NetMessage.ChatMessage.Data(string.Format(messNoKick, m_userProfileNameToKick, m_userAccountNameToKick), GameVoteKick.SECONDARY_MESSAGE_COLOR));
                }
            }
        }
        else
        {
            m_nextAvailableVoteKickTimeStamp = NetTime.Now + Settings.Get<int>(SettingKey.VoteKickFailCooldown);
            gameInfo.ShowChatMessage(new NetMessage.ChatMessage.Data(string.Format(messNoVotes, m_userProfileNameToKick, m_userAccountNameToKick), GameVoteKick.SECONDARY_MESSAGE_COLOR));
        }

        ConsoleOutput.ShowMessage(ConsoleOutputType.Information, string.Format("Set vote-kicking cooldown to {0}, {1} seconds from now", m_nextAvailableVoteKickTimeStamp, m_nextAvailableVoteKickTimeStamp - NetTime.Now));
    }

    public static float VOTE_MINIMUM_PERCENTAGE { get { return 0.60f; } }
    public static Color PRIMARY_MESSAGE_COLOR { get { return Color.Yellow; } }
    public static Color SECONDARY_MESSAGE_COLOR { get { return PRIMARY_MESSAGE_COLOR * 0.6f; } }

    public static bool CanVoteKick { get { return NetTime.Now >= m_nextAvailableVoteKickTimeStamp; } }
    private static double m_nextAvailableVoteKickTimeStamp = 0;

    private bool m_voteHandled;
    private readonly sbyte m_alternativeYesOption;
    private readonly string m_userAddressToKick;
    private readonly string m_userProfileNameToKick;
    private readonly string m_userAccountNameToKick;
}