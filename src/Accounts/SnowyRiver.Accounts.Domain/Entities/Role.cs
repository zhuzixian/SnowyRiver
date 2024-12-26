using System;
using System.Collections.Generic;
using SnowyRiver.Accounts.Domain.Shared;
using SnowyRiver.Domain.Entities;

namespace SnowyRiver.Accounts.Domain.Entities;
public class Role : HasNameCreationTimeSoftDeleteEntity<Guid>
{
    /// <summary>
    /// 权限范围
    /// </summary>
    public PermissionsScope Scope { get; set; }
    public List<Permission> Permissions { get; set; }
    public List<User> Users { get; set; }
}
