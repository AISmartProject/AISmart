using System.Threading.Tasks;

namespace AISmart.Rag;

public interface IEmbeddingProvider
{
    Task<float[]> GetEmbeddingAsync(string text);
}