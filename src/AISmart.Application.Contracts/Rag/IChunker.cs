using System.Collections.Generic;
using System.Threading.Tasks;

namespace AISmart.Rag;

public interface IChunker
{
    public Task<List<string>> BaseChunk(string text, int chunkSize);
    public Task<List<string>> SmartChunkTextAsync(string text, int maxChunkSize);
}