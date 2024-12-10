using System;
using System.Collections.Generic;
using SnowyRiver.Domain.Entities;

namespace SnowyRiver.Accounts.Domain.Entities;
public class Role : HasNameCreationTimeSoftDeleteEntity<Guid>
{
    public List<Permission> Permissions { get; set; }
    public List<User> Users { get; set; }
}
