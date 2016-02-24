using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DemoApp.Persistence.Common
{
    public class DiagramItem : PersistableItemBase
    {
        public DiagramItem() 
        {
            this.DesignerItems = new List<DiagramItemData>();
            this.ConnectionIds = new List<int>();
        }

        public List<DiagramItemData> DesignerItems { get; set; }
        public List<int> ConnectionIds { get; set; }
    }


    public class DiagramItemData
    {
        public DiagramItemData(int itemId, Type itemType)
        {
            this.ItemId = itemId;
            this.ItemType = itemType;
        }

        public int ItemId { get; set; }
        public Type ItemType { get; set; }
    }  
}
