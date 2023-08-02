using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeonUI.ViewModels
{
    public class WindowViewModel : ViewModelBase
    {
        public Action? CloseWindow { get; set; }
    }
}
