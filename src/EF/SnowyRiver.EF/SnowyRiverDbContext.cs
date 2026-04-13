using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SnowyRiver.Domain.Shared.Entities;

namespace SnowyRiver.EF;

public class SnowyRiverDbContext(DbContextOptions options) : DbContext(options)
{
    protected virtual string DbTablePrefix => "";
    protected virtual string? DbSchema => null;
}
