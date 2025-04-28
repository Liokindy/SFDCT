using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SFD;
using SFD.Code;
using SFD.Materials;
using SFD.Parser;
using SFD.Projectiles;
using SFD.Tiles;
using SFD.Weapons;
using SFDCT.Helper;
using SFDCT.Weapons;
using CGlobals = SFDCT.Misc.Globals;

namespace SFDCT.Bootstrap.Scripts
{
    internal static class CTWeaponsLoader
    {
        internal static RWeaponProperties GetBaseHandgunProperties()
        {
            return new RWeaponProperties(0, "", "", false, WeaponCategory.Secondary)
            {
               MaxMagsInWeapon = 1,
               MaxRoundsInMag = 15,
               MaxCarriedSpareMags = 4,
               StartMags = 2,
               CooldownBeforePostAction = 200,
               CooldownAfterPostAction = 0,
               ExtraAutomaticCooldown = 200,
               ShellID = "ShellSmall",
               AccuracyDeflection = SFDMath.DegToRad(5f),
               ProjectileID = WeaponItem.ID.PISTOL,
               MuzzlePosition = new Vector2(2f, -2f),
               MuzzleEffectTextureID = "MuzzleFlashM",
               DrawSoundID = "PistolDraw",
               BlastSoundID = "Pistol",
               GrabAmmoSoundID = "PistolReload",
               OutOfAmmoSoundID = "OutOfAmmoLight",
               CursorAimOffset = new Vector2(0f, 3f),
               LazerPosition = new Vector2(2f, -0.5f),
               AimStartSoundID = "PistolAim",
               AI_DamageOutput = DamageOutputType.Low,
               SpecialAmmoBulletsRefill = 15,
               BreakDebris = [],
               BurstCooldown = 0,
               BurstRoundsToFire = 0,
               CanRefilAtAmmoStashes = true,
               CanUseBouncingBulletsPowerup = true,
               CanUseFireBulletsPowerup = true,
               ClearRoundsOnReloadStart = true,
               ProjectilesEachBlast = 1,
               ReloadPostCooldown = 0,
            };
        }
        internal static MWeaponProperties GetBaseMeleeProperties()
        {
            return new MWeaponProperties(0, "", 0f, 0f, "", "", "", "", "", "", "", false, WeaponCategory.Supply, false)
            {
                Category = WeaponCategory.Melee,
                Range = 10f,
                HitboxHeightOffset = 0f,
                DamagePlayers = 11.5f,
                DamageObjects = 11.5f,
                PlayerHitSoundID = "MeleeHitBlunt",
                ObjectHitSoundID = "MeleeBlock",
                PlayerHitEffectID = "HIT_B",
                ObjectHitEffectID = "HIT_B",
                SwingSoundID = "MeleeSwing",
                DrawSoundID = "MeleeDraw",
                WeaponMaterial = MaterialDatabase.Get("wood"),
                MeleeWeaponType = MeleeWeaponTypeEnum.TwoHanded,
                DurabilityLossOnHitPlayers = 10f,
                DurabilityLossOnHitBlockingPlayers = 50f,
                ThrownDurabilityLossOnHitPlayers = 10f,
                ThrownDurabilityLossOnHitBlockingPlayers = 5f,
                DurabilityLossOnHitObjects = 10f,
                DeflectionDuringBlock = new DeflectionProperties(SFDMath.DegToRad(180f)),
                DeflectionOnAttack = new DeflectionProperties(SFDMath.DegToRad(30f)),
                ReplacementWeaponID = -1,
                Handling = MeleeHandlingType.Default,
                AI_DamageOutput = DamageOutputType.Standard,
                AutoDestroyOnDurabilityEmpty = true,
                BreakDebris = [],
            };
        }
        internal static MWeaponVisuals GetBaseMeleeVisuals()
        {
            MWeaponVisuals result = new MWeaponVisuals()
            {
                AnimBlockUpper = "UpperBlockMelee2H",
                AnimMeleeAttack1 = "UpperMelee2H1",
                AnimMeleeAttack2 = "UpperMelee2H2",
                AnimMeleeAttack3 = "UpperMelee2H3",
                AnimFullJumpAttack = "FullJumpAttackMelee",
                AnimDraw = "UpperDrawMelee",
                AnimCrouchUpper = "UpperCrouchMelee2H",
                AnimIdleUpper = "UpperIdleMelee2H",
                AnimJumpKickUpper = "UpperJumpKickMelee",
                AnimJumpUpper = "UpperJumpMelee2H",
                AnimJumpUpperFalling = "UpperJumpFallingMelee2H",
                AnimKickUpper = "UpperKickMelee2H",
                AnimStaggerUpper = "UpperStagger",
                AnimRunUpper = "UpperRunMelee2H",
                AnimWalkUpper = "UpperWalkMelee2H",
                AnimFullLand = "FullLandMelee",
                AnimToggleThrowingMode = "UpperToggleThrowing",
            };
            
            result.SetModelTexture("BatM");
            result.SetDrawnTexture("BatD");
            result.SetSheathedTexture("BatS");
            result.SetHolsterTexture("BatH");
            return result;
        }
        internal static RWeaponVisuals GetBaseHandgunVisuals()
        {
            RWeaponVisuals result = new RWeaponVisuals()
            {
                AnimIdleUpper = "UpperIdleHandgun",
                AnimCrouchUpper = "UpperCrouchHandgun",
                AnimJumpKickUpper = "UpperJumpKickHandgun",
                AnimJumpUpper = "UpperJumpHandgun",
                AnimJumpUpperFalling = "UpperJumpFallingHandgun",
                AnimKickUpper = "UpperKickHandgun",
                AnimStaggerUpper = "UpperStaggerHandgun",
                AnimRunUpper = "UpperRunHandgun",
                AnimWalkUpper = "UpperWalkHandgun",
                AnimUpperHipfire = "UpperHipfireHandgun",
                AnimFireArmLength = 7f,
                AnimDraw = "UpperDrawHandgun",
                AnimManualAim = "ManualAimHandgun",
                AnimManualAimStart = "ManualAimHandgunStart",
                AnimReloadUpper = "UpperReload",
                AnimFullLand = "FullLandHandgun",
                AnimToggleThrowingMode = "UpperToggleThrowing",
            };

            result.SetModelTexture("PistolM");
            result.SetDrawnTexture("PistolD");
            result.SetThrowingTexture("PistolThrowing");
            return result;
        }
        internal static RWeaponProperties GetBaseRifleProperties()
        {
            return new RWeaponProperties(0, "", "", false, WeaponCategory.Primary)
            {
                MaxMagsInWeapon = 1,
                MaxRoundsInMag = 30,
                MaxCarriedSpareMags = 4,
                StartMags = 2,
                CooldownBeforePostAction = 100,
                CooldownAfterPostAction = 0,
                ExtraAutomaticCooldown = 0,
                ShellID = "ShellBig",
                AccuracyDeflection = SFDMath.DegToRad(3f),
                ProjectileID = WeaponItem.ID.ASSAULT,
                ProjectilesEachBlast = 1,
                MuzzlePosition = new Vector2(13f, -2.5f),
                CursorAimOffset = new Vector2(0f, 2.5f),
                LazerPosition = new Vector2(12f, -1.5f),
                MuzzleEffectTextureID = "MuzzleFlashS",
                BlastSoundID = "AssaultRifle",
                DrawSoundID = "AssaultRifleDraw",
                GrabAmmoSoundID = "AssaultRifleReload",
                OutOfAmmoSoundID = "OutOfAmmoHeavy",
                AimStartSoundID = "PistolAim",
                AI_DamageOutput = DamageOutputType.Low,
                SpecialAmmoBulletsRefill = 30,
            };
        }
        internal static RWeaponVisuals GetBaseRifleVisuals()
        {
            RWeaponVisuals result = new RWeaponVisuals()
            {
                AnimIdleUpper = "UpperIdleRifle",
                AnimCrouchUpper = "UpperCrouchRifle",
                AnimJumpKickUpper = "UpperJumpKickRifle",
                AnimJumpUpper = "UpperJumpRifle",
                AnimJumpUpperFalling = "UpperJumpFallingRifle",
                AnimKickUpper = "UpperKickRifle",
                AnimStaggerUpper = "UpperStaggerHandgun",
                AnimRunUpper = "UpperRunRifle",
                AnimWalkUpper = "UpperWalkRifle",
                AnimUpperHipfire = "UpperHipfireRifle",
                AnimFireArmLength = 2f,
                AnimDraw = "UpperDrawRifle",
                AnimManualAim = "ManualAimRifle",
                AnimManualAimStart = "ManualAimRifleStart",
                AnimReloadUpper = "UpperReload",
                AnimFullLand = "FullLandHandgun",
                AnimToggleThrowingMode = "UpperToggleThrowing",
            };

            result.SetModelTexture("CarbineM");
            result.SetDrawnTexture("CarbineD");
            result.SetHolsterTexture("CarbineH");
            result.SetSheathedTexture("CarbineS");
            result.SetThrowingTexture("CarbineThrowing");
            return result;
        }
        internal static WeaponBaseVisuals GetBaseVisuals()
        {
            return new WeaponBaseVisuals()
            {
                AnimCrouchUpper = "UpperCrouch",
                AnimDraw = "UpperDrawThrowable",
                AnimFullJumpAttack = "FullJumpAttack",
                AnimFullLand = "FullLand",
                AnimIdleUpper = "UpperIdle",
                AnimJumpKickUpper = "UpperJumpKick",
                AnimJumpUpper = "UpperJump",
                AnimJumpUpperFalling = "UpperJumpFalling",
                AnimKickUpper = "UpperKick",
                AnimReloadUpper = "UpperReload",
                AnimRunUpper = "UpperRun",
                AnimStaggerUpper = "UpperStagger",
                AnimToggleThrowingMode = "UpperToggleThrowingMode",
                AnimWalkUpper = "UpperWalk",
            };
        }
        internal static WeaponBaseProperties GetBaseProperties()
        {
            return new WeaponBaseProperties("", 0, "", false, WeaponCategory.Supply, false);
        }

        internal static void ReadBasePropertiesAndVisuals(IniHandler handler, ref WeaponBaseProperties properties, ref WeaponBaseVisuals visuals)
        {
            // properties.Category;
            // properties.WeaponID;
            // properties.WeaponNameID;
            handler.TryReadValueBool("ISMAKESHIFT", false, out bool isMakeshift); properties.IsMakeshift = isMakeshift;
            handler.TryReadValue("VISUALTEXT", out string visualText); properties.VisualText = visualText;
            handler.TryReadValue("MODELID", out string modelID); properties.ModelID = modelID;
            handler.TryReadValueBool("SPAWNSINSHEATH", false, out bool spawnsInSheath); properties.SpawnsInSheath = spawnsInSheath;
            handler.TryReadValue("BREAKDEBRIS", out string breakDebris); properties.BreakDebris = breakDebris.Split(' ');

            handler.TryReadValue("ANIMTOGGLETHROWINGMODE", out string animToggleThrowingMode); visuals.AnimToggleThrowingMode = animToggleThrowingMode;
            handler.TryReadValue("ANIMDRAW", out string animDraw); visuals.AnimDraw = animDraw;
            handler.TryReadValue("ANIMRUNUPPER", out string animRunUpper); visuals.AnimRunUpper = animRunUpper;
            handler.TryReadValue("ANIMWALKUPPER", out string animWalkUpper); visuals.AnimWalkUpper = animWalkUpper;
            handler.TryReadValue("ANIMSTAGGERUPPER", out string animStaggerUpper); visuals.AnimStaggerUpper = animStaggerUpper;
            handler.TryReadValue("ANIMKICKUPPER", out string animKickUpper); visuals.AnimKickUpper = animKickUpper;
            handler.TryReadValue("ANIMJUMPUPPER", out string animJumpUpper); visuals.AnimJumpUpper = animJumpUpper;
            handler.TryReadValue("ANIMJUMPUPPERFALLING", out string animJumpUpperFalling); visuals.AnimJumpUpperFalling = animJumpUpperFalling;
            handler.TryReadValue("ANIMJUMPKICKUPPER", out string animJumpKickUpper); visuals.AnimJumpKickUpper = animJumpKickUpper;
            handler.TryReadValue("ANIMFULLLAND", out string animFullLand); visuals.AnimFullLand = animFullLand;
            handler.TryReadValue("ANIMCROUCHUPPER", out string animCrouchUpper); visuals.AnimCrouchUpper = animCrouchUpper;
            handler.TryReadValue("ANIMIDLEUPPER", out string animIdleUpper); visuals.AnimIdleUpper = animIdleUpper;
            handler.TryReadValue("ANIMRELOADUPPER", out string animReloadUpper); visuals.AnimReloadUpper = animReloadUpper;
            handler.TryReadValue("ANIMFULLJUMPATTACK", out string animFullJumpAttack); visuals.AnimFullJumpAttack = animFullJumpAttack;
        }
        internal static void ReadRPropertiesAndRVisuals(IniHandler handler, ref RWeaponProperties properties, ref RWeaponVisuals visuals)
        {
            handler.TryReadValueInt("MAXMAGSINWEAPON", properties.MaxMagsInWeapon, out int maxMagsInWeaponValue); properties.MaxMagsInWeapon = (ushort)maxMagsInWeaponValue;
            handler.TryReadValueInt("MAXROUNDSINMAG", properties.MaxRoundsInMag, out int maxRoundsInMagValue); properties.MaxRoundsInMag = (ushort)maxRoundsInMagValue;
            handler.TryReadValueInt("MAXCARRIEDSPAREMAGS", properties.MaxCarriedSpareMags, out int maxCarriedSpareMagsValue); properties.MaxCarriedSpareMags = (ushort)maxCarriedSpareMagsValue;
            handler.TryReadValueInt("STARTMAGS", properties.StartMags, out int startMagsValue); properties.StartMags = (ushort)startMagsValue;
            handler.TryReadValueFloat("ACCURACYDEFLECTION", properties.AccuracyDeflection, out float accuracyDeflectionValue); properties.AccuracyDeflection = accuracyDeflectionValue;
            handler.TryReadValueInt("BURSTROUNDSTOFIRE", properties.BurstRoundsToFire, out int burstRoundsToFireValue); properties.BurstRoundsToFire = (ushort)burstRoundsToFireValue;
            handler.TryReadValueInt("BURSTCOOLDOWN", properties.BurstCooldown, out int burstCooldownValue); properties.BurstCooldown = (ushort)burstCooldownValue;
            handler.TryReadValueInt("COOLDOWNBEFOREPOSTACTION", properties.CooldownBeforePostAction, out int cooldownBeforePostActionValue); properties.CooldownBeforePostAction = (ushort)cooldownBeforePostActionValue;
            handler.TryReadValueInt("COOLDOWNAFTERPOSTACTION", properties.CooldownAfterPostAction, out int cooldownAfterPostActionValue); properties.CooldownAfterPostAction = (ushort)cooldownAfterPostActionValue;
            handler.TryReadValueInt("EXTRAAUTOMATICCOOLDOWN", properties.ExtraAutomaticCooldown, out int extraAutomaticCooldownValue); properties.ExtraAutomaticCooldown = (ushort)extraAutomaticCooldownValue;
            handler.TryReadValueInt("PROJECTILESEACHBLAST", properties.ProjectilesEachBlast, out int projectilesEachBlastValue); properties.ProjectilesEachBlast = (ushort)projectilesEachBlastValue;
            handler.TryReadValueInt("PROJECTILEID", properties.ProjectileID, out int projectileIDValue); properties.ProjectileID = (short)projectileIDValue;
            handler.TryReadValue("MUZZLEPOSITION", out string muzzlePositionValue); properties.MuzzlePosition = string.IsNullOrEmpty(muzzlePositionValue) ? properties.MuzzlePosition : new Vector2(SFDXParser.ParseFloat(muzzlePositionValue.Split(' ')[0]), SFDXParser.ParseFloat(muzzlePositionValue.Split(' ')[1]));
            handler.TryReadValue("LAZERPOSITION", out string lazerPositionValue); properties.LazerPosition = string.IsNullOrEmpty(lazerPositionValue) ? properties.LazerPosition : new Vector2(SFDXParser.ParseFloat(lazerPositionValue.Split(' ')[0]), SFDXParser.ParseFloat(lazerPositionValue.Split(' ')[1]));
            handler.TryReadValue("MUZZLEEFFECTTEXTUREID", out string muzzleEffectTextureIDValue); properties.MuzzleEffectTextureID = string.IsNullOrEmpty(muzzleEffectTextureIDValue) ? properties.MuzzleEffectTextureID : (string)muzzleEffectTextureIDValue;
            handler.TryReadValue("BLASTSOUNDID", out string blastSoundIDValue); properties.BlastSoundID = string.IsNullOrEmpty(blastSoundIDValue) ? properties.BlastSoundID : (string)blastSoundIDValue;
            handler.TryReadValue("DRAWSOUNDID", out string drawSoundIDValue); properties.DrawSoundID = string.IsNullOrEmpty(drawSoundIDValue) ? properties.DrawSoundID : (string)drawSoundIDValue;
            handler.TryReadValue("AIMSTARTSOUNDID", out string aimStartSoundIDValue); properties.AimStartSoundID = string.IsNullOrEmpty(aimStartSoundIDValue) ? properties.AimStartSoundID : (string)aimStartSoundIDValue;
            handler.TryReadValue("GRABAMMOSOUNDID", out string grabAmmoSoundIDValue); properties.GrabAmmoSoundID = string.IsNullOrEmpty(grabAmmoSoundIDValue) ? properties.GrabAmmoSoundID : (string)grabAmmoSoundIDValue;
            handler.TryReadValue("OUTOFAMMOSOUNDID", out string outOfAmmoSoundIDValue); properties.OutOfAmmoSoundID = string.IsNullOrEmpty(outOfAmmoSoundIDValue) ? properties.OutOfAmmoSoundID : (string)outOfAmmoSoundIDValue;
            handler.TryReadValue("SHELLID", out string shellIDValue); properties.ShellID = string.IsNullOrEmpty(shellIDValue) ? properties.ShellID : (string)shellIDValue;
            handler.TryReadValueBool("CLEARROUNDSONRELOADSTART", properties.ClearRoundsOnReloadStart, out bool clearRoundsOnReloadStartValue); properties.ClearRoundsOnReloadStart = clearRoundsOnReloadStartValue;
            handler.TryReadValue("CURSORAIMOFFSET", out string cursorAimOffsetValue); properties.CursorAimOffset = string.IsNullOrEmpty(cursorAimOffsetValue) ? properties.CursorAimOffset : new Vector2(SFDXParser.ParseFloat(cursorAimOffsetValue.Split(' ')[0]), SFDXParser.ParseFloat(cursorAimOffsetValue.Split(' ')[1]));
            handler.TryReadValueFloat("RELOADPOSTCOOLDOWN", properties.ReloadPostCooldown, out float reloadPostCooldownValue); properties.ReloadPostCooldown = reloadPostCooldownValue;
            handler.TryReadValueFloat("AI_EFFECTIVERANGE", properties.AI_EffectiveRange, out float aI_EffectiveRangeValue); properties.AI_EffectiveRange = aI_EffectiveRangeValue;
            handler.TryReadValueFloat("AI_MAXRANGE", properties.AI_MaxRange, out float aI_MaxRangeValue); properties.AI_MaxRange = aI_MaxRangeValue;
            handler.TryReadValueFloat("AI_GRAVITYARCINGEFFECT", properties.AI_GravityArcingEffect, out float aI_GravityArcingEffectValue); properties.AI_GravityArcingEffect = aI_GravityArcingEffectValue;
            handler.TryReadValueFloat("AI_IMPACTAOERADIUS", properties.AI_ImpactAoERadius, out float aI_ImpactAoERadiusValue); properties.AI_ImpactAoERadius = aI_ImpactAoERadiusValue;
            handler.TryReadValueBool("AI_HASONESHOTPOTENTIAL", properties.AI_HasOneShotPotential, out bool aI_HasOneShotPotentialValue); properties.AI_HasOneShotPotential = aI_HasOneShotPotentialValue;
            handler.TryReadValue("AI_DAMAGEOUTPUT", out string aI_DamageOutputValue); properties.AI_DamageOutput = Enum.TryParse(aI_DamageOutputValue, out DamageOutputType aI_DamageOutputValue2) ? aI_DamageOutputValue2 : DamageOutputType.Standard;
            handler.TryReadValueBool("CANREFILATAMMOSTASHES", properties.CanRefilAtAmmoStashes, out bool canRefilAtAmmoStashesValue); properties.CanRefilAtAmmoStashes = canRefilAtAmmoStashesValue;
            handler.TryReadValueBool("CANUSEFIREBULLETSPOWERUP", properties.CanUseFireBulletsPowerup, out bool canUseFireBulletsPowerupValue); properties.CanUseFireBulletsPowerup = canUseFireBulletsPowerupValue;
            handler.TryReadValueBool("CANUSEBOUNCINGBULLETSPOWERUP", properties.CanUseBouncingBulletsPowerup, out bool canUseBouncingBulletsPowerupValue); properties.CanUseBouncingBulletsPowerup = canUseBouncingBulletsPowerupValue;
            handler.TryReadValueInt("SPECIALAMMOBULLETSREFILL", properties.SpecialAmmoBulletsRefill, out int specialAmmoBulletsRefillValue); properties.SpecialAmmoBulletsRefill = specialAmmoBulletsRefillValue;

            handler.TryReadValue("ANIMUPPERHIPFIRE", out string animUpperHipfire); visuals.AnimUpperHipfire = string.IsNullOrEmpty(animUpperHipfire) ? visuals.AnimUpperHipfire : animUpperHipfire;
            handler.TryReadValue("ANIMMANUALAIM", out string animManualAim); visuals.AnimManualAim = string.IsNullOrEmpty(animManualAim) ? visuals.AnimManualAim : animManualAim;
            handler.TryReadValue("ANIMMANUALAIMSTART", out string animManualAimStart); visuals.AnimManualAimStart = string.IsNullOrEmpty(animManualAimStart) ? visuals.AnimManualAimStart : animManualAimStart;
            handler.TryReadValueFloat("ANIMFIREARMLENGTH", visuals.AnimFireArmLength, out float animFireArmLength); visuals.AnimFireArmLength = animFireArmLength;
        }
        internal static void ReadMPropertiesAndMVisuals(IniHandler handler, ref MWeaponProperties properties, ref MWeaponVisuals visuals)
        {
            handler.TryReadValueFloat("RANGE", properties.Range, out float range); properties.Range = range;
            handler.TryReadValueFloat("HITBOXHEIGHTOFFSET", properties.HitboxHeightOffset, out float hitboxHeightOffset); properties.HitboxHeightOffset = hitboxHeightOffset;
            handler.TryReadValueFloat("DAMAGEPLAYERS", properties.DamagePlayers, out float damagePlayers); properties.DamagePlayers = damagePlayers;
            handler.TryReadValueFloat("DAMAGEOBJECTS", properties.DamageObjects, out float damageObjects); properties.DamageObjects = damageObjects;
            handler.TryReadValue("PLAYERHITSOUNDID", out string playerHitSoundID); properties.PlayerHitSoundID = string.IsNullOrEmpty(playerHitSoundID) ? properties.PlayerHitSoundID : playerHitSoundID;
            handler.TryReadValue("OBJECTHITSOUNDID", out string objectHitSoundID); properties.ObjectHitSoundID = string.IsNullOrEmpty(objectHitSoundID) ? properties.ObjectHitSoundID : objectHitSoundID;
            handler.TryReadValue("PLAYERHITEFFECTID", out string playerHitEffectID); properties.PlayerHitEffectID = string.IsNullOrEmpty(playerHitEffectID) ? properties.PlayerHitEffectID : playerHitEffectID;
            handler.TryReadValue("OBJECTHITEFFECTID", out string objectHitEffectID); properties.ObjectHitEffectID = string.IsNullOrEmpty(objectHitEffectID) ? properties.ObjectHitEffectID : objectHitEffectID;
            handler.TryReadValue("SWINGSOUNDID", out string swingSoundID); properties.SwingSoundID = string.IsNullOrEmpty(swingSoundID) ? properties.SwingSoundID : swingSoundID;
            handler.TryReadValue("DRAWSOUNDID", out string drawSoundID); properties.DrawSoundID = string.IsNullOrEmpty(drawSoundID) ? properties.DrawSoundID : drawSoundID;
            handler.TryReadValue("WEAPONMATERIAL", out string materialValue); properties.WeaponMaterial = string.IsNullOrEmpty(materialValue) ? properties.WeaponMaterial : MaterialDatabase.Get(materialValue);
            handler.TryReadValue("MELEEWEAPONTYPEENUM", out string meleeWeaponTypeEnum); properties.MeleeWeaponType = Enum.TryParse(meleeWeaponTypeEnum, out MeleeWeaponTypeEnum meleeWeaponTypeEnum2) ? meleeWeaponTypeEnum2 : MeleeWeaponTypeEnum.OneHanded;
            handler.TryReadValueFloat("DURABILITYLOSSONHITPLAYERS", properties.DurabilityLossOnHitPlayers, out float durabilityLossOnHitPlayers); properties.DurabilityLossOnHitPlayers = durabilityLossOnHitPlayers;
            handler.TryReadValueFloat("DURABILITYLOSSONHITBLOCKINGPLAYERS", properties.DurabilityLossOnHitBlockingPlayers, out float durabilityLossOnHitBlockingPlayers); properties.DurabilityLossOnHitBlockingPlayers = durabilityLossOnHitBlockingPlayers;
            handler.TryReadValueFloat("THROWNDURABILITYLOSSONHITPLAYERS", properties.ThrownDurabilityLossOnHitPlayers, out float thrownDurabilityLossOnHitPlayers); properties.ThrownDurabilityLossOnHitPlayers = thrownDurabilityLossOnHitPlayers;
            handler.TryReadValueFloat("THROWNDURABILITYLOSSONHITBLOCKINGPLAYERS", properties.ThrownDurabilityLossOnHitBlockingPlayers, out float thrownDurabilityLossOnHitBlockingPlayers); properties.ThrownDurabilityLossOnHitBlockingPlayers = thrownDurabilityLossOnHitBlockingPlayers;
            handler.TryReadValueFloat("DURABILITYLOSSONHITOBJECTS", properties.DurabilityLossOnHitObjects, out float durabilityLossOnHitObjects); properties.DurabilityLossOnHitObjects = durabilityLossOnHitObjects;
            // properties.DeflectionDuringBlock
            // properties.DeflectionOnAttack
            handler.TryReadValueInt("REPLACEMENTWEAPONID", properties.ReplacementWeaponID, out int replacementWeaponID); properties.ReplacementWeaponID = (short)replacementWeaponID;
            handler.TryReadValue("AI_DAMAGEOUTPUT", out string aI_DamageOutput); properties.AI_DamageOutput = Enum.TryParse(aI_DamageOutput, out DamageOutputType aI_DamageOutput2) ? aI_DamageOutput2 : properties.AI_DamageOutput;
            handler.TryReadValueBool("AUTODESTROYONDURABILITYEMPTY", properties.AutoDestroyOnDurabilityEmpty, out bool autoDestroyOnDurabilityEmpty); properties.AutoDestroyOnDurabilityEmpty = autoDestroyOnDurabilityEmpty;
            handler.TryReadValue("ANIMBLOCKUPPER", out string animBlockUpper); visuals.AnimBlockUpper = string.IsNullOrEmpty(animBlockUpper) ? visuals.AnimBlockUpper : animBlockUpper;
            handler.TryReadValue("ANIMMELEEATTACK1", out string animMeleeAttack1); visuals.AnimMeleeAttack1 = string.IsNullOrEmpty(animMeleeAttack1) ? visuals.AnimMeleeAttack1 : animMeleeAttack1;
            handler.TryReadValue("ANIMMELEEATTACK2", out string animMeleeAttack2); visuals.AnimMeleeAttack2 = string.IsNullOrEmpty(animMeleeAttack2) ? visuals.AnimMeleeAttack2 : animMeleeAttack2;
            handler.TryReadValue("ANIMMELEEATTACK3", out string animMeleeAttack3); visuals.AnimMeleeAttack3 = string.IsNullOrEmpty(animMeleeAttack3) ? visuals.AnimMeleeAttack3 : animMeleeAttack3;
            handler.TryReadValue("ANIMFULLJUMPATTACK", out string animFullJumpAttack); visuals.AnimFullJumpAttack = string.IsNullOrEmpty(animFullJumpAttack) ? visuals.AnimFullJumpAttack : animFullJumpAttack;
            handler.TryReadValue("MODELTEXTURENAME", out string modelTextureName); visuals.ModelTextureName = modelTextureName;
            handler.TryReadValue("HOLSTERTEXTURENAME", out string holsterTextureName); visuals.HolsterTextureName = holsterTextureName;
            handler.TryReadValue("SHEATHEDTEXTURENAME", out string sheathedTextureName); visuals.SheathedTextureName = sheathedTextureName;
            handler.TryReadValue("DRAWNTEXTURENAME", out string drawnTextureName); visuals.DrawnTextureName = drawnTextureName;
            handler.TryReadValue("THROWINGTEXTURENAME", out string throwingTextureName); visuals.ThrowingTextureName = throwingTextureName;
        }

        internal static void CopyPropertiesAndVisuals(ref RWeaponProperties toProperties, ref RWeaponVisuals toVisuals, WeaponBaseProperties fromProperties, WeaponBaseVisuals fromVisuals)
        {
            toProperties.Category = fromProperties.Category;
            toProperties.IsMakeshift = fromProperties.IsMakeshift;
            toProperties.ModelID = fromProperties.ModelID;
            toProperties.SpawnsInSheath = fromProperties.SpawnsInSheath;
            toProperties.VisualText = fromProperties.VisualText;
            toProperties.WeaponID = fromProperties.WeaponID;
            toProperties.WeaponNameID = fromProperties.WeaponNameID;
            toProperties.BreakDebris = fromProperties.BreakDebris;

            toVisuals.AnimToggleThrowingMode = fromVisuals.AnimToggleThrowingMode;
            toVisuals.AnimDraw = fromVisuals.AnimDraw;
            toVisuals.AnimRunUpper = fromVisuals.AnimRunUpper;
            toVisuals.AnimWalkUpper = fromVisuals.AnimWalkUpper;
            toVisuals.AnimStaggerUpper = fromVisuals.AnimStaggerUpper;
            toVisuals.AnimKickUpper = fromVisuals.AnimKickUpper;
            toVisuals.AnimJumpUpper = fromVisuals.AnimJumpUpper;
            toVisuals.AnimJumpUpperFalling = fromVisuals.AnimJumpUpperFalling;
            toVisuals.AnimJumpKickUpper = fromVisuals.AnimJumpKickUpper;
            toVisuals.AnimFullLand = fromVisuals.AnimFullLand;
            toVisuals.AnimCrouchUpper = fromVisuals.AnimCrouchUpper;
            toVisuals.AnimIdleUpper = fromVisuals.AnimIdleUpper;
            toVisuals.AnimReloadUpper = fromVisuals.AnimReloadUpper;
            toVisuals.AnimFullJumpAttack = fromVisuals.AnimFullJumpAttack;
        }
        internal static void CopyPropertiesAndVisuals(ref MWeaponProperties toProperties, ref MWeaponVisuals toVisuals, WeaponBaseProperties fromProperties, WeaponBaseVisuals fromVisuals)
        {
            toProperties.Category = fromProperties.Category;
            toProperties.IsMakeshift = fromProperties.IsMakeshift;
            toProperties.ModelID = fromProperties.ModelID;
            toProperties.SpawnsInSheath = fromProperties.SpawnsInSheath;
            toProperties.VisualText = fromProperties.VisualText;
            toProperties.WeaponID = fromProperties.WeaponID;
            toProperties.WeaponNameID = fromProperties.WeaponNameID;
            toProperties.BreakDebris = fromProperties.BreakDebris;

            if (string.IsNullOrEmpty(toVisuals.AnimToggleThrowingMode)) { toVisuals.AnimToggleThrowingMode = fromVisuals.AnimToggleThrowingMode; }
            if (string.IsNullOrEmpty(toVisuals.AnimDraw)) { toVisuals.AnimDraw = fromVisuals.AnimDraw; }
            if (string.IsNullOrEmpty(toVisuals.AnimRunUpper)) { toVisuals.AnimRunUpper = fromVisuals.AnimRunUpper; }
            if (string.IsNullOrEmpty(toVisuals.AnimWalkUpper)) { toVisuals.AnimWalkUpper = fromVisuals.AnimWalkUpper; }
            if (string.IsNullOrEmpty(toVisuals.AnimStaggerUpper)) { toVisuals.AnimStaggerUpper = fromVisuals.AnimStaggerUpper; }
            if (string.IsNullOrEmpty(toVisuals.AnimKickUpper)) { toVisuals.AnimKickUpper = fromVisuals.AnimKickUpper; }
            if (string.IsNullOrEmpty(toVisuals.AnimJumpUpper)) { toVisuals.AnimJumpUpper = fromVisuals.AnimJumpUpper; }
            if (string.IsNullOrEmpty(toVisuals.AnimJumpUpperFalling)) { toVisuals.AnimJumpUpperFalling = fromVisuals.AnimJumpUpperFalling; }
            if (string.IsNullOrEmpty(toVisuals.AnimJumpKickUpper)) { toVisuals.AnimJumpKickUpper = fromVisuals.AnimJumpKickUpper; }
            if (string.IsNullOrEmpty(toVisuals.AnimFullLand)) { toVisuals.AnimFullLand = fromVisuals.AnimFullLand; }
            if (string.IsNullOrEmpty(toVisuals.AnimCrouchUpper)) { toVisuals.AnimCrouchUpper = fromVisuals.AnimCrouchUpper; }
            if (string.IsNullOrEmpty(toVisuals.AnimIdleUpper)) { toVisuals.AnimIdleUpper = fromVisuals.AnimIdleUpper; }
            if (string.IsNullOrEmpty(toVisuals.AnimReloadUpper)) { toVisuals.AnimReloadUpper = fromVisuals.AnimReloadUpper; }
            if (string.IsNullOrEmpty(toVisuals.AnimFullJumpAttack)) { toVisuals.AnimFullJumpAttack = fromVisuals.AnimFullJumpAttack; }
        }

        internal static void ReadRWeaponExtra(IniHandler handler, ref WpnBaseRWeapon rweapon)
        {
            handler.TryReadValue("RELOADMAGAZINEEJECTSOUNDID", out string reloadMagazineEjectSoundID); rweapon.ReloadMagazineEjectSoundID = string.IsNullOrEmpty(reloadMagazineEjectSoundID) ? rweapon.ReloadMagazineEjectSoundID : reloadMagazineEjectSoundID;
            handler.TryReadValue("RELOADMAGAZINETILEID", out string reloadMagazineTileID); rweapon.ReloadMagazineTileID = string.IsNullOrEmpty(reloadMagazineTileID) ? rweapon.ReloadMagazineTileID : reloadMagazineTileID;
            handler.TryReadValue("RELOADMAGAZINEOFFSET", out string reloadMagazineOffset); rweapon.ReloadMagazineOffset = string.IsNullOrEmpty(reloadMagazineOffset) ? rweapon.ReloadMagazineOffset : new Vector2(SFDXParser.ParseFloat(reloadMagazineOffset.Split(' ')[0]), SFDXParser.ParseFloat(reloadMagazineOffset.Split(' ')[1]));
            handler.TryReadValueInt("RELOADMAGAZINEEJECTFRAMEINDEX", rweapon.ReloadMagazineEjectFrameIndex, out int reloadMagazineEjectFrameIndex); rweapon.ReloadMagazineEjectFrameIndex = (short)reloadMagazineEjectFrameIndex;
            handler.TryReadValue("RELOADSTARTSOUNDID", out string reloadStartSoundID); rweapon.ReloadStartSoundID = string.IsNullOrEmpty(reloadStartSoundID) ? rweapon.ReloadStartSoundID : reloadStartSoundID;
            handler.TryReadValueInt("RELOADSTARTSOUNDFRAMEINDEX", rweapon.ReloadStartSoundFrameIndex, out int reloadStartSoundFrameIndex); rweapon.ReloadStartSoundFrameIndex = (short)reloadStartSoundFrameIndex;
            handler.TryReadValue("RELOADENDSOUNDID", out string reloadEndSoundID); rweapon.ReloadEndSoundID = string.IsNullOrEmpty(reloadEndSoundID) ? rweapon.ReloadEndSoundID : reloadEndSoundID;
            handler.TryReadValueInt("RELOADENDSOUNDFRAMEINDEX", rweapon.ReloadEndSoundFrameIndex, out int reloadEndSoundFrameIndex); rweapon.ReloadEndSoundFrameIndex = (short)reloadEndSoundFrameIndex;
            handler.TryReadValueInt("RELOADSHELLSPAWNCOUNT", rweapon.ReloadShellSpawnCount, out int reloadShellSpawnCount); rweapon.ReloadShellSpawnCount = (short)reloadShellSpawnCount;
            handler.TryReadValueInt("RELOADSHELLSPAWNFRAMEINDEX", rweapon.ReloadShellSpawnFrameIndex, out int reloadShellSpawnFrameIndex); rweapon.ReloadShellSpawnFrameIndex = (short)reloadShellSpawnFrameIndex;
            handler.TryReadValue("DRAWSTARTSOUNDID", out string drawStartSoundID); rweapon.DrawStartSoundID = string.IsNullOrEmpty(drawStartSoundID) ? rweapon.DrawStartSoundID : drawStartSoundID;
            handler.TryReadValueInt("DRAWSTARTSOUNDFRAMEINDEX", rweapon.DrawStartSoundFrameIndex, out int drawStartSoundFrameIndex); rweapon.DrawStartSoundFrameIndex = (short)drawStartSoundFrameIndex;
            handler.TryReadValue("DRAWENDSOUNDID", out string drawEndSoundID); rweapon.DrawEndSoundID = string.IsNullOrEmpty(drawEndSoundID) ? rweapon.DrawEndSoundID : drawEndSoundID;
            handler.TryReadValueInt("DRAWENDSOUNDFRAMEINDEX", rweapon.DrawEndSoundFrameIndex, out int drawEndSoundFrameIndex); rweapon.DrawEndSoundFrameIndex = (short)drawEndSoundFrameIndex;
            handler.TryReadValue("THROWLINEARVELOCITYMODIFIER", out string throwLinearVelocityModifier); rweapon.ThrowLinearVelocityModifier = string.IsNullOrEmpty(throwLinearVelocityModifier) ? rweapon.ThrowLinearVelocityModifier : new Vector2(SFDXParser.ParseFloat(throwLinearVelocityModifier.Split(' ')[0]), SFDXParser.ParseFloat(throwLinearVelocityModifier.Split(' ')[1]));
            handler.TryReadValueFloat("THROWANGULARVELOCITYMODIFIER", rweapon.ThrowAngularVelocityModifier, out var throwAngularVelocityModifier);
        }
        internal static void ReadMWeaponExtra(IniHandler handler, ref WpnBaseMWeapon mweapon)
        {
            handler.TryReadValue("DESTROYEDSOUNDSID", out string destroyedSoundsIDValue);
            mweapon.DestroyedSoundsID = string.IsNullOrEmpty(destroyedSoundsIDValue) ? mweapon.DestroyedSoundsID : destroyedSoundsIDValue.Split(' ');
            handler.TryReadValue("DESTROYEDEFFECTSID", out string destroyedEffectsIDValue);
            mweapon.DestroyedEffectsID = string.IsNullOrEmpty(destroyedEffectsIDValue) ? mweapon.DestroyedEffectsID : destroyedEffectsIDValue.Split(' ');
            handler.TryReadValue("DESTROYEDCENTEROFFSET", out string destroyedCenterOffsetValue);
            mweapon.DestroyedCenterOffset = string.IsNullOrEmpty(destroyedCenterOffsetValue) ? mweapon.DestroyedCenterOffset : new Vector2(SFDXParser.ParseFloat(destroyedCenterOffsetValue.Split(' ')[0]), SFDXParser.ParseFloat(destroyedCenterOffsetValue.Split(' ')[1]));
        }

        internal static void Load()
        {
            IniHandler handler = new IniHandler();

            IEnumerable<string> scriptFilePaths = Directory.EnumerateFiles(Path.Combine(CGlobals.Paths.SCRIPTS, "Weapons"), "*.ini", SearchOption.AllDirectories);
            foreach(string scriptFilePath in scriptFilePaths)
            {
                string scriptFileName = Path.GetFileNameWithoutExtension(scriptFilePath);
                if (scriptFileName.StartsWith("_") || scriptFileName.Equals("BASE", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                short newWeaponID = (short)(Database.m_customWeaponsDic.Count + 69);
                if (newWeaponID < 69 || newWeaponID >= 255)
                {
                    string mess = $"CUSTOM WEAPONS: Tried to add weapon #{newWeaponID}, ID range is from 69 to 255!!";
                    Logger.LogError(mess);
                    throw new Exception(mess);
                }
                Logger.LogInfo($"CUSTOM WEAPONS: Reading file '{scriptFileName}'...");
                handler.ReadFile(scriptFilePath);

                WeaponItemType newWeaponType = handler.ReadValue("TYPE").Equals("MELEE", StringComparison.OrdinalIgnoreCase) ? WeaponItemType.Melee : (handler.ReadValue("TYPE").Equals("RIFLE", StringComparison.OrdinalIgnoreCase) ? WeaponItemType.Rifle : WeaponItemType.Handgun);
                WeaponCategory newWeaponCategory = newWeaponType == WeaponItemType.Melee ? WeaponCategory.Melee : (newWeaponType == WeaponItemType.Rifle ? WeaponCategory.Primary : WeaponCategory.Secondary);
                string newWeaponNameID = handler.ReadValue("NAMEID");
                object newWeaponData = null;

                WeaponBaseProperties newWeaponBaseProperties = GetBaseProperties();
                WeaponBaseVisuals newWeaponBaseVisuals = GetBaseVisuals();
                ReadBasePropertiesAndVisuals(handler, ref newWeaponBaseProperties, ref newWeaponBaseVisuals);
                
                newWeaponBaseProperties.WeaponID = newWeaponID;
                newWeaponBaseProperties.WeaponNameID = newWeaponNameID;
                newWeaponBaseProperties.Category = newWeaponCategory;
                newWeaponBaseProperties.ModelID = TileDatabase.Exist(newWeaponNameID) ? newWeaponNameID : "WpnPistol";
                if (string.IsNullOrEmpty(newWeaponBaseProperties.VisualText)) { newWeaponBaseProperties.VisualText = newWeaponNameID; }

                switch (newWeaponCategory)
                {
                    case WeaponCategory.Secondary:
                    case WeaponCategory.Primary:
                        RWeaponProperties rproperties = new RWeaponProperties(0, "", "", false, WeaponCategory.Supply);
                        RWeaponVisuals rvisuals = new RWeaponVisuals();

                        rproperties = newWeaponCategory == WeaponCategory.Primary ? GetBaseRifleProperties() : GetBaseHandgunProperties();
                        rvisuals = newWeaponCategory == WeaponCategory.Primary ? GetBaseRifleVisuals() : GetBaseHandgunVisuals();
                        CopyPropertiesAndVisuals(ref rproperties, ref rvisuals, newWeaponBaseProperties, newWeaponBaseVisuals);

                        ReadRPropertiesAndRVisuals(handler, ref rproperties, ref rvisuals);
                        rvisuals.SetDrawnTexture(rvisuals.DrawnTextureName);
                        rvisuals.SetModelTexture(rvisuals.ModelTextureName);
                        rvisuals.SetHolsterTexture(rvisuals.HolsterTextureName);
                        rvisuals.SetSheathedTexture(rvisuals.SheathedTextureName);
                        rvisuals.SetThrowingTexture(rvisuals.ThrowingTextureName);

                        WpnBaseRWeapon rweapon = new WpnBaseRWeapon(newWeaponID, rproperties, rvisuals);
                        ReadRWeaponExtra(handler, ref rweapon);

                        newWeaponData = rweapon;
                        break;
                    case WeaponCategory.Melee:
                        MWeaponProperties mproperties = new MWeaponProperties(newWeaponID, "", 0f, 0f, "", "", "", "", "", "", "", false, WeaponCategory.Supply, false);
                        MWeaponVisuals mvisuals = new MWeaponVisuals();

                        mproperties = GetBaseMeleeProperties();
                        mvisuals = GetBaseMeleeVisuals();
                        CopyPropertiesAndVisuals(ref mproperties, ref mvisuals, newWeaponBaseProperties, newWeaponBaseVisuals);

                        ReadMPropertiesAndMVisuals(handler, ref mproperties, ref mvisuals);
                        mvisuals.SetModelTexture(mvisuals.ModelTextureName);
                        mvisuals.SetHolsterTexture(mvisuals.HolsterTextureName);
                        mvisuals.SetSheathedTexture(mvisuals.SheathedTextureName);
                        mvisuals.SetDrawnTexture(mvisuals.DrawnTextureName);
                        mvisuals.SetThrowingTexture(mvisuals.ThrowingTextureName);

                        WpnBaseMWeapon mweapon = new WpnBaseMWeapon(newWeaponID, mproperties, mvisuals);
                        ReadMWeaponExtra(handler, ref mweapon);

                        newWeaponData = mweapon;
                        break;
                }

                Logger.LogInfo($"CUSTOM WEAPONS: - Weapon {newWeaponID} {newWeaponType}");

                WeaponItem weaponItem = new WeaponItem(newWeaponType, newWeaponData);
                Database.m_customWeaponsDic.Add(newWeaponID, weaponItem);

                Logger.LogInfo($"CUSTOM WEAPONS: - Added weapon #{newWeaponID} '{newWeaponNameID}'");

                handler.Clear();
            }
        }
    }
}
