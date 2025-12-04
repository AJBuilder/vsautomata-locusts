using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace LocustLogistics.Logistics.Storage
{
    public class BlockHiveStorageBeacon : Block
    {
        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            // Open GUI on right-click
            BlockEntity be = world.BlockAccessor.GetBlockEntity(blockSel.Position);
            if (be != null)
            {
                var behavior = be.GetBehavior<BEBehaviorHiveStorageBeacon>();
                if (behavior != null)
                {
                    return behavior.OnPlayerRightClick(byPlayer, blockSel);
                }
            }

            return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }

        public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
        {
            // Don't drop the contents of the proxied inventory - just drop the beacon itself
            base.OnBlockBroken(world, pos, byPlayer, dropQuantityMultiplier);
        }
    }
}
