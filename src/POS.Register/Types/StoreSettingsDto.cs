namespace POS.Register.Types;

public record StoreSettingsDto(
    string StoreName,
    string Address,
    string ReceiptFooter,
    decimal DefaultUtangMarkup,
    bool TrackEWalletFloat,
    Guid? EWalletFeeItemId);
