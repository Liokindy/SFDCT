//using System;
//using HarmonyLib;
//using Microsoft.Xna.Framework;
//using SFD;
//using SFD.Sounds;
//using CSettings = SFDCT.Settings.Values;

//namespace SFDCT.Game;

//[HarmonyPatch]
//internal static class SoundPatches
//{
//    internal static readonly Type typeof_soundHandler = typeof(SoundHandler);
//    internal static readonly string nameof_soundHandlerPlaySound = nameof(SoundHandler.PlaySound);
//    internal static readonly Type[] typeof_StringVector2Gameworld = new Type[]
//    {
//            typeof(string),
//            typeof(Vector2),
//            typeof(GameWorld)
//    };

//    /// <summary>
//    ///     Tweaks SoundHandler.PlaySound to use position
//    /// </summary>
//    [HarmonyPrefix]
//    [HarmonyPatch(typeof(SoundHandler), nameof(SoundHandler.PlaySound), new Type[] { typeof(string), typeof(Vector2), typeof(float), typeof(GameWorld) })]
//    private static bool PlaySound(string soundID, Vector2 worldPosition, float volumeModifier, GameWorld gameWorld)
//    {
//        if (gameWorld == null || gameWorld.InLoading || gameWorld.MuteSounds)
//        {
//            return false;
//        }
//        if (gameWorld.GameOwner != GameOwnerEnum.Server)
//        {
//            PlayGlobalPannedSound(soundID, gameWorld, worldPosition, volumeModifier);
            
//            return false;
//        }
//        if (SoundHandler.game.Server == null)
//        {
//            throw new Exception("Error: SoundHandler.PlaySound() parameter isServer is true even though the server doesn't exist");
//        }
//        SoundHandler.game.Server.SendMessage(MessageType.Sound, new NetMessage.Sound.Data(soundID, false, Converter.WorldToBox2D(worldPosition), volumeModifier));

//        return false;
//    }

//    /// <summary>
//    ///     Plays a screen-space panned sound. if Position
//    ///     equals WorldOrigin panning is disabled.
//    /// </summary>
//    private static void PlayGlobalPannedSound(string soundID, GameWorld gameWorld, Vector2 worldPosition, float volumeModifier = 1f)
//    {
//        if (SoundHandler.m_soundsDisabled)
//        {
//            return;
//        }

//        SoundHandler.SoundEffectGroup soundEffectGroup = SoundHandler.soundEffects.Find(soundID);
//        if (soundEffectGroup != null)
//        {
//            // Sound pitch
//            float soundPitch = (gameWorld.SlowmotionHandler.SlowmotionModifier) - 1f; // -1f to 1f

//            // Sound volume
//            float soundVolumeModifier = 1f;

//            // Sound Panning
//            float soundPanning = 0f;
//            if (worldPosition != Vector2.Zero)
//            {
//                if (CSettings.Get<bool>(CSettings.GetKey(CSettings.SettingKey.SoundAttenuationEnabled)))
//                {
//                    Vector2 listenerPos;
//                    if (CSettings.Get<bool>(CSettings.GetKey(CSettings.SettingKey.SoundAttenuationForceScreenSpace)) ||
//                        GameInfo.LocalPlayerCount >= 2 ||
//                        gameWorld.PrimaryLocalPlayer == null ||
//                        gameWorld.PrimaryLocalPlayer.IsDisposed ||
//                        gameWorld.PrimaryLocalPlayer.IsRemoved ||
//                        gameWorld.PrimaryLocalPlayer.IsDead
//                    )
//                    {
//                        listenerPos = Camera.ConvertWorldToScreen(worldPosition);

//                        float distanceFromCenter = (listenerPos - new Vector2(GameSFD.GAME_WIDTHf, GameSFD.GAME_HEIGHTf) * 0.5f).CalcSafeLength();
//                        if (distanceFromCenter <= 0f)
//                        {
//                            soundVolumeModifier = 0f;
//                        }
//                        else
//                        {
//                            soundVolumeModifier = distanceFromCenter / MathHelper.Max(GameSFD.GAME_WIDTHf, GameSFD.GAME_HEIGHTf);
//                        }
//                    }
//                    else
//                    {
//                        float worldThreshold = CSettings.Get<int>(CSettings.GetKey(CSettings.SettingKey.SoundAttenuationInworldThreshold));
//                        float worldDistance = CSettings.Get<int>(CSettings.GetKey(CSettings.SettingKey.SoundAttenuationInworldDistance));
//                        float dist = (worldPosition - gameWorld.PrimaryLocalPlayer.Position).CalcSafeLength();

//                        if (dist >= worldThreshold)
//                        {
//                            soundVolumeModifier = (dist - worldThreshold) / worldDistance;
//                        }
//                        else
//                        {
//                            soundVolumeModifier = 0f;
//                        }
//                    }

//                    soundVolumeModifier = MathHelper.Clamp(1 - soundVolumeModifier, CSettings.Get<float>(CSettings.GetKey(CSettings.SettingKey.SoundAttenuationMin)), 1f);
//                }

//                if (CSettings.Get<bool>(CSettings.GetKey(CSettings.SettingKey.SoundPanningEnabled)) && CSettings.Get<float>(CSettings.GetKey(CSettings.SettingKey.SoundPanningStrength)) != 0f)
//                {
//                    float listenerPosX;
//                    // Use screen-space if told to, if playing locally,
//                    // or the current player is dead/removed/null
//                    if (CSettings.Get<bool>(CSettings.GetKey(CSettings.SettingKey.SoundPanningForceScreenSpace)) ||
//                        GameInfo.LocalPlayerCount >= 2 ||
//                        gameWorld.PrimaryLocalPlayer == null ||
//                        gameWorld.PrimaryLocalPlayer.IsDisposed ||
//                        gameWorld.PrimaryLocalPlayer.IsRemoved || 
//                        gameWorld.PrimaryLocalPlayer.IsDead
//                    )
//                    {
//                        listenerPosX = Camera.ConvertWorldToScreenX(worldPosition.X);
//                        soundPanning = (listenerPosX - GameSFD.GAME_WIDTHf * 0.5f) / GameSFD.GAME_WIDTHf * 0.5f;
//                    }
//                    else
//                    {
//                        float worldThreshold = CSettings.Get<int>(CSettings.GetKey(CSettings.SettingKey.SoundPanningInworldThreshold));
//                        float worldDistance = CSettings.Get<int>(CSettings.GetKey(CSettings.SettingKey.SoundPanningInworldDistance));
//                        float distX = worldPosition.X - gameWorld.PrimaryLocalPlayer.Position.X;

//                        // The side of the panning is decided by the X position difference.
//                        // Negative is left. Positive is right.
                    
//                        // The distance from the player to the sound is over the threshold.
//                        // If it's below, it will not have any panning.
//                        if (Math.Abs(distX) >= worldThreshold)
//                        {
//                            soundPanning = (distX - worldThreshold) / worldDistance;
//                        }
//                    }
//                    soundPanning = MathHelper.Clamp(soundPanning * CSettings.Get<float>(CSettings.GetKey(CSettings.SettingKey.SoundPanningStrength)), -1f, 1f);
//                }
//            }
            
//            // Play the sound
//            SoundHandler.PlaySoundEffectGroup(soundEffectGroup, soundEffectGroup.VolumeModifier * volumeModifier * soundVolumeModifier, soundPitch, soundPanning);
//            return;
//        }

//        if (soundID.Equals("NONE", StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(soundID))
//        {
//            return;
//        }
//        ConsoleOutput.ShowMessage(ConsoleOutputType.Warning, "Sound '" + soundID + "' could not be found");
//    }
//}