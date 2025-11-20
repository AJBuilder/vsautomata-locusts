using LocustLogistics.Core;
using LocustLogistics.Logistics.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace LocustLogistics.TransferItems.BlockEntities
{
    public class HiveInventoryBeacon : BlockEntity, IHiveStorage
    {
        public IInventory Inventory {
            get
            {
                BlockPos targetPos = Pos.DownCopy();
                return (Api.World.BlockAccessor.GetBlockEntity(targetPos) as IBlockEntityContainer).Inventory;
            }
        }

        public Vec3d Position => Pos.ToVec3d();

        public int Dimension => Pos.dimension;

        public LocustHive Hive { get; set; }

    }
}
