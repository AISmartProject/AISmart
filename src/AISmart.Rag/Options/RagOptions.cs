namespace AISmart.Options;

public class RagOptions
{
    public string QdrantUrl { get; set; }
    public string CollectionName { get; set; }
    public string APIKey { get; set; }
    public int VectorSize { get; set; } = 0;
}