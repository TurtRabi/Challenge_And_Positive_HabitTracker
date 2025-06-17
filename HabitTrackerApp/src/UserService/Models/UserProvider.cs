using System;
using System.Collections.Generic;

namespace UserService.Models;

public partial class UserProvider
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string Provider { get; set; } = null!;

    public string ProviderUserId { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
