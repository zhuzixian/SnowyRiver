namespace SnowyRiver.WorkFlows;

public class WorkFlow : WorkFlow<Guid, WorkState, Guid, WorkStep, WorkState>;
public class WorkFlow<TWorkStep> : WorkFlow<Guid, WorkState, Guid, TWorkStep, WorkState>
    where TWorkStep : WorkStep<Guid, WorkState>;
