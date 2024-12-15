using FangJia.BusinessLogic.Models;

namespace FangJia.BusinessLogic.Interfaces;

public interface ICrawler<T>
{
    Task<List<T>> GetListAsync(IProgress<CrawlerProgress> progress); 
    
}
