using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SFD;
using SFD.MenuControls;
using SFDCT.Configuration;
using SFDCT.UI.MenuItems;
using System;
using System.Collections.Generic;

namespace SFDCT.UI.Panels;

internal class SFDCTSettingsPanel : Panel
{
    private Menu m_menu;
    private MenuItemDropdown m_menuItemSoundPanningEnabled;
    private MenuItemSlider m_menuItemSoundPanningStrength;
    private MenuItemDropdown m_menuItemSoundPanningForceScreenSpace;
    private MenuItemSlider m_menuItemSoundPanningInworldThreshold;
    private MenuItemSlider m_menuItemSoundPanningInworldDistance;
    private MenuItemDropdown m_menuItemSoundAttenuationEnabled;
    private MenuItemSlider m_menuItemSoundAttenuationMin;
    private MenuItemDropdown m_menuItemSoundAttenuationForceScreenSpace;
    private MenuItemSlider m_menuItemSoundAttenuationInworldThreshold;
    private MenuItemSlider m_menuItemSoundAttenuationInworldDistance;
    private MenuItemSlider m_menuItemLowHealthSaturationFactor;
    private MenuItemSlider m_menuItemLowHealthThreshold;
    private MenuItemSlider m_menuItemLowHealthHurtLevel1Threshold;
    private MenuItemSlider m_menuItemLowHealthHurtLevel2Threshold;
    private MenuItemDropdown m_menuItemHideFilmgrain;
    private MenuItemDropdown m_menuItemLanguage;
    private MenuItemSlider m_menuItemSpectatorsMaximum;
    private MenuItemDropdown m_menuItemSpectatorsOnlyModerators;
    private MenuItemDropdown m_menuItemVoteKickEnabled;
    private MenuItemSlider m_menuItemVoteKickFailCooldown;
    private MenuItemSlider m_menuItemVoteKickSuccessCooldown;
    private SFDCTMenuItemDropdownColor m_menuItemPrimaryColor;
    private MenuItemText m_menuItemPrimaryColorHex;
    private MenuItemButton m_menuItemSubContentFolders;
    private MenuItemDropdown m_menuItemSubContentEnabled;
    private bool m_settingsNeedGameRestart;

    private bool m_originalSoundPanningEnabled = SFDCTConfig.Get<bool>(CTSettingKey.SoundPanningEnabled);
    private float m_originalSoundPanningStrength = SFDCTConfig.Get<float>(CTSettingKey.SoundPanningStrength);
    private bool m_originalSoundPanningForceScreenSpace = SFDCTConfig.Get<bool>(CTSettingKey.SoundPanningForceScreenSpace);
    private int m_originalSoundPanningInworldThreshold = SFDCTConfig.Get<int>(CTSettingKey.SoundPanningInworldThreshold);
    private int m_originalSoundPanningInworldDistance = SFDCTConfig.Get<int>(CTSettingKey.SoundPanningInworldDistance);
    private bool m_originalSoundAttenuationEnabled = SFDCTConfig.Get<bool>(CTSettingKey.SoundAttenuationEnabled);
    private float m_originalSoundAttenuationMin = SFDCTConfig.Get<float>(CTSettingKey.SoundAttenuationMin);
    private bool m_originalSoundAttenuationForceScreenSpace = SFDCTConfig.Get<bool>(CTSettingKey.SoundAttenuationForceScreenSpace);
    private int m_originalSoundAttenuationInworldThreshold = SFDCTConfig.Get<int>(CTSettingKey.SoundAttenuationInworldThreshold);
    private int m_originalSoundAttenuationInworldDistance = SFDCTConfig.Get<int>(CTSettingKey.SoundAttenuationInworldDistance);
    private float m_originalLowHealthSaturationFactor = SFDCTConfig.Get<float>(CTSettingKey.LowHealthSaturationFactor);
    private float m_originalLowHealthThreshold = SFDCTConfig.Get<float>(CTSettingKey.LowHealthThreshold);
    private float m_originalLowHealthHurtLevel1Threshold = SFDCTConfig.Get<float>(CTSettingKey.LowHealthHurtLevel1Threshold);
    private float m_originalLowHealthHurtLevel2Threshold = SFDCTConfig.Get<float>(CTSettingKey.LowHealthHurtLevel2Threshold);
    private bool m_originalHideFilmgrain = SFDCTConfig.Get<bool>(CTSettingKey.HideFilmgrain);
    private string m_originalLanguage = SFDCTConfig.Get<string>(CTSettingKey.Language);
    private int m_originalSpectatorsMaximum = SFDCTConfig.Get<int>(CTSettingKey.SpectatorsMaximum);
    private bool m_originalSpectatorsOnlyModerators = SFDCTConfig.Get<bool>(CTSettingKey.SpectatorsOnlyModerators);
    private bool m_originalVoteKickEnabled = SFDCTConfig.Get<bool>(CTSettingKey.VoteKickEnabled);
    private int m_originalVoteKickFailCooldown = SFDCTConfig.Get<int>(CTSettingKey.VoteKickFailCooldown);
    private int m_originalVoteKickSuccessCooldown = SFDCTConfig.Get<int>(CTSettingKey.VoteKickSuccessCooldown);
    private bool m_originalSubContent = SFDCTConfig.Get<bool>(CTSettingKey.SubContent);
    private string m_originalSubContentDisabledFolders = SFDCTConfig.Get<string>(CTSettingKey.SubContentDisabledFolders);
    private string m_originalSubContentEnabledFolders = SFDCTConfig.Get<string>(CTSettingKey.SubContentEnabledFolders);
    private string m_originalPrimaryColorHex = Constants.COLORS.MENU_BLUE.ToHex();

    public SFDCTSettingsPanel() : base(LanguageHelper.GetText("sfdct.setting.header"), 500, 500)
    {
        m_menu = new Menu(new Vector2(0, 50), Width, Height - 50, this, []);
        m_settingsNeedGameRestart = false;

        m_menu.Add(new MenuItemButton(LanguageHelper.GetText("sfdct.credits.name"), new ControlEvents.ChooseEvent((object _) =>
        {
            OpenSubPanel(new SFDCTCreditsPanel());
        })));

        m_menu.Add(new MenuItemSeparator(LanguageHelper.GetText("sfdct.setting.category.soundpanning")));

        m_menuItemSoundPanningEnabled = new(LanguageHelper.GetText("sfdct.setting.name.soundpanningenabled"), [LanguageHelper.GetText("general.on"), LanguageHelper.GetText("general.off")]);
        m_menuItemSoundPanningEnabled.SetStartValue(SFDCTConfig.Get<bool>(CTSettingKey.SoundPanningEnabled) ? 0 : 1);
        m_menuItemSoundPanningEnabled.DropdownItemVisibleCount = 2;
        m_menuItemSoundPanningEnabled.Tooltip = LanguageHelper.GetText("sfdct.setting.help.soundpanningenabled");
        EventHelper.Add(m_menuItemSoundPanningEnabled, "ValueChangedEvent", new MenuItemValueChangedEvent((MenuItem _) =>
        {
            SFDCTConfig.Set(CTSettingKey.SoundPanningEnabled, m_menuItemSoundPanningEnabled.ValueId == 0);
        }));
        m_menu.Add(m_menuItemSoundPanningEnabled);

        m_menuItemSoundPanningStrength = new(LanguageHelper.GetText("sfdct.setting.name.soundpanningstrength"), (int)(100 * SFDCTConfig.Get<float>(CTSettingKey.SoundPanningStrength)), 0, 100, 1);
        m_menuItemSoundPanningStrength.SetStartValue((int)(SFDCTConfig.Get<float>(CTSettingKey.SoundPanningStrength) * 100));
        m_menuItemSoundPanningStrength.Tooltip = LanguageHelper.GetText("sfdct.setting.help.soundpanningstrength");
        EventHelper.Add(m_menuItemSoundPanningStrength, "ValueChangedEvent", new MenuItemValueChangedEvent((MenuItem _) =>
        {
            SFDCTConfig.Set<float>(CTSettingKey.SoundPanningStrength, m_menuItemSoundPanningStrength.Value * 0.01f);
        }));
        m_menu.Add(m_menuItemSoundPanningStrength);

        m_menuItemSoundPanningForceScreenSpace = new(LanguageHelper.GetText("sfdct.setting.name.soundpanningforcescreenspace"), [LanguageHelper.GetText("general.on"), LanguageHelper.GetText("general.off")]);
        m_menuItemSoundPanningForceScreenSpace.SetStartValue(SFDCTConfig.Get<bool>(CTSettingKey.SoundPanningForceScreenSpace) ? 0 : 1);
        m_menuItemSoundPanningForceScreenSpace.DropdownItemVisibleCount = 2;
        m_menuItemSoundPanningForceScreenSpace.Tooltip = LanguageHelper.GetText("sfdct.setting.help.soundpanningforcescreenspace");
        EventHelper.Add(m_menuItemSoundPanningForceScreenSpace, "ValueChangedEvent", new MenuItemValueChangedEvent((MenuItem _) =>
        {
            SFDCTConfig.Set<bool>(CTSettingKey.SoundPanningForceScreenSpace, m_menuItemSoundPanningForceScreenSpace.ValueId == 0);
        }));
        m_menu.Add(m_menuItemSoundPanningForceScreenSpace);

        m_menuItemSoundPanningInworldThreshold = new(LanguageHelper.GetText("sfdct.setting.name.soundpanninginworldthreshold"), SFDCTConfig.Get<int>(CTSettingKey.SoundPanningInworldThreshold), 0, 1000, 5);
        m_menuItemSoundPanningInworldThreshold.SetStartValue(SFDCTConfig.Get<int>(CTSettingKey.SoundPanningInworldThreshold));
        m_menuItemSoundPanningInworldThreshold.Tooltip = LanguageHelper.GetText("sfdct.setting.help.soundpanninginworldthreshold");
        EventHelper.Add(m_menuItemSoundPanningInworldThreshold, "ValueChangedEvent", new MenuItemValueChangedEvent((MenuItem _) =>
        {
            SFDCTConfig.Set<int>(CTSettingKey.SoundPanningInworldThreshold, m_menuItemSoundPanningInworldThreshold.Value);
        }));
        m_menu.Add(m_menuItemSoundPanningInworldThreshold);

        m_menuItemSoundPanningInworldDistance = new(LanguageHelper.GetText("sfdct.setting.name.soundpanninginworlddistance"), SFDCTConfig.Get<int>(CTSettingKey.SoundPanningInworldDistance), 0, 1000, 5);
        m_menuItemSoundPanningInworldDistance.SetStartValue(SFDCTConfig.Get<int>(CTSettingKey.SoundPanningInworldDistance));
        m_menuItemSoundPanningInworldDistance.Tooltip = LanguageHelper.GetText("sfdct.setting.help.soundpanninginworlddistance");
        EventHelper.Add(m_menuItemSoundPanningInworldDistance, "ValueChangedEvent", new MenuItemValueChangedEvent((MenuItem _) =>
        {
            SFDCTConfig.Set<int>(CTSettingKey.SoundPanningInworldDistance, m_menuItemSoundPanningInworldDistance.Value);
        }));
        m_menu.Add(m_menuItemSoundPanningInworldDistance);

        m_menu.Add(new MenuItemSeparator(LanguageHelper.GetText("sfdct.setting.category.soundattenuation")));

        m_menuItemSoundAttenuationEnabled = new(LanguageHelper.GetText("sfdct.setting.name.soundattenuationenabled"), [LanguageHelper.GetText("general.on"), LanguageHelper.GetText("general.off")]);
        m_menuItemSoundAttenuationEnabled.SetStartValue(SFDCTConfig.Get<bool>(CTSettingKey.SoundAttenuationEnabled) ? 0 : 1);
        m_menuItemSoundAttenuationEnabled.DropdownItemVisibleCount = 2;
        m_menuItemSoundAttenuationEnabled.Tooltip = LanguageHelper.GetText("sfdct.setting.help.soundattenuationenabled");
        EventHelper.Add(m_menuItemSoundAttenuationEnabled, "ValueChangedEvent", new MenuItemValueChangedEvent((MenuItem _) =>
        {
            SFDCTConfig.Set<bool>(CTSettingKey.SoundAttenuationEnabled, m_menuItemSoundAttenuationEnabled.ValueId == 0);
        }));
        m_menu.Add(m_menuItemSoundAttenuationEnabled);

        m_menuItemSoundAttenuationMin = new(LanguageHelper.GetText("sfdct.setting.name.soundattenuationmin"), (int)(100 * SFDCTConfig.Get<float>(CTSettingKey.SoundAttenuationMin)), 0, 100, 1);
        m_menuItemSoundAttenuationMin.SetStartValue((int)(100 * SFDCTConfig.Get<float>(CTSettingKey.SoundAttenuationMin)));
        m_menuItemSoundAttenuationMin.Tooltip = LanguageHelper.GetText("sfdct.setting.help.soundattenuationmin");
        EventHelper.Add(m_menuItemSoundAttenuationMin, "ValueChangedEvent", new MenuItemValueChangedEvent((MenuItem _) =>
        {
            SFDCTConfig.Set<float>(CTSettingKey.SoundAttenuationMin, m_menuItemSoundAttenuationMin.Value * 0.01f);
        }));
        m_menu.Add(m_menuItemSoundAttenuationMin);

        m_menuItemSoundAttenuationForceScreenSpace = new(LanguageHelper.GetText("sfdct.setting.name.soundattenuationforcescreenspace"), [LanguageHelper.GetText("general.on"), LanguageHelper.GetText("general.off")]);
        m_menuItemSoundAttenuationForceScreenSpace.SetStartValue(SFDCTConfig.Get<bool>(CTSettingKey.SoundAttenuationForceScreenSpace) ? 0 : 1);
        m_menuItemSoundAttenuationForceScreenSpace.DropdownItemVisibleCount = 2;
        m_menuItemSoundAttenuationForceScreenSpace.Tooltip = LanguageHelper.GetText("sfdct.setting.help.soundattenuationforcescreenspace");
        EventHelper.Add(m_menuItemSoundAttenuationForceScreenSpace, "ValueChangedEvent", new MenuItemValueChangedEvent((MenuItem _) =>
        {
            SFDCTConfig.Set<bool>(CTSettingKey.SoundAttenuationForceScreenSpace, m_menuItemSoundAttenuationForceScreenSpace.ValueId == 0);
        }));
        m_menu.Add(m_menuItemSoundAttenuationForceScreenSpace);

        m_menuItemSoundAttenuationInworldThreshold = new(LanguageHelper.GetText("sfdct.setting.name.soundattenuationinworldthreshold"), SFDCTConfig.Get<int>(CTSettingKey.SoundAttenuationInworldThreshold), 0, 1000, 5);
        m_menuItemSoundAttenuationInworldThreshold.SetStartValue(SFDCTConfig.Get<int>(CTSettingKey.SoundAttenuationInworldThreshold));
        m_menuItemSoundAttenuationInworldThreshold.Tooltip = LanguageHelper.GetText("sfdct.setting.help.soundattenuationinworldthreshold");
        EventHelper.Add(m_menuItemSoundAttenuationInworldThreshold, "ValueChangedEvent", new MenuItemValueChangedEvent((MenuItem _) =>
        {
            SFDCTConfig.Set<int>(CTSettingKey.SoundAttenuationInworldThreshold, m_menuItemSoundAttenuationInworldThreshold.Value);
        }));
        m_menu.Add(m_menuItemSoundAttenuationInworldThreshold);


        m_menuItemSoundAttenuationInworldDistance = new(LanguageHelper.GetText("sfdct.setting.name.soundattenuationinworlddistance"), SFDCTConfig.Get<int>(CTSettingKey.SoundAttenuationInworldDistance), 0, 1000, 5);
        m_menuItemSoundAttenuationInworldDistance.SetStartValue(SFDCTConfig.Get<int>(CTSettingKey.SoundAttenuationInworldDistance));
        m_menuItemSoundAttenuationInworldDistance.Tooltip = LanguageHelper.GetText("sfdct.setting.help.soundattenuationinworlddistance");
        EventHelper.Add(m_menuItemSoundAttenuationInworldDistance, "ValueChangedEvent", new MenuItemValueChangedEvent((MenuItem _) =>
        {
            m_settingsNeedGameRestart = true;
            SFDCTConfig.Set<int>(CTSettingKey.SoundAttenuationInworldDistance, m_menuItemSoundAttenuationInworldDistance.Value);
        }));
        m_menu.Add(m_menuItemSoundAttenuationInworldDistance);

        m_menu.Add(new MenuItemSeparator(LanguageHelper.GetText("sfdct.setting.category.lowhealth")));

        m_menuItemLowHealthSaturationFactor = new(LanguageHelper.GetText("sfdct.setting.name.lowhealthsaturationfactor"), (int)(100 * SFDCTConfig.Get<float>(CTSettingKey.LowHealthSaturationFactor)), 0, 100, 1);
        m_menuItemLowHealthSaturationFactor.SetStartValue((int)(100 * SFDCTConfig.Get<float>(CTSettingKey.LowHealthSaturationFactor)));
        m_menuItemLowHealthSaturationFactor.Tooltip = LanguageHelper.GetText("sfdct.setting.help.lowhealthsaturationfactor");
        EventHelper.Add(m_menuItemLowHealthSaturationFactor, "ValueChangedEvent", new MenuItemValueChangedEvent((MenuItem _) =>
        {
            m_settingsNeedGameRestart = true;
            SFDCTConfig.Set<float>(CTSettingKey.LowHealthSaturationFactor, m_menuItemLowHealthSaturationFactor.Value * 0.01f);
        }));
        m_menu.Add(m_menuItemLowHealthSaturationFactor);

        m_menuItemLowHealthThreshold = new(LanguageHelper.GetText("sfdct.setting.name.lowhealththreshold"), (int)(100 * SFDCTConfig.Get<float>(CTSettingKey.LowHealthThreshold)), 0, 100, 1);
        m_menuItemLowHealthThreshold.SetStartValue((int)(100 * SFDCTConfig.Get<float>(CTSettingKey.LowHealthThreshold)));
        m_menuItemLowHealthThreshold.Tooltip = LanguageHelper.GetText("sfdct.setting.help.lowhealththreshold");
        EventHelper.Add(m_menuItemLowHealthThreshold, "ValueChangedEvent", new MenuItemValueChangedEvent((MenuItem _) =>
        {
            m_settingsNeedGameRestart = true;
            SFDCTConfig.Set<float>(CTSettingKey.LowHealthThreshold, m_menuItemLowHealthThreshold.Value * 0.01f);
        }));
        m_menu.Add(m_menuItemLowHealthThreshold);

        m_menuItemLowHealthHurtLevel1Threshold = new(LanguageHelper.GetText("sfdct.setting.name.lowhealthhurtlevel1threshold"), (int)(100 * SFDCTConfig.Get<float>(CTSettingKey.LowHealthHurtLevel1Threshold)), 0, 100, 1);
        m_menuItemLowHealthHurtLevel1Threshold.SetStartValue((int)(100 * SFDCTConfig.Get<float>(CTSettingKey.LowHealthHurtLevel1Threshold)));
        m_menuItemLowHealthHurtLevel1Threshold.Tooltip = LanguageHelper.GetText("sfdct.setting.help.lowhealthhurtlevel1threshold");
        EventHelper.Add(m_menuItemLowHealthHurtLevel1Threshold, "ValueChangedEvent", new MenuItemValueChangedEvent((MenuItem _) =>
        {
            m_settingsNeedGameRestart = true;
            SFDCTConfig.Set<float>(CTSettingKey.LowHealthHurtLevel1Threshold, m_menuItemLowHealthHurtLevel1Threshold.Value * 0.01f);
        }));
        m_menu.Add(m_menuItemLowHealthHurtLevel1Threshold);

        m_menuItemLowHealthHurtLevel2Threshold = new(LanguageHelper.GetText("sfdct.setting.name.lowhealthhurtlevel2threshold"), (int)(100 * SFDCTConfig.Get<float>(CTSettingKey.LowHealthHurtLevel2Threshold)), 0, 100, 1);
        m_menuItemLowHealthHurtLevel2Threshold.SetStartValue((int)(100 * SFDCTConfig.Get<float>(CTSettingKey.LowHealthHurtLevel2Threshold)));
        m_menuItemLowHealthHurtLevel2Threshold.Tooltip = LanguageHelper.GetText("sfdct.setting.help.lowhealthhurtlevel2threshold");
        EventHelper.Add(m_menuItemLowHealthHurtLevel2Threshold, "ValueChangedEvent", new MenuItemValueChangedEvent((MenuItem _) =>
        {
            m_settingsNeedGameRestart = true;
            SFDCTConfig.Set<float>(CTSettingKey.LowHealthHurtLevel2Threshold, m_menuItemLowHealthHurtLevel2Threshold.Value * 0.01f);
        }));
        m_menu.Add(m_menuItemLowHealthHurtLevel2Threshold);

        m_menu.Add(new MenuItemSeparator(LanguageHelper.GetText("sfdct.setting.category.spectators")));



        m_menuItemSpectatorsMaximum = new(LanguageHelper.GetText("sfdct.setting.name.spectatorsmaximum"), SFDCTConfig.Get<int>(CTSettingKey.SpectatorsMaximum), 1, 8, 1);
        m_menuItemSpectatorsMaximum.SetStartValue(SFDCTConfig.Get<int>(CTSettingKey.SpectatorsMaximum));
        m_menuItemSpectatorsMaximum.Tooltip = LanguageHelper.GetText("sfdct.setting.help.spectatorsmaximum");
        EventHelper.Add(m_menuItemSpectatorsMaximum, "ValueChangedEvent", new MenuItemValueChangedEvent((MenuItem _) =>
        {
            SFDCTConfig.Set<int>(CTSettingKey.SpectatorsMaximum, m_menuItemSpectatorsMaximum.Value);
        }));
        m_menu.Add(m_menuItemSpectatorsMaximum);

        m_menuItemSpectatorsOnlyModerators = new(LanguageHelper.GetText("sfdct.setting.name.spectatorsonlymoderators"), [LanguageHelper.GetText("general.on"), LanguageHelper.GetText("general.off")]);
        m_menuItemSpectatorsOnlyModerators.SetStartValue(SFDCTConfig.Get<bool>(CTSettingKey.SpectatorsOnlyModerators) ? 0 : 1);
        m_menuItemSpectatorsOnlyModerators.DropdownItemVisibleCount = 2;
        m_menuItemSpectatorsOnlyModerators.Tooltip = LanguageHelper.GetText("sfdct.setting.help.spectatorsonlymoderators");
        EventHelper.Add(m_menuItemSpectatorsOnlyModerators, "ValueChangedEvent", new MenuItemValueChangedEvent((MenuItem _) =>
        {
            SFDCTConfig.Set<bool>(CTSettingKey.SpectatorsOnlyModerators, m_menuItemSpectatorsOnlyModerators.ValueId == 0);
        }));
        m_menu.Add(m_menuItemSpectatorsOnlyModerators);

        m_menu.Add(new MenuItemSeparator(LanguageHelper.GetText("sfdct.setting.category.votekick")));

        m_menuItemVoteKickEnabled = new(LanguageHelper.GetText("sfdct.setting.name.votekickenabled"), [LanguageHelper.GetText("general.on"), LanguageHelper.GetText("general.off")]);
        m_menuItemVoteKickEnabled.SetStartValue(SFDCTConfig.Get<bool>(CTSettingKey.VoteKickEnabled) ? 0 : 1);
        m_menuItemVoteKickEnabled.DropdownItemVisibleCount = 2;
        m_menuItemVoteKickEnabled.Tooltip = LanguageHelper.GetText("sfdct.setting.help.votekickenabled");
        EventHelper.Add(m_menuItemVoteKickEnabled, "ValueChangedEvent", new MenuItemValueChangedEvent((MenuItem _) =>
        {
            SFDCTConfig.Set<bool>(CTSettingKey.VoteKickEnabled, m_menuItemVoteKickEnabled.ValueId == 0);
        }));
        m_menu.Add(m_menuItemVoteKickEnabled);

        m_menuItemVoteKickFailCooldown = new(LanguageHelper.GetText("sfdct.setting.name.votekickfailcooldown"), SFDCTConfig.Get<int>(CTSettingKey.VoteKickFailCooldown), 30, 300, 5);
        m_menuItemVoteKickFailCooldown.SetStartValue(SFDCTConfig.Get<int>(CTSettingKey.VoteKickFailCooldown));
        m_menuItemVoteKickFailCooldown.Tooltip = LanguageHelper.GetText("sfdct.setting.help.votekickfailcooldown");
        EventHelper.Add(m_menuItemVoteKickFailCooldown, "ValueChangedEvent", new MenuItemValueChangedEvent((MenuItem _) =>
        {
            SFDCTConfig.Set<int>(CTSettingKey.VoteKickFailCooldown, m_menuItemVoteKickFailCooldown.Value);
        }));
        m_menu.Add(m_menuItemVoteKickFailCooldown);

        m_menuItemVoteKickSuccessCooldown = new(LanguageHelper.GetText("sfdct.setting.name.votekicksuccesscooldown"), SFDCTConfig.Get<int>(CTSettingKey.VoteKickSuccessCooldown), 30, 300, 5);
        m_menuItemVoteKickSuccessCooldown.SetStartValue(SFDCTConfig.Get<int>(CTSettingKey.VoteKickSuccessCooldown));
        m_menuItemVoteKickSuccessCooldown.Tooltip = LanguageHelper.GetText("sfdct.setting.help.votekicksuccesscooldown");
        EventHelper.Add(m_menuItemVoteKickSuccessCooldown, "ValueChangedEvent", new MenuItemValueChangedEvent((MenuItem _) =>
        {
            SFDCTConfig.Set<int>(CTSettingKey.VoteKickSuccessCooldown, m_menuItemVoteKickSuccessCooldown.Value);
        }));
        m_menu.Add(m_menuItemVoteKickSuccessCooldown);

        m_menu.Add(new MenuItemSeparator(LanguageHelper.GetText("sfdct.setting.category.misc")));

        m_menuItemHideFilmgrain = new(LanguageHelper.GetText("sfdct.setting.name.hidefilmgrain"), [LanguageHelper.GetText("general.on"), LanguageHelper.GetText("general.off")]);
        m_menuItemHideFilmgrain.SetStartValue(SFDCTConfig.Get<bool>(CTSettingKey.HideFilmgrain) ? 0 : 1);
        m_menuItemHideFilmgrain.DropdownItemVisibleCount = 2;
        m_menuItemHideFilmgrain.Tooltip = LanguageHelper.GetText("sfdct.setting.help.hidefilmgrain");
        EventHelper.Add(m_menuItemHideFilmgrain, "ValueChangedEvent", new MenuItemValueChangedEvent((MenuItem _) =>
        {
            SFDCTConfig.Set<bool>(CTSettingKey.HideFilmgrain, m_menuItemHideFilmgrain.ValueId == 0);
        }));
        m_menu.Add(m_menuItemHideFilmgrain);

        var availableSFDCTLanguages = new List<string>();
        availableSFDCTLanguages.AddRange(LanguageFileTranslator.m_languageFileMappings.Keys);
        for (int i = availableSFDCTLanguages.Count - 1; i >= 0; i--)
        {
            string language = availableSFDCTLanguages[i];

            if (!language.StartsWith("SFDCT", StringComparison.OrdinalIgnoreCase))
            {
                availableSFDCTLanguages.RemoveAt(i);
            }
        }

        m_menuItemLanguage = new(LanguageHelper.GetText("sfdct.setting.name.language"), [.. availableSFDCTLanguages]);
        m_menuItemLanguage.SetStartValue(Math.Max(0, availableSFDCTLanguages.IndexOf(SFDCTConfig.Get<string>(CTSettingKey.Language))));
        m_menuItemLanguage.DropdownItemVisibleCount = availableSFDCTLanguages.Count;
        m_menuItemLanguage.Tooltip = LanguageHelper.GetText("sfdct.setting.help.language");
        EventHelper.Add(m_menuItemLanguage, "ValueChangedEvent", new MenuItemValueChangedEvent((MenuItem _) =>
        {
            m_settingsNeedGameRestart = true;
            SFDCTConfig.Set<string>(CTSettingKey.Language, m_menuItemLanguage.Value);
        }));
        m_menu.Add(m_menuItemLanguage);

        m_menu.Add(new MenuItemSeparator(LanguageHelper.GetText("sfdct.setting.category.subcontent")));

        m_menuItemSubContentEnabled = new(LanguageHelper.GetText("sfdct.setting.name.subcontentenabled"), [LanguageHelper.GetText("general.on"), LanguageHelper.GetText("general.off")]);
        m_menuItemSubContentEnabled.SetStartValue(SFDCTConfig.Get<bool>(CTSettingKey.SubContent) ? 0 : 1);
        m_menuItemSubContentEnabled.DropdownItemVisibleCount = 2;
        m_menuItemSubContentEnabled.Tooltip = LanguageHelper.GetText("sfdct.setting.help.subcontentenabled");
        EventHelper.Add(m_menuItemSubContentEnabled, "ValueChangedEvent", new MenuItemValueChangedEvent((MenuItem _) =>
        {
            m_settingsNeedGameRestart = true;
            SFDCTConfig.Set<bool>(CTSettingKey.SubContent, m_menuItemSubContentEnabled.ValueId == 0);
        }));
        m_menu.Add(m_menuItemSubContentEnabled);

        m_menuItemSubContentFolders = new(LanguageHelper.GetText("sfdct.setting.name.subcontentfolders"), (object _) =>
        {
            OpenSubPanel(new SFDCTSubContentPanel());
        }, MenuIcons.Settings);
        m_menu.Add(m_menuItemSubContentFolders);

        m_menu.Add(new MenuItemSeparator(LanguageHelper.GetText("sfdct.setting.category.primarycolor")));

        Color menuBlue = Constants.COLORS.MENU_BLUE;

        m_menuItemPrimaryColorHex = new(LanguageHelper.GetText("sfdct.setting.name.primarycolorhex"), menuBlue.ToHex());
        EventHelper.Add(m_menuItemPrimaryColorHex.TextSetValidationItem, "TextValidationEvent", new TextValidationEvent((string setText, TextValidationEventArgs _) =>
        {
            if (setText.IsHex())
            {
                Constants.COLORS.MENU_BLUE = setText.ToColor();
                if (m_menuItemPrimaryColor.Color != Constants.COLORS.MENU_BLUE) m_menuItemPrimaryColor.SetColor(Constants.COLORS.MENU_BLUE);
            }
            else
            {
                m_menuItemPrimaryColorHex.SetValue(Constants.COLORS.MENU_BLUE.ToHex());
            }
        }));
        m_menu.Add(m_menuItemPrimaryColorHex);

        Color[] presetColors =
        [
            new Color (064, 064, 064),
            new Color (232, 96, 96),
            new Color (180, 032, 000),
            new Color (192, 096, 000),
            new Color (208, 192, 000),
            new Color (016, 128, 000),
            new Color (008, 096, 096),
            new Color (48, 48, 192),
            new Color (160, 032, 160),
            new Color (096, 048, 032),

            new Color(32, 0, 192),
        ];

        m_menuItemPrimaryColor = new(LanguageHelper.GetText("sfdct.setting.name.primarycolor"), Constants.COLORS.MENU_BLUE, presetColors);
        m_menuItemPrimaryColor.ValueChangedEvent += (MenuItem _) =>
        {
            Constants.COLORS.MENU_BLUE = m_menuItemPrimaryColor.Color;
            if (m_menuItemPrimaryColorHex.Value != Constants.COLORS.MENU_BLUE.ToHex()) m_menuItemPrimaryColorHex.SetValue(Constants.COLORS.MENU_BLUE.ToHex());
        };
        m_menu.Add(m_menuItemPrimaryColor);

        m_menu.Add(new MenuItemSeparator(string.Empty));
        m_menu.Add(new MenuItemButton(LanguageHelper.GetText("button.done"), new ControlEvents.ChooseEvent(ok), MenuIcons.Ok));
        m_menu.Add(new MenuItemButton(LanguageHelper.GetText("button.back"), new ControlEvents.ChooseEvent(back), MenuIcons.Cancel));

        members.Add(m_menu);
        m_menu.SelectFirst();
    }

    public override void KeyPress(Keys key)
    {
        if (subPanel == null && key == Keys.Escape)
        {
            back(null);
            return;
        }

        base.KeyPress(key);
    }

    private void ok(object _)
    {
        if (m_settingsNeedGameRestart)
        {
            MessageStack.Show(LanguageHelper.GetText("menu.settings.restartrequiredmessage"), MessageStackType.Information);
        }

        SFDCTConfig.SaveFile();

        if (m_originalPrimaryColorHex != Constants.COLORS.MENU_BLUE.ToHex())
        {
            SFDConfig.SaveConfig(SFDConfigSaveMode.Settings);
        }

        ParentPanel.CloseSubPanel();
    }

    private void back(object _)
    {
        OpenSubPanel(new ConfirmYesNoPanel(LanguageHelper.GetText("menu.settings.confirmcancel"), LanguageHelper.GetText("general.yes"), LanguageHelper.GetText("general.no"), (object _) =>
        {
            SFDCTConfig.Set(CTSettingKey.SoundPanningEnabled, m_originalSoundPanningEnabled);
            SFDCTConfig.Set(CTSettingKey.SoundPanningStrength, m_originalSoundPanningStrength);
            SFDCTConfig.Set(CTSettingKey.SoundPanningForceScreenSpace, m_originalSoundPanningForceScreenSpace);
            SFDCTConfig.Set(CTSettingKey.SoundPanningInworldThreshold, m_originalSoundPanningInworldThreshold);
            SFDCTConfig.Set(CTSettingKey.SoundPanningInworldDistance, m_originalSoundPanningInworldDistance);
            SFDCTConfig.Set(CTSettingKey.SoundAttenuationEnabled, m_originalSoundAttenuationEnabled);
            SFDCTConfig.Set(CTSettingKey.SoundAttenuationMin, m_originalSoundAttenuationMin);
            SFDCTConfig.Set(CTSettingKey.SoundAttenuationForceScreenSpace, m_originalSoundAttenuationForceScreenSpace);
            SFDCTConfig.Set(CTSettingKey.SoundAttenuationInworldThreshold, m_originalSoundAttenuationInworldThreshold);
            SFDCTConfig.Set(CTSettingKey.SoundAttenuationInworldDistance, m_originalSoundAttenuationInworldDistance);
            SFDCTConfig.Set(CTSettingKey.LowHealthSaturationFactor, m_originalLowHealthSaturationFactor);
            SFDCTConfig.Set(CTSettingKey.LowHealthThreshold, m_originalLowHealthThreshold);
            SFDCTConfig.Set(CTSettingKey.LowHealthHurtLevel1Threshold, m_originalLowHealthHurtLevel1Threshold);
            SFDCTConfig.Set(CTSettingKey.LowHealthHurtLevel2Threshold, m_originalLowHealthHurtLevel2Threshold);
            SFDCTConfig.Set(CTSettingKey.HideFilmgrain, m_originalHideFilmgrain);
            SFDCTConfig.Set(CTSettingKey.Language, m_originalLanguage);
            SFDCTConfig.Set(CTSettingKey.SpectatorsMaximum, m_originalSpectatorsMaximum);
            SFDCTConfig.Set(CTSettingKey.SpectatorsOnlyModerators, m_originalSpectatorsOnlyModerators);
            SFDCTConfig.Set(CTSettingKey.VoteKickEnabled, m_originalVoteKickEnabled);
            SFDCTConfig.Set(CTSettingKey.VoteKickFailCooldown, m_originalVoteKickFailCooldown);
            SFDCTConfig.Set(CTSettingKey.VoteKickSuccessCooldown, m_originalVoteKickSuccessCooldown);
            SFDCTConfig.Set(CTSettingKey.SubContent, m_originalSubContent);
            SFDCTConfig.Set(CTSettingKey.SubContentDisabledFolders, m_originalSubContentDisabledFolders);
            SFDCTConfig.Set(CTSettingKey.SubContentEnabledFolders, m_originalSubContentEnabledFolders);

            if (m_originalPrimaryColorHex != Constants.COLORS.MENU_BLUE.ToHex())
            {
                Constants.COLORS.MENU_BLUE = m_originalPrimaryColorHex.ToColor();
                SFDConfig.SaveConfig(SFDConfigSaveMode.Settings);
            }

            if (m_settingsNeedGameRestart)
            {
                MessageStack.Show(LanguageHelper.GetText("menu.settings.restartrequiredmessage"), MessageStackType.Information);
            }

            ParentPanel.CloseSubPanel();
        }, (object _) =>
        {
            CloseSubPanel();
        }));
    }
}
