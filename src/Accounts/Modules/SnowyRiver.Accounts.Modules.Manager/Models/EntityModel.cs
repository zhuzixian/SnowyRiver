using System;
using SnowyRiver.WPF.NotifyPropertyChangedBase;

namespace SnowyRiver.Accounts.Modules.Manager.Models;
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
}
