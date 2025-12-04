using LocustLogistics.Core;
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

namespace LocustLogistics.Nests
{
    public class BEBehaviorHiveLocustNest : BlockEntityBehavior, ILocustNest
    {
        List<(string code, byte[] data)> storedLocustData;

        public IEnumerable<EntityLocust> StoredLocusts
        {
            get
            {
                if (Api == null) yield break;

                foreach (var (code, data) in storedLocustData) yield return CreateEntityClass(code, data);
            }
        }
        public int MaxCapacity => 5;
        public bool HasRoom => storedLocustData.Count < MaxCapacity;

        public Vec3d Position => Pos.ToVec3d().Add(0.5f, -0.5f, 0.5f);

        public int Dimension => Blockentity.Pos.dimension;

        public BEBehaviorHiveLocustNest(BlockEntity blockentity) : base(blockentity)
        {
            storedLocustData = new List<(string code, byte[] data)>();
        }

        public override void Initialize(ICoreAPI api, JsonObject properties)
        {
            base.Initialize(api, properties);
            var modSystem = api.ModLoader.GetModSystem<LocustNestModSystem>();
            Blockentity.GetBehavior<BEBehaviorHiveTunable>().OnTuned += (int? prevHive, int? hive) =>
            {
                modSystem.UpdateNestHiveMembership(this, prevHive, hive);
            };
        }

        public override void OnBlockRemoved()
        {
            base.OnBlockRemoved();
            for(var i = storedLocustData.Count; i > 0; i--)
            {
                TryUnstoreLocust(i);
            }
        }

        public bool TryStoreLocust(EntityLocust locust)
        {
            if (storedLocustData.Count >= MaxCapacity) return false;

            // Serialize the locust to raw data
            byte[] data = SerializerUtil.ToBytes((writer) => locust.ToBytes(writer, false));
            string code = locust.Code.ToString();

            storedLocustData.Add((code, data));

            // Despawn the locust from the world
            locust.Die(EnumDespawnReason.PickedUp, null);

            Blockentity.MarkDirty(true);
            return true;
        }

        public bool TryUnstoreLocust(int index)
        {
            if (index < 0 || index >= storedLocustData.Count) return false;

            // Get and remove the raw data entry
            var (code, data) = storedLocustData[index];
            storedLocustData.RemoveAt(index);

            // Create a fresh EntityLocust from the raw data
            var entity = CreateEntityClass(code, data);

            // Spawn the entity at the nest position
            entity.ServerPos.SetPosWithDimension(Position);
            entity.Pos.SetFrom(entity.ServerPos);
            Api.World.SpawnEntity(entity);

            entity.Attributes.SetLong("unstoredMs", Api.World.ElapsedMilliseconds);

            Blockentity.MarkDirty(true);
            return true;
        }

        private EntityLocust CreateEntityClass(string code, byte[] bytes)
        {
            var entityType = Api.World.GetEntityType(new AssetLocation(code));
            var entity = Api.World.ClassRegistry.CreateEntity(entityType) as EntityLocust;
            SerializerUtil.FromBytes(bytes, (reader) => entity.FromBytes(reader, false));
            return entity;
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
            for (int i = 0; i < storedLocustData.Count; i++)
            {
                var (code, data) = storedLocustData[i];
                tree.SetBytes($"locust_{i}_data", data);
                tree.SetString($"locust_{i}_code", code);
            }
            tree.SetInt("locustCount", storedLocustData.Count);
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
        {
            base.FromTreeAttributes(tree, worldAccessForResolve);

            int count = tree.GetInt("locustCount");

            storedLocustData = Enumerable.Range(0, count)
                .Select(i => (
                    code: tree.GetString($"locust_{i}_code"),
                    data: tree.GetBytes($"locust_{i}_data")
                ))
                .Where(x => !string.IsNullOrEmpty(x.code) && x.data != null)
                .ToList();
        }


        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder dsc)
        {
            base.GetBlockInfo(forPlayer, dsc);
            dsc.AppendLine($"Locusts: {storedLocustData.Count}/{MaxCapacity}");
        }

    }
}
