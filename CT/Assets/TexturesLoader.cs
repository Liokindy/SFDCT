using SFD;
using SFD.Tiles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SFDCT.Assets;

internal static class TexturesLoader
{
    internal static bool Load(GameSFD game)
    {
        game.ShowLoadingText(LanguageHelper.GetText("loading.textures"));

        var contents = SubContentHandler.GetContents()
                        .Where(content => Directory.Exists(Path.Combine(content.Directory, Constants.Paths.DATA_IMAGES)))
                        .Reverse();

        var totalTextures = new Dictionary<string, string>();

        // get all texture files and their keys in reversed loading order,
        // so we dont keep duplicates from lower priority content
        foreach (var content in contents)
        {
            foreach (var textureFilePath in Directory.EnumerateFiles(Path.Combine(content.Directory, Constants.Paths.DATA_IMAGES), "*.png", SearchOption.AllDirectories))
            {
                var textureKey = Path.GetFileNameWithoutExtension(textureFilePath).ToUpperInvariant();

                if (totalTextures.ContainsKey(textureKey)) continue;
                totalTextures.Add(textureKey, textureFilePath);
            }
        }

        // actually read and load unique textures
        foreach (var path in totalTextures.Values)
        {
            ConsoleOutput.ShowMessage(ConsoleOutputType.Loading, $"Loading texture file: {path}");

            // re-use SFD's code to load textures, it is modified 
            // from some patches in SubContentHandler to support
            // absolute paths instead of only relative paths to
            // SFD's content folder
            Textures.m_tileTextures.Load(path);
        }

        return true;
    }
}
