using System;
using Volo.Abp.Domain.Entities;

namespace AISmart.User;

public class IdentityUserExtension: Entity<Guid>
{
    public Guid UserId { get; set; }
    /// <summary>
    /// EOA Address or CA Address
    /// </summary>
    public string WalletAddress { get; set; }
    
    public IdentityUserExtension(Guid id)
    {
        Id = id;
    }
}