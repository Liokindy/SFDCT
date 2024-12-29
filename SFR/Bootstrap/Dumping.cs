using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using SFD;
using SFDCT.Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using CConst = SFDCT.Misc.Constants;

namespace SFDCT.Bootstrap;

/// <summary>
///     This class is used to dump raw PNG files from the clothing
///     items, and raw TXT files from the animations XNB file.
///     
///     Animations TXT files are dumped in the format used by Animations.LoadAnimationsDataPipeline
///     
///     Items are dumped into folders with PNGs and a properties TXT, they can be loaded
///     using Dumping.LoadItemDump however, it's reasonably slower than raw XNBs.
/// 
///     (This class has no use in-SFDCT yet)
/// </summary>
internal static class Dumping
{
    public static bool DoAssetDump = false;
    private static void CheckDirectory(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }
    private static void CheckFile(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
    private static bool Texture2DIsTransparency(Texture2D tex)
    {
        Color[] pixelData = new Color[tex.Width * tex.Height];
        tex.GetData<Color>(pixelData);

        for (int i = 0; i < pixelData.Length; i++)
        {
            if (pixelData[i].A > 16)
            {
                return false;
            }
        }
        return true;
    }

    public static void DumpItems()
    {
        if (!DoAssetDump) { return; }

        Logger.LogInfo("DUMPING: Starting item dump...");
        Stopwatch dumpItemStopwatch = new();
        dumpItemStopwatch.Start();

        string dumpRootFolder = Path.Combine(CConst.Paths.CONTENT, "Dump");

        string dumpItemFolder = Path.Combine(dumpRootFolder, "Items");
        string dumpItemNone = Path.Combine(dumpItemFolder, "None");
        string dumpItemAccessory = Path.Combine(dumpItemFolder, "Accessory");
        string dumpItemChestOver = Path.Combine(dumpItemFolder, "ChestOver");
        string dumpItemChestUnder = Path.Combine(dumpItemFolder, "ChestUnder");
        string dumpItemFeet = Path.Combine(dumpItemFolder, "Feet");
        string dumpItemHands = Path.Combine(dumpItemFolder, "Hands");
        string dumpItemHead = Path.Combine(dumpItemFolder, "Head");
        string dumpItemHurt = Path.Combine(dumpItemFolder, "Hurt");
        string dumpItemLegs = Path.Combine(dumpItemFolder, "Legs");
        string dumpItemSkin = Path.Combine(dumpItemFolder, "Skin");
        string dumpItemWaist = Path.Combine(dumpItemFolder, "Waist");

        CheckDirectory(dumpRootFolder);
        CheckDirectory(dumpItemFolder);

        CheckDirectory(dumpItemNone);
        CheckDirectory(dumpItemAccessory);
        CheckDirectory(dumpItemChestOver);
        CheckDirectory(dumpItemChestUnder);
        CheckDirectory(dumpItemFeet);
        CheckDirectory(dumpItemHands);
        CheckDirectory(dumpItemHead);
        CheckDirectory(dumpItemHurt);
        CheckDirectory(dumpItemLegs);
        CheckDirectory(dumpItemSkin);
        CheckDirectory(dumpItemWaist);

        int itemIndex = 0;
        int itemCount = Items.m_allItems.Count;
        foreach (Item item in Items.m_allItems)
        {
            Logger.LogDebug($"DUMPING: Item ({itemIndex}/{itemCount}) \"{item.GameName}\" ({item.Filename})...");
            string itemFolder = item.EquipmentLayer switch
            {
                0 => dumpItemSkin,
                1 => dumpItemChestUnder,
                2 => dumpItemLegs,
                3 => dumpItemWaist,
                4 => dumpItemFeet,
                5 => dumpItemChestOver,
                6 => dumpItemAccessory,
                7 => dumpItemHands,
                8 => dumpItemHead,
                9 => dumpItemHurt,
                _ => dumpItemNone,
            };

            itemFolder = Path.Combine(itemFolder, item.Filename);
            string itemPropertiesPath = Path.Combine(itemFolder, "properties.txt");

            CheckDirectory(itemFolder);
            CheckFile(itemPropertiesPath);

            StreamWriter streamWriter = File.CreateText(itemPropertiesPath);
            streamWriter.WriteLine($"GameName {item.GameName}");
            streamWriter.WriteLine($"Filename {item.Filename}");
            streamWriter.WriteLine($"EquipmentLayer {item.EquipmentLayer}");
            streamWriter.WriteLine($"ID {item.ID}");
            streamWriter.WriteLine($"JacketUnderBelt {item.JacketUnderBelt}");
            streamWriter.WriteLine($"CanEquip {item.CanEquip}");
            streamWriter.WriteLine($"CanScript {item.CanScript}");
            streamWriter.WriteLine($"ColorPalette {item.ColorPalette}");

            int itemPartCount = item.Parts.Length;
            string line = "";
            for(int i = 0; i < item.Parts.Length; i++)
            {
                ItemPart itemPart = item.Parts[i];

                line += i + " ";
                if (itemPart == null || itemPart.IsDisposed)
                {
                    continue;
                }

                int itemPartTexCount = itemPart.Textures.Length;
                for(int k = 0; k < itemPart.Textures.Length; k++)
                {
                    Texture2D itemPartTex = itemPart.Textures[k];
                    if (itemPartTex == null || itemPartTex.IsDisposed)
                    {
                        continue;
                    }
                    if (Texture2DIsTransparency(itemPartTex))
                    {
                        continue;
                    }

                    string itemPartTexPath = Path.Combine(itemFolder, $"{itemPart.Type}_{k}.png");
                    line += $"{itemPart.Type}_{k} ";
                    CheckFile(itemPartTexPath);

                    Stream stream = File.Create(itemPartTexPath);
                    itemPartTex.SaveAsPng(stream, itemPartTex.Width, itemPartTex.Height);
                    stream.Close();
                    stream.Dispose();
                }

                streamWriter.WriteLine(line);
                line = "";
            }

            streamWriter.Close();
            streamWriter.Dispose();
            itemIndex++;
        }

        dumpItemStopwatch.Stop();
        Logger.LogInfo($"DUMPING: Item dump finished in {(int)dumpItemStopwatch.Elapsed.TotalSeconds}s");
    }
    public static void DumpAnimations()
    {
        if (!DoAssetDump) { return; }

        Logger.LogInfo("DUMPING: Starting animations dump...");
        Stopwatch dumpAnimStopwatch = new();
        dumpAnimStopwatch.Start();

        string dumpRootFolder = Path.Combine(CConst.Paths.CONTENT, "Dump");
        string dumpAnimationsFolder = Path.Combine(dumpRootFolder, "Animations");

        CheckDirectory(dumpRootFolder);
        CheckDirectory(dumpAnimationsFolder);

        int animIndex = 0;
        int animCount = Animations.Data.Animations.Length;
        foreach (AnimationData animData in Animations.Data.Animations)
        {
            Logger.LogDebug($"DUMPING: Animation ({animIndex}/{animCount}) \"{animData.Name}\"...");

            string animationFilePath = Path.Combine(dumpAnimationsFolder, $"{animData.Name}.txt");
            CheckFile(animationFilePath);

            StreamWriter streamWriter = File.CreateText(animationFilePath);
            foreach(AnimationFrameData frameData in animData.Frames)
            {
                streamWriter.WriteLine("frame");
                streamWriter.WriteLine("time " + frameData.Time);
                streamWriter.WriteLine("event " + frameData.Event);
                foreach(AnimationPartData partData in frameData.Parts)
                {
                    streamWriter.WriteLine($"part {partData.GlobalId} {partData.X} {partData.Y} {partData.Rotation} {(int)partData.Flip} {partData.Scale.X} {partData.Scale.Y}");
                }
            }
            streamWriter.Close();
            streamWriter.Dispose();
            animIndex++;
        }

        dumpAnimStopwatch.Stop();
        Logger.LogInfo($"DUMPING: Animation dump finished in {(int)dumpAnimStopwatch.Elapsed.TotalSeconds}s");
    }
    public static bool LoadItemDump(GameSFD game)
    {
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

        string[] propertyFilesPaths = Directory.GetFiles(Path.Combine(Path.Combine(CConst.Paths.CONTENT, "Dump"), "Items"), "*.txt", SearchOption.AllDirectories);
        foreach (string propertyFilePath in propertyFilesPaths)
        {
            string itemFolderPath = propertyFilePath.Substring(0, propertyFilePath.Length - ("/properties.txt").Length);
            ConsoleOutput.ShowMessage(ConsoleOutputType.Loading, $"Loading custom equipment '{itemFolderPath}'");

            bool[] partsFound = new bool[6];
            ItemPart[] parts = new ItemPart[6];
            string gameName = "";
            string fileName = "";
            int equipmentLayer = 0;
            string id = "";
            bool jacketUnderBelt = false;
            bool canEquip = false;
            bool canScript = false;
            string colorPalette = "";

            string[] propertyFileLines = File.ReadAllLines(propertyFilePath);
            for (int lineIndex = 0; lineIndex < propertyFileLines.Length; lineIndex++)
            {
                string line = propertyFileLines[lineIndex];
                switch (lineIndex)
                {
                    case 0:
                        gameName = line.Substring(9);
                        break;
                    case 1:
                        fileName = line.Substring(9);
                        break;
                    case 2:
                        equipmentLayer = int.Parse(line.Substring(15));
                        break;
                    case 3:
                        id = line.Substring(3);
                        break;
                    case 4:
                        jacketUnderBelt = line.Substring(16) == "True";
                        break;
                    case 5:
                        canEquip = line.Substring(9) == "True";
                        break;
                    case 6:
                        canScript = line.Substring(10) == "True";
                        break;
                    case 7:
                        colorPalette = line.Substring(13);
                        break;
                }
                if (lineIndex > 7)
                {
                    if (line.Split(' ').Length > 1)
                    {
                        partsFound[lineIndex - 8] = true;
                    }
                }
            }

            for (int i = 0; i < parts.Length; i++)
            {
                if (!partsFound[i])
                {
                    continue;
                }

                Texture2D[] textures = new Texture2D[21];
                for (int k = 0; k < textures.Length; k++)
                {
                    string texturePath = Path.Combine(itemFolderPath, $"{i}_{k}.png");
                    if (File.Exists(texturePath))
                    {
                        Utils.WaitForGraphicsDevice();
                        Texture2D texture = PremultiplyTexture(texturePath, game.GraphicsDevice);
                        if (texture != null)
                        {
                            textures[k] = texture;
                        }
                    }
                }

                parts[i] = new(textures, i);
            }

            Item item = new(parts, null, gameName, fileName, equipmentLayer, id, jacketUnderBelt, canEquip, canScript, colorPalette);
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
    
    private static Texture2D PremultiplyTexture(string FilePath, GraphicsDevice device)
    {
        Texture2D texture2D;
        using (FileStream fileStream = File.OpenRead(FilePath))
        {
            texture2D = Texture2D.FromStream(device, fileStream);
            fileStream.Close();
        }
        Color[] array = new Color[texture2D.Width * texture2D.Height];
        texture2D.GetData<Color>(array);
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i].R == 255 && array[i].G == 0 && array[i].B == 255 && array[i].A == 255)
            {
                array[i] = Color.FromNonPremultiplied(255, 0, 255, 0);
            }
            else
            {
                array[i] = Color.FromNonPremultiplied((int)array[i].R, (int)array[i].G, (int)array[i].B, (int)array[i].A);
            }
        }
        texture2D.SetData<Color>(array);
        return texture2D;
    }
}
