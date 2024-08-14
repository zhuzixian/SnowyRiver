using System;

namespace SnowyRiver.Domain.Entities;

public interface IHasCreationTime
{
    DateTime CreationTime { get; }
}
