namespace SnowyRiver.Domain.Shared.Entities;

public interface IAudited<TUser, TTeam>
    : ICreationAudited, IModificationAudited,IHasCreatorUser<TUser>, IHasCreatorTeam<TTeam>,
        IHasLastModifierUser<TUser>, IHasLastModifierTeam<TTeam>
{
  
}
