using SnowyRiver.ComponentModel.NotifyPropertyChanged;

namespace SnowyRiver.Accounts.Modules.Manager.Interfaces.Models;
public class EntityModel : NotifyPropertyChangedObject
{
    public Guid Id { get; set; }

    private DateTime _creationTime = DateTime.Now;

    public DateTime CreationTime
    {
        get => _creationTime;
        set => Set(ref _creationTime, value);
    }

    private int _sortId;
    public int SortId
    {
        get => _sortId;
        set => SetProperty(ref _sortId, value);
    }

    private string _name = string.Empty;
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}
