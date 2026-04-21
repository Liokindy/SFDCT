using Microsoft.Xna.Framework.Graphics;
using SFD;
using SFD.Code;
using SFD.Parser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SFDCT.Assets;

internal static class AnimationsLoader
{
    internal static AnimationData ReadAnimationFromTextFile(string filePath)
    {
        // most of this code was taken from SFR's "SFD v1.3.7x",
        // with some minor additions to support some QoL features

        var lines = File.ReadAllLines(filePath)
                        .Where(s => !s.StartsWith(";") && !s.StartsWith("#") && !string.IsNullOrEmpty(s) && !string.IsNullOrWhiteSpace(s));
        var lineCount = lines.Count();

        var animationName = Path.GetFileNameWithoutExtension(filePath);
        var animationFrames = new List<AnimationFrameData>();

        var frameTime = 0;
        var frameEvent = string.Empty;
        var frameParts = new List<AnimationPartData>();
        var frameCollisions = new List<AnimationCollisionData>();

        for (int i = 0; i < lineCount; i++)
        {
            string line = lines.ElementAt(i);
            string[] lineBits = line.Split(' ');

            switch (lineBits[0].ToUpperInvariant())
            {
                case "TIME":
                    frameTime = int.Parse(lineBits[1]);
                    break;
                case "EVENT":
                    frameEvent = string.Join(" ", lineBits, 1, lineBits.Length - 1);
                    break;
                case "FRAME":
                    if (lineBits.Length == 1) break;

                    frameTime = int.Parse(lineBits[1]);
                    frameEvent = string.Join(" ", lineBits, 2, lineBits.Length - 2);
                    break;
                case "PART":
                    int partGlobalID;

                    if (!int.TryParse(lineBits[1], out partGlobalID))
                    {
                        switch (lineBits[1].ToUpperInvariant())
                        {
                            case "SUBANIMATION":
                                partGlobalID = -ItemPart.TYPE.M_SUBANIMATION;
                                break;
                            case "TAIL":
                                partGlobalID = -ItemPart.TYPE.M_TAIL;
                                break;
                            case "SHEATHED_RIFLE":
                                partGlobalID = -ItemPart.TYPE.M_SHEATHED_RIFLE;
                                break;
                            case "SHEATHED_MELEE":
                                partGlobalID = -ItemPart.TYPE.M_SHEATHED_MELEE;
                                break;
                            case "SHEATHED_HANDGUN":
                                partGlobalID = -ItemPart.TYPE.M_SHEATHED_HANDGUN;
                                break;
                            case "WPN_MAINHAND":
                                partGlobalID = -ItemPart.TYPE.M_WPN_MAINHAND;
                                break;
                            case "WPN_OFFHAND":
                                partGlobalID = -ItemPart.TYPE.M_WPN_OFFHAND;
                                break;
                            default:
                                int partTypeID, partLocalID;
                                string[] partIDBits = lineBits[1].Split('_');

                                if (partIDBits.Length == 2 && int.TryParse(partIDBits[0], out partTypeID) && int.TryParse(partIDBits[1], out partLocalID))
                                {
                                    partGlobalID = partTypeID * ItemPart.TYPE.PART_RANGE + partLocalID;
                                }
                                break;
                        }
                    }

                    float partX = SFDXParser.ParseFloat(lineBits[2]);
                    float partY = SFDXParser.ParseFloat(lineBits[3]);
                    float partAngle = SFDXParser.ParseFloat(lineBits[4]);
                    SpriteEffects partFlip = (SpriteEffects)int.Parse(lineBits[5]);
                    float partScaleX = SFDXParser.ParseFloat(lineBits[6]);
                    float partScaleY = SFDXParser.ParseFloat(lineBits[7]);
                    string partPostFix = lineBits.ElementAtOrDefault(8);

                    frameParts.Add(new(partGlobalID, partX, partY, partAngle, partFlip, partScaleX, partScaleY, partPostFix));
                    break;
                case "COLLISION":
                    int collisionID = int.Parse(lineBits[1]);
                    float collisionX = SFDXParser.ParseFloat(lineBits[2]);
                    float collisionY = SFDXParser.ParseFloat(lineBits[3]);
                    float collisionWidth = SFDXParser.ParseFloat(lineBits[4]);
                    float collisionHeight = SFDXParser.ParseFloat(lineBits[5]);

                    frameCollisions.Add(new(collisionID, collisionX, collisionY, collisionWidth, collisionHeight));
                    break;
            }

            if (i == lineCount - 1 || (i > 0 && lineBits[0].ToUpperInvariant().Equals("FRAME")))
            {
                animationFrames.Add(new(frameParts.ToArray(), frameCollisions.ToArray(), frameEvent, frameTime));

                frameParts.Clear();
                frameCollisions.Clear();
                frameEvent = string.Empty;
                frameTime = 0;
            }
        }

        return new(animationFrames.ToArray(), animationName);
    }

    internal static Animations.AnalyzeAnimation[] GetOfficialAnalyzeAnimationData()
    {
        return
        [
            new("BaseKick",                 [25, 50, 50, 50, 100, 100],     ["STEP", "", "", "MELEESWING", "KICK", "STOP"]),
            new("FullCharge",               [500, 100, 100, 100, 100, 100], ["TELEGRAPH", "", "", "", "", ""]),
            new("FullChargeA",              [500],                          ["TELEGRAPH"]),
            new("FullChargeB",              [100, 100, 100, 100],           ["", "", "", ""]),
            new("FullRoll",                 [100, 100, 100, 100, 100],      ["", "", "", "", ""]),
            new("UpperBlock",               [50, 50, 200],                  ["", "", "STOP"]),
            new("UpperBlockChainsaw",       [50, 50, 200],                  ["", "", "STOP"]),
            new("UpperBlockMelee",          [50, 50, 200],                  ["", "", "STOP"]),
            new("UpperBlockMelee2H",        [50, 50, 200],                  ["", "", "STOP"]),
            new("UpperBlockMelee2HEnd",     [200],                          [""]),
            new("UpperBlockMeleeEnd",       [200],                          [""]),
            new("UpperMelee1H1",            [100, 50, 25, 75, 250],         ["", "", "MELEESWING", "HIT", "STOP"]),
            new("UpperMelee1H1End",         [75, 200],                      ["", "STOP"]),
            new("UpperMelee1H2",            [150, 50, 25, 75, 250],         ["", "", "MELEESWING", "HIT", "STOP"]),
            new("UpperMelee1H2End",         [75, 200],                      ["", "STOP"]),
            new("UpperMelee1H3",            [200, 50, 25, 75, 300],         ["", "", "MELEESWING", "HIT", "STOP"]),
            new("UpperMelee1H3Chain",       [200, 50, 25, 75, 300],         ["", "", "MELEESWING", "HIT", "STOP"]),
            new("UpperMelee1H3ChainEnd",    [75, 300],                      ["", "STOP"]),
            new("UpperMelee1H3End",         [75, 300],                      ["", "STOP"]),
            new("UpperMelee1H4",            [100, 50, 50, 50, 250],         ["", "", "MELEESWING", "HIT", "STOP"]),
            new("UpperMelee2H1",            [100, 50, 25, 75, 250],         ["", "", "MELEESWING", "HIT", "STOP"]),
            new("UpperMelee2H1End",         [75, 250],                      ["", "STOP"]),
            new("UpperMelee2H2",            [150, 50, 25, 75, 250],         ["", "", "MELEESWING", "HIT", "STOP"]),
            new("UpperMelee2H2End",         [75, 250],                      ["", "STOP"]),
            new("UpperMelee2H3",            [200, 50, 25, 75, 300],         ["", "", "MELEESWING", "HIT", "STOP"]),
            new("UpperMelee2H3End",         [75, 300],                      ["", "STOP"]),
            new("UpperMelee2H4",            [100, 50, 50, 100],             ["", "MELEESWING", "HIT", "STOP"]),
            new("UpperMeleeHit1",           [50, 50],                       ["", "STOP"]),
            new("UpperMeleeHit2",           [50, 50],                       ["", "STOP"]),
            new("UpperPunch1",              [150, 25, 75, 250],             ["", "MELEESWING", "HIT", "STOP"]),
            new("UpperPunch2",              [150, 25, 75, 250],             ["", "MELEESWING", "HIT", "STOP"]),
            new("UpperPunch3",              [200, 25, 75, 50, 250],         ["", "MELEESWING", "HIT", "", "STOP"]),
            new("UpperPunch4",              [100, 25, 25, 200],             ["", "", "MELEESWING_HIT", "STOP"])
        ];
    }

    internal static bool AnalyzeAnimation(AnimationData animationData, Animations.AnalyzeAnimation analyzeData)
    {
        // TODO:
        // - rewrite this check to allow more/less frames if the events are kept in the same place

        if (animationData.Frames.Length != analyzeData.FrameTimes.Length)
        {
            return false;
        }

        for (int j = 0; j < animationData.Frames.Length; j++)
        {
            if (animationData.Frames[j].Time != analyzeData.FrameTimes[j] || animationData.Frames[j].Event != analyzeData.FrameEvents[j])
            {
                return false;
            }
        }

        return true;
    }

    internal static bool Load(GameSFD game)
    {
        var loadingText = LanguageHelper.GetText("loading.animations");
        game.ShowLoadingText(loadingText);

        // create an empty AnimationsData here to avoid
        // an extra loop through all animations at the end
        var totalAnimations = new AnimationsData([]);

        var contents = SubContentHandler.GetContents()
                        .Where(content => Directory.Exists(Path.Combine(content.Directory, Constants.Paths.DATA_ANIMATIONS)))
                        .Reverse();

        foreach (var content in contents)
        {
            var contentAnimationsFolderPath = Path.Combine(content.Directory, Constants.Paths.DATA_ANIMATIONS);

            // text files
            foreach (var path in Directory.EnumerateFiles(contentAnimationsFolderPath, "*.txt", SearchOption.AllDirectories))
            {
                ConsoleOutput.ShowMessage(ConsoleOutputType.Loading, $"Reading animation text file: {path}");

                var animation = ReadAnimationFromTextFile(path);

                if (totalAnimations.DicAnimations.ContainsKey(animation.Name)) continue;

                //ConsoleOutput.ShowMessage(ConsoleOutputType.Loading, $"Adding animation: '{animation.Name}'");
                totalAnimations.DicAnimations.Add(animation.Name, animation);
            }

            // 'char_anims' files
            foreach (var path in Directory.EnumerateFiles(contentAnimationsFolderPath, "char_anims", SearchOption.AllDirectories))
            {
                ConsoleOutput.ShowMessage(ConsoleOutputType.Loading, $"Reading animations file: {path}");

                var data = Content.Load<AnimationsData>(path);

                foreach (var animation in data.Animations)
                {
                    if (totalAnimations.DicAnimations.ContainsKey(animation.Name)) continue;

                    //ConsoleOutput.ShowMessage(ConsoleOutputType.Loading, $"Adding animation: '{animation.Name}'");
                    totalAnimations.DicAnimations.Add(animation.Name, animation);
                }
            }
        }

        // finish AnimationsData setup
        totalAnimations.Animations = totalAnimations.DicAnimations.Values.ToArray();

        // vanilla check
        foreach (var analyzeData in GetOfficialAnalyzeAnimationData())
        {
            if (!totalAnimations.DicAnimations.ContainsKey(analyzeData.Animation)) continue;

            var animationData = totalAnimations.DicAnimations[analyzeData.Animation];
            var analyzeSuccess = AnalyzeAnimation(animationData, analyzeData);

            if (!analyzeSuccess)
            {
                SFD.Program.ShowError(new Exception("Core animation data file has been modified in an unintended way. Restore your char_anims file."), "Core animation data modified!", true);

                return false;
            }
        }

        Animations.Data = totalAnimations;
        return true;
    }
}
