using LocustLogistics.Core;
using LocustLogistics.Logistics.Retrieval;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;

namespace LocustLogistics.Logistics
{
    public class EntityBehaviorLogisticsWorker : EntityBehavior, IHiveLogisticsWorker
    {
        RetrievalRequest retrievalRequest;
        public IInventory Inventory { get; }
        public Vec3d Position => entity.Pos.XYZ;
        public RetrievalRequest AssignedRetrievalRequest { get; }

        public EntityBehaviorLogisticsWorker(Entity entity) : base(entity)
        {
            Inventory = new InventoryGeneric(1, $"logisticsworker-{entity.GetName()}-{entity.EntityId}", entity.Api);
        }

        public override void Initialize(EntityProperties properties, JsonObject attributes)
        {
            base.Initialize(properties, attributes);
            entity.GetBehavior<EntityBehaviorHiveTunable>().OnTuned += (int? prevHive, int? hive) =>
            {
                entity.Api.ModLoader.GetModSystem<LogisticsWorkersModSystem>().UpdateLogisticsWorkerHiveMembership(this, prevHive, hive);
            };
        }

        public override void FromBytes(bool isSync)
        {
            base.FromBytes(isSync);
        }

        public override void ToBytes(bool forClient)
        {
            base.ToBytes(forClient);
        }

        public override string PropertyName()
        {
            return "hivelogisticsworker";
        }

        public bool TryAssignRetrievalRequest(RetrievalRequest request)
        {
            // NOTE: This logic assumes empty == can't take more items. Ok for now, technically incorrect.
            if (retrievalRequest != null || !Inventory.Empty) return false;
            retrievalRequest = request;
            request.CancelledEvent += () =>
                {
                    retrievalRequest = null;
                };
            return true;
        }

    } 
}
