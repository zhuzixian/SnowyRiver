namespace SnowyRiver.WorkFlows;
public class WorkStep<TKey>: WorkStep
{
    private TKey _id;
    public TKey Id
    {
        get => _id;
        set => Set(ref _id, value);
    }
}
