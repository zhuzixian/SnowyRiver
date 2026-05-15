using SnowyRiver.Domain.Shared.WorkFlows;

namespace SnowyRiver.WorkFlows;

public class WorkStep : WorkStep<Guid, WorkState>;

public class WorkStep<T> : WorkStep<Guid, WorkState>
    where T : WorkStep<T>;
