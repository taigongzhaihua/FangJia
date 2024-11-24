namespace FangJia.Cores.Interfaces
{
    public interface INavigationService
    {
        void NavigateTo(string? viewName);
        void GoBack();
    }

}
