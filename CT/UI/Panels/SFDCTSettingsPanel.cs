using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SFD;
using SFD.MenuControls;
using SFDCT.Configuration;
using SFDCT.Misc;

namespace SFDCT.UI.Panels
{
    public class SFDCTSettingsPanel : Panel
    {
        private Dictionary<string, object> m_originalValues;
        private Dictionary<string, MenuItem> m_menuItems;
        private Dictionary<string, Action> m_menuItemsActions;
        private MenuItem[] m_menuItemArray;

        public SFDCTSettingsPanel() : base("CUSTOM SETTINGS", 600, 500)
        {
            m_menuItems = new(Settings.List.Count);
            m_originalValues = new(Settings.List.Count);
            m_menuItemsActions = new(Settings.List.Count);
            m_menuItemArray = new MenuItem[Settings.List.Count + 2];

            int i = 0;
            foreach (var settingPair in Settings.List)
            {
                switch (settingPair.Value.Type)
                {
                    default:
                        m_menuItems.Add(settingPair.Key, new MenuItemSeparator(settingPair.Key));
                        ((MenuItemSeparator)m_menuItems[settingPair.Key]).lblName.Color = Color.Red * 0.5f;

                        m_menuItemArray[i] = m_menuItems[settingPair.Key];
                        break;
                    case IniSettingType.Bool:
                        m_menuItems.Add(settingPair.Key, new MenuItemDropdown(settingPair.Key, [LanguageHelper.GetText("general.on"),LanguageHelper.GetText("general.off")]));
                        ((MenuItemDropdown)m_menuItems[settingPair.Key]).DropdownItemVisibleCount = 2;
                        ((MenuItemDropdown)m_menuItems[settingPair.Key]).SetStartValue((bool)settingPair.Value.Get() ? 0 : 1);
                        m_originalValues.Add(settingPair.Key, settingPair.Value.Get());
                        m_menuItemsActions.Add(settingPair.Key, () =>
                        {
                            Settings.Set<bool>(settingPair.Key, ((MenuItemDropdown)m_menuItems[settingPair.Key]).ValueId == 0);
                        });
                        HookHandler.Hook(m_menuItems[settingPair.Key], m_menuItemsActions[settingPair.Key]);

                        m_menuItemArray[i] = m_menuItems[settingPair.Key];
                        break;
                    case IniSettingType.Int:
                    case IniSettingType.Float:
                        int value = 0;
                        int minValue = 0;
                        int maxValue = 0;
                        int floatScale = 100;

                        if (settingPair.Value.Type == IniSettingType.Int)
                        {
                            value = (int)settingPair.Value.Value;
                            minValue = (int)settingPair.Value.MinValue;
                            maxValue = (int)settingPair.Value.MaxValue;
                        }
                        else
                        {
                            value = (int)((float)settingPair.Value.Value * (float)floatScale);
                            minValue = (int)((float)settingPair.Value.MinValue * (float)floatScale);
                            maxValue = (int)((float)settingPair.Value.MaxValue * (float)floatScale);
                        }

                        m_menuItems.Add(settingPair.Key, new MenuItemSlider(settingPair.Key, value, minValue, maxValue, 1));
                        ((MenuItemSlider)m_menuItems[settingPair.Key]).SetStartValue(value);
                        
                        m_originalValues.Add(settingPair.Key, settingPair.Value.Get());
                        m_menuItemsActions.Add(settingPair.Key, () =>
                        {
                            if (settingPair.Value.Type == IniSettingType.Int)
                            {
                                Settings.Set<int>(settingPair.Key, ((MenuItemSlider)(m_menuItems[settingPair.Key])).Value);
                            }
                            else
                            {
                                Settings.Set<float>(settingPair.Key, (float)((MenuItemSlider)(m_menuItems[settingPair.Key])).Value / (float)floatScale);
                            }
                        });
                        HookHandler.Hook(m_menuItems[settingPair.Key], m_menuItemsActions[settingPair.Key]);

                        m_menuItemArray[i] = m_menuItems[settingPair.Key];
                        break;
                }

                i++;
            }

            m_menuItemArray[i] = new MenuItemButton(LanguageHelper.GetText("button.done"), new ControlEvents.ChooseEvent(this.ok), "micon_ok");
            i++;
            m_menuItemArray[i] = new MenuItemButton(LanguageHelper.GetText("button.back"), new ControlEvents.ChooseEvent(this.back), "micon_cancel");

            Menu menu = new Menu(Vector2.UnitY * 50, this.Width, this.Height - 50, this, m_menuItemArray);
            this.members.Add(menu);
        }

        public override void Dispose()
        {
            foreach (var menuItem in m_menuItems)
            {
                HookHandler.DisposeHook(menuItem);
            }

            m_menuItems.Clear();
            m_menuItemArray = null;
            m_menuItemsActions.Clear();

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

        private void ok(object sender)
        {
            IniFile.NeedsSaving = true;
            IniFile.Save();
            IniFile.Refresh();

            this.ParentPanel.CloseSubPanel();
        }

        private void back(object sender)
        {
            this.ParentPanel.CloseSubPanel();
        }
    }
}
