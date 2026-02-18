using HarmonyLib;
using Microsoft.Xna.Framework;
using SFD;
using SFD.Code.MenuControls;
using SFD.GameKeyboard;
using SFD.MenuControls;
using SFD.States;
using SFDCT.Misc;
using SFDCT.Sync;
using SFDCT.Sync.Data;
using SFDCT.UI.Panels;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace SFDCT.UI;

[HarmonyPatch]
internal static class MenuHandler
{
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(GameSFD), nameof(GameSFD.DrawInner))]
    private static IEnumerable<CodeInstruction> GameSFD_DrawInner_Transpiler_VersionLabel(IEnumerable<CodeInstruction> instructions)
    {
        foreach (var instruction in instructions)
        {
            if (instruction.opcode == OpCodes.Ldstr && instruction.operand?.Equals(Constants.VERSION) == true)
            {
                instruction.operand = $"{Constants.VERSION} - {Globals.Version.SFDCT}";
            }
        }

        return instructions;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(MainMenuPanel), MethodType.Constructor)]
    private static void MainMenuPanel_Constructor_Postfix_InsertSFDCTOption(MainMenuPanel __instance)
    {
        var menu = __instance.menu;
        var sfdctMenuItem = new MainMenuItem("SFDCT", new ControlEvents.ChooseEvent((object _) =>
        {
            __instance.OpenSubPanel(new SFDCTSettingsPanel());
        }));

        sfdctMenuItem.Initialize(menu);

        __instance.Height += 1;

        menu.Height += 1;
        menu.Items.Insert(Math.Max(menu.Items.Count - 2, 0), sfdctMenuItem);

        __instance.UpdatePosition();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameMenuPanel), MethodType.Constructor)]
    private static void GameMenuPanel_Constructor_Postfix_InsertExtraOptions(GameMenuPanel __instance)
    {
        var menu = (Menu)__instance.members[0];
        var sfdctMenuItem = new MainMenuItem("SFDCT", new ControlEvents.ChooseEvent((object _) =>
        {
            __instance.OpenSubPanel(new SFDCTSettingsPanel());
        }));

        sfdctMenuItem.Initialize(menu);

        menu.Height += 1;
        menu.Items.Insert(menu.Items.Count - 1, sfdctMenuItem);

        menu.NeighborUpId = 5;
        menu.NeighborDownId = 4;

        int[] neighborUpMap = { 2, 3, 4, 0, 6, 7, 8, 1 };
        int[] neighborDownMap = { 8, 1, 2, 3, 0, 5, 6, 7 };

        for (int i = 0; i < 8; i++)
        {
            var playerSlot = new MainMenuPlayerSlot(new Vector2(10, 100), __instance, i, i >= 2);
            playerSlot.SetProfile(Profile.GetPlayerProfile(i));

            __instance.members.Add(playerSlot);

            playerSlot.NeighborUpId = neighborUpMap[i];
            playerSlot.NeighborDownId = neighborDownMap[i];
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameMenuPanel), nameof(GameMenuPanel.Update))]
    private static void GameMenuPanel_Update_Postfix_ProfilePlayerSlotPosition(GameMenuPanel __instance, float elapsed)
    {
        for (int i = 0; i < __instance.members.Count; i++)
        {
            if (__instance.members[i] is MainMenuPlayerSlot playerSlot)
            {
                playerSlot.Update(elapsed);
            }
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameMenuPanel), nameof(GameMenuPanel.UpdatePosition))]
    private static void GameMenuPanel_UpdatePosition_Postfix_ProfilePlayerSlotPosition(GameMenuPanel __instance)
    {
        // The scoreboard panel gets in the way of the PlayerSlots
        // (since it isn't there in the main menu)

        for (int i = 0; i < __instance.members.Count; i++)
        {
            if (__instance.members[i] is MainMenuPlayerSlot playerSlot)
            {
                int x = -__instance.Area.X + 8;
                int y = -__instance.Area.Y;

                if (i <= 4)
                {
                    y = -__instance.Area.Y + GameSFD.SCREEN_HEIGHT - GameSFD.GAME_SCREEN_OFFSET_Y * 2 - 100 - 58 * (i - 1);
                }
                else
                {
                    y = -__instance.Area.Y + GameSFD.GAME_SCREEN_OFFSET_Y * 2 + 8 + (58 * 4) - 58 * (i - 4);
                }

                playerSlot.LocalPosition = new(x, y);
            }
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(MainMenuPlayerSlot), nameof(MainMenuPlayerSlot.ProfilePanel_SelectProfile))]
    private static bool MainMenuPlayerSlot_ProfilePanel_SelectProfile_Prefix_ParentPanelRefreshFix(MainMenuPlayerSlot __instance, Profile selectedProfile, int profileSlot)
    {
        // This line in the original method causes the game
        // to crash if the ParentPanel is not a MainMenuPanel
        // > (this.ParentPanel as MainMenuPanel).RefreshProfiles();

        if (selectedProfile == null) return false;

        Constants.PLAYER_PROFILE[__instance.PlayerIndex] = profileSlot;
        __instance.SetProfile(selectedProfile);

        if (__instance.ParentPanel is MainMenuPanel mainMenuPanel)
        {
            mainMenuPanel.RefreshProfiles();
        }
        else
        {
            if (GameSFD.Handle.CurrentState == State.GameOffline)
            {
                var gameInfo = StateGameOffline.GameInfo;
                var gameUser = gameInfo.GetGameUserByUserIdentifier(gameInfo.GetLocalGameUserIdentifier(__instance.PlayerIndex));

                gameUser.Profile = selectedProfile;
                gameUser.Profile.Updated = true;
            }
            else if (GameSFD.Handle.CurrentState == State.Game)
            {
                if (GameSFD.Handle.Client != null)
                {
                    SFDCTMessageData data = new()
                    {
                        Type = MessageHandler.SFDCTMessageDataType.ProfileChangeRequest,
                        Data = [
                            __instance.PlayerIndex,
                            selectedProfile,
                        ]
                    };

                    MessageHandler.Send(GameSFD.Handle.Client, data);
                }
            }
        }

        return false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(KeyBindPanel), MethodType.Constructor)]
    private static void KeyBindPanel_Constructor_Postfix_8PlayerKeyBinds(KeyBindPanel __instance)
    {
        // Add all the elements of player 5-8 keybinds, and then
        // shift the new keybind elements back (before the misc keys).
        // This way uses the original code and saves a lot of hassle.

        var originalElementCount = __instance.menu.Items.Count;
        var currentKeyBinds = new KeyBindPanel.PlayerKeyBindItems[8];
        for (int i = 0; i < 8; i++)
        {
            if (i < __instance.playerKeyBindings.Length)
            {
                currentKeyBinds[i] = __instance.playerKeyBindings[i];
            }
            else
            {
                var keyBind = new KeyBindPanel.PlayerKeyBindItems(i + 1);

                keyBind.SetupControls(__instance.menu, __instance, true);
                currentKeyBinds[i] = keyBind;
            }
        }

        __instance.playerKeyBindings = currentKeyBinds;

        var currentElementCount = __instance.menu.Items.Count;
        var miscElementCount = 1 + __instance.miscKeys.Length + 3; // Separator + Misc Keys + Empty Separator + OK + CANCEL

        var newKeyBindElements = __instance.menu.Items.GetRange(originalElementCount, currentElementCount - originalElementCount);
        __instance.menu.Items.RemoveRange(originalElementCount, currentElementCount - originalElementCount);
        __instance.menu.Items.InsertRange(originalElementCount - miscElementCount, newKeyBindElements);

        __instance.UpdateGamePadTexts();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(KeyBindPanel), nameof(KeyBindPanel.keyBindPanel_OK))]
    private static void KeyBindPanel_keyBindPanel_OK_Postfix_8PlayerKeyBinds()
    {
        for (int i = 5; i < 9; i++)
        {
            VirtualKeyboard.BindedKeys[i].Setup();
        }
    }
}
