﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NeonUI.ViewModels;

public class MenuItemVM : ViewModelBase
{
    public string Header { get; set; }
    public ICommand Command { get; set; }
}
