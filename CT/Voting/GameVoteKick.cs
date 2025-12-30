using Lidgren.Network;
using Microsoft.Xna.Framework;
using SFD;
using SFD.ManageLists;
using SFD.Voting;
using SFDCT.Configuration;

namespace SFDCT.Voting;

internal class GameVoteKick : GameVote
{
    internal GameVoteKick(int voteID, GameUser gameUserToKick) : base(voteID, GameVote.Type.KickVote)
    {
        m_userNetAddressToKick = gameUserToKick.GetNetIP();
        m_userAccountNameToKick = gameUserToKick.AccountName;
        m_userProfileNameToKick = gameUserToKick.GetProfileName();

        DescriptionTextID = "button.vote";
        DescriptionParameters = [string.Format("'{0}' ({1})", gameUserToKick.GetProfileName(), gameUserToKick.AccountName)];

        Alternatives.Add(new GameVoteAlternative(0, "general.yes"));
        Alternatives.Add(new GameVoteAlternative(1, "general.no"));
    }

    public override void AnswersUpdated(GameInfo gameInfo)
    {
        int successVoteCount = ValidRemoteUniqueIdentifiers.Count / 2 + 1;

        if (VotedRemoteUniqueIdentifiers.Count == ValidRemoteUniqueIdentifiers.Count || VotedRemoteUniqueIdentifiers.Count > successVoteCount)
        {
            VoteTimeout(gameInfo);
            Remove();
            return;
        }
    }

    public override void VoteTimeout(GameInfo gameInfo)
    {
        if (m_voteHandled) return;
        m_voteHandled = true;

        Color primaryMessageColor = Color.Yellow;
        Color secondaryMessageColor = primaryMessageColor * 0.6f;

        gameInfo.ShowChatMessage(new NetMessage.ChatMessage.Data(LanguageHelper.GetText("sfdct.vote.kick.end"), primaryMessageColor));

        GameUser userToKick = gameInfo.GetGameUserByIP(m_userNetAddressToKick);
        if (userToKick == null || userToKick.IsDisposed)
        {
            gameInfo.ShowChatMessage(new(LanguageHelper.GetText("sfdct.vote.kick.nouser", m_userProfileNameToKick, m_userAccountNameToKick)));
            KickList.Add(m_userNetAddressToKick, m_userProfileNameToKick, Constants.HOST_GAME_DEFAULT_KICK_DURATION_MINUTES);
            return;
        }

        int successVoteCount = ValidRemoteUniqueIdentifiers.Count / 2 + 1;

        if (VotedRemoteUniqueIdentifiers.Count >= successVoteCount)
        {
            int voteCount = 2;
            GameVoteAlternative highestAlternative = GetHighestVoteCount(out voteCount);

            if (highestAlternative != null)
            {
                if (highestAlternative.Index == 0)
                {
                    gameInfo.ShowChatMessage(new NetMessage.ChatMessage.Data(LanguageHelper.GetText("sfdct.vote.kick.success", m_userProfileNameToKick, m_userAccountNameToKick), secondaryMessageColor));
                    m_nextAvailableVoteKickTimeStamp = NetTime.Now + SFDCTConfig.Get<int>(CTSettingKey.VoteKickSuccessCooldown);

                    gameInfo.RunServerCommand("/KICK " + userToKick.UserIdentifier.ToString());
                }
                else
                {
                    m_nextAvailableVoteKickTimeStamp = NetTime.Now + SFDCTConfig.Get<int>(CTSettingKey.VoteKickFailCooldown);
                    gameInfo.ShowChatMessage(new NetMessage.ChatMessage.Data(LanguageHelper.GetText("sfdct.vote.kick.fail", m_userProfileNameToKick, m_userAccountNameToKick), secondaryMessageColor));
                }
            }
        }
        else
        {
            m_nextAvailableVoteKickTimeStamp = NetTime.Now + SFDCTConfig.Get<int>(CTSettingKey.VoteKickFailCooldown);
            gameInfo.ShowChatMessage(new NetMessage.ChatMessage.Data(LanguageHelper.GetText("sfdct.vote.kick.fail", m_userProfileNameToKick, m_userAccountNameToKick), secondaryMessageColor));
        }

        ConsoleOutput.ShowMessage(ConsoleOutputType.Information, string.Format("Set vote-kicking cooldown to {0}, {1} seconds from now", m_nextAvailableVoteKickTimeStamp, m_nextAvailableVoteKickTimeStamp - NetTime.Now));
    }

    internal static bool CanVoteKick { get { return NetTime.Now >= m_nextAvailableVoteKickTimeStamp; } }
    private static double m_nextAvailableVoteKickTimeStamp;

    private bool m_voteHandled;
    private readonly string m_userNetAddressToKick;
    private readonly string m_userProfileNameToKick;
    private readonly string m_userAccountNameToKick;
}