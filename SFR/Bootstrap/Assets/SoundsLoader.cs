using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Microsoft.Xna.Framework.Audio;
using SFD.Core;
using SFD.Parser;
using SFD.Sounds;
using SFD;
using HarmonyLib;
using CGlobals = SFDCT.Misc.Globals;
using CIni = SFDCT.Settings.Values;

namespace SFDCT.Bootstrap.Assets
{
    [HarmonyPatch]
    internal static class SoundsLoader
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SFD.Sounds.SoundHandler), nameof(SFD.Sounds.SoundHandler.Load))]
        internal static bool Load(GameSFD game)
        {
            if (!CIni.GetBool("USE_1_4_0_ASSETS"))
            {
                return true;
            }

            ConsoleOutput.ShowMessage(ConsoleOutputType.Loading, "Loading sounds...");
            SoundHandler.game = game;
            SoundHandler.m_recentlyPlayedSoundClassPool = new GenericClassPool<SoundHandler.RecentlyPlayedSound>(() => new SoundHandler.RecentlyPlayedSound(), 1, 0);
            List<SoundHandler.RecentlyPlayedSound> list = new List<SoundHandler.RecentlyPlayedSound>();
            for (int i = 0; i < 30; i++)
            {
                list.Add(SoundHandler.m_recentlyPlayedSoundClassPool.GetFreeItem());
            }
            foreach (SoundHandler.RecentlyPlayedSound recentlyPlayedSound in list)
            {
                recentlyPlayedSound.InUse = false;
                SoundHandler.m_recentlyPlayedSoundClassPool.FlagFreeItem(recentlyPlayedSound);
            }
            SoundHandler.soundEffects = new SoundHandler.SoundEffectGroups();
            string contentFullPath = Path.GetFullPath(Path.Combine("SFDCT\\Content", "Data\\Sounds\\"));
            if (!Directory.Exists(contentFullPath))
            {
                SoundHandler.m_soundsDisabled = true;
                SFD.Program.LogErrorMessage("No sound folder found", new Exception(string.Format("Folder '{0}' could not be found. Sound will be disabled.", new object[0])));
                return false;
            }
            string[] files = Directory.GetFiles(contentFullPath, "*.sfds");
            foreach (string text in files)
            {
                ConsoleOutput.ShowMessage(ConsoleOutputType.Loading, "Loading sounds: " + text);
                foreach (string text2 in SFDSimpleReader.Read(text))
                {
                    string[] array2 = SFDSimpleReader.Interpret(text2).ToArray();
                    if (array2.Length >= 3)
                    {
                        SoundEffect[] array3 = new SoundEffect[array2.Length - 2];
                        float num = 1f;
                        try
                        {
                            num = SFDXParser.ParseFloat(array2[1]);
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(string.Concat(new string[]
                            {
                        "Error: Could not parse volume modifier in line \n'",
                        text2,
                        "'\n in file '",
                        text,
                        "'\r\n",
                        ex.ToString()
                            }));
                        }
                        for (int k = 0; k < array3.Length; k++)
                        {
                            string loadPath = Constants.GetLoadPath(Path.Combine("SFDCT\\Content", "Data\\Sounds\\", array2[k + 2]));
                            try
                            {
                                array3[k] = Content.Load<SoundEffect>(loadPath);
                            }
                            catch (NoAudioHardwareException)
                            {
                                if (!SoundHandler.m_soundsDisabled)
                                {
                                    SoundHandler.m_soundsDisabled = true;
                                    ConsoleOutput.ShowMessage(ConsoleOutputType.Loading, "Loading sounds aborted - no hardware or drivers");
                                    MessageBox.Show("No audio hardware or drivers detected. Sounds will be disabled.", "No audio hardware or drivers!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                }
                                return false;
                            }
                            catch (Exception)
                            {
                                ConsoleOutput.ShowMessage(ConsoleOutputType.Error, string.Concat(new string[]
                                {
                            "Error: Could not load sound '",
                            loadPath,
                            "' in line '",
                            text2,
                            "' in file '",
                            text
                                }));
                            }
                        }
                        int num2 = 0;
                        for (int l = 0; l < array3.Length; l++)
                        {
                            if (array3[l] == null)
                            {
                                num2++;
                            }
                        }
                        if (num2 == array3.Length)
                        {
                            ConsoleOutput.ShowMessage(ConsoleOutputType.Error, "GroupSoundEffects contains only null elements - skipping sound group " + array2[0]);
                        }
                        else
                        {
                            if (num2 > 0)
                            {
                                SoundEffect[] array4 = new SoundEffect[array3.Length - num2];
                                int num3 = 0;
                                for (int m = 0; m < array3.Length; m++)
                                {
                                    if (array3[m] != null)
                                    {
                                        array4[num3] = array3[m];
                                        num3++;
                                    }
                                }
                                array3 = array4;
                            }
                            SoundHandler.SoundEffectGroup soundEffectGroup = new SoundHandler.SoundEffectGroup(array2[0], num, array3);
                            if (soundEffectGroup.IsValid)
                            {
                                if (soundEffectGroup.Key == "CHAINSAW")
                                {
                                    SoundHandler.GlobalLoopSounds.Chainsaw = new SoundHandler.GlobalLoopSound(soundEffectGroup.Key, array3[0].CreateInstance(), num);
                                }
                                if (soundEffectGroup.Key == "STREETSWEEPERPROPELLER")
                                {
                                    SoundHandler.GlobalLoopSounds.StreetsweeperPropeller = new SoundHandler.GlobalLoopSound(soundEffectGroup.Key, array3[0].CreateInstance(), num);
                                }
                                SoundHandler.soundEffects.Add(soundEffectGroup);
                                ConsoleOutput.ShowMessage(ConsoleOutputType.Loading, "Sound added: " + array2[0]);
                            }
                        }
                    }
                }
            }
            ConsoleOutput.ShowMessage(ConsoleOutputType.Loading, "Loading sounds finilizing");
            if (files.Length == 0 || SoundHandler.soundEffects.Count == 0)
            {
                SoundHandler.m_soundsDisabled = true;
                return false;
            }
            ConsoleOutput.ShowMessage(ConsoleOutputType.Loading, "Loading sounds completed");

            return false;
        }
    }
}
