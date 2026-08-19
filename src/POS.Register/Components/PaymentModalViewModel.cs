using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS.Register.Lib;
using AppShell = POS.Register.Shell;

namespace POS.Register.Components;

public partial class PaymentModalViewModel : ObservableObject, AppShell.IDefaultAction
{
    public System.Windows.Input.ICommand DefaultCommand => CompleteCommand;
    public System.Windows.Input.ICommand DismissCommand => DismissStepCommand;

    private readonly AppShell.ShellViewModel _shell;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsCash))]
    [NotifyPropertyChangedFor(nameof(EwalletCaption))]
    private string method = "CASH";

    [ObservableProperty]
    private bool isMethodOpen;

    public bool IsCash => Method == "CASH";
    public string TotalDisplay => Peso.Format(CannedDay.CartTotal);
    public string TenderedDisplay => CannedDay.TenderedDisplay;
    public string ChangeDisplay => CannedDay.ChangeDisplay;
    public string EwalletCaption => "Customer pays " + TotalDisplay + " via " + Method + " — enter the reference number to complete.";
    public IReadOnlyList<string> QuickBills => CannedDay.QuickBills;

    public PaymentModalViewModel(AppShell.ShellViewModel shell) => _shell = shell;

    [RelayCommand]
    private void ToggleMethod() => IsMethodOpen = !IsMethodOpen;

    [RelayCommand]
    private void PickMethod(string value)
    {
        Method = value;
        IsMethodOpen = false;
    }

    [RelayCommand]
    private void Complete()
    {
        _shell.CloseModal();
        _shell.OpenModal(new SuccessModalViewModel(_shell));
    }

    [RelayCommand]
    private void Cancel() => _shell.CloseModal();

    [RelayCommand]
    private void DismissStep()
    {
        if (IsMethodOpen)
        {
            IsMethodOpen = false;
            return;
        }
        Cancel();
    }
}
