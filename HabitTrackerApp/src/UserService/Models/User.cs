using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UserService.Models;

public partial class User
{
    [Key]
    public Guid Id { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? PasswordHash { get; set; }

    public string? FullName { get; set; }

    public string? AvatarUrl { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Gender { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? Status { get; set; }

    public DateTime? LastLogin { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public bool? EmailVerified { get; set; }

    public bool? PhoneVerified { get; set; }

    public virtual ICollection<UserProvider> UserProviders { get; set; } = new List<UserProvider>();

    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
}
