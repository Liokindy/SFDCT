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
}