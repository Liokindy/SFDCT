//using Lidgren.Network;
//using Microsoft.Xna.Framework;
//using Mono.Cecil;
//using SFD;
//using SFD.Materials;
//using SFD.Projectiles;
//using SFD.Weapons;
//using SFDCT.Helper;
//using SFDCT.Weapons;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace SFDCT.Sync.Custom
//{
//    public static class NetCTWeapon
//    {
//        public struct Data
//        {
//            public Data() { }

//            public WeaponItem Weapon;
//        }

//        public static NetOutgoingMessage Write(ref Data messageToWrite, NetOutgoingMessage netOutgoingMessage)
//        {
//            netOutgoingMessage.WriteRangedInteger(-1, 6, (int)messageToWrite.Weapon.Type);

//            WeaponBaseProperties baseProperties = messageToWrite.Weapon.BaseProperties;
//            netOutgoingMessage.Write((sbyte)baseProperties.Category);
//            netOutgoingMessage.Write(baseProperties.IsMakeshift);
//            netOutgoingMessage.Write(baseProperties.VisualText);
//            netOutgoingMessage.Write(baseProperties.WeaponNameID);
//            netOutgoingMessage.Write(baseProperties.WeaponID);
//            netOutgoingMessage.Write(baseProperties.ModelID);
//            netOutgoingMessage.Write(baseProperties.SpawnsInSheath);
//            netOutgoingMessage.Write(baseProperties.BreakDebris.Length > 0);
//            if (baseProperties.BreakDebris.Length > 0)
//            {
//                netOutgoingMessage.Write(baseProperties.BreakDebris.Length);

//                for (int i = 0; i < baseProperties.BreakDebris.Length; i++)
//                {
//                    netOutgoingMessage.Write(baseProperties.BreakDebris[i]);
//                }
//            }

//            WeaponBaseVisuals baseVisuals = null;
//            switch(messageToWrite.Weapon.Type)
//            {
//                case WeaponItemType.Rifle:
//                case WeaponItemType.Handgun:
//                    baseVisuals = messageToWrite.Weapon.RWeaponData.Visuals;
//                    break;
//                case WeaponItemType.Thrown:
//                    baseVisuals = messageToWrite.Weapon.TWeaponData.Visuals;
//                    break;
//                case WeaponItemType.Melee:
//                    baseVisuals = messageToWrite.Weapon.MWeaponData.Visuals;
//                    break;
//            }
//            netOutgoingMessage.Write(baseVisuals.AnimToggleThrowingMode);
//            netOutgoingMessage.Write(baseVisuals.AnimDraw);
//            netOutgoingMessage.Write(baseVisuals.AnimRunUpper);
//            netOutgoingMessage.Write(baseVisuals.AnimWalkUpper);
//            netOutgoingMessage.Write(baseVisuals.AnimStaggerUpper);
//            netOutgoingMessage.Write(baseVisuals.AnimKickUpper);
//            netOutgoingMessage.Write(baseVisuals.AnimJumpUpper);
//            netOutgoingMessage.Write(baseVisuals.AnimJumpUpperFalling);
//            netOutgoingMessage.Write(baseVisuals.AnimJumpKickUpper);
//            netOutgoingMessage.Write(baseVisuals.AnimFullLand);
//            netOutgoingMessage.Write(baseVisuals.AnimCrouchUpper);
//            netOutgoingMessage.Write(baseVisuals.AnimIdleUpper);
//            netOutgoingMessage.Write(baseVisuals.AnimReloadUpper);
//            netOutgoingMessage.Write(baseVisuals.AnimFullJumpAttack);

//            switch(messageToWrite.Weapon.Type)
//            {
//                case WeaponItemType.Melee:
//                    MWeaponProperties mproperties = messageToWrite.Weapon.MWeaponData.Properties;
//                    netOutgoingMessage.Write((float)mproperties.Range);
//                    netOutgoingMessage.Write((float)mproperties.HitboxHeightOffset);
//                    netOutgoingMessage.Write((float)mproperties.DamagePlayers);
//                    netOutgoingMessage.Write((float)mproperties.DamageObjects);
//                    netOutgoingMessage.Write((string)mproperties.PlayerHitSoundID);
//                    netOutgoingMessage.Write((string)mproperties.ObjectHitSoundID);
//                    netOutgoingMessage.Write((string)mproperties.PlayerHitEffectID);
//                    netOutgoingMessage.Write((string)mproperties.ObjectHitEffectID);
//                    netOutgoingMessage.Write((string)mproperties.SwingSoundID);
//                    netOutgoingMessage.Write((string)mproperties.DrawSoundID);
//                    netOutgoingMessage.Write((string)mproperties.WeaponMaterial.Key);
//                    netOutgoingMessage.Write((sbyte)mproperties.MeleeWeaponType);
//                    netOutgoingMessage.Write((float)mproperties.DurabilityLossOnHitPlayers);
//                    netOutgoingMessage.Write((float)mproperties.DurabilityLossOnHitBlockingPlayers);
//                    netOutgoingMessage.Write((float)mproperties.ThrownDurabilityLossOnHitPlayers);
//                    netOutgoingMessage.Write((float)mproperties.ThrownDurabilityLossOnHitBlockingPlayers);
//                    netOutgoingMessage.Write((float)mproperties.DurabilityLossOnHitObjects);
//                    netOutgoingMessage.Write((float)mproperties.DeflectionDuringBlock.DeflectCone);
//                    netOutgoingMessage.Write((sbyte)mproperties.DeflectionDuringBlock.DeflectType);
//                    netOutgoingMessage.Write((float)mproperties.DeflectionDuringBlock.DurabilityLoss);
//                    netOutgoingMessage.Write((float)mproperties.DeflectionOnAttack.DeflectCone);
//                    netOutgoingMessage.Write((sbyte)mproperties.DeflectionOnAttack.DeflectType);
//                    netOutgoingMessage.Write((float)mproperties.DeflectionOnAttack.DurabilityLoss);
//                    netOutgoingMessage.Write((short)mproperties.ReplacementWeaponID);
//                    netOutgoingMessage.Write((sbyte)mproperties.Handling);
//                    netOutgoingMessage.Write((bool)mproperties.AutoDestroyOnDurabilityEmpty);

//                    MWeaponVisuals mvisuals = messageToWrite.Weapon.MWeaponData.Visuals;
//                    netOutgoingMessage.Write((string)mvisuals.ModelTextureName);
//                    netOutgoingMessage.Write((string)mvisuals.HolsterTextureName);
//                    netOutgoingMessage.Write((string)mvisuals.SheathedTextureName);
//                    netOutgoingMessage.Write((string)mvisuals.DrawnTextureName);
//                    netOutgoingMessage.Write((string)mvisuals.ThrowingTextureName);
//                    netOutgoingMessage.Write((string)mvisuals.AnimBlockUpper);
//                    netOutgoingMessage.Write((string)mvisuals.AnimMeleeAttack1);
//                    netOutgoingMessage.Write((string)mvisuals.AnimMeleeAttack2);
//                    netOutgoingMessage.Write((string)mvisuals.AnimMeleeAttack3);
//                    break;
//                case WeaponItemType.Rifle:
//                case WeaponItemType.Handgun:
//                    RWeaponProperties rproperties = messageToWrite.Weapon.RWeaponData.Properties;
//                    netOutgoingMessage.Write(rproperties.MaxMagsInWeapon);
//                    netOutgoingMessage.Write(rproperties.MaxRoundsInMag);
//                    netOutgoingMessage.Write(rproperties.MaxCarriedSpareMags);
//                    netOutgoingMessage.Write(rproperties.StartMags);
//                    netOutgoingMessage.Write(rproperties.AccuracyDeflection);
//                    netOutgoingMessage.Write(rproperties.BurstRoundsToFire);
//                    netOutgoingMessage.Write(rproperties.BurstCooldown);
//                    netOutgoingMessage.Write(rproperties.CooldownBeforePostAction);
//                    netOutgoingMessage.Write(rproperties.CooldownAfterPostAction);
//                    netOutgoingMessage.Write(rproperties.ExtraAutomaticCooldown);
//                    netOutgoingMessage.Write(rproperties.ProjectilesEachBlast);
//                    netOutgoingMessage.Write(rproperties.ProjectileID);
//                    netOutgoingMessage.Write(rproperties.MuzzlePosition.X);
//                    netOutgoingMessage.Write(rproperties.MuzzlePosition.Y);
//                    netOutgoingMessage.Write(rproperties.LazerPosition.X);
//                    netOutgoingMessage.Write(rproperties.LazerPosition.Y);
//                    netOutgoingMessage.Write(rproperties.MuzzleEffectTextureID);
//                    netOutgoingMessage.Write(rproperties.BlastSoundID);
//                    netOutgoingMessage.Write(rproperties.DrawSoundID);
//                    netOutgoingMessage.Write(rproperties.AimStartSoundID);
//                    netOutgoingMessage.Write(rproperties.GrabAmmoSoundID);
//                    netOutgoingMessage.Write(rproperties.OutOfAmmoSoundID);
//                    netOutgoingMessage.Write(rproperties.ShellID);
//                    netOutgoingMessage.Write(rproperties.ClearRoundsOnReloadStart);
//                    netOutgoingMessage.Write(rproperties.CursorAimOffset.X);
//                    netOutgoingMessage.Write(rproperties.CursorAimOffset.Y);
//                    netOutgoingMessage.Write(rproperties.ReloadPostCooldown);
//                    netOutgoingMessage.Write(rproperties.CanRefilAtAmmoStashes);
//                    netOutgoingMessage.Write(rproperties.CanUseFireBulletsPowerup);
//                    netOutgoingMessage.Write(rproperties.CanUseBouncingBulletsPowerup);
//                    netOutgoingMessage.Write(rproperties.SpecialAmmoBulletsRefill);

//                    RWeaponVisuals rvisuals = messageToWrite.Weapon.RWeaponData.Visuals;
//                    netOutgoingMessage.Write(rvisuals.ModelTextureName);
//                    netOutgoingMessage.Write(rvisuals.HolsterTextureName);
//                    netOutgoingMessage.Write(rvisuals.SheathedTextureName);
//                    netOutgoingMessage.Write(rvisuals.DrawnTextureName);
//                    netOutgoingMessage.Write(rvisuals.ThrowingTextureName);
//                    netOutgoingMessage.Write(rvisuals.RecoilDistance);
//                    netOutgoingMessage.Write(rvisuals.AnimFireArmLength);
//                    netOutgoingMessage.Write(rvisuals.HipFireWeaponOffset.X);
//                    netOutgoingMessage.Write(rvisuals.HipFireWeaponOffset.Y);
//                    netOutgoingMessage.Write(rvisuals.AnimManualAim);
//                    netOutgoingMessage.Write(rvisuals.AnimManualAimStart);
//                    netOutgoingMessage.Write(rvisuals.m_animUpperHipfire);
//                    break;
//            }

//            return netOutgoingMessage;
//        }
//        public static Data Read(NetIncomingMessage netIncomingMessage)
//        {
//            WeaponItemType wpnItemType = (WeaponItemType)netIncomingMessage.ReadRangedInteger(-1, 6);
//            object wpnData = null;

//            WeaponBaseProperties baseProperties = new WeaponBaseProperties("", 0, "", false, WeaponCategory.Supply, false);
//            baseProperties.Category = (WeaponCategory)netIncomingMessage.ReadSByte();
//            baseProperties.IsMakeshift = netIncomingMessage.ReadBoolean();
//            baseProperties.VisualText = netIncomingMessage.ReadString();
//            baseProperties.WeaponNameID = netIncomingMessage.ReadString();
//            baseProperties.WeaponID = netIncomingMessage.ReadInt16();
//            baseProperties.ModelID = netIncomingMessage.ReadString();
//            baseProperties.SpawnsInSheath = netIncomingMessage.ReadBoolean();
//            baseProperties.BreakDebris = [];
//            if (netIncomingMessage.ReadBoolean())
//            {
//                baseProperties.BreakDebris = new string[netIncomingMessage.ReadInt32()];

//                for (int i = 0; i < baseProperties.BreakDebris.Length; i++)
//                {
//                    baseProperties.BreakDebris[i] = netIncomingMessage.ReadString();
//                }
//            }

//            WeaponBaseVisuals baseVisuals = new WeaponBaseVisuals();
//            baseVisuals.AnimToggleThrowingMode = netIncomingMessage.ReadString();
//            baseVisuals.AnimDraw = netIncomingMessage.ReadString();
//            baseVisuals.AnimRunUpper = netIncomingMessage.ReadString();
//            baseVisuals.AnimWalkUpper = netIncomingMessage.ReadString();
//            baseVisuals.AnimStaggerUpper = netIncomingMessage.ReadString();
//            baseVisuals.AnimKickUpper = netIncomingMessage.ReadString();
//            baseVisuals.AnimJumpUpper = netIncomingMessage.ReadString();
//            baseVisuals.AnimJumpUpperFalling = netIncomingMessage.ReadString();
//            baseVisuals.AnimJumpKickUpper = netIncomingMessage.ReadString();
//            baseVisuals.AnimFullLand = netIncomingMessage.ReadString();
//            baseVisuals.AnimCrouchUpper = netIncomingMessage.ReadString();
//            baseVisuals.AnimIdleUpper = netIncomingMessage.ReadString();
//            baseVisuals.AnimReloadUpper = netIncomingMessage.ReadString();
//            baseVisuals.AnimFullJumpAttack = netIncomingMessage.ReadString();

//            switch(wpnItemType)
//            {
//                case WeaponItemType.Melee:
//                    MWeaponProperties mproperties = new MWeaponProperties(0, "", 0, 0, "", "", "", "", "", "", "", false, WeaponCategory.Supply, false);
//                    mproperties.Category = baseProperties.Category;
//                    mproperties.IsMakeshift = baseProperties.IsMakeshift;
//                    mproperties.VisualText = baseProperties.VisualText;
//                    mproperties.WeaponNameID = baseProperties.WeaponNameID;
//                    mproperties.WeaponID = baseProperties.WeaponID;
//                    mproperties.ModelID = baseProperties.ModelID;
//                    mproperties.SpawnsInSheath = baseProperties.SpawnsInSheath;
//                    mproperties.BreakDebris = baseProperties.BreakDebris;
//                    mproperties.Range = netIncomingMessage.ReadSingle();
//                    mproperties.HitboxHeightOffset = netIncomingMessage.ReadSingle();
//                    mproperties.DamagePlayers = netIncomingMessage.ReadSingle();
//                    mproperties.DamageObjects = netIncomingMessage.ReadSingle();
//                    mproperties.PlayerHitSoundID = netIncomingMessage.ReadString();
//                    mproperties.ObjectHitSoundID = netIncomingMessage.ReadString();
//                    mproperties.PlayerHitEffectID = netIncomingMessage.ReadString();
//                    mproperties.ObjectHitEffectID = netIncomingMessage.ReadString();
//                    mproperties.SwingSoundID = netIncomingMessage.ReadString();
//                    mproperties.DrawSoundID = netIncomingMessage.ReadString();
//                    mproperties.WeaponMaterial = MaterialDatabase.Get(netIncomingMessage.ReadString());
//                    mproperties.MeleeWeaponType = (MeleeWeaponTypeEnum)netIncomingMessage.ReadSByte();
//                    mproperties.DurabilityLossOnHitPlayers = netIncomingMessage.ReadSingle();
//                    mproperties.DurabilityLossOnHitBlockingPlayers = netIncomingMessage.ReadSingle();
//                    mproperties.ThrownDurabilityLossOnHitPlayers = netIncomingMessage.ReadSingle();
//                    mproperties.ThrownDurabilityLossOnHitBlockingPlayers = netIncomingMessage.ReadSingle();
//                    mproperties.DurabilityLossOnHitObjects = netIncomingMessage.ReadSingle();
//                    mproperties.DeflectionDuringBlock = new DeflectionProperties(0.0f);
//                    mproperties.DeflectionDuringBlock.DeflectCone = netIncomingMessage.ReadSingle();
//                    mproperties.DeflectionDuringBlock.DeflectType = (DeflectBulletType)netIncomingMessage.ReadSByte();
//                    mproperties.DeflectionDuringBlock.DurabilityLoss = netIncomingMessage.ReadSingle();
//                    mproperties.DeflectionOnAttack = new DeflectionProperties(0.0f);
//                    mproperties.DeflectionOnAttack.DeflectCone = netIncomingMessage.ReadSingle();
//                    mproperties.DeflectionOnAttack.DeflectType = (DeflectBulletType)netIncomingMessage.ReadSByte();
//                    mproperties.DeflectionOnAttack.DurabilityLoss = netIncomingMessage.ReadSingle();
//                    mproperties.ReplacementWeaponID = netIncomingMessage.ReadInt16();
//                    mproperties.Handling = (MeleeHandlingType)netIncomingMessage.ReadSByte();
//                    mproperties.AutoDestroyOnDurabilityEmpty = netIncomingMessage.ReadBoolean();

//                    MWeaponVisuals mvisuals = new MWeaponVisuals();
//                    mvisuals.AnimToggleThrowingMode = baseVisuals.AnimToggleThrowingMode;
//                    mvisuals.AnimDraw = baseVisuals.AnimDraw;
//                    mvisuals.AnimRunUpper = baseVisuals.AnimRunUpper;
//                    mvisuals.AnimWalkUpper = baseVisuals.AnimWalkUpper;
//                    mvisuals.AnimStaggerUpper = baseVisuals.AnimStaggerUpper;
//                    mvisuals.AnimKickUpper = baseVisuals.AnimKickUpper;
//                    mvisuals.AnimJumpUpper = baseVisuals.AnimJumpUpper;
//                    mvisuals.AnimJumpUpperFalling = baseVisuals.AnimJumpUpperFalling;
//                    mvisuals.AnimJumpKickUpper = baseVisuals.AnimJumpKickUpper;
//                    mvisuals.AnimFullLand = baseVisuals.AnimFullLand;
//                    mvisuals.AnimCrouchUpper = baseVisuals.AnimCrouchUpper;
//                    mvisuals.AnimIdleUpper = baseVisuals.AnimIdleUpper;
//                    mvisuals.AnimReloadUpper = baseVisuals.AnimReloadUpper;
//                    mvisuals.AnimFullJumpAttack = baseVisuals.AnimFullJumpAttack;
//                    mvisuals.ModelTextureName = netIncomingMessage.ReadString();
//                    mvisuals.SetModelTexture(mvisuals.ModelTextureName);
//                    mvisuals.HolsterTextureName = netIncomingMessage.ReadString();
//                    mvisuals.SetHolsterTexture(mvisuals.HolsterTextureName);
//                    mvisuals.SheathedTextureName = netIncomingMessage.ReadString();
//                    mvisuals.SetSheathedTexture(mvisuals.SheathedTextureName);
//                    mvisuals.DrawnTextureName = netIncomingMessage.ReadString();
//                    mvisuals.SetDrawnTexture(mvisuals.DrawnTextureName);
//                    mvisuals.ThrowingTextureName = netIncomingMessage.ReadString();
//                    mvisuals.SetThrowingTexture(mvisuals.ThrowingTextureName);
//                    mvisuals.AnimBlockUpper = netIncomingMessage.ReadString();
//                    mvisuals.AnimMeleeAttack1 = netIncomingMessage.ReadString();
//                    mvisuals.AnimMeleeAttack2 = netIncomingMessage.ReadString();
//                    mvisuals.AnimMeleeAttack3 = netIncomingMessage.ReadString();

//                    wpnData = new WpnBaseMWeapon(mproperties.WeaponID, mproperties, mvisuals);
//                    break;
//                case WeaponItemType.Handgun:
//                case WeaponItemType.Rifle:
//                    RWeaponProperties rproperties = new RWeaponProperties(0, "", "", false, WeaponCategory.Supply);
//                    rproperties.Category = baseProperties.Category;
//                    rproperties.IsMakeshift = baseProperties.IsMakeshift;
//                    rproperties.VisualText = baseProperties.VisualText;
//                    rproperties.WeaponNameID = baseProperties.WeaponNameID;
//                    rproperties.WeaponID = baseProperties.WeaponID;
//                    rproperties.ModelID = baseProperties.ModelID;
//                    rproperties.SpawnsInSheath = baseProperties.SpawnsInSheath;
//                    rproperties.BreakDebris = baseProperties.BreakDebris;
//                    rproperties.MaxMagsInWeapon = netIncomingMessage.ReadUInt16();
//                    rproperties.MaxRoundsInMag = netIncomingMessage.ReadUInt16();
//                    rproperties.MaxCarriedSpareMags = netIncomingMessage.ReadUInt16();
//                    rproperties.StartMags = netIncomingMessage.ReadUInt16();
//                    rproperties.AccuracyDeflection = netIncomingMessage.ReadSingle();
//                    rproperties.BurstRoundsToFire = netIncomingMessage.ReadUInt16();
//                    rproperties.BurstCooldown = netIncomingMessage.ReadUInt16();
//                    rproperties.CooldownBeforePostAction = netIncomingMessage.ReadUInt16();
//                    rproperties.CooldownAfterPostAction = netIncomingMessage.ReadUInt16();
//                    rproperties.ExtraAutomaticCooldown = netIncomingMessage.ReadUInt16();
//                    rproperties.ProjectilesEachBlast = netIncomingMessage.ReadUInt16();
//                    rproperties.ProjectileID = netIncomingMessage.ReadInt16();
//                    rproperties.MuzzlePosition = new Vector2(netIncomingMessage.ReadSingle(), netIncomingMessage.ReadSingle());
//                    rproperties.LazerPosition = new Vector2(netIncomingMessage.ReadSingle(), netIncomingMessage.ReadSingle());
//                    rproperties.MuzzleEffectTextureID = netIncomingMessage.ReadString();
//                    rproperties.BlastSoundID = netIncomingMessage.ReadString();
//                    rproperties.DrawSoundID = netIncomingMessage.ReadString();
//                    rproperties.AimStartSoundID = netIncomingMessage.ReadString();
//                    rproperties.GrabAmmoSoundID = netIncomingMessage.ReadString();
//                    rproperties.OutOfAmmoSoundID = netIncomingMessage.ReadString();
//                    rproperties.ShellID = netIncomingMessage.ReadString();
//                    rproperties.ClearRoundsOnReloadStart = netIncomingMessage.ReadBoolean();
//                    rproperties.CursorAimOffset = new Vector2(netIncomingMessage.ReadSingle(), netIncomingMessage.ReadSingle());
//                    rproperties.ReloadPostCooldown = netIncomingMessage.ReadSingle();
//                    rproperties.CanRefilAtAmmoStashes = netIncomingMessage.ReadBoolean();
//                    rproperties.CanUseFireBulletsPowerup = netIncomingMessage.ReadBoolean();
//                    rproperties.CanUseBouncingBulletsPowerup = netIncomingMessage.ReadBoolean();
//                    rproperties.SpecialAmmoBulletsRefill = netIncomingMessage.ReadInt32();

//                    RWeaponVisuals rvisuals = new RWeaponVisuals();
//                    rvisuals.AnimToggleThrowingMode = baseVisuals.AnimToggleThrowingMode;
//                    rvisuals.AnimDraw = baseVisuals.AnimDraw;
//                    rvisuals.AnimRunUpper = baseVisuals.AnimRunUpper;
//                    rvisuals.AnimWalkUpper = baseVisuals.AnimWalkUpper;
//                    rvisuals.AnimStaggerUpper = baseVisuals.AnimStaggerUpper;
//                    rvisuals.AnimKickUpper = baseVisuals.AnimKickUpper;
//                    rvisuals.AnimJumpUpper = baseVisuals.AnimJumpUpper;
//                    rvisuals.AnimJumpUpperFalling = baseVisuals.AnimJumpUpperFalling;
//                    rvisuals.AnimJumpKickUpper = baseVisuals.AnimJumpKickUpper;
//                    rvisuals.AnimFullLand = baseVisuals.AnimFullLand;
//                    rvisuals.AnimCrouchUpper = baseVisuals.AnimCrouchUpper;
//                    rvisuals.AnimIdleUpper = baseVisuals.AnimIdleUpper;
//                    rvisuals.AnimReloadUpper = baseVisuals.AnimReloadUpper;
//                    rvisuals.AnimFullJumpAttack = baseVisuals.AnimFullJumpAttack;
//                    rvisuals.ModelTextureName = netIncomingMessage.ReadString();
//                    rvisuals.SetModelTexture(rvisuals.ModelTextureName);
//                    rvisuals.HolsterTextureName = netIncomingMessage.ReadString();
//                    rvisuals.SetHolsterTexture(rvisuals.HolsterTextureName);
//                    rvisuals.SheathedTextureName = netIncomingMessage.ReadString();
//                    rvisuals.SetSheathedTexture(rvisuals.SheathedTextureName);
//                    rvisuals.DrawnTextureName = netIncomingMessage.ReadString();
//                    rvisuals.SetDrawnTexture(rvisuals.DrawnTextureName);
//                    rvisuals.ThrowingTextureName = netIncomingMessage.ReadString();
//                    rvisuals.SetThrowingTexture(rvisuals.ThrowingTextureName);
//                    rvisuals.RecoilDistance = netIncomingMessage.ReadSingle();
//                    rvisuals.AnimFireArmLength = netIncomingMessage.ReadSingle();
//                    rvisuals.HipFireWeaponOffset = new Vector2(netIncomingMessage.ReadSingle(), netIncomingMessage.ReadSingle());
//                    rvisuals.AnimManualAim = netIncomingMessage.ReadString();
//                    rvisuals.AnimManualAimStart = netIncomingMessage.ReadString();
//                    rvisuals.m_animUpperHipfire = netIncomingMessage.ReadString();

//                    wpnData = new WpnBaseRWeapon(rproperties.WeaponID, rproperties, rvisuals);
//                    break;
//            }

//            Data result = new Data();
//            result.Weapon = new WeaponItem(wpnItemType, wpnData);
//            return result;
//        }
//    }
//}
