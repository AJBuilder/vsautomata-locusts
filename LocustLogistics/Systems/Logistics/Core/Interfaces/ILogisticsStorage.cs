using LocustHives.Systems.Logistics.Core;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace LocustHives.Systems.Logistics.Core.Interfaces
{

    /// <summary>
    /// A logistics storage can have logistics operations performed on it.
    /// </summary>
    public interface ILogisticsStorage
    {
        IEnumerable<ItemStack> Stacks {get;}

        IEnumerable<IStorageAccessMethod> AccessMethods { get; }

    }
}
