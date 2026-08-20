namespace POS.Register.Types;

public record StoreSettingsDto(
    string StoreName,
    string Address,
    string ReceiptFooter,
    bool AcceptUtang,
    decimal DefaultUtangMarkup,
    bool TrackEWalletFloat,
    Guid? EWalletFeeItemId);
