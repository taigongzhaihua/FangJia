using System.Windows.Input;

namespace FangJia.Models.DataModel;

public class TabItem(string? name, string? pageName, ICommand command)
{


    protected bool Equals(TabItem other)
    {
        return Name == other.Name && PageName == other.PageName && Equals(Command, other.Command);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == GetType() && Equals((TabItem)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, PageName);
    }

    public string? Name { get; } = name;
    public string? PageName { get; } = pageName;
    public ICommand? Command { get; set; } = command;

    public static bool operator ==(TabItem left, TabItem right)
    {
        return left.Name == right.Name && left.PageName == right.PageName;
    }

    public static bool operator !=(TabItem left, TabItem right)
    {
        return !(left == right);
    }
}