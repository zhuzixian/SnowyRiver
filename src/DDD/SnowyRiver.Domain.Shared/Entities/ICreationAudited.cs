namespace SnowyRiver.Domain.Shared.Entities;

/// <summary>
/// 创建人信息
/// </summary>
public interface ICreationAudited:IHasCreationTime
{
    Guid? CreatorUserId { get; set; }
    Guid? CreatorTeamId { get; set; }
}
