using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SFD;
using SFD.MenuControls;
using SFDCT.UI.MenuItems;

namespace SFDCT.UI.Panels;

internal class SFDCTDropdownColorPanel : Panel
{
    internal SFDCTDropdownColorPanel(SFDCTMenuItemDropdownColor parentItem, int colorRows = 5) : base(140, (parentItem.Colors.Length / colorRows + 1) * 28)
    {
        m_parentItem = parentItem;
        m_colors = parentItem.Colors;
        m_colorRows = colorRows;
    }

    private void Close()
    {
        m_parentItem.Deselect();
        m_parentItem.CloseSubPanel();
    }

    public override void KeyPress(Keys key)
    {
        if (key == Keys.Escape)
        {
            Close();
            return;
        }

        if (key == Keys.Enter)
        {
            m_parentItem.SetColor(m_colors[m_selectedColorID]);
            return;
        }

        base.KeyPress(key);

        switch (key)
        {
            case Keys.Left:
                SelectColor(m_selectedColorID - 1);
                return;
            case Keys.Up:
                SelectColor(m_selectedColorID - m_colorRows);
                return;
            case Keys.Right:
                SelectColor(m_selectedColorID + 1);
                return;
            case Keys.Down:
                SelectColor(m_selectedColorID + m_colorRows);
                return;
        }
    }

    public override void MouseMove(Rectangle currentSelection, Rectangle previousSelection)
    {
        for (int colorID = 0; colorID < m_colors.Length; colorID++)
        {
            if (currentSelection.Intersects(GetColorRectangle(colorID))) SelectColor(colorID);
        }
    }

    public override void MouseClick(Rectangle mouseSelection)
    {
        if (!mouseSelection.Intersects(Area))
        {
            Close();
            return;
        }

        for (int colorID = 0; colorID < m_colors.Length; colorID++)
        {
            if (mouseSelection.Intersects(GetColorRectangle(colorID)))
            {
                m_parentItem.SetColor(m_colors[colorID]);
                return;
            }
        }
    }

    public override void UpdatePosition()
    {
        Position = m_parentItem.Position + Vector2.UnitX * (m_parentItem.Width - Width);
    }

    public override void Draw(SpriteBatch batch, float elapsed)
    {
        base.Draw(batch, elapsed);

        for (int colorID = 0; colorID < m_colors.Length; colorID++)
        {
            var destinationRectangle = GetColorRectangle(colorID);

            destinationRectangle.Inflate(-2, -2);
            batch.Draw(Constants.WhitePixel, destinationRectangle, Color.Black);

            if (colorID == m_selectedColorID)
            {
                batch.Draw(Constants.WhitePixel, destinationRectangle, Color.White);
            }
            else
            {
                batch.Draw(Constants.WhitePixel, destinationRectangle, Color.Gray);
            }

            destinationRectangle.Inflate(-2, -2);
            batch.Draw(Constants.WhitePixel, destinationRectangle, m_colors[colorID]);
        }
    }

    internal void SetSelectedColor(int id)
    {
        SelectColor(id);
    }

    private Rectangle GetColorRectangle(int id)
    {
        var result = new Rectangle(0, 0, 28, 28);
        result.X = (int)Position.X + (id % m_colorRows) * 28 + (Width / 2) - (m_colorRows * 28 / 2);
        result.Y = (int)Position.Y + (id / m_colorRows) * 28;

        return result;
    }

    private void SelectColor(int id)
    {
        if (id < 0 || id > m_colors.Length - 1) return;

        m_selectedColorID = id;
    }

    private int m_colorRows;
    private int m_selectedColorID;
    private SFDCTMenuItemDropdownColor m_parentItem;
    private Color[] m_colors;
}
