using HarmonyLib;
using SFD;
using SFDCT.Configuration;
using SFDCT.Helper;
using SFDCT.Misc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SFDCT.Bootstrap;

[HarmonyPatch]
internal static class LanguageHandler
{
    internal static string[] GetAvailableLanguages() => LanguageFileTranslator.m_languageFileMappings.Keys
                                                        .Where(lang => lang.StartsWith("SFDCT", StringComparison.OrdinalIgnoreCase))
                                                        .ToArray();

    [HarmonyPostfix]
    [HarmonyPatch(typeof(LanguageFileTranslator), nameof(LanguageFileTranslator.ListLanguageNames))]
    private static void LanguageFileTranslator_ListLanguageNames_Postfix_RemoveSFDCTLanguages(ref List<string> __result)
    {
        for (int i = __result.Count - 1; i >= 0; i--)
        {
            string language = __result[i];

            if (language.StartsWith("SFDCT", StringComparison.OrdinalIgnoreCase))
            {
                __result.RemoveAt(i);
            }
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(LanguageHelper), nameof(LanguageHelper.Load))]
    private static void LanguageHelper_Load_Postfix_LoadSFDCTLanguage()
    {
        string filePath = Path.Combine(Globals.Paths.Language, SFDCTConfig.Get<string>(CTSettingKey.Language)) + ".xml";

        if (!File.Exists(filePath))
        {
            if (LanguageFileTranslator.m_languageFileMappings.ContainsKey(SFDCTConfig.Get<string>(CTSettingKey.Language)))
            {
                filePath = LanguageFileTranslator.GetLanguageFileFromName(SFDCTConfig.Get<string>(CTSettingKey.Language));
            }
        }

        if (!File.Exists(filePath))
        {
            Logger.LogError($"LOADING [LANGUAGE]: Failed to find language file: '{filePath}'");
            Logger.LogError("LOADING [LANGUAGE]: Using default language");

            filePath = Path.Combine(Globals.Paths.Language, "SFDCT_default.xml");
            SFDCTConfig.Set(CTSettingKey.Language, "SFDCT_default");
        }

        filePath = Path.GetFullPath(filePath);

        if (!File.Exists(filePath))
        {
            Logger.LogError("LOADING [LANGUAGE]: Failed to default language file");
            return;
        }

        LanguageHelper.ReadFile(filePath, LanguageHelper.m_texts, LanguageHelper.m_textHashes);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(LanguageFileTranslator), nameof(LanguageFileTranslator.Load))]
    private static void LanguageFileTranslator_Load_Postfix()
    {
        string folderPath = Path.GetFullPath(Globals.Paths.Language);

        LanguageFileTranslator.LoadFolder(folderPath);
    }
}
