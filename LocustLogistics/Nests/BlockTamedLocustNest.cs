using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace LocustLogistics.Nests
{
    public class BlockTamedLocustNest : Block
    {
        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            base.OnBlockInteractStart(world, byPlayer, blockSel);
            var success = GetBlockEntity<BlockEntity>(blockSel).GetBehavior<ILocustNest>()?.TryUnstoreLocust(0);
            if (success.HasValue && !success.Value && api is ICoreClientAPI capi) capi.TriggerIngameError(this, "Failed to unstore locust", "No stored Locusts");
            return true;
        }
    }
}
