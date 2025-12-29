using Microsoft.Xna.Framework.Graphics;
using SFD;
using SFD.Code;
using SFD.Tiles;
using SFDCT.Helper;
using SFDCT.Misc;
using System;
using System.Collections.Generic;
using System.IO;

namespace SFDCT.Assets;

internal static class TextureLoader
{
    internal static void LoadTexture(string filePath)
    {
        if (!File.Exists(filePath)) return;

        string textureName = Path.GetFileNameWithoutExtension(filePath);
        if (Textures.m_tileTextures.TextureExists(textureName))
        {
            Texture2D existingTexture = Textures.GetTexture(textureName);
            if (existingTexture != null && !existingTexture.IsDisposed)
            {
                Textures.m_tileTextures.RemoveTexture(textureName);
            }
            else if (TileTextures.m_alreadyLoadedFiles.Contains(filePath))
            {
                return;
            }
        }

        TileTextures.m_alreadyLoadedFiles.Add(filePath);

        string documentsPath = filePath.Replace(Globals.Paths.SubContent, Constants.Paths.UserDocumentsSFDUserDataPath).Replace(Constants.ExecutablePath, Constants.Paths.UserDocumentsSFDUserDataPath);
        if (File.Exists(documentsPath))
        {
            try
            {
                Utils.WaitForGraphicsDevice();
                Texture2D texturePNG = Textures.m_tileTextures.PremultiplyTexture(documentsPath, Textures.m_tileTextures.m_game.GraphicsDevice);

                if (texturePNG != null)
                {
                    Textures.m_tileTextures.AddTexture(texturePNG, textureName.ToUpperInvariant());
                    return;
                }
            }
            catch (Exception)
            {
                ConsoleOutput.ShowMessage(ConsoleOutputType.Error, string.Format("Loading custom texture '{0}' failed", documentsPath));
            }
        }

        Texture2D texture = null;
        Utils.WaitForGraphicsDevice();

        try
        {
            lock (GameSFD.SpriteBatchResourceObject)
            {
                texture = Content.Load<Texture2D>(filePath);
            }
        }
        catch
        {
            texture = null;

            try
            {
                lock (GameSFD.SpriteBatchResourceObject)
                {
                    texture = Content.Load<Texture2DB>(filePath).Texture;
                }
            }
            catch
            {
                texture = null;

                ConsoleOutput.ShowMessage(ConsoleOutputType.Error, string.Format("Failed to load texture {0}", textureName));
            }
        }

        if (texture != null) Textures.m_tileTextures.AddTexture(texture, textureName.ToUpperInvariant());
    }

    internal static void Load()
    {
        Dictionary<string, string> textureFilesDic = [];

        Logger.LogInfo($"LOADING [TEXTURES]: Official");
        string officialImagesPath = Path.Combine(Constants.Paths.ContentPath, Constants.Paths.DATA_IMAGES);
        foreach (var filePath in Directory.GetFiles(officialImagesPath, "*.png", SearchOption.AllDirectories))
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);

            textureFilesDic.Add(fileName, filePath);
        }

        string documentsImagesPath = Path.Combine(Constants.Paths.UserDocumentsContentCustomPath, Constants.Paths.DATA_IMAGES);

        if (Directory.Exists(documentsImagesPath))
        {
            Logger.LogInfo($"LOADING [TEXTURES]: Documents");

            foreach (var filePath in Directory.GetFiles(documentsImagesPath, "*.png", SearchOption.AllDirectories))
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);

                if (textureFilesDic.ContainsKey(fileName))
                {
                    textureFilesDic[fileName] = filePath;
                }
            }
        }

        foreach (var subContentFolder in SubContentHandler.Folders)
        {
            string subContentImagesPath = SubContentHandler.GetPath(subContentFolder, Constants.Paths.DATA_IMAGES);

            if (Directory.Exists(subContentImagesPath))
            {
                Logger.LogInfo($"LOADING [TEXTURES]: {subContentFolder}");

                foreach (var filePath in Directory.GetFiles(subContentImagesPath, "*.png", SearchOption.AllDirectories))
                {
                    string fileName = Path.GetFileNameWithoutExtension(filePath);

                    if (textureFilesDic.ContainsKey(filePath))
                    {
                        textureFilesDic[fileName] = filePath;
                    }
                }
            }
        }

        foreach (var filePath in textureFilesDic.Values)
        {
            ConsoleOutput.ShowMessage(ConsoleOutputType.Loading, "Loading texture: " + filePath);

            LoadTexture(filePath);
        }
    }
}
