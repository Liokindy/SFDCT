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

    private readonly int m_userIdentifier;
    private readonly string m_userNetAddress;
    private readonly string m_userProfileName;
    private readonly string m_userAccountName;

    internal GameVoteKick(int voteID, GameUser userToKick) : base(voteID, string.Format("'{0}' ({1})", userToKick.GetProfileName(), userToKick.AccountName))
    {
        m_userIdentifier = userToKick.UserIdentifier;
        m_userNetAddress = userToKick.GetNetIP();
        m_userAccountName = userToKick.AccountName;
        m_userProfileName = userToKick.GetProfileName();
    }

    internal static bool CanStartVoteKick(GameInfo gameInfo)
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
        gameInfo.ShowChatMessage(new NetMessage.ChatMessage.Data(LanguageHelper.GetText("sfdct.vote.kick.fail", m_userProfileName, m_userAccountName), SecondaryMessageColor));
    }

    public override void OnYes(GameInfo gameInfo)
    {
        SetVoteKickCooldown();

        GameUser userToKick = gameInfo.GetGameUserByUserIdentifier(m_userIdentifier);
        if (userToKick == null || userToKick.IsDisposed)
        {
            KickList.Add(m_userNetAddress, m_userProfileName, Constants.HOST_GAME_DEFAULT_KICK_DURATION_MINUTES);

            ShowEndingChatMessage(gameInfo);
            gameInfo.ShowChatMessage(new(LanguageHelper.GetText("sfdct.vote.kick.nouser", m_userProfileName, m_userAccountName)));
            return;
        }

        ShowEndingChatMessage(gameInfo);
        gameInfo.ShowChatMessage(new NetMessage.ChatMessage.Data(LanguageHelper.GetText("sfdct.vote.kick.success", m_userProfileName, m_userAccountName), SecondaryMessageColor));
        gameInfo.RunServerCommand("/KICK_USER " + m_userIdentifier);
    }

    public override void OnTie(GameInfo gameInfo)
    {
        SetVoteKickCooldown();

        ShowEndingChatMessage(gameInfo);
        gameInfo.ShowChatMessage(new NetMessage.ChatMessage.Data(LanguageHelper.GetText("sfdct.vote.kick.fail", m_userProfileName, m_userAccountName), SecondaryMessageColor));
    }
}