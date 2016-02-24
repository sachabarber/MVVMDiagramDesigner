using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace DiagramDesigner
{
    public class FullyCreatedConnectorInfo : ConnectorInfoBase
    {
        private bool showConnectors = false;

        public FullyCreatedConnectorInfo(DesignerItemViewModelBase dataItem, ConnectorOrientation orientation)
            : base(orientation)
        {
            this.DataItem = dataItem;
        }


        public DesignerItemViewModelBase DataItem { get; private set; }

        public bool ShowConnectors
        {
            get
            {
                return showConnectors;
            }
            set
            {
                if (showConnectors != value)
                {
                    showConnectors = value;
                    NotifyChanged("ShowConnectors");
                }
            }
        }
    }
}
