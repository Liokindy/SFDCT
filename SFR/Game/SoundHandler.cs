using System;
using HarmonyLib;
using Microsoft.Xna.Framework;
using SFD;
using Constants = SFR.Misc.Constants;

namespace SFR.Game;

[HarmonyPatch]
internal static class SoundHandler
{
    internal static readonly Type typeof_soundHandler = typeof(SFD.Sounds.SoundHandler);
    internal static readonly string nameof_soundHandlerPlaySound = nameof(SFD.Sounds.SoundHandler.PlaySound);
    internal static readonly Type[] typeof_StringVector2Gameworld = new Type[]
    {
            typeof(string),
            typeof(Microsoft.Xna.Framework.Vector2),
            typeof(GameWorld)
    };

    /// <summary>
    ///     Tweaks SoundHandler.PlaySound to use position
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(SFD.Sounds.SoundHandler), nameof(SFD.Sounds.SoundHandler.PlaySound), new Type[] { typeof(string), typeof(Vector2), typeof(float), typeof(GameWorld) })]
    private static bool PlaySound(string soundID, Vector2 worldPosition, float volumeModifier, GameWorld gameWorld)
    {
        if (gameWorld == null || gameWorld.InLoading || gameWorld.MuteSounds)
        {
            return false;
        }
        if (gameWorld.GameOwner != GameOwnerEnum.Server)
        {
            PlayGlobalPannedSound(soundID, gameWorld, worldPosition, volumeModifier);
            
            return false;
        }
        if (SFD.Sounds.SoundHandler.game.Server == null)
        {
            throw new Exception("Error: SoundHandler.PlaySound() parameter isServer is true even though the server doesn't exist");
        }
        SFD.Sounds.SoundHandler.game.Server.SendMessage(MessageType.Sound, new NetMessage.Sound.Data(soundID, false, Converter.WorldToBox2D(worldPosition), volumeModifier));

        return false;
    }

    /// <summary>
    ///     Plays a screen-space panned sound. if Position
    ///     equals WorldOrigin panning is disabled.
    /// </summary>
    private static void PlayGlobalPannedSound(string soundID, GameWorld gameWorld, Vector2 worldPosition, float volumeModifier = 1f)
    {
        if (SFD.Sounds.SoundHandler.m_soundsDisabled)
        {
            return;
        }
        SFD.Sounds.SoundHandler.SoundEffectGroup soundEffectGroup = SFD.Sounds.SoundHandler.soundEffects.Find(soundID);
        if (soundEffectGroup != null)
        {
            // Sound pitch
            float soundPitch = (gameWorld.SlowmotionHandler.SlowmotionModifier) - 1f; // -1f to 1f

            // Sound Panning
            float soundPanning = 0f;
            if (worldPosition != Vector2.Zero && Constants.SoundPanning_Strength > 0f)
            {
                float listenerPosX;
                if (Constants.SoundPanning_IsScreenSpace || (gameWorld.PrimaryLocalPlayer == null || gameWorld.PrimaryLocalPlayer.IsRemoved))
                {
                    listenerPosX = Camera.ConvertWorldToScreenX(worldPosition.X);
                    soundPanning = (listenerPosX-GameSFD.GAME_WIDTHf*0.5f) / GameSFD.GAME_WIDTHf*0.5f;
                }
                else
                {
                    listenerPosX = gameWorld.PrimaryLocalPlayer.Position.X;
                    float soundPosXDiff = worldPosition.X-listenerPosX;
                    if (Math.Abs(soundPosXDiff) >= Constants.SoundPanning_InWorld_Threshold)
                    {
                        if (soundPosXDiff > 0)
                        {
                            soundPosXDiff -= Constants.SoundPanning_InWorld_Threshold;
                        }
                        else
                        {
                            soundPosXDiff += Constants.SoundPanning_InWorld_Threshold;
                        }
                        soundPanning = soundPosXDiff / Constants.SoundPanning_InWorld_Distance;
                    }
                }
                soundPanning = MathHelper.Clamp(soundPanning * Constants.SoundPanning_Strength, -1f, 1f);
            }
            
            // Play the sound
            SFD.Sounds.SoundHandler.PlaySoundEffectGroup(soundEffectGroup, soundEffectGroup.VolumeModifier * volumeModifier, soundPitch, soundPanning);
            return;
        }
        if (soundID == "NONE")
        {
            return;
        }
        ConsoleOutput.ShowMessage(ConsoleOutputType.Warning, "Sound '" + soundID + "' could not be found");
    }
}