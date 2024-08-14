namespace SnowyRiver.Domain.Shared.Entities;

public interface ISoftDelete
{
    bool IsDeleted { get;  }
    void SoftDelete();
}
