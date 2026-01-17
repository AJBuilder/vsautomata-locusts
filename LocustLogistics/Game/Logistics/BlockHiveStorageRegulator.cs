using LocustHives.Systems.Logistics.Core.Interfaces;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace LocustHives.Game.Logistics
{
    public class BlockHiveStorageRegulator : Block
    {
        const int ITEM_BOX = 2;
        const int INCREMENT_BOX = 3;
        const int DECREMENT_BOX = 4;

        public override bool DoParticalSelection(IWorldAccessor world, BlockPos pos)
        {
            return true;
        }

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            if (world.Side != EnumAppSide.Server) return true;

            var gauge = world.BlockAccessor.GetBlockEntity(blockSel.Position)?.GetBehavior<BEBehaviorHiveStorageRegulator>();
            if (gauge == null) return base.OnBlockInteractStart(world, byPlayer, blockSel);

            var heldStack = byPlayer.InventoryManager.ActiveHotbarSlot?.Itemstack;

            switch (blockSel.SelectionBoxIndex)
            {
                case ITEM_BOX:
                    if (heldStack != null)
                    {
                        gauge.TrackedItem = heldStack;
                    }
                    else
                    {
                        gauge.TrackedItem = null;
                    }
                    return true;

                case INCREMENT_BOX:
                    gauge.TargetCount++;
                    return true;

                case DECREMENT_BOX:
                    if (gauge.TargetCount > 1)
                    {
                        gauge.TargetCount--;
                    }
                    return true;

                default:
                    return base.OnBlockInteractStart(world, byPlayer, blockSel);
            }
        }
    }
}
