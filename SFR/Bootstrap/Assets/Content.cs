using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using SFD;
using System;
using System.Collections.Generic;
using System.IO;

namespace SFDCT.Bootstrap.Assets
{
    /// <summary>
    ///     We praise to our god, the Odex64 :pray:
    ///     xoxo
    /// </summary>
    internal static class Content
    {
        internal static bool IsXnb(string path)
        {
            string extension = Path.GetExtension(path);
            return extension == ".xnb" || (string.IsNullOrEmpty(extension) && File.Exists(Path.ChangeExtension(GetFullContentPath(path), "xnb")));
        }
        internal static string GetFullContentPath(string path)
        {
            if (path.StartsWith("Data"))
            {
                return Path.Combine("Content", path);
            }
            return path;
        }

        internal static T Load<T>(string path)
        {
            Type typeFromHandle = typeof(T);
            foreach (KeyValuePair<Type, Content.RawContent> keyValuePair in Content._loaders)
            {
                if (keyValuePair.Key == typeFromHandle)
                {
                    if (Content.IsXnb(path))
                    {
                        return GameSFD.Handle.Content.Load<T>(path);
                    }
                    using (FileStream fileStream = File.OpenRead(Path.ChangeExtension(GetFullContentPath(path), keyValuePair.Value.Extension)))
                    {
                        return (T)((object)keyValuePair.Value.Loader(fileStream));
                    }
                }
            }
            return default(T);
        }

        private static AnimationsData LoadAnimations(FileStream stream)
        {
            AnimationsData result;
            using (BinaryReader binaryReader = new BinaryReader(stream))
            {
                int num = binaryReader.ReadInt32();
                AnimationData[] array = new AnimationData[num];
                for (int i = 0; i < num; i++)
                {
                    string name = binaryReader.ReadString();
                    int num2 = binaryReader.ReadInt32();
                    AnimationFrameData[] array2 = new AnimationFrameData[num2];
                    for (int j = 0; j < num2; j++)
                    {
                        string frameEvent = binaryReader.ReadString();
                        int time = binaryReader.ReadInt32();
                        int num3 = binaryReader.ReadInt32();
                        AnimationCollisionData[] array3 = new AnimationCollisionData[num3];
                        for (int k = 0; k < num3; k++)
                        {
                            int id = binaryReader.ReadInt32();
                            float width = binaryReader.ReadSingle();
                            float height = binaryReader.ReadSingle();
                            float x = binaryReader.ReadSingle();
                            float y = binaryReader.ReadSingle();
                            array3[k] = new AnimationCollisionData(id, x, y, width, height);
                        }
                        int num4 = binaryReader.ReadInt32();
                        AnimationPartData[] array4 = new AnimationPartData[num4];
                        for (int l = 0; l < num4; l++)
                        {
                            int id2 = binaryReader.ReadInt32();
                            float x2 = binaryReader.ReadSingle();
                            float y2 = binaryReader.ReadSingle();
                            float rotation = binaryReader.ReadSingle();
                            SpriteEffects flip = (SpriteEffects)binaryReader.ReadInt32();
                            float sx = binaryReader.ReadSingle();
                            float sy = binaryReader.ReadSingle();
                            string postFix = binaryReader.ReadString();
                            array4[l] = new AnimationPartData(id2, x2, y2, rotation, flip, sx, sy, postFix);
                        }
                        binaryReader.ReadChar();
                        array2[j] = new AnimationFrameData(array4, array3, frameEvent, time);
                    }
                    binaryReader.ReadChar();
                    array[i] = new AnimationData(array2, name);
                }
                result = new AnimationsData(array);
            }
            return result;
        }

        private static Texture2DB LoadEffect(FileStream stream)
        {
            List<Color> list = new List<Color>();
            Texture2DB result;
            using (BinaryReader binaryReader = new BinaryReader(stream))
            {
                int num = (int)binaryReader.ReadByte();
                for (int i = 0; i < num; i++)
                {
                    byte r = binaryReader.ReadByte();
                    byte g = binaryReader.ReadByte();
                    byte b = binaryReader.ReadByte();
                    byte a = binaryReader.ReadByte();
                    list.Add(new Color((int)r, (int)g, (int)b, (int)a));
                }
                int num2 = binaryReader.ReadInt32();
                int num3 = binaryReader.ReadInt32();
                Color[] array = new Color[num2 * num3];
                Color color = default(Color);
                for (int j = 0; j < array.Length; j++)
                {
                    if (binaryReader.ReadBoolean())
                    {
                        array[j] = new Color((int)color.R, (int)color.G, (int)color.B, (int)color.A);
                    }
                    else
                    {
                        byte index = binaryReader.ReadByte();
                        array[j] = list[(int)index];
                        color = array[j];
                    }
                }
                Texture2DB texture2DB = new Texture2DB();
                texture2DB.Texture = Utils.NewTexture2D(GameSFD.Handle.GraphicsDevice, num2, num3);
                texture2DB.Texture.SetData<Color>(array);
                result = texture2DB;
            }
            return result;
        }

        private static Texture2D LoadTexture(FileStream stream)
        {
            return Texture2D.FromStream(GameSFD.Handle.GraphicsDevice, stream);
        }

        internal static void PreMultiply(this Texture2D texture)
        {
            Color[] array = new Color[texture.Width * texture.Height];
            texture.GetData<Color>(array);
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = Color.FromNonPremultiplied((int)array[i].R, (int)array[i].G, (int)array[i].B, (int)array[i].A);
            }
            texture.SetData<Color>(array);
        }

        private static SoundEffect LoadSound(FileStream stream)
        {
            return SoundEffect.FromStream(stream);
        }

        private static Item LoadItem(FileStream stream)
        {
            Item result;
            using (BinaryReader binaryReader = new BinaryReader(stream))
            {
                string fileName = binaryReader.ReadString();
                string gameName = binaryReader.ReadString();
                int equipmentLayer = binaryReader.ReadInt32();
                string id = binaryReader.ReadString();
                bool jacketUnderBelt = binaryReader.ReadBoolean();
                bool canEquip = binaryReader.ReadBoolean();
                bool canScript = binaryReader.ReadBoolean();
                Texture2D image = null;
                string colorPalette = binaryReader.ReadString();
                int num = binaryReader.ReadInt32();
                int num2 = binaryReader.ReadInt32();
                List<Color> list = new List<Color>();
                int num3 = (int)binaryReader.ReadByte();
                for (int i = 0; i < num3; i++)
                {
                    byte r = binaryReader.ReadByte();
                    byte g = binaryReader.ReadByte();
                    byte b = binaryReader.ReadByte();
                    byte a = binaryReader.ReadByte();
                    list.Add(new Color((int)r, (int)g, (int)b, (int)a));
                }
                int num4 = binaryReader.ReadInt32();
                binaryReader.ReadChar();
                ItemPart[] array = new ItemPart[num4];
                for (int j = 0; j < num4; j++)
                {
                    int type = binaryReader.ReadInt32();
                    int num5 = binaryReader.ReadInt32();
                    int num6 = num * num2;
                    Texture2D[] array2 = new Texture2D[num5];
                    for (int k = 0; k < num5; k++)
                    {
                        if (binaryReader.ReadBoolean())
                        {
                            Color color = default(Color);
                            Color[] array3 = new Color[num6];
                            for (int l = 0; l < num6; l++)
                            {
                                if (binaryReader.ReadBoolean())
                                {
                                    array3[l] = new Color((int)color.R, (int)color.G, (int)color.B, (int)color.A);
                                }
                                else
                                {
                                    byte index = binaryReader.ReadByte();
                                    array3[l] = list[(int)index];
                                    color = array3[l];
                                }
                            }
                            binaryReader.ReadChar();
                            array2[k] = Utils.NewTexture2D(GameSFD.Handle.GraphicsDevice, num, num2);
                            array2[k].SetData<Color>(array3);
                        }
                        else
                        {
                            array2[k] = null;
                        }
                    }
                    array[j] = new ItemPart(array2, type);
                }
                result = new Item(array, image, gameName, fileName, equipmentLayer, id, jacketUnderBelt, canEquip, canScript, colorPalette);
            }
            return result;
        }

        private static readonly Dictionary<Type, Content.RawContent> _loaders = new Dictionary<Type, Content.RawContent>
        {
            {
                typeof(AnimationsData),
                new Content.RawContent(string.Empty, new Func<FileStream, object>(Content.LoadAnimations))
            },
            {
                typeof(Texture2DB),
                new Content.RawContent(".effect", new Func<FileStream, object>(Content.LoadEffect))
            },
            {
                typeof(Texture2D),
                new Content.RawContent(".png", new Func<FileStream, object>(Content.LoadTexture))
            },
            {
                typeof(SoundEffect),
                new Content.RawContent(".wav", new Func<FileStream, object>(Content.LoadSound))
            },
            {
                typeof(Item),
                new Content.RawContent(".item", new Func<FileStream, object>(Content.LoadItem))
            }
        };

        private class RawContent
        {
            internal RawContent(string extension, Func<FileStream, object> loader)
            {
                this.Extension = extension;
                this.Loader = loader;
            }

            internal string Extension;

            internal Func<FileStream, object> Loader;
        }
    }
}
