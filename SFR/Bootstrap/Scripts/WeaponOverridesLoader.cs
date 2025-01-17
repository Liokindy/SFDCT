using System;
using System.Collections.Generic;
using System.Reflection;
using MonoMod.Utils;
using SFD.Weapons;
using SFD;
using SFDCT.Helper;
using static SFDCT.Bootstrap.Assets.ScriptsLoader;
using HarmonyLib;

namespace SFDCT.Bootstrap.Assets
{
    [HarmonyPatch]
    internal static class WeaponOverridesLoader
    {
        private static readonly Dictionary<string, ParseType> AllowedBaseWeaponVisuals = new Dictionary<string, ParseType>()
        {
            { "AnimDraw", ParseType.String },
            { "AnimRunUpper", ParseType.String },
            { "AnimWalkUpper", ParseType.String },
            { "AnimStaggerUpper", ParseType.String },
            { "AnimKickUpper", ParseType.String },
            { "AnimJumpUpper", ParseType.String },
            { "AnimJumpUpperFalling", ParseType.String },
            { "AnimJumpKickUpper", ParseType.String },
            { "AnimFullLand", ParseType.String },
            { "AnimCrouchUpper", ParseType.String },
            { "AnimIdleUpper", ParseType.String },
            { "AnimReloadUpper", ParseType.String },
            { "AnimFullJumpAttack", ParseType.String },
        };
        private static readonly Dictionary<string, ParseType> AllowedBaseWeaponProperties = new Dictionary<string, ParseType>()
        {
            { "Category", ParseType.WeaponCategory },
            { "IsMakeshift", ParseType.Bool },
            { "VisualText", ParseType.String },
            //{ "WeaponNameID", ParseType.String },
            //{ "WeaponID", ParseType.Short },
            { "WeaponCanBeEquipped", ParseType.Bool },
            //{ "ModelID", ParseType.String },
            { "SpawnsInSheath", ParseType.Bool },
            { "BreakDebris", ParseType.StringArray },
        };

        private static readonly Dictionary<string, ParseType> AllowedRWeaponVisuals = new Dictionary<string, ParseType>()
        {
            { "DrawnTextureName", ParseType.String },
            { "HolsterTextureName", ParseType.String },
            { "ModelTextureName", ParseType.String },
            { "SheathedTextureName", ParseType.String },
            { "ThrowingTextureName", ParseType.String },
            { "AnimFireArmLength", ParseType.Float },
            { "AnimManualAim", ParseType.String },
            { "AnimManualAimStart", ParseType.String },
            { "AnimUpperHipfire", ParseType.String },
            { "HipFireWeaponOffset", ParseType.Vector2 },
            { "RecoilDistance", ParseType.Float },
        };
        private static readonly Dictionary<string, ParseType> AllowedMWeaponVisuals = new Dictionary<string, ParseType>()
        {
            { "DrawnTextureName", ParseType.String },
            { "HolsterTextureName", ParseType.String },
            { "ModelTextureName", ParseType.String },
            { "SheathedTextureName", ParseType.String },
            { "ThrowingTextureName", ParseType.String },
            { "AnimBlockUpper", ParseType.String },
            { "AnimMeleeAttack1", ParseType.String },
            { "AnimMeleeAttack2", ParseType.String },
            { "AnimMeleeAttack3", ParseType.String },
        };
        private static readonly Dictionary<string, ParseType> AllowedTWeaponVisuals = new Dictionary<string, ParseType>()
        {
            { "DrawnTextureName", ParseType.String },
            { "ModelTextureName", ParseType.String },
            { "ThrowingTextureName", ParseType.String },
            { "AnimManualAim", ParseType.String },
            { "AnimManualAimStart", ParseType.String },
        };
        private static readonly Dictionary<string, ParseType> AllowedTWeaponProperties = new Dictionary<string, ParseType>()
        {
            { "AimStartSoundID", ParseType.String },
            { "DrawSoundID", ParseType.String },
            { "ThrowSoundID", ParseType.String },
            { "PrepareThrowSoundID", ParseType.String },
            { "PickupSoundID", ParseType.String },
            { "GrabAmmoSoundID", ParseType.String },
            { "MaxCarriedTotalThrowables", ParseType.Ushort },
            { "NumberOfThrowables", ParseType.Ushort },
            { "ThrowLocationOffset", ParseType.Vector2 },
            { "ThrowPower", ParseType.Float },
            { "ThrowExtraUp", ParseType.Float },
            { "ThrowObjectID", ParseType.String },
            { "ThrowDeadlineTimer", ParseType.Float },
            { "CanEnterManualAim", ParseType.Bool },
            { "CanBeSheathedDuringAim", ParseType.Bool },
            { "Stackable", ParseType.Bool },
        };
        private static readonly Dictionary<string, ParseType> AllowedMWeaponProperties = new Dictionary<string, ParseType>()
        {
            { "Range", ParseType.Float },
            { "HitboxHeightOffset", ParseType.Float },
            { "DamagePlayers", ParseType.Float },
            { "DamageObjects", ParseType.Float },
            { "PlayerHitSoundID", ParseType.String },
            { "ObjectHitSoundID", ParseType.String },
            { "PlayerHitEffectID", ParseType.String },
            { "ObjectHitEffectID", ParseType.String },
            { "SwingSoundID", ParseType.String },
            { "DrawSoundID", ParseType.String },
            { "WeaponMaterial", ParseType.Material },
            //{ "MeleeWeaponType", ParseType.MeleeWeaponTypeEnum },
            { "DurabilityLossOnHitPlayers", ParseType.Float },
            { "DurabilityLossOnHitBlockingPlayers", ParseType.Float },
            { "ThrownDurabilityLossOnHitPlayers", ParseType.Float },
            { "ThrownDurabilityLossOnHitBlockingPlayers", ParseType.Float },
            { "DurabilityLossOnHitObjects", ParseType.Float },
            { "DeflectionDuringBlock", ParseType.DeflectionProperties },
            { "DeflectionOnAttack", ParseType.DeflectionProperties },
            { "ReplacementWeaponID", ParseType.Short },
            { "Handling", ParseType.MeleeHandlingType },
            { "AI_DamageOutput", ParseType.DamageOutputType },
            { "AutoDestroyOnDurabilityEmpty", ParseType.Bool },
        };
        private static readonly Dictionary<string, ParseType> AllowedRWeaponProperties = new Dictionary<string, ParseType>()
        {
            { "MaxMagsInWeapon", ParseType.Ushort  },
            { "MaxRoundsInMag", ParseType.Ushort },
            { "MaxRoundsInWeapon", ParseType.Ushort },
            { "MaxCarriedSpareMags", ParseType.Ushort },
            { "MaxRoundsTotal", ParseType.Ushort },
            { "StartMags", ParseType.Ushort },
            { "AccuracyDeflection", ParseType.Float },
            { "BurstRoundsToFire", ParseType.Ushort },
            { "BurstCooldown", ParseType.Ushort },
            { "CooldownBeforePostAction", ParseType.Ushort },
            { "CooldownAfterPostAction", ParseType.Ushort },
            { "ExtraAutomaticCooldown", ParseType.Ushort },
            { "ProjectilesEachBlast", ParseType.Ushort },
            { "ProjectileID", ParseType.Short },
            { "MuzzlePosition", ParseType.Vector2 },
            { "LazerPosition", ParseType.Vector2 },
            { "MuzzleEffectTextureID", ParseType.String },
            { "BlastSoundID", ParseType.String },
            { "DrawSoundID", ParseType.String },
            { "AimStartSoundID", ParseType.String },
            { "GrabAmmoSoundID", ParseType.String },
            { "OutOfAmmoSoundID", ParseType.String },
            //{ "Projectile", ParseType.Projectile },
            { "ShellID", ParseType.String },
            { "ClearRoundsOnReloadStart", ParseType.Bool },
            { "CursorAimOffset", ParseType.Vector2 },
            { "ReloadPostCooldown", ParseType.Float },
            { "AI_EffectiveRange", ParseType.Float },
            { "AI_MaxRange", ParseType.Float },
            { "AI_GravityArcingEffect", ParseType.Float },
            { "AI_ImpactAoERadius", ParseType.Float },
            { "AI_HasOneShotPotential", ParseType.Bool },
            { "AI_DamageOutput", ParseType.DamageOutputType },
            { "CanRefilAtAmmoStashes", ParseType.Bool },
            { "CanUseFireBulletsPowerup", ParseType.Bool },
            { "CanUseBouncingBulletsPowerup", ParseType.Bool },
            { "SpecialAmmoBulletsRefill", ParseType.Int },
        };

        private static void HandleOverrides(object baseWeapon, WeaponBaseProperties baseProperties, WeaponBaseVisuals baseVisuals, out object properties, out object visuals)
        {
            visuals = baseVisuals;
            properties = baseProperties;

            ushort index = 0;
            string scriptName = "";
            try
            {
                CTScript[] scripts = GetByType(CTScript.ScriptType.WeaponOverride);
                if (scripts == null)
                {
                    return;
                }

                foreach (CTScript script in scripts)
                {
                    index = 1;
                    scriptName = script.FileName;

                    if (script.Entries.ContainsKey("Weapon") && script.Entries["Weapon"].Equals(baseProperties.WeaponNameID))
                    {
                        index = 2;
                        Dictionary<PropertyInfo, object> propertyList = [];
                        Dictionary<PropertyInfo, object> visualList = [];
                        
                        index = 3;
                        foreach (string entrykey in script.Entries.Keys)
                        {
                            index = 4;
                            Type visualsType = null;
                            Dictionary<string, ParseType> allowedVisuals = new Dictionary<string, ParseType>(AllowedBaseWeaponVisuals);

                            if (baseVisuals is RWeaponVisuals)
                            {
                                visualsType = typeof(RWeaponVisuals);
                                allowedVisuals.AddRange(AllowedRWeaponVisuals);
                            }
                            else if (baseVisuals is MWeaponVisuals)
                            {
                                visualsType = typeof(MWeaponVisuals);
                                allowedVisuals.AddRange(AllowedMWeaponVisuals);
                            }
                            else if (baseVisuals is TWeaponVisuals)
                            {
                                visualsType = typeof(TWeaponVisuals);
                                allowedVisuals.AddRange(AllowedTWeaponVisuals);
                            }

                            if (visualsType != null && allowedVisuals != null)
                            {
                                index = 5;
                                if (allowedVisuals.ContainsKey(entrykey))
                                {
                                    string entryValue = script.Entries[entrykey];
                                    object entryObject = TryParse(entryValue, allowedVisuals[entrykey]);

                                    index = 6;
                                    visualList.Add(visualsType.GetProperty(entrykey), entryObject);
                                }

                                if (baseVisuals is RWeaponVisuals rWpnVisuals)
                                {
                                    index = 7;
                                    if (string.IsNullOrEmpty(rWpnVisuals.DrawnTextureName)) { rWpnVisuals.Drawn = null; rWpnVisuals.DrawnTextureName = string.Empty; } else {rWpnVisuals.SetDrawnTexture(rWpnVisuals.DrawnTextureName);}
                                    if (string.IsNullOrEmpty(rWpnVisuals.HolsterTextureName)) { rWpnVisuals.Holster = null; rWpnVisuals.HolsterTextureName = string.Empty; } else {rWpnVisuals.SetHolsterTexture(rWpnVisuals.HolsterTextureName);}
                                    if (string.IsNullOrEmpty(rWpnVisuals.ModelTextureName)) { rWpnVisuals.Model = null; rWpnVisuals.ModelTextureName = string.Empty; } else {rWpnVisuals.SetModelTexture(rWpnVisuals.ModelTextureName);}
                                    if (string.IsNullOrEmpty(rWpnVisuals.ThrowingTextureName)) { rWpnVisuals.Throwing = null; rWpnVisuals.ThrowingTextureName = string.Empty; } else {rWpnVisuals.SetThrowingTexture(rWpnVisuals.ThrowingTextureName);}
                                    if (string.IsNullOrEmpty(rWpnVisuals.SheathedTextureName)) { rWpnVisuals.Sheathed = null; rWpnVisuals.SheathedTextureName = string.Empty; } else {rWpnVisuals.SetSheathedTexture(rWpnVisuals.SheathedTextureName);}
                                }
                                else if (baseVisuals is MWeaponVisuals mWpnVisuals)
                                {
                                    index = 8;
                                    if (string.IsNullOrEmpty(mWpnVisuals.DrawnTextureName)) { mWpnVisuals.Drawn = null; mWpnVisuals.DrawnTextureName = string.Empty; } else {mWpnVisuals.SetDrawnTexture(mWpnVisuals.DrawnTextureName);}
                                    if (string.IsNullOrEmpty(mWpnVisuals.HolsterTextureName)) { mWpnVisuals.Holster = null; mWpnVisuals.HolsterTextureName = string.Empty; } else {mWpnVisuals.SetHolsterTexture(mWpnVisuals.HolsterTextureName);}
                                    if (string.IsNullOrEmpty(mWpnVisuals.ModelTextureName)) { mWpnVisuals.Model = null; mWpnVisuals.ModelTextureName = string.Empty; } else {mWpnVisuals.SetModelTexture(mWpnVisuals.ModelTextureName);}
                                    if (string.IsNullOrEmpty(mWpnVisuals.ThrowingTextureName)) { mWpnVisuals.Throwing = null; mWpnVisuals.ThrowingTextureName = string.Empty; } else {mWpnVisuals.SetThrowingTexture(mWpnVisuals.ThrowingTextureName);}
                                    if (string.IsNullOrEmpty(mWpnVisuals.SheathedTextureName)) { mWpnVisuals.Sheathed = null; mWpnVisuals.SheathedTextureName = string.Empty; } else {mWpnVisuals.SetSheathedTexture(mWpnVisuals.SheathedTextureName);}
                                }
                                else if (baseVisuals is TWeaponVisuals tWpnVisuals)
                                {
                                    index = 9;
                                    if (string.IsNullOrEmpty(tWpnVisuals.DrawnTextureName)) { tWpnVisuals.Drawn = null; tWpnVisuals.DrawnTextureName = string.Empty; } else {tWpnVisuals.SetDrawnTexture(tWpnVisuals.DrawnTextureName);}
                                    if (string.IsNullOrEmpty(tWpnVisuals.ModelTextureName)) { tWpnVisuals.Model = null; tWpnVisuals.ModelTextureName = string.Empty; } else {tWpnVisuals.SetModelTexture(tWpnVisuals.ModelTextureName);}
                                    if (string.IsNullOrEmpty(tWpnVisuals.ThrowingTextureName)) { tWpnVisuals.Throwing = null; tWpnVisuals.ThrowingTextureName = string.Empty; } else {tWpnVisuals.SetThrowingTexture(tWpnVisuals.ThrowingTextureName);}
                                }
                            }

                            index = 10;
                            Type propertiesType = null;
                            Dictionary<string, ParseType> allowedProperties = new Dictionary<string, ParseType>(AllowedBaseWeaponProperties);

                            if (baseProperties is RWeaponProperties)
                            {
                                index = 11;
                                propertiesType = typeof(RWeaponProperties);
                                allowedProperties.AddRange(AllowedRWeaponProperties);
                            }
                            else if (baseProperties is MWeaponProperties)
                            {
                                index = 12;
                                propertiesType = typeof(MWeaponProperties);
                                allowedProperties.AddRange(AllowedMWeaponProperties);
                            }
                            else if (baseProperties is TWeaponProperties)
                            {
                                index = 13;
                                propertiesType = typeof(TWeaponProperties);
                                allowedProperties.AddRange(AllowedTWeaponProperties);
                            }

                            index = 14;
                            if (propertiesType != null && allowedProperties != null)
                            {
                                index = 15;
                                if (allowedProperties.ContainsKey(entrykey))
                                {
                                    string entryValue = script.Entries[entrykey];
                                    object entryObject = TryParse(entryValue, allowedProperties[entrykey]);

                                    index = 16;
                                    propertyList.Add(propertiesType.GetProperty(entrykey), entryObject);
                                }
                            }
                        }

                        index = 17;
                        foreach (KeyValuePair<PropertyInfo, object> property in propertyList)
                        {
                            //Logger.LogDebug($"WEAPONS OVERRIDES LOADER: {property.Key.Name}: {property.Value}");
                            property.Key.SetValue(baseProperties, property.Value);
                        }
                        index = 18;
                        foreach (KeyValuePair<PropertyInfo, object> visual in visualList)
                        {
                            //Logger.LogDebug($"WEAPONS OVERRIDES LOADER: {visual.Key.Name}: {visual.Value}");
                            visual.Key.SetValue(baseVisuals, visual.Value);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Logger.LogError($"WEAPON OVERRIDES LOADER: Failed to handle overrides at {index} in script '{scriptName}'!");
                Logger.LogError(ex.ToString());

                visuals = baseVisuals;
                properties = baseProperties;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(RWeapon), nameof(RWeapon.SetPropertiesAndVisuals))]
        private static void RWeaponSetPropertiesAndVisuals(RWeapon __instance, RWeaponProperties properties, RWeaponVisuals visuals)
        {
            if (!GameSFD.Handle.ImHosting || (GameSFD.Handle.Client != null && (GameSFD.Handle.Client.CurrentIPEndpoint != null && (GameSFD.Handle.Client.CurrentIPEndpoint.Address.ToReadableString() != "127.0.0.1" && GameSFD.Handle.Client.CurrentIPEndpoint.Address.ToReadableString() != "localhost"))))
            {
                return;
            }

            HandleOverrides(__instance, properties, visuals, out object newProperties, out object newVisuals);

            __instance.Properties = (RWeaponProperties)newProperties;
            __instance.Visuals = (RWeaponVisuals)newVisuals;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MWeapon), nameof(MWeapon.SetPropertiesAndVisuals))]
        private static void MWeaponSetPropertiesAndVisuals(MWeapon __instance, MWeaponProperties properties, MWeaponVisuals visuals)
        {
            if (!GameSFD.Handle.ImHosting || (GameSFD.Handle.Client != null && (GameSFD.Handle.Client.CurrentIPEndpoint != null && (GameSFD.Handle.Client.CurrentIPEndpoint.Address.ToReadableString() != "127.0.0.1" && GameSFD.Handle.Client.CurrentIPEndpoint.Address.ToReadableString() != "localhost"))))
            {
                return;
            }

            HandleOverrides(__instance, properties, visuals, out object newProperties, out object newVisuals);

            __instance.Properties = (MWeaponProperties)newProperties;
            __instance.Visuals = (MWeaponVisuals)newVisuals;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(TWeapon), nameof(TWeapon.SetPropertiesAndVisuals))]
        private static void TWeaponSetPropertiesAndVisuals(TWeapon __instance, TWeaponProperties throwableProperties, TWeaponVisuals throwableVisuals)
        {
            if (!GameSFD.Handle.ImHosting || (GameSFD.Handle.Client != null && (GameSFD.Handle.Client.CurrentIPEndpoint != null && (GameSFD.Handle.Client.CurrentIPEndpoint.Address.ToReadableString() != "127.0.0.1" && GameSFD.Handle.Client.CurrentIPEndpoint.Address.ToReadableString() != "localhost"))))
            {
                return;
            }

            HandleOverrides(__instance, throwableProperties, throwableVisuals, out object newProperties, out object newVisuals);

            __instance.Properties = (TWeaponProperties)newProperties;
            __instance.Visuals = (TWeaponVisuals)newVisuals;
        }
    }
}
