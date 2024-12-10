using System;
using System.Collections.Generic;
using SnowyRiver.Domain.Entities;

namespace SnowyRiver.Accounts.Domain.Entities;
public class Permission : HasNameCreationTimeSoftDeleteEntity<Guid>
{
    public List<Role> Roles { get; set; }
}
