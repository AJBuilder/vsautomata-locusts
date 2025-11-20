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
    /// An order to get an item stack from a hive storage
    /// </summary>
    public class RetrieveStack
    {
        public ItemStack Target { get; }
        public IHiveStorage From { get; }

        public RetrieveStack(ItemStack stack, IHiveStorage from) {
            Target = stack;
            From = from;
        }

    }
}
