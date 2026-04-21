using Microsoft.Xna.Framework.Audio;
using SFD;
using SFD.Code;
using SFD.Parser;
using SFD.Sounds;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace SFDCT.Assets;

internal static class SoundsLoader
{
    internal class SoundEffectGroupDefinition
    {
        internal string Key;
        internal string[] FilePaths;
        internal float VolumeModifier;
    }

    internal static IEnumerable<SoundEffectGroupDefinition> ReadSoundsFile(string sfdsFilePath)
    {
        var definitions = new List<SoundEffectGroupDefinition>();

        foreach (string line in SFDSimpleReader.Read(sfdsFilePath))
        {
            var lineBits = SFDSimpleReader.Interpret(line);
            if (lineBits.Count < 3) continue;

            var soundGroupKey = lineBits[0];
            var soundGroupFilePaths = new List<string>();
            var soundGroupVolumeModifier = 1f;

            if (!SFDXParser.TryParseFloat(lineBits[1], out soundGroupVolumeModifier))
            {
                throw new Exception("Error: Could not parse volume modifier in line \n'" + line + "'\n in file '" + sfdsFilePath + "'\r\n");
            }

            for (int i = 2; i < lineBits.Count; i++)
            {
                string path = lineBits[i];

                soundGroupFilePaths.Add(path);
            }

            var definition = new SoundEffectGroupDefinition()
            {
                Key = soundGroupKey,
                FilePaths = soundGroupFilePaths.ToArray(),
                VolumeModifier = soundGroupVolumeModifier
            };

            definitions.Add(definition);
        }

        return definitions;
    }

    internal static bool Load(GameSFD game)
    {
        game.ShowLoadingText(LanguageHelper.GetText("loading.sounds"));

        // SoundHandler vanilla setup
        SoundHandler.game = game;
        SoundHandler.soundEffects = new();
        SoundHandler.m_recentlyPlayedSoundClassPool = new(() => new SoundHandler.RecentlyPlayedSound());

        for (int i = 0; i < 30; i++)
        {
            var recentlyPlayedSound = SoundHandler.m_recentlyPlayedSoundClassPool.GetFreeItem();
            recentlyPlayedSound.InUse = false;

            SoundHandler.m_recentlyPlayedSoundClassPool.FlagFreeItem(recentlyPlayedSound);
        }

        var contents = SubContentHandler.GetContents()
                        .Where(content => Directory.Exists(Path.Combine(content.Directory, Constants.Paths.DATA_SOUNDS)));

        var totalSounds = new List<SoundHandler.SoundEffectGroup>();
        var totalSoundDefinitions = new Dictionary<string, SoundEffectGroupDefinition>();

        // read sound definition files
        foreach (var content in contents)
        {
            foreach (var sfdsFilePath in Directory.GetFiles(Path.Combine(content.Directory, Constants.Paths.DATA_SOUNDS), "*.sfds", SearchOption.AllDirectories))
            {
                ConsoleOutput.ShowMessage(ConsoleOutputType.Loading, $"Reading sounds file: {sfdsFilePath}");

                var sfdsSoundGroupDefinitions = ReadSoundsFile(sfdsFilePath);

                foreach (var soundDefinition in sfdsSoundGroupDefinitions)
                {
                    if (totalSoundDefinitions.ContainsKey(soundDefinition.Key)) continue;
                    totalSoundDefinitions.Add(soundDefinition.Key, soundDefinition);
                }
            }
        }

        if (totalSoundDefinitions.Count == 0)
        {
            SoundHandler.m_soundsDisabled = true;
            return true;
        }

        // read definitions
        foreach (var definition in totalSoundDefinitions.Values)
        {
            var sounds = new List<SoundEffect>();
            var paths = definition.FilePaths.ToList();

            // check contents in reverse so sounds can be overwritten
            foreach (var content in contents.Reverse())
            {
                var soundsFolderPath = Path.Combine(content.Directory, Constants.Paths.DATA_SOUNDS);

                for (int i = paths.Count - 1; i >= 0; i--)
                {
                    var soundPath = paths[i];
                    var soundContentPath = Path.Combine(soundsFolderPath, soundPath) + ".wav";

                    if (!File.Exists(soundContentPath)) continue;

                    SoundEffect soundEffect = null;

                    try
                    {
                        soundEffect = Content.Load<SoundEffect>(soundContentPath);
                    }
                    catch (NoAudioHardwareException)
                    {
                        SoundHandler.m_soundsDisabled = true;
                        ConsoleOutput.ShowMessage(ConsoleOutputType.Loading, "Loading sounds aborted - no hardware or drivers");

                        if (!SFD.Program.AutoStart)
                        {
                            MessageBox.Show("No audio hardware or drivers detected. Sounds will be disabled.", "No audio hardware or drivers!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        }

                        return true;
                    }
                    catch (Exception)
                    {
                        soundEffect = null;

                        ConsoleOutput.ShowMessage(ConsoleOutputType.Error, $"Error: Could not load sound effect '{soundPath}' from sound group '{definition.Key}'");
                    }

                    if (soundEffect != null)
                    {
                        sounds.Add(soundEffect);

                        // stop checking found sounds in later contents
                        paths.RemoveAt(i);
                    }
                }
            }

            if (sounds.Count == 0) continue;

            // create and load sound group
            var sound = new SoundHandler.SoundEffectGroup(definition.Key, definition.VolumeModifier, sounds.ToArray());

            if (sound.Key == "CHAINSAW") SoundHandler.GlobalLoopSounds.Chainsaw = new SoundHandler.GlobalLoopSound(sound.Key, sound.SoundEffects[0].CreateInstance(), sound.VolumeModifier);
            if (sound.Key == "STREETSWEEPERPROPELLER") SoundHandler.GlobalLoopSounds.StreetsweeperPropeller = new SoundHandler.GlobalLoopSound(sound.Key, sound.SoundEffects[0].CreateInstance(), sound.VolumeModifier);

            ConsoleOutput.ShowMessage(ConsoleOutputType.Loading, $"Adding sound group: '{sound.Key}' {sound.VolumeModifier * 100}% ({sound.SoundEffects.Length})");
            SoundHandler.soundEffects.Add(sound);
        }

        return true;
    }
}
