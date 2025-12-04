using LocustLogistics.Core;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace LocustLogistics.Logistics.Storage
{
    public interface IHiveStorage
    {
        Vec3d Position { get; }
        IInventory Inventory { get; }
    }
}
