using System.Threading.Tasks;

namespace AISmart.Provider;

public interface IEmbeddingProvider
{
    Task<float[]> GetEmbeddingAsync(string text);
}