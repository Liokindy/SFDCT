using Microsoft.Xna.Framework.Audio;
using SFD;
using SFD.Code;
using SFD.Core;
using SFD.Parser;
using SFD.Sounds;
using SFDCT.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace SFDCT.Assets;

internal static class SoundLoader
{
    internal static List<SoundHandler.SoundEffectGroup> LoadSoundsFromSFDSFile(string filePath, string soundsFolderPath)
    {
        List<string> fileLines = SFDSimpleReader.Read(filePath);
        List<SoundHandler.SoundEffectGroup> result = [];

        foreach (var line in fileLines)
        {
            string[] lineBits = SFDSimpleReader.Interpret(line).ToArray();
            if (lineBits.Length < 3) continue;

            List<SoundEffect> soundVariations = [];
            float soundVolume = 1;

            try
            {
                soundVolume = SFDXParser.ParseFloat(lineBits[1]);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error: Could not parse volume modifier in line \n'{line}'\n in file '{filePath}'\r\n{ex}");
            }

            for (int i = 0; i < lineBits.Length - 2; i++)
            {
                string soundVariationPath = Path.Combine(soundsFolderPath, lineBits[i + 2]);

                SoundEffect sound = null;

                try
                {
                    sound = Content.Load<SoundEffect>(soundVariationPath);
                }
                catch (NoAudioHardwareException)
                {
                    if (!SoundHandler.m_soundsDisabled)
                    {
                        SoundHandler.m_soundsDisabled = true;
                        ConsoleOutput.ShowMessage(ConsoleOutputType.Loading, "Loading sounds aborted - no hardware or drivers");
                        if (!SFD.Program.AutoStart)
                        {
                            MessageBox.Show("No audio hardware or drivers detected. Sounds will be disabled.", "No audio hardware or drivers!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        }
                    }

                    return null;
                }
                catch (Exception)
                {
                    ConsoleOutput.ShowMessage(ConsoleOutputType.Error, $"Error: Could not load sound '{soundVariationPath}' in line '{line}' in file '{filePath}");
                    sound = null;
                }

                if (sound != null)
                {
                    soundVariations.Add(sound);
                }
            }

            if (soundVariations.Count == 0)
            {
                ConsoleOutput.ShowMessage(ConsoleOutputType.Error, $"GroupSoundEffects contains only null elements - skipping sound group {lineBits[0]}");
            }
            else
            {
                SoundHandler.SoundEffectGroup soundEffectGroup = new SoundHandler.SoundEffectGroup(lineBits[0], soundVolume, soundVariations.ToArray());

                result.Add(soundEffectGroup);
            }
        }

        return result;
    }

    internal static void Load()
    {
        Dictionary<string, SoundHandler.SoundEffectGroup> sounds = [];

        string officialSoundsPath = Path.Combine(Constants.Paths.ContentPath, Constants.Paths.DATA_SOUNDS);

        if (!Directory.Exists(officialSoundsPath))
        {
            SoundHandler.m_soundsDisabled = true;
            SFD.Program.LogErrorMessage("No sound folder found", new Exception($"Folder '{officialSoundsPath}' could not be found. Sound will be disabled."));
            return;
        }

        Logger.LogInfo("LOADING [SOUNDS]: Official");
        foreach (var filePath in Directory.GetFiles(officialSoundsPath, "*.sfds"))
        {
            List<SoundHandler.SoundEffectGroup> officialSoundGroupList = LoadSoundsFromSFDSFile(filePath, officialSoundsPath);

            foreach (var soundGroup in officialSoundGroupList)
            {
                if (sounds.ContainsKey(soundGroup.Key))
                {
                    throw new Exception($"Error: Invalid sound key '{soundGroup.Key}' - it's already taken");
                }
                else
                {
                    sounds.Add(soundGroup.Key, soundGroup);
                }
            }
        }

        string documentsSoundsPath = Path.Combine(Constants.Paths.UserDocumentsContentCustomPath, Constants.Paths.DATA_SOUNDS);
        if (Directory.Exists(documentsSoundsPath))
        {
            Logger.LogInfo("LOADING [SOUNDS]: Documents");
            foreach (var filePath in Directory.GetFiles(documentsSoundsPath, "*.sfds"))
            {
                List<SoundHandler.SoundEffectGroup> documentsSoundGroupList = LoadSoundsFromSFDSFile(filePath, documentsSoundsPath);

                foreach (var soundGroup in documentsSoundGroupList)
                {
                    SubContentHandler.AddOrSetDictionaryValue(sounds, soundGroup.Key, soundGroup);
                }
            }
        }

        foreach (var subContentFolder in SubContentHandler.Folders)
        {
            string subContentSoundsPath = SubContentHandler.GetPath(subContentFolder, Constants.Paths.DATA_SOUNDS);

            if (Directory.Exists(subContentSoundsPath))
            {
                Logger.LogInfo($"LOADING [SOUNDS]: {subContentFolder}");

                foreach (var filePath in Directory.GetFiles(subContentSoundsPath, "*.sfds"))
                {
                    List<SoundHandler.SoundEffectGroup> subContentSoundGroupList = LoadSoundsFromSFDSFile(filePath, subContentSoundsPath);

                    foreach (var soundGroup in subContentSoundGroupList)
                    {
                        SubContentHandler.AddOrSetDictionaryValue(sounds, soundGroup.Key, soundGroup);
                    }
                }
            }
        }

        SoundHandler.game = GameSFD.Handle;
        SoundHandler.m_recentlyPlayedSoundClassPool = new GenericClassPool<SoundHandler.RecentlyPlayedSound>(() => new SoundHandler.RecentlyPlayedSound(), 1, 0);
        SoundHandler.soundEffects = new SoundHandler.SoundEffectGroups();

        List<SoundHandler.RecentlyPlayedSound> list = [];
        for (int i = 0; i < 30; i++)
        {
            list.Add(SoundHandler.m_recentlyPlayedSoundClassPool.GetFreeItem());
        }

        foreach (SoundHandler.RecentlyPlayedSound recentlyPlayedSound in list)
        {
            recentlyPlayedSound.InUse = false;
            SoundHandler.m_recentlyPlayedSoundClassPool.FlagFreeItem(recentlyPlayedSound);
        }

        foreach (var sound in sounds.Values)
        {
            ConsoleOutput.ShowMessage(ConsoleOutputType.Loading, $"Sound added: {sound.Key}");
            SoundHandler.soundEffects.Add(sound);
        }

        ConsoleOutput.ShowMessage(ConsoleOutputType.Loading, "Loading sounds finilizing");
        if (SoundHandler.soundEffects.Count == 0)
        {
            SoundHandler.m_soundsDisabled = true;
            return;
        }
        ConsoleOutput.ShowMessage(ConsoleOutputType.Loading, "Loading sounds completed");
    }
}
