using Unity;

namespace FangJia.BusinessLogic.Services;

public static class ServiceLocator
{
    private static IUnityContainer? _container;

    public static void Initialize(IUnityContainer? container)
    {
        _container = container;
    }

    public static T GetService<T>()
    {
        return _container.Resolve<T>();
    }
    public static T GetService<T>(string name)
    {
        return _container.Resolve<T>(name);
    }
}