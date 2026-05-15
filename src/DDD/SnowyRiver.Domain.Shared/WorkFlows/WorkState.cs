namespace SnowyRiver.Domain.Shared.WorkFlows;
public enum WorkState
{
    Waiting,
    Running,
    Paused,
    Cancelled,
    Skipped,
    Finished,
    Failed,
}
