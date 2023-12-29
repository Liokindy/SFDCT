using System;
using System.Collections.Generic;
using System.Linq;
using SFD;
using HarmonyLib;
using Microsoft.Xna.Framework;

namespace SFR.Game;

[HarmonyPatch]
internal static class SoundHandler
{
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
            float soundPitch = 0f;
            if (gameWorld.SlowmotionHandler.SlowmotionModifier != 1)
            {
                soundPitch = Microsoft.Xna.Framework.MathHelper.Lerp(-1f, 0f, gameWorld.SlowmotionHandler.SlowmotionModifier);
            }

            SFR.Game.SoundHandler.PlayGlobalPannedSound(soundID, worldPosition, volumeModifier, soundPitch);
            
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
    ///     Plays a panned sound depending on ScreenPosition.
    /// </summary>
    private static void PlayGlobalPannedSound(string soundID, Vector2 worldPosition, float volumeModifier = 1f, float soundPitch = 0)
    {
        if (SFD.Sounds.SoundHandler.m_soundsDisabled)
        {
            return;
        }
        SFD.Sounds.SoundHandler.SoundEffectGroup soundEffectGroup = SFD.Sounds.SoundHandler.soundEffects.Find(soundID);
        if (soundEffectGroup != null)
        {
            // Panning
            float worldCamPosX = Camera.ConvertWorldToScreenX(worldPosition.X);
            float soundPanning = 0f;
            if (worldPosition != Vector2.Zero)
            {
                soundPanning = (worldCamPosX-GameSFD.GAME_WIDTHf*0.5f) / GameSFD.GAME_WIDTHf * 0.5f;
                soundPanning = MathHelper.Clamp(soundPanning * 4.00f, -1f, 1f);
            }

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