using Microsoft.Xna.Framework;
using SFD;
using SFD.MenuControls;

namespace SFDCT.UI.Panels;

internal class SFDCTCreditsPanel : Panel
{
    private static readonly Color[] CreditColors =
    [
        new Color(255, 255, 255), // Azure
        new Color(203, 255, 201), // (1144477755) ElDou's¹
        new Color(201, 255, 234), // (204624521) Liokindy
        new Color(255, 231, 201), // (912907660) Nult
    ];

    internal SFDCTCreditsPanel() : base(LanguageHelper.GetText("sfdct.credits.header"), 350, 50 + 28 * 6)
    {
        m_menu = new Menu(new Vector2(0, 50), Width, Height, this, null);

        m_buttonAzure = new MenuItemButton("\"Azure\", " + LanguageHelper.GetText("sfdct.credits.category.azure"), (object _) => { });
        m_buttonAzure.lblName = new Label(m_buttonAzure.lblName.Text, Constants.Font1Outline, Color.White, false);
        m_buttonAzure.EnabledTextColor = CreditColors[0];
        m_menu.Add(m_buttonAzure);
        m_buttonElDous1 = new MenuItemButton("\"ElDou's 1\", " + LanguageHelper.GetText("sfdct.credits.category.eldous1"), (object _) => { });
        m_buttonElDous1.lblName = new Label(m_buttonElDous1.lblName.Text, Constants.Font1Outline, Color.White, false);
        m_buttonElDous1.EnabledTextColor = CreditColors[1];
        m_menu.Add(m_buttonElDous1);
        m_buttonLiokindy = new MenuItemButton("\"Liokindy\", " + LanguageHelper.GetText("sfdct.credits.category.liokindy"), (object _) => { });
        m_buttonLiokindy.lblName = new Label(m_buttonLiokindy.lblName.Text, Constants.Font1Outline, Color.White, false);
        m_buttonLiokindy.EnabledTextColor = CreditColors[2];
        m_menu.Add(m_buttonLiokindy);
        m_buttonNult = new MenuItemButton("\"Nult\", " + LanguageHelper.GetText("sfdct.credits.category.nult"), (object _) => { });
        m_buttonNult.lblName = new Label(m_buttonNult.lblName.Text, Constants.Font1Outline, Color.White, false);
        m_buttonNult.EnabledTextColor = CreditColors[3];
        m_menu.Add(m_buttonNult);

        m_menu.Add(new MenuItemSeparator(string.Empty));
        m_menu.Add(new MenuItemButton(LanguageHelper.GetText("button.back"), new ControlEvents.ChooseEvent((object _) =>
        {
            ParentPanel.CloseSubPanel();
        }), MenuIcons.Cancel));

        members.Add(m_menu);
    }

    private readonly MenuItemButton m_buttonAzure;
    private readonly MenuItemButton m_buttonElDous1;
    private readonly MenuItemButton m_buttonLiokindy;
    private readonly MenuItemButton m_buttonNult;
    private readonly Menu m_menu;
}
