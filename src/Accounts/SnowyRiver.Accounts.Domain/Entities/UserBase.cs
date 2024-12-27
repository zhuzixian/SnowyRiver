using System;
using System.Collections.Generic;
using SnowyRiver.Domain.Entities;

namespace SnowyRiver.Accounts.Domain.Entities;
public class User<TTeam, TRole> : HasNameCreationTimeSoftDeleteEntity<Guid>
        where TTeam : Team 
        where TRole : Role
{
    public int UserId { get; set; }

    public string Password { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;

    public List<TRole> Roles { get; set; } = [];
    public List<TTeam> Teams { get; set; } = [];
}
