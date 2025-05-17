using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SFD;
using SFD.MenuControls;
using SFDCT.Configuration;
using SFDCT.Misc;

namespace SFDCT.UI.Panels;

internal class SFDCTSettingsPanel : Panel
{
    private Dictionary<string, object> m_originalValues;
    private Dictionary<string, MenuItem> m_menuItems;
    private Dictionary<string, Action> m_menuItemsActions;

    public SFDCTSettingsPanel() : base("CUSTOM SETTINGS", 500, 500)
    {
        m_menuItems = new(Settings.List.Count);
        m_originalValues = new(Settings.List.Count);
        m_menuItemsActions = new(Settings.List.Count);
        List<MenuItem> m_menuItemList = new();

        int i = 0;
        string categoryName = string.Empty;
        foreach (var settingPair in Settings.List)
        {
            if (!categoryName.Equals(settingPair.Value.Category, StringComparison.OrdinalIgnoreCase))
            {
                categoryName = settingPair.Value.Category;
                m_menuItemList.Add(new MenuItemSeparator(categoryName));
            }

            switch (settingPair.Value.Type)
            {
                default:
                    m_menuItems.Add(settingPair.Key, new MenuItemSeparator(settingPair.Value.Name));
                    ((MenuItemSeparator)m_menuItems[settingPair.Key]).lblName.Color = Color.Red * 0.5f;
                    ((MenuItemSeparator)m_menuItems[settingPair.Key]).Tooltip = settingPair.Value.Help;

                    m_menuItemList.Add(m_menuItems[settingPair.Key]);
                    break;
                case IniSettingType.Bool:
                    m_menuItems.Add(settingPair.Key, new MenuItemDropdown(settingPair.Value.Name, [LanguageHelper.GetText("general.on"),LanguageHelper.GetText("general.off")]));
                    ((MenuItemDropdown)m_menuItems[settingPair.Key]).DropdownItemVisibleCount = 2;
                    ((MenuItemDropdown)m_menuItems[settingPair.Key]).SetStartValue((bool)settingPair.Value.Get() ? 0 : 1);
                    ((MenuItemDropdown)m_menuItems[settingPair.Key]).Tooltip = settingPair.Value.Help;
                    
                    m_originalValues.Add(settingPair.Key, settingPair.Value.Get());

                    m_menuItemsActions.Add(settingPair.Key, () =>
                    {
                        Settings.Set<bool>(settingPair.Key, ((MenuItemDropdown)m_menuItems[settingPair.Key]).ValueId == 0);
                    });
                    HookHandler.Hook(m_menuItems[settingPair.Key], m_menuItemsActions[settingPair.Key]);

                    m_menuItemList.Add(m_menuItems[settingPair.Key]);
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

                    m_menuItems.Add(settingPair.Key, new MenuItemSlider(settingPair.Value.Name, value, minValue, maxValue, 1));
                    ((MenuItemSlider)m_menuItems[settingPair.Key]).SetStartValue(value);
                    ((MenuItemSlider)m_menuItems[settingPair.Key]).Tooltip = settingPair.Value.Help;

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

                    m_menuItemList.Add(m_menuItems[settingPair.Key]);
                    break;
            }

            i++;
        }

        m_menuItemList.Add(new MenuItemSeparator(string.Empty));
        m_menuItemList.Add(new MenuItemButton("Reset", new ControlEvents.ChooseEvent(this.reset), MenuIcons.Settings));
        m_menuItemList.Add(new MenuItemButton(LanguageHelper.GetText("button.done"), new ControlEvents.ChooseEvent(this.ok), MenuIcons.Ok));
        m_menuItemList.Add(new MenuItemButton(LanguageHelper.GetText("button.back"), new ControlEvents.ChooseEvent(this.back), MenuIcons.Cancel));

        Menu menu = new Menu(Vector2.UnitY * 50, this.Width, this.Height - 50, this, m_menuItemList.ToArray());
        this.members.Add(menu);
    }

    public override void Dispose()
    {
        foreach (var menuItem in m_menuItems)
        {
            HookHandler.DisposeHook(menuItem);
        }

        m_menuItems.Clear();
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
        IniFile.Save(true);
        IniFile.Refresh();

        this.ParentPanel.CloseSubPanel();
    }

    private void reset(object sender)
    {
        this.OpenSubPanel(new ConfirmYesNoPanel("Reset to defaults?", LanguageHelper.GetText("general.yes"), LanguageHelper.GetText("general.no"),
        (object sender) =>
        {
            foreach (var setting in Settings.List)
            {
                Settings.Set(setting.Key, Settings.Get<object>(setting.Key, true));
            }

            this.CloseSubPanel();
            this.ok(sender);
        }, (object sender) => 
        {
            this.CloseSubPanel();
        }));
    }

    private void back(object sender)
    {
        foreach (var menuItem in m_menuItems)
        {
            Settings.Set(menuItem.Key, m_originalValues[menuItem.Key]);
        }

        this.ParentPanel.CloseSubPanel();
    }
}
