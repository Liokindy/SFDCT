using HarmonyLib;
using Microsoft.Xna.Framework.Input;
using SFD;
using SFD.GameKeyboard;
using System.IO;
using System.Threading;
using System;

namespace SFDCT.Game;

[HarmonyPatch]
internal static class GameHandler
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(SFDConfig), nameof(SFDConfig.SaveConfig))]
    private static void SFDConfigSaveConfig(SFDConfigSaveMode mode)
    {
        lock (SFDConfig.m_saveConfigLock)
        {
            if (mode == (SFDConfigSaveMode)10 || mode == SFDConfigSaveMode.All)
            {
                SFDConfig.ConfigHandler.UpdateValue("MODERATOR_COMMANDS", string.Join(" ", Constants.MODDERATOR_COMMANDS));
            }

            string configPath = Constants.Paths.CustomConfig;
            if (string.IsNullOrWhiteSpace(configPath)) configPath = Constants.Paths.GetPath(Constants.Paths.UserDocumentsSFDUserDataPath, "config.ini");
            
            if (!File.Exists(configPath))
            {
                try
                {
                    using (FileStream fileStream = File.Create(configPath))
                    {
                        fileStream.Close();
                    }
                    Thread.Sleep(50);
                }
                catch (UnauthorizedAccessException)
                {
                    MessageStack.Show("Error creating config.ini - Access denied", MessageStackType.Error);
                    return;
                }
                catch (Exception ex)
                {
                    MessageStack.Show("Error creating config.ini - " + ex.Message, MessageStackType.Error);
                    return;
                }
            }

            try
            {
                SFDConfig.ConfigHandler.SaveFile(configPath);
            }
            catch (UnauthorizedAccessException)
            {
                MessageStack.Show("Error saving config.ini - Access denied", MessageStackType.Error);
            }
            catch (Exception ex2)
            {
                MessageStack.Show("Error saving config.ini - " + ex2.Message, MessageStackType.Error);
            }
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameSFD), nameof(GameSFD.InitializeKeys))]
    private static void InitializeKeys()
    {
        GameSFD.m_keyVotes = [
            Keys.F1,
            Keys.F2,
            Keys.F3,
            Keys.F4,
            Keys.F5,
            Keys.F6,
            Keys.F7,
            Keys.F8,
        ];
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(VirtualKeyboard), nameof(VirtualKeyboard.LoadDefaultKeys))]
    private static void LoadDefaultKeys()
    {
        VirtualKeyboard.VOTE_MISC_KEYS =
        [
            41,
            42,
            43,
            44,
            45,
            46,
            47,
            48,
        ];
    }
}
