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
        private bool m_OriginalHideFilmgrain;
        private Color m_OriginalMenuColor;
        private float m_OriginalSoundPanningStrength;
        private bool m_OriginalSoundPanningScreenSpace;
        private int m_OriginalSoundPanningThreshold;
        private int m_OriginalSoundPanningDistance;
        private bool m_OriginalMainMenuRandomTrackEnabled;
        private bool m_OriginalMainMenuBlackEnabled;
        private bool  m_OriginalSFRTeamColorsEnabled;
        private bool  m_OriginalSpectatorEnabled;
        private bool m_OriginalSpectatorOnlyModEnabled;
        private int m_OriginalSpectatorCount;
        private bool  m_OriginalSoundAttenuationEnabled;
        private float m_OriginalSoundAttenuationMin;
        private bool m_OriginalSoundAttenuationScreenSpace;
        private int m_OriginalSoundAttenuationThreshold;
        private int m_OriginalSoundAttenuationDistance;

        private MenuItemDropdown m_menuItemSoundPanningEnabled = null;
        private MenuItemText m_menuItemMenuColor = null;
        private MenuItemSlider m_menuItemSoundPanningStrength = null;
        private MenuItemDropdown m_menuItemSoundPanningScreenSpace = null;
        private MenuItemSlider m_menuItemSoundPanningThreshold = null;
        private MenuItemSlider m_menuItemSoundPanningDistance = null;

        private MenuItemDropdown m_menuItemHideFilmgrain = null;

        private MenuItemDropdown m_menuItemMainMenuRandomTrackEnabled = null;
        private MenuItemDropdown m_menuItemMainMenuBlackEnabled = null;

        private MenuItemDropdown m_menuItemSFRTeamColorsEnabled = null;

        private MenuItemDropdown m_menuItemSpectatorEnabled = null;
        private MenuItemDropdown m_menuItemSpectatorOnlyModEnabled = null;
        private MenuItemSlider m_menuItemSpectatorCount = null;

        private MenuItemDropdown m_menuItemSoundAttenuationEnabled = null;
        private MenuItemSlider m_menuItemSoundAttenuationMin = null;
        private MenuItemDropdown m_menuItemSoundAttenuationScreenSpace = null;
        private MenuItemSlider m_menuItemSoundAttenuationThreshold = null;
        private MenuItemSlider m_menuItemSoundAttenuationDistance = null;

        private MenuItemValueChangedEvent m_menuItemHideFilmgrainValueChanged = null;
        private MenuItemValueChangedEvent m_menuItemSoundPanningEnabledValueChanged = null;
        private MenuItemValueChangedEvent m_menuItemSoundPanningStrengthValueChanged = null;
        private MenuItemValueChangedEvent m_menuItemSoundPanningScreenSpaceValueChanged = null;
        private MenuItemValueChangedEvent m_menuItemSoundPanningThresholdValueChanged = null;
        private MenuItemValueChangedEvent m_menuItemSoundPanningDistanceValueChanged = null;
        private MenuItemValueChangedEvent m_menuItemMainMenuRandomTrackEnabledValueChanged = null;
        private MenuItemValueChangedEvent m_menuItemMainMenuBlackEnabledValueChanged = null;
        private MenuItemValueChangedEvent m_menuItemSFRTeamColorsEnabledValueChanged = null;
        private MenuItemValueChangedEvent m_menuItemSpectatorEnabledValueChanged = null;
        private MenuItemValueChangedEvent m_menuItemSpectatorOnlyModEnabledValueChanged = null;
        private MenuItemValueChangedEvent m_menuItemSpectatorCountValueChanged = null;
        private MenuItemValueChangedEvent m_menuItemSoundAttenuationEnabledValueChanged = null;
        private MenuItemValueChangedEvent m_menuItemSoundAttenuationMinValueChanged = null;
        private MenuItemValueChangedEvent m_menuItemSoundAttenuationScreenSpaceValueChanged = null;
        private MenuItemValueChangedEvent m_menuItemSoundAttenuationThresholdValueChanged = null;
        private MenuItemValueChangedEvent m_menuItemSoundAttenuationDistanceValueChanged = null;
        private TextValidationEvent m_menuItemMenuColorTextSetValidationItem = null;

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

            Color color = CSettings.Get<Color>(CSettings.GetKey(CSettings.SettingKey.MenuColor));
            string hexColor = color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2") + (color.A != 255 ? color.A.ToString("X2") : "");
            m_menuItemMenuColor = new MenuItemText("UI COLOR", hexColor);

            m_menuItemMainMenuRandomTrackEnabled = new MenuItemDropdown("USE RANDOM TRACK FOR MENU", [
                LanguageHelper.GetText("general.on"),
                LanguageHelper.GetText("general.off"),
            ]);
            m_menuItemMainMenuRandomTrackEnabled.SetStartValue(CSettings.Get<bool>(CSettings.GetKey(CSettings.SettingKey.MainMenuTrackRandom)) ? 0 : 1);
            m_menuItemMainMenuBlackEnabled = new MenuItemDropdown("USE BLACK FOR MENU BACKGROUND", [
                LanguageHelper.GetText("general.on"),
                LanguageHelper.GetText("general.off"),
            ]);
            m_menuItemMainMenuBlackEnabled.SetStartValue(CSettings.Get<bool>(CSettings.GetKey(CSettings.SettingKey.MainMenuBgUseBlack)) ? 0 : 1);
            
            m_menuItemSpectatorEnabled = new MenuItemDropdown("ALLOW SPECTATORS", [
                LanguageHelper.GetText("general.on"),
                LanguageHelper.GetText("general.off"),
            ]);
            m_menuItemSpectatorEnabled.SetStartValue(CSettings.Get<bool>(CSettings.GetKey(CSettings.SettingKey.AllowSpectators)) ? 0 : 1);
            
            m_menuItemSpectatorOnlyModEnabled = new MenuItemDropdown("ONLY ALLOW MODERATORS", [
                LanguageHelper.GetText("general.on"),
                LanguageHelper.GetText("general.off"),
            ]);
            m_menuItemSpectatorOnlyModEnabled.SetStartValue(CSettings.Get<bool>(CSettings.GetKey(CSettings.SettingKey.AllowSpectatorsOnlyModerators)) ? 0 : 1);
            m_menuItemSpectatorCount = new MenuItemSlider("MAXIMUM SPECTATOR COUNT", CSettings.Get<int>(CSettings.GetKey(CSettings.SettingKey.AllowSpectatorsCount)), CSettings.GetLimit<int>(CSettings.GetKey(CSettings.SettingKey.AllowSpectatorsCount)), CSettings.GetLimit<int>(CSettings.GetKey(CSettings.SettingKey.AllowSpectatorsCount), true));
            m_menuItemSFRTeamColorsEnabled = new MenuItemDropdown("USE SFR TEAM COLORS", [
                LanguageHelper.GetText("general.on"),
                LanguageHelper.GetText("general.off"),
            ]);
            m_menuItemSFRTeamColorsEnabled.SetStartValue(CSettings.Get<bool>(CSettings.GetKey(CSettings.SettingKey.UseSfrColorsForTeam5Team6)) ? 0 : 1);
            m_menuItemHideFilmgrain = new MenuItemDropdown("HIDE FILMGRAIN", [
                LanguageHelper.GetText("general.yes"),
                LanguageHelper.GetText("general.no"),
            ]);
            m_menuItemHideFilmgrain.SetStartValue(CSettings.Get<bool>(CSettings.GetKey(CSettings.SettingKey.HideFilmgrain)) ? 0 : 1);

            m_menuItemHideFilmgrainValueChanged += m_menuItemHideFilmgrain_ValueChanged; HookHandler.Hook(m_menuItemHideFilmgrain, m_menuItemHideFilmgrainValueChanged);
            m_menuItemSpectatorEnabledValueChanged += m_menuItemSpectatorEnabled_ValueChanged; HookHandler.Hook(m_menuItemSpectatorEnabled, m_menuItemSpectatorEnabledValueChanged);
            m_menuItemSoundPanningEnabledValueChanged += m_menuItemSoundPanningEnabled_ValueChanged; HookHandler.Hook(m_menuItemSoundPanningEnabled, m_menuItemSoundPanningEnabledValueChanged);
            m_menuItemSoundPanningStrengthValueChanged += m_menuItemSoundPanningStrength_ValueChanged; HookHandler.Hook(m_menuItemSoundPanningStrength, m_menuItemSoundPanningStrengthValueChanged);
            m_menuItemSoundPanningScreenSpaceValueChanged += m_menuItemSoundPanningScreenSpace_ValueChanged; HookHandler.Hook(m_menuItemSoundPanningScreenSpace, m_menuItemSoundPanningScreenSpaceValueChanged);
            m_menuItemSoundPanningThresholdValueChanged += m_menuItemSoundPanningThreshold_ValueChanged; HookHandler.Hook(m_menuItemSoundPanningThreshold, m_menuItemSoundPanningThresholdValueChanged);
            m_menuItemSoundPanningDistanceValueChanged += m_menuItemSoundPanningDistance_ValueChanged; HookHandler.Hook(m_menuItemSoundPanningDistance, m_menuItemSoundPanningDistanceValueChanged);
            m_menuItemMainMenuRandomTrackEnabledValueChanged += m_menuItemMainMenuRandomTrackEnabled_ValueChanged; HookHandler.Hook(m_menuItemMainMenuRandomTrackEnabled, m_menuItemMainMenuRandomTrackEnabledValueChanged);
            m_menuItemMainMenuBlackEnabledValueChanged += m_menuItemMainMenuBlackEnabled_ValueChanged; HookHandler.Hook(m_menuItemMainMenuBlackEnabled, m_menuItemMainMenuBlackEnabledValueChanged);
            m_menuItemSFRTeamColorsEnabledValueChanged += m_menuItemSFRTeamColorsEnabled_ValueChanged; HookHandler.Hook(m_menuItemSFRTeamColorsEnabled, m_menuItemSFRTeamColorsEnabledValueChanged);
            m_menuItemSpectatorEnabledValueChanged += m_menuItemSpectatorEnabled_ValueChanged; HookHandler.Hook(m_menuItemSpectatorEnabled, m_menuItemSpectatorEnabledValueChanged);
            m_menuItemSpectatorOnlyModEnabledValueChanged += m_menuItemSpectatorOnlyModEnabled_ValueChanged; HookHandler.Hook(m_menuItemSpectatorOnlyModEnabled, m_menuItemSpectatorOnlyModEnabledValueChanged);
            m_menuItemSpectatorCountValueChanged += m_menuItemSpectatorCount_ValueChanged; HookHandler.Hook(m_menuItemSpectatorCount, m_menuItemSpectatorCountValueChanged);
            m_menuItemSoundAttenuationEnabledValueChanged += m_menuItemSoundAttenuationEnabled_ValueChanged; HookHandler.Hook(m_menuItemSoundAttenuationEnabled, m_menuItemSoundAttenuationEnabledValueChanged);
            m_menuItemSoundAttenuationMinValueChanged += m_menuItemSoundAttenuationMin_ValueChanged; HookHandler.Hook(m_menuItemSoundAttenuationMin, m_menuItemSoundAttenuationMinValueChanged);
            m_menuItemSoundAttenuationScreenSpaceValueChanged += m_menuItemSoundAttenuationScreenSpace_ValueChanged; HookHandler.Hook(m_menuItemSoundAttenuationScreenSpace, m_menuItemSoundAttenuationScreenSpaceValueChanged);
            m_menuItemSoundAttenuationThresholdValueChanged += m_menuItemSoundAttenuationThreshold_ValueChanged; HookHandler.Hook(m_menuItemSoundAttenuationThreshold, m_menuItemSoundAttenuationThresholdValueChanged);
            m_menuItemSoundAttenuationDistanceValueChanged += m_menuItemSoundAttenuationDistance_ValueChanged; HookHandler.Hook(m_menuItemSoundAttenuationDistance, m_menuItemSoundAttenuationDistanceValueChanged);
            m_menuItemMenuColorTextSetValidationItem += m_menuItemMenuColor_TextSetValidationItem; HookHandler.Hook(m_menuItemMenuColor.TextSetValidationItem, m_menuItemMenuColorTextSetValidationItem);

            List <MenuItem> items = [
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
                m_menuItemMenuColor,
                m_menuItemMainMenuRandomTrackEnabled,
                m_menuItemMainMenuBlackEnabled,
                new MenuItemSeparator("SPECTATORS"),
                m_menuItemSpectatorEnabled,
                m_menuItemSpectatorOnlyModEnabled,
                m_menuItemSpectatorCount,
                new MenuItemSeparator("MISC"),
                m_menuItemSFRTeamColorsEnabled,
                m_menuItemHideFilmgrain,
                new MenuItemSeparator(""),
                new MenuItemButton("RESET TO DEFAULT", new ControlEvents.ChooseEvent(this.setDefault), "micon_settings"),
                new MenuItemButton(LanguageHelper.GetText("button.done"), new ControlEvents.ChooseEvent(this.ok), "micon_ok"),
                new MenuItemButton(LanguageHelper.GetText("button.back"), new ControlEvents.ChooseEvent(this.back), "micon_cancel"),
            ];

            Menu menu = new Menu(Vector2.UnitY * 50, this.Width, this.Height - 50, this, items.ToArray());
            this.members.Add(menu);
        }
        private void SetOriginalValues()
        {
            m_modifiedSettings = false;
            m_OriginalHideFilmgrain = CSettings.Get<bool>(CSettings.GetKey(CSettings.SettingKey.HideFilmgrain));
            m_OriginalSoundPanningEnabled = CSettings.Get<bool>(CSettings.GetKey(CSettings.SettingKey.SoundPanningEnabled));
            m_OriginalMenuColor = CSettings.Get<Color>(CSettings.GetKey(CSettings.SettingKey.MenuColor));
            m_OriginalSoundPanningStrength = CSettings.Get<float>(CSettings.GetKey(CSettings.SettingKey.SoundPanningStrength));
            m_OriginalSoundPanningScreenSpace = CSettings.Get<bool>(CSettings.GetKey(CSettings.SettingKey.SoundPanningForceScreenSpace));
            m_OriginalSoundPanningThreshold = CSettings.Get<int>(CSettings.GetKey(CSettings.SettingKey.SoundPanningInworldThreshold));
            m_OriginalSoundPanningDistance = CSettings.Get<int>(CSettings.GetKey(CSettings.SettingKey.SoundPanningInworldDistance));
            m_OriginalMainMenuRandomTrackEnabled = CSettings.Get<bool>(CSettings.GetKey(CSettings.SettingKey.MainMenuTrackRandom));
            m_OriginalMainMenuBlackEnabled = CSettings.Get<bool>(CSettings.GetKey(CSettings.SettingKey.MainMenuBgUseBlack));
            m_OriginalSFRTeamColorsEnabled = CSettings.Get<bool>(CSettings.GetKey(CSettings.SettingKey.UseSfrColorsForTeam5Team6));
            m_OriginalSpectatorEnabled = CSettings.Get<bool>(CSettings.GetKey(CSettings.SettingKey.AllowSpectators));
            m_OriginalSpectatorOnlyModEnabled = CSettings.Get<bool>(CSettings.GetKey(CSettings.SettingKey.AllowSpectatorsOnlyModerators));
            m_OriginalSpectatorCount = CSettings.Get<int>(CSettings.GetKey(CSettings.SettingKey.AllowSpectatorsCount));
            m_OriginalSoundAttenuationEnabled = CSettings.Get<bool>(CSettings.GetKey(CSettings.SettingKey.SoundAttenuationEnabled));
            m_OriginalSoundAttenuationMin = CSettings.Get<float>(CSettings.GetKey(CSettings.SettingKey.SoundAttenuationMin));
            m_OriginalSoundAttenuationScreenSpace = CSettings.Get<bool>(CSettings.GetKey(CSettings.SettingKey.SoundAttenuationForceScreenSpace));
            m_OriginalSoundAttenuationThreshold = CSettings.Get<int>(CSettings.GetKey(CSettings.SettingKey.SoundAttenuationInworldThreshold));
            m_OriginalSoundAttenuationDistance = CSettings.Get<int>(CSettings.GetKey(CSettings.SettingKey.SoundAttenuationInworldDistance));
        }
        public override void Dispose()
        {
            HookHandler.DisposeHook(m_menuItemHideFilmgrain); m_menuItemHideFilmgrainValueChanged = null;
            HookHandler.DisposeHook(m_menuItemSoundPanningEnabled); m_menuItemSoundPanningEnabledValueChanged = null;
            HookHandler.DisposeHook(m_menuItemSoundPanningStrength); m_menuItemSoundPanningStrengthValueChanged = null;
            HookHandler.DisposeHook(m_menuItemSoundPanningScreenSpace); m_menuItemSoundPanningScreenSpaceValueChanged = null;
            HookHandler.DisposeHook(m_menuItemSoundPanningThreshold); m_menuItemSoundPanningThresholdValueChanged = null;
            HookHandler.DisposeHook(m_menuItemSoundPanningDistance); m_menuItemSoundPanningDistanceValueChanged = null;
            HookHandler.DisposeHook(m_menuItemMainMenuRandomTrackEnabled); m_menuItemMainMenuRandomTrackEnabledValueChanged = null;
            HookHandler.DisposeHook(m_menuItemMainMenuBlackEnabled); m_menuItemMainMenuBlackEnabledValueChanged = null;
            HookHandler.DisposeHook(m_menuItemSFRTeamColorsEnabled); m_menuItemSFRTeamColorsEnabledValueChanged = null;
            HookHandler.DisposeHook(m_menuItemSpectatorEnabled); m_menuItemSpectatorEnabledValueChanged = null;
            HookHandler.DisposeHook(m_menuItemSpectatorOnlyModEnabled); m_menuItemSpectatorOnlyModEnabledValueChanged = null;
            HookHandler.DisposeHook(m_menuItemSpectatorCount); m_menuItemSpectatorCountValueChanged = null;
            HookHandler.DisposeHook(m_menuItemSoundAttenuationEnabled); m_menuItemSoundAttenuationEnabledValueChanged = null;
            HookHandler.DisposeHook(m_menuItemSoundAttenuationMin); m_menuItemSoundAttenuationMinValueChanged = null;
            HookHandler.DisposeHook(m_menuItemSoundAttenuationScreenSpace); m_menuItemSoundAttenuationScreenSpaceValueChanged = null;
            HookHandler.DisposeHook(m_menuItemSoundAttenuationThreshold); m_menuItemSoundAttenuationThresholdValueChanged = null;
            HookHandler.DisposeHook(m_menuItemSoundAttenuationDistance); m_menuItemSoundAttenuationDistanceValueChanged = null;
            HookHandler.DisposeHook(m_menuItemMenuColor); m_menuItemMenuColorTextSetValidationItem = null;

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
        
        private void m_menuItemMenuColor_TextSetValidationItem(string textToValidate, TextValidationEventArgs e)
        {
            textToValidate = textToValidate.Replace("#", "");
            e.Invalid = textToValidate.Length != 6 && textToValidate.Length != 8;

            if (e.Invalid)
            {
                MessageStack.Show("Invalid HEX color format", MessageStackType.Information);
            }
            else
            {
                string r = textToValidate.Substring(0, 2);
                string g = textToValidate.Substring(2, 2);
                string b = textToValidate.Substring(4, 2);
                string a = textToValidate.Length == 8 ? textToValidate.Substring(6, 2) : "FF";

                byte cR;
                byte cG;
                byte cB;
                byte cA;

                Color c;
                if (byte.TryParse(r, NumberStyles.HexNumber, NumberFormatInfo.InvariantInfo, out cR) && byte.TryParse(g, NumberStyles.HexNumber, NumberFormatInfo.InvariantInfo, out cG) && byte.TryParse(b, NumberStyles.HexNumber, NumberFormatInfo.InvariantInfo, out cB))
                {
                    if (!byte.TryParse(a, NumberStyles.HexNumber, NumberFormatInfo.InvariantInfo, out cA))
                    {
                        cA = 255;
                    }

                    c = new Color(cR, cG, cB, cA);
                    textToValidate = c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2") + (c.A != 255 ? c.A.ToString("X2") : "");

                    m_modifiedSettings = true;
                    Constants.COLORS.MENU_BLUE = c;
                    CSettings.Set<Color>(CSettings.GetKey(CSettings.SettingKey.MenuColor), c);
                }
                else
                {
                    MessageStack.Show("Failed parsing HEX color", MessageStackType.Information);
                }
            }
        }

        private void m_menuItemHideFilmgrain_ValueChanged(MenuItem sender)
        {
            m_modifiedSettings = true;
            CSettings.Set<bool>(CSettings.GetKey(CSettings.SettingKey.HideFilmgrain), m_menuItemHideFilmgrain.ValueId == 0);
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
        private void m_menuItemMainMenuRandomTrackEnabled_ValueChanged(MenuItem sender)
        {
            m_modifiedSettings = true;
            CSettings.Set<bool>(CSettings.GetKey(CSettings.SettingKey.MainMenuTrackRandom), m_menuItemMainMenuRandomTrackEnabled.ValueId == 0);
        }
        private void m_menuItemMainMenuBlackEnabled_ValueChanged(MenuItem sender)
        {
            m_modifiedSettings = true;
            CSettings.Set<bool>(CSettings.GetKey(CSettings.SettingKey.MainMenuBgUseBlack), m_menuItemMainMenuBlackEnabled.ValueId == 0);
        }
        private void m_menuItemSFRTeamColorsEnabled_ValueChanged(MenuItem sender)
        {
            m_modifiedSettings = true;
            CSettings.Set<bool>(CSettings.GetKey(CSettings.SettingKey.UseSfrColorsForTeam5Team6), m_menuItemSFRTeamColorsEnabled.ValueId == 0);
        }
        private void m_menuItemSpectatorEnabled_ValueChanged(MenuItem sender)
        {
            m_modifiedSettings = true;
            CSettings.Set<bool>(CSettings.GetKey(CSettings.SettingKey.AllowSpectators), m_menuItemSpectatorEnabled.ValueId == 0);
        }
        private void m_menuItemSpectatorOnlyModEnabled_ValueChanged(MenuItem sender)
        {
            m_modifiedSettings = true;
            CSettings.Set<bool>(CSettings.GetKey(CSettings.SettingKey.AllowSpectatorsOnlyModerators), m_menuItemSpectatorOnlyModEnabled.ValueId == 0);
        }
        private void m_menuItemSpectatorCount_ValueChanged(MenuItem sender)
        {
            m_modifiedSettings = true;
            CSettings.Set<int>(CSettings.GetKey(CSettings.SettingKey.AllowSpectatorsCount), m_menuItemSpectatorCount.Value);
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

            m_menuItemHideFilmgrain.SetStartValue(CSettings.Get<bool>(CSettings.GetKey(CSettings.SettingKey.HideFilmgrain)) ? 0 : 1);
            m_menuItemSoundPanningEnabled.SetStartValue(CSettings.Get<bool>(CSettings.GetKey(CSettings.SettingKey.SoundPanningEnabled)) ? 0 : 1);
            m_menuItemSoundPanningScreenSpace.SetStartValue(CSettings.Get<bool>(CSettings.GetKey(CSettings.SettingKey.SoundPanningForceScreenSpace)) ? 0 : 1);
            m_menuItemSoundAttenuationEnabled.SetStartValue(CSettings.Get<bool>(CSettings.GetKey(CSettings.SettingKey.SoundAttenuationEnabled)) ? 0 : 1);
            m_menuItemSoundAttenuationScreenSpace.SetStartValue(CSettings.Get<bool>(CSettings.GetKey(CSettings.SettingKey.SoundAttenuationForceScreenSpace)) ? 0 : 1);
            m_menuItemMainMenuRandomTrackEnabled.SetStartValue(CSettings.Get<bool>(CSettings.GetKey(CSettings.SettingKey.MainMenuTrackRandom)) ? 0 : 1);
            m_menuItemMainMenuBlackEnabled.SetStartValue(CSettings.Get<bool>(CSettings.GetKey(CSettings.SettingKey.MainMenuBgUseBlack)) ? 0 : 1);
            m_menuItemSpectatorEnabled.SetStartValue(CSettings.Get<bool>(CSettings.GetKey(CSettings.SettingKey.AllowSpectators)) ? 0 : 1);
            m_menuItemSpectatorOnlyModEnabled.SetStartValue(CSettings.Get<bool>(CSettings.GetKey(CSettings.SettingKey.AllowSpectatorsOnlyModerators)) ? 0 : 1);
            m_menuItemSFRTeamColorsEnabled.SetStartValue(CSettings.Get<bool>(CSettings.GetKey(CSettings.SettingKey.UseSfrColorsForTeam5Team6)) ? 0 : 1);
            m_menuItemSoundPanningEnabled.SetStartValue(CSettings.Get<bool>(CSettings.GetKey(CSettings.SettingKey.SoundPanningEnabled)) ? 0 : 1);

            Color color = CSettings.Get<Color>(CSettings.GetKey(CSettings.SettingKey.MenuColor));
            string hexColor = color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2") + (color.A != 255 ? color.A.ToString("X2") : "");
            m_menuItemMenuColor.SetValue(hexColor);
            m_menuItemMenuColor.Focus = Focus.None;

            m_menuItemSoundPanningStrength.SetStartValue((int)(CSettings.Get<float>(CSettings.GetKey(CSettings.SettingKey.SoundPanningStrength)) * 100f));
            m_menuItemSoundPanningThreshold.SetStartValue(CSettings.Get<int>(CSettings.GetKey(CSettings.SettingKey.SoundPanningInworldThreshold)));
            m_menuItemSoundPanningDistance.SetStartValue(CSettings.Get<int>(CSettings.GetKey(CSettings.SettingKey.SoundPanningInworldDistance)));
            m_menuItemSoundAttenuationMin.SetStartValue((int)(CSettings.Get<float>(CSettings.GetKey(CSettings.SettingKey.SoundAttenuationMin)) * 100f));
            m_menuItemSoundAttenuationThreshold.SetStartValue(CSettings.Get<int>(CSettings.GetKey(CSettings.SettingKey.SoundAttenuationInworldThreshold)));
            m_menuItemSoundAttenuationDistance.SetStartValue(CSettings.Get<int>(CSettings.GetKey(CSettings.SettingKey.SoundAttenuationInworldDistance)));
            m_menuItemSpectatorCount.SetStartValue(CSettings.Get<int>(CSettings.GetKey(CSettings.SettingKey.AllowSpectatorsCount)));
        }

        private void revert(object sender)
        {
            CSettings.Set<bool>(CSettings.GetKey(CSettings.SettingKey.SoundPanningEnabled), m_OriginalSoundPanningEnabled);
            CSettings.Set<Color>(CSettings.GetKey(CSettings.SettingKey.MenuColor), m_OriginalMenuColor);
            CSettings.Set<float>(CSettings.GetKey(CSettings.SettingKey.SoundPanningStrength), m_OriginalSoundPanningStrength);
            CSettings.Set<bool>(CSettings.GetKey(CSettings.SettingKey.SoundPanningForceScreenSpace), m_OriginalSoundPanningScreenSpace);
            CSettings.Set<int>(CSettings.GetKey(CSettings.SettingKey.SoundPanningInworldThreshold), m_OriginalSoundPanningThreshold);
            CSettings.Set<int>(CSettings.GetKey(CSettings.SettingKey.SoundPanningInworldDistance), m_OriginalSoundPanningDistance);
            CSettings.Set<bool>(CSettings.GetKey(CSettings.SettingKey.MainMenuTrackRandom), m_OriginalMainMenuRandomTrackEnabled);
            CSettings.Set<bool>(CSettings.GetKey(CSettings.SettingKey.MainMenuBgUseBlack), m_OriginalMainMenuBlackEnabled);
            CSettings.Set<bool>(CSettings.GetKey(CSettings.SettingKey.UseSfrColorsForTeam5Team6), m_OriginalSFRTeamColorsEnabled);
            CSettings.Set<bool>(CSettings.GetKey(CSettings.SettingKey.AllowSpectators), m_OriginalSpectatorEnabled);
            CSettings.Set<bool>(CSettings.GetKey(CSettings.SettingKey.AllowSpectatorsOnlyModerators), m_OriginalSpectatorOnlyModEnabled);
            CSettings.Set<int>(CSettings.GetKey(CSettings.SettingKey.AllowSpectatorsCount), m_OriginalSpectatorCount);
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
