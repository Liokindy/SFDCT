using HarmonyLib;
using SFD;
using SFD.Colors;
using SFD.Sounds;
using SFD.Tiles;
using SFDCT.Misc;
using System.Collections.Generic;
using System.IO;

namespace SFDCT.Assets;

[HarmonyPatch]
internal static class SubContentHandler
{
    internal static string[] Folders;

    internal static void Load()
    {
        Folders = Directory.GetDirectories(Globals.Paths.SubContent, "*", SearchOption.TopDirectoryOnly);

        for (int i = 0; i < Folders.Length; i++)
        {
            Folders[i] = Path.GetFileName(Folders[i]);
        }
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
            if (toDic.ContainsKey(kvp.Key))
            {
                toDic[kvp.Key] = kvp.Value;
            }
            else
            {
                toDic.Add(kvp.Key, kvp.Value);
            }
        }
    }

    internal static string GetPath(string subContentFolder, params string[] path)
    {
        return Path.Combine(Globals.Paths.SubContent, subContentFolder, Path.Combine(path));
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Animations), nameof(Animations.Load))]
    private static bool Animations_Load_Prefix_OverrideLoad(ref bool __result)
    {
        __result = AnimationLoader.Load();

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Textures), nameof(Textures.Load), [typeof(string)])]
    private static bool Textures_Load_Prefix_OverrideLoad()
    {
        TextureLoader.Load();

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Items), nameof(Items.Load), [typeof(GameSFD)])]
    private static bool Items_Load_Prefix_OverrideLoad()
    {
        ItemLoader.Load();

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ColorPaletteDatabase), nameof(ColorPaletteDatabase.Load), [typeof(GameSFD)])]
    private static bool ColorPaletteDatabase_Load_Prefix_OverrideLoad()
    {
        ColorPaletteLoader.Load();

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ColorDatabase), nameof(ColorDatabase.Load), [typeof(GameSFD)])]
    private static bool ColorDatabase_Load_Prefix_OverrideLoad()
    {
        ColorLoader.Load();

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(SoundHandler), nameof(SoundHandler.Load), [typeof(GameSFD)])]
    private static bool SoundHandler_Load_Prefix_OverrideLoad()
    {
        SoundLoader.Load();

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(TileDatabase), nameof(TileDatabase.Load), [typeof(GameSFD)])]
    private static bool TileDatabase_Load_Prefix_OverrideLoad()
    {
        TileLoader.Load();

        return false;
    }
}
