using System;

namespace SnowyRiver.Domain.Entities;

public interface IHasDeletionTime
{
    DateTime? DeletionTime { get; }
}
