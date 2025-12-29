using HarmonyLib;
using Microsoft.Xna.Framework;
using SFD;
using SFD.Sounds;
using SFDCT.Configuration;
using System;

namespace SFDCT.Sound;

[HarmonyPatch]
internal static class Panning
{
    // These properties are used for patching instructions in projectiles and objects that play sounds,
    // until SFD fixes those lines and gives them proper position arguments.

    // internal static readonly Type typeof_SoundHandler = typeof(SoundHandler);
    // internal static readonly string nameof_SoundHandlerPlaySound = nameof(SoundHandler.PlaySound);
    // internal static readonly Type[] typeof_String_Vector2_GameWorld = [typeof(string), typeof(Vector2), typeof(GameWorld)];

    [HarmonyPrefix]
    [HarmonyPatch(typeof(SoundHandler), nameof(SoundHandler.PlaySound), [typeof(string), typeof(Vector2), typeof(float), typeof(GameWorld)])]
    private static bool SoundHandler_PlaySound_Prefix_SoundPanning(string soundID, Vector2 worldPosition, float volumeModifier, GameWorld gameWorld)
    {
        if (gameWorld == null || gameWorld.InLoading || gameWorld.MuteSounds)
        {
            return false;
        }

        if (gameWorld.GameOwner != GameOwnerEnum.Server)
        {
            Panning.PlayGlobalPannedSound(soundID, gameWorld, worldPosition, volumeModifier);
            return false;
        }

        if (SoundHandler.game.Server == null)
        {
            throw new Exception("Error: SoundHandler.PlaySound() parameter isServer is true even though the server doesn't exist");
        }

        SoundHandler.game.Server.SendMessage(MessageType.Sound, new NetMessage.Sound.Data(soundID, false, Converter.WorldToBox2D(worldPosition), volumeModifier));
        return false;
    }

    public static void PlayGlobalPannedSound(string soundID, GameWorld gameWorld, Vector2 worldPosition, float volumeModifier = 1f)
    {
        if (SoundHandler.m_soundsDisabled)
        {
            return;
        }

        SoundHandler.SoundEffectGroup soundEffectGroup = SoundHandler.soundEffects.Find(soundID);
        if (soundEffectGroup != null)
        {
            // Sound pitch
            float soundPitch = (gameWorld.SlowmotionHandler.SlowmotionModifier) - 1f; // -1f to 1f

            // Sound volume
            float soundVolumeModifier = 1f;

            // Sound Panning
            float soundPanning = 0f;
            if (worldPosition != Vector2.Zero)
            {
                if (SFDCTConfig.Get<bool>(CTSettingKey.SoundAttenuationEnabled))
                {
                    Vector2 listenerPos;
                    if (SFDCTConfig.Get<bool>(CTSettingKey.SoundAttenuationForceScreenSpace) ||
                        GameInfo.LocalPlayerCount >= 2 ||
                        gameWorld.PrimaryLocalPlayer == null ||
                        gameWorld.PrimaryLocalPlayer.IsDisposed ||
                        gameWorld.PrimaryLocalPlayer.IsRemoved ||
                        gameWorld.PrimaryLocalPlayer.IsDead
                    )
                    {
                        listenerPos = Camera.ConvertWorldToScreen(worldPosition);

                        float distanceFromCenter = (listenerPos - new Vector2(GameSFD.GAME_WIDTHf, GameSFD.GAME_HEIGHTf) * 0.5f).CalcSafeLength();
                        if (distanceFromCenter <= 0f)
                        {
                            soundVolumeModifier = 0f;
                        }
                        else
                        {
                            soundVolumeModifier = distanceFromCenter / MathHelper.Max(GameSFD.GAME_WIDTHf, GameSFD.GAME_HEIGHTf);
                        }
                    }
                    else
                    {
                        float worldThreshold = SFDCTConfig.Get<int>(CTSettingKey.SoundAttenuationInworldThreshold);
                        float worldDistance = SFDCTConfig.Get<int>(CTSettingKey.SoundAttenuationInworldDistance);
                        float dist = (worldPosition - gameWorld.PrimaryLocalPlayer.Position).CalcSafeLength();

                        if (dist >= worldThreshold)
                        {
                            soundVolumeModifier = (dist - worldThreshold) / worldDistance;
                        }
                        else
                        {
                            soundVolumeModifier = 0f;
                        }
                    }

                    soundVolumeModifier = MathHelper.Clamp(1 - soundVolumeModifier, SFDCTConfig.Get<float>(CTSettingKey.SoundAttenuationMin), 1f);
                }

                if (SFDCTConfig.Get<bool>(CTSettingKey.SoundPanningEnabled) && SFDCTConfig.Get<float>(CTSettingKey.SoundPanningStrength) != 0f)
                {
                    float listenerPosX;
                    // Use screen-space if told to, if playing locally,
                    // or the current player is dead/removed/null
                    if (SFDCTConfig.Get<bool>(CTSettingKey.SoundPanningForceScreenSpace) ||
                        GameInfo.LocalPlayerCount >= 2 ||
                        gameWorld.PrimaryLocalPlayer == null ||
                        gameWorld.PrimaryLocalPlayer.IsDisposed ||
                        gameWorld.PrimaryLocalPlayer.IsRemoved ||
                        gameWorld.PrimaryLocalPlayer.IsDead
                    )
                    {
                        listenerPosX = Camera.ConvertWorldToScreenX(worldPosition.X);
                        soundPanning = (listenerPosX - GameSFD.GAME_WIDTHf * 0.5f) / GameSFD.GAME_WIDTHf * 0.5f;
                    }
                    else
                    {
                        float worldThreshold = SFDCTConfig.Get<int>(CTSettingKey.SoundPanningInworldThreshold);
                        float worldDistance = SFDCTConfig.Get<int>(CTSettingKey.SoundPanningInworldDistance);
                        float distX = worldPosition.X - gameWorld.PrimaryLocalPlayer.Position.X;

                        // The side of the panning is decided by the X position difference.
                        // Negative is left. Positive is right.

                        // The distance from the player to the sound is over the threshold.
                        // If it's below, it will not have any panning.
                        if (Math.Abs(distX) >= worldThreshold)
                        {
                            soundPanning = (distX - worldThreshold) / worldDistance;
                        }
                    }
                    soundPanning = MathHelper.Clamp(soundPanning * SFDCTConfig.Get<float>(CTSettingKey.SoundPanningStrength), -1f, 1f);
                }
            }

            // Play the sound
            SoundHandler.PlaySoundEffectGroup(soundEffectGroup, soundEffectGroup.VolumeModifier * volumeModifier * soundVolumeModifier, soundPitch, soundPanning);
            return;
        }

        if (soundID.Equals("none", StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(soundID))
        {
            return;
        }
        ConsoleOutput.ShowMessage(ConsoleOutputType.Warning, $"Sound '{soundID}' could not be found");
    }
}
