using System;

namespace SnowyRiver.Domain.Shared.Entities;

public interface IHasModificationTime
{
    DateTime? LastModificationTime { get; }
}
