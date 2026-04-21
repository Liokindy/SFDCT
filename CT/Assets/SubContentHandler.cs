using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SFD;
using SFD.Colors;
using SFD.Loading;
using SFD.Sounds;
using SFD.Tiles;
using SFDCT.Configuration;
using SFDCT.Helper;
using SFDCT.Misc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Emit;

namespace SFDCT.Assets;

[HarmonyPatch]
internal static class SubContentHandler
{
    internal const char FOLDER_SETTING_SEPARATOR = '|';

    internal static bool IsEnabled()
    {
        return SFDCTConfig.Get<bool>(CTSettingKey.SubContent);
    }

    internal static void Load()
    {
        if (!IsEnabled()) return;

        var enabledFolders = GetEnabled().ToList();
        var disabledFolders = GetDisabled().ToList();

        var deletedFolders = new List<string>();
        deletedFolders.AddRange(enabledFolders);
        deletedFolders.AddRange(disabledFolders);
        deletedFolders = deletedFolders.Where(p => !Directory.Exists(Path.Combine(Globals.Paths.SubContent, p))).ToList();

        foreach (string folder in deletedFolders)
        {
            disabledFolders.Remove(folder);
            enabledFolders.Remove(folder);

            ConsoleOutput.ShowMessage(ConsoleOutputType.Loading, $"SFDCT: Removing deleted sub-content folder: {folder}");
        }

        foreach (string folder in GetNew())
        {
            ConsoleOutput.ShowMessage(ConsoleOutputType.Loading, $"SFDCT: Adding new sub-content folder: {folder}");

            enabledFolders.Add(folder);
        }

        SFDCTConfig.Set(CTSettingKey.SubContentEnabledFolders, JoinFoldersInSettingLine(enabledFolders));
        SFDCTConfig.Set(CTSettingKey.SubContentDisabledFolders, JoinFoldersInSettingLine(disabledFolders));
        SFDCTConfig.Save();
    }

    internal static IEnumerable<string> SplitFoldersInSettingLine(string line)
    {
        return line.Split([FOLDER_SETTING_SEPARATOR], StringSplitOptions.RemoveEmptyEntries);
    }

    internal static string JoinFoldersInSettingLine(IEnumerable<string> folders)
    {
        return string.Join(FOLDER_SETTING_SEPARATOR.ToString(), folders);
    }

    internal static IEnumerable<string> GetNew()
    {
        var knownFolders = GetEnabled().Concat(GetDisabled());

        return GetAll()
                .Where(p => !knownFolders.Contains(p) && !knownFolders.Contains(p));
    }

    internal static IEnumerable<string> GetAll()
    {
        return Directory.EnumerateDirectories(Globals.Paths.SubContent, "*", SearchOption.TopDirectoryOnly)
                .Where(d => Directory.Exists(Path.Combine(d, Constants.Paths.DATA)));
    }

    internal static IEnumerable<string> GetEnabled()
    {
        return SplitFoldersInSettingLine(SFDCTConfig.Get<string>(CTSettingKey.SubContentEnabledFolders));
    }

    internal static IEnumerable<string> GetDisabled()
    {
        return SplitFoldersInSettingLine(SFDCTConfig.Get<string>(CTSettingKey.SubContentDisabledFolders));
    }

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

        foreach (string subContentFolder in GetEnabled())
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

    internal static IEnumerable<ContentData> GetContents()
    {
        var contents = new List<ContentData>();

        var officialContent = new ContentData()
        {
            Directory = Path.Combine(Constants.Paths.ContentPath),
            OriginType = ContentOriginType.Official,
        };

        var documentsContent = new ContentData()
        {
            Directory = Path.Combine(Constants.Paths.UserDocumentsContentCustomPath),
            OriginType = ContentOriginType.Documents
        };

        contents.Add(officialContent);
        contents.Add(documentsContent);

        foreach (var folder in GetEnabled())
        {
            var subContent = new ContentData()
            {
                Directory = Path.Combine(Globals.Paths.SubContent, folder),
                OriginType = ContentOriginType.SubContent
            };

            contents.Add(subContent);
        }

        return contents;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Animations), nameof(Animations.Load))]
    private static bool Animations_Load_Prefix_OverrideLoad(ref bool __result, GameSFD game)
    {
        if (!IsEnabled()) return true;

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        __result = AnimationsLoader.Load(game);

        //var analyzeAnimationsArray = new Animations.AnalyzeAnimation[]
        //{
        //    new("BaseKick",                 [25, 50, 50, 50, 100, 100],     ["STEP", "", "", "MELEESWING", "KICK", "STOP"]),
        //    new("FullCharge",               [500, 100, 100, 100, 100, 100], ["TELEGRAPH", "", "", "", "", ""]),
        //    new("FullChargeA",              [500],                          ["TELEGRAPH"]),
        //    new("FullChargeB",              [100, 100, 100, 100],           ["", "", "", ""]),
        //    new("FullRoll",                 [100, 100, 100, 100, 100],      ["", "", "", "", ""]),
        //    new("UpperBlock",               [50, 50, 200],                  ["", "", "STOP"]),
        //    new("UpperBlockChainsaw",       [50, 50, 200],                  ["", "", "STOP"]),
        //    new("UpperBlockMelee",          [50, 50, 200],                  ["", "", "STOP"]),
        //    new("UpperBlockMelee2H",        [50, 50, 200],                  ["", "", "STOP"]),
        //    new("UpperBlockMelee2HEnd",     [200],                          [""]),
        //    new("UpperBlockMeleeEnd",       [200],                          [""]),
        //    new("UpperMelee1H1",            [100, 50, 25, 75, 250],         ["", "", "MELEESWING", "HIT", "STOP"]),
        //    new("UpperMelee1H1End",         [75, 200],                      ["", "STOP"]),
        //    new("UpperMelee1H2",            [150, 50, 25, 75, 250],         ["", "", "MELEESWING", "HIT", "STOP"]),
        //    new("UpperMelee1H2End",         [75, 200],                      ["", "STOP"]),
        //    new("UpperMelee1H3",            [200, 50, 25, 75, 300],         ["", "", "MELEESWING", "HIT", "STOP"]),
        //    new("UpperMelee1H3Chain",       [200, 50, 25, 75, 300],         ["", "", "MELEESWING", "HIT", "STOP"]),
        //    new("UpperMelee1H3ChainEnd",    [75, 300],                      ["", "STOP"]),
        //    new("UpperMelee1H3End",         [75, 300],                      ["", "STOP"]),
        //    new("UpperMelee1H4",            [100, 50, 50, 50, 250],         ["", "", "MELEESWING", "HIT", "STOP"]),
        //    new("UpperMelee2H1",            [100, 50, 25, 75, 250],         ["", "", "MELEESWING", "HIT", "STOP"]),
        //    new("UpperMelee2H1End",         [75, 250],                      ["", "STOP"]),
        //    new("UpperMelee2H2",            [150, 50, 25, 75, 250],         ["", "", "MELEESWING", "HIT", "STOP"]),
        //    new("UpperMelee2H2End",         [75, 250],                      ["", "STOP"]),
        //    new("UpperMelee2H3",            [200, 50, 25, 75, 300],         ["", "", "MELEESWING", "HIT", "STOP"]),
        //    new("UpperMelee2H3End",         [75, 300],                      ["", "STOP"]),
        //    new("UpperMelee2H4",            [100, 50, 50, 100],             ["", "MELEESWING", "HIT", "STOP"]),
        //    new("UpperMeleeHit1",           [50, 50],                       ["", "STOP"]),
        //    new("UpperMeleeHit2",           [50, 50],                       ["", "STOP"]),
        //    new("UpperPunch1",              [150, 25, 75, 250],             ["", "MELEESWING", "HIT", "STOP"]),
        //    new("UpperPunch2",              [150, 25, 75, 250],             ["", "MELEESWING", "HIT", "STOP"]),
        //    new("UpperPunch3",              [200, 25, 75, 50, 250],         ["", "MELEESWING", "HIT", "", "STOP"]),
        //    new("UpperPunch4",              [100, 25, 25, 200],             ["", "", "MELEESWING_HIT", "STOP"])
        //};

        //var animationsContent = GetContentFiles("char_anims", SearchOption.AllDirectories, Constants.Paths.DATA_ANIMATIONS);
        //var currentContent = 1;
        //var totalContent = animationsContent.Count();
        //var animations = new Dictionary<string, AnimationData>();

        //foreach (var kvp in animationsContent)
        //{
        //    Logger.LogInfo($"LOADING [ANIMATIONS]: {kvp.Key}");
        //    GameSFD.Handle.ShowLoadingText($"{LanguageHelper.GetText("loading.animations")} ({currentContent}/{totalContent})");

        //    int current = 0;
        //    int total = kvp.Value.Length;

        //    foreach (var path in kvp.Value)
        //    {
        //        ConsoleOutput.ShowMessage(ConsoleOutputType.Loading, $"Loading animations file: {path}");

        //        AnimationsData animationsData = Content.Load<AnimationsData>(path);
        //        CopyAndReplaceDictionary(animationsData.DicAnimations, animations);

        //        current++;
        //    }

        //    currentContent++;
        //}

        //foreach (var analyzeData in analyzeAnimationsArray)
        //{
        //    if (!animations.ContainsKey(analyzeData.Animation)) continue;

        //    var animationData = animations[analyzeData.Animation];
        //    var analyzeResult = true;

        //    if (animationData.Frames.Length != analyzeData.FrameTimes.Length)
        //    {
        //        analyzeResult = false;
        //    }
        //    else
        //    {
        //        for (int j = 0; j < animationData.Frames.Length; j++)
        //        {
        //            if (animationData.Frames[j].Time != analyzeData.FrameTimes[j] || animationData.Frames[j].Event != analyzeData.FrameEvents[j])
        //            {
        //                analyzeResult = false;
        //                break;
        //            }
        //        }
        //    }

        //    if (!analyzeResult)
        //    {
        //        SFD.Program.ShowError(new Exception("Core animation data file has been modified in an unintended way. Restore your char_anims file."), "Core animation data modified!", true);

        //        __result = false;
        //        return false;
        //    }
        //}

        //Animations.Data = new(animations.Values.ToArray());

        //__result = true;

        stopwatch.Stop();
        Logger.LogDebug($"Loaded animations in {stopwatch.ElapsedMilliseconds}ms");
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Textures), nameof(Textures.Load), [])]
    private static bool Textures_Load_Prefix_OverrideLoad()
    {
        if (!IsEnabled()) return true;

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        TexturesLoader.Load(GameSFD.Handle);

        //// Reverse the order of textureContents because we use the default load
        //// method, it checks if the texture names exist before adding them
        //var textureContents = GetContentFiles("*.png", SearchOption.AllDirectories, Constants.Paths.DATA_IMAGES).Reverse();
        //var currentContent = 1;
        //var totalContent = textureContents.Count();

        //foreach (var kvp in textureContents)
        //{
        //    Logger.LogInfo($"LOADING [TEXTURES]: {kvp.Key}");
        //    GameSFD.Handle.ShowLoadingText($"{LanguageHelper.GetText("loading.textures")} ({currentContent}/{totalContent})");

        //    int current = 0;
        //    int total = kvp.Value.Length;

        //    foreach (var path in kvp.Value)
        //    {
        //        ConsoleOutput.ShowMessage(ConsoleOutputType.Loading, $"Loading texture file: {path}");

        //        Textures.m_tileTextures.Load(path, kvp.Key != ContentOriginType.Official);

        //        current++;
        //    }


        //    currentContent++;
        //}

        stopwatch.Stop();
        Logger.LogDebug($"Loaded textures in {stopwatch.ElapsedMilliseconds}ms");
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Items), nameof(Items.Load), [typeof(GameSFD)])]
    private static bool Items_Load_Prefix_OverrideLoad(GameSFD game)
    {
        if (!IsEnabled()) return true;

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        ItemsLoader.Load(game);

        //Items.m_allItems = [];
        //Items.m_allFemaleItems = [];
        //Items.m_allMaleItems = [];

        //Items.m_slotAllItems = new List<Item>[Equipment.TOTAL_INTERNAL_LAYERS];
        //Items.m_slotFemaleItems = new List<Item>[Equipment.TOTAL_INTERNAL_LAYERS];
        //Items.m_slotMaleItems = new List<Item>[Equipment.TOTAL_INTERNAL_LAYERS];
        //for (int i = 0; i < Equipment.TOTAL_INTERNAL_LAYERS; i++)
        //{
        //    Items.m_slotAllItems[i] = [];
        //    Items.m_slotFemaleItems[i] = [];
        //    Items.m_slotMaleItems[i] = [];
        //}

        //var itemLoadedIDs = new HashSet<string>();
        //var itemContents = GetContentFiles("*.item", SearchOption.AllDirectories, Constants.Paths.DATA_ITEMS);
        //var currentContent = 1;
        //var totalContent = itemContents.Count();

        //foreach (var kvp in itemContents)
        //{
        //    Logger.LogInfo($"LOADING [ITEMS]: {kvp.Key}");
        //    GameSFD.Handle.ShowLoadingText($"{LanguageHelper.GetText("loading.equipment")} ({currentContent}/{totalContent})");

        //    int current = 0;
        //    int total = kvp.Value.Length;

        //    foreach (var path in kvp.Value)
        //    {
        //        if (GameSFD.Closing) return false;

        //        ConsoleOutput.ShowMessage(ConsoleOutputType.Loading, $"Loading item file: {path}");

        //        Item item = Content.Load<Item>(path);
        //        item.PostProcess();

        //        current++;
        //        if (!itemLoadedIDs.Add(item.ID) && kvp.Key == ContentOriginType.Official)
        //        {
        //            foreach (Item item2 in Items.m_allItems)
        //            {
        //                if (item2.ID == item.ID)
        //                {
        //                    throw new Exception($"Error: Item ID collision between item '{item2}' and '{item}' while loading '{path}'");
        //                }
        //            }

        //            throw new Exception($"Error: Item ID collision, item with ID '{item.ID}' has already been loaded, cannot load item '{item}' from '{path}'");
        //        }

        //        Items.m_allItems.Add(item);
        //        Items.m_slotAllItems[item.EquipmentLayer].Add(item);
        //    }


        //    currentContent++;
        //}


        //Items.PostProcessGenders();

        //Player.HurtLevel1 = Items.GetItem("HurtLevel1");
        //Player.HurtLevel2 = Items.GetItem("HurtLevel2");
        //Player.HurtLevel2 ??= Player.HurtLevel1;

        //Items.IsLoaded = true;

        stopwatch.Stop();
        Logger.LogDebug($"Loaded items in {stopwatch.ElapsedMilliseconds}ms");
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ColorPaletteDatabase), nameof(ColorPaletteDatabase.Load), [typeof(GameSFD)])]
    private static bool ColorPaletteDatabase_Load_Prefix_OverrideLoad()
    {
        if (!IsEnabled()) return true;

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var totalColorPalettes = new Dictionary<string, ColorPalette>();
        var contents = GetContents()
                        .Where(content => Directory.Exists(Path.Combine(content.Directory, Constants.Paths.DATA_COLORS_PALETTES)))
                        .Reverse();

        foreach (var content in contents)
        {
            // need to specify TopDirectoryOnly because otherwise vanilla official content breaks

            foreach (var path in Directory.EnumerateFiles(Path.Combine(content.Directory, Constants.Paths.DATA_COLORS_PALETTES), "*.sfdx", SearchOption.TopDirectoryOnly))
            {
                ConsoleOutput.ShowMessage(ConsoleOutputType.Loading, $"Loading color palettes from file '{path}'");

                DatabaseLoader.Load(path, ref ColorPaletteDatabase.m_palettes, ref totalColorPalettes);
            }
        }

        ColorPaletteDatabase.m_palettes = totalColorPalettes;

        //var colorPalettes = new Dictionary<string, ColorPalette>();
        //var colorPaletteContents = GetContentFiles("*.sfdx", SearchOption.TopDirectoryOnly, Constants.Paths.DATA_COLORS_PALETTES);
        //var currentContent = 1;
        //var totalContent = colorPaletteContents.Count();

        //foreach (var kvp in colorPaletteContents)
        //{
        //    Logger.LogInfo($"LOADING [COLOR-PALETTES]: {kvp.Key}");
        //    GameSFD.Handle.ShowLoadingText($"{LanguageHelper.GetText("loading.colors")} ({currentContent}/{totalContent})");

        //    foreach (var path in kvp.Value)
        //    {
        //        ConsoleOutput.ShowMessage(ConsoleOutputType.Loading, $"Loading color-palette file: {path}");

        //        SFDXReader.ReadDataFromSFDXFile(path);

        //        CopyAndReplaceDictionary(ColorPaletteDatabase.m_palettes, colorPalettes);
        //        ColorPaletteDatabase.m_palettes.Clear();
        //    }

        //    currentContent++;
        //}

        //ColorPaletteDatabase.m_palettes = colorPalettes;

        stopwatch.Stop();
        Logger.LogDebug($"Loaded color palettes in {stopwatch.ElapsedMilliseconds}ms");
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ColorDatabase), nameof(ColorDatabase.Load), [typeof(GameSFD)])]
    private static bool ColorDatabase_Load_Prefix_OverrideLoad()
    {
        if (!IsEnabled()) return true;

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var totalColors = new Dictionary<string, Color[]>();
        var contents = GetContents()
                        .Where(content => Directory.Exists(Path.Combine(content.Directory, Constants.Paths.DATA_COLORS_COLORS)))
                        .Reverse();

        foreach (var content in contents)
        {
            // need to specify TopDirectoryOnly because otherwise vanilla official content breaks

            foreach (var path in Directory.EnumerateFiles(Path.Combine(content.Directory, Constants.Paths.DATA_COLORS_COLORS), "*.sfdx", SearchOption.TopDirectoryOnly))
            {
                ConsoleOutput.ShowMessage(ConsoleOutputType.Loading, $"Loading colors from file '{path}'");

                DatabaseLoader.Load(path, ref ColorDatabase.m_colors, ref totalColors);
            }
        }

        ColorDatabase.m_colors = totalColors;

        //var colors = new Dictionary<string, Color[]>();
        //var colorsContents = GetContentFiles("*.sfdx", SearchOption.TopDirectoryOnly, Constants.Paths.DATA_COLORS_COLORS);
        //var currentContent = 1;
        //var totalContent = colorsContents.Count();

        //foreach (var kvp in colorsContents)
        //{
        //    Logger.LogInfo($"LOADING [COLORS]: {kvp.Key}");
        //    GameSFD.Handle.ShowLoadingText($"{LanguageHelper.GetText("loading.colors")} ({currentContent}/{totalContent})");

        //    foreach (var path in kvp.Value)
        //    {
        //        ConsoleOutput.ShowMessage(ConsoleOutputType.Loading, $"Loading color file: {path}");

        //        SFDXReader.ReadDataFromSFDXFile(path);

        //        CopyAndReplaceDictionary(ColorDatabase.m_colors, colors);
        //        ColorDatabase.m_colors.Clear();
        //    }

        //    currentContent++;
        //}

        //ColorDatabase.m_colors = colors;

        stopwatch.Stop();
        Logger.LogDebug($"Loaded colors in {stopwatch.ElapsedMilliseconds}ms");
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(SoundHandler), nameof(SoundHandler.Load), [typeof(GameSFD)])]
    private static bool SoundHandler_Load_Prefix_OverrideLoad(GameSFD game)
    {
        if (!IsEnabled()) return true;

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        SoundsLoader.Load(game);

        //ConsoleOutput.ShowMessage(ConsoleOutputType.Loading, "Loading sounds...");
        //SoundHandler.game = game;
        //SoundHandler.soundEffects = new SoundHandler.SoundEffectGroups();
        //SoundHandler.m_recentlyPlayedSoundClassPool = new GenericClassPool<SoundHandler.RecentlyPlayedSound>(() => new SoundHandler.RecentlyPlayedSound(), 1, 0);

        //for (int i = 0; i < 30; i++)
        //{
        //    SoundHandler.RecentlyPlayedSound recentlyPlayedSound = SoundHandler.m_recentlyPlayedSoundClassPool.GetFreeItem();

        //    recentlyPlayedSound.InUse = false;
        //    SoundHandler.m_recentlyPlayedSoundClassPool.FlagFreeItem(recentlyPlayedSound);
        //}

        //var soundContents = GetContentFiles("*.sfds", SearchOption.TopDirectoryOnly, Constants.Paths.DATA_SOUNDS);
        //var sounds = new Dictionary<string, SoundHandler.SoundEffectGroup>();
        //var soundFileCount = 0;

        //foreach (var kvp in soundContents)
        //{
        //    Logger.LogInfo($"LOADING [SOUNDS]: {kvp.Key}");

        //    if (kvp.Key == ContentOriginType.Official)
        //    {
        //        foreach (var path in kvp.Value)
        //        {
        //            ConsoleOutput.ShowMessage(ConsoleOutputType.Loading, $"Loading sound definitions file: {path}");

        //            var folderPath = Path.GetDirectoryName(path);
        //            var soundGroups = LoadSoundEffectGroups(folderPath, path);

        //            if (soundGroups == null) continue;

        //            foreach (var group in soundGroups)
        //            {
        //                if (sounds.ContainsKey(group.Key))
        //                {
        //                    throw new Exception($"Error: Invalid sound key '{group.Key}' - it's already taken");
        //                }

        //                sounds.Add(group.Key, group);
        //            }
        //        }
        //    }
        //    else
        //    {
        //        foreach (var path in kvp.Value)
        //        {
        //            ConsoleOutput.ShowMessage(ConsoleOutputType.Loading, $"Loading sound definitions file: {path}");

        //            var folderPath = Path.GetDirectoryName(path);
        //            var soundGroups = LoadSoundEffectGroups(folderPath, path);

        //            if (soundGroups == null) continue;

        //            foreach (var group in soundGroups)
        //            {
        //                sounds[group.Key] = group;
        //            }
        //        }
        //    }

        //    soundFileCount++;
        //}

        //ConsoleOutput.ShowMessage(ConsoleOutputType.Loading, "Loading sounds finilizing");

        //foreach (var soundEffectGroup in sounds.Values)
        //{
        //    if (soundEffectGroup.Key == "CHAINSAW") SoundHandler.GlobalLoopSounds.Chainsaw = new SoundHandler.GlobalLoopSound(soundEffectGroup.Key, soundEffectGroup.SoundEffects[0].CreateInstance(), soundEffectGroup.VolumeModifier);
        //    if (soundEffectGroup.Key == "STREETSWEEPERPROPELLER") SoundHandler.GlobalLoopSounds.StreetsweeperPropeller = new SoundHandler.GlobalLoopSound(soundEffectGroup.Key, soundEffectGroup.SoundEffects[0].CreateInstance(), soundEffectGroup.VolumeModifier);

        //    SoundHandler.soundEffects.Add(soundEffectGroup);
        //    ConsoleOutput.ShowMessage(ConsoleOutputType.Loading, $"Sound added: {soundEffectGroup.Key}");
        //}

        //if (soundFileCount == 0 || SoundHandler.soundEffects.Count == 0)
        //{
        //    SoundHandler.m_soundsDisabled = true;
        //}

        //ConsoleOutput.ShowMessage(ConsoleOutputType.Loading, "Loading sounds completed");

        stopwatch.Stop();
        Logger.LogDebug($"Loaded sounds in {stopwatch.ElapsedMilliseconds}ms");
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(TileDatabase), nameof(TileDatabase.Load), [typeof(GameSFD)])]
    private static bool TileDatabase_Load_Prefix_OverrideLoad()
    {
        if (!IsEnabled()) return true;

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var totalTiles = new Dictionary<string, Tile>();
        var contents = GetContents()
                        .Where(content =>
                            Directory.Exists(Path.Combine(content.Directory, Constants.Paths.DATA_TILES)) ||
                            Directory.Exists(Path.Combine(content.Directory, Constants.Paths.DATA_WEAPONS)))
                        .Reverse();

        foreach (var content in contents)
        {
            // tiles are divided into 2 separate folders,
            // the main tile folders and the tiles reserved for weapons

            var contentTilesFolderPath = Path.Combine(content.Directory, Constants.Paths.DATA_TILES);
            var contentWeaponTilesFolderPath = Path.Combine(content.Directory, Constants.Paths.DATA_WEAPONS);

            // need to specify TopDirectoryOnly because otherwise vanilla official content breaks
            if (Directory.Exists(contentTilesFolderPath))
            {
                foreach (var path in Directory.EnumerateFiles(contentTilesFolderPath, "*.sfdx", SearchOption.TopDirectoryOnly))
                {
                    ConsoleOutput.ShowMessage(ConsoleOutputType.Loading, $"Loading tiles from file '{path}'");

                    DatabaseLoader.Load(path, ref TileDatabase.m_tiles, ref totalTiles);
                }
            }

            if (Directory.Exists(contentWeaponTilesFolderPath))
            {
                foreach (var path in Directory.EnumerateFiles(contentWeaponTilesFolderPath, "*.sfdx", SearchOption.TopDirectoryOnly))
                {
                    ConsoleOutput.ShowMessage(ConsoleOutputType.Loading, $"Loading tiles from file '{path}'");

                    DatabaseLoader.Load(path, ref TileDatabase.m_tiles, ref totalTiles);
                }
            }

            // loading tiles also adds entries to TileDatabase.m_categorizedTiles,
            // this is handled later by individually adding back all the found unique tiles
            TileDatabase.m_categorizedTiles.Clear();
        }

        // this is so TileDatabase.m_categorizedTiles gets properly constructed again
        foreach (var tile in totalTiles.Values)
        {
            TileDatabase.Add(tile);
        }

        // the player tile needs to override any conflicting tile
        var playerTile = new Tile(TileStructure.GetPlayerTileStructure());
        TileDatabase.Remove(playerTile.Key);
        TileDatabase.Add(playerTile, true);

        //var tiles = new Dictionary<string, Tile>();
        //var tilesContents = GetContentFiles("*.sfdx", SearchOption.TopDirectoryOnly, Constants.Paths.DATA_TILES);
        //var weaponTilesContents = GetContentFiles("*.sfdx", SearchOption.TopDirectoryOnly, Constants.Paths.DATA_WEAPONS);

        //int currentContent = 1;
        //int totalContent = tilesContents.Count();

        //foreach (var kvp in tilesContents)
        //{
        //    Logger.LogInfo($"LOADING [TILES]: {kvp.Key}");
        //    GameSFD.Handle.ShowLoadingText($"{LanguageHelper.GetText("loading.tiles")} ({currentContent}/{totalContent})");

        //    foreach (var path in kvp.Value)
        //    {
        //        ConsoleOutput.ShowMessage(ConsoleOutputType.Loading, $"Loading tile file: {path}");

        //        SFDXReader.ReadDataFromSFDXFile(path);

        //        CopyAndReplaceDictionary(TileDatabase.m_tiles, tiles);
        //        TileDatabase.m_tiles.Clear();
        //        TileDatabase.m_categorizedTiles.Clear();
        //    }

        //    currentContent++;
        //}

        //currentContent = 1;
        //totalContent = weaponTilesContents.Count();

        //foreach (var kvp in weaponTilesContents)
        //{
        //    Logger.LogInfo($"LOADING [WEAPON-TILES]: {kvp.Key}");
        //    GameSFD.Handle.ShowLoadingText($"{LanguageHelper.GetText("loading.tiles")} ({currentContent}/{totalContent})");

        //    foreach (var path in kvp.Value)
        //    {
        //        ConsoleOutput.ShowMessage(ConsoleOutputType.Loading, $"Loading weapon tile file: {path}");

        //        SFDXReader.ReadDataFromSFDXFile(path);
        //    }

        //    CopyAndReplaceDictionary(TileDatabase.m_tiles, tiles);
        //    TileDatabase.m_tiles.Clear();
        //    TileDatabase.m_categorizedTiles.Clear();

        //    currentContent++;
        //}

        //foreach (var tile in tiles.Values)
        //{
        //    TileDatabase.Add(tile);
        //}

        //var playerTile = new Tile(TileStructure.GetPlayerTileStructure());
        //TileDatabase.Remove(playerTile.Key);
        //TileDatabase.Add(playerTile);

        stopwatch.Stop();
        Logger.LogDebug($"Loaded tiles in {stopwatch.ElapsedMilliseconds}ms");
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(BackgroundImage), nameof(BackgroundImage.Load))]
    private static bool BackgroundImage_Load_Prefix_OverrideLoad()
    {
        if (!IsEnabled()) return true;

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        // only a single file matters so check it here
        var contents = GetContents()
                        .Where(content => File.Exists(Path.Combine(content.Directory, "SFD.jpg")))
                        .Reverse();

        // SFD vanilla check
        if (BackgroundImage.m_image != null && !BackgroundImage.m_image.IsDisposed)
        {
            BackgroundImage.m_image.Dispose();
            BackgroundImage.m_image = null;
        }

        foreach (var content in contents)
        {
            var contentBackgroundImagePath = Path.Combine(content.Directory, "SFD.jpg");

            try
            {
                ConsoleOutput.ShowMessage(ConsoleOutputType.Loading, $"Loading background image file: {contentBackgroundImagePath}");

                using (FileStream fileStream = File.OpenRead(contentBackgroundImagePath))
                {
                    Utils.WaitForGraphicsDevice();
                    BackgroundImage.m_image = Texture2D.FromStream(GameSFD.Handle.GraphicsDevice, fileStream);

                    fileStream.Close();
                }

                // stop here if the image loaded correctly,
                // otherwise move to the next content
                break;
            }
            catch (Exception ex)
            {
                ConsoleOutput.ShowMessage(ConsoleOutputType.Error, $"file '{contentBackgroundImagePath}' could not be loaded: " + ex.Message);
                BackgroundImage.m_image = null;
            }
        }

        //if (BackgroundImage.m_image != null && !BackgroundImage.m_image.IsDisposed)
        //{
        //    BackgroundImage.m_image.Dispose();
        //    BackgroundImage.m_image = null;
        //}

        //var backgroundImageContents = GetContentFiles("SFD.jpg", SearchOption.TopDirectoryOnly, Constants.Paths.DATA_MISC);
        //string SFDjpgImagePath = null;

        //foreach (var kvp in backgroundImageContents)
        //{
        //    if (kvp.Value.Length <= 0) continue;

        //    Logger.LogInfo($"LOADING [BACKGROUND-IMAGE]: {kvp.Key}");
        //    SFDjpgImagePath = kvp.Value.Last();
        //}

        //if (SFDjpgImagePath != null && File.Exists(SFDjpgImagePath))
        //{
        //    try
        //    {
        //        ConsoleOutput.ShowMessage(ConsoleOutputType.Loading, $"Loading background image file: {SFDjpgImagePath}");

        //        using (FileStream fileStream = File.OpenRead(SFDjpgImagePath))
        //        {
        //            Utils.WaitForGraphicsDevice();
        //            BackgroundImage.m_image = Texture2D.FromStream(GameSFD.Handle.GraphicsDevice, fileStream);

        //            fileStream.Close();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ConsoleOutput.ShowMessage(ConsoleOutputType.Error, $"file '{SFDjpgImagePath}' could not be loaded: " + ex.Message);
        //    }
        //}

        stopwatch.Stop();
        Logger.LogDebug($"Loaded background image in {stopwatch.ElapsedMilliseconds}ms");
        return false;
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(TileTextures), nameof(TileTextures.Load))]
    private static IEnumerable<CodeInstruction> TileTextures_Load_Prefix_MinorChanges(IEnumerable<CodeInstruction> instructions)
    {
        // These tiny changes allows re-using the method without too many changes.

        // change 'Constants.Paths.GetContentAssetPathFromFullPath' to use the unmodified path instead
        instructions.ElementAt(23).opcode = OpCodes.Nop;

        // assume the PNG texture in documents never exists
        instructions.ElementAt(69).opcode = OpCodes.Nop;
        instructions.ElementAt(70).opcode = OpCodes.Ldc_I4_0;

        return instructions;
    }
}
