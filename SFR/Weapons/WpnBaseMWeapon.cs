using System;
using Microsoft.Xna.Framework;
using SFD;
using SFD.Effects;
using SFD.Sounds;
using SFD.Weapons;

namespace SFDCT.Weapons
{
    public class WpnBaseMWeapon : MWeapon
    {
        public WpnBaseMWeapon(short weaponID, MWeaponProperties properties, MWeaponVisuals visuals)
        {
            this.SetPropertiesAndVisuals(properties, visuals);
            this.Properties.WeaponID = weaponID;

            this.DestroyedSoundsID = [];
            this.DestroyedEffectsID = [];
            this.DestroyedCenterOffset = new Vector2(0, 16);
        }

        public string[] DestroyedSoundsID;
        public string[] DestroyedEffectsID;
        public Vector2 DestroyedCenterOffset;

        public override void Destroyed(Player ownerPlayer)
        {
            if (DestroyedSoundsID.Length > 0)
            {
                for (int i = 0; i < DestroyedSoundsID.Length; i++)
                {
                    SoundHandler.PlaySound(DestroyedSoundsID[i], ownerPlayer.Position, ownerPlayer.GameWorld);
                }
            }

            if (DestroyedEffectsID.Length > 0)
            {
                for (int i = 0; i < DestroyedEffectsID.Length; i++)
                {
                    EffectHandler.PlayEffect(DestroyedEffectsID[i], ownerPlayer.Position, ownerPlayer.GameWorld);
                }
            }
            
            ownerPlayer.GameWorld.SpawnDebris(ownerPlayer.ObjectData, ownerPlayer.Position + DestroyedCenterOffset, 8f, this.Properties.BreakDebris, 1, true);
        }

        public override MWeapon Copy()
        {
            WpnBaseMWeapon result = new WpnBaseMWeapon(this.Properties.WeaponID, this.Properties, this.Visuals);
            result.Durability.CurrentValue = this.Durability.CurrentValue;
            return result;
        }
    }
}
