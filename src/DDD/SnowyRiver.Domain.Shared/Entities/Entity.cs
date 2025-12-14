namespace SnowyRiver.Domain.Shared.Entities;
public class Entity<TKey>:IEntity<TKey>
{
    public Entity()
    {
    }

    public Entity(TKey id):this()
    {
        Id = id;
    }

    public TKey Id { get;  set; }
}
