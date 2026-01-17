using LocustHives.Systems.Logistics.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace LocustHives.Systems.Logistics.Core.Interfaces
{
    public interface IStorageAccessMethod
    {
        /// <summary>
        /// The priority of using this method over others for the same storage.
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// Returns how much of the operation can be performed.
        ///
        /// Stack sign indicates operation:
        /// - Positive = Give (check room)
        /// - Negative = Take (check available)
        /// </summary>
        uint CanDo(ItemStack stack);

    }

    public interface IInWorldStorageAccessMethod : IStorageAccessMethod
    {
        Vec3d Position { get; }
    }
}
