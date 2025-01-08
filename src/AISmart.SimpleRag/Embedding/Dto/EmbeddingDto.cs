using System.Collections.Generic;

namespace AISmart.Embedding.Dto;

public class EmbedResponse
{
    public List<EmbedData> Data { get; set; }
}

public class EmbedData
{
    public float[] Embedding { get; set; }
}