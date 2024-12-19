using MongoDB.Driver;
using Orleans.Storage;

namespace AISmart.EventSourcing.MongoDB.Options;

public class MongoDbStorageOptions : IStorageProviderSerializerOptions
{
    public IGrainStorageSerializer GrainStorageSerializer { get; set; } = null!;

    [Redact] public MongoClientSettings ClientSettings { get; set; } = null!;

    [Redact] public MongoCredential? Credentials { get; set; }
}