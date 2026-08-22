using CommunityToolkit.Mvvm.ComponentModel;
using POS.Register.Types;

namespace POS.Register.Store;

public partial class UtangStore : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasAnyBalance))]
    private List<SukiDto> sukis = [];

    public bool HasAnyBalance => Sukis.Any(s => s.Balance != 0m);

    public string? NameOf(Guid? sukiId)
        => sukiId is null ? null : Sukis.FirstOrDefault(s => s.Id == sukiId)?.Name;

    public void Set(List<SukiDto> list) => Sukis = list;
}
