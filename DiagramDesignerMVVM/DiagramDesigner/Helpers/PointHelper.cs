using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace DiagramDesigner
{
    public class PointHelper
    {
        public static Point GetPointForConnector(FullyCreatedConnectorInfo connector)
        {
            Point point =new Point();

            switch(connector.Orientation)
            {
                case ConnectorOrientation.Top:
                    point = new Point(connector.DataItem.Left + (DesignerItemViewModelBase.ItemWidth / 2), connector.DataItem.Top - (ConnectorInfoBase.ConnectorHeight));
                    break;
                case ConnectorOrientation.Bottom:
                    point = new Point(connector.DataItem.Left + (DesignerItemViewModelBase.ItemWidth / 2), (connector.DataItem.Top + DesignerItemViewModelBase.ItemHeight) + (ConnectorInfoBase.ConnectorHeight / 2));
                    break;
                case ConnectorOrientation.Right:
                    point = new Point(connector.DataItem.Left + DesignerItemViewModelBase.ItemWidth + (ConnectorInfoBase.ConnectorWidth), connector.DataItem.Top + (DesignerItemViewModelBase.ItemHeight / 2));
                    break;
                case ConnectorOrientation.Left:
                    point = new Point(connector.DataItem.Left - ConnectorInfoBase.ConnectorWidth, connector.DataItem.Top + (DesignerItemViewModelBase.ItemHeight / 2));
                    break;
            }

            return point;
        }


    }
}
