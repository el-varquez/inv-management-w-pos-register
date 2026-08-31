using CommunityToolkit.Mvvm.ComponentModel;
using POS.Register.Types;

namespace POS.Register.Store;

public partial class PaymentMethodStore : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ActiveMethods), nameof(HasActiveInvoice))]
    private List<PaymentMethodDto> all = [];

    public IReadOnlyList<PaymentMethodDto> ActiveMethods
        => All.Where(m => m.IsActive).ToList();

    public bool HasActiveInvoice
        => All.Any(m => m.IsActive && m.Type == "Invoice");

    public string? NameOf(Guid id) => All.FirstOrDefault(m => m.Id == id)?.Name;
}
