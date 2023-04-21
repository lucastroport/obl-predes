using F1.Constants;
using F1.Domain.Model;
using F1.Presentation.Views.Menu.Model;

namespace F1.Presentation.Views.Menu
{
    public class MenuOptions
    {
        public static List<MenuItem> MenuItems = new()
        {
            new(MenuItemConstants.LogIn, "Log in", new[] { UserType.Admin, UserType.Mechanic }, true),
            new(MenuItemConstants.AddUser, "Add user", new[] { UserType.Admin }, true),
            new(MenuItemConstants.Parts, "Parts", new[] { UserType.Admin, UserType.Mechanic }),
            new(MenuItemConstants.Messages, "Messages", new[] { UserType.Admin, UserType.Mechanic }, true),
            new(MenuItemConstants.ExitMenu, "Exit Menu", new[] { UserType.Admin }),
            new(MenuItemConstants.AddPart, "Add part", new[] { UserType.Admin, UserType.Mechanic }, true),
            new(MenuItemConstants.AddPartCategory, "Add part category", new[] { UserType.Admin, UserType.Mechanic },
                true),
            new(MenuItemConstants.AssociateCategoryToPart, "Associate category to part",
                new[] { UserType.Admin, UserType.Mechanic }, true),
            new(MenuItemConstants.AssociatePictureToPart, "Associate picture to part",
                new[] { UserType.Admin, UserType.Mechanic }, true),
            new(MenuItemConstants.SearchPartById, "Search part by id", new[] { UserType.Admin, UserType.Mechanic },
                true),
            new(MenuItemConstants.SearchPartByName, "Search part by name", new[] { UserType.Admin, UserType.Mechanic },
                true),
            new(MenuItemConstants.Chat, "Chat", new[] { UserType.Admin, UserType.Mechanic }),
            new(MenuItemConstants.History, "History", new[] { UserType.Admin, UserType.Mechanic }, true),
            new(MenuItemConstants.LogOut, "Exit/LogOut", new[] { UserType.Admin, UserType.Mechanic }, true)
        };

        public static List<MenuItem> PartMenuItems = new()
        {
            new(MenuItemConstants.AddPart, "Add part", new[] { UserType.Admin, UserType.Mechanic }, true),
            new(MenuItemConstants.AddPartCategory, "Add part category", new[] { UserType.Admin, UserType.Mechanic },
                true),
            new(MenuItemConstants.AssociateCategoryToPart, "Associate category to part",
                new[] { UserType.Admin, UserType.Mechanic }, true),
            new(MenuItemConstants.AssociatePictureToPart, "Associate picture to part",
                new[] { UserType.Admin, UserType.Mechanic }, true),
            new(MenuItemConstants.SearchPartById, "Search part by id", new[] { UserType.Admin, UserType.Mechanic },
                true),
            new(MenuItemConstants.SearchPartByName, "Search part by name", new[] { UserType.Admin, UserType.Mechanic },
                true)
        };

        public static string ListItems(List<MenuItem> items)
        {
            string menuString = "";
            foreach (MenuItem item in items)
            {
                menuString += item + "\n";
            }

            return menuString;
        }
    }
}
