using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DemoApp.Persistence.Common
{
    public abstract class DesignerItemBase : PersistableItemBase
    {
        public DesignerItemBase(int id, double left, double top, double itemWidth, double itemHeight) : base(id)
        {
            this.Left = left;
            this.Top = top;
            this.ItemWidth = itemWidth;
            this.ItemHeight = itemHeight;
        }

        public double ItemHeight { get; private set; }
        public double ItemWidth { get; private set; }
        public double Left { get; private set; }
        public double Top { get; private set; }

    }
}
