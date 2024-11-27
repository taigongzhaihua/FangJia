using System.Windows.Input;

namespace FangJia.Models;

public class MainMenuItemData(string? name, string? icon, string? pageName, ICommand command)
{
    public string? Name { get; set; } = name;
    public string? Icon { get; set; } = icon;
    public string? PageName { get; set; } = pageName;
    public ICommand? Command { get; set; } = command;

    public static bool operator ==(MainMenuItemData left, MainMenuItemData right)
    {
        return left.Name == right.Name && left.Icon == right.Icon && left.PageName == right.PageName;
    }

    public static bool operator !=(MainMenuItemData left, MainMenuItemData right)
    {
        return !(left == right);
    }
}