namespace SnowyRiver.Domain.Entities;

public interface ISoftDelete
{
    bool IsDeleted { get;  }
    void SoftDelete();
}
