using POS.Register.Lib;

namespace POS.Register.Features.Auth.Screens;

public class LoginScreenViewModel
{
    public string StoreName => CannedDay.StoreName;
    public string DateLong => DateTime.Now.ToString("dddd, MMMM d, yyyy");
}
