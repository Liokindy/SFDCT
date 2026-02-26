using HarmonyLib;
using Microsoft.Xna.Framework;
using SFD;
using SFD.Projectiles;
using SFD.Sounds;
using SFDCT.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace SFDCT.Sound;

[HarmonyPatch]
internal static class PanningHandler
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(SoundHandler), nameof(SoundHandler.PlaySound), [typeof(string), typeof(Vector2), typeof(float), typeof(GameWorld)])]
    private static bool SoundHandler_PlaySound_Prefix_SoundPanning(string soundID, Vector2 worldPosition, float volumeModifier, GameWorld gameWorld)
    {
        if (gameWorld == null || gameWorld.InLoading || gameWorld.MuteSounds) return false;

        if (gameWorld.GameOwner == GameOwnerEnum.Server)
        {
            if (SoundHandler.game.Server == null)
            {
                throw new Exception("Error: SoundHandler.PlaySound() parameter isServer is true even though the server doesn't exist");
            }

            SoundHandler.game.Server.SendMessage(MessageType.Sound, new NetMessage.Sound.Data(soundID, false, Converter.WorldToBox2D(worldPosition), volumeModifier));
        }
        else
        {
            PlayGlobalSound(soundID, gameWorld, worldPosition, volumeModifier);
        }

        return false;
    }

    internal static void PlayGlobalSound(string soundID, GameWorld gameWorld, Vector2 worldPosition, float volumeModifier = 1f)
    {
        if (SoundHandler.m_soundsDisabled) return;
        if (soundID.Equals("none", StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(soundID)) return;

        var group = SoundHandler.soundEffects.Find(soundID);

        if (group == null)
        {
            ConsoleOutput.ShowMessage(ConsoleOutputType.Warning, $"Sound '{soundID}' could not be found");
            return;
        }

        var pitch = gameWorld.SlowmotionHandler.SlowmotionModifier - 1f; // [-1, 1]
        var volume = 1f;
        var pan = 0f;

        if (worldPosition != Vector2.Zero)
        {
            var invalidLocalPlayer = GameInfo.LocalPlayerCount > 1
                                            || gameWorld.PrimaryLocalPlayer == null
                                            || gameWorld.PrimaryLocalPlayer.IsDisposed
                                            || gameWorld.PrimaryLocalPlayer.IsRemoved
                                            || gameWorld.PrimaryLocalPlayer.IsDead;

            if (SFDCTConfig.Get<bool>(CTSettingKey.SoundAttenuationEnabled))
            {
                if (SFDCTConfig.Get<bool>(CTSettingKey.SoundAttenuationForceScreenSpace) || invalidLocalPlayer)
                {
                    var distanceFromCenter = (Camera.ConvertWorldToScreen(worldPosition) - new Vector2(GameSFD.GAME_WIDTHf, GameSFD.GAME_HEIGHTf) * 0.5f).CalcSafeLength();

                    if (distanceFromCenter <= 0f)
                    {
                        volume = 0f;
                    }
                    else
                    {
                        volume = distanceFromCenter / MathHelper.Max(GameSFD.GAME_WIDTHf, GameSFD.GAME_HEIGHTf);
                    }
                }
                else
                {
                    var worldThreshold = (float)SFDCTConfig.Get<int>(CTSettingKey.SoundAttenuationInworldThreshold);
                    var worldDistance = (float)SFDCTConfig.Get<int>(CTSettingKey.SoundAttenuationInworldDistance);
                    var dist = (worldPosition - gameWorld.PrimaryLocalPlayer.Position).CalcSafeLength();

                    if (dist >= worldThreshold)
                    {
                        volume = (dist - worldThreshold) / worldDistance;
                    }
                    else
                    {
                        volume = 0f;
                    }
                }

                volume = MathHelper.Clamp(1 - volume, SFDCTConfig.Get<float>(CTSettingKey.SoundAttenuationMin), 1f);
            }

            if (SFDCTConfig.Get<bool>(CTSettingKey.SoundPanningEnabled))
            {
                if (SFDCTConfig.Get<bool>(CTSettingKey.SoundPanningForceScreenSpace) || invalidLocalPlayer)
                {
                    pan = (Camera.ConvertWorldToScreenX(worldPosition.X) - GameSFD.GAME_WIDTHf * 0.5f) / GameSFD.GAME_WIDTHf * 0.5f;
                }
                else
                {
                    var spWorldThreshold = (float)SFDCTConfig.Get<int>(CTSettingKey.SoundPanningInworldThreshold);
                    var spWorldDistance = (float)SFDCTConfig.Get<int>(CTSettingKey.SoundPanningInworldDistance);

                    var horizontalDistance = worldPosition.X - gameWorld.PrimaryLocalPlayer.Position.X;

                    if (Math.Abs(horizontalDistance) >= spWorldThreshold)
                    {
                        pan = (horizontalDistance - spWorldThreshold) / spWorldDistance;
                    }
                }
            }
        }

        volume = volume * group.VolumeModifier * volumeModifier;
        pan = MathHelper.Clamp(pan * SFDCTConfig.Get<float>(CTSettingKey.SoundPanningStrength), -1f, 1f);

        SoundHandler.PlaySoundEffectGroup(group, volume, pitch, pan);
    }

    // A lot of objects in SFD play sounds and don't specify their world position,
    // so they are played as a global sound, these patches fix a majority of these,
    // but not all.

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(ObjectData), nameof(ObjectData.OnDestroyGenericCheck))]
    private static IEnumerable<CodeInstruction> ObjectData_OnDestroyGenericCheck_Transpiler_AddSoundPosition(IEnumerable<CodeInstruction> instructions)
    {
        var code = new List<CodeInstruction>(instructions);

        code.Insert(29, new(OpCodes.Ldarg_0));
        code.Insert(30, new(OpCodes.Call, AccessTools.Method(typeof(ObjectData), nameof(ObjectData.GetWorldPosition))));
        code.ElementAt(33).operand = AccessTools.Method(typeof(SoundHandler), nameof(SoundHandler.PlaySound), [typeof(string), typeof(Vector2), typeof(GameWorld)]);

        return code;
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(Projectile), nameof(Projectile.DefaultHitObject))]
    private static IEnumerable<CodeInstruction> Projectile_DefaultHitObject_Transpiler_AddSoundPosition(IEnumerable<CodeInstruction> instructions)
    {
        var code = new List<CodeInstruction>(instructions);

        code.ElementAt(33).operand = AccessTools.Method(typeof(SoundHandler), nameof(SoundHandler.PlaySound), [typeof(string), typeof(Vector2), typeof(GameWorld)]);
        code.Insert(31, new CodeInstruction(OpCodes.Ldarg_0));
        code.Insert(32, new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(Projectile), nameof(Projectile.Position))));

        return code;
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(Projectile), nameof(Projectile.DefaultHitPlayer))]
    private static IEnumerable<CodeInstruction> Projectile_DefaultHitPlayer_Transpiler_AddSoundPosition(IEnumerable<CodeInstruction> instructions)
    {
        var code = new List<CodeInstruction>(instructions);

        code.ElementAt(22).operand = AccessTools.Method(typeof(SoundHandler), nameof(SoundHandler.PlaySound), [typeof(string), typeof(Vector2), typeof(GameWorld)]);
        code.Insert(20, new CodeInstruction(OpCodes.Ldarg_0));
        code.Insert(21, new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(Projectile), nameof(Projectile.Position))));

        return code;
    }
}
