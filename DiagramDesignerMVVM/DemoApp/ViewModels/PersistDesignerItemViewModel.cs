using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DiagramDesigner;
using System.Windows.Input;

namespace DemoApp
{
    public class PersistDesignerItemViewModel : DesignerItemViewModelBase, ISupportDataChanges
    {
        private IUIVisualizerService visualiserService;

        public PersistDesignerItemViewModel(int id, DiagramViewModel parent, double left, double top, string hostUrl) : base(id,parent, left,top)
        {
            this.HostUrl = hostUrl;
            Init();
        }

        public PersistDesignerItemViewModel() : base()
        {
            Init();
        }


        public String HostUrl { get; set; }
        public ICommand ShowDataChangeWindowCommand { get; private set; }

        public void ExecuteShowDataChangeWindowCommand(object parameter)
        {
            PersistDesignerItemData data = new PersistDesignerItemData(HostUrl);
            if (visualiserService.ShowDialog(data) == true)
            {
                this.HostUrl = data.HostUrl;
            }
        }


        private void Init()
        {
            visualiserService = ApplicationServicesProvider.Instance.Provider.VisualizerService;
            ShowDataChangeWindowCommand = new SimpleCommand(ExecuteShowDataChangeWindowCommand);
            this.ShowConnectors = false;

        }
    }
}
