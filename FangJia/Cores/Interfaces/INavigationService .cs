namespace FangJia.Cores.Interfaces
{
    public interface INavigationService
    {
        string CurrentViewName();
        void NavigateTo(string? viewName);
        void GoBack();
        void GoForward();
    }

}
