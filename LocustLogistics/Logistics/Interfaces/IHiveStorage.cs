using LocustLogistics.Core.Interfaces;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace LocustLogistics.Logistics.Interfaces
{
    public interface IHiveStorage : IHiveMember
    {
        IInventory Inventory { get; }
    }
}
