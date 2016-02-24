using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace DiagramDesigner
{
    public class PartCreatedConnectionInfo : ConnectorInfoBase
    {
        public Point CurrentLocation { get; private set; }

        public PartCreatedConnectionInfo(Point currentLocation) : base(ConnectorOrientation.None)
        {
            this.CurrentLocation = currentLocation;
        }
    }
}
