using System.Collections.ObjectModel;

namespace FangJia.Models.DataModel;

public class Category
{
    public string Name { get; set; }
    public ObservableCollection<Category> SubCategories { get; set; } = [];
    public ObservableCollection<Formula> Formulas { get; set; } = [];
}

public class Formula
{
    public string? Name { get; set; }
    public string? Medicines { get; set; }
    public string? Purpose { get; set; }
}