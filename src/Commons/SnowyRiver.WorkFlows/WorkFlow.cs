namespace SnowyRiver.WorkFlows;
public class WorkFlow<TKey, TStep, TStepKey>: WorkFlow<TStep>
     where TStep : WorkStep<TStepKey>
{
    private TKey _id;
    public TKey Id
    {
        get => _id;
        set => Set(ref _id, value);
    }
}
