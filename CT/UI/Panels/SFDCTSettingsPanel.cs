using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SFD;
using SFD.MenuControls;
using SFDCT.Bootstrap;
using SFDCT.Configuration;
using SFDCT.UI.MenuItems;
using System;

namespace SFDCT.UI.Panels;

internal class SFDCTSettingsPanel : Panel
{
    private Menu m_menu;
    private bool m_settingsNeedGameRestart;

    private readonly bool m_originalSoundPanningEnabled = SFDCTConfig.Get<bool>(CTSettingKey.SoundPanningEnabled);
    private readonly float m_originalSoundPanningStrength = SFDCTConfig.Get<float>(CTSettingKey.SoundPanningStrength);
    private readonly bool m_originalSoundPanningForceScreenSpace = SFDCTConfig.Get<bool>(CTSettingKey.SoundPanningForceScreenSpace);
    private readonly int m_originalSoundPanningInworldThreshold = SFDCTConfig.Get<int>(CTSettingKey.SoundPanningInworldThreshold);
    private readonly int m_originalSoundPanningInworldDistance = SFDCTConfig.Get<int>(CTSettingKey.SoundPanningInworldDistance);
    private readonly bool m_originalSoundAttenuationEnabled = SFDCTConfig.Get<bool>(CTSettingKey.SoundAttenuationEnabled);
    private readonly float m_originalSoundAttenuationMin = SFDCTConfig.Get<float>(CTSettingKey.SoundAttenuationMin);
    private readonly bool m_originalSoundAttenuationForceScreenSpace = SFDCTConfig.Get<bool>(CTSettingKey.SoundAttenuationForceScreenSpace);
    private readonly int m_originalSoundAttenuationInworldThreshold = SFDCTConfig.Get<int>(CTSettingKey.SoundAttenuationInworldThreshold);
    private readonly int m_originalSoundAttenuationInworldDistance = SFDCTConfig.Get<int>(CTSettingKey.SoundAttenuationInworldDistance);
    private readonly float m_originalLowHealthSaturationFactor = SFDCTConfig.Get<float>(CTSettingKey.LowHealthSaturationFactor);
    private readonly float m_originalLowHealthThreshold = SFDCTConfig.Get<float>(CTSettingKey.LowHealthThreshold);
    private readonly float m_originalLowHealthHurtLevel1Threshold = SFDCTConfig.Get<float>(CTSettingKey.LowHealthHurtLevel1Threshold);
    private readonly float m_originalLowHealthHurtLevel2Threshold = SFDCTConfig.Get<float>(CTSettingKey.LowHealthHurtLevel2Threshold);
    private readonly bool m_originalHideFilmgrain = SFDCTConfig.Get<bool>(CTSettingKey.HideFilmgrain);
    private readonly string m_originalLanguage = SFDCTConfig.Get<string>(CTSettingKey.Language);
    private readonly int m_originalSpectatorsMaximum = SFDCTConfig.Get<int>(CTSettingKey.SpectatorsMaximum);
    private readonly bool m_originalSpectatorsOnlyModerators = SFDCTConfig.Get<bool>(CTSettingKey.SpectatorsOnlyModerators);
    private readonly bool m_originalVoteKickEnabled = SFDCTConfig.Get<bool>(CTSettingKey.VoteKickEnabled);
    private readonly int m_originalVoteKickFailCooldown = SFDCTConfig.Get<int>(CTSettingKey.VoteKickFailCooldown);
    private readonly int m_originalVoteKickSuccessCooldown = SFDCTConfig.Get<int>(CTSettingKey.VoteKickSuccessCooldown);
    private readonly bool m_originalSubContent = SFDCTConfig.Get<bool>(CTSettingKey.SubContent);
    private readonly string m_originalSubContentDisabledFolders = SFDCTConfig.Get<string>(CTSettingKey.SubContentDisabledFolders);
    private readonly string m_originalSubContentEnabledFolders = SFDCTConfig.Get<string>(CTSettingKey.SubContentEnabledFolders);
    private readonly string m_originalPrimaryColorHex = Constants.COLORS.MENU_BLUE.ToHex();
    private readonly int m_originalChatWidth = SFDCTConfig.Get<int>(CTSettingKey.ChatWidth);
    private readonly int m_originalChatHeight = SFDCTConfig.Get<int>(CTSettingKey.ChatHeight);
    private readonly int m_originalChatExtraHeight = SFDCTConfig.Get<int>(CTSettingKey.ChatExtraHeight);

    public SFDCTSettingsPanel() : base(LanguageHelper.GetText("sfdct.setting.header"), 500, 500)
    {
        m_menu = new Menu(new Vector2(0, 50), Width, Height - 50, this, []);
        m_settingsNeedGameRestart = false;

        // Credits
        m_menu.Add(new MenuItemButton(LanguageHelper.GetText("sfdct.credits.name"), new ControlEvents.ChooseEvent(_ => OpenSubPanel(new SFDCTCreditsPanel()))));

        // Sound Panning
        m_menu.Add(new MenuItemSeparator(LanguageHelper.GetText("sfdct.setting.category.soundpanning")));
        m_menu.Add(CreateBoolSetting(CTSettingKey.SoundPanningEnabled, "sfdct.setting.name.soundpanningenabled"));
        m_menu.Add(CreateFloatPercentSetting(CTSettingKey.SoundPanningStrength, "sfdct.setting.name.soundpanningstrength"));
        m_menu.Add(CreateBoolSetting(CTSettingKey.SoundPanningForceScreenSpace, "sfdct.setting.name.soundpanningforcescreenspace"));
        m_menu.Add(CreateIntSetting(CTSettingKey.SoundPanningInworldThreshold, 0, 1000, 5, "sfdct.setting.name.soundpanninginworldthreshold", "sfdct.setting.help.soundpanninginworldthreshold"));
        m_menu.Add(CreateIntSetting(CTSettingKey.SoundPanningInworldDistance, 0, 1000, 5, "sfdct.setting.name.soundpanninginworlddistance", "sfdct.setting.help.soundpanninginworlddistance"));

        // Sound Attenuation
        m_menu.Add(new MenuItemSeparator(LanguageHelper.GetText("sfdct.setting.category.soundattenuation")));
        m_menu.Add(CreateBoolSetting(CTSettingKey.SoundAttenuationEnabled, "sfdct.setting.name.soundattenuationenabled"));
        m_menu.Add(CreateFloatPercentSetting(CTSettingKey.SoundAttenuationMin, "sfdct.setting.name.soundattenuationmin", "sfdct.setting.help.soundattenuationmin"));
        m_menu.Add(CreateBoolSetting(CTSettingKey.SoundAttenuationForceScreenSpace, "sfdct.setting.name.soundattenuationforcescreenspace"));
        m_menu.Add(CreateIntSetting(CTSettingKey.SoundAttenuationInworldThreshold, 0, 1000, 5, "sfdct.setting.name.soundattenuationinworldthreshold", "sfdct.setting.help.soundattenuationinworldthreshold"));
        m_menu.Add(CreateIntSetting(CTSettingKey.SoundAttenuationInworldDistance, 0, 1000, 5, "sfdct.setting.name.soundattenuationinworlddistance", "sfdct.setting.help.soundattenuationinworlddistance"));

        // Low Health
        m_menu.Add(new MenuItemSeparator(LanguageHelper.GetText("sfdct.setting.category.lowhealth")));
        m_menu.Add(CreateFloatPercentSetting(CTSettingKey.LowHealthSaturationFactor, "sfdct.setting.name.lowhealthsaturationfactor", null, true));
        m_menu.Add(CreateFloatPercentSetting(CTSettingKey.LowHealthThreshold, "sfdct.setting.name.lowhealththreshold", null, true));
        m_menu.Add(CreateFloatPercentSetting(CTSettingKey.LowHealthHurtLevel1Threshold, "sfdct.setting.name.lowhealthhurtlevel1threshold", null, true));
        m_menu.Add(CreateFloatPercentSetting(CTSettingKey.LowHealthHurtLevel2Threshold, "sfdct.setting.name.lowhealthhurtlevel2threshold", null, true));

        // Spectators
        m_menu.Add(new MenuItemSeparator(LanguageHelper.GetText("sfdct.setting.category.spectators")));
        m_menu.Add(CreateIntSetting(CTSettingKey.SpectatorsMaximum, 1, 8, 1, "sfdct.setting.name.spectatorsmaximum"));
        m_menu.Add(CreateBoolSetting(CTSettingKey.SpectatorsOnlyModerators, "sfdct.setting.name.spectatorsonlymoderators", "sfdct.setting.help.spectatorsonlymoderators"));

        // Vote Kick
        m_menu.Add(new MenuItemSeparator(LanguageHelper.GetText("sfdct.setting.category.votekick")));
        m_menu.Add(CreateBoolSetting(CTSettingKey.VoteKickEnabled, "sfdct.setting.name.votekickenabled"));
        m_menu.Add(CreateIntSetting(CTSettingKey.VoteKickFailCooldown, 30, 300, 5, "sfdct.setting.name.votekickfailcooldown"));
        m_menu.Add(CreateIntSetting(CTSettingKey.VoteKickSuccessCooldown, 30, 300, 5, "sfdct.setting.name.votekicksuccesscooldown"));

        // Misc
        m_menu.Add(new MenuItemSeparator(LanguageHelper.GetText("sfdct.setting.category.misc")));
        m_menu.Add(CreateBoolSetting(CTSettingKey.HideFilmgrain, "sfdct.setting.name.hidefilmgrain"));

        // these ranges and steps make it easier to set back the default value,
        // the true range is from half to double
        m_menu.Add(CreateIntSetting(CTSettingKey.ChatWidth, 218, 848, 10, "sfdct.setting.name.chatwidth"));
        m_menu.Add(CreateIntSetting(CTSettingKey.ChatHeight, 90, 360, 10, "sfdct.setting.name.chatheight"));
        m_menu.Add(CreateIntSetting(CTSettingKey.ChatExtraHeight, 0, 180, 10, "sfdct.setting.name.chatextraheight", "sfdct.setting.help.chatextraheight"));

        var availableSFDCTLanguages = LanguageHandler.GetAvailableLanguages();
        var m_menuItemLanguage = new MenuItemDropdown(LanguageHelper.GetText("sfdct.setting.name.language"), availableSFDCTLanguages);
        m_menuItemLanguage.SetStartValue(Math.Max(0, Array.IndexOf(availableSFDCTLanguages, SFDCTConfig.Get<string>(CTSettingKey.Language))));
        m_menuItemLanguage.DropdownItemVisibleCount = availableSFDCTLanguages.Length;
        EventHelper.Add(m_menuItemLanguage, "ValueChangedEvent", new MenuItemValueChangedEvent(_ =>
        {
            m_settingsNeedGameRestart = true;
            SFDCTConfig.Set(CTSettingKey.Language, m_menuItemLanguage.Value);
        }));
        m_menu.Add(m_menuItemLanguage);

        // Subcontent
        m_menu.Add(new MenuItemSeparator(LanguageHelper.GetText("sfdct.setting.category.subcontent")));
        m_menu.Add(CreateBoolSetting(CTSettingKey.SubContent, "sfdct.setting.name.subcontentenabled", "sfdct.setting.help.subcontentenabled", true));
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

    private MenuItemDropdown CreateBoolSetting(CTSettingKey configKey, string labelKey, string tooltipKey = null, bool requiresRestart = false)
    {
        var item = new MenuItemDropdown(
            LanguageHelper.GetText(labelKey),
            [LanguageHelper.GetText("general.on"), LanguageHelper.GetText("general.off")]
        );
        item.SetStartValue(SFDCTConfig.Get<bool>(configKey) ? 0 : 1);
        item.DropdownItemVisibleCount = 2;
        item.Tooltip = tooltipKey != null ? LanguageHelper.GetText(tooltipKey) : null;

        EventHelper.Add(item, "ValueChangedEvent", new MenuItemValueChangedEvent(_ =>
        {
            if (requiresRestart) m_settingsNeedGameRestart = true;
            SFDCTConfig.Set(configKey, item.ValueId == 0);
        }));

        return item;
    }

    private MenuItemSlider CreateIntSetting(CTSettingKey configKey, int min, int max, int step, string labelKey, string tooltipKey = null, bool requiresRestart = false)
    {
        var item = new MenuItemSlider(
            LanguageHelper.GetText(labelKey),
            SFDCTConfig.Get<int>(configKey),
            min, max, step
        );
        item.SetStartValue(SFDCTConfig.Get<int>(configKey));
        item.Tooltip = tooltipKey != null ? LanguageHelper.GetText(tooltipKey) : null;

        EventHelper.Add(item, "ValueChangedEvent", new MenuItemValueChangedEvent(_ =>
        {
            if (requiresRestart) m_settingsNeedGameRestart = true;
            SFDCTConfig.Set(configKey, item.Value);
        }));

        return item;
    }

    private MenuItemSlider CreateFloatPercentSetting(CTSettingKey configKey, string labelKey, string tooltipKey = null, bool requiresRestart = false)
    {
        var item = new MenuItemSlider(
            LanguageHelper.GetText(labelKey),
            (int)(100 * SFDCTConfig.Get<float>(configKey)),
            0, 100, 1
        );
        item.SetStartValue((int)(100 * SFDCTConfig.Get<float>(configKey)));
        item.Tooltip = tooltipKey != null ? LanguageHelper.GetText(tooltipKey) : null;

        EventHelper.Add(item, "ValueChangedEvent", new MenuItemValueChangedEvent(_ =>
        {
            if (requiresRestart) m_settingsNeedGameRestart = true;
            SFDCTConfig.Set(configKey, item.Value * 0.01f);
        }));

        return item;
    }

    public override void KeyPress(Keys key)
    {
        if (key == Keys.Escape)
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
        OpenSubPanel(new ConfirmYesNoPanel(LanguageHelper.GetText("menu.settings.confirmcancel"), LanguageHelper.GetText("general.yes"), LanguageHelper.GetText("general.no"), _ =>
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
            SFDCTConfig.Set(CTSettingKey.ChatWidth, m_originalChatWidth);
            SFDCTConfig.Set(CTSettingKey.ChatHeight, m_originalChatHeight);
            SFDCTConfig.Set(CTSettingKey.ChatExtraHeight, m_originalChatExtraHeight);

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
        }, _ =>
        {
            CloseSubPanel();
        }));
    }
}
