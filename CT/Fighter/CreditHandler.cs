using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace SFDCT.Fighter;

internal static class CreditHandler
{
    private static readonly Dictionary<string, Color> CreditColors = new()
    {
        {"204624521",  new Color(201, 255, 234)}, // Liokindy, main dev
        {"1144477755", new Color(203, 255, 201)}, // ElDou's¹, many ideas
        {"912907660",  new Color(255, 231, 201)}, // Nult, ideas

        // - SFDCT wouldn't exist without SFR
        {"913199347", new Color(242, 201, 255)}, // Odex, SFR's main dev
    };

    internal static bool IsCredit(string accountID)
    {
        return !string.IsNullOrEmpty(accountID) && CreditColors.ContainsKey(accountID.Substring(1));
    }

    internal static Color GetCreditColor(string accountID)
    {
        if (!IsCredit(accountID))
        {
            return Color.White;
        }

        return CreditColors[accountID.Substring(1)];
    }
}
