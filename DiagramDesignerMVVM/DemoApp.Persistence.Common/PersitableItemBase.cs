using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DemoApp.Persistence.Common
{
    public abstract class PersistableItemBase
    {
        public PersistableItemBase()
        {

        }

        public PersistableItemBase(int id)
        {
            this.Id = id;
        }

        public int Id { get; set; }
    }
}
