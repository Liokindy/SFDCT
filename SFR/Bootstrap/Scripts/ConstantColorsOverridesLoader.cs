using HarmonyLib;
using Microsoft.Xna.Framework;
using MonoMod.Utils;
using SFD;
using SFD.Parser;
using SFD.Weapons;
using SFDCT.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static SFDCT.Bootstrap.Assets.ScriptsLoader;

namespace SFDCT.Bootstrap.Scripts
{
    [HarmonyPatch]
    internal static class ConstantColorsOverridesLoader
    {

        private static readonly List<string> AllowedConstantColors = new List<string>()
        {
            "SELECTION",
            "COMPONENT",
            "SELECTION_HIGHLIGHT",
            //"MENU_BLUE",
            "MENU_ORANGE",
            "DEATH_TEXT",
            "SUDDEN_DEATH",
            "TEAM_INDEPENDENT",
            "TEAM_1",
            "TEAM_2",
            "TEAM_3",
            "TEAM_4",
            //"TEAM_SERVER",
            "TEAM_ALLY_BLUE",
            "TEAM_ALLY_GREEN",
            "TEAM_ENEMY_RED",
            "TEAM_INDEPENDENT_CHAT_NAME",
            "TEAM_1_CHAT_NAME",
            "TEAM_2_CHAT_NAME",
            "TEAM_3_CHAT_NAME",
            "TEAM_4_CHAT_NAME",
            //"TEAM_SERVER_CHAT_NAME",
            "TEAM_1_CHAT_TAG",
            "TEAM_2_CHAT_TAG",
            "TEAM_3_CHAT_TAG",
            "TEAM_4_CHAT_TAG",
            //"TEAM_SERVER_CHAT_TAG",
            "CHAT_ALL_MESSAGE",
            "TEAM_1_CHAT_MESSAGE",
            "TEAM_2_CHAT_MESSAGE",
            "TEAM_3_CHAT_MESSAGE",
            "TEAM_4_CHAT_MESSAGE",
            //"TEAM_SERVER_CHAT_MESSAGE",
            "WHISPER_CHAT_TAG",
            "WHISPER_CHAT_NAME",
            "WHISPER_CHAT_MESSAGE",
            "PING_RED",
            "PING_YELLOW",
            "PING_GREEN",
            "LAZER",
            "LAZER_END_DOT",
            "LAZER_FULL_STRENGTH",
            "LIGHT_GRAY",
            "GRAY",
            "RED",
            "YELLOW",
            "LIFE_BAR",
            "LIFE_BAR_OVERHEALTH_A",
            "LIFE_BAR_OVERHEALTH_B",
            "ENERGY_BAR",
            "ARMOR_BAR",
            "STATUS_TEXT",
            "MODERATOR_MESSAGE",
            "PLAYER_CONNECTED",
            "SERVER_PREVIEW_CONNECTED",
            "PLAYER_LEFT_GAME",
            "PLAYER_DISCONNECTED",
            "EFFECT_BLOOD",
            "EFFECT_DUST",
            "EFFECT_SMOKE",
            "EFFECT_ACID",
            "EFFECT_DIRT",
            "FIRE_NODE_AIR",
            "FIRE_NODE_GROUND",
            "FIRE_NODE_FLAME_START",
            "FIRE_NODE_TRAIL_START",
            "FIRE_NODE_TRAIL_END",
            "EFFECT_WOOD",
            "EFFECT_CLOTH",
            "PLAYER_FLASH_LIGHT",
            "CHAT_ICON",
            "MENU_BLACK_ALPHA",
            "DAMAGE_FLASH_PLAYER",
            "STRENGTH_BOOST_PLAYER",
            "SPEED_BOOST_PLAYER",
            //"DAMAGE_FLASH_OBJECT",
        };

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Constants), nameof(Constants.Load))]
        private static void Load()
        {
            int index = 0;
            string scriptName = "";
            try
            {
                CTScript[] scripts = GetByType(CTScript.ScriptType.ConstantColorOverride);
                if (scripts == null)
                {
                    return;
                }

                foreach (CTScript script in scripts)
                {
                    scriptName = script.FileName;
                    foreach (string entrykey in script.Entries.Keys)
                    {
                        index = 1;
                        if (AllowedConstantColors.Contains(entrykey))
                        {
                            index = 2;
                            string entryValue = script.Entries[entrykey];
                            Color entryObject = Color.Magenta;
                            Constants.TryParseColor(entryValue, out entryObject);

                            switch(entrykey)
                            {
                                case "SELECTION":
                                    SFD.Constants.COLORS.SELECTION = entryObject;
                                    break;
                                case "COMPONENT":
                                    SFD.Constants.COLORS.COMPONENT = entryObject;
                                    break;
                                case "SELECTION_HIGHLIGHT":
                                    SFD.Constants.COLORS.SELECTION_HIGHLIGHT = entryObject;
                                    break;
                                //case "MENU_BLUE":
                                //    SFD.Constants.COLORS.MENU_BLUE = entryObject;
                                //    break;
                                case "MENU_ORANGE":
                                    SFD.Constants.COLORS.MENU_ORANGE = entryObject;
                                    break;
                                case "DEATH_TEXT":
                                    SFD.Constants.COLORS.DEATH_TEXT = entryObject;
                                    break;
                                case "SUDDEN_DEATH":
                                    SFD.Constants.COLORS.SUDDEN_DEATH = entryObject;
                                    break;
                                case "TEAM_INDEPENDENT":
                                    SFD.Constants.COLORS.TEAM_INDEPENDENT = entryObject;
                                    break;
                                case "TEAM_1":
                                    SFD.Constants.COLORS.TEAM_1 = entryObject;
                                    break;
                                case "TEAM_2":
                                    SFD.Constants.COLORS.TEAM_2 = entryObject;
                                    break;
                                case "TEAM_3":
                                    SFD.Constants.COLORS.TEAM_3 = entryObject;
                                    break;
                                case "TEAM_4":
                                    SFD.Constants.COLORS.TEAM_4 = entryObject;
                                    break;
                                //case "TEAM_SERVER":
                                //    SFD.Constants.COLORS.TEAM_SERVER = entryObject;
                                //    break;
                                case "TEAM_ALLY_BLUE":
                                    SFD.Constants.COLORS.TEAM_ALLY_BLUE = entryObject;
                                    break;
                                case "TEAM_ALLY_GREEN":
                                    SFD.Constants.COLORS.TEAM_ALLY_GREEN = entryObject;
                                    break;
                                case "TEAM_ENEMY_RED":
                                    SFD.Constants.COLORS.TEAM_ENEMY_RED = entryObject;
                                    break;
                                case "TEAM_INDEPENDENT_CHAT_NAME":
                                    SFD.Constants.COLORS.TEAM_INDEPENDENT_CHAT_NAME = entryObject;
                                    break;
                                case "TEAM_1_CHAT_NAME":
                                    SFD.Constants.COLORS.TEAM_1_CHAT_NAME = entryObject;
                                    break;
                                case "TEAM_2_CHAT_NAME":
                                    SFD.Constants.COLORS.TEAM_2_CHAT_NAME = entryObject;
                                    break;
                                case "TEAM_3_CHAT_NAME":
                                    SFD.Constants.COLORS.TEAM_3_CHAT_NAME = entryObject;
                                    break;
                                case "TEAM_4_CHAT_NAME":
                                    SFD.Constants.COLORS.TEAM_4_CHAT_NAME = entryObject;
                                    break;
                                //case "TEAM_SERVER_CHAT_NAME":
                                //    SFD.Constants.COLORS.TEAM_SERVER_CHAT_NAME = entryObject;
                                //    break;
                                case "TEAM_1_CHAT_TAG":
                                    SFD.Constants.COLORS.TEAM_1_CHAT_TAG = entryObject;
                                    break;
                                case "TEAM_2_CHAT_TAG":
                                    SFD.Constants.COLORS.TEAM_2_CHAT_TAG = entryObject;
                                    break;
                                case "TEAM_3_CHAT_TAG":
                                    SFD.Constants.COLORS.TEAM_3_CHAT_TAG = entryObject;
                                    break;
                                case "TEAM_4_CHAT_TAG":
                                    SFD.Constants.COLORS.TEAM_4_CHAT_TAG = entryObject;
                                    break;
                                //case "TEAM_SERVER_CHAT_TAG":
                                //    SFD.Constants.COLORS.TEAM_SERVER_CHAT_TAG = entryObject;
                                //    break;
                                case "CHAT_ALL_MESSAGE":
                                    SFD.Constants.COLORS.CHAT_ALL_MESSAGE = entryObject;
                                    break;
                                case "TEAM_1_CHAT_MESSAGE":
                                    SFD.Constants.COLORS.TEAM_1_CHAT_MESSAGE = entryObject;
                                    break;
                                case "TEAM_2_CHAT_MESSAGE":
                                    SFD.Constants.COLORS.TEAM_2_CHAT_MESSAGE = entryObject;
                                    break;
                                case "TEAM_3_CHAT_MESSAGE":
                                    SFD.Constants.COLORS.TEAM_3_CHAT_MESSAGE = entryObject;
                                    break;
                                case "TEAM_4_CHAT_MESSAGE":
                                    SFD.Constants.COLORS.TEAM_4_CHAT_MESSAGE = entryObject;
                                    break;
                                //case "TEAM_SERVER_CHAT_MESSAGE":
                                //    SFD.Constants.COLORS.TEAM_SERVER_CHAT_MESSAGE = entryObject;
                                //    break;
                                case "WHISPER_CHAT_TAG":
                                    SFD.Constants.COLORS.WHISPER_CHAT_TAG = entryObject;
                                    break;
                                case "WHISPER_CHAT_NAME":
                                    SFD.Constants.COLORS.WHISPER_CHAT_NAME = entryObject;
                                    break;
                                case "WHISPER_CHAT_MESSAGE":
                                    SFD.Constants.COLORS.WHISPER_CHAT_MESSAGE = entryObject;
                                    break;
                                case "PING_RED":
                                    SFD.Constants.COLORS.PING_RED = entryObject;
                                    break;
                                case "PING_YELLOW":
                                    SFD.Constants.COLORS.PING_YELLOW = entryObject;
                                    break;
                                case "PING_GREEN":
                                    SFD.Constants.COLORS.PING_GREEN = entryObject;
                                    break;
                                case "LAZER":
                                    SFD.Constants.COLORS.LAZER = entryObject;
                                    break;
                                case "LAZER_END_DOT":
                                    SFD.Constants.COLORS.LAZER_END_DOT = entryObject;
                                    break;
                                case "LAZER_FULL_STRENGTH":
                                    SFD.Constants.COLORS.LAZER_FULL_STRENGTH = entryObject;
                                    break;
                                case "LIGHT_GRAY":
                                    SFD.Constants.COLORS.LIGHT_GRAY = entryObject;
                                    break;
                                case "GRAY":
                                    SFD.Constants.COLORS.GRAY = entryObject;
                                    break;
                                case "RED":
                                    SFD.Constants.COLORS.RED = entryObject;
                                    break;
                                case "YELLOW":
                                    SFD.Constants.COLORS.YELLOW = entryObject;
                                    break;
                                case "LIFE_BAR":
                                    SFD.Constants.COLORS.LIFE_BAR = entryObject;
                                    break;
                                case "LIFE_BAR_OVERHEALTH_A":
                                    SFD.Constants.COLORS.LIFE_BAR_OVERHEALTH_A = entryObject;
                                    break;
                                case "LIFE_BAR_OVERHEALTH_B":
                                    SFD.Constants.COLORS.LIFE_BAR_OVERHEALTH_B = entryObject;
                                    break;
                                case "ENERGY_BAR":
                                    SFD.Constants.COLORS.ENERGY_BAR = entryObject;
                                    break;
                                case "ARMOR_BAR":
                                    SFD.Constants.COLORS.ARMOR_BAR = entryObject;
                                    break;
                                case "STATUS_TEXT":
                                    SFD.Constants.COLORS.STATUS_TEXT = entryObject;
                                    break;
                                case "MODERATOR_MESSAGE":
                                    SFD.Constants.COLORS.MODERATOR_MESSAGE = entryObject;
                                    break;
                                case "PLAYER_CONNECTED":
                                    SFD.Constants.COLORS.PLAYER_CONNECTED = entryObject;
                                    break;
                                case "SERVER_PREVIEW_CONNECTED":
                                    SFD.Constants.COLORS.SERVER_PREVIEW_CONNECTED = entryObject;
                                    break;
                                case "PLAYER_LEFT_GAME":
                                    SFD.Constants.COLORS.PLAYER_LEFT_GAME = entryObject;
                                    break;
                                case "PLAYER_DISCONNECTED":
                                    SFD.Constants.COLORS.PLAYER_DISCONNECTED = entryObject;
                                    break;
                                case "EFFECT_BLOOD":
                                    SFD.Constants.COLORS.EFFECT_BLOOD = entryObject;
                                    break;
                                case "EFFECT_DUST":
                                    SFD.Constants.COLORS.EFFECT_DUST = entryObject;
                                    break;
                                case "EFFECT_SMOKE":
                                    SFD.Constants.COLORS.EFFECT_SMOKE = entryObject;
                                    break;
                                case "EFFECT_ACID":
                                    SFD.Constants.COLORS.EFFECT_ACID = entryObject;
                                    break;
                                case "EFFECT_DIRT":
                                    SFD.Constants.COLORS.EFFECT_DIRT = entryObject;
                                    break;
                                case "FIRE_NODE_AIR":
                                    SFD.Constants.COLORS.FIRE_NODE_AIR = entryObject;
                                    break;
                                case "FIRE_NODE_GROUND":
                                    SFD.Constants.COLORS.FIRE_NODE_GROUND = entryObject;
                                    break;
                                case "FIRE_NODE_FLAME_START":
                                    SFD.Constants.COLORS.FIRE_NODE_FLAME_START = entryObject;
                                    break;
                                case "FIRE_NODE_TRAIL_START":
                                    SFD.Constants.COLORS.FIRE_NODE_TRAIL_START = entryObject;
                                    break;
                                case "FIRE_NODE_TRAIL_END":
                                    SFD.Constants.COLORS.FIRE_NODE_TRAIL_END = entryObject;
                                    break;
                                case "EFFECT_WOOD":
                                    SFD.Constants.COLORS.EFFECT_WOOD = entryObject;
                                    break;
                                case "EFFECT_CLOTH":
                                    SFD.Constants.COLORS.EFFECT_CLOTH = entryObject;
                                    break;
                                case "PLAYER_FLASH_LIGHT":
                                    SFD.Constants.COLORS.PLAYER_FLASH_LIGHT = entryObject;
                                    break;
                                case "CHAT_ICON":
                                    SFD.Constants.COLORS.CHAT_ICON = entryObject;
                                    break;
                                case "MENU_BLACK_ALPHA":
                                    SFD.Constants.COLORS.MENU_BLACK_ALPHA = entryObject;
                                    break;
                                case "DAMAGE_FLASH_PLAYER":
                                    SFD.Constants.COLORS.DAMAGE_FLASH_PLAYER = entryObject;
                                    break;
                                case "STRENGTH_BOOST_PLAYER":
                                    SFD.Constants.COLORS.STRENGTH_BOOST_PLAYER = entryObject;
                                    break;
                                case "SPEED_BOOST_PLAYER":
                                    SFD.Constants.COLORS.SPEED_BOOST_PLAYER = entryObject;
                                    break;
                                //case "DAMAGE_FLASH_OBJECT":
                                //    SFD.Constants.COLORS.DAMAGE_FLASH_OBJECT = entryObject;
                                //    break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"CONSTANT COLORS OVERRIDES LOADER: Failed to handle overrides at {index} in script '{scriptName}'!");
                Logger.LogError(ex.ToString());
            }
        }
    }
}
