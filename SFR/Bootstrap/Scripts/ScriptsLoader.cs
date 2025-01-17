using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using Microsoft.Xna.Framework;
using SFD.Weapons;
using SFDCT.Helper;
using CGlobals = SFDCT.Misc.Globals;
using HarmonyLib;

namespace SFDCT.Bootstrap.Assets
{
    [HarmonyPatch]
    internal static class Refresh
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SFD.GameSFD), nameof(SFD.GameSFD.StateKeyUpEvent))]
        private static void RefreshScriptsOnKeyUp(Microsoft.Xna.Framework.Input.Keys key)
        {
            if (key == Microsoft.Xna.Framework.Input.Keys.F6 && Keyboard.IsLeftShiftDown)
            {
                SFD.ConsoleOutput.ShowMessage(SFD.ConsoleOutputType.ScriptFiles, $"Refreshing '{CGlobals.Paths.SCRIPTS}'...");
                ScriptsLoader.Load();
            }
        }
    }

    internal static class ScriptsLoader
    {
        internal static List<CTScript> LoadedScripts = null;
        internal static Dictionary<CTScript.ScriptType, List<int>> LoadedScriptsTypeLookUpDictionary = null;

        internal static void Load()
        {
            Logger.LogDebug("SCRIPTS: Loading...");
            if (LoadedScripts != null)
            {
                LoadedScripts.Clear();
            }
            if (LoadedScriptsTypeLookUpDictionary != null)
            {
                LoadedScriptsTypeLookUpDictionary.Clear();
            }

            if (Directory.Exists(CGlobals.Paths.SCRIPTS))
            {
                LoadedScripts = new List<CTScript>();

                Logger.LogDebug($"SCRIPTS: Found scripts directory: '{CGlobals.Paths.SCRIPTS}'");
                
                List<string> foundScripts = Directory.EnumerateFiles(CGlobals.Paths.SCRIPTS, "*.ini", SearchOption.AllDirectories).ToList();
                Logger.LogDebug($"SCRIPTS: Found {foundScripts.Count} scripts");
                foreach (string scriptPath in foundScripts)
                {
                    string scriptRelativePath = scriptPath.Substring(CGlobals.Paths.SCRIPTS.Length + 1);
                    string[] scriptPathFolders = scriptRelativePath.Split(Path.DirectorySeparatorChar);
                    string scriptFileName = Path.GetFileNameWithoutExtension(scriptPath);
                    CTScript.ScriptType scriptType = CTScript.ScriptType.None;

                    if (scriptFileName.StartsWith("_") || scriptFileName.Equals("base", StringComparison.OrdinalIgnoreCase) || scriptFileName.Equals("example", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    Logger.LogDebug($"SCRIPTS: Reading '{scriptRelativePath}'...");

                    if (scriptPathFolders.Length >= 1)
                    {
                        switch (scriptPathFolders[0])
                        {
                            case "Weapons":
                                scriptType = CTScript.ScriptType.WeaponOverride;
                                break;
                            case "Colors":
                                scriptType = CTScript.ScriptType.ConstantColorOverride;
                                break;
                        }
                    }

                    Dictionary<string, string> keyValues = new Dictionary<string, string>();
                    string[] scriptFileLines = File.ReadAllLines(scriptPath);

                    foreach (string line in scriptFileLines)
                    {
                        if (line.StartsWith(";"))
                        {
                            continue;
                        }

                        string[] lineSplit = line.Split('=');
                        keyValues.Add(lineSplit[0], lineSplit[1]);
                    }

                    CTScript script = new CTScript()
                    {
                        FilePath = scriptPath,
                        Entries = new Dictionary<string, string>(keyValues),
                        Type = scriptType,
                    };
                    LoadedScripts.Add(script);

                    Logger.LogDebug($"SCRIPTS: Added script: '{script.FileName}', {script.Type}");
                }

                LoadedScriptsTypeLookUpDictionary = new Dictionary<CTScript.ScriptType, List<int>>();
                foreach (CTScript script in LoadedScripts)
                {
                    if (!LoadedScriptsTypeLookUpDictionary.ContainsKey(script.Type))
                    {
                        LoadedScriptsTypeLookUpDictionary.Add(script.Type, new List<int>());
                    }

                    LoadedScriptsTypeLookUpDictionary[script.Type].Add(LoadedScripts.IndexOf(script));
                }
            }
        }

        internal static CTScript[] GetByType(CTScript.ScriptType type)
        {
            if (!LoadedScriptsTypeLookUpDictionary.ContainsKey(type))
            {
                return null;
            }

            List<int> ids = LoadedScriptsTypeLookUpDictionary[type];
            int count = ids.Count;

            CTScript[] result = new CTScript[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = LoadedScripts[ids[i]];
            }
            
            return result;
        }

        internal enum ParseType
        {
            String = 0,
            Float,
            Int,
            Ushort,
            Short,
            Vector2,
            DeflectionProperties,
            DamageOutputType,
            Bool,
            Material,
            Projectile,
            MeleeWeaponTypeEnum,
            MeleeHandlingType,
            WeaponCategory,
            StringArray,
        }
        internal static object TryParse(string value, ParseType type)
        {
            switch (type)
            {
                default:
                case ParseType.String:
                    return value;
                case ParseType.Float:
                    return float.Parse(value.Replace(',', '.'), CultureInfo.InvariantCulture);
                case ParseType.Int:
                    return int.Parse(value, CultureInfo.InvariantCulture);
                case ParseType.Ushort:
                    return ushort.Parse(value, CultureInfo.InvariantCulture);
                case ParseType.Short:
                    return short.Parse(value, CultureInfo.InvariantCulture);
                case ParseType.Vector2:
                    string[] pos = value.Replace(',', '.').Split(' ');
                    return new Vector2(float.Parse(pos[0], CultureInfo.InvariantCulture), float.Parse(pos[1], CultureInfo.InvariantCulture));
                case ParseType.DeflectionProperties:
                    string[] args = value.Replace(',', '.').Split(' ');

                    float durabilityLoss = 5f;
                    float deflectCone = 0.01f;
                    DeflectBulletType deflectType = DeflectBulletType.Absorb;

                    deflectCone = float.Parse(args[0], CultureInfo.InvariantCulture);
                    if (args.Length >= 2)
                    {
                        deflectType = (DeflectBulletType)Enum.Parse(typeof(DeflectBulletType), args[1]);
                    }
                    if (args.Length >= 3)
                    {
                        durabilityLoss = float.Parse(args[2], CultureInfo.InvariantCulture);
                    }

                    return new DeflectionProperties(0.01f)
                    {
                        DeflectCone = Math.Max(SFD.SFDMath.DegToRad(deflectCone), 0.01f),
                        DeflectType = deflectType,
                        DurabilityLoss = durabilityLoss,
                    };
                case ParseType.DamageOutputType:
                    return Enum.Parse(typeof(DamageOutputType), value);
                case ParseType.Bool:
                    return bool.Parse(value);
                case ParseType.Material:
                    return SFD.Materials.MaterialDatabase.Get(value);
                case ParseType.Projectile:
                    return SFD.Projectiles.ProjectileDatabase.GetProjectile(short.Parse(value));
                case ParseType.MeleeWeaponTypeEnum:
                    return Enum.Parse(typeof(MeleeWeaponTypeEnum), value);
                case ParseType.MeleeHandlingType:
                    return Enum.Parse(typeof(MeleeHandlingType), value);
                case ParseType.StringArray:
                    return value.Split('|');
                case ParseType.WeaponCategory:
                    return Enum.Parse(typeof(WeaponCategory), value);
            }
        }
        internal class CTScript
        {
            public string FilePath;
            public string FileName
            {
                get
                {
                    return Path.GetFileNameWithoutExtension(FilePath);
                }
            }

            public ScriptType Type;
            // Keys are always stored in UpperCase!!
            public Dictionary<string, string> Entries;

            public enum ScriptType
            {
                None = 0,
                WeaponOverride,
                ConstantColorOverride,
            }
        }
    }
}