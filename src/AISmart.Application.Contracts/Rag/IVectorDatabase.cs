using System.Collections.Generic;
using System.Threading.Tasks;

namespace AISmart.Rag;

public interface IVectorDatabase
{
    Task StoreAsync(string chunk, float[] embedding);
    Task<List<string>> RetrieveAsync(float[] queryEmbedding, int topK = 5);
}