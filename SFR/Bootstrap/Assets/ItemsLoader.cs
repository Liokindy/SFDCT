using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SFD;
using HarmonyLib;
using CGlobals = SFDCT.Misc.Globals;
using CIni = SFDCT.Settings.Values;

namespace SFDCT.Bootstrap.Assets
{
    [HarmonyPatch]
    internal static class ItemsLoader
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SFD.Items), nameof(SFD.Items.Load))]
        public static bool Load(GameSFD game)
        {
            if (!CIni.GetBool("USE_1_4_0_ASSETS"))
            {
                return true;
            }

            Items.m_allItems = new List<Item>();
            Items.m_allFemaleItems = new List<Item>();
            Items.m_allMaleItems = new List<Item>();
            Items.m_slotAllItems = new List<Item>[10];
            Items.m_slotFemaleItems = new List<Item>[10];
            Items.m_slotMaleItems = new List<Item>[10];
            for (int i = 0; i < Items.m_slotAllItems.Length; i++)
            {
                Items.m_slotAllItems[i] = new List<Item>();
                Items.m_slotFemaleItems[i] = new List<Item>();
                Items.m_slotMaleItems[i] = new List<Item>();
            }
            string[] allowedExtensions = new string[]
            {
        ".xnb",
        ".item"
            };
            string[] array = (from f in Directory.EnumerateFiles(Path.GetFullPath(Path.Combine(CGlobals.Paths.CONTENT, "Data\\Items\\")), "*.*", SearchOption.AllDirectories)
                              where allowedExtensions.Any(new Func<string, bool>(f.EndsWith))
                              select f).ToArray<string>();
            for (int j = 0; j < array.Length; j++)
            {
                if (GameSFD.Closing)
                {
                    return false;
                }
                string text = array[j];
                if (Content.IsXnb(text))
                {
                    text = array[j].Substring(0, array[j].Length - 4);
                }
                text = text.Remove(0, Constants.Paths.ExecutablePath.Length + "SFDCT\\Content\\".Length);
                while (text.StartsWith("\\") || text.StartsWith("/"))
                {
                    text = text.Remove(0, 1);
                }

                ConsoleOutput.ShowMessage(ConsoleOutputType.Loading, "Loading equipment '" + text + "'");
                text = Path.Combine("SFDCT\\Content", text);
                Item item = Content.Load<Item>(text);
                foreach (Item item2 in Items.m_allItems)
                {
                    if (item2.ID == item.ID)
                    {
                        throw new Exception(string.Concat(new string[]
                        {
                    "Error: Item ID collision between item '",
                    item2.ToString(),
                    "' and '",
                    item.ToString(),
                    "' while loading '",
                    array[j],
                    "'"
                        }));
                    }
                }
                item.PostProcess();
                Items.m_allItems.Add(item);
                Items.m_slotAllItems[item.EquipmentLayer].Add(item);
            }
            Items.PostProcessGenders();
            Player.HurtLevel1 = Items.GetItem("HurtLevel1");
            Player.HurtLevel2 = Items.GetItem("HurtLevel2");
            if (Player.HurtLevel2 == null)
            {
                Player.HurtLevel2 = Player.HurtLevel1;
            }

            return false;
        }
    }
}
