using SFD.Weapons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using SFD;
using SFD.Tiles;
using SFDCT.Helper;

namespace SFDCT.Weapons
{
    [HarmonyPatch]
    internal static class Database
    {
        public static Dictionary<short, WeaponItem> m_weaponsDic;
        public static Dictionary<short, WeaponItem> m_customWeaponsDic;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(WeaponDatabase), nameof(WeaponDatabase.GetWeapon), new Type[] { typeof(short) })]
        private static bool GetWeapon(ref WeaponItem __result, short weaponID)
        {
            if (m_customWeaponsDic.ContainsKey(weaponID))
            {
                __result = m_customWeaponsDic[weaponID];
                return false;
            }

            if (!m_weaponsDic.ContainsKey(weaponID))
            {
                ConsoleOutput.ShowMessage(ConsoleOutputType.Warning, "WDatabase.GetWeapon(short weaponID) value out of range - " + weaponID);
                __result = null;
                return false;
            }
            __result = m_weaponsDic[weaponID];
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(WeaponDatabase), nameof(WeaponDatabase.GetWeapon), new Type[] { typeof(string) })]
        private static bool GetWeapon(ref WeaponItem __result, string weaponName)
        {
            weaponName = weaponName.ToUpperInvariant();
            foreach (WeaponItem weapon in m_customWeaponsDic.Values)
            {
                if (weapon != null && weapon.BaseProperties.WeaponNameID != string.Empty && weapon.BaseProperties.WeaponNameID.ToUpperInvariant() == weaponName)
                {
                    __result = weapon;
                    return false;
                }
            }

            foreach (WeaponItem weapon in m_weaponsDic.Values)
            {
                if (weapon != null && weapon.BaseProperties.WeaponNameID != string.Empty && weapon.BaseProperties.WeaponNameID.ToUpperInvariant() == weaponName)
                {
                    __result = weapon;
                    return false;
                }
            }

            __result = null;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(WeaponDatabase), nameof(WeaponDatabase.GetWeaponItemOfType))]
        public static bool GetWeaponItemOfType(ref List<WeaponItem> __result, WeaponItemType wpnType)
        {
            List<WeaponItem> list = new List<WeaponItem>();
            foreach (WeaponItem weapon in m_customWeaponsDic.Values)
            {
                if (weapon != null && weapon.Type == wpnType && weapon.BaseProperties.WeaponCanBeEquipped)
                {
                    list.Add(weapon);
                }
            }

            foreach (WeaponItem weapon in m_weaponsDic.Values)
            {
                if (weapon != null && weapon.Type == wpnType && weapon.BaseProperties.WeaponCanBeEquipped)
                {
                    list.Add(weapon);
                }
            }

            __result = list;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(WeaponDatabase), nameof(WeaponDatabase.TotalWeapons))]
        public static bool TotalWeapons(ref int __result)
        {
            // Uses of this method needed to be replaced entirely
            if (m_weaponsDic != null)
            {
                List<short> weaponIDs = m_weaponsDic.Keys.ToList();
                weaponIDs.Sort();

                __result = weaponIDs.Last();
                return false;
            }
            __result = 0;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(WeaponDatabase), nameof(WeaponDatabase.TryGetWeapon))]
        public static bool TryGetWeapon(ref bool __result, short weaponID, out WeaponItem value)
        {
            if (m_customWeaponsDic.ContainsKey(weaponID))
            {
                value = m_weaponsDic[weaponID];
                if (value != null)
                {
                    return false;
                }
            }
            if (!m_weaponsDic.ContainsKey(weaponID))
            {
                value = null;
                return false;
            }

            value = m_weaponsDic[weaponID];
            return value != null;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(WeaponDatabase), nameof(WeaponDatabase.Load))]
        private static bool Load(GameSFD game)
        {
            m_weaponsDic = [];
            // IDs 69 - 255 (186 available)
            m_customWeaponsDic = [];
            Bootstrap.Scripts.CTWeaponsLoader.Load();

            WeaponDatabase.m_lazerAttachment = Textures.GetTexture("Lazer");
            
            WpnThrowingMode.Init();

            m_weaponsDic.Add(24, new WeaponItem(WeaponItemType.Handgun, new WpnPistol()));
            m_weaponsDic.Add(1, new WeaponItem(WeaponItemType.Handgun, new WpnMagnum()));
            m_weaponsDic.Add(28, new WeaponItem(WeaponItemType.Handgun, new WpnRevolver()));
            m_weaponsDic.Add(2, new WeaponItem(WeaponItemType.Rifle, new WpnPumpShotgun()));
            m_weaponsDic.Add(17, new WeaponItem(WeaponItemType.Rifle, new WpnBazooka()));
            m_weaponsDic.Add(6, new WeaponItem(WeaponItemType.Rifle, new WpnM60()));
            m_weaponsDic.Add(5, new WeaponItem(WeaponItemType.Rifle, new WpnTommygun()));
            m_weaponsDic.Add(19, new WeaponItem(WeaponItemType.Rifle, new WpnAssaultRifle()));
            m_weaponsDic.Add(26, new WeaponItem(WeaponItemType.Rifle, new WpnFlamethrower()));
            m_weaponsDic.Add(7, new WeaponItem(WeaponItemType.Melee, new WpnFists()));
            m_weaponsDic.Add(22, new WeaponItem(WeaponItemType.Melee, new WpnFeet()));
            m_weaponsDic.Add(3, new WeaponItem(WeaponItemType.Melee, new WpnKatana()));
            m_weaponsDic.Add(4, new WeaponItem(WeaponItemType.Melee, new WpnPipeWrench()));
            m_weaponsDic.Add(31, new WeaponItem(WeaponItemType.Melee, new WpnHammer()));
            m_weaponsDic.Add(8, new WeaponItem(WeaponItemType.Melee, new WpnMachete()));
            m_weaponsDic.Add(11, new WeaponItem(WeaponItemType.Melee, new WpnBat()));
            m_weaponsDic.Add(41, new WeaponItem(WeaponItemType.Melee, new WpnBaton()));
            m_weaponsDic.Add(10, new WeaponItem(WeaponItemType.Rifle, new WpnSawedOffShotgun()));
            m_weaponsDic.Add(12, new WeaponItem(WeaponItemType.Handgun, new WpnUzi()));
            m_weaponsDic.Add(18, new WeaponItem(WeaponItemType.Melee, new WpnAxe()));
            m_weaponsDic.Add(13, new WeaponItem(WeaponItemType.InstantPickup, new WpnPills()));
            m_weaponsDic.Add(14, new WeaponItem(WeaponItemType.InstantPickup, new WpnMedkit()));
            m_weaponsDic.Add(15, new WeaponItem(WeaponItemType.Powerup, new WpnSlowmo5()));
            m_weaponsDic.Add(16, new WeaponItem(WeaponItemType.Powerup, new WpnSlowmo10()));
            m_weaponsDic.Add(20, new WeaponItem(WeaponItemType.Thrown, new WpnGrenades()));
            m_weaponsDic.Add(25, new WeaponItem(WeaponItemType.Thrown, new WpnMolotovs()));
            m_weaponsDic.Add(27, new WeaponItem(WeaponItemType.Handgun, new WpnFlareGun()));
            m_weaponsDic.Add(29, new WeaponItem(WeaponItemType.Rifle, new WpnGrenadeLauncher()));
            m_weaponsDic.Add(9, new WeaponItem(WeaponItemType.Rifle, new WpnSniperRifle()));
            m_weaponsDic.Add(23, new WeaponItem(WeaponItemType.Rifle, new WpnCarbine()));
            m_weaponsDic.Add(42, new WeaponItem(WeaponItemType.Thrown, new WpnC4()));
            m_weaponsDic.Add(43, new WeaponItem(WeaponItemType.Thrown, new WpnC4Detonator()));
            m_weaponsDic.Add(44, new WeaponItem(WeaponItemType.Thrown, new WpnMines()));
            m_weaponsDic.Add(45, new WeaponItem(WeaponItemType.Thrown, new WpnShuriken()));
            m_weaponsDic.Add(21, new WeaponItem(WeaponItemType.InstantPickup, new WpnLazer()));
            m_weaponsDic.Add(30, new WeaponItem(WeaponItemType.Rifle, new WpnSMG()));
            m_weaponsDic.Add(32, new WeaponItem(WeaponItemType.Melee, new WpnChair()));
            m_weaponsDic.Add(33, new WeaponItem(WeaponItemType.Melee, new WpnChairLeg()));
            m_weaponsDic.Add(58, new WeaponItem(WeaponItemType.Melee, new WpnBaseball()));
            m_weaponsDic.Add(34, new WeaponItem(WeaponItemType.Melee, new WpnBottle()));
            m_weaponsDic.Add(35, new WeaponItem(WeaponItemType.Melee, new WpnBrokenBottle()));
            m_weaponsDic.Add(36, new WeaponItem(WeaponItemType.Melee, new WpnCueStick()));
            m_weaponsDic.Add(37, new WeaponItem(WeaponItemType.Melee, new WpnCueStickShaft()));
            m_weaponsDic.Add(38, new WeaponItem(WeaponItemType.Melee, new WpnSuitcase()));
            m_weaponsDic.Add(39, new WeaponItem(WeaponItemType.Handgun, new WpnSilencedPistol()));
            m_weaponsDic.Add(40, new WeaponItem(WeaponItemType.Handgun, new WpnSilencedUzi()));
            m_weaponsDic.Add(49, new WeaponItem(WeaponItemType.Melee, new WpnKnife()));
            m_weaponsDic.Add(47, new WeaponItem(WeaponItemType.Melee, new WpnPillow()));
            m_weaponsDic.Add(48, new WeaponItem(WeaponItemType.Melee, new WpnFlagpole()));
            m_weaponsDic.Add(46, new WeaponItem(WeaponItemType.Melee, new WpnChain()));
            m_weaponsDic.Add(50, new WeaponItem(WeaponItemType.Melee, new WpnTeapot()));
            m_weaponsDic.Add(51, new WeaponItem(WeaponItemType.Melee, new WpnTrashcanLid()));
            m_weaponsDic.Add(52, new WeaponItem(WeaponItemType.Melee, new WpnTrashBag()));
            m_weaponsDic.Add(53, new WeaponItem(WeaponItemType.Handgun, new WpnMachinePistol()));
            m_weaponsDic.Add(55, new WeaponItem(WeaponItemType.Rifle, new WpnMP50()));
            m_weaponsDic.Add(54, new WeaponItem(WeaponItemType.Rifle, new WpnDarkShotgun()));
            m_weaponsDic.Add(57, new WeaponItem(WeaponItemType.Melee, new WpnShockBaton()));
            m_weaponsDic.Add(56, new WeaponItem(WeaponItemType.Melee, new WpnLeadPipe()));
            m_weaponsDic.Add(59, new WeaponItem(WeaponItemType.Melee, new WpnChainsaw()));
            m_weaponsDic.Add(62, new WeaponItem(WeaponItemType.Powerup, new WpnStrengthBoost()));
            m_weaponsDic.Add(61, new WeaponItem(WeaponItemType.Handgun, new WpnPistol45()));
            m_weaponsDic.Add(63, new WeaponItem(WeaponItemType.Powerup, new WpnSpeedBoost()));
            m_weaponsDic.Add(64, new WeaponItem(WeaponItemType.Rifle, new WpnBow()));
            m_weaponsDic.Add(65, new WeaponItem(WeaponItemType.Melee, new WpnWhip()));
            m_weaponsDic.Add(66, new WeaponItem(WeaponItemType.InstantPickup, new WpnBouncingAmmo()));
            m_weaponsDic.Add(67, new WeaponItem(WeaponItemType.InstantPickup, new WpnFireAmmo()));
            m_weaponsDic.Add(68, new WeaponItem(WeaponItemType.InstantPickup, new WpnStreetsweeper()));

            WeaponDatabase.m_allWeapons = new List<WeaponItem>();
            WeaponDatabase.m_primaryWeapons = new List<WeaponItem>();
            WeaponDatabase.m_secondaryWeapons = new List<WeaponItem>();
            WeaponDatabase.m_meleeWeapons = new List<WeaponItem>();
            WeaponDatabase.m_supplyWeapons = new List<WeaponItem>();
            WeaponDatabase.m_thrownWeapons = new List<WeaponItem>();
            WeaponDatabase.m_powerupItems = new List<WeaponItem>();
            WeaponDatabase.m_healthItems = new List<WeaponItem>();
            WeaponDatabase.m_makeshiftWeapons = new List<WeaponItem>();

            foreach (WeaponItem weaponItem in m_weaponsDic.Values)
            {
                if (weaponItem != null && weaponItem.BaseProperties.WeaponCanBeEquipped)
                {
                    WeaponDatabase.m_allWeapons.Add(weaponItem);
                    if (weaponItem.BaseProperties.IsMakeshift)
                    {
                        WeaponDatabase.m_makeshiftWeapons.Add(weaponItem);
                    }
                    else
                    {
                        WeaponItemType weaponItemType = weaponItem.Type;
                        if (weaponItem.BaseProperties.WeaponID == 67 || weaponItem.BaseProperties.WeaponID == 66 || weaponItem.BaseProperties.WeaponID == 21)
                        {
                            weaponItemType = WeaponItemType.Powerup;
                        }
                        switch (weaponItemType)
                        {
                            case WeaponItemType.Handgun:
                                WeaponDatabase.m_secondaryWeapons.Add(weaponItem);
                                break;
                            case WeaponItemType.Rifle:
                                WeaponDatabase.m_primaryWeapons.Add(weaponItem);
                                break;
                            case WeaponItemType.Thrown:
                                WeaponDatabase.m_thrownWeapons.Add(weaponItem);
                                WeaponDatabase.m_supplyWeapons.Add(weaponItem);
                                break;
                            case WeaponItemType.Melee:
                                WeaponDatabase.m_meleeWeapons.Add(weaponItem);
                                break;
                            case WeaponItemType.Powerup:
                                WeaponDatabase.m_powerupItems.Add(weaponItem);
                                WeaponDatabase.m_supplyWeapons.Add(weaponItem);
                                break;
                            case WeaponItemType.InstantPickup:
                                WeaponDatabase.m_healthItems.Add(weaponItem);
                                WeaponDatabase.m_supplyWeapons.Add(weaponItem);
                                break;
                        }
                    }
                }
            }
            return false;
        }
    }
}
