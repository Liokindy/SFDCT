namespace SFDCT.Configuration;

internal enum CTSettingKey
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
    LowHealthHurtLevel1Threshold,
    LowHealthHurtLevel2Threshold,

    HideFilmgrain,
    DisableClockTicking,
    Language,

    FightersCanAlwaysRecoveryRoll,
    FightersAlwaysRecoveryKneel,

    SpectatorsMaximum,
    SpectatorsOnlyModerators,

    VoteKickEnabled,
    VoteKickSuccessCooldown,
    VoteKickFailCooldown,
}
