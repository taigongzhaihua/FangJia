using System.Windows.Input;

namespace FangJia.Models
{
    public class MainMenuItemData(string? name, string? icon, ICommand command)
    {
        public string? Name { get; set; } = name;
        public string? Icon { get; set; } = icon;
        public ICommand? Command { get; set; } = command;
    }
}
