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
            Point point = new Point();

            switch (connector.Orientation)
            {
                case ConnectorOrientation.Top:
                    point = new Point(connector.DataItem.Left + (connector.DataItem.ItemWidth / 2), connector.DataItem.Top - (ConnectorInfoBase.ConnectorHeight));
                    break;
                case ConnectorOrientation.Bottom:
                    point = new Point(connector.DataItem.Left + (connector.DataItem.ItemWidth / 2), (connector.DataItem.Top + connector.DataItem.ItemHeight) + (ConnectorInfoBase.ConnectorHeight / 2));
                    break;
                case ConnectorOrientation.Right:
                    point = new Point(connector.DataItem.Left + connector.DataItem.ItemWidth + (ConnectorInfoBase.ConnectorWidth), connector.DataItem.Top + (connector.DataItem.ItemHeight / 2));
                    break;
                case ConnectorOrientation.Left:
                    point = new Point(connector.DataItem.Left - ConnectorInfoBase.ConnectorWidth, connector.DataItem.Top + (connector.DataItem.ItemHeight / 2));
                    break;
            }

            return point;
        }

        public static Rect GetBoundingRectangle(IEnumerable<SelectableDesignerItemViewModelBase> items, double margin)
        {
            double x1 = Double.MaxValue;
            double y1 = Double.MaxValue;
            double x2 = Double.MinValue;
            double y2 = Double.MinValue;

            foreach (DesignerItemViewModelBase item in items.OfType<DesignerItemViewModelBase>())
            {
                x1 = Math.Min(item.Left - margin, x1);
                y1 = Math.Min(item.Top - margin, y1);

                x2 = Math.Max(item.Left + item.ItemWidth + margin, x2);
                y2 = Math.Max(item.Top + item.ItemHeight + margin, y2);
            }

            return new Rect(new Point(x1, y1), new Point(x2, y2));
        }

    }
}
