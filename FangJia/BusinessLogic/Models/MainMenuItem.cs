using System.Windows.Input;

namespace FangJia.BusinessLogic.Models;

public class MainMenuItem(string? name, string? icon, string? pageName, ICommand command)
{
    protected bool Equals(MainMenuItem other)
    {
        return Name == other.Name && Icon == other.Icon && PageName == other.PageName && Equals(Command, other.Command);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == GetType() && Equals((MainMenuItem)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, Icon, PageName);
    }

    public string? Name { get; } = name;
    public string? Icon { get; } = icon;
    public string? PageName { get; } = pageName;
    public ICommand? Command { get; set; } = command;

    public static bool operator ==(MainMenuItem left, MainMenuItem right)
    {
        return left.Name == right.Name && left.Icon == right.Icon && left.PageName == right.PageName;
    }

    public static bool operator !=(MainMenuItem left, MainMenuItem right)
    {
        return !(left == right);
    }
}