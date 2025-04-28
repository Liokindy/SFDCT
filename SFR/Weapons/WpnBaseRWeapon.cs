using System;
using Microsoft.Xna.Framework;
using SFD;
using SFD.Objects;
using SFD.Sounds;
using SFD.Weapons;

namespace SFDCT.Weapons
{
    public class WpnBaseRWeapon : RWeapon
    {
        public WpnBaseRWeapon(short weaponID, RWeaponProperties properties, RWeaponVisuals visuals)
        {
            this.SetPropertiesAndVisuals(properties, visuals);
            this.Properties.WeaponID = weaponID;

            this.ReloadMagazineEjectSoundID = string.Empty;
            this.ReloadMagazineTileID = string.Empty;
            this.ReloadMagazineOffset = Vector2.Zero;
            this.ReloadMagazineEjectFrameIndex = -1;
            this.ReloadStartSoundID = string.Empty;
            this.ReloadStartSoundFrameIndex = -1;
            this.ReloadEndSoundID = string.Empty;
            this.ReloadEndSoundFrameIndex = -1;
            this.ReloadShellSpawnCount = 0;
            this.ReloadShellSpawnFrameIndex = -1;

            this.DrawStartSoundID = string.Empty;
            this.DrawStartSoundFrameIndex = -1;
            this.DrawEndSoundID = string.Empty;
            this.DrawEndSoundFrameIndex = -1;

            this.ThrowLinearVelocityModifier = Vector2.One;
            this.ThrowAngularVelocityModifier = 1f;
        }

        public string ReloadMagazineEjectSoundID;
        public string ReloadMagazineTileID;
        public Vector2 ReloadMagazineOffset;
        public short ReloadMagazineEjectFrameIndex;
        public string ReloadStartSoundID;
        public short ReloadStartSoundFrameIndex;
        public string ReloadEndSoundID;
        public short ReloadEndSoundFrameIndex;
        public short ReloadShellSpawnCount;
        public short ReloadShellSpawnFrameIndex;

        public string DrawStartSoundID;
        public short DrawStartSoundFrameIndex;
        public string DrawEndSoundID;
        public short DrawEndSoundFrameIndex;

        public Vector2 ThrowLinearVelocityModifier;
        public float ThrowAngularVelocityModifier;


        public override void OnReloadAnimationEvent(Player player, AnimationEvent animEvent, SubAnimationPlayer subAnim)
        {
            if (player.GameOwner != GameOwnerEnum.Server && animEvent == AnimationEvent.EnterFrame)
            {
                if (this.ReloadShellSpawnFrameIndex != -1 && subAnim.GetCurrentFrameIndex() == this.ReloadShellSpawnFrameIndex)
                {
                    for (short i = 0; i < this.ReloadShellSpawnCount; i++)
                    {
                        base.SpawnUnsyncedShell(player, this.Properties.ShellID);
                    }
                }

                if (this.ReloadMagazineEjectFrameIndex != -1 && subAnim.GetCurrentFrameIndex() == this.ReloadMagazineEjectFrameIndex)
                {
                    base.SpawnMagazine(player, this.ReloadMagazineTileID, ReloadMagazineOffset);
                }

                if (this.ReloadStartSoundFrameIndex != -1 && subAnim.GetCurrentFrameIndex() == this.ReloadStartSoundFrameIndex)
                {
                    SoundHandler.PlaySound(this.ReloadStartSoundID, player.Position, player.GameWorld);    
                }

                if (this.ReloadEndSoundFrameIndex != -1 && subAnim.GetCurrentFrameIndex() == this.ReloadEndSoundFrameIndex)
                {
                    SoundHandler.PlaySound(this.ReloadEndSoundID, player.Position, player.GameWorld);    
                }
            }
        }
        public override void OnSubAnimationEvent(Player player, AnimationEvent animationEvent, AnimationData animationData, int currentFrameIndex)
        {
            if (player.GameOwner != GameOwnerEnum.Server && animationEvent == AnimationEvent.EnterFrame && animationData.Name == this.Visuals.AnimDraw)
            {
                if (this.DrawStartSoundFrameIndex != -1 && currentFrameIndex == this.DrawStartSoundFrameIndex)
                {
                    SoundHandler.PlaySound(this.DrawStartSoundID, player.Position, player.GameWorld);
                }

                if (this.DrawEndSoundFrameIndex != -1 && currentFrameIndex == this.DrawEndSoundFrameIndex)
                {
                    SoundHandler.PlaySound(this.DrawEndSoundID, player.Position, player.GameWorld);
                }
            }
        }

        public override void OnThrowWeaponItem(Player player, ObjectWeaponItem thrownWeaponItem)
        {
            thrownWeaponItem.Body.SetAngularVelocity(thrownWeaponItem.Body.GetAngularVelocity() * this.ThrowAngularVelocityModifier);
            thrownWeaponItem.Body.SetLinearVelocity(thrownWeaponItem.Body.GetLinearVelocity() * this.ThrowLinearVelocityModifier);

            base.OnThrowWeaponItem(player, thrownWeaponItem);
        }

        public override RWeapon Copy()
        {
            WpnBaseRWeapon result = new WpnBaseRWeapon(this.Properties.WeaponID, this.Properties, this.Visuals);
            result.CopyStatsFrom(this);
            return result;
        }
    }
}
