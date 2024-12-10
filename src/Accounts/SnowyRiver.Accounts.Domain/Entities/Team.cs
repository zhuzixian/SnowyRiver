using System;
using System.Collections.Generic;
using SnowyRiver.Domain.Entities;

namespace SnowyRiver.Accounts.Domain.Entities;
public class Team : HasNameCreationTimeSoftDeleteEntity<Guid>
{
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsActive { get; set; }

    public List<User> Users { get; set; } = [];
}
