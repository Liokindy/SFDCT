using System;
using SFD;
using SFD.MenuControls;
using Microsoft.Xna.Framework;
using HarmonyLib;
using CConst = SFDCT.Misc.Constants;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;

namespace SFDCT.UI;

[HarmonyPatch]
internal static class Scoreboard
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ScoreboardPanel), nameof(ScoreboardPanel.Update))]
    private static void Update(ScoreboardPanel __instance, float elapsed)
    {
        if (!CConst.HOST_GAME_EXTENDED_SLOTS)
        {
            return;
        }

        GameInfo lobbyGameInfo = LobbyCommandHandler.GetLobbyGameInfo();
        if (lobbyGameInfo != null && !lobbyGameInfo.IsDisposed)
        {
            for (int i = 8; i < CConst.SLOTCOUNT; i++)
            {
                __instance.m_slotsRows[i].Update(elapsed, lobbyGameInfo, __instance.m_forceUpdateLabels);
            }
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ScoreboardPanel), nameof(ScoreboardPanel.FillPlayerList))]
    private static bool FillPlayerList(ScoreboardPanel __instance)
    {
        if (!CConst.HOST_GAME_EXTENDED_SLOTS)
        {
            return true;
        }

        int StatusWidth = 150 - 18;
        int PortraitWidth = 50; // Constant
        int TeamWidth = 90 - 16;
        int LatencyWidth = 50 - 4;
        int WinsWidth = 75 - 25 - 4;
        int LossesWidth = WinsWidth;
        int LoadingProgressWidth = 50 - 4;
        int ScoreboardWidth = StatusWidth + PortraitWidth + TeamWidth + LatencyWidth + WinsWidth + LossesWidth + LoadingProgressWidth;

        __instance.Height = 568 - 50 * 8 + 50 * (int)Math.Ceiling(CConst.SLOTCOUNT * 0.5f);
        __instance.Width = ScoreboardWidth * 2;

        __instance.m_slotsRows = new LobbySlotRow[CConst.SLOTCOUNT];
        int num = 0;
        int num2 = 120;
        LobbySlotHeaderColumn lobbySlotHeaderColumn = new LobbySlotHeaderColumn(LanguageHelper.GetText("menu.scoreboard.header.status"), new Vector2((float)num, (float)num2), StatusWidth, __instance);
        num += lobbySlotHeaderColumn.Width;
        __instance.m_headers.Add(lobbySlotHeaderColumn);

        lobbySlotHeaderColumn = new LobbySlotHeaderColumn("-", new Vector2((float)num, (float)num2), PortraitWidth, __instance);
        num += lobbySlotHeaderColumn.Width;
        __instance.m_headers.Add(lobbySlotHeaderColumn);

        lobbySlotHeaderColumn = new LobbySlotHeaderColumn(LanguageHelper.GetText("menu.scoreboard.header.team"), new Vector2((float)num, (float)num2), TeamWidth, __instance);
        num += lobbySlotHeaderColumn.Width;
        __instance.m_headers.Add(lobbySlotHeaderColumn);

        lobbySlotHeaderColumn = new LobbySlotHeaderColumn(LanguageHelper.GetText("menu.scoreboard.header.ping"), new Vector2((float)num, (float)num2), LatencyWidth, __instance);
        num += lobbySlotHeaderColumn.Width;
        __instance.m_headers.Add(lobbySlotHeaderColumn);

        lobbySlotHeaderColumn = new LobbySlotHeaderColumn(LanguageHelper.GetText("menu.scoreboard.header.wins"), new Vector2((float)num, (float)num2), WinsWidth, __instance);
        num += lobbySlotHeaderColumn.Width;
        __instance.m_headers.Add(lobbySlotHeaderColumn);

        __instance.m_headerLosses = new LobbySlotHeaderColumn(LanguageHelper.GetText("menu.scoreboard.header.losses"), new Vector2((float)num, (float)num2), LossesWidth, __instance);
        num += __instance.m_headerLosses.Width;
        __instance.m_headers.Add(__instance.m_headerLosses);

        lobbySlotHeaderColumn = new LobbySlotHeaderColumn("-", new Vector2((float)num, (float)num2), LoadingProgressWidth, __instance);
        __instance.m_headers.Add(lobbySlotHeaderColumn);

        bool flag = GameSFD.Handle.Server != null || LobbyCommandHandler.CheckCanConsumeCommand(null);
        int xx = 0;
        int yy = num2 + 28;

        for (int i = 0; i < __instance.m_slotsRows.Length; i++)
        {
            int num3 = xx;
            int num4 = yy;

            xx += ScoreboardWidth;
            if (xx > ScoreboardWidth)
            {
                xx = 0;
                yy += 50;
            }

            LobbySlotStatus lobbySlotStatus = new LobbySlotStatus(new Vector2(num3, num4), StatusWidth, __instance);
            __instance.members.Add(lobbySlotStatus);
            num3 += lobbySlotStatus.Width;
            LobbySlotPortrait lobbySlotPortrait = new LobbySlotPortrait(new Vector2(num3, num4), __instance);
            __instance.members.Add(lobbySlotPortrait);
            num3 += lobbySlotPortrait.Width;
            LobbySlotTeam lobbySlotTeam = new LobbySlotTeam(new Vector2(num3, num4), TeamWidth, __instance, true, flag);
            __instance.members.Add(lobbySlotTeam);
            num3 += lobbySlotTeam.Width;
            LobbySlotLatency lobbySlotLatency = new LobbySlotLatency(new Vector2(num3, num4), LatencyWidth, __instance);
            __instance.members.Add(lobbySlotLatency);
            num3 += lobbySlotLatency.Width;
            LobbySlotWinsLosses lobbySlotWinsLosses = new LobbySlotWinsLosses(new Vector2(num3, num4), WinsWidth, __instance);
            __instance.members.Add(lobbySlotWinsLosses);
            num3 += lobbySlotWinsLosses.Width;
            LobbySlotWinsLosses lobbySlotWinsLosses2 = new LobbySlotWinsLosses(new Vector2(num3, num4), LossesWidth, __instance);
            __instance.members.Add(lobbySlotWinsLosses2);
            num3 += lobbySlotWinsLosses2.Width;
            LobbySlotLoadingProgress lobbySlotLoadingProgress = new LobbySlotLoadingProgress(new Vector2(num3, num4), LoadingProgressWidth, __instance);
            __instance.members.Add(lobbySlotLoadingProgress);

            __instance.m_slotsRows[i] = new LobbySlotRow(i, flag, lobbySlotStatus, lobbySlotPortrait, lobbySlotTeam, lobbySlotLatency, lobbySlotWinsLosses, lobbySlotWinsLosses2, lobbySlotLoadingProgress);
        }

        // NeighborIDs
        int membersPerRow = 7 * 2;
        for (int i = 0; i < __instance.members.Count; i++)
        {
            __instance.members[i].NeighborLeftId = -1;
            __instance.members[i].NeighborUpId = -1;
            __instance.members[i].NeighborRightId = -1;
            __instance.members[i].NeighborDownId = -1;

            // Left
            if (__instance.members.ElementAtOrDefault(i - 1) != null)
            {
                __instance.members[i].NeighborLeftId = i - 1;
            }
            // Right
            if (__instance.members.ElementAtOrDefault(i + 1) != null)
            {
                __instance.members[i].NeighborRightId = i + 1;
            }
            // Down
            if (__instance.members.ElementAtOrDefault(i + membersPerRow) != null)
            {
                __instance.members[i].NeighborDownId = i + membersPerRow;
            }
            // Up
            if (__instance.members.ElementAtOrDefault(i - membersPerRow) != null)
            {
                __instance.members[i].NeighborUpId = i - membersPerRow;
            }

            // Left/Right Edges
            if (i % membersPerRow == 0)
            {
                __instance.members[i].NeighborLeftId = -1;
            }
            if ((i + 1) % membersPerRow == 0)
            {
                __instance.members[i].NeighborRightId = -1;
            }
        }

        return false;
    }
}
