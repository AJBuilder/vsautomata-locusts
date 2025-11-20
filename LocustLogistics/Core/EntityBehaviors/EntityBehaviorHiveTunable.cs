using Vintagestory.API.Common.Entities;

namespace LocustLogistics.Core.EntityBehaviors
{
    public class EntityBehaviorHiveTunable : EntityBehavior
    {
        LocustHive hive;
        AutomataLocustsCore modSystem;
        public LocustHive Hive { get; set; }

        public EntityBehaviorHiveTunable(Entity entity) : base(entity)
        {
            modSystem = entity.Api.ModLoader.GetModSystem<AutomataLocustsCore>();
        }

        public override void ToBytes(bool forClient)
        {
            if(hive != null)
            {
                entity.WatchedAttributes.SetLong("hiveId", hive.Id);
            }
        }

        public override void FromBytes(bool isSync)
        {
            var id = entity.WatchedAttributes.TryGetLong("hiveId");
            if (id.HasValue)
            {
                hive = modSystem.GetHive(id.Value);
            } else
            {
                hive = modSystem.CreateHive();
            }
        }
        public override string PropertyName()
        {
            return "hiveworker";
        }

    }
}
