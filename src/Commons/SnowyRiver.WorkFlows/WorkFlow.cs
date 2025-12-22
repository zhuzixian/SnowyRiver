namespace SnowyRiver.WorkFlows;

public class WorkFlow : WorkFlow<Guid, WorkState, Guid, WorkStep, WorkState, WorkFlow>;
public class WorkFlow<TWorkStep, T> : WorkFlow<Guid, WorkState, Guid, TWorkStep, WorkState, T>
    where TWorkStep : WorkStep<Guid, WorkState, TWorkStep>
    where T: WorkFlow<TWorkStep, T>;
