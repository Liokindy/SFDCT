using SFDCT.Helper;
using System;

namespace SFDCT.Configuration;

public static partial class Settings
{
    public enum SettingKey
    {
        SoundPanningEnabled,
        SoundPanningStrength,
        SoundPanningForceScreenSpace,
        SoundPanningInworldThreshold,
        SoundPanningInworldDistance,
        SoundAttenuationEnabled,
        SoundAttenuationMin,
        SoundAttenuationForceScreenSpace,
        SoundAttenuationInworldThreshold,
        SoundAttenuationInworldDistance,
        LowHealthSaturationFactor,
        LowHealthThreshold,
        HideFilmgrain,
    }

    public static string GetKey(SettingKey key)
    {
        switch (key)
        {
            default:
                string mess = $"CONFIG.INI: Fail at GetKey, SettingKey '{key}' does not have a key!";
                Logger.LogError(mess);
                throw new Exception(mess);
            case SettingKey.SoundPanningEnabled: return "SOUNDPANNING_ENABLED";
            case SettingKey.SoundPanningStrength: return "SOUNDPANNING_STRENGTH";
            case SettingKey.SoundPanningForceScreenSpace: return "SOUNDPANNING_FORCE_SCREEN_SPACE";
            case SettingKey.SoundPanningInworldThreshold: return "SOUNDPANNING_INWORLD_THRESHOLD";
            case SettingKey.SoundPanningInworldDistance: return "SOUNDPANNING_INWORLD_DISTANCE";
            case SettingKey.SoundAttenuationEnabled: return "SOUNDATTENUATION_ENABLED";
            case SettingKey.SoundAttenuationMin: return "SOUNDATTENUATION_MIN";
            case SettingKey.SoundAttenuationForceScreenSpace: return "SOUNDATTENUATION_FORCE_SCREEN_SPACE";
            case SettingKey.SoundAttenuationInworldThreshold: return "SOUNDATTENUATION_INWORLD_THRESHOLD";
            case SettingKey.SoundAttenuationInworldDistance: return "SOUNDATTENUATION_INWORLD_DISTANCE"; ;
            case SettingKey.LowHealthSaturationFactor: return "LOW_HEALTH_SATURATION_FACTOR";
            case SettingKey.LowHealthThreshold: return "LOW_HEALTH_THRESHOLD";
            case SettingKey.HideFilmgrain: return "HIDE_FILMGRAIN";
        }
    }
}