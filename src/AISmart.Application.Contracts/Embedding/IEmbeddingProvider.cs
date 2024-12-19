using System.Threading.Tasks;

namespace AISmart.Embedding;

public interface IEmbeddingProvider
{
    Task<float[]> GetEmbeddingAsync(string text);
}