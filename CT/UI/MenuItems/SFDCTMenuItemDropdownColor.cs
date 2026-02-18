using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SFD;
using SFD.MenuControls;
using SFDCT.UI.Panels;
using System;
using System.Linq;

namespace SFDCT.UI.MenuItems;

internal class SFDCTMenuItemDropdownColor : MenuItemButton
{
    internal SFDCTMenuItemDropdownColor(string name, Color color, params Color[] availableColors) : base(name, null)
    {
        ChooseEvent = (ControlEvents.ChooseEvent)Delegate.Combine(ChooseEvent, new ControlEvents.ChooseEvent(openDropdownColor));

        m_color = color;
        m_availableColors = availableColors;
    }

    public Color Color
    {
        get { return m_color; }
    }

    public Color[] Colors
    {
        get { return m_availableColors; }
    }

    public override void Draw(SpriteBatch batch, float elapsed)
    {
        base.Draw(batch, elapsed);

        var colorRectangle = new Rectangle((int)Position.X + Width - 16 - 8, (int)Position.Y + Menu.ITEM_HEIGHT / 2 - 8, 16, 16);

        batch.Draw(Constants.WhitePixel, colorRectangle, Color.Gray);
        colorRectangle.Inflate(-2, -2);
        batch.Draw(Constants.WhitePixel, colorRectangle, m_color);
    }

    private void TriggerValueChangedEvent()
    {
        ValueChangedEvent?.Invoke(this);
    }

    internal void SetColor(Color newColor)
    {
        m_color = newColor;

        TriggerValueChangedEvent();

        Deselect();
        CloseSubPanel();
    }

    private void openDropdownColor(object _)
    {
        m_subPanel = new(this);
        m_subPanel.SetSelectedColor(m_availableColors.ToList().IndexOf(m_color));

        ParentMenu.OpenSubPanel(m_subPanel);
        Focus = Focus.HardFocus;
    }

    internal event MenuItemValueChangedEvent ValueChangedEvent;
    private SFDCTDropdownColorPanel m_subPanel;
    private Color m_color;
    private Color[] m_availableColors;
}
