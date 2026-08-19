namespace POS.Register.Types;

public record LoginResponse(
    string? Token,
    string? Name,
    string? Username,
    string? Role,
    bool PasswordSetupRequired);
