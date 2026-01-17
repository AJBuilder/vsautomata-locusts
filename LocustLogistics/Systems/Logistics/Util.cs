using LocustHives.Systems.Logistics.AccessMethods;
using LocustHives.Systems.Logistics.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace LocustHives.Systems.Logistics
{
    public static class Util
    {

        public static uint CanProvide(this IInventory inventory, ItemStack stack)
        {
            return (uint)inventory.Sum(slot =>
            {
                var s = slot.Itemstack;
                if (s == null || !s.Satisfies(stack)) return 0;
                else return s.StackSize;
            });
        }

        public static uint CanAccept(this IInventory inventory, ItemStack stack)
        {
            return (uint)Math.Max(0, inventory.Sum(slot =>
            {
                var s = slot.Itemstack;
                if (s == null) return stack.Collectible.MaxStackSize;
                else return s.Satisfies(stack) ? Math.Max(0, stack.Collectible.MaxStackSize - s.StackSize) : 0;
            }));
        }

        /// <summary>
        /// Returns how much of the operation can be performed.
        /// Positive stack size = how much room for receiving (Give)
        /// Negative stack size = how much available to provide (Take)
        /// Will not be more in quantity than the stack size.
        /// </summary>
        public static uint CanDo(this IInventory inventory, ItemStack stack)
        {
            if (stack.StackSize > 0)
            {
                // Give: check room
                return Math.Min((uint)stack.StackSize, CanAccept(inventory, stack));
            }
            else if (stack.StackSize < 0)
            {
                // Take: check available
                return Math.Min((uint)stack.StackSize, CanProvide(inventory, stack));
            }
            return 0;
        }

        public static ItemStack CloneWithSize(this ItemStack stack, int size)
        {
            var clone = stack.GetEmptyClone();
            clone.StackSize = size;
            return clone;
        }

        public static BlockPos InBlockPos(this Vec3d pos)
        {
            return new BlockPos((int)Math.Floor(pos.X), (int)Math.Floor(pos.Y), (int)Math.Floor(pos.Z));
        }
    }
}
