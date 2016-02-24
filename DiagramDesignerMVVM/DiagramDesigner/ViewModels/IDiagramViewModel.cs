using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace DiagramDesigner
{
    public interface IDiagramViewModel
    {
        SimpleCommand AddItemCommand { get; }
        SimpleCommand RemoveItemCommand { get;  }
        SimpleCommand ClearSelectedItemsCommand { get;  }
        List<SelectableDesignerItemViewModelBase> SelectedItems { get; }
        ObservableCollection<SelectableDesignerItemViewModelBase> Items { get; }
    }
}
