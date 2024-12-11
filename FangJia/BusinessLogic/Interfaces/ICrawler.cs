namespace FangJia.BusinessLogic.Interfaces;

public interface ICrawler<T>
{
    Task<List<T>> GetListAsync();
}