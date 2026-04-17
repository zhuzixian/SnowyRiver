using Microsoft.EntityFrameworkCore;

namespace SnowyRiver.EF;

public class SnowyRiverDbContext(DbContextOptions options) : DbContext(options)
{
    protected virtual string DbTablePrefix => "";
    protected virtual string? DbSchema => null;
}
