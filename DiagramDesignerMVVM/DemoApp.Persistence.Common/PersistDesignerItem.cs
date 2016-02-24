using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DemoApp.Persistence.Common
{
    public class PersistDesignerItem : DesignerItemBase
    {
        public PersistDesignerItem(int id, double left, double top, string hostUrl) : base(id, left, top)
        {
            this.HostUrl = hostUrl;
        }

        public string HostUrl { get; set; }

    }
}
