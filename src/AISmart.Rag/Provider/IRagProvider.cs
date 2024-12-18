using System.Collections.Generic;
using System.Threading.Tasks;

namespace AISmart.Provider;

public interface IRagProvider
{
    Task StoreTextAsync(string text);
    Task BatchStoreTextsAsync(IEnumerable<string> texts);
    Task<string> RetrieveAnswerAsync(string query);
}