namespace FangJia.BusinessLogic.Models.Config;

public class Group
{
    public string? Title { get; set; }
    public string? Key { get; set; }
    public List<Item>? Items { get; set; }
}

public class Item
{
    public string? Name { get; set; }
    public string? Key { get; set; }
    public string? Type { get; set; }
    public string? ControlType { get; set; }
    public string? ControlStyle { get; set; }
    public List<string>? Options { get; set; }
    public bool IsEnable { get; set; }
    public string? Tip { get; set; }
}