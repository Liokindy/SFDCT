using Lidgren.Network;
using Microsoft.Xna.Framework;
using SFD;
using SFD.ManageLists;
using SFDCT.Configuration;

namespace SFDCT.Voting;

internal class GameVoteKick : GameVoteYesNo
{
    internal static Color PrimaryMessageColor = Color.Yellow;
    internal static Color SecondaryMessageColor = PrimaryMessageColor * 0.6f;

    private static double m_nextAvailableVoteKickTimeStamp;

    private readonly string m_userNetAddressToKick;
    private readonly string m_userProfileNameToKick;
    private readonly string m_userAccountNameToKick;

    internal GameVoteKick(int voteID, string userProfileName, string userAccountName, string userNetAddress) : base(voteID, [string.Format("'{0}' ({1})", userProfileName, userAccountName)])
    {
        m_userNetAddressToKick = userNetAddress;
        m_userAccountNameToKick = userAccountName;
        m_userProfileNameToKick = userProfileName;
    }

    internal static bool CanStartVote()
    {
        return NetTime.Now >= m_nextAvailableVoteKickTimeStamp;
    }

    private static void SetVoteKickCooldown()
    {
        m_nextAvailableVoteKickTimeStamp = NetTime.Now + SFDCTConfig.Get<int>(CTSettingKey.VoteKickSuccessCooldown);
        ConsoleOutput.ShowMessage(ConsoleOutputType.Information, string.Format("Set vote-kicking cooldown to {0}, {1} seconds from now", m_nextAvailableVoteKickTimeStamp, m_nextAvailableVoteKickTimeStamp - NetTime.Now));
    }

    private static void ShowEndingChatMessage(GameInfo gameInfo)
    {
        gameInfo.ShowChatMessage(new NetMessage.ChatMessage.Data(LanguageHelper.GetText("sfdct.vote.kick.end"), PrimaryMessageColor));
    }

    public override void OnNo(GameInfo gameInfo)
    {
        SetVoteKickCooldown();

        ShowEndingChatMessage(gameInfo);
        gameInfo.ShowChatMessage(new NetMessage.ChatMessage.Data(LanguageHelper.GetText("sfdct.vote.kick.fail", m_userProfileNameToKick, m_userAccountNameToKick), SecondaryMessageColor));
    }

    public override void OnYes(GameInfo gameInfo)
    {
        SetVoteKickCooldown();
        GameUser userToKick = gameInfo.GetGameUserByAccount(m_userNetAddressToKick);
        if (userToKick == null || userToKick.IsDisposed)
        {
            KickList.Add(m_userNetAddressToKick, m_userProfileNameToKick, Constants.HOST_GAME_DEFAULT_KICK_DURATION_MINUTES);

            ShowEndingChatMessage(gameInfo);
            gameInfo.ShowChatMessage(new(LanguageHelper.GetText("sfdct.vote.kick.nouser", m_userProfileNameToKick, m_userAccountNameToKick)));
            return;
        }

        ShowEndingChatMessage(gameInfo);
        gameInfo.ShowChatMessage(new NetMessage.ChatMessage.Data(LanguageHelper.GetText("sfdct.vote.kick.success", m_userProfileNameToKick, m_userAccountNameToKick), SecondaryMessageColor));
        gameInfo.RunServerCommand("/KICK " + userToKick.UserIdentifier.ToString());
    }

    public override void OnTie(GameInfo gameInfo)
    {
        SetVoteKickCooldown();

        ShowEndingChatMessage(gameInfo);
        gameInfo.ShowChatMessage(new NetMessage.ChatMessage.Data(LanguageHelper.GetText("sfdct.vote.kick.fail", m_userProfileNameToKick, m_userAccountNameToKick), SecondaryMessageColor));
    }
}