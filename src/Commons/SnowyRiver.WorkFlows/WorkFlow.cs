namespace SnowyRiver.WorkFlows;
public class WorkFlow<TKey, TState, TStep, TStepKey, TStepState>: WorkFlow<TState, TStep, TStepState>
     where TStep : WorkStep<TStepKey, TStepState>
{
    private TKey _id;
    public TKey Id
    {
        get => _id;
        set => Set(ref _id, value);
    }
}
