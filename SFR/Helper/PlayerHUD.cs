using SFD;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SFDCT.Misc;

namespace SFDCT.Helper;

internal static class PlayerHUD
{
    private static Color m_colorBlackAlpha = SFD.Constants.COLORS.MENU_BLACK_ALPHA;
    private static Color m_colorSideTop = new Color(32, 32, 32, 32);
    
    public static void DrawBar(SpriteBatch sb, Color barColor, float fullness, int x = 0, int y = 0, int width = 184, int height = 16, bool drawBg = true, bool drawShine = true)
    {
        int num = (int)(width * fullness);
        if (num > 0)
        {
            // Bar inner
            Rectangle destinationRectangle = new Rectangle(x, y, num, height);
            sb.Draw(SFD.Constants.WhitePixel, destinationRectangle, barColor);
        }
        if (num < width && drawBg)
        {
            // Bar background
            Rectangle destinationRectangle2 = new Rectangle(x + num, y, width - num, height);
            sb.Draw(SFD.Constants.WhitePixel, destinationRectangle2, m_colorBlackAlpha);
        }
        if (drawShine)
        {
            // Left and top white lines
            sb.Draw(SFD.Constants.WhitePixel, new Rectangle(x, y, width, 2), m_colorSideTop);
            sb.Draw(SFD.Constants.WhitePixel, new Rectangle(x, y, 2, height), m_colorSideTop);
        }
    }

    public static Color GetPlayerTeamOutlineColor(Player player)
    {
        if (SFD.Constants.TEAM_DISPLAY_MODE == TeamDisplayMode.TeamColors || GameInfo.LocalPlayerCount >= 2 || GameSFD.Handle.CurrentState == SFD.States.State.MainMenu)
        {
            switch(player.CurrentTeam)
            {
                case Team.Team1: return SFDCT.Misc.Constants.Colors.Outline_Team_1;
                case Team.Team2: return SFDCT.Misc.Constants.Colors.Outline_Team_2;
                case Team.Team3: return SFDCT.Misc.Constants.Colors.Outline_Team_3;
                case Team.Team4: return SFDCT.Misc.Constants.Colors.Outline_Team_4;
            };
            return SFDCT.Misc.Constants.Colors.Outline_Team_Independent;
        }
        else
        {
            bool isEnemy = player.GameWorld.GUI_TeamDisplay_LocalGameUserIdentifier != player.m_userIdentifier && SFD.Constants.IsEnemyTeams(player.GameWorld.GUI_TeamDisplay_LocalGameUserTeam, player.CurrentTeam);
            switch(SFD.Constants.TEAM_DISPLAY_MODE) 
            {
                case TeamDisplayMode.GreenRed: return (isEnemy ? SFDCT.Misc.Constants.Colors.Outline_Team_EnemyRed : SFDCT.Misc.Constants.Colors.Outline_Team_AllyGreen);
                case TeamDisplayMode.BlueRed: return (isEnemy ? SFDCT.Misc.Constants.Colors.Outline_Team_EnemyRed : SFDCT.Misc.Constants.Colors.Outline_Team_AllyBlue);
            };
            return SFDCT.Misc.Constants.Colors.Outline_Team_Independent;
        }
    }
}