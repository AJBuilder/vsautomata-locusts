using System;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;

namespace LocustLogistics.Core
{
    public class EntityBehaviorHiveTunable : EntityBehavior, IHiveMember
    {
        public event Action<int?, int?> OnTuned;

        public int? hiveId;
        public LocustHivesModSystem hivesSystem;


        public EntityBehaviorHiveTunable(Entity entity) : base(entity)
        {
            hivesSystem = entity.Api.ModLoader.GetModSystem<LocustHivesModSystem>();
        }

        public override void AfterInitialized(bool onFirstSpawn)
        {
            base.AfterInitialized(onFirstSpawn);
            hivesSystem.Tune(hiveId, this);
        }

        public override void OnEntityDespawn(EntityDespawnData despawn)
        {
            base.OnEntityDespawn(despawn);
             hivesSystem.Tune(null, this);

        }

        public override void ToBytes(bool forClient)
        {
            if(hiveId.HasValue)
            {
                entity.WatchedAttributes.SetInt("hiveId", hiveId.Value);
            }
        }

        public override void FromBytes(bool isSync)
        {
            var id = entity.WatchedAttributes.TryGetInt("hiveId");
            // If modSystem not set yet, then this is on-load. We'll do it later in Initialize.
            if(hivesSystem == null) hiveId = id;
            else if (id.HasValue != hiveId.HasValue ||
                id.HasValue && hiveId.HasValue && id != hiveId) hivesSystem?.Tune(id, this);
        }

        public override void GetInfoText(StringBuilder infotext)
        {
            infotext.AppendLine($"Hive: {(hiveId == null ? "None" : hiveId)}");
        }
        public override string PropertyName()
        {
            return "hiveworker";
        }
        public void WasTuned(int? prevHive, int? newHive)
        {
            hiveId = newHive;
            OnTuned?.Invoke(prevHive, newHive);
        }

    }
}
