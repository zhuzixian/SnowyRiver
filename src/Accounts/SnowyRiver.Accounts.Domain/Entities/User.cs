using System;
using System.Collections.Generic;
using SnowyRiver.Domain.Entities;

namespace SnowyRiver.Accounts.Domain.Entities;
public class User : HasNameCreationTimeSoftDeleteEntity<Guid>
{
    public int UserId { get; set; }

    public string Password { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;

    public List<Role> Roles { get; set; } = [];
    public List<Team> Teams { get; set; } = [];
}
