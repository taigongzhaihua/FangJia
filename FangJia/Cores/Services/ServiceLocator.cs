using Unity;

namespace FangJia.Cores.Services;

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
}