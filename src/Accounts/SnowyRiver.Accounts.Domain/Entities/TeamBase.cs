﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using SnowyRiver.Domain.Entities;

namespace SnowyRiver.Accounts.Domain.Entities;
public class Team<TUser, TRole, TTeam, TPermission> : HasNameCreationTimeSoftDeleteEntity<Guid>
    where TTeam : Team<TUser, TRole, TTeam, TPermission>
    where TUser : User<TUser, TRole, TTeam, TPermission>
    where TRole : Role<TUser, TRole, TTeam, TPermission>
    where TPermission : Permission<TUser, TRole, TTeam, TPermission>
{
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsActive { get; set; }

    public List<TUser> Users { get; set; } = [];
}
