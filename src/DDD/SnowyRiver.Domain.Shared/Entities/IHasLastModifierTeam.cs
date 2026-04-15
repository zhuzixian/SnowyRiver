namespace SnowyRiver.Domain.Shared.Entities;

public interface IHasLastModifierTeam<TTeam>
{
    TTeam? LastModifierTeam { get; set; }
}
