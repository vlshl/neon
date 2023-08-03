using System.Windows.Input;

namespace NeonUI.ViewModels;

public class MenuItem
{
    public string Header { get; set; }
    public ICommand Command { get; set; }
}
