namespace FangJia.BusinessLogic.Models;

public class ObservableStack<T>
{
	private readonly Stack<T> _stack = new();

	/// <summary>
	/// 当栈发生变化时触发的事件。
	/// </summary>
	public event Action? StackChanged;

	/// <summary>
	/// 入栈操作。
	/// </summary>
	public void Push(T item)
	{
		_stack.Push(item);
		OnStackChanged();
	}

	/// <summary>
	/// 出栈操作。
	/// </summary>
	public T Pop()
	{
		var item = _stack.Pop();
		OnStackChanged();
		return item;
	}

	/// <summary>
	/// 查看栈顶元素。
	/// </summary>
	public T Peek() => _stack.Peek();

	/// <summary>
	/// 获取栈的数量。
	/// </summary>
	public int Count => _stack.Count;

	/// <summary>
	/// 清空栈。
	/// </summary>
	public void Clear()
	{
		_stack.Clear();
		OnStackChanged();
	}

	/// <summary>
	/// 触发 StackChanged 事件。
	/// </summary>
	protected virtual void OnStackChanged()
	{
		StackChanged?.Invoke();
	}
}
