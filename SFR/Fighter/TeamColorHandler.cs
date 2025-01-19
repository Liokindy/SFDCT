using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SFD;
using SFD.Tiles;
using SFD.States;
using CSettings = SFDCT.Settings.Values;
using CGlobals = SFDCT.Misc.Globals;
using HarmonyLib;

namespace SFDCT.Fighter
{
    [HarmonyPatch]
    internal static class TeamColorHandler
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(StateLoading), nameof(StateLoading.Load))]
        private static void SetupTextures()
        {
            CGlobals.TEAM_5_ICON = Textures.TextureExist("TeamIcon5") ? Textures.GetTexture("TeamIcon5") : Textures.GetTexture("TeamIconS");
            CGlobals.TEAM_6_ICON = Textures.TextureExist("TeamIcon6") ? Textures.GetTexture("TeamIcon6") : Textures.GetTexture("TeamIcon0");
            CGlobals.TEAM_SPECTATOR_ICON = Textures.TextureExist("TeamIconSP") ? Textures.GetTexture("TeamIconSP") : Textures.GetTexture("TeamIcon0");

            SFD.GUI.Text.TextIcons.m_icons["TEAM_5"].m_texture = CGlobals.TEAM_5_ICON;
            SFD.GUI.Text.TextIcons.m_icons["TEAM_5"].m_textureOrigin = new Vector2(CGlobals.TEAM_5_ICON.Width * 0.5f, CGlobals.TEAM_5_ICON.Height * 0.5f);

            SFD.GUI.Text.TextIcons.Add("TEAM_6", CGlobals.TEAM_6_ICON);
            SFD.GUI.Text.TextIcons.Add("TEAM_" + CGlobals.TEAM_SPECTATOR_INDEX.ToString(), CGlobals.TEAM_SPECTATOR_ICON);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Constants), nameof(SFD.Constants.GetTeamIcon))]
        private static bool GetTeamIcon(ref Texture2D __result, Team team)
        {
            switch ((int)team)
            {
                case 5:
                    __result = CGlobals.TEAM_5_ICON;
                    return false;
                case 6:
                    __result = CGlobals.TEAM_6_ICON;
                    return false;
                case CGlobals.TEAM_SPECTATOR_INDEX:
                    __result = CGlobals.TEAM_SPECTATOR_ICON;
                    return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Constants), nameof(Constants.GetTeamColor), new Type[] { typeof(int) })]
        private static bool GetTeamColor(ref Color __result, int team)
        {
            bool useSFRTeams = CSettings.Get<bool>(CSettings.GetKey(CSettings.SettingKey.UseSfrColorsForTeam5Team6));

            switch (team)
            {
                case 5:
                    if (useSFRTeams)
                    {
                        __result = CGlobals.Colors.TEAM_SFR_5 * 2f;
                        return false;
                    }
                    break;
                case 6:
                    if (useSFRTeams)
                    {
                        __result = CGlobals.Colors.TEAM_SFR_6 * 2f;
                        return false;
                    }
                    else
                    {
                        __result = CGlobals.Colors.TEAM_6 * 2f;
                        return false;
                    }
                case CGlobals.TEAM_SPECTATOR_INDEX:
                    __result = CGlobals.Colors.TEAM_SPECTATOR * 2f;
                    return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Constants.COLORS), nameof(Constants.COLORS.GetTeamColor), new Type[] { typeof(TeamIcon), typeof(Constants.COLORS.TeamColorType) })]
        private static bool ConstantsColorsGetTeamColor(ref Color __result, TeamIcon team, Constants.COLORS.TeamColorType type = Constants.COLORS.TeamColorType.Default)
        {
            bool useSFRTeams = CSettings.Get<bool>(CSettings.GetKey(CSettings.SettingKey.UseSfrColorsForTeam5Team6));

            switch (type)
            {
                case Constants.COLORS.TeamColorType.Default:
                    switch ((int)team)
                    {
                        case 5:
                            if (useSFRTeams)
                            {
                                __result = CGlobals.Colors.TEAM_SFR_5;
                                return false;
                            }
                            break;
                        case 6:
                            if (useSFRTeams)
                            {
                                __result = CGlobals.Colors.TEAM_SFR_6;
                                return false;
                            }
                            else
                            {
                                __result = CGlobals.Colors.TEAM_6;
                                return false;
                            }
                        case CGlobals.TEAM_SPECTATOR_INDEX:
                            __result = CGlobals.Colors.TEAM_SPECTATOR;
                            return false;
                    }
                    break;
                case Constants.COLORS.TeamColorType.TeamMessage:
                    switch ((int)team)
                    {
                        case 5:
                            if (useSFRTeams)
                            {
                                __result = CGlobals.Colors.TEAM_SFR_5_CHAT_MESSAGE;
                                return false;
                            }
                            break;
                        case 6:
                            if (useSFRTeams)
                            {
                                __result = CGlobals.Colors.TEAM_SFR_6_CHAT_MESSAGE;
                                return false;
                            }
                            else
                            {
                                __result = CGlobals.Colors.TEAM_6_CHAT_MESSAGE;
                                return false;
                            }
                        case CGlobals.TEAM_SPECTATOR_INDEX:
                            __result = CGlobals.Colors.TEAM_SPECTATOR_CHAT_MESSAGE;
                            return false;
                    }
                    break;
                case Constants.COLORS.TeamColorType.ChatName:
                    switch ((int)team)
                    {
                        case 5:
                            if (useSFRTeams)
                            {
                                __result = CGlobals.Colors.TEAM_SFR_5_CHAT_NAME;
                                return false;
                            }
                            break;
                        case 6:
                            if (useSFRTeams)
                            {
                                __result = CGlobals.Colors.TEAM_SFR_6_CHAT_NAME;
                                return false;
                            }
                            else
                            {
                                __result = CGlobals.Colors.TEAM_6_CHAT_NAME;
                                return false;
                            }
                        case CGlobals.TEAM_SPECTATOR_INDEX:
                            __result = CGlobals.Colors.TEAM_SPECTATOR_CHAT_NAME;
                            return false;
                    }
                    break;
                case Constants.COLORS.TeamColorType.ChatTag:
                    switch ((int)team)
                    {
                        case 5:
                            if (useSFRTeams)
                            {
                                __result = CGlobals.Colors.TEAM_SFR_5_CHAT_TAG;
                                return false;
                            }
                            break;
                        case 6:
                            if (useSFRTeams)
                            {
                                __result = CGlobals.Colors.TEAM_SFR_6_CHAT_TAG;
                                return false;
                            }
                            else
                            {
                                __result = CGlobals.Colors.TEAM_6_CHAT_TAG;
                                return false;
                            }
                        case CGlobals.TEAM_SPECTATOR_INDEX:
                            __result = CGlobals.Colors.TEAM_SPECTATOR_CHAT_TAG;
                            return false;
                    }
                    break;
            }

            return true;
        }
    }
}
