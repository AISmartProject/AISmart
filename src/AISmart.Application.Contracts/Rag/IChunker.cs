using System.Collections.Generic;
using System.Threading.Tasks;

namespace AISmart.Rag;

public interface IChunker
{
    public Task<List<string>> Chunk(string text, int chunkSize);
}