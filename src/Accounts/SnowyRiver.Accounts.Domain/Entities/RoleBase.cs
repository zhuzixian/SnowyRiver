using System;
using System.Collections.Generic;
using SnowyRiver.Accounts.Domain.Shared;
using SnowyRiver.Domain.Entities;

namespace SnowyRiver.Accounts.Domain.Entities;
public class Role<TUser, TPermission> : HasNameCreationTimeSoftDeleteEntity<Guid>
    where TUser : User
    where TPermission : Permission
{
    /// <summary>
    /// 权限范围
    /// </summary>
    public PermissionsScope Scope { get; set; }
    public List<TPermission> Permissions { get; set; }
    public List<TUser> Users { get; set; }
}
