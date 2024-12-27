using System;
using System.Collections.Generic;
using SnowyRiver.Domain.Entities;

namespace SnowyRiver.Accounts.Domain.Entities;
public class Team<TUser>: HasNameCreationTimeSoftDeleteEntity<Guid>
        where TUser : User
{
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsActive { get; set; }

    public List<TUser> Users { get; set; } = [];
}
