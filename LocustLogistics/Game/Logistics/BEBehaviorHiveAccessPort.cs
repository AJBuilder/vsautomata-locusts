using LocustHives.Game.Core;
using LocustHives.Systems.Logistics;
using LocustHives.Systems.Logistics.AccessMethods;
using LocustHives.Systems.Logistics.Core;
using LocustHives.Systems.Logistics.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Util;

namespace LocustHives.Game.Logistics
{



    public class BEBehaviorHiveAccessPort : BlockEntityBehavior, ILogisticsStorage
    {
        // Faces towards the inventory. Access opening is opposite of facing.
        BlockFacing facing;
        HashSet<LogisticsReservation> reservations;

        public IInventory Inventory
        {
            get
            {
                if (facing == null) return null;
                BlockPos targetPos = Pos.AddCopy(facing);
                return (Api.World.BlockAccessor.GetBlockEntity(targetPos) as IBlockEntityContainer)?.Inventory;
            }
        }


        public IEnumerable<IStorageAccessMethod> AccessMethods
        {
            get
            {
                yield return new BlockFaceAccessible(Blockentity.Pos, facing.Opposite, 0, CanDo);
            }
        }

        public BEBehaviorHiveAccessPort(BlockEntity blockentity) : base(blockentity)
        {
        }

        public override void Initialize(ICoreAPI api, JsonObject properties)
        {
            base.Initialize(api, properties);

            var facingCode = properties["facingCode"].AsString();
            facing = BlockFacing.FromCode(Blockentity.Block.Variant[facingCode]);

            var tunableBehavior = Blockentity.GetBehavior<BEBehaviorLocustHiveTunable>();
            if (tunableBehavior != null)
            {
                tunableBehavior.OnTuned += (prevHive, hive) =>
                {
                    api.ModLoader.GetModSystem<LogisticsSystem>().UpdateLogisticsStorageMembership(this, hive);
                };
            }

            if (api is ICoreServerAPI)
            {
                reservations = new HashSet<LogisticsReservation>();
            }
        }

        public LogisticsReservation TryReserve(ItemStack stack, LogisticsOperation operation)
        {
            var available = CanDo(stack, operation);
            if (available >= stack.StackSize)
            {
                var reservation = new LogisticsReservation(stack, this, operation);
                reservations.Add(reservation);
                reservation.ReleasedEvent += () =>
                {
                    reservations.Remove(reservation);
                };
                return reservation;
            }
            return null;
        }

        private uint CanDo(ItemStack stack, LogisticsOperation op)
        {
            var inventory = Inventory;
            if (inventory == null) return 0;
            var reserved = (uint)reservations.Where(r => r.Stack.Satisfies(stack) && r.Operation == op).Sum(r => r.Stack.StackSize);
            var able = op switch
            {
                LogisticsOperation.Take => inventory.CanProvide(stack),
                LogisticsOperation.Give => inventory.CanAccept(stack),
            };
            return Math.Max(0, able - reserved);
        }

        public override void OnBlockRemoved()
        {
            base.OnBlockRemoved();
            DisconnectFromLogistics();
        }

        public override void OnBlockUnloaded()
        {
            base.OnBlockUnloaded();
            DisconnectFromLogistics();
        }

        public void DisconnectFromLogistics()
        {
            reservations?.Foreach(r => r.Release());
        }
    }
}
