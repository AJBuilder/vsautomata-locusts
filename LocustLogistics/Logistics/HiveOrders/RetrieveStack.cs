using LocustLogistics.Logistics.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;

namespace LocustLogistics.Logistics.HiveOrders
{

    /// <summary>
    /// An order to put an item stack from a hive storage
    /// </summary>
    public class PutStack
    {
        public ItemStack Target { get; }
        public IHiveStorage To { get; }

        public PutStack(ItemStack stack, IHiveStorage to) {
            Target = stack;
            To = to;
        }

    }
}
