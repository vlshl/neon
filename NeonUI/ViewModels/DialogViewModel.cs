using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using NeonUI.Views;

namespace NeonUI.ViewModels;

public class DialogViewModel : ViewModelBase
{
    public DialogViewModel() {
        OkCommand = ReactiveCommand.Create(() => OnOk());
        CancelCommand = ReactiveCommand.Create(() => OnCancel());
        CloseDialog = null;
        MessagePanel = null;
    }

    public ICommand OkCommand { get; }
    public ICommand CancelCommand { get; }

    public virtual void OnOk()
    {
        CloseDialog?.Invoke(true);
    }

    public virtual void OnCancel()
    {
        CloseDialog?.Invoke(false);
    }

    public Action<bool>? CloseDialog { get; set; }

    public IMessagePanel? MessagePanel { get; set; }
}
