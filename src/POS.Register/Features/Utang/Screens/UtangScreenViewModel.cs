using System.Collections.ObjectModel;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS.Register.Lib;
using POS.Register.Services;
using POS.Register.Types;
using AppShell = POS.Register.Shell;
using SharedComp = POS.Register.Components;
using UtangComp = POS.Register.Features.Utang.Components;

namespace POS.Register.Features.Utang.Screens;

public partial class SukiListRow : ObservableObject
{
    public SukiDto Suki { get; }
    public string Initial => Suki.Name.Length > 0 ? Suki.Name[..1] : "?";
    public string Name => Suki.Name;
    public string PhoneDisplay => string.IsNullOrWhiteSpace(Suki.Phone) ? "—" : Suki.Phone;
    public string BalanceDisplay => Peso.Format(Suki.Balance);
    public bool IsZeroBalance => Suki.Balance == 0m;

    [ObservableProperty]
    private bool isSelected;

    public SukiListRow(SukiDto suki, bool selected)
    {
        Suki = suki;
        isSelected = selected;
    }
}

public record LedgerRow(
    Guid Id,
    string Date,
    string Entry,
    string Sub,
    bool HasSub,
    string ChargeDisplay,
    string PayDisplay,
    string BalanceDisplay,
    bool IsVoided,
    bool CanEdit,
    bool CanVoid,
    decimal Amount,
    bool IsCharge,
    Guid? TransactionId);

public partial class UtangScreenViewModel : ObservableObject
{
    public AppShell.ShellViewModel ShellVm { get; }

    private readonly DispatcherTimer _searchTimer;

    public ObservableCollection<SukiListRow> SukiRows { get; } = [];
    public ObservableCollection<LedgerRow> LedgerRows { get; } = [];

    [ObservableProperty]
    private string search = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(
        nameof(HasCust), nameof(CustName), nameof(CustPhone),
        nameof(BalanceDisplay), nameof(IsZeroBalance), nameof(CanCollect))]
    private SukiDto? selected;

    [ObservableProperty]
    private bool addingSuki;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveSukiCommand))]
    private string newName = "";

    [ObservableProperty]
    private string newPhone = "";

    public bool OffBanner => !ShellVm.Methods.HasActiveInvoice;
    public bool CanAddSuki => ShellVm.Methods.HasActiveInvoice;
    public bool HasCust => Selected is not null;
    public string CustName => Selected?.Name ?? "";
    public string CustPhone => Selected is { } s && !string.IsNullOrWhiteSpace(s.Phone) ? s.Phone : "—";
    public string BalanceDisplay => Peso.Format(Selected?.Balance ?? 0m);
    public bool IsZeroBalance => (Selected?.Balance ?? 0m) == 0m;
    public bool CanCollect => (Selected?.Balance ?? 0m) > 0m && ShellVm.Day.IsShiftOpen;
    public string CollectCaption => ShellVm.Day.IsShiftOpen
        ? ""
        : "Collections need an open shift — declare starting cash first.";

    public UtangScreenViewModel(AppShell.ShellViewModel shell)
    {
        ShellVm = shell;
        _searchTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250) };
        _searchTimer.Tick += (_, _) =>
        {
            _searchTimer.Stop();
            _ = RunSearchAsync(Search.Trim());
        };
        ShellVm.Methods.PropertyChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(OffBanner));
            OnPropertyChanged(nameof(CanAddSuki));
        };
        ShellVm.Day.PropertyChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(CanCollect));
            OnPropertyChanged(nameof(CollectCaption));
        };
    }

    partial void OnSearchChanged(string value)
    {
        _searchTimer.Stop();
        _searchTimer.Start();
    }

    public async Task LoadAsync()
    {
        await RunSearchAsync(Search.Trim());
        if (Selected is not null)
        {
            await LoadLedgerAsync(Selected.Id);
        }
    }

    private async Task RunSearchAsync(string term)
    {
        try
        {
            var sukis = await ShellVm.UtangApi.GetSukisAsync(
                term.Length == 0 ? null : term);
            if (term != Search.Trim())
            {
                return;
            }
            SukiRows.Clear();
            foreach (var suki in sukis)
            {
                SukiRows.Add(new SukiListRow(suki, suki.Id == Selected?.Id));
            }
            if (Selected is { } current)
            {
                var refreshed = sukis.FirstOrDefault(s => s.Id == current.Id);
                if (refreshed is not null)
                {
                    Selected = refreshed;
                }
            }
        }
        catch (ApiException ex)
        {
            ShellVm.ShowToast(ex.Message);
        }
    }

    partial void OnSelectedChanged(SukiDto? value)
    {
        foreach (var row in SukiRows)
        {
            row.IsSelected = row.Suki.Id == value?.Id;
        }
    }

    [RelayCommand]
    private async Task SelectSukiAsync(SukiListRow row)
    {
        Selected = row.Suki;
        await LoadLedgerAsync(row.Suki.Id);
    }

    private async Task LoadLedgerAsync(Guid sukiId)
    {
        try
        {
            var ledger = await ShellVm.UtangApi.GetLedgerAsync(sukiId);
            if (Selected?.Id != sukiId)
            {
                return;
            }
            Selected = new SukiDto(ledger.Id, ledger.Name, ledger.Phone, ledger.Balance);
            LedgerRows.Clear();
            var running = 0m;
            foreach (var entry in ledger.Entries)
            {
                var isCharge = entry.Type == "Charge";
                var isAdjustment = entry.Type == "Adjustment";
                var isPayment = !isCharge && !isAdjustment;
                if (!entry.IsVoided)
                {
                    running += isPayment ? -entry.Amount : entry.Amount;
                }
                var label = isCharge
                    ? $"Charge · {entry.ReceiptNumber}"
                    : isAdjustment
                        ? $"Adjustment · {entry.Note}"
                        : entry.TransactionId is not null
                            ? $"{entry.Note} · {entry.ReceiptNumber}"
                            : entry.Note ?? "Payment received";
                var sub = entry.EditedFrom is { } was ? $"edited · was {Peso.Format(was)}" : "";
                var showsAsCharge = isCharge || (isAdjustment && entry.Amount > 0m);
                LedgerRows.Add(new LedgerRow(
                    entry.Id,
                    entry.CreatedAt.ToLocalTime().ToString("MMM d"),
                    label,
                    sub,
                    sub.Length > 0,
                    showsAsCharge ? Peso.Format(entry.Amount) : "",
                    showsAsCharge ? "" : Peso.Format(Math.Abs(entry.Amount)),
                    entry.IsVoided ? "—" : Peso.Format(running),
                    entry.IsVoided,
                    isPayment && !entry.IsVoided,
                    !isAdjustment && !entry.IsVoided,
                    entry.Amount,
                    isCharge,
                    entry.TransactionId));
            }
        }
        catch (ApiException ex)
        {
            ShellVm.ShowToast(ex.Message);
        }
    }

    public async Task RefreshAfterActionAsync()
    {
        if (Selected is { } suki)
        {
            await LoadLedgerAsync(suki.Id);
        }
        await RunSearchAsync(Search.Trim());
        _ = ShellVm.RefreshUtangAsync();
        _ = ShellVm.RefreshShiftAsync();
    }

    [RelayCommand]
    private void ToggleAddSuki()
    {
        AddingSuki = !AddingSuki;
        NewName = "";
        NewPhone = "";
    }

    private bool CanSaveSuki() => NewName.Trim().Length > 0;

    [RelayCommand(CanExecute = nameof(CanSaveSuki))]
    private async Task SaveSukiAsync()
    {
        try
        {
            var created = await ShellVm.UtangApi.CreateSukiAsync(
                NewName.Trim(),
                string.IsNullOrWhiteSpace(NewPhone) ? null : NewPhone.Trim());
            AddingSuki = false;
            NewName = "";
            NewPhone = "";
            await RunSearchAsync(Search.Trim());
            Selected = created;
            await LoadLedgerAsync(created.Id);
            _ = ShellVm.RefreshUtangAsync();
        }
        catch (ApiException ex)
        {
            ShellVm.ShowToast(ex.Message);
        }
    }

    [RelayCommand]
    private void OpenCollect()
    {
        if (!CanCollect || Selected is not { } suki)
        {
            return;
        }
        ShellVm.OpenModal(new UtangComp.CollectModalViewModel(ShellVm, this, suki));
    }

    [RelayCommand]
    private void PrintStatement() => ShellVm.ShowToast("Statement sent to printer");

    [RelayCommand]
    private void VoidEntry(LedgerRow row)
    {
        ShellVm.OpenModal(new SharedComp.AdminOverrideModalViewModel(
            ShellVm,
            $"Void ledger entry: {row.Entry} ({Peso.Format(row.Amount)})",
            token => _ = VoidEntryAsync(row, token)));
    }

    private async Task VoidEntryAsync(LedgerRow row, string token)
    {
        try
        {
            if (row.IsCharge)
            {
                if (row.TransactionId is not { } transactionId)
                {
                    return;
                }
                await ShellVm.UtangApi.VoidChargeAsync(transactionId, token);
            }
            else
            {
                await ShellVm.UtangApi.VoidPaymentAsync(row.Id, token);
            }
            ShellVm.ShowToast("Entry voided — kept on the ledger");
            await RefreshAfterActionAsync();
        }
        catch (ApiException ex)
        {
            ShellVm.ShowToast(ex.Message);
        }
    }

    [RelayCommand]
    private void EditEntry(LedgerRow row)
    {
        ShellVm.OpenModal(new SharedComp.AdminOverrideModalViewModel(
            ShellVm,
            $"Edit payment of {Peso.Format(row.Amount)}",
            token => ShellVm.OpenModal(
                new UtangComp.EditPaymentModalViewModel(ShellVm, this, row, token))));
    }
}
