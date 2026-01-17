using LocustHives.Game.Util;
using LocustHives.Systems.Logistics;
using LocustHives.Systems.Logistics.Core;
using LocustHives.Systems.Logistics.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace LocustHives.Game.Logistics
{
    public class BEBehaviorHiveStorageRegulator : BlockEntityBehavior, IStorageRegulator
    {
        ItemStack trackedItem;
        uint targetCount = 1;
        BlockFacing facing;
        LogisticsSystem modSystem;
        List<LogisticsPromise> promises;

        public ItemStack TrackedItem
        {
            get => trackedItem?.Clone();
            set
            {
                trackedItem = value?.Clone();
                if (trackedItem != null)
                {
                    trackedItem.StackSize = 1;
                }
                Blockentity.MarkDirty();
                CheckInventoryLevel();
            }
        }

        public uint TargetCount
        {
            get => targetCount;
            set
            {
                if (trackedItem != null)
                {
                    int maxStack = trackedItem.Collectible.MaxStackSize;
                    targetCount = Math.Max(1u, Math.Min(value, (uint)maxStack));
                }
                else
                {
                    targetCount = Math.Max(1u, value);
                }
                Blockentity.MarkDirty();
                CheckInventoryLevel();
            }
        }

        public ILogisticsStorage AttachedStorage
        {
            get
            {
                if (facing == null) return null;
                BlockPos targetPos = Pos.AddCopy(facing.Opposite);
                var be = Api.World.BlockAccessor.GetBlockEntity(targetPos);
                return be?.GetAs<ILogisticsStorage>();
            }
        }

        public BEBehaviorHiveStorageRegulator(BlockEntity blockentity) : base(blockentity)
        {
        }

        public override void Initialize(ICoreAPI api, JsonObject properties)
        {
            base.Initialize(api, properties);

            if (trackedItem != null && !trackedItem.ResolveBlockOrItem(Api.World)) trackedItem = null;

            if (api is ICoreServerAPI serverAPI)
            {
                promises = new List<LogisticsPromise>();

                var facingCode = properties["facingCode"].AsString();
                facing = BlockFacing.FromCode(Blockentity.Block.Variant[facingCode]);

                modSystem = api.ModLoader.GetModSystem<LogisticsSystem>();

                Blockentity.RegisterGameTickListener((dt) =>
                {
                    CheckInventoryLevel();
                }, 3000);
            }
        }

        private void CheckInventoryLevel()
        {
            if (trackedItem == null) return;

            var storage = AttachedStorage;
            if(storage == null) return;

            var inventory = storage.Inventory;
            if (inventory == null) return;

            // Get current level using CanProvide
            uint currentLevel = (uint)inventory.CanProvide(trackedItem);

            // Calculate unfulfilled amount (accounting for active promises)
            uint promisedAmount = (uint)promises
                .Where(p => p.State == LogisticsPromiseState.Unfulfilled)
                .Sum(p => p.Stack.StackSize);

            if (currentLevel > targetCount)
            {
                // Too many items - push excess
                uint excess = currentLevel - targetCount - promisedAmount;
                if (excess > 0)
                {
                    var stack = trackedItem.Clone();
                    stack.StackSize = (int)excess;

                    if (modSystem.StorageMembership.GetMembershipOf(storage, out var hiveId))
                    {
                        var promise = modSystem.GetNetworkFor(hiveId)?.Push(stack, AttachedStorage);
                        if (promise != null)
                        {
                            promise.CompletedEvent += (state) =>
                            {
                                promises.Remove(promise);
                                Blockentity.MarkDirty();
                                CheckInventoryLevel();
                            };
                            promises.Add(promise);
                            Blockentity.MarkDirty();
                        }
                    }
                }
            }
            else if (currentLevel < targetCount)
            {
                // Too few items - pull deficit
                uint deficit = targetCount - currentLevel - promisedAmount;
                if (deficit > 0)
                {
                    var stack = trackedItem.Clone();
                    stack.StackSize = (int)deficit;
                    if (modSystem.StorageMembership.GetMembershipOf(storage, out var hiveId))
                    {
                        var promise = modSystem.GetNetworkFor(hiveId)?.Pull(stack, AttachedStorage);
                        if (promise != null)
                        {
                            promise.CompletedEvent += (state) =>
                            {
                                promises.Remove(promise);
                                Blockentity.MarkDirty();
                                CheckInventoryLevel();
                            };
                            promises.Add(promise);
                            Blockentity.MarkDirty();
                        }
                    }
                }
            }
        }

        public void Cleanup()
        {
            if(Api is ICoreServerAPI)
            {
                promises.ForEach(r => r.Cancel());
                promises.Clear();

            }
        }

        public override void OnBlockRemoved()
        {
            base.OnBlockRemoved();
            if (Api is ICoreServerAPI)
            {
                Cleanup();
            }
        }

        public override void OnBlockUnloaded()
        {
            base.OnBlockUnloaded();
            if (Api is ICoreServerAPI)
            {
                Cleanup();
            }
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);

            if (trackedItem != null)
            {
                tree.SetItemstack("trackedItem", trackedItem);
            }
            tree.SetInt("targetCount", (int)targetCount);
            tree.SetInt("promiseCount", promises?.Count ?? 0);
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
        {
            base.FromTreeAttributes(tree, worldAccessForResolve);

            trackedItem = tree.GetItemstack("trackedItem");
            if(Api != null && trackedItem != null && !trackedItem.ResolveBlockOrItem(Api.World)) trackedItem = null;
            targetCount = (uint)tree.GetInt("targetCount", 1);
        }

        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder dsc)
        {
            base.GetBlockInfo(forPlayer, dsc);

            if (trackedItem != null)
            {
                dsc.AppendLine($"Tracking: {trackedItem.GetName()}");
                dsc.AppendLine($"Target: {targetCount}");
                var inventory = AttachedStorage?.Inventory;
                if (inventory != null)
                {
                    uint currentLevel = (uint)inventory.CanProvide(trackedItem);
                    dsc.AppendLine($"Current: {currentLevel}");
                }
                dsc.AppendLine($"Active promises: {promises?.Count ?? 0}");
            }
            else
            {
                dsc.AppendLine("Not configured");
            }
        }
    }
}
