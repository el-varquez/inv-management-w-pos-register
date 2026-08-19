using System.Windows.Input;

namespace POS.Register.Shell;

public interface IDefaultAction
{
    ICommand DefaultCommand { get; }
    ICommand DismissCommand { get; }
}
