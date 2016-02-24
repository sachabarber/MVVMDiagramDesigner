using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DemoApp.Persistence.Common
{
    public abstract class DesignerItemBase : PersistableItemBase
    {
        public DesignerItemBase(int id, double left, double top) : base(id)
        {
            this.Left = left;
            this.Top = top;
        }

        public double Left { get; private set; }
        public double Top { get; private set; }

    }
}
