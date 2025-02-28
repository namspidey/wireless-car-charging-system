using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class RefreshToken
{
    public RefreshToken()
    {
    }
    public RefreshToken(int tokenId, int userId, string? token, DateTime? expiresAt, bool? revoked, DateTime? createdAt)
    {
        TokenId = tokenId;
        UserId = userId;
        Token = token;
        ExpiresAt = expiresAt;
        Revoked = revoked;
        CreatedAt = createdAt;
    }

    public int TokenId { get; set; }

    public int UserId { get; set; }

    public string? Token { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public bool? Revoked { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;

  
}
