using SFD.Parser;
using System.Collections.Generic;

namespace SFDCT.Assets;

internal static class DatabaseLoader
{
    internal static bool Load<T>(string sfdxPath, ref Dictionary<string, T> originalDictionary, ref Dictionary<string, T> totalDictionary)
    {
        // all databases are loaded from SFDX files, the code
        // that read them also adds them to their corresponding database.
        // to avoid duplicating all the code that handles the reading, read them,
        // adding new entries to a temporary dictionary, and then clear the original one

        SFDXReader.ReadDataFromSFDXFile(sfdxPath);

        foreach (var kvp in originalDictionary)
        {
            // not overriding existing entries here means prior loaded entries
            // are not overwritten by later loaded entries
            if (totalDictionary.ContainsKey(kvp.Key)) continue;

            totalDictionary[kvp.Key] = kvp.Value;
        }

        originalDictionary.Clear();

        return true;
    }
}
