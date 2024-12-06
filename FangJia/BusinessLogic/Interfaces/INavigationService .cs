namespace FangJia.BusinessLogic.Interfaces;

public interface INavigationService
{
    string? CurrentViewName();
    void NavigateTo(string? viewName);
    void GoBack();
    void GoForward();

    bool CanGoBack { get; }
    bool CanGoForward { get; }
}