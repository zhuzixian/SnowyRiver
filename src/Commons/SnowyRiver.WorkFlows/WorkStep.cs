namespace SnowyRiver.WorkFlows;

public class WorkStep : WorkStep<Guid, WorkState, WorkStep>;

public class WorkStep<T> : WorkStep<Guid, WorkState, T>
    where T : WorkStep<T>;
