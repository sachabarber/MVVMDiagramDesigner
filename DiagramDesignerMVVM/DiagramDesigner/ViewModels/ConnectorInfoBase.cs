using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace DiagramDesigner
{
    public abstract class ConnectorInfoBase : INPCBase
    {
        private static double connectorWidth = 8;
        private static double connectorHeight = 8;

        public ConnectorInfoBase(ConnectorOrientation orientation)
        {
            this.Orientation = orientation;
        }

        public ConnectorOrientation Orientation { get; private set; }

        public static double ConnectorWidth
        {
            get { return connectorWidth; }
        }

        public static double ConnectorHeight
        {
            get { return connectorHeight; }
        }
    }
}
