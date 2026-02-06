using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SFD.MenuControls;

namespace SFDCT.UI.Panels;

internal class SFDCTConfirmMultiplePanel : Panel
{
    public SFDCTConfirmMultiplePanel(string message, string[] optionTexts, ControlEvents.ChooseEvent[] optionEvents, string[] optionIcons) : base("", 240, 36 + Menu.ITEM_HEIGHT * optionTexts.Length)
    {
        m_label = new(message)
        {
            Width = Width,
            TextAlign = Align.Center
        };

        m_label.SetScrollingText(true);
        m_events = optionEvents;

        Menu menu = new(new(0, 32), Width, Menu.ITEM_HEIGHT * optionTexts.Length, this);
        for (int i = 0; i < optionTexts.Length; i++)
        {
            string optionText = optionTexts[i].ToUpperInvariant();
            ControlEvents.ChooseEvent optionEvent = i < optionEvents.Length ? optionEvents[i] : null;
            string optionIcon = i < optionIcons.Length ? optionIcons[i] : null;
            optionIcon ??= string.Empty;

            menu.Add(new MenuItemButton(optionText, optionEvent, optionIcon));
        }

        menu.SelectFirst();
        members.Add(menu);
    }

    public override void KeyPress(Keys key)
    {
        if (subPanel == null && key == Keys.Escape && m_events.Length > 0)
        {
            m_events[0](null);
            return;
        }

        base.KeyPress(key);
    }

    public override void Draw(SpriteBatch batch, float elapsed)
    {
        base.Draw(batch, elapsed);

        m_label.Draw(batch, elapsed, Position + new Vector2(Width * 0.5f, 0));
    }

    private ControlEvents.ChooseEvent[] m_events;
    private Label m_label;
}
