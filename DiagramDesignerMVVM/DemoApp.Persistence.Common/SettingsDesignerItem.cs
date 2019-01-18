using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DemoApp.Persistence.Common
{
    public class SettingsDesignerItem: DesignerItemBase
    {
        public SettingsDesignerItem(int id, double left, double top, double itemWidth, double itemHeight, string setting1)
            : base(id, left, top, itemWidth, itemHeight)
        {
            this.Setting1 = setting1;
        }

        public string Setting1 { get; set; }
    }
}
