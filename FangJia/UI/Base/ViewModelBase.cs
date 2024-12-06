using NLog;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FangJia.UI.Base;

/// <summary>
/// BaseViewModel 类是所有 ViewModel 的基类，实现了 INotifyPropertyChanged 接口，用于在属性值发生变化时通知绑定。
/// </summary>
public partial class ViewModelBase : INotifyPropertyChanged
{
    /// <summary>
    /// NLog 日志记录器，用于记录调试信息。
    /// </summary>
    protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// 属性变化通知事件，当绑定的属性值发生变化时触发。
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// 通知绑定系统某个属性值已发生变化。
    /// </summary>
    /// <param name="propertyName">发生变化的属性名称。</param>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        Logger.Debug(message: $"属性 {propertyName} 发生变化。");
        PropertyChanged?.Invoke(sender: this, e: new PropertyChangedEventArgs(propertyName: propertyName));
    }

    /// <summary>
    /// 更新目标字段的值并触发属性变化通知。
    /// </summary>
    /// <typeparam name="T">字段的类型。</typeparam>
    /// <param name="field">目标字段的引用。</param>
    /// <param name="newValue">新的值。</param>
    /// <param name="propertyName">属性名称，用于触发变化通知。</param>
    /// <returns>如果字段值发生变化则返回 true，否则返回 false。</returns>
    protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(objA: field, objB: newValue)) return false;
        field = newValue;
        OnPropertyChanged(propertyName: propertyName);
        return true;
    }

}