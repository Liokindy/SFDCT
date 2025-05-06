using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SFD;

namespace SFDCT.Fighter;

internal static class CreditHandler
{
    private static readonly Dictionary<string, Color> CreditColors = new()
    {
        {"204624521", new Color(255, 255, 255)}, // Liokindy, main dev
        {"000000000", new Color(255, 255, 255)}, // ElDou's1, friend
        {"000000000", new Color(255, 255, 255)}, // Nult, friend
        {"000000000", new Color(255, 255, 255)}, // Odex, SFR's main dev
    };

    internal static Color? GetCreditColor(string accountID)
    {
        string accountIDWithoutS = accountID.Substring(1);
        
        if (!CreditColors.ContainsKey(accountIDWithoutS))
        {
            return null;
        }

        return CreditColors[accountIDWithoutS];
    }
}
