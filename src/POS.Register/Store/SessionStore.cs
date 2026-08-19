using CommunityToolkit.Mvvm.ComponentModel;

namespace POS.Register.Store;

public partial class SessionStore : ObservableObject
{
    [ObservableProperty]
    private string? token;

    [ObservableProperty]
    private string name = "";

    [ObservableProperty]
    private string username = "";

    [ObservableProperty]
    private string role = "";

    public void Set(string token, string name, string username, string role)
    {
        Token = token;
        Name = name;
        Username = username;
        Role = role;
    }

    public void Clear()
    {
        Token = null;
        Name = "";
        Username = "";
        Role = "";
    }
}
