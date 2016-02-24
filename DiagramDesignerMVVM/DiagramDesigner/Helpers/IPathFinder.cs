using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace DiagramDesigner
{
    public interface IPathFinder
    {
        List<Point> GetConnectionLine(ConnectorInfo source, ConnectorInfo sink, bool showLastLine);
        List<Point> GetConnectionLine(ConnectorInfo source, Point sinkPoint, ConnectorOrientation preferredOrientation);
    }
}
