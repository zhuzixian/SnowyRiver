namespace SnowyRiver.Domain.Shared.Entities;

public interface IHasTeamId
{
    Guid? TeamId { get; set; }
}