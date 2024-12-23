using Microsoft.Extensions.DependencyInjection;

namespace AISmart.EventSourcing.Core;

public static class InMemoryLogConsistentStorageFactory
{
    public static InMemoryLogConsistentStorage Create(IServiceProvider serviceProvider, object? name)
    {
        return ActivatorUtilities.CreateInstance<InMemoryLogConsistentStorage>(serviceProvider);
    }
}