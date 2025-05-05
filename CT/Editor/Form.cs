//using System;
//using System.Collections.Generic;
//using System.Drawing;
//using System.IO;
//using System.Threading.Tasks;
//using System.Windows.Forms;
//using HarmonyLib;
//using SFD;
//using SFD.Core;
//using SFD.MapEditor;
//using SFD.States;
//using SFD.Tiles;
//using CConst = SFDCT.Misc.Globals;

//namespace SFDCT.Editor;

//[HarmonyPatch]
//internal static class Form
//{
//    /// <summary>
//    ///     The game checks the version written into the ImagesList.sfdx file,
//    ///     if it's different it rebuilds it again. This makes the game build it
//    ///     with the target SFD version instead of "v.1.3.7x". So if the
//    ///     user uses vanilla-SFD, the imagelist won't be re-built.
//    /// </summary>
//    [HarmonyPrefix]
//    [HarmonyPatch(typeof(StateEditor), nameof(StateEditor.Load))]
//    private static bool EditorLoad(StateEditor __instance, ref LoadState loadingState)
//    {
//        if (!__instance.m_testing)
//        {
//            GameplayTips.SetTip(GameplayTips.Tip.Random);

//            if (!__instance.m_isLoaded)
//            {
//                MapEditorHelp.LoadMapEditorHelp();
//                __instance.m_isLoaded = true;
//            }

//            __instance.ActiveEditorMap = null;
//            __instance.EditorMaps = [];
//            __instance.m_mapEditorForm = new(__instance.m_game);
//            __instance.m_mapEditorForm.FormClosing += __instance.MapEditorForm_FormClosing;
//            //__instance.m_mapEditorForm.BuildTreeViewImageList();
//            EditorBuildTreeViewImageList(__instance.m_mapEditorForm);
//            __instance.m_mapEditorForm.SetNodeData();
//        }

//        return false;
//    }

//    // ugly ctrl c ctrl v code straight from dnspy
//    private static void EditorBuildTreeViewImageList(SFDMapEditor mapEditorForm)
//    {
//        Image image = null;
//        lock (GameSFD.SpriteBatchResourceObject)
//        {
//            image = Texture2DToImage.Texture2Image(Constants.ErrorTexture8x8, 16, 16);
//        }

//        mapEditorForm.m_imageList_16x16.Images.Add("NONE", image);
//        mapEditorForm.m_imageList_16x16.Images.Add("NULL", image);
//        if (Constants.EDITOR.EDITOR_DISABLE_LIST_IMAGES)
//        {
//            mapEditorForm.m_imageList_16x16.TransparentColor = System.Drawing.Color.Magenta;
//            mapEditorForm.treeViewTiles.ImageList = mapEditorForm.m_imageList_16x16;
//            return;
//        }

//        string path = Path.Combine(Constants.Paths.UserDocumentsCachePath, "ListImagesFailed.txt");
//        if (File.Exists(path))
//        {
//            try
//            {
//                string[] array = File.ReadAllLines(path);
//                if (array != null && array.Length > 0 && array[0] == CConst.Version.SFD) //"v.1.3.7x")
//                {
//                    mapEditorForm.m_imageList_16x16.TransparentColor = System.Drawing.Color.Magenta;
//                    mapEditorForm.treeViewTiles.ImageList = mapEditorForm.m_imageList_16x16;
//                    return;
//                }
//            }
//            catch (Exception ex)
//            {
//                ConsoleOutput.ShowMessage(ConsoleOutputType.Error, "Error reading ListImagesFailed.txt " + ex.Message);
//            }
//        }

//        Constants.EDITOR_LOADING_LIST_IMAGES = true;
//        List<ItemContainer<string, Tile>> list = new List<ItemContainer<string, Tile>>();
//        HashSet<string> hashSet = new HashSet<string>();
//        List<Tuple<string, byte[]>> cachedImgList = new List<Tuple<string, byte[]>>();
//        string fullPath = Path.GetFullPath(Path.Combine(Constants.Paths.UserDocumentsCachePath, "ListImages.sfdx"));
//        string directoryName = Path.GetDirectoryName(fullPath);
//        string a = "";
//        if (File.Exists(fullPath))
//        {
//            try
//            {
//                using (Stream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read))
//                {
//                    using (SFDBinaryReader sfdbinaryReader = new SFDBinaryReader(stream))
//                    {
//                        a = sfdbinaryReader.ReadString();
//                        if (a == CConst.Version.SFD) //"v.1.3.7x")
//                        {
//                            while (sfdbinaryReader.BaseStream.Position < sfdbinaryReader.BaseStream.Length)
//                            {
//                                string text = sfdbinaryReader.ReadString();
//                                byte[] item = sfdbinaryReader.ReadImageBytes();
//                                cachedImgList.Add(new Tuple<string, byte[]>(text, item));
//                                hashSet.Add(text);
//                            }
//                        }
//                        sfdbinaryReader.Close();
//                    }
//                }
//            }
//            catch (Exception exception)
//            {
//                try
//                {
//                    SFD.Program.LogErrorMessage("sfd_editor_readcache_crash", exception);
//                }
//                catch
//                {
//                }
//            }
//        }
//        foreach (KeyValuePair<string, Tile> keyValuePair in TileDatabase.Tiles)
//        {
//            string text2 = (!string.IsNullOrEmpty(keyValuePair.Value.ListTextureName)) ? keyValuePair.Value.ListTextureName : keyValuePair.Value.TextureName;
//            if (!hashSet.Contains(text2))
//            {
//                list.Add(new ItemContainer<string, Tile>(text2, keyValuePair.Value));
//            }
//        }
//        if (list.Count > 0)
//        {
//            List<Pair<string, Image>> list2 = new List<Pair<string, Image>>();
//            int num = 0;
//            foreach (ItemContainer<string, Tile> pair in list)
//            {
//                num++;
//                float num2 = (float)num / (float)list.Count;
//                GameSFD.Handle.ShowLoadingText(string.Format("{0} {1:f0} %", LanguageHelper.GetText("loading.mapeditor.creatingListImages"), num2 * 100f));
//                Pair<string, Image> pair2 = mapEditorForm.BuildTreeViewImageListProcess(pair);
//                if (pair2 != null)
//                {
//                    list2.Add(pair2);
//                }
//            }
//            try
//            {
//                GameSFD.Handle.ShowLoadingText("Saving List Images...");
//                if (!Directory.Exists(directoryName))
//                {
//                    Directory.CreateDirectory(directoryName);
//                }
                
//                //FileMode mode = (a == "v.1.3.7x") ? FileMode.Append : FileMode.Create;
//                FileMode mode = (a == CConst.Version.SFD) ? FileMode.Append : FileMode.Create;
//                using (Stream stream2 = new FileStream(fullPath, mode, FileAccess.Write, FileShare.None))
//                {
//                    using (SFDBinaryWriter sfdbinaryWriter = new SFDBinaryWriter(stream2))
//                    {
//                        if (sfdbinaryWriter.BaseStream.Length == 0L)
//                        {
//                            sfdbinaryWriter.Write(CConst.Version.SFD); //("v.1.3.7x");
//                        }
//                        foreach (Pair<string, Image> pair3 in list2)
//                        {
//                            sfdbinaryWriter.Write(pair3.ItemA);
//                            sfdbinaryWriter.Write(pair3.ItemB);
//                        }
//                        sfdbinaryWriter.Flush();
//                        sfdbinaryWriter.Close();
//                    }
//                }
//            }

//            catch (Exception exception2)
//            {
//                try
//                {
//                    SFD.Program.LogErrorMessage("sfd_editor_createcache_crash", exception2);
//                }
//                catch
//                {
//                }
//            }
//        }

//        GameSFD.Handle.ShowLoadingText(LanguageHelper.GetText("loading.mapeditor"));
//        if (cachedImgList.Count > 0)
//        {
//            Task.Factory.StartNew(delegate ()
//            {
//                ParallelOptions parallelOptions = new ParallelOptions();
//                if (Environment.ProcessorCount >= 4)
//                {
//                    parallelOptions.MaxDegreeOfParallelism = Environment.ProcessorCount - 1;
//                }
//                Parallel.ForEach<Tuple<string, byte[]>>(cachedImgList, parallelOptions, delegate (Tuple<string, byte[]> oImgData)
//                {
//                    if (!mapEditorForm.m_imageList_16x16_disposed)
//                    {
//                        Image image2 = null;
//                        using (Stream stream3 = new MemoryStream(oImgData.Item2))
//                        {
//                            image2 = Image.FromStream(stream3);
//                        }
//                        lock (mapEditorForm.m_imageList_16x16)
//                        {
//                            if (!mapEditorForm.m_imageList_16x16_disposed)
//                            {
//                                if (!mapEditorForm.m_imageList_16x16.Images.ContainsKey(oImgData.Item1))
//                                {
//                                    mapEditorForm.m_imageList_16x16.Images.Add(oImgData.Item1, image2);
//                                }
//                                else
//                                {
//                                    image2.Dispose();
//                                }
//                            }
//                            else
//                            {
//                                image2.Dispose();
//                            }
//                        }
//                    }
//                });
//                if (!mapEditorForm.m_imageList_16x16_disposed)
//                {
//                    lock (mapEditorForm.m_imageList_16x16)
//                    {
//                        if (!mapEditorForm.m_imageList_16x16_disposed)
//                        {
//                            mapEditorForm.m_imageList_16x16.TransparentColor = System.Drawing.Color.Magenta;
//                            mapEditorForm.treeViewTiles.Invoke(new Action(delegate ()
//                            {
//                                if (!mapEditorForm.m_imageList_16x16_disposed)
//                                {
//                                    lock (mapEditorForm.treeViewTiles)
//                                    {
//                                        ConsoleOutput.ShowMessage(ConsoleOutputType.MapEditor, "Updating list images - mapEditorForm can take a few seconds...");
//                                        mapEditorForm.Cursor = Cursors.WaitCursor;
//                                        mapEditorForm.treeViewTiles.BeginUpdate();
//                                        mapEditorForm.treeViewTiles.ImageList = mapEditorForm.m_imageList_16x16;
//                                        foreach (object obj2 in mapEditorForm.treeViewTiles.Nodes)
//                                        {
//                                            TreeNode node = (TreeNode)obj2;
//                                            mapEditorForm.UpdateTreeViewNodeImages(node);
//                                        }
//                                        mapEditorForm.treeViewTiles.EndUpdate();
//                                        mapEditorForm.Cursor = Cursors.Default;
//                                        ConsoleOutput.ShowMessage(ConsoleOutputType.MapEditor, "Done updating list images.");
//                                        Constants.EDITOR_LOADING_LIST_IMAGES = false;
//                                    }
//                                }
//                            }));
//                        }
//                    }
//                }
//            }, TaskCreationOptions.LongRunning);
//        }
//        else
//        {
//            mapEditorForm.m_imageList_16x16.TransparentColor = System.Drawing.Color.Magenta;
//            mapEditorForm.treeViewTiles.ImageList = mapEditorForm.m_imageList_16x16;
//            Constants.EDITOR_LOADING_LIST_IMAGES = false;
//        }
//        hashSet.Clear();
//        list.Clear();
//    }
//}