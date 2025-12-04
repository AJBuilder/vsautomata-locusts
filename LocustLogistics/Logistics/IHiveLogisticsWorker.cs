using LocustLogistics.Logistics.Retrieval;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

#nullable disable warnings

namespace LocustLogistics.Logistics
{
    public interface IHiveLogisticsWorker
    {
        IInventory Inventory { get; }
        Vec3d Position { get; }
        public bool TryAssignRetrievalRequest(RetrievalRequest request);

    }
}
