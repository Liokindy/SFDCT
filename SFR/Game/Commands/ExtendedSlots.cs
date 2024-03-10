using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFD;
using HarmonyLib;
using SFD.MenuControls;
using CConst = SFDCT.Misc.Constants;

namespace SFDCT.Game.Commands;

internal static class ExtendedSlots
{
    public static string SlotToString(GameSlot slot)
    {
        string team = slot.CurrentTeam.ToString();
        if (slot.CurrentTeam != slot.NextTeam)
        {
            team = slot.CurrentTeam + "->" + slot.NextTeam;
        }
        team.Replace("Independent", "None");

        string state = "Closed";
        if (slot.IsOpen)
        {
            state = "Opened";
        }

        if (slot.IsOccupiedByBot)
        {
            state = string.Format("Bot ({0})", slot.GameUser.BotDifficutlyLevel);
        }
        else if (slot.IsOccupiedByUser)
        {
            state = string.Format("User ('{0}')", slot.GameUser.AccountName);
        }

        return string.Format("{0} - ({1})", state, team);
    }

    public static byte GetSlotStateByStringInput(string input)
    {
        return input.ToUpper() switch
        {
            "1" or "OPENED" => 1,
            "2" or "NORMAL" => 2,
            "4" or "EASY" => 4,
            "5" or "HARD" => 5,
            "6" or "EXPERT" => 6,
            "0" or "CLOSED" or _ => 0,
        };
    }
    public static Team GetSlotTeamByStringInput(string input)
    {
        return input.ToUpper() switch
        {
            "1" or "TEAM1" or "T1" => Team.Team1,
            "2" or "TEAM2" or "T2" => Team.Team2,
            "3" or "TEAM3" or "T3" => Team.Team3,
            "4" or "TEAM4" or "T4" => Team.Team4,
            "0" or "NONE" or "INDEPENDENT" or "TI" or _ => Team.Independent,
        };
    }
}
