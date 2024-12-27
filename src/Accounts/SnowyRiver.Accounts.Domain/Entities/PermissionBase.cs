using System;
using System.Collections.Generic;
using SnowyRiver.Domain.Entities;

namespace SnowyRiver.Accounts.Domain.Entities;
public class Permission<TRole> : HasNameCreationTimeSoftDeleteEntity<Guid>
        where TRole : Role
{
    public List<TRole> Roles { get; set; }
}
