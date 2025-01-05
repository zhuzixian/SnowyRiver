namespace SnowyRiver.Accounts.Modules.Manager.Interfaces.Models;

public class Team<TUser, TRole, TTeam, TPermission> : EntityModel
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
