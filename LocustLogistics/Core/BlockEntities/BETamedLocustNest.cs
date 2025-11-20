using LocustLogistics.Core;
using LocustLogistics.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace LocustLogistics.Core.BlockEntities
{
    public class BETamedLocustNest : BlockEntity, ILocustNest
    {
        ICoreAPI api;
        AutomataLocustsCore modSystem;
        HashSet<EntityLocust> locusts;
        public LocustHive Hive { get; set; }

        public ISet<EntityLocust> StoredLocusts => locusts;

        public Vec3d Position => Pos.ToVec3d();

        public int Dimension => Pos.dimension;

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
            this.api = api;
            modSystem = api.ModLoader.GetModSystem<AutomataLocustsCore>();

        }
        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            tree.SetLong("hiveId", Hive.Id);
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
        {
            var id = tree.TryGetLong("hiveId");
            if (id.HasValue)
            {
                Hive = modSystem.GetHive(id.Value);
            } else
            {
                Hive = modSystem.CreateHive();
            }
        }
    }
}
