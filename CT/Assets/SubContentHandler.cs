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
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace SFDCT.Assets;

[HarmonyPatch]
internal static class SubContentHandler
{
    internal const string ANIMATIONS_FILE_NAME = "char_anims";
    internal const char SUB_CONTENT_FOLDER_SEPARATOR = '|';
    internal static string[] Folders = [];

    internal static void Load()
    {
        if (!SFDCTConfig.Get<bool>(CTSettingKey.SubContent)) return;

        List<string> enabledSubContentFolders = [.. GetEnabledFolders()];
        List<string> disabledSubContentFolders = [.. GetDisabledFolders()];

        foreach (string folderName in GetNewFolders())
        {
            Logger.LogInfo($"[SUB-CONTENT] Adding new folder: {folderName}");
            enabledSubContentFolders.Add(folderName);
        }

        foreach (string folderName in GetDeletedFolders())
        {
            if (enabledSubContentFolders.Remove(folderName) | disabledSubContentFolders.Remove(folderName))
            {
                Logger.LogInfo($"[SUB-CONTENT] Removing deleted folder: {folderName}");
            }
        }

        SFDCTConfig.Set(CTSettingKey.SubContentEnabledFolders, string.Join("|", enabledSubContentFolders));
        SFDCTConfig.Set(CTSettingKey.SubContentDisabledFolders, string.Join("|", disabledSubContentFolders));
        SFDCTConfig.SaveFile();

        Folders = [.. enabledSubContentFolders];
    }

    internal static string[] GetAllFolders()
    {
        return Directory.GetDirectories(Globals.Paths.SubContent, "*", SearchOption.TopDirectoryOnly).Select((string s) => { return Path.GetFileName(s); }).ToArray();
    }

    internal static string[] GetDeletedFolders()
    {
        List<string> folders = [.. GetEnabledFolders(), .. GetDisabledFolders()];

        return [.. folders.Where((string s) => string.IsNullOrEmpty(s) || string.IsNullOrWhiteSpace(s) || !Directory.Exists(Path.Combine(Globals.Paths.SubContent, s)))];
    }

    internal static string[] GetNewFolders()
    {
        string[] allFolders = GetAllFolders();
        string[] enabledFolders = GetEnabledFolders();
        string[] disabledFolders = GetDisabledFolders();

        return [.. allFolders.Where((string s) => !enabledFolders.Contains(s) && !disabledFolders.Contains(s))];
    }

    internal static string[] GetDisabledFolders()
    {
        return [.. SFDCTConfig.Get<string>(CTSettingKey.SubContentDisabledFolders).Trim('|').Split('|').Where((string s) => s != string.Empty)];
    }

    internal static string[] GetEnabledFolders()
    {
        return [.. SFDCTConfig.Get<string>(CTSettingKey.SubContentEnabledFolders).Trim('|').Split('|').Where((string s) => s != string.Empty)];
    }

    internal static void AddOrSetDictionaryValue<TKey, TValue>(Dictionary<TKey, TValue> dic, TKey key, TValue value)
    {
        if (dic.ContainsKey(key))
        {
            dic[key] = value;
        }
        else
        {
            dic.Add(key, value);
        }
    }

    internal static void CopyAndReplaceDictionary<TKey, TValue>(Dictionary<TKey, TValue> fromDic, Dictionary<TKey, TValue> toDic)
    {
        foreach (var kvp in fromDic)
        {
            AddOrSetDictionaryValue(toDic, kvp.Key, kvp.Value);
        }
    }

    internal static void EnumerateContentFiles(Action<ContentOriginType, string, string[]> callback, string searchPattern, SearchOption searchOption, params string[] relativeToContentPath)
    {
        string officialPath = Path.Combine(Constants.Paths.ContentPath, Path.Combine(relativeToContentPath));

        if (Directory.Exists(officialPath))
        {
            callback.Invoke(ContentOriginType.Official, officialPath, Directory.GetFiles(officialPath, searchPattern, searchOption));
        }

        string documentsPath = Path.Combine(Constants.Paths.UserDocumentsContentCustomPath, Path.Combine(relativeToContentPath));
        if (Directory.Exists(documentsPath))
        {
            callback.Invoke(ContentOriginType.Documents, documentsPath, Directory.GetFiles(documentsPath, searchPattern, searchOption));
        }

        foreach (string subContentFolder in Folders)
        {
            string subContentPath = Path.Combine(Globals.Paths.SubContent, subContentFolder, "Content", Path.Combine(relativeToContentPath));

            if (Directory.Exists(subContentPath))
            {
                callback.Invoke(ContentOriginType.SubContent, subContentPath, Directory.GetFiles(subContentPath, searchPattern, searchOption));
            }
        }
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

            SoundHandler.SoundEffectGroup soundEffectGroup = new(key, volume, [.. variations]);
            if (!soundEffectGroup.IsValid) continue;

            result.Add(soundEffectGroup);
        }

        return result;
    }

    internal static void LoadTexture(string filePath)
    {
        string textureName = Path.GetFileNameWithoutExtension(filePath);
        if (Textures.m_tileTextures.TextureExists(textureName))
        {
            Texture2D existingTexture = Textures.GetTexture(textureName);
            if (existingTexture != null && !existingTexture.IsDisposed)
            {
                Textures.m_tileTextures.RemoveTexture(textureName);
            }
            else if (TileTextures.m_alreadyLoadedFiles.Contains(filePath))
            {
                return;
            }
        }

        TileTextures.m_alreadyLoadedFiles.Add(filePath);

        Texture2D texture = null;
        Utils.WaitForGraphicsDevice();

        try
        {
            lock (GameSFD.SpriteBatchResourceObject)
            {
                texture = Content.Load<Texture2D>(filePath);
            }
        }
        catch
        {
            texture = null;

            try
            {
                lock (GameSFD.SpriteBatchResourceObject)
                {
                    texture = Content.Load<Texture2DB>(filePath).Texture;
                }
            }
            catch
            {
                texture = null;

                ConsoleOutput.ShowMessage(ConsoleOutputType.Error, string.Format("Failed to load texture {0}", textureName));
            }
        }

        if (texture != null) Textures.m_tileTextures.AddTexture(texture, textureName.ToUpperInvariant());
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

        Dictionary<string, AnimationData> animations = [];

        EnumerateContentFiles((ContentOriginType originType, string folderPath, string[] filePaths) =>
        {
            Logger.LogInfo($"LOADING [ANIMATIONS]: {originType}");

            foreach (var filePath in filePaths)
            {
                ConsoleOutput.ShowMessage(ConsoleOutputType.Loading, $"Loading animations file '{filePath}'");
                AnimationsData animationsData = Content.Load<AnimationsData>(filePath);

                CopyAndReplaceDictionary(animationsData.DicAnimations, animations);
            }
        }, ANIMATIONS_FILE_NAME, SearchOption.AllDirectories, Constants.Paths.DATA_ANIMATIONS);

        foreach (var animation in animations.Values)
        {
            if (analyzeDataDic.ContainsKey(animation.Name) && !AnalyzeAnimation(animation, analyzeDataDic[animation.Name]))
            {
                SFD.Program.ShowError(new Exception("Core animation data file has been modified in an unintended way. Restore your char_anims file."), "Core animation data modified!", true);
                __result = false;
                return false;
            }
        }

        Animations.Data = new([.. animations.Values]);
        __result = true;
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Textures), nameof(Textures.Load), [typeof(string)])]
    private static bool Textures_Load_Prefix_OverrideLoad()
    {
        if (!SFDCTConfig.Get<bool>(CTSettingKey.SubContent)) return true;

        Dictionary<string, string> textureFiles = [];

        EnumerateContentFiles((ContentOriginType originType, string folderPath, string[] filePaths) =>
        {
            Logger.LogInfo($"LOADING [TEXTURES]: {originType}");

            foreach (var filePath in filePaths)
            {
                AddOrSetDictionaryValue(textureFiles, Path.GetFileNameWithoutExtension(filePath), filePath);
            }
        }, "*.png", SearchOption.AllDirectories, Constants.Paths.DATA_IMAGES);

        foreach (var filePath in textureFiles.Values)
        {
            ConsoleOutput.ShowMessage(ConsoleOutputType.Loading, $"Loading texture: {filePath}");

            LoadTexture(filePath);
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
        Items.m_slotAllItems = new List<Item>[10];
        Items.m_slotFemaleItems = new List<Item>[10];
        Items.m_slotMaleItems = new List<Item>[10];
        for (int i = 0; i < 10; i++)
        {
            Items.m_slotAllItems[i] = [];
            Items.m_slotFemaleItems[i] = [];
            Items.m_slotMaleItems[i] = [];
        }

        Dictionary<string, Item> items = [];

        EnumerateContentFiles((ContentOriginType originType, string folderPath, string[] filePaths) =>
        {
            Logger.LogInfo($"LOADING [ITEMS]: {originType}");

            switch (originType)
            {
                case ContentOriginType.Official:
                    foreach (var filePath in filePaths)
                    {
                        ConsoleOutput.ShowMessage(ConsoleOutputType.Loading, $"Loading equipment file '{filePath}'");

                        Item item = Content.Load<Item>(filePath);
                        if (items.ContainsKey(item.ID))
                        {
                            throw new Exception($"Error: Item ID collision between item '{items[item.ID]}' and '{item}' while loading '{filePath}'");
                        }

                        items.Add(item.ID, item);
                    }
                    break;
                case ContentOriginType.Documents:
                case ContentOriginType.SubContent:
                    foreach (var filePath in filePaths)
                    {
                        ConsoleOutput.ShowMessage(ConsoleOutputType.Loading, $"Loading equipment file '{filePath}'");

                        Item item = Content.Load<Item>(filePath);
                        AddOrSetDictionaryValue(items, item.ID, item);
                    }
                    break;
            }
        }, "*.item", SearchOption.AllDirectories, Constants.Paths.DATA_ITEMS);


        ConsoleOutput.ShowMessage(ConsoleOutputType.Loading, $"Post processing equipment");

        foreach (var item in items.Values)
        {
            item.PostProcess();

            Items.m_allItems.Add(item);
            Items.m_slotAllItems[item.EquipmentLayer].Add(item);
        }

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

        Dictionary<string, ColorPalette> colorPalettes = [];

        EnumerateContentFiles((ContentOriginType originType, string folderPath, string[] filePaths) =>
        {
            Logger.LogInfo($"LOADING [COLOR-PALETTES]: {originType}");

            foreach (var filePath in filePaths)
            {
                SFDXReader.ReadDataFromSFDXFile(filePath);
            }

            CopyAndReplaceDictionary(ColorPaletteDatabase.m_palettes, colorPalettes);
            ColorPaletteDatabase.m_palettes.Clear();
        }, "*.sfdx", SearchOption.TopDirectoryOnly, Constants.Paths.DATA_COLORS_PALETTES);

        ColorPaletteDatabase.m_palettes = colorPalettes;

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ColorDatabase), nameof(ColorDatabase.Load), [typeof(GameSFD)])]
    private static bool ColorDatabase_Load_Prefix_OverrideLoad()
    {
        if (!SFDCTConfig.Get<bool>(CTSettingKey.SubContent)) return true;

        Dictionary<string, Color[]> colors = [];

        EnumerateContentFiles((ContentOriginType originType, string folderPath, string[] filePaths) =>
        {
            Logger.LogInfo($"LOADING [COLOR]: {originType}");

            switch (originType)
            {
                case ContentOriginType.Official:
                    foreach (var filePath in filePaths)
                    {
                        SFDXReader.ReadDataFromSFDXFile(filePath);
                    }
                    break;
                case ContentOriginType.Documents:
                case ContentOriginType.SubContent:
                    foreach (var filePath in filePaths)
                    {
                        SFDXReader.ReadDataFromSFDXFile(filePath);
                    }
                    break;
            }

            CopyAndReplaceDictionary(ColorDatabase.m_colors, colors);
            ColorDatabase.m_colors.Clear();
        }, "*.sfdx", SearchOption.TopDirectoryOnly, Constants.Paths.DATA_COLORS_COLORS);

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

        Dictionary<string, SoundHandler.SoundEffectGroup> sounds = [];
        int soundFileCount = 0;
        EnumerateContentFiles((ContentOriginType originType, string folderPath, string[] filePaths) =>
        {
            Logger.LogInfo($"LOADING [SOUNDS]: {originType}");
            soundFileCount++;

            switch (originType)
            {
                case ContentOriginType.Official:
                    foreach (var filePath in filePaths)
                    {
                        var soundGroups = LoadSoundEffectGroups(folderPath, filePath);

                        if (soundGroups == null) continue;
                        foreach (var group in soundGroups)
                        {
                            if (sounds.ContainsKey(group.Key))
                            {
                                throw new Exception($"Error: Invalid sound key '{group.Key}' - it's already taken");
                            }
                            else
                            {
                                sounds.Add(group.Key, group);
                            }
                        }
                    }
                    break;
                case ContentOriginType.Documents:
                case ContentOriginType.SubContent:
                    foreach (var filePath in filePaths)
                    {
                        var soundGroups = LoadSoundEffectGroups(folderPath, filePath);

                        if (soundGroups == null) continue;
                        foreach (var group in soundGroups)
                        {
                            AddOrSetDictionaryValue(sounds, group.Key, group);
                        }
                    }
                    break;
            }
        }, "*.sfds", SearchOption.TopDirectoryOnly, Constants.Paths.DATA_SOUNDS);

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

        Dictionary<string, Tile> tiles = [];

        EnumerateContentFiles((ContentOriginType originType, string folderPath, string[] filePaths) =>
        {
            Logger.LogInfo($"LOADING [TILES]: {originType}");

            foreach (var filePath in filePaths)
            {
                SFDXReader.ReadDataFromSFDXFile(filePath);
            }

            CopyAndReplaceDictionary(TileDatabase.m_tiles, tiles);
            TileDatabase.m_tiles.Clear();
            TileDatabase.m_categorizedTiles.Clear();
        }, "*.sfdx", SearchOption.TopDirectoryOnly, Constants.Paths.DATA_TILES);

        EnumerateContentFiles((ContentOriginType originType, string folderPath, string[] filePaths) =>
        {
            Logger.LogInfo($"LOADING [WEAPON-TILES]: {originType}");

            foreach (var filePath in filePaths)
            {
                SFDXReader.ReadDataFromSFDXFile(filePath);
            }

            CopyAndReplaceDictionary(TileDatabase.m_tiles, tiles);
            TileDatabase.m_tiles.Clear();
            TileDatabase.m_categorizedTiles.Clear();
        }, "*.sfdx", SearchOption.TopDirectoryOnly, Constants.Paths.DATA_WEAPONS);

        foreach (var tile in tiles.Values)
        {
            TileDatabase.Add(tile);
        }

        TileDatabase.Add(new Tile(TileStructure.GetPlayerTileStructure()), true);

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(BackgroundImage), nameof(BackgroundImage.Load))]
    private static bool BackgroundImage_Load_Prefix_OverrideLoad()
    {
        if (!SFDCTConfig.Get<bool>(CTSettingKey.SubContent)) return true;

        if (BackgroundImage.m_image != null && !BackgroundImage.m_image.IsDisposed)
        {
            BackgroundImage.m_image.Dispose();
            BackgroundImage.m_image = null;
        }

        string SFDjpgImagePath = null;

        EnumerateContentFiles((ContentOriginType originType, string folderPath, string[] filePaths) =>
        {
            Logger.LogInfo($"LOADING [BACKGROUND-IMAGE]: {originType}");

            if (filePaths.Length > 0)
            {
                SFDjpgImagePath = filePaths.Last();
            }
        }, "SFD.jpg", SearchOption.TopDirectoryOnly, Constants.Paths.DATA_MISC);

        if (SFDjpgImagePath != null && File.Exists(SFDjpgImagePath))
        {
            try
            {
                ConsoleOutput.ShowMessage(ConsoleOutputType.Loading, $"Loading background image: {SFDjpgImagePath}");

                using FileStream fileStream = File.OpenRead(SFDjpgImagePath);

                Utils.WaitForGraphicsDevice();
                BackgroundImage.m_image = Texture2D.FromStream(GameSFD.Handle.GraphicsDevice, fileStream);

                fileStream.Close();
            }
            catch (Exception ex)
            {
                ConsoleOutput.ShowMessage(ConsoleOutputType.Error, $"file '{SFDjpgImagePath}' could not be loaded: " + ex.Message);
            }
        }

        return false;
    }
}
