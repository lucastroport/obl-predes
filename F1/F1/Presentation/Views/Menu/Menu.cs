using System.Text;
using F1.Constants;
using F1.Presentation.Views.Menu.Model;

namespace F1.Presentation.Views.Menu;

public class Menu
{
    private readonly List<MenuItem> _menuItems;
    public Menu()
    {
        _menuItems = MenuOptions.MenuItems;
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        foreach (MenuItem menuItem in _menuItems)
        {
            if (menuItem.Enabled)
            {
                sb.AppendLine(menuItem.ToString());
            }
        }
        return sb.ToString();
    }

    public MenuItem GetMenu(int menuId) => _menuItems.First(m => m.Id == menuId);

    public void TriggerLoggedInMenu()
    {
        _menuItems.ForEach(item => { item.Enabled = item.Id != MenuItemConstants.LogIn; });
    }
    
    public void TriggerNotAuthMenu()
    {
        _menuItems.ForEach(item => { item.Enabled = item.Id == MenuItemConstants.LogIn; });
    }
}