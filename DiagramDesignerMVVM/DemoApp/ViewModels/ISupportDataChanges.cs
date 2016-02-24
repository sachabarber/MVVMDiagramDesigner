using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace DemoApp
{
    public interface ISupportDataChanges
    {
        ICommand ShowDataChangeWindowCommand { get; }
    }
}
