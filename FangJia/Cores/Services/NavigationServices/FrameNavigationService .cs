using FangJia.Cores.Interfaces;
using FangJia.Models;
using System.Windows.Controls;

namespace FangJia.Cores.Services.NavigationServices;

public class FrameNavigationService : INavigationService
{
    private readonly Frame _frame;
    private readonly Dictionary<string, string> _pageMappings;
    private readonly Stack<object> _navigationHistory;

    public FrameNavigationService(Frame frame, List<PageConfig> pageConfigs)
    {
        _frame = frame ?? throw new ArgumentNullException(nameof(frame));
        _pageMappings = [];
        _navigationHistory = new Stack<object>();

        foreach (var config in pageConfigs)
        {
            _pageMappings[config.Name!] = config.Uri!;
        }
    }

    public void NavigateTo(string viewName)
    {
        if (_pageMappings.TryGetValue(viewName, out var uri))
        {
            _navigationHistory.Push(_frame.Content);
            _frame.Navigate(new Uri(uri, UriKind.Relative));
        }
        else
        {
            throw new ArgumentException($"View {viewName} not found in configuration.");
        }
    }

    public void GoBack()
    {
        if (_frame.CanGoBack)
        {
            _frame.GoBack();
        }
    }
}