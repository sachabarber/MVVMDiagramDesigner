using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DemoApp.Persistence.Common
{
    public class Connection : PersistableItemBase
    {
        public Connection(int id, int sourceId, Orientation sourceOrientation, 
            Type sourceType, int sinkId, Orientation sinkOrientation, Type sinkType) : base(id)
        {
            this.SourceId = sourceId;
            this.SourceOrientation = sourceOrientation;
            this.SourceType = sourceType;
            this.SinkId = sinkId;
            this.SinkOrientation = sinkOrientation;
            this.SinkType = sinkType;
        }

        public int SourceId { get; private set; }
        public Orientation SourceOrientation { get; private set; }
        public Type SourceType { get; private set; }
        public int SinkId { get; private set; }
        public Orientation SinkOrientation { get; private set; }
        public Type SinkType { get; private set; }
    }

}
