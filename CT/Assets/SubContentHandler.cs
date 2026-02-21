using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using SFD;
using SFD.Code;
using SFD.Colors;
using SFD.Core;
using SFD.Loading;
using SFD.Parser;
using SFD.Sounds;
using SFD.Tiles;
using SFDCT.Configuration;
using SFDCT.Helper;
using SFDCT.Misc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SFDCT.Assets;

[HarmonyPatch]
internal static class SubContentHandler
{
    internal const string FilterSeparator = "|";
    internal static string[] Folders { get; private set; } = [];

    internal static void Load()
    {
        if (!SFDCTConfig.Get<bool>(CTSettingKey.SubContent)) return;

        var enabled = new List<string>(GetEnabledFolders());
        var disabled = new List<string>(GetDisabledFolders());

        foreach (string folder in GetNewFolders())
        {
            Logger.LogInfo($"[SUB-CONTENT] Adding new folder: {folder}");
            enabled.Add(folder);
        }

        foreach (string folder in GetDeletedFolders())
        {
            if (disabled.Contains(folder)) disabled.Remove(folder);
            if (enabled.Contains(folder)) enabled.Remove(folder);
        }

        SFDCTConfig.Set(CTSettingKey.SubContentEnabledFolders, string.Join(FilterSeparator, enabled));
        SFDCTConfig.Set(CTSettingKey.SubContentDisabledFolders, string.Join(FilterSeparator, disabled));
        SFDCTConfig.SaveFile();

        Folders = enabled.ToArray();
    }

    internal static string[] GetDeletedFolders()
    {
        var known = GetKnownFolders();

        return known.Where(folder => string.IsNullOrWhiteSpace(folder) || !Directory.Exists(Path.Combine(Globals.Paths.SubContent, folder))).ToArray();
    }

    internal static string[] GetNewFolders()
    {
        var known = GetKnownFolders();

        return GetAllFolders().Where(folder => !known.Contains(folder)).ToArray();
    }

    internal static string[] GetFoldersFromSetting(string setting) => setting.Trim(FilterSeparator.ToCharArray()).Split([FilterSeparator], StringSplitOptions.RemoveEmptyEntries);
    internal static string[] GetAllFolders() => Directory.GetDirectories(Globals.Paths.SubContent, "*", SearchOption.TopDirectoryOnly).Select(Path.GetFileName).ToArray();
    internal static string[] GetKnownFolders() => GetEnabledFolders().Concat(GetDisabledFolders()).ToArray();
    internal static string[] GetEnabledFolders() => GetFoldersFromSetting(SFDCTConfig.Get<string>(CTSettingKey.SubContentEnabledFolders));
    internal static string[] GetDisabledFolders() => GetFoldersFromSetting(SFDCTConfig.Get<string>(CTSettingKey.SubContentDisabledFolders));

    internal static void CopyAndReplaceDictionary<TKey, TValue>(Dictionary<TKey, TValue> fromDic, Dictionary<TKey, TValue> toDic)
    {
        foreach (var kvp in fromDic)
        {
            toDic[kvp.Key] = kvp.Value;
        }
    }

    internal static Dictionary<ContentOriginType, string[]> GetContentFiles(string searchPattern, SearchOption searchOption, params string[] relativeToContentPath)
    {
        var files = new Dictionary<ContentOriginType, string[]>();

        string officialPath = Path.Combine(Constants.Paths.ContentPath, Path.Combine(relativeToContentPath));
        string documentsPath = Path.Combine(Constants.Paths.UserDocumentsContentCustomPath, Path.Combine(relativeToContentPath));

        if (Directory.Exists(officialPath))
        {
            files.Add(ContentOriginType.Official, Directory.GetFiles(officialPath, searchPattern, searchOption));
        }

        if (Directory.Exists(documentsPath))
        {
            files.Add(ContentOriginType.Documents, Directory.GetFiles(documentsPath, searchPattern, searchOption));
        }

        var subContentFiles = new List<string>();

        foreach (string subContentFolder in Folders)
        {
            string subContentPath = Path.Combine(Globals.Paths.SubContent, subContentFolder, "Content", Path.Combine(relativeToContentPath));

            if (Directory.Exists(subContentPath))
            {
                subContentFiles.AddRange(Directory.GetFiles(subContentPath, searchPattern, searchOption));
            }
        }

        if (subContentFiles.Count > 0)
        {
            files.Add(ContentOriginType.SubContent, subContentFiles.ToArray());
        }

        return files;
    }

    internal static List<SoundHandler.SoundEffectGroup> LoadSoundEffectGroups(string soundsFolderPath, string sfdsFilePath)
    {
        if (SoundHandler.m_soundsDisabled) return null;

        List<SoundHandler.SoundEffectGroup> result = [];
        foreach (var line in SFDSimpleReader.Read(sfdsFilePath))
        {
            var lineBits = SFDSimpleReader.Interpret(line);

            if (lineBits.Count < 3)
            {
                ConsoleOutput.ShowMessage(ConsoleOutputType.Error, $"Error: Invalid format in line '{line}' in file '{sfdsFilePath}'");
                continue;
            }

            string key = lineBits[0];
            float volume;
            List<SoundEffect> variations;

            try
            {
                volume = SFDXParser.ParseFloat(lineBits[1]);
            }
            catch
            {
                ConsoleOutput.ShowMessage(ConsoleOutputType.Error, $"Error: Could not parse volume modifier in line '{line}' in file '{sfdsFilePath}'");
                continue;
            }

            variations = [];
            for (int i = 0; i < lineBits.Count - 2; i++)
            {
                string variationPath = Path.Combine(soundsFolderPath, lineBits[i + 2]);

                try
                {
                    SoundEffect variationSound = null;
                    variationSound = Content.Load<SoundEffect>(variationPath);

                    if (variationSound != null) variations.Add(variationSound);
                }
                catch (NoAudioHardwareException)
                {
                    SoundHandler.m_soundsDisabled = true;
                    ConsoleOutput.ShowMessage(ConsoleOutputType.Loading, "Loading sounds aborted - no hardware or drivers");

                    if (!SFD.Program.AutoStart)
                    {
                        MessageBox.Show("No audio hardware or drivers detected. Sounds will be disabled.", "No audio hardware or drivers!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                    return null;
                }
                catch
                {
                    ConsoleOutput.ShowMessage(ConsoleOutputType.Error, $"Error: Could not load sound '{variationPath}' in line '{line}' in file '{sfdsFilePath}");
                }
            }

            var soundEffectGroup = new SoundHandler.SoundEffectGroup(key, volume, variations.ToArray());
            if (!soundEffectGroup.IsValid) continue;

            result.Add(soundEffectGroup);
        }

        return result;
    }

    //internal static AnimationsData LoadAnimationsFromTextFiles(string[] filePaths)
    //{
    //    AnimationData[] animations = new AnimationData[filePaths.Length];

    //    for (int i = 0; i < animations.Length; i++)
    //    {
    //        string filePath = filePaths[i];
    //        string[] fileLines = File.ReadAllLines(filePath);
    //        string animName = Path.GetFileNameWithoutExtension(filePath);

    //        int frameTime = 0;
    //        string frameEvent = string.Empty;
    //        List<AnimationFrameData> frames = [];
    //        List<AnimationPartData> parts = [];
    //        List<AnimationCollisionData> collisions = [];

    //        for (int j = 0; j < fileLines.Length; j++)
    //        {
    //            string line = fileLines[j].Trim();
    //            string[] lineBits = line.Split(' ');

    //            if (!string.IsNullOrEmpty(line))
    //            {
    //                if (lineBits[0].Equals("FRAME", StringComparison.OrdinalIgnoreCase))
    //                {
    //                    frameTime = int.Parse(lineBits[1]);
    //                    frameEvent = lineBits.Length > 2 ? lineBits[2] : string.Empty;
    //                }
    //                else if (lineBits[0].Equals("PART", StringComparison.OrdinalIgnoreCase))
    //                {
    //                    int id;
    //                    if (!int.TryParse(lineBits[1], out id))
    //                    {
    //                        if (lineBits[1].Equals("SUBANIMATION", StringComparison.OrdinalIgnoreCase))
    //                        {
    //                            id = ItemPart.TYPE.M_SUBANIMATION;
    //                        }
    //                        else if (lineBits[1].Equals("TAIL", StringComparison.OrdinalIgnoreCase))
    //                        {
    //                            id = ItemPart.TYPE.M_TAIL;
    //                        }
    //                        else if (lineBits[1].Equals("SHEATHED_RIFLE", StringComparison.OrdinalIgnoreCase))
    //                        {
    //                            id = ItemPart.TYPE.M_SHEATHED_RIFLE;
    //                        }
    //                        else if (lineBits[1].Equals("SHEATHED_MELEE", StringComparison.OrdinalIgnoreCase))
    //                        {
    //                            id = ItemPart.TYPE.M_SHEATHED_MELEE;
    //                        }
    //                        else if (lineBits[1].Equals("SHEATHED_HANDGUN", StringComparison.OrdinalIgnoreCase))
    //                        {
    //                            id = ItemPart.TYPE.M_SHEATHED_HANDGUN;
    //                        }
    //                        else if (lineBits[1].Equals("WPN_MAINHAND", StringComparison.OrdinalIgnoreCase))
    //                        {
    //                            id = ItemPart.TYPE.M_WPN_MAINHAND;
    //                        }
    //                        else if (lineBits[1].Equals("WPN_OFFHAND", StringComparison.OrdinalIgnoreCase))
    //                        {
    //                            id = ItemPart.TYPE.M_WPN_OFFHAND;
    //                        }
    //                        else
    //                        {
    //                            int partID, textureID;
    //                            string[] partIDBits = lineBits[1].Split('_');
    //                            if (partIDBits.Length == 2 && int.TryParse(partIDBits[0], out partID) && int.TryParse(partIDBits[1], out textureID))
    //                            {
    //                                id = ItemPart.TYPE.PART_RANGE * partID + textureID;
    //                            }
    //                        }
    //                    }

    //                    float x = SFDXParser.ParseFloat(lineBits[2]);
    //                    float y = SFDXParser.ParseFloat(lineBits[3]);
    //                    float rotation = SFDXParser.ParseFloat(lineBits[4]);
    //                    SpriteEffects flip = (SpriteEffects)int.Parse(lineBits[5]);
    //                    float sx = SFDXParser.ParseFloat(lineBits[6]);
    //                    float sy = SFDXParser.ParseFloat(lineBits[7]);
    //                    string postFix = lineBits.ElementAtOrDefault(8);

    //                    parts.Add(new(id, x, y, rotation, flip, sx, sy, postFix));
    //                }
    //                else if (lineBits[0].Equals("COLLISION", StringComparison.OrdinalIgnoreCase))
    //                {
    //                    int id = int.Parse(lineBits[1]);
    //                    float x = SFDXParser.ParseFloat(lineBits[2]);
    //                    float y = SFDXParser.ParseFloat(lineBits[3]);
    //                    float width = SFDXParser.ParseFloat(lineBits[4]);
    //                    float height = SFDXParser.ParseFloat(lineBits[5]);
    //                    collisions.Add(new(id, x, y, width, height));
    //                }

    //                if (j == fileLines.Length - 1 || (j > 0 && lineBits[0].Equals("FRAME", StringComparison.OrdinalIgnoreCase)))
    //                {
    //                    frames.Add(new(parts.ToArray(), collisions.ToArray(), frameEvent, frameTime));
    //                }
    //            }
    //        }

    //        animations[i] = new(frames.ToArray(), animName);
    //    }

    //    return new AnimationsData(animations);
    //}

    internal static bool AnalyzeAnimation(AnimationData data, Animations.AnalyzeAnimation analyzeData)
    {
        if (data.Frames.Length != analyzeData.FrameTimes.Length) return false;

        for (int j = 0; j < data.Frames.Length; j++)
        {
            if (data.Frames[j].Time != analyzeData.FrameTimes[j] || data.Frames[j].Event != analyzeData.FrameEvents[j])
            {
                return false;
            }
        }

        return true;
    }

    internal static string GetPath(string subContentFolder, params string[] path)
    {
        return Path.Combine(Globals.Paths.SubContent, subContentFolder, Path.Combine(path));
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Animations), nameof(Animations.Load))]
    private static bool Animations_Load_Prefix_OverrideLoad(ref bool __result)
    {
        if (!SFDCTConfig.Get<bool>(CTSettingKey.SubContent)) return true;

        var analyzeDataDic = new Dictionary<string, Animations.AnalyzeAnimation>
        {
            { "BaseKick",               new("BaseKick",                 [25, 50, 50, 50, 100, 100],     ["STEP", "", "", "MELEESWING", "KICK", "STOP"]) },
            { "FullCharge",             new("FullCharge",               [500, 100, 100, 100, 100, 100], ["TELEGRAPH", "", "", "", "", ""]) },
            { "FullChargeA",            new("FullChargeA",              [500],                          ["TELEGRAPH"]) },
            { "FullChargeB",            new("FullChargeB",              [100, 100, 100, 100],           ["", "", "", ""]) },
            { "FullRoll",               new("FullRoll",                 [100, 100, 100, 100, 100],      ["", "", "", "", ""]) },
            { "UpperBlock",             new("UpperBlock",               [50, 50, 200],                  ["", "", "STOP"]) },
            { "UpperBlockChainsaw",     new("UpperBlockChainsaw",       [50, 50, 200],                  ["", "", "STOP"]) },
            { "UpperBlockMelee",        new("UpperBlockMelee",          [50, 50, 200],                  ["", "", "STOP"]) },
            { "UpperBlockMelee2H",      new("UpperBlockMelee2H",        [50, 50, 200],                  ["", "", "STOP"]) },
            { "UpperBlockMelee2HEnd",   new("UpperBlockMelee2HEnd",     [200],                          [""]) },
            { "UpperBlockMeleeEnd",     new("UpperBlockMeleeEnd",       [200],                          [""]) },
            { "UpperMelee1H1",          new("UpperMelee1H1",            [100, 50, 25, 75, 250],         ["", "", "MELEESWING", "HIT", "STOP"]) },
            { "UpperMelee1H1End",       new("UpperMelee1H1End",         [75, 200],                      ["", "STOP"]) },
            { "UpperMelee1H2",          new("UpperMelee1H2",            [150, 50, 25, 75, 250],         ["", "", "MELEESWING", "HIT", "STOP"]) },
            { "UpperMelee1H2End",       new("UpperMelee1H2End",         [75, 200],                      ["", "STOP"]) },
            { "UpperMelee1H3",          new("UpperMelee1H3",            [200, 50, 25, 75, 300],         ["", "", "MELEESWING", "HIT", "STOP"]) },
            { "UpperMelee1H3Chain",     new("UpperMelee1H3Chain",       [200, 50, 25, 75, 300],         ["", "", "MELEESWING", "HIT", "STOP"]) },
            { "UpperMelee1H3ChainEnd",  new("UpperMelee1H3ChainEnd",    [75, 300],                      ["", "STOP"]) },
            { "UpperMelee1H3End",       new("UpperMelee1H3End",         [75, 300],                      ["", "STOP"]) },
            { "UpperMelee1H4",          new("UpperMelee1H4",            [100, 50, 50, 50, 250],         ["", "", "MELEESWING", "HIT", "STOP"]) },
            { "UpperMelee2H1",          new("UpperMelee2H1",            [100, 50, 25, 75, 250],         ["", "", "MELEESWING", "HIT", "STOP"]) },
            { "UpperMelee2H1End",       new("UpperMelee2H1End",         [75, 250],                      ["", "STOP"]) },
            { "UpperMelee2H2",          new("UpperMelee2H2",            [150, 50, 25, 75, 250],         ["", "", "MELEESWING", "HIT", "STOP"]) },
            { "UpperMelee2H2End",       new("UpperMelee2H2End",         [75, 250],                      ["", "STOP"]) },
            { "UpperMelee2H3",          new("UpperMelee2H3",            [200, 50, 25, 75, 300],         ["", "", "MELEESWING", "HIT", "STOP"]) },
            { "UpperMelee2H3End",       new("UpperMelee2H3End",         [75, 300],                      ["", "STOP"]) },
            { "UpperMelee2H4",          new("UpperMelee2H4",            [100, 50, 50, 100],             ["", "MELEESWING", "HIT", "STOP"]) },
            { "UpperMeleeHit1",         new("UpperMeleeHit1",           [50, 50],                       ["", "STOP"]) },
            { "UpperMeleeHit2",         new("UpperMeleeHit2",           [50, 50],                       ["", "STOP"]) },
            { "UpperPunch1",            new("UpperPunch1",              [150, 25, 75, 250],             ["", "MELEESWING", "HIT", "STOP"]) },
            { "UpperPunch2",            new("UpperPunch2",              [150, 25, 75, 250],             ["", "MELEESWING", "HIT", "STOP"]) },
            { "UpperPunch3",            new("UpperPunch3",              [200, 25, 75, 50, 250],         ["", "MELEESWING", "HIT", "", "STOP"]) },
            { "UpperPunch4",            new("UpperPunch4",              [100, 25, 25, 200],             ["", "", "MELEESWING_HIT", "STOP"]) }
        };

        var animationsContent = GetContentFiles("char_anims", SearchOption.AllDirectories, Constants.Paths.DATA_ANIMATIONS);
        var currentContent = 1;
        var totalContent = animationsContent.Count();
        var animations = new Dictionary<string, AnimationData>();

        foreach (var kvp in animationsContent)
        {
            Logger.LogInfo($"LOADING [ANIMATIONS]: {kvp.Key}");
            GameSFD.Handle.ShowLoadingText($"{LanguageHelper.GetText("loading.animations")} ({currentContent}/{totalContent})");

            int current = 0;
            int total = kvp.Value.Length;

            foreach (var path in kvp.Value)
            {
                GameSFD.Handle.SetLoadingProgress(current, total);
                AnimationsData animationsData = Content.Load<AnimationsData>(path);
                CopyAndReplaceDictionary(animationsData.DicAnimations, animations);

                current++;
            }

            GameSFD.Handle.SetLoadingProgress(0, 0);
            currentContent++;
        }

        foreach (var analyzeData in analyzeDataDic)
        {
            if (animations.ContainsKey(analyzeData.Key) && !AnalyzeAnimation(animations[analyzeData.Key], analyzeData.Value))
            {
                SFD.Program.ShowError(new Exception("Core animation data file has been modified in an unintended way. Restore your char_anims file."), "Core animation data modified!", true);

                __result = false;
                return false;
            }
        }

        Animations.Data = new(animations.Values.ToArray());

        __result = true;
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Textures), nameof(Textures.Load), [typeof(string)])]
    private static bool Textures_Load_Prefix_OverrideLoad()
    {
        if (!SFDCTConfig.Get<bool>(CTSettingKey.SubContent)) return true;

        // Reverse the order of textureContents because we use the default load
        // method, it checks if the texture names exist before adding them
        var textureContents = GetContentFiles("*.png", SearchOption.AllDirectories, Constants.Paths.DATA_IMAGES).Reverse();
        var currentContent = 1;
        var totalContent = textureContents.Count();

        foreach (var kvp in textureContents)
        {
            Logger.LogInfo($"LOADING [TEXTURES]: {kvp.Key}");
            GameSFD.Handle.ShowLoadingText($"{LanguageHelper.GetText("loading.textures")} ({currentContent}/{totalContent})");

            int current = 0;
            int total = kvp.Value.Length;

            GameSFD.Handle.SetLoadingProgress(current, total);
            Parallel.ForEach(kvp.Value, (path) =>
            {
                Textures.m_tileTextures.Load(path, kvp.Key != ContentOriginType.Official);

                Interlocked.Increment(ref current);
                GameSFD.Handle.SetLoadingProgress(current, total);
            });

            GameSFD.Handle.SetLoadingProgress(0, 0);

            currentContent++;
        }

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Items), nameof(Items.Load), [typeof(GameSFD)])]
    private static bool Items_Load_Prefix_OverrideLoad()
    {
        if (!SFDCTConfig.Get<bool>(CTSettingKey.SubContent)) return true;

        Items.m_allItems = [];
        Items.m_allFemaleItems = [];
        Items.m_allMaleItems = [];

        Items.m_slotAllItems = new List<Item>[Equipment.TOTAL_INTERNAL_LAYERS];
        Items.m_slotFemaleItems = new List<Item>[Equipment.TOTAL_INTERNAL_LAYERS];
        Items.m_slotMaleItems = new List<Item>[Equipment.TOTAL_INTERNAL_LAYERS];
        for (int i = 0; i < Equipment.TOTAL_INTERNAL_LAYERS; i++)
        {
            Items.m_slotAllItems[i] = [];
            Items.m_slotFemaleItems[i] = [];
            Items.m_slotMaleItems[i] = [];
        }

        var semaphore = new SemaphoreSlim(1);
        var itemLoadedIDs = new HashSet<string>();
        var itemContents = GetContentFiles("*.item", SearchOption.AllDirectories, Constants.Paths.DATA_ITEMS);
        var currentContent = 1;
        var totalContent = itemContents.Count();

        foreach (var kvp in itemContents)
        {
            Logger.LogInfo($"LOADING [ITEMS]: {kvp.Key}");
            GameSFD.Handle.ShowLoadingText($"{LanguageHelper.GetText("loading.equipment")} ({currentContent}/{totalContent})");

            int current = 0;
            int total = kvp.Value.Length;

            GameSFD.Handle.SetLoadingProgress(current, total);
            Parallel.ForEach(kvp.Value, (path) =>
            {
                if (GameSFD.Closing) return;

                Item item = Content.Load<Item>(path);
                item.PostProcess();

                try
                {
                    semaphore.Wait();

                    Interlocked.Increment(ref current);
                    GameSFD.Handle.SetLoadingProgress(current, total);

                    if (!itemLoadedIDs.Add(item.ID) && kvp.Key == ContentOriginType.Official)
                    {
                        foreach (Item item2 in Items.m_allItems)
                        {
                            if (item2.ID == item.ID)
                            {
                                throw new Exception($"Error: Item ID collision between item '{item2}' and '{item}' while loading '{path}'");
                            }
                        }

                        throw new Exception($"Error: Item ID collision, item with ID '{item.ID}' has already been loaded, cannot load item '{item}' from '{path}'");
                    }

                    Items.m_allItems.Add(item);
                    Items.m_slotAllItems[item.EquipmentLayer].Add(item);
                }
                finally
                {
                    semaphore.Release();
                }
            });

            GameSFD.Handle.SetLoadingProgress(0, 0);

            currentContent++;
        }

        semaphore.Dispose();

        Items.PostProcessGenders();

        Player.HurtLevel1 = Items.GetItem("HurtLevel1");
        Player.HurtLevel2 = Items.GetItem("HurtLevel2");
        Player.HurtLevel2 ??= Player.HurtLevel1;

        Items.IsLoaded = true;
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ColorPaletteDatabase), nameof(ColorPaletteDatabase.Load), [typeof(GameSFD)])]
    private static bool ColorPaletteDatabase_Load_Prefix_OverrideLoad()
    {
        if (!SFDCTConfig.Get<bool>(CTSettingKey.SubContent)) return true;

        var colorPalettes = new Dictionary<string, ColorPalette>();
        var colorPaletteContents = GetContentFiles("*.sfdx", SearchOption.TopDirectoryOnly, Constants.Paths.DATA_COLORS_PALETTES);
        var currentContent = 1;
        var totalContent = colorPaletteContents.Count();

        foreach (var kvp in colorPaletteContents)
        {
            Logger.LogInfo($"LOADING [COLOR-PALETTES]: {kvp.Key}");
            GameSFD.Handle.ShowLoadingText($"{LanguageHelper.GetText("loading.colors")} ({currentContent}/{totalContent})");

            foreach (var path in kvp.Value)
            {
                SFDXReader.ReadDataFromSFDXFile(path);

                CopyAndReplaceDictionary(ColorPaletteDatabase.m_palettes, colorPalettes);
                ColorPaletteDatabase.m_palettes.Clear();
            }

            currentContent++;
        }

        ColorPaletteDatabase.m_palettes = colorPalettes;
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ColorDatabase), nameof(ColorDatabase.Load), [typeof(GameSFD)])]
    private static bool ColorDatabase_Load_Prefix_OverrideLoad()
    {
        if (!SFDCTConfig.Get<bool>(CTSettingKey.SubContent)) return true;

        var colors = new Dictionary<string, Color[]>();
        var colorsContents = GetContentFiles("*.sfdx", SearchOption.TopDirectoryOnly, Constants.Paths.DATA_COLORS_COLORS);
        var currentContent = 1;
        var totalContent = colorsContents.Count();

        foreach (var kvp in colorsContents)
        {
            Logger.LogInfo($"LOADING [COLORS]: {kvp.Key}");
            GameSFD.Handle.ShowLoadingText($"{LanguageHelper.GetText("loading.colors")} ({currentContent}/{totalContent})");

            foreach (var path in kvp.Value)
            {
                SFDXReader.ReadDataFromSFDXFile(path);

                CopyAndReplaceDictionary(ColorDatabase.m_colors, colors);
                ColorDatabase.m_colors.Clear();
            }

            currentContent++;
        }

        ColorDatabase.m_colors = colors;
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(SoundHandler), nameof(SoundHandler.Load), [typeof(GameSFD)])]
    private static bool SoundHandler_Load_Prefix_OverrideLoad(GameSFD game)
    {
        if (!SFDCTConfig.Get<bool>(CTSettingKey.SubContent)) return true;

        ConsoleOutput.ShowMessage(ConsoleOutputType.Loading, "Loading sounds...");
        SoundHandler.game = game;
        SoundHandler.soundEffects = new SoundHandler.SoundEffectGroups();
        SoundHandler.m_recentlyPlayedSoundClassPool = new GenericClassPool<SoundHandler.RecentlyPlayedSound>(() => new SoundHandler.RecentlyPlayedSound(), 1, 0);

        for (int i = 0; i < 30; i++)
        {
            SoundHandler.RecentlyPlayedSound recentlyPlayedSound = SoundHandler.m_recentlyPlayedSoundClassPool.GetFreeItem();

            recentlyPlayedSound.InUse = false;
            SoundHandler.m_recentlyPlayedSoundClassPool.FlagFreeItem(recentlyPlayedSound);
        }

        var soundContents = GetContentFiles("*.sfds", SearchOption.TopDirectoryOnly, Constants.Paths.DATA_SOUNDS);
        var sounds = new Dictionary<string, SoundHandler.SoundEffectGroup>();
        var soundFileCount = 0;

        foreach (var kvp in soundContents)
        {
            Logger.LogInfo($"LOADING [SOUNDS]: {kvp.Key}");

            if (kvp.Key == ContentOriginType.Official)
            {
                foreach (var path in kvp.Value)
                {
                    var folderPath = Path.GetDirectoryName(path);
                    var soundGroups = LoadSoundEffectGroups(folderPath, path);

                    if (soundGroups == null) continue;

                    foreach (var group in soundGroups)
                    {
                        if (sounds.ContainsKey(group.Key))
                        {
                            throw new Exception($"Error: Invalid sound key '{group.Key}' - it's already taken");
                        }

                        sounds.Add(group.Key, group);
                    }
                }
            }
            else
            {
                foreach (var path in kvp.Value)
                {
                    var folderPath = Path.GetDirectoryName(path);
                    var soundGroups = LoadSoundEffectGroups(folderPath, path);

                    if (soundGroups == null) continue;

                    foreach (var group in soundGroups)
                    {
                        sounds[group.Key] = group;
                    }
                }
            }

            soundFileCount++;
        }

        ConsoleOutput.ShowMessage(ConsoleOutputType.Loading, "Loading sounds finilizing");

        foreach (var soundEffectGroup in sounds.Values)
        {
            if (soundEffectGroup.Key == "CHAINSAW") SoundHandler.GlobalLoopSounds.Chainsaw = new SoundHandler.GlobalLoopSound(soundEffectGroup.Key, soundEffectGroup.SoundEffects[0].CreateInstance(), soundEffectGroup.VolumeModifier);
            if (soundEffectGroup.Key == "STREETSWEEPERPROPELLER") SoundHandler.GlobalLoopSounds.StreetsweeperPropeller = new SoundHandler.GlobalLoopSound(soundEffectGroup.Key, soundEffectGroup.SoundEffects[0].CreateInstance(), soundEffectGroup.VolumeModifier);

            SoundHandler.soundEffects.Add(soundEffectGroup);
            ConsoleOutput.ShowMessage(ConsoleOutputType.Loading, $"Sound added: {soundEffectGroup.Key}");
        }

        if (soundFileCount == 0 || SoundHandler.soundEffects.Count == 0)
        {
            SoundHandler.m_soundsDisabled = true;
        }

        ConsoleOutput.ShowMessage(ConsoleOutputType.Loading, "Loading sounds completed");

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(TileDatabase), nameof(TileDatabase.Load), [typeof(GameSFD)])]
    private static bool TileDatabase_Load_Prefix_OverrideLoad()
    {
        if (!SFDCTConfig.Get<bool>(CTSettingKey.SubContent)) return true;

        var tiles = new Dictionary<string, Tile>();
        var tilesContents = GetContentFiles("*.sfdx", SearchOption.TopDirectoryOnly, Constants.Paths.DATA_TILES);
        var weaponTilesContents = GetContentFiles("*.sfdx", SearchOption.TopDirectoryOnly, Constants.Paths.DATA_WEAPONS);

        int currentContent = 1;
        int totalContent = tilesContents.Count();

        foreach (var kvp in tilesContents)
        {
            Logger.LogInfo($"LOADING [TILES]: {kvp.Key}");
            GameSFD.Handle.ShowLoadingText($"{LanguageHelper.GetText("loading.tiles")} ({currentContent}/{totalContent})");

            foreach (var filePath in kvp.Value)
            {
                SFDXReader.ReadDataFromSFDXFile(filePath);

                CopyAndReplaceDictionary(TileDatabase.m_tiles, tiles);
                TileDatabase.m_tiles.Clear();
                TileDatabase.m_categorizedTiles.Clear();
            }

            currentContent++;
        }

        currentContent = 1;
        totalContent = weaponTilesContents.Count();

        foreach (var kvp in weaponTilesContents)
        {
            Logger.LogInfo($"LOADING [WEAPON-TILES]: {kvp.Key}");
            GameSFD.Handle.ShowLoadingText($"{LanguageHelper.GetText("loading.tiles")} ({currentContent}/{totalContent})");

            foreach (var filePath in kvp.Value)
            {
                SFDXReader.ReadDataFromSFDXFile(filePath);
            }

            CopyAndReplaceDictionary(TileDatabase.m_tiles, tiles);
            TileDatabase.m_tiles.Clear();
            TileDatabase.m_categorizedTiles.Clear();

            currentContent++;
        }

        foreach (var tile in tiles.Values)
        {
            TileDatabase.Add(tile);
        }

        var playerTile = new Tile(TileStructure.GetPlayerTileStructure());
        TileDatabase.Remove(playerTile.Key);
        TileDatabase.Add(playerTile);
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(BackgroundImage), nameof(BackgroundImage.LoadTexture))]
    private static bool BackgroundImage_LoadTexture_Prefix_OverrideLoad()
    {
        if (!SFDCTConfig.Get<bool>(CTSettingKey.SubContent)) return true;

        if (BackgroundImage.m_image != null && !BackgroundImage.m_image.IsDisposed)
        {
            BackgroundImage.m_image.Dispose();
            BackgroundImage.m_image = null;
        }

        var backgroundImageContents = GetContentFiles("SFD.jpg", SearchOption.TopDirectoryOnly, Constants.Paths.DATA_MISC);
        string SFDjpgImagePath = null;

        foreach (var kvp in backgroundImageContents)
        {
            if (kvp.Value.Length <= 0) continue;

            Logger.LogInfo($"LOADING [BACKGROUND-IMAGE]: {kvp.Key}");
            SFDjpgImagePath = kvp.Value.Last();
        }

        if (SFDjpgImagePath != null && File.Exists(SFDjpgImagePath))
        {
            try
            {
                ConsoleOutput.ShowMessage(ConsoleOutputType.Loading, $"Loading background image: {SFDjpgImagePath}");

                using (FileStream fileStream = File.OpenRead(SFDjpgImagePath))
                {
                    Utils.WaitForGraphicsDevice();
                    BackgroundImage.m_image = Texture2D.FromStream(GameSFD.Handle.GraphicsDevice, fileStream);

                    fileStream.Close();
                }
            }
            catch (Exception ex)
            {
                ConsoleOutput.ShowMessage(ConsoleOutputType.Error, $"file '{SFDjpgImagePath}' could not be loaded: " + ex.Message);
            }
        }

        return false;
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(TileTextures), nameof(TileTextures.Load))]
    private static IEnumerable<CodeInstruction> TileTextures_Load_Prefix_PathChange(IEnumerable<CodeInstruction> instructions)
    {
        // These tiny changes allows re-using the method without too many changes.

        // Do not call 'Constants.Paths.GetContentAssetPathFromFullPath' and use full path instead
        instructions.ElementAt(23).opcode = OpCodes.Nop;

        // Don't check here for custom PNG textures in documents
        // Assume the PNG texture in documents never exists, always branch
        instructions.ElementAt(69).opcode = OpCodes.Nop;
        instructions.ElementAt(70).opcode = OpCodes.Ldc_I4_0;

        return instructions;
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(TileTextures), nameof(TileTextures.AddTexture))]
    private static IEnumerable<CodeInstruction> TileTextures_AddTexture_Prefix_DuplicateTextureMessage(IEnumerable<CodeInstruction> instructions)
    {
        instructions.ElementAt(27).opcode = OpCodes.Nop;
        instructions.ElementAt(28).opcode = OpCodes.Nop;
        instructions.ElementAt(29).opcode = OpCodes.Nop;
        instructions.ElementAt(30).opcode = OpCodes.Nop;
        instructions.ElementAt(31).opcode = OpCodes.Nop;

        return instructions;
    }
}