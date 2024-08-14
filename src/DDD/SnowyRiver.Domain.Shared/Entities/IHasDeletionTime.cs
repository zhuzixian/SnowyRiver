using System;

namespace SnowyRiver.Domain.Shared.Entities;

public interface IHasDeletionTime
{
    DateTime? DeletionTime { get; }
}
