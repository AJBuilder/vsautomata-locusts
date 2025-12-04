using LocustLogistics.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace LocustLogistics.Core
{
    public class BEBehaviorHiveTunable : BlockEntityBehavior, IHiveMember
    {
        public event Action<int?, int?> OnTuned;

        int? hiveId;
        LocustHivesModSystem modSystem;

        public BEBehaviorHiveTunable(BlockEntity blockentity) : base(blockentity)
        {
        }


        public override void Initialize(ICoreAPI api, JsonObject properties)
        {
            base.Initialize(api, properties);
            modSystem = api.ModLoader.GetModSystem<LocustHivesModSystem>();

            if (!hiveId.HasValue) hiveId = modSystem.CreateHive();

            // Kinda hacky, but we have to delay tuning to hive other behaviors a change to register to the OnTuned event.
            api.Event.RegisterCallback((dt) => {
                modSystem.Tune(hiveId, this);
                }            , 0);
        }

        public override void OnBlockRemoved()
        {
            base.OnBlockRemoved();
            modSystem.Tune(null, this);
        }

        public override void OnBlockUnloaded()
        {
            base.OnBlockUnloaded();
            modSystem.Tune(null, this);
        }


        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
            if (hiveId.HasValue) tree.SetInt("hiveId", hiveId.Value);
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
        {
            base.FromTreeAttributes(tree, worldAccessForResolve);

            var id = tree.TryGetInt("hiveId");
            // If modSystem not set yet, then this is on-load. We'll do it later in Initialize.
            if (modSystem == null) hiveId = id;
            else if (id.HasValue != hiveId.HasValue ||
                id.HasValue && hiveId.HasValue && id != hiveId) modSystem?.Tune(id, this);
        }


        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder dsc)
        {
            dsc.AppendLine($"Hive: {(hiveId.HasValue ? hiveId.Value : "None")}");
        }

        public void WasTuned(int? prevHive, int? newHive)
        {
            hiveId = newHive;
            OnTuned?.Invoke(prevHive, newHive);
        }

    }
}
