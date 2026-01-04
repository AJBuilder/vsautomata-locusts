using LocustHives.Systems.Membership;
using System;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace LocustHives.Game.Core
{
    public class EntityBehaviorHiveTunable : EntityBehavior, IHiveMember
    {

        public TuningSystem modSystem;
        public Action<int?, int?> OnTuned { get; set; }

        public int? LocalHiveId
        {
            get => entity.WatchedAttributes.TryGetInt("hiveId");
            set
            {
                if (value.HasValue)
                {
                    entity.WatchedAttributes.SetInt("hiveId", value.Value);
                }
                else
                {
                    entity.WatchedAttributes.RemoveAttribute("hiveId");
                }

            }
        }


        public EntityBehaviorHiveTunable(Entity entity) : base(entity)
        {
            OnTuned += (_, newId) => LocalHiveId = newId;
        }

        public override void Initialize(EntityProperties properties, JsonObject attributes)
        {
            base.Initialize(properties, attributes);

            // We set the modsystem in initialize so that we don't call Tune in FromBytes.
            if (entity.Api is ICoreServerAPI)
            {
                modSystem = entity.Api.ModLoader.GetModSystem<TuningSystem>();
                if (!LocalHiveId.HasValue && attributes["createsHive"].AsBool()) LocalHiveId = modSystem.CreateHive();
            }
        }

        public override void AfterInitialized(bool onFirstSpawn)
        {
            base.AfterInitialized(onFirstSpawn);
            modSystem?.Tune(this, LocalHiveId);
        }

        public override void OnEntityDespawn(EntityDespawnData despawn)
        {
            base.OnEntityDespawn(despawn);
            modSystem?.Tune(this, null);
        }

        public override void GetInfoText(StringBuilder infotext)
        {
            var local = LocalHiveId;
            infotext.AppendLine($"Hive: {(!local.HasValue ? "None" : local.Value)}");
        }
        public override string PropertyName()
        {
            return "hiveworker";
        }

    }
}
