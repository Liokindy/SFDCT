using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using SFD;
using SFDCT.Helper;
using CGlobals = SFDCT.Misc.Globals;
using CIni = SFDCT.Settings.Values;

namespace SFDCT.Bootstrap.Assets
{
    [HarmonyPatch]
    internal static class AnimationsLoader
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SFD.Animations), nameof(SFD.Animations.Load))]
        internal static bool Load(Microsoft.Xna.Framework.Game game, ref bool __result)
        {
            string animationsPath = Path.Combine("SFDCT\\Content", "Data\\Animations\\");

            if (!CIni.Get<bool>(CIni.GetKey(CIni.SettingKey.Use140Assets)) || (Directory.Exists(animationsPath) && Directory.EnumerateFiles(animationsPath).Count() > 0))
            {
                return true;
            }

            string[] animationTextFiles = Directory.EnumerateFiles(animationsPath).ToArray();
            if (animationTextFiles.Length >= 2)
            {
                List<AnimationData> animations = [];
                foreach(string animationFile in animationTextFiles)
                {
                    if (Path.GetFileNameWithoutExtension(animationFile) != "char_anims")
                    {
                        animations.Add(Import(Path.GetFileNameWithoutExtension(animationFile), File.ReadAllLines(animationFile)));
                    }
                }

                Animations.Data = new AnimationsData(animations.ToArray());
            }
            else
            {
                Animations.Data = Content.Load<AnimationsData>(Constants.GetLoadPath(Path.Combine("SFDCT\\Content", "Data\\Animations\\", "char_anims")));
            }

            if (!AnalizeAnimations())
            {
                SFD.Program.ShowError(new Exception("Core animation data file has been modified in an unintended way. Restore your char_anims file."), "Core animation data modified!", true);
                __result = false;
                return false;
            }

            __result = true;
            return false;
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SFD.Animations), nameof(SFD.Animations.Load))]
        internal static void LoadPostfix(Microsoft.Xna.Framework.Game game, ref bool __result)
        {
            if (CIni.Get<bool>(CIni.GetKey(CIni.SettingKey.Use140Assets)))
            {
                return;
            }

            if (!AnalizeAnimations())
            {
                SFD.Program.ShowError(new Exception("Core animation data file has been modified in an unintended way. Restore your char_anims file."), "Core animation data modified!", true);
                __result = false;
            }
        }

        internal static bool AnalizeAnimations()
        {
            foreach (Animations.AnalyzeAnimation analyzeAnimation in new Animations.AnalyzeAnimation[]
            {
                new Animations.AnalyzeAnimation("BaseKick",             [25,50,50,50,100,100],      ["STEP","","","MELEESWING","KICK","STOP"]),
                new Animations.AnalyzeAnimation("FullCharge",           [500,100,100,100,100,100],  ["TELEGRAPH","","","","",""]),
                new Animations.AnalyzeAnimation("FullChargeA",          [500],                      ["TELEGRAPH"]),
                new Animations.AnalyzeAnimation("FullChargeB",          [100,100,100,100],          ["","","",""]),
                new Animations.AnalyzeAnimation("FullRoll",             [100,100,100,100,100],      ["","","","",""]),
                new Animations.AnalyzeAnimation("UpperBlock",           [50,50,200],                ["","","STOP"]),
                new Animations.AnalyzeAnimation("UpperBlockChainsaw",   [50,50,200],                ["","","STOP"]),
                new Animations.AnalyzeAnimation("UpperBlockMelee",      [50,50,200],                ["","","STOP"]),
                new Animations.AnalyzeAnimation("UpperBlockMelee2H",    [50,50,200],                ["","","STOP"]),
                new Animations.AnalyzeAnimation("UpperBlockMelee2HEnd", [200],                      [""]),
                new Animations.AnalyzeAnimation("UpperBlockMeleeEnd",   [200],                      [""]),
                new Animations.AnalyzeAnimation("UpperMelee1H1",        [100,50,25,75,250],         ["","","MELEESWING","HIT","STOP"]),
                new Animations.AnalyzeAnimation("UpperMelee1H1End",     [75,200],                   ["","STOP"]),
                new Animations.AnalyzeAnimation("UpperMelee1H2",        [150,50,25,75,250],         ["","","MELEESWING","HIT","STOP"]),
                new Animations.AnalyzeAnimation("UpperMelee1H2End",     [75,200],                   ["","STOP"]),
                new Animations.AnalyzeAnimation("UpperMelee1H3",        [200,50,25,75,300],         ["","","MELEESWING","HIT","STOP"]),
                new Animations.AnalyzeAnimation("UpperMelee1H3Chain",   [200,50,25,75,300],         ["","","MELEESWING","HIT","STOP"]),
                new Animations.AnalyzeAnimation("UpperMelee1H3ChainEnd",[75,300],                   ["","STOP"]),
                new Animations.AnalyzeAnimation("UpperMelee1H3End",     [75,300],                   ["","STOP"]),
                new Animations.AnalyzeAnimation("UpperMelee1H4",        [100,50,50,50,250],         ["","","MELEESWING","HIT","STOP"]),
                new Animations.AnalyzeAnimation("UpperMelee2H1",        [100,50,25,75,250],         ["","","MELEESWING","HIT","STOP"]),
                new Animations.AnalyzeAnimation("UpperMelee2H1End",     [75,250],                   ["","STOP"]),
                new Animations.AnalyzeAnimation("UpperMelee2H2",        [150,50,25,75,250],         ["","","MELEESWING","HIT","STOP"]),
                new Animations.AnalyzeAnimation("UpperMelee2H2End",     [75,250],                   ["","STOP"]),
                new Animations.AnalyzeAnimation("UpperMelee2H3",        [200,50,25,75,300],         ["","","MELEESWING","HIT","STOP"]),
                new Animations.AnalyzeAnimation("UpperMelee2H3End",     [75,300],                   ["","STOP"]),
                new Animations.AnalyzeAnimation("UpperMelee2H4",        [100,50,50,100],            ["","MELEESWING","HIT","STOP"]),
                new Animations.AnalyzeAnimation("UpperMeleeHit1",       [50,50],                    ["","STOP"]),
                new Animations.AnalyzeAnimation("UpperMeleeHit2",       [50,50],                    ["","STOP"]),
                new Animations.AnalyzeAnimation("UpperPunch1",          [150,25,75,250],            ["","MELEESWING","HIT","STOP"]),
                new Animations.AnalyzeAnimation("UpperPunch2",          [150,25,75,250],            ["","MELEESWING","HIT","STOP"]),
                new Animations.AnalyzeAnimation("UpperPunch3",          [200,25,75,50,250],         ["","MELEESWING","HIT","","STOP"]),
                new Animations.AnalyzeAnimation("UpperPunch4",          [100,25,25,200],            ["","","MELEESWING_HIT","STOP"]),
            })
            {
                bool flag = false;
                AnimationData animationData = Animations.Data.DicAnimations[analyzeAnimation.Animation];
                if (animationData.Frames.Length != analyzeAnimation.FrameTimes.Length)
                {
                    flag = true;
                }
                else
                {
                    for (int j = 0; j < animationData.Frames.Length; j++)
                    {
                        if (animationData.Frames[j].Time != analyzeAnimation.FrameTimes[j] || animationData.Frames[j].Event != analyzeAnimation.FrameEvents[j])
                        {
                            flag = true;
                            break;
                        }
                    }
                }

                if (flag)
                {
                    return false;
                }
            }

            return true;
        }
        
        internal static AnimationData Import(string name, string[] animationFileLines)
        {
            List<AnimationFrameData> frames = [];
            List<AnimationPartData> frameParts = [];
            List<AnimationCollisionData> frameCollisions = [];
            int frameTime = 0;
            string frameEvent = string.Empty;

            for (int j = 0; j < animationFileLines.Length; j++)
            {
                string line = animationFileLines[j];
                string[] lineSplit = line.Split(' ');

                if (line.StartsWith("PART", StringComparison.OrdinalIgnoreCase))
                {
                    int partGlobalId = 0;

                    if (lineSplit[1].Length <= 5 && lineSplit[1].Contains("_"))
                    {
                        string[] typeLocalIndex = lineSplit[1].Split('_');
                        partGlobalId = int.Parse(typeLocalIndex[0]) * ItemPart.TYPE.PART_RANGE + int.Parse(typeLocalIndex[1]);
                    }
                    else
                    {
                        switch(lineSplit[1].ToUpperInvariant())
                        {
                            default:
                                partGlobalId = int.Parse(lineSplit[1]);
                                break;
                            case "SUBANIMATION":
                                partGlobalId = -ItemPart.TYPE.M_SUBANIMATION;
                                break;
                            case "SHEATHED_MELEE":
                                partGlobalId = -ItemPart.TYPE.M_SHEATHED_MELEE;
                                break;
                            case "SHEATHED_HANDGUN":
                                partGlobalId = -ItemPart.TYPE.M_SHEATHED_HANDGUN;
                                break;
                            case "SHEATHED_RIFLE":
                                partGlobalId = -ItemPart.TYPE.M_SHEATHED_RIFLE;
                                break;
                            case "WPN_MAINHAND":
                                partGlobalId = -ItemPart.TYPE.M_WPN_MAINHAND;
                                break;
                            case "WPN_OFFHAND":
                                partGlobalId = -ItemPart.TYPE.M_WPN_OFFHAND;
                                break;
                            case "TAIL":
                                partGlobalId = -ItemPart.TYPE.M_TAIL;
                                break;
                        }
                    }

                    float partX = float.Parse(lineSplit[2].Replace(',', '.'), CultureInfo.InvariantCulture);
                    float partY = float.Parse(lineSplit[3].Replace(',', '.'), CultureInfo.InvariantCulture);
                    float partRotation = float.Parse(lineSplit[4].Replace(',', '.'), CultureInfo.InvariantCulture);
                    SpriteEffects partFlip = (SpriteEffects)int.Parse(lineSplit[5]);
                    float partScaleX = float.Parse(lineSplit[6].Replace(',', '.'), CultureInfo.InvariantCulture);
                    float partScaleY = float.Parse(lineSplit[7].Replace(',', '.'), CultureInfo.InvariantCulture);
                    string partPostFix = lineSplit[8];

                    frameParts.Add(new AnimationPartData(partGlobalId, partX, partY, partRotation, partFlip, partScaleX, partScaleY, partPostFix));
                    //Logger.LogInfo($"- - - Part {frameParts.Count}: {partGlobalId} {partX} {partY} {partRotation} {(int)partFlip} {partScaleX} {partScaleY} '{partPostFix}'");
                }
                if (line.StartsWith("COLLISION", StringComparison.OrdinalIgnoreCase))
                {
                    int collId = int.Parse(lineSplit[1]);
                    int collWidth = int.Parse(lineSplit[2]);
                    int collHeight = int.Parse(lineSplit[3]);
                    float collX = float.Parse(lineSplit[4].Replace(',', '.'), CultureInfo.InvariantCulture);
                    float collY = float.Parse(lineSplit[5].Replace(',', '.'), CultureInfo.InvariantCulture);

                    frameCollisions.Add(new AnimationCollisionData(collId, collX, collY, collWidth, collHeight));
                    //Logger.LogInfo($"- - - Collision {frameCollisions.Count}: {collId} {collX} {collY} {collWidth} {collHeight}");
                }

                if ((line.StartsWith("FRAME", StringComparison.OrdinalIgnoreCase) && j != 0) || (j == animationFileLines.Length - 1))
                {
                    frames.Add(new AnimationFrameData(frameParts.ToArray(), frameCollisions.ToArray(), frameEvent, frameTime));
                    //Logger.LogInfo($"- - Frame {frames.Count}: {frameParts.Count} {frameCollisions.Count} {frameTime} '{frameEvent}'");
                }
                if (line.StartsWith("FRAME", StringComparison.OrdinalIgnoreCase))
                {
                    frameParts = [];
                    frameCollisions = [];
                    frameTime = int.Parse(lineSplit[1]);
                    frameEvent = lineSplit[2];
                }
            }

            return new AnimationData(frames.ToArray(), name);
        }
        internal static void Export(AnimationData[] animationsToExport, string animationFilePath)
        {
            string animationsFolderPath = Path.Combine(animationFilePath, "char_anims");
            Directory.CreateDirectory(animationsFolderPath);

            Logger.LogDebug($"EXPORTING ({animationsToExport.Length}): '{animationFilePath}'...");

            for (int i = 0; i < animationsToExport.Length; i++)
            {
                string animName = animationsToExport[i].Name;
                AnimationFrameData[] animFrames = animationsToExport[i].Frames;
                //Logger.LogInfo($"- Animation '{animName}' {i}: {animFrames.Length}");

                using (StreamWriter streamWriter = File.CreateText(Path.Combine(animationsFolderPath, string.Join(".", animName, "txt"))))
                {
                    for (int j = 0; j < animFrames.Length; j++)
                    {
                        AnimationPartData[] animFrameParts = animFrames[j].Parts;
                        AnimationCollisionData[] animFrameCollisions = animFrames[j].Collisions;
                        string animFrameEvent = animFrames[j].Event;
                        int animFrameTime = animFrames[j].Time;

                        //Logger.LogInfo($"- - Frame {j}: {animFrameParts.Length} {animFrameCollisions.Length} {animFrameTime} '{animFrameEvent}'");
                        streamWriter.WriteLine(string.Join(" ", "FRAME", animFrameTime, animFrameEvent));

                        for (int l = 0; l < animFrameCollisions.Length; l++)
                        {
                            AnimationCollisionData animColl = animFrameCollisions[l];

                            //Logger.LogInfo($"- - - Collision {l}: {animColl.Id} {animColl.X} {animColl.Y} {animColl.Width} {animColl.Height}");
                            streamWriter.WriteLine(string.Join(" ", "COLLISION", animColl.Id, animColl.X, animColl.Y, animColl.Width, animColl.Height));
                        }
                        for (int k = 0; k < animFrameParts.Length; k++)
                        {
                            AnimationPartData animFramePart = animFrameParts[k];

                            string animFramePartID = animFramePart.GlobalId.ToString();
                            if (animFramePart.Type >= 0)
                            {
                                animFramePartID = string.Join("_", animFramePart.Type, animFramePart.LocalId);
                            }
                            else
                            {
                                switch(animFramePart.LocalId)
                                {
                                    case ItemPart.TYPE.M_SUBANIMATION:
                                        animFramePartID = "SUBANIMATION";
                                        break;
                                    case ItemPart.TYPE.M_SHEATHED_MELEE:
                                        animFramePartID = "SHEATHED_MELEE";
                                        break;
                                    case ItemPart.TYPE.M_SHEATHED_HANDGUN:
                                        animFramePartID = "SHEATHED_HANDGUN";
                                        break;
                                    case ItemPart.TYPE.M_SHEATHED_RIFLE:
                                        animFramePartID = "SHEATHED_RIFLE";
                                        break;
                                    case ItemPart.TYPE.M_WPN_MAINHAND:
                                        animFramePartID = "WPN_MAINHAND";
                                        break;
                                    case ItemPart.TYPE.M_WPN_OFFHAND:
                                        animFramePartID = "WPN_OFFHAND";
                                        break;
                                    case ItemPart.TYPE.M_TAIL:
                                        animFramePartID = "TAIL";
                                        break;
                                }
                            }

                            //Logger.LogInfo($"- - - Part {k}: {animFramePartID} {animFramePart.X} {animFramePart.Y} {animFramePart.Rotation} {(int)animFramePart.Flip} {animFramePart.Scale.X} {animFramePart.Scale.Y} '{animFramePart.PostFix}'");
                            streamWriter.WriteLine(string.Join(" ", "PART", animFramePartID, animFramePart.X, animFramePart.Y, animFramePart.Rotation, (int)animFramePart.Flip, animFramePart.Scale.X, animFramePart.Scale.Y, animFramePart.PostFix));
                        }
                    }
                    streamWriter.Close();
                }

            }
        }
    }
}
