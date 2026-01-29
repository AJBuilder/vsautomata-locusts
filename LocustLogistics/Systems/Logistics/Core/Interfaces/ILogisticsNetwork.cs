using LocustHives.Systems.Logistics.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;

namespace LocustHives.Systems.Logistics.Core.Interfaces
{
    public interface ILogisticsNetwork
    {
        IEnumerable<ILogisticsWorker> Workers { get; }
        IEnumerable<ILogisticsStorage> Storages { get; }
        LogisticsPromise Request(ItemStack stack, ILogisticsStorage target, bool blocking = true);
    }
}
