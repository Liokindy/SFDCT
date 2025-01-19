using SFD.Tiles;
using SFD;
using System;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using HarmonyLib;
using CGlobals = SFDCT.Misc.Globals;
using CIni = SFDCT.Settings.Values;

namespace SFDCT.Bootstrap.Assets
{
    [HarmonyPatch]
    internal static class TexturesLoader
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SFD.Tiles.Textures), nameof(SFD.Tiles.Textures.Load), [typeof(string)])]
        internal static bool Load(string path)
        {
            if (!CIni.Get<bool>(CIni.GetKey(CIni.SettingKey.Use140Assets)))
            {
                return true;
            }

            foreach (FileInfo fileInfo in (from f in new DirectoryInfo(Constants.Paths.GetPath(CGlobals.Paths.CONTENT, path)).EnumerateFiles("*.*", SearchOption.AllDirectories)
                                           where f.Extension == ".xnb" || f.Extension == ".png"
                                           select f).ToArray<FileInfo>())
            {
                ConsoleOutput.ShowMessage(ConsoleOutputType.Loading, "Loading texture: " + fileInfo.FullName);
                Textures.m_tileTextures.Load(fileInfo.FullName, false);
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SFD.Tiles.Textures), nameof(SFD.Tiles.Textures.LoadTexture))]
        internal static bool LoadTexture(ref Texture2D __result, string texture)
        {
            if (!CIni.Get<bool>(CIni.GetKey(CIni.SettingKey.Use140Assets)))
            {
                return true;
            }

            if (Textures.m_tileTextures.TextureExists(texture))
            {
                __result = Textures.m_tileTextures.GetTexture(texture);
                return false;
            }
            string text = Constants.GetLoadPath(texture);
            text = Path.Combine(Constants.Paths.ContentPath, texture);
            if (File.Exists(Path.ChangeExtension(text, ".xnb")))
            {
                text = Path.ChangeExtension(text, ".xnb");
            }
            else if (File.Exists(Path.ChangeExtension(text, ".png")))
            {
                text = Path.ChangeExtension(text, ".png");
            }
            Textures.m_tileTextures.Load(text, true);
            __result = Textures.GetTexture(Path.GetFileNameWithoutExtension(texture));
            return false;
        }
    }

    [HarmonyPatch]
    internal static class TileTexturesLoader
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SFD.Tiles.TileTextures), nameof(SFD.Tiles.TileTextures.Load))]
        internal static bool Load(TileTextures __instance, string textureFile, bool ignoreErrors = false)
        {
            if (!CIni.Get<bool>(CIni.GetKey(CIni.SettingKey.Use140Assets)))
            {
                return true;
            }

            string path = Constants.Paths.GetPath(Constants.Paths.ExecutablePath, textureFile);
            if (File.Exists(path))
            {
                string contentAssetPathFromFullPath = path; //Constants.Paths.GetContentAssetPathFromFullPath(path);
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(textureFile);
                if (__instance.TextureExists(fileNameWithoutExtension))
                {
                    Texture2D texture = __instance.GetTexture(fileNameWithoutExtension);
                    if (texture != null && texture.IsDisposed)
                    {
                        __instance.RemoveTexture(fileNameWithoutExtension);
                    }
                    else if (TileTextures.m_alreadyLoadedFiles.Contains(path))
                    {
                        return false;
                    }
                }
                TileTextures.m_alreadyLoadedFiles.Add(path);
                string text = path.Replace(Constants.ExecutablePath, Constants.Paths.UserDocumentsSFDUserDataPath);
                text = Path.ChangeExtension(text, ".png");
                if (File.Exists(text))
                {
                    try
                    {
                        Utils.WaitForGraphicsDevice();
                        Texture2D texture2D = __instance.PremultiplyTexture(text, __instance.m_game.GraphicsDevice);
                        if (texture2D != null)
                        {
                            __instance.AddTexture(texture2D, fileNameWithoutExtension.ToUpperInvariant());
                            return false;
                        }
                    }
                    catch (Exception)
                    {
                        ConsoleOutput.ShowMessage(ConsoleOutputType.Error, string.Format("Loading custom texture '{0}' failed", textureFile));
                    }
                }
                Texture2D texture2D2 = null;
                Utils.WaitForGraphicsDevice();
                try
                {
                    if (texture2D2 == null)
                    {
                        lock (GameSFD.SpriteBatchResourceObject)
                        {
                            texture2D2 = Content.Load<Texture2D>(contentAssetPathFromFullPath);
                        }
                    }
                }
                catch
                {
                    try
                    {
                        lock (GameSFD.SpriteBatchResourceObject)
                        {
                            texture2D2 = Content.Load<Texture2DB>(contentAssetPathFromFullPath).Texture;
                        }
                    }
                    catch
                    {
                        ConsoleOutput.ShowMessage(ConsoleOutputType.Error, string.Format("Failed to load texture {0}", textureFile));
                        return false;
                    }
                }
                if (texture2D2 != null)
                {
                    __instance.AddTexture(texture2D2, fileNameWithoutExtension.ToUpperInvariant());
                }
                return false;
            }
            if (ignoreErrors)
            {
                ConsoleOutput.ShowMessage(ConsoleOutputType.Error, "Error: Could not find '" + textureFile + "', make sure it exist");
                return false;
            }
            throw new Exception("Error: Could not find '" + textureFile + "', make sure it exist");
        }
    }
}
