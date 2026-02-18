using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SFD;
using SFD.MenuControls;
using SFD.Sounds;
using SFDCT.Bootstrap;
using SFDCT.Configuration;
using SFDCT.UI.MenuItems;
using System;

namespace SFDCT.UI.Panels;

internal class SFDCTSettingsPanel : Panel
{
    private Menu m_menu;
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

        // Credits
        m_menu.Add(new MenuItemButton(LanguageHelper.GetText("sfdct.credits.name"), new ControlEvents.ChooseEvent(_ => OpenSubPanel(new SFDCTCreditsPanel()))));

        // Sound Panning
        m_menu.Add(new MenuItemSeparator(LanguageHelper.GetText("sfdct.setting.category.soundpanning")));
        m_menu.Add(CreateBoolSetting("sfdct.setting.name.soundpanningenabled", "sfdct.setting.help.soundpanningenabled", CTSettingKey.SoundPanningEnabled));
        m_menu.Add(CreateFloatPercentSetting("sfdct.setting.name.soundpanningstrength", "sfdct.setting.help.soundpanningstrength", CTSettingKey.SoundPanningStrength));
        m_menu.Add(CreateBoolSetting("sfdct.setting.name.soundpanningforcescreenspace", "sfdct.setting.help.soundpanningforcescreenspace", CTSettingKey.SoundPanningForceScreenSpace));
        m_menu.Add(CreateIntSetting("sfdct.setting.name.soundpanninginworldthreshold", "sfdct.setting.help.soundpanninginworldthreshold", CTSettingKey.SoundPanningInworldThreshold, 0, 1000, 5));
        m_menu.Add(CreateIntSetting("sfdct.setting.name.soundpanninginworlddistance", "sfdct.setting.help.soundpanninginworlddistance", CTSettingKey.SoundPanningInworldDistance, 0, 1000, 5));

        // Sound Attenuation
        m_menu.Add(new MenuItemSeparator(LanguageHelper.GetText("sfdct.setting.category.soundattenuation")));
        m_menu.Add(CreateBoolSetting("sfdct.setting.name.soundattenuationenabled", "sfdct.setting.help.soundattenuationenabled", CTSettingKey.SoundAttenuationEnabled));
        m_menu.Add(CreateFloatPercentSetting("sfdct.setting.name.soundattenuationmin", "sfdct.setting.help.soundattenuationmin", CTSettingKey.SoundAttenuationMin));
        m_menu.Add(CreateBoolSetting("sfdct.setting.name.soundattenuationforcescreenspace", "sfdct.setting.help.soundattenuationforcescreenspace", CTSettingKey.SoundAttenuationForceScreenSpace));
        m_menu.Add(CreateIntSetting("sfdct.setting.name.soundattenuationinworldthreshold", "sfdct.setting.help.soundattenuationinworldthreshold", CTSettingKey.SoundAttenuationInworldThreshold, 0, 1000, 5));
        m_menu.Add(CreateIntSetting("sfdct.setting.name.soundattenuationinworlddistance", "sfdct.setting.help.soundattenuationinworlddistance", CTSettingKey.SoundAttenuationInworldDistance, 0, 1000, 5, requiresRestart: true));

        // Low Health
        m_menu.Add(new MenuItemSeparator(LanguageHelper.GetText("sfdct.setting.category.lowhealth")));
        m_menu.Add(CreateFloatPercentSetting("sfdct.setting.name.lowhealthsaturationfactor", "sfdct.setting.help.lowhealthsaturationfactor", CTSettingKey.LowHealthSaturationFactor, requiresRestart: true));
        m_menu.Add(CreateFloatPercentSetting("sfdct.setting.name.lowhealththreshold", "sfdct.setting.help.lowhealththreshold", CTSettingKey.LowHealthThreshold, requiresRestart: true));
        m_menu.Add(CreateFloatPercentSetting("sfdct.setting.name.lowhealthhurtlevel1threshold", "sfdct.setting.help.lowhealthhurtlevel1threshold", CTSettingKey.LowHealthHurtLevel1Threshold, requiresRestart: true));
        m_menu.Add(CreateFloatPercentSetting("sfdct.setting.name.lowhealthhurtlevel2threshold", "sfdct.setting.help.lowhealthhurtlevel2threshold", CTSettingKey.LowHealthHurtLevel2Threshold, requiresRestart: true));

        // Spectators
        m_menu.Add(new MenuItemSeparator(LanguageHelper.GetText("sfdct.setting.category.spectators")));
        m_menu.Add(CreateIntSetting("sfdct.setting.name.spectatorsmaximum", "sfdct.setting.help.spectatorsmaximum", CTSettingKey.SpectatorsMaximum, 1, 8, 1));
        m_menu.Add(CreateBoolSetting("sfdct.setting.name.spectatorsonlymoderators", "sfdct.setting.help.spectatorsonlymoderators", CTSettingKey.SpectatorsOnlyModerators));

        // Vote Kick
        m_menu.Add(new MenuItemSeparator(LanguageHelper.GetText("sfdct.setting.category.votekick")));
        m_menu.Add(CreateBoolSetting("sfdct.setting.name.votekickenabled", "sfdct.setting.help.votekickenabled", CTSettingKey.VoteKickEnabled));
        m_menu.Add(CreateIntSetting("sfdct.setting.name.votekickfailcooldown", "sfdct.setting.help.votekickfailcooldown", CTSettingKey.VoteKickFailCooldown, 30, 300, 5));
        m_menu.Add(CreateIntSetting("sfdct.setting.name.votekicksuccesscooldown", "sfdct.setting.help.votekicksuccesscooldown", CTSettingKey.VoteKickSuccessCooldown, 30, 300, 5));

        // Misc
        m_menu.Add(new MenuItemSeparator(LanguageHelper.GetText("sfdct.setting.category.misc")));
        m_menu.Add(CreateBoolSetting("sfdct.setting.name.hidefilmgrain", "sfdct.setting.help.hidefilmgrain", CTSettingKey.HideFilmgrain));

        // Language
        var availableSFDCTLanguages = LanguageHandler.GetAvailableLanguages();
        var m_menuItemLanguage = new MenuItemDropdown(LanguageHelper.GetText("sfdct.setting.name.language"), availableSFDCTLanguages);
        m_menuItemLanguage.SetStartValue(Math.Max(0, Array.IndexOf(availableSFDCTLanguages, SFDCTConfig.Get<string>(CTSettingKey.Language))));
        m_menuItemLanguage.DropdownItemVisibleCount = availableSFDCTLanguages.Length;
        m_menuItemLanguage.Tooltip = LanguageHelper.GetText("sfdct.setting.help.language");
        EventHelper.Add(m_menuItemLanguage, "ValueChangedEvent", new MenuItemValueChangedEvent(_ =>
        {
            m_settingsNeedGameRestart = true;
            SFDCTConfig.Set(CTSettingKey.Language, m_menuItemLanguage.Value);
        }));
        m_menu.Add(m_menuItemLanguage);

        // Subcontent
        m_menu.Add(new MenuItemSeparator(LanguageHelper.GetText("sfdct.setting.category.subcontent")));
        m_menu.Add(CreateBoolSetting("sfdct.setting.name.subcontentenabled", "sfdct.setting.help.subcontentenabled", CTSettingKey.SubContent, requiresRestart: true));
        m_menu.Add(new MenuItemButton(LanguageHelper.GetText("sfdct.setting.name.subcontentfolders"), _ => OpenSubPanel(new SFDCTSubContentPanel()), MenuIcons.Settings));

        // Primary Color
        MenuItemText m_menuItemPrimaryColorHex = null;
        SFDCTMenuItemDropdownColor m_menuItemPrimaryColor = null;

        m_menuItemPrimaryColorHex = new MenuItemText(LanguageHelper.GetText("sfdct.setting.name.primarycolorhex"), Constants.COLORS.MENU_BLUE.ToHex());
        EventHelper.Add(m_menuItemPrimaryColorHex.TextSetValidationItem, "TextValidationEvent", new TextValidationEvent((string setText, TextValidationEventArgs _) =>
        {
            if (setText.IsHex())
            {
                Constants.COLORS.MENU_BLUE = setText.ToColor();

                if (m_menuItemPrimaryColor.Color != Constants.COLORS.MENU_BLUE)
                {
                    m_menuItemPrimaryColor.SetColor(Constants.COLORS.MENU_BLUE);
                }
            }
            else
            {
                m_menuItemPrimaryColorHex.SetValue(Constants.COLORS.MENU_BLUE.ToHex());
            }
        }));

        m_menuItemPrimaryColor = new SFDCTMenuItemDropdownColor(LanguageHelper.GetText("sfdct.setting.name.primarycolor"), Constants.COLORS.MENU_BLUE, [
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
            new Color(32, 0, 192), // MENU_BLUE Default
        ]);
        m_menuItemPrimaryColor.ValueChangedEvent += (MenuItem _) =>
        {
            Constants.COLORS.MENU_BLUE = m_menuItemPrimaryColor.Color;

            if (m_menuItemPrimaryColorHex.Value != Constants.COLORS.MENU_BLUE.ToHex())
            {
                m_menuItemPrimaryColorHex.SetValue(Constants.COLORS.MENU_BLUE.ToHex());
            }
        };

        m_menu.Add(new MenuItemSeparator(LanguageHelper.GetText("sfdct.setting.category.primarycolor")));
        m_menu.Add(m_menuItemPrimaryColorHex);
        m_menu.Add(m_menuItemPrimaryColor);

        m_menu.Add(new MenuItemSeparator(string.Empty));
        m_menu.Add(new MenuItemButton(LanguageHelper.GetText("button.done"), new ControlEvents.ChooseEvent(ok), MenuIcons.Ok));
        m_menu.Add(new MenuItemButton(LanguageHelper.GetText("button.back"), new ControlEvents.ChooseEvent(back), MenuIcons.Cancel));

        members.Add(m_menu);
        m_menu.SelectFirst();
    }

    private MenuItemDropdown CreateBoolSetting(string labelKey, string tooltipKey, CTSettingKey configKey, bool requiresRestart = false)
    {
        var item = new MenuItemDropdown(
            LanguageHelper.GetText(labelKey),
            [LanguageHelper.GetText("general.on"), LanguageHelper.GetText("general.off")]
        );
        item.SetStartValue(SFDCTConfig.Get<bool>(configKey) ? 0 : 1);
        item.DropdownItemVisibleCount = 2;
        item.Tooltip = LanguageHelper.GetText(tooltipKey);

        EventHelper.Add(item, "ValueChangedEvent", new MenuItemValueChangedEvent(_ =>
        {
            if (requiresRestart) m_settingsNeedGameRestart = true;
            SFDCTConfig.Set(configKey, item.ValueId == 0);
        }));

        return item;
    }

    private MenuItemSlider CreateIntSetting(string labelKey, string tooltipKey, CTSettingKey configKey, int min, int max, int step, bool requiresRestart = false)
    {
        var item = new MenuItemSlider(
            LanguageHelper.GetText(labelKey),
            SFDCTConfig.Get<int>(configKey),
            min, max, step
        );
        item.SetStartValue(SFDCTConfig.Get<int>(configKey));
        item.Tooltip = LanguageHelper.GetText(tooltipKey);

        EventHelper.Add(item, "ValueChangedEvent", new MenuItemValueChangedEvent(_ =>
        {
            if (requiresRestart) m_settingsNeedGameRestart = true;
            SFDCTConfig.Set(configKey, item.Value);
        }));

        return item;
    }

    private MenuItemSlider CreateFloatPercentSetting(string labelKey, string tooltipKey, CTSettingKey configKey, bool requiresRestart = false)
    {
        var item = new MenuItemSlider(
            LanguageHelper.GetText(labelKey),
            (int)(100 * SFDCTConfig.Get<float>(configKey)),
            0, 100, 1
        );
        item.SetStartValue((int)(100 * SFDCTConfig.Get<float>(configKey)));
        item.Tooltip = LanguageHelper.GetText(tooltipKey);

        EventHelper.Add(item, "ValueChangedEvent", new MenuItemValueChangedEvent(_ =>
        {
            if (requiresRestart) m_settingsNeedGameRestart = true;
            SFDCTConfig.Set(configKey, item.Value * 0.01f);
        }));

        return item;
    }

    public override void KeyPress(Keys key)
    {
        if (subPanel != null && key == Keys.Escape)
        {
            base.KeyPress(key);
            return;
        }

        if (m_menu.FocusMenuAndSelectLastItem())
        {
            SoundHandler.PlayGlobalSound("MenuMove");
            return;
        }

        back(null);
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
