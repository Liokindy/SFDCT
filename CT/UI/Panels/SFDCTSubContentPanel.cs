using Microsoft.Xna.Framework.Input;
using SFD;
using SFD.MenuControls;
using SFDCT.Assets;
using SFDCT.Configuration;

namespace SFDCT.UI.Panels;

internal class SFDCTSubContentPanel : Panel
{
    private Menu m_enabledMenu;
    private Menu m_disabledMenu;
    private Menu m_menu;
    private bool m_changed = false;

    public SFDCTSubContentPanel() : base(LanguageHelper.GetText("sfdct.setting.header.subcontent"), 600, Menu.ITEM_HEIGHT * 12 + 50)
    {
        m_enabledMenu = new(new(0, 50), Width / 2, Menu.ITEM_HEIGHT * 10, this);
        m_disabledMenu = new(new(Width / 2, 50), Width / 2, Menu.ITEM_HEIGHT * 10, this);
        m_menu = new(new(0, 50 + Menu.ITEM_HEIGHT * 11), Width, Menu.ITEM_HEIGHT, this);

        m_enabledMenu.NeighborRightId = 2;
        m_enabledMenu.NeighborDownId = 0;
        m_disabledMenu.NeighborLeftId = 1;
        m_disabledMenu.NeighborDownId = 0;
        m_menu.NeighborUpId = 1;

        m_menu.Add(new MenuItemButton(LanguageHelper.GetText("button.done"), ok, MenuIcons.Ok));

        members.Add(m_menu);
        members.Add(m_enabledMenu);
        members.Add(m_disabledMenu);
        m_menu.SelectFirst();

        string[] disabledFolders = SubContentHandler.GetDisabledFolders();
        string[] enabledFolders = SubContentHandler.GetEnabledFolders();

        m_enabledMenu.Add(new MenuItemSeparator(LanguageHelper.GetText("general.on")));
        foreach (string folderName in enabledFolders)
        {
            m_enabledMenu.Add(new MenuItemButton(folderName, folder));
        }

        foreach (string folderName in SubContentHandler.GetNewFolders())
        {
            m_enabledMenu.Add(new MenuItemButton(folderName, folder));
        }

        m_disabledMenu.Add(new MenuItemSeparator(LanguageHelper.GetText("general.off")));
        foreach (string folderName in disabledFolders)
        {
            m_disabledMenu.Add(new MenuItemButton(folderName, folder));
        }
    }

    private void MoveItem(Menu menu, MenuItem item, bool up)
    {
        int index = menu.IndexOf(item);
        int otherIndex = index + (up ? -1 : 1);

        var otherItem = menu.ItemAt(otherIndex);
        if (otherItem == null) return;

        menu.Items[index] = otherItem;
        menu.Items[otherIndex] = item;
        menu.UpdatePositions();
        menu.UpdateScrollbar();
    }

    private void SwapItem(Menu fromMenu, Menu toMenu, MenuItem item)
    {
        int index = fromMenu.IndexOf(item);
        if (index == -1) return;

        fromMenu.Remove(item);
        toMenu.Add(item);

        item.Deselect();
    }

    private void folder(object obj)
    {
        if (obj is not MenuItemButton menuItem) return;

        if (menuItem.ParentMenu == m_disabledMenu)
        {
            OpenSubPanel(new SFDCTConfirmMultiplePanel(menuItem.lblName.Text,
            [
                "Back",
                "Enable"
            ], [
                (object _) =>
                {
                    CloseSubPanel();
                },
                (object _) =>
                {
                    m_changed = true;
                    SwapItem(m_disabledMenu, m_enabledMenu, menuItem);
                    CloseSubPanel();
                },
            ], [
                MenuIcons.Cancel,
                MenuIcons.Settings,
            ]));
        }
        else if (menuItem.ParentMenu == m_enabledMenu)
        {
            OpenSubPanel(new SFDCTConfirmMultiplePanel(menuItem.lblName.Text,
            [
                "Back",
                "Disable",
                "Move Up",
                "Move Down"
            ], [
                (object _) =>
                {
                    CloseSubPanel();
                },
                (object _) =>
                {
                    m_changed = true;
                    SwapItem(m_enabledMenu, m_disabledMenu, menuItem);
                    CloseSubPanel();
                },
                (object _) =>
                {
                    m_changed = true;
                    MoveItem(m_enabledMenu, menuItem, true);
                    CloseSubPanel();
                },
                (object _) =>
                {
                    m_changed = true;
                    MoveItem(m_enabledMenu, menuItem, false);
                    CloseSubPanel();
                }
            ], [
                MenuIcons.Cancel,
                MenuIcons.Settings,
                null,
                null
            ]));
        }
    }

    private void ok(object _)
    {
        if (m_changed)
        {
            string enabledFolders = "";
            foreach (var menuItem in m_enabledMenu.Items)
            {
                if (menuItem is MenuItemSeparator) continue;
                if (menuItem is not MenuItemButton menuItemButton) continue;

                enabledFolders += SubContentHandler.SUB_CONTENT_FOLDER_SEPARATOR + menuItemButton.lblName.Text;
            }
            enabledFolders += SubContentHandler.SUB_CONTENT_FOLDER_SEPARATOR;

            string disabledFolders = "";
            foreach (var menuItem in m_disabledMenu.Items)
            {
                if (menuItem is MenuItemSeparator) continue;
                if (menuItem is not MenuItemButton menuItemButton) continue;

                disabledFolders += SubContentHandler.SUB_CONTENT_FOLDER_SEPARATOR + menuItemButton.lblName.Text;
            }
            disabledFolders += SubContentHandler.SUB_CONTENT_FOLDER_SEPARATOR;

            SFDCTConfig.Set<string>(CTSettingKey.SubContentEnabledFolders, enabledFolders);
            SFDCTConfig.Set<string>(CTSettingKey.SubContentDisabledFolders, disabledFolders);
            SFDCTConfig.SaveFile();

            MessageStack.Show(LanguageHelper.GetText("menu.settings.restartrequiredmessage"), MessageStackType.Information);
        }

        ParentPanel.CloseSubPanel();
    }

    public override void KeyPress(Keys key)
    {
        if (subPanel == null && key == Keys.Escape)
        {
            ParentPanel.CloseSubPanel();
            return;
        }

        base.KeyPress(key);
    }
}
