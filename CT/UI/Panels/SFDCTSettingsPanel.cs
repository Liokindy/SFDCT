using System.Globalization;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SFD;
using SFD.MenuControls;
using CIni = SFDCT.Settings.Config;
using CSettings = SFDCT.Settings.Values;

namespace SFDCT.UI.Panels
{
    public class SFDCTSettingsPanel : Panel
    {
        private bool m_modifiedSettings;
        private bool m_OriginalSoundPanningEnabled;
        private float m_OriginalSoundPanningStrength;
        private bool m_OriginalSoundPanningScreenSpace;
        private int m_OriginalSoundPanningThreshold;
        private int m_OriginalSoundPanningDistance;
        private bool m_OriginalSoundAttenuationEnabled;
        private float m_OriginalSoundAttenuationMin;
        private bool m_OriginalSoundAttenuationScreenSpace;
        private int m_OriginalSoundAttenuationThreshold;
        private int m_OriginalSoundAttenuationDistance;

        private MenuItemDropdown m_menuItemSoundPanningEnabled = null;
        private MenuItemSlider m_menuItemSoundPanningStrength = null;
        private MenuItemDropdown m_menuItemSoundPanningScreenSpace = null;
        private MenuItemSlider m_menuItemSoundPanningThreshold = null;
        private MenuItemSlider m_menuItemSoundPanningDistance = null;
        private MenuItemDropdown m_menuItemSoundAttenuationEnabled = null;
        private MenuItemSlider m_menuItemSoundAttenuationMin = null;
        private MenuItemDropdown m_menuItemSoundAttenuationScreenSpace = null;
        private MenuItemSlider m_menuItemSoundAttenuationThreshold = null;
        private MenuItemSlider m_menuItemSoundAttenuationDistance = null;
        private MenuItemButton m_menuItemResetToDefault = null;

        private MenuItemValueChangedEvent m_menuItemSoundPanningEnabledValueChanged = null;
        private MenuItemValueChangedEvent m_menuItemSoundPanningStrengthValueChanged = null;
        private MenuItemValueChangedEvent m_menuItemSoundPanningScreenSpaceValueChanged = null;
        private MenuItemValueChangedEvent m_menuItemSoundPanningThresholdValueChanged = null;
        private MenuItemValueChangedEvent m_menuItemSoundPanningDistanceValueChanged = null;
        private MenuItemValueChangedEvent m_menuItemSoundAttenuationEnabledValueChanged = null;
        private MenuItemValueChangedEvent m_menuItemSoundAttenuationMinValueChanged = null;
        private MenuItemValueChangedEvent m_menuItemSoundAttenuationScreenSpaceValueChanged = null;
        private MenuItemValueChangedEvent m_menuItemSoundAttenuationThresholdValueChanged = null;
        private MenuItemValueChangedEvent m_menuItemSoundAttenuationDistanceValueChanged = null;

        public SFDCTSettingsPanel() : base("CTSETTINGS", 500, 500)
        {
            SetOriginalValues();

            m_menuItemSoundPanningEnabled = new MenuItemDropdown("ENABLE PANNING", [
                LanguageHelper.GetText("general.on"),
                LanguageHelper.GetText("general.off"),
            ]);
            m_menuItemSoundPanningEnabled.SetStartValue(CSettings.Get<bool>(CSettings.GetKey(CSettings.SettingKey.SoundPanningEnabled)) ? 0 : 1);

            m_menuItemSoundPanningStrength = new MenuItemSlider("STRENGTH", (int)(CSettings.Get<float>(CSettings.GetKey(CSettings.SettingKey.SoundPanningStrength)) * 100f), (int)(CSettings.GetLimit<float>(CSettings.GetKey(CSettings.SettingKey.SoundPanningStrength)) * 100), (int)(CSettings.GetLimit<float>(CSettings.GetKey(CSettings.SettingKey.SoundPanningStrength), true) * 100));
            m_menuItemSoundPanningScreenSpace = new MenuItemDropdown("USE SCREEN SPACE", [
                LanguageHelper.GetText("general.on"),
                LanguageHelper.GetText("general.off"),
            ]);
            m_menuItemSoundPanningScreenSpace.SetStartValue(CSettings.Get<bool>(CSettings.GetKey(CSettings.SettingKey.SoundPanningForceScreenSpace)) ? 0 : 1);
            m_menuItemSoundPanningThreshold = new MenuItemSlider("THRESHOLD", CSettings.Get<int>(CSettings.GetKey(CSettings.SettingKey.SoundPanningInworldThreshold)), CSettings.GetLimit<int>(CSettings.GetKey(CSettings.SettingKey.SoundPanningInworldThreshold)), (CSettings.GetLimit<int>(CSettings.GetKey(CSettings.SettingKey.SoundPanningInworldThreshold), true)), 5);
            m_menuItemSoundPanningDistance = new MenuItemSlider("DISTANCE", CSettings.Get<int>(CSettings.GetKey(CSettings.SettingKey.SoundPanningInworldDistance)), CSettings.GetLimit<int>(CSettings.GetKey(CSettings.SettingKey.SoundPanningInworldDistance)), (CSettings.GetLimit<int>(CSettings.GetKey(CSettings.SettingKey.SoundPanningInworldDistance), true)), 5);

            m_menuItemSoundAttenuationEnabled = new MenuItemDropdown("ENABLE ATTENUATION", [
                LanguageHelper.GetText("general.on"),
                LanguageHelper.GetText("general.off"),
            ]);
            m_menuItemSoundAttenuationEnabled.SetStartValue(CSettings.Get<bool>(CSettings.GetKey(CSettings.SettingKey.SoundAttenuationEnabled)) ? 0 : 1);
            m_menuItemSoundAttenuationMin = new MenuItemSlider("STRENGTH", (int)(CSettings.Get<float>(CSettings.GetKey(CSettings.SettingKey.SoundAttenuationMin)) * 100f), (int)(CSettings.GetLimit<float>(CSettings.GetKey(CSettings.SettingKey.SoundAttenuationMin)) * 100), (int)(CSettings.GetLimit<float>(CSettings.GetKey(CSettings.SettingKey.SoundAttenuationMin), true) * 100));
            m_menuItemSoundAttenuationScreenSpace = new MenuItemDropdown("USE SCREEN SPACE", [
                LanguageHelper.GetText("general.on"),
                LanguageHelper.GetText("general.off"),
            ]);
            m_menuItemSoundAttenuationScreenSpace.SetStartValue(CSettings.Get<bool>(CSettings.GetKey(CSettings.SettingKey.SoundAttenuationForceScreenSpace)) ? 0 : 1);
            m_menuItemSoundAttenuationThreshold = new MenuItemSlider("THRESHOLD", CSettings.Get<int>(CSettings.GetKey(CSettings.SettingKey.SoundAttenuationInworldThreshold)), CSettings.GetLimit<int>(CSettings.GetKey(CSettings.SettingKey.SoundAttenuationInworldThreshold)), CSettings.GetLimit<int>(CSettings.GetKey(CSettings.SettingKey.SoundAttenuationInworldThreshold), true), 5);
            m_menuItemSoundAttenuationDistance = new MenuItemSlider("DISTANCE", CSettings.Get<int>(CSettings.GetKey(CSettings.SettingKey.SoundAttenuationInworldDistance)), CSettings.GetLimit<int>(CSettings.GetKey(CSettings.SettingKey.SoundAttenuationInworldDistance)), CSettings.GetLimit<int>(CSettings.GetKey(CSettings.SettingKey.SoundAttenuationInworldDistance), true), 5);

            m_menuItemResetToDefault = new MenuItemButton("RESET TO DEFAULT", new ControlEvents.ChooseEvent(this.setDefault), "micon_settings");

            m_menuItemSoundPanningEnabledValueChanged += m_menuItemSoundPanningEnabled_ValueChanged; HookHandler.Hook(m_menuItemSoundPanningEnabled, m_menuItemSoundPanningEnabledValueChanged);
            m_menuItemSoundPanningStrengthValueChanged += m_menuItemSoundPanningStrength_ValueChanged; HookHandler.Hook(m_menuItemSoundPanningStrength, m_menuItemSoundPanningStrengthValueChanged);
            m_menuItemSoundPanningScreenSpaceValueChanged += m_menuItemSoundPanningScreenSpace_ValueChanged; HookHandler.Hook(m_menuItemSoundPanningScreenSpace, m_menuItemSoundPanningScreenSpaceValueChanged);
            m_menuItemSoundPanningThresholdValueChanged += m_menuItemSoundPanningThreshold_ValueChanged; HookHandler.Hook(m_menuItemSoundPanningThreshold, m_menuItemSoundPanningThresholdValueChanged);
            m_menuItemSoundPanningDistanceValueChanged += m_menuItemSoundPanningDistance_ValueChanged; HookHandler.Hook(m_menuItemSoundPanningDistance, m_menuItemSoundPanningDistanceValueChanged);
            m_menuItemSoundAttenuationEnabledValueChanged += m_menuItemSoundAttenuationEnabled_ValueChanged; HookHandler.Hook(m_menuItemSoundAttenuationEnabled, m_menuItemSoundAttenuationEnabledValueChanged);
            m_menuItemSoundAttenuationMinValueChanged += m_menuItemSoundAttenuationMin_ValueChanged; HookHandler.Hook(m_menuItemSoundAttenuationMin, m_menuItemSoundAttenuationMinValueChanged);
            m_menuItemSoundAttenuationScreenSpaceValueChanged += m_menuItemSoundAttenuationScreenSpace_ValueChanged; HookHandler.Hook(m_menuItemSoundAttenuationScreenSpace, m_menuItemSoundAttenuationScreenSpaceValueChanged);
            m_menuItemSoundAttenuationThresholdValueChanged += m_menuItemSoundAttenuationThreshold_ValueChanged; HookHandler.Hook(m_menuItemSoundAttenuationThreshold, m_menuItemSoundAttenuationThresholdValueChanged);
            m_menuItemSoundAttenuationDistanceValueChanged += m_menuItemSoundAttenuationDistance_ValueChanged; HookHandler.Hook(m_menuItemSoundAttenuationDistance, m_menuItemSoundAttenuationDistanceValueChanged);
            
            SetTooltips();

            List<MenuItem> items = [
                new MenuItemSeparator("SOUND PANNING"),
                m_menuItemSoundPanningEnabled,
                m_menuItemSoundPanningStrength,
                m_menuItemSoundPanningScreenSpace,
                m_menuItemSoundPanningThreshold,
                m_menuItemSoundPanningDistance,
                new MenuItemSeparator("SOUND ATTENUATION"),
                m_menuItemSoundAttenuationEnabled,
                m_menuItemSoundAttenuationMin,
                m_menuItemSoundAttenuationScreenSpace,
                m_menuItemSoundAttenuationThreshold,
                m_menuItemSoundAttenuationDistance,
                new MenuItemSeparator("MAIN MENU"),
                new MenuItemSeparator("SPECTATORS"),
                new MenuItemSeparator("MISC"),
                new MenuItemSeparator(""),
                m_menuItemResetToDefault,
                new MenuItemButton(LanguageHelper.GetText("button.done"), new ControlEvents.ChooseEvent(this.ok), "micon_ok"),
                new MenuItemButton(LanguageHelper.GetText("button.back"), new ControlEvents.ChooseEvent(this.back), "micon_cancel"),
            ];

            Menu menu = new Menu(Vector2.UnitY * 50, this.Width, this.Height - 50, this, items.ToArray());
            this.members.Add(menu);
        }
        private void SetTooltips()
        {
            m_menuItemSoundPanningEnabled.Tooltip = "Enables or disables sound-panning, sound-panning pans sound to the left or right audio channels";
            m_menuItemSoundPanningStrength.Tooltip = "Sets the strength of sound-panning, higher strength means more noticeable sound-panning";
            m_menuItemSoundPanningScreenSpace.Tooltip = "Sets if sound-panning is calculated using the position on-screen rather than the in-world position of the sound";
            m_menuItemSoundPanningThreshold.Tooltip = "Sets the in-world threshold of sound-panning in pixels, sounds within this distance of your character will not be panned";
            m_menuItemSoundPanningDistance.Tooltip = "Sets the in-world distance of sound-panning in pixels, sounds further than this distance will be fully panned";

            m_menuItemSoundAttenuationEnabled.Tooltip = "Enables or disables sound-attenuation, sound-attenuation lowers the volume of far away sounds";
            m_menuItemSoundAttenuationMin.Tooltip = "Sets the minimum value of sound-attenuation, a lower minimum value means stronger sound-attenuation";
            m_menuItemSoundAttenuationScreenSpace.Tooltip = "Sets if sound-attenuation is calculated using the position on-screen rather than the in-world position of the sound";
            m_menuItemSoundAttenuationThreshold.Tooltip = "Sets the in-world threshold of sound-attenuation in pixels, sounds within this distance of your character will not be attenuated";
            m_menuItemSoundAttenuationDistance.Tooltip = "Sets the in-world distance of sound-attenuation in pixels, sounds further than this distance will be fully attenuated";

            m_menuItemResetToDefault.Tooltip = "Reset all values to their default values";
        }
        private void SetOriginalValues()
        {
            m_modifiedSettings = false;
            m_OriginalSoundPanningEnabled = CSettings.Get<bool>(CSettings.GetKey(CSettings.SettingKey.SoundPanningEnabled));
            m_OriginalSoundPanningStrength = CSettings.Get<float>(CSettings.GetKey(CSettings.SettingKey.SoundPanningStrength));
            m_OriginalSoundPanningScreenSpace = CSettings.Get<bool>(CSettings.GetKey(CSettings.SettingKey.SoundPanningForceScreenSpace));
            m_OriginalSoundPanningThreshold = CSettings.Get<int>(CSettings.GetKey(CSettings.SettingKey.SoundPanningInworldThreshold));
            m_OriginalSoundPanningDistance = CSettings.Get<int>(CSettings.GetKey(CSettings.SettingKey.SoundPanningInworldDistance));
            m_OriginalSoundAttenuationEnabled = CSettings.Get<bool>(CSettings.GetKey(CSettings.SettingKey.SoundAttenuationEnabled));
            m_OriginalSoundAttenuationMin = CSettings.Get<float>(CSettings.GetKey(CSettings.SettingKey.SoundAttenuationMin));
            m_OriginalSoundAttenuationScreenSpace = CSettings.Get<bool>(CSettings.GetKey(CSettings.SettingKey.SoundAttenuationForceScreenSpace));
            m_OriginalSoundAttenuationThreshold = CSettings.Get<int>(CSettings.GetKey(CSettings.SettingKey.SoundAttenuationInworldThreshold));
            m_OriginalSoundAttenuationDistance = CSettings.Get<int>(CSettings.GetKey(CSettings.SettingKey.SoundAttenuationInworldDistance));
        }
        public override void Dispose()
        {
            HookHandler.DisposeHook(m_menuItemSoundPanningEnabled); m_menuItemSoundPanningEnabledValueChanged = null;
            HookHandler.DisposeHook(m_menuItemSoundPanningStrength); m_menuItemSoundPanningStrengthValueChanged = null;
            HookHandler.DisposeHook(m_menuItemSoundPanningScreenSpace); m_menuItemSoundPanningScreenSpaceValueChanged = null;
            HookHandler.DisposeHook(m_menuItemSoundPanningThreshold); m_menuItemSoundPanningThresholdValueChanged = null;
            HookHandler.DisposeHook(m_menuItemSoundPanningDistance); m_menuItemSoundPanningDistanceValueChanged = null;
            HookHandler.DisposeHook(m_menuItemSoundAttenuationEnabled); m_menuItemSoundAttenuationEnabledValueChanged = null;
            HookHandler.DisposeHook(m_menuItemSoundAttenuationMin); m_menuItemSoundAttenuationMinValueChanged = null;
            HookHandler.DisposeHook(m_menuItemSoundAttenuationScreenSpace); m_menuItemSoundAttenuationScreenSpaceValueChanged = null;
            HookHandler.DisposeHook(m_menuItemSoundAttenuationThreshold); m_menuItemSoundAttenuationThresholdValueChanged = null;
            HookHandler.DisposeHook(m_menuItemSoundAttenuationDistance); m_menuItemSoundAttenuationDistanceValueChanged = null;

            base.Dispose();
        }
        public override void KeyPress(Keys key)
        {
            if (this.subPanel == null && key == Keys.Escape)
            {
                this.back(null);
                return;
            }
            base.KeyPress(key);
        }

        private void m_menuItemSoundPanningEnabled_ValueChanged(MenuItem sender)
        {
            m_modifiedSettings = true;
            CSettings.Set<bool>(CSettings.GetKey(CSettings.SettingKey.SoundPanningEnabled), m_menuItemSoundPanningEnabled.ValueId == 0);
        }
        private void m_menuItemSoundPanningStrength_ValueChanged(MenuItem sender)
        {
            m_modifiedSettings = true;
            CSettings.Set<float>(CSettings.GetKey(CSettings.SettingKey.SoundPanningStrength), (float)m_menuItemSoundPanningStrength.Value * 0.01f);
        }
        private void m_menuItemSoundPanningScreenSpace_ValueChanged(MenuItem sender)
        {
            m_modifiedSettings = true;
            CSettings.Set<bool>(CSettings.GetKey(CSettings.SettingKey.SoundPanningForceScreenSpace), m_menuItemSoundPanningScreenSpace.ValueId == 0);
        }
        private void m_menuItemSoundPanningThreshold_ValueChanged(MenuItem sender)
        {
            m_modifiedSettings = true;
            CSettings.Set<int>(CSettings.GetKey(CSettings.SettingKey.SoundPanningInworldThreshold), m_menuItemSoundPanningThreshold.Value);
        }
        private void m_menuItemSoundPanningDistance_ValueChanged(MenuItem sender)
        {
            m_modifiedSettings = true;
            CSettings.Set<int>(CSettings.GetKey(CSettings.SettingKey.SoundPanningInworldDistance), m_menuItemSoundPanningDistance.Value);
        }
        private void m_menuItemSoundAttenuationEnabled_ValueChanged(MenuItem sender)
        {
            m_modifiedSettings = true;
            CSettings.Set<bool>(CSettings.GetKey(CSettings.SettingKey.SoundAttenuationEnabled), m_menuItemSoundAttenuationEnabled.ValueId == 0);
        }
        private void m_menuItemSoundAttenuationMin_ValueChanged(MenuItem sender)
        {
            m_modifiedSettings = true;
            CSettings.Set<float>(CSettings.GetKey(CSettings.SettingKey.SoundAttenuationMin), (float)m_menuItemSoundAttenuationMin.Value * 0.01f);
        }
        private void m_menuItemSoundAttenuationScreenSpace_ValueChanged(MenuItem sender)
        {
            m_modifiedSettings = true;
            CSettings.Set<bool>(CSettings.GetKey(CSettings.SettingKey.SoundAttenuationForceScreenSpace), m_menuItemSoundAttenuationScreenSpace.ValueId == 0);
        }
        private void m_menuItemSoundAttenuationThreshold_ValueChanged(MenuItem sender)
        {
            m_modifiedSettings = true;
            CSettings.Set<int>(CSettings.GetKey(CSettings.SettingKey.SoundAttenuationInworldThreshold), m_menuItemSoundAttenuationThreshold.Value);
        }
        private void m_menuItemSoundAttenuationDistance_ValueChanged(MenuItem sender)
        {
            m_modifiedSettings = true;
            CSettings.Set<int>(CSettings.GetKey(CSettings.SettingKey.SoundAttenuationInworldDistance), m_menuItemSoundAttenuationDistance.Value);
        }

        private void setDefault(object sender)
        {
            CSettings.ResetToDefaults();

            m_menuItemSoundPanningEnabled.SetStartValue(CSettings.Get<bool>(CSettings.GetKey(CSettings.SettingKey.SoundPanningEnabled)) ? 0 : 1);
            m_menuItemSoundPanningScreenSpace.SetStartValue(CSettings.Get<bool>(CSettings.GetKey(CSettings.SettingKey.SoundPanningForceScreenSpace)) ? 0 : 1);
            m_menuItemSoundAttenuationEnabled.SetStartValue(CSettings.Get<bool>(CSettings.GetKey(CSettings.SettingKey.SoundAttenuationEnabled)) ? 0 : 1);
            m_menuItemSoundAttenuationScreenSpace.SetStartValue(CSettings.Get<bool>(CSettings.GetKey(CSettings.SettingKey.SoundAttenuationForceScreenSpace)) ? 0 : 1);
            m_menuItemSoundPanningEnabled.SetStartValue(CSettings.Get<bool>(CSettings.GetKey(CSettings.SettingKey.SoundPanningEnabled)) ? 0 : 1);
            m_menuItemSoundPanningStrength.SetStartValue((int)(CSettings.Get<float>(CSettings.GetKey(CSettings.SettingKey.SoundPanningStrength)) * 100f));
            m_menuItemSoundPanningThreshold.SetStartValue(CSettings.Get<int>(CSettings.GetKey(CSettings.SettingKey.SoundPanningInworldThreshold)));
            m_menuItemSoundPanningDistance.SetStartValue(CSettings.Get<int>(CSettings.GetKey(CSettings.SettingKey.SoundPanningInworldDistance)));
            m_menuItemSoundAttenuationMin.SetStartValue((int)(CSettings.Get<float>(CSettings.GetKey(CSettings.SettingKey.SoundAttenuationMin)) * 100f));
            m_menuItemSoundAttenuationThreshold.SetStartValue(CSettings.Get<int>(CSettings.GetKey(CSettings.SettingKey.SoundAttenuationInworldThreshold)));
            m_menuItemSoundAttenuationDistance.SetStartValue(CSettings.Get<int>(CSettings.GetKey(CSettings.SettingKey.SoundAttenuationInworldDistance)));
        }

        private void revert(object sender)
        {
            CSettings.Set<bool>(CSettings.GetKey(CSettings.SettingKey.SoundPanningEnabled), m_OriginalSoundPanningEnabled);
            CSettings.Set<float>(CSettings.GetKey(CSettings.SettingKey.SoundPanningStrength), m_OriginalSoundPanningStrength);
            CSettings.Set<bool>(CSettings.GetKey(CSettings.SettingKey.SoundPanningForceScreenSpace), m_OriginalSoundPanningScreenSpace);
            CSettings.Set<int>(CSettings.GetKey(CSettings.SettingKey.SoundPanningInworldThreshold), m_OriginalSoundPanningThreshold);
            CSettings.Set<int>(CSettings.GetKey(CSettings.SettingKey.SoundPanningInworldDistance), m_OriginalSoundPanningDistance);
            CSettings.Set<bool>(CSettings.GetKey(CSettings.SettingKey.SoundAttenuationEnabled), m_OriginalSoundAttenuationEnabled);
            CSettings.Set<float>(CSettings.GetKey(CSettings.SettingKey.SoundAttenuationMin), m_OriginalSoundAttenuationMin);
            CSettings.Set<bool>(CSettings.GetKey(CSettings.SettingKey.SoundAttenuationForceScreenSpace), m_OriginalSoundAttenuationScreenSpace);
            CSettings.Set<int>(CSettings.GetKey(CSettings.SettingKey.SoundAttenuationInworldThreshold), m_OriginalSoundAttenuationThreshold);
            CSettings.Set<int>(CSettings.GetKey(CSettings.SettingKey.SoundAttenuationInworldDistance), m_OriginalSoundAttenuationDistance);

            this.ParentPanel.CloseSubPanel();
        }
        private void ok(object sender)
        {
            CIni.NeedsSaving = true;
            CIni.Save();
            CIni.Refresh();

            this.ParentPanel.CloseSubPanel();
        }
        private void closeDialog(object Sender)
        {
            base.CloseSubPanel();
        }
        private void back(object sender)
        {
            if (m_modifiedSettings)
            {
                base.OpenSubPanel(new ConfirmYesNoPanel(LanguageHelper.GetText("menu.settings.confirmcancel"), LanguageHelper.GetText("general.yes"), LanguageHelper.GetText("general.no"), new ControlEvents.ChooseEvent(this.revert), new ControlEvents.ChooseEvent(this.closeDialog)));
            }
            else
            {
                this.ParentPanel.CloseSubPanel();
            }
        }
    }
}
