using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection.Emit;
using Microsoft.Xna.Framework;
using SFD;
using SFD.MenuControls;
using HarmonyLib;
using SFDCT.Helper;

namespace SFDCT.Fighter;

[HarmonyPatch]
internal static class ProfilesHandler
{
    private const int ExtendedProfileCount = 9;
    private const int ProfilesPerRow = 6;
    private const int ProfileSeparation = 88;

    /// <summary>
    ///     Remove the call for RefreshGrid and only call it after we add
    ///     the extra ProfileGridItems.
    /// </summary>
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(SFD.MenuControls.ProfilePanel), MethodType.Constructor)]
    private static IEnumerable<CodeInstruction> PatchProfilePanelConstructor(IEnumerable<CodeInstruction> instructions)
    {
        instructions.ElementAt(293).opcode = OpCodes.Nop;
        instructions.ElementAt(294).opcode = OpCodes.Nop;
        return instructions;
    }

    /// <summary>
    ///     Modify the profile panel size, and add the extra ProfileGridItems.
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(SFD.MenuControls.ProfilePanel), MethodType.Constructor)]
    private static void AddExtendedProfilePanels(SFD.MenuControls.ProfilePanel __instance)
    {
        // Make the menu larger
        __instance.Width = ProfilesPerRow * ProfileSeparation + 12;
        __instance.Height = (int)System.Math.Ceiling((9 + ExtendedProfileCount) / (float)ProfilesPerRow) * ProfileSeparation + 84 + 44;
        __instance.m_menu.Width = __instance.Width;
        __instance.m_menu.LocalPosition = new Vector2(0, __instance.Height - 84);

        // Modify existing ProfileGridItems
        int xx, yy;
        xx = 0;
        yy = 24;
        for (int i = 0; i < 9 + ExtendedProfileCount; i++)
        {
            if (i <= 8)
            {
                __instance.members[i].LocalPosition = new Vector2(xx, yy);
            }
            else
            {
                __instance.members.Insert(i, new ProfileGridItem(new Vector2(xx, yy), __instance));
            }

            xx += ProfileSeparation;
            if (xx >= ProfilesPerRow * ProfileSeparation)
            {
                xx = 0;
                yy += ProfileSeparation;
            }
        }

        /*
        __instance.members.ElementAt(0).LocalPosition = new Vector2(0, 32);
        __instance.members.ElementAt(1).LocalPosition = new Vector2(90, 32);
        __instance.members.ElementAt(2).LocalPosition = new Vector2(180, 32);
        __instance.members.ElementAt(3).LocalPosition = new Vector2(270, 32);
        __instance.members.ElementAt(4).LocalPosition = new Vector2(360, 32);
        __instance.members.ElementAt(5).LocalPosition = new Vector2(450, 32);
        __instance.members.ElementAt(6).LocalPosition = new Vector2(0, 32 + 90);
        __instance.members.ElementAt(7).LocalPosition = new Vector2(90, 32 + 90);
        __instance.members.ElementAt(8).LocalPosition = new Vector2(180, 32 + 90);
        */

        // Add new ones
        /*
        for(int i = 0; i < ExtendedProfileCount; i++)
        {
            __instance.members.Insert(9 + i, new ProfileGridItem(new Vector2(xx, yy), __instance));
            xx += 90;
            if (xx > 450)
            {
                xx = 0;
                yy += 90;
            }
        }
        */

        // Arrow key navigation
        // [0 ] [1 ] [2 ] [3 ] [4 ] [5]
        // [6 ] [7 ] [8 ] [9 ] [10] [11]
        // [12] [13] [14] [15] [16] [17]
        // [18]
        __instance.m_menu.NeighborUpId = __instance.members.Count - 7;
        int menuID = __instance.members.Count - 1;

        for(int i = 0; i < __instance.members.Count - 1; i++)
        {
            __instance.members[i].NeighborLeftId = -1;
            __instance.members[i].NeighborUpId = -1;
            __instance.members[i].NeighborRightId = -1;
            __instance.members[i].NeighborDownId = -1;

            // Left
            if (__instance.members.ElementAtOrDefault(i - 1) != null)
            {
                __instance.members[i].NeighborLeftId = i - 1;
            }
            // Right
            if (__instance.members.ElementAtOrDefault(i + 1) != null)
            {
                __instance.members[i].NeighborRightId = i + 1;
            }
            // Down
            if (__instance.members.ElementAtOrDefault(i + ProfilesPerRow) != null)
            {
                __instance.members[i].NeighborDownId = i + ProfilesPerRow;
            }
            // Up
            if (__instance.members.ElementAtOrDefault(i - ProfilesPerRow) != null)
            {
                __instance.members[i].NeighborUpId = i - ProfilesPerRow;
            }

            // Left/Right Edges
            if (i % ProfilesPerRow == 0)
            {
                __instance.members[i].NeighborLeftId = -1;
            }
            if ((i + 1) % ProfilesPerRow == 0)
            {
                __instance.members[i].NeighborRightId = -1;
            }

            // Bottom row
            if (i >= (__instance.members.Count - 1) - ProfilesPerRow)
            {
                __instance.members[i].NeighborDownId = menuID;
            }
        }
        __instance.RefreshGrid();
    }

    /// <summary>
    ///     Using the keyboard focuses the menu, patch the ID.
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ProfilePanel), nameof(ProfilePanel.CheckSelectedItem))]
    private static void PatchOnEnterPanel(SFD.MenuControls.ProfilePanel __instance, bool keyboard)
    {
        if (keyboard)
        {
            __instance.SelectMember(__instance.members.Count - 1);
        }
    }
    
    /// <summary>
    ///     Resize the SavedProfiles array
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Profile), nameof(Profile.LoadProfiles))]
    private static void ResizeSavedProfiles()
    {
        if (Profile.SavedProfiles.Length <= 9)
        {
            Profile.SavedProfiles = new Profile[9 + ExtendedProfileCount];
        }
    }

    /// <summary>
    ///     Load extended profiles
    /// </summary>
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(Profile), nameof(Profile.LoadProfilesInstances))]
    private static IEnumerable<CodeInstruction> LoadExtendedProfiles(IEnumerable<CodeInstruction> instructions)
    {
        instructions.ElementAt(26).opcode = OpCodes.Ldc_I4_S;
        instructions.ElementAt(26).operand = 8 + ExtendedProfileCount;
        return instructions;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Profile), nameof(Profile.Load))]
    private static bool Load(ref Profile __result, int slot)
    {
        if (slot <= 8)
        {
            // Use vanilla method to load normal profiles.
            return true;
        }
        Logger.LogDebug($"PROFILE: Loading {slot}...");

        // Return a default profile if we can't load the requested profile
        __result = new Profile();
        string pathToProfileFile = Path.GetFullPath(Path.Combine(Misc.Globals.Paths.PROFILES, $"profile{slot}.sfdp"));

        if (!File.Exists(pathToProfileFile))
        {
            __result = null;
            return false;
        }

        using FileStream fileStream = new(pathToProfileFile, FileMode.Open, FileAccess.Read, FileShare.Read);
        using BinaryReader binaryReader = new(fileStream);

        // This is writte in case the player wants to drag-and-drop the profiles
        // into SFD's profile folder while playing unpatched.
        string version = binaryReader.ReadString();

        __result.Name = binaryReader.ReadString();
        __result.Gender = (Player.GenderType)binaryReader.ReadInt16();

        // Read clothing items
        for (int i = 0; i < 9; i++)
        {
            if (!binaryReader.ReadBoolean())
            {
                __result.EquippedItems[i] = null;
                continue;
            }

            try
            {
                __result.EquippedItems[i] = Items.GetItem(binaryReader.ReadString());
            }
            catch
            {
                List<Item> useableAndUnlockedItems = Items.GetUseableAndUnlockedItems(__result.GenderItems, i);
                if (useableAndUnlockedItems.Count > 0)
                {
                    __result.EquippedItems[i] = useableAndUnlockedItems[Constants.RANDOM.Next(0, useableAndUnlockedItems.Count)];
                }
                else
                {
                    __result.EquippedItems[i] = null;
                }
            }

            __result.EquippedItemsColors[i][0] = binaryReader.ReadString();
            __result.EquippedItemsColors[i][1] = binaryReader.ReadString();
            __result.EquippedItemsColors[i][2] = binaryReader.ReadString();
        }

        binaryReader.Close();
        fileStream.Close();

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Profile), nameof(Profile.Save))]
    private static bool Save(Profile profileToSave, int slot)
    {
        if (slot <= 8)
        {
            // Use vanilla method to save normal profiles.
            return true;
        }
        Logger.LogDebug($"PROFILE: Saving {slot}...");

        string pathToProfileFile = Path.GetFullPath(Path.Combine(Misc.Globals.Paths.PROFILES, $"profile{slot}.sfdp"));
        profileToSave.ValidateProfileIntegrity(true, Profile.ValidateProfileType.CanEquipAndUnlocked);

        // Create "SFDCT/Profile" folder if it doesn't exist
        if (!Directory.Exists(Misc.Globals.Paths.PROFILES))
        {
            Directory.CreateDirectory(Misc.Globals.Paths.PROFILES);
        }

        using FileStream fileStream = new(pathToProfileFile, FileMode.Create);
        using BinaryWriter binaryWriter = new(fileStream);

        binaryWriter.Write(Misc.Globals.Version.SFD);
        binaryWriter.Write(profileToSave.Name);
        binaryWriter.Write((short)profileToSave.Gender);
        for (int i = 0; i < 9; i++)
        {
            if (profileToSave.EquippedItems[i] == null)
            {
                binaryWriter.Write(false);
                continue;
            }

            binaryWriter.Write(true);
            binaryWriter.Write(profileToSave.EquippedItems[i].ID);
            binaryWriter.Write(profileToSave.EquippedItemsColors[i][0]);
            binaryWriter.Write(profileToSave.EquippedItemsColors[i][1]);
            binaryWriter.Write(profileToSave.EquippedItemsColors[i][2]);
        }

        binaryWriter.Close();
        fileStream.Close();
        Profile.SavedProfiles[slot] = profileToSave;

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Profile), nameof(Profile.DeleteProfile))]
    private static bool Delete(int slot)
    {
        if (slot <= 8)
        {
            return true;
        }
        Logger.LogDebug($"PROFILE: Deleting {slot}...");

        string pathToProfileFile = Path.GetFullPath(Path.Combine(Misc.Globals.Paths.PROFILES, $"profile{slot}.sfdp"));
        if (File.Exists(pathToProfileFile))
        {
            try
            {
                File.Delete(pathToProfileFile);
            }
            catch(Exception ex)
            {
                Logger.LogError($"PROFILE: Cannot delete '{pathToProfileFile}' - {ex.Message}");
            }
        }

        Profile.SavedProfiles[slot] = null;
        Profile.RefreshPlayerProfiles();
        return false;
    }
}
