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

        /// <summary>
        /// Attempts to take items matching the stack from this storage into the sink slot.
        /// Uses stack.StackSize (absolute value) as the quantity to take.
        /// </summary>
        /// <param name="stack">The item type and quantity to take (uses Satisfies for matching)</param>
        /// <param name="sinkSlot">The slot to receive items</param>
        /// <returns>Amount actually transferred</returns>
        uint TryTakeOut(ItemStack stack, ItemSlot sinkSlot);

        /// <summary>
        /// Attempts to put items from the source slot into this storage.
        /// </summary>
        /// <param name="sourceSlot">The slot providing items</param>
        /// <param name="quantity">Maximum quantity to put</param>
        /// <returns>Amount actually transferred</returns>
        uint TryPutInto(ItemSlot sourceSlot, uint quantity);

    }

    public interface IInWorldStorageAccessMethod : IStorageAccessMethod
    {
        Vec3d Position { get; }
    }
}
