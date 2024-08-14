using System;

namespace SnowyRiver.Domain.Entities;

public interface IHasModificationTime
{
    DateTime? LastModificationTime { get; }
}
