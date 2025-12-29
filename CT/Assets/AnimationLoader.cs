using SFD;
using SFD.Code;
using SFDCT.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SFDCT.Assets;

internal static class AnimationLoader
{
    internal const string ANIMATIONS_FILE_NAME = "char_anims";

    internal static bool CheckAnalyzeAnimation(AnimationData data, Animations.AnalyzeAnimation analyzeData)
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

    internal static bool Load()
    {
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

        Dictionary<string, AnimationData> animationsDic = [];

        Logger.LogInfo("LOADING [ANIMATIONS]: Official");

        string officialAnimationsPath = Path.Combine(Constants.Paths.ContentPath, Constants.Paths.DATA_ANIMATIONS, ANIMATIONS_FILE_NAME);
        AnimationsData officialAnimationsData = Content.Load<AnimationsData>(officialAnimationsPath);

        foreach (var animation in officialAnimationsData.Animations)
        {
            if (analyzeDataDic.ContainsKey(animation.Name) && !CheckAnalyzeAnimation(animation, analyzeDataDic[animation.Name]))
            {
                SFD.Program.ShowError(new Exception("Core animation data file has been modified in an unintended way. Restore your char_anims file."), "Core animation data modified!", true);
                return false;
            }
            else
            {
                animationsDic.Add(animation.Name, animation);
            }
        }

        string documentsAnimationsPath = Path.Combine(Constants.Paths.UserDocumentsContentCustomPath, Constants.Paths.DATA_ANIMATIONS);
        string documentsAnimationsBinaryFilePath = Path.Combine(Constants.Paths.UserDocumentsContentCustomPath, Constants.Paths.DATA_ANIMATIONS, ANIMATIONS_FILE_NAME);

        if (Directory.Exists(documentsAnimationsPath) && File.Exists(documentsAnimationsBinaryFilePath))
        {
            Logger.LogInfo("LOADING [ANIMATIONS]: Documents");
            AnimationsData documentsAnimationsData = Content.Load<AnimationsData>(documentsAnimationsBinaryFilePath);

            foreach (var animation in officialAnimationsData.Animations)
            {
                if (!analyzeDataDic.ContainsKey(animation.Name) || CheckAnalyzeAnimation(animation, analyzeDataDic[animation.Name]))
                {
                    SubContentHandler.AddOrSetDictionaryValue(animationsDic, animation.Name, animation);
                }
            }
        }

        foreach (var subContentFolder in SubContentHandler.Folders)
        {
            string subContentAnimationsPath = SubContentHandler.GetPath(subContentFolder, Constants.Paths.DATA_ANIMATIONS);

            if (Directory.Exists(subContentAnimationsPath))
            {
                Logger.LogInfo($"LOADING [ANIMATIONS]: {subContentFolder}");

                string subContentAnimationsBinaryFilePath = Path.Combine(subContentAnimationsPath, ANIMATIONS_FILE_NAME);

                AnimationsData subContentAnimationsData = null;

                if (File.Exists(subContentAnimationsBinaryFilePath))
                {
                    subContentAnimationsData = Content.Load<AnimationsData>(subContentAnimationsBinaryFilePath);
                }
                //else
                //{
                //    string[] textFiles = Directory.GetFiles(subContentAnimationsPath, "*.txt", SearchOption.AllDirectories);

                //    if (textFiles.Length > 0)
                //    {
                //        subContentAnimationsData = LoadAnimationsFromTextFiles(textFiles);
                //    }
                //}

                if (subContentAnimationsData != null)
                {
                    foreach (var animation in subContentAnimationsData.Animations)
                    {
                        if (!analyzeDataDic.ContainsKey(animation.Name) || CheckAnalyzeAnimation(animation, analyzeDataDic[animation.Name]))
                        {
                            SubContentHandler.AddOrSetDictionaryValue(animationsDic, animation.Name, animation);
                        }
                    }
                }
            }
        }

        Animations.Data = new(animationsDic.Values.ToArray());
        return true;
    }
}
