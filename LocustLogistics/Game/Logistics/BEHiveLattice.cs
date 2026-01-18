using LocustHives.Game.Core;
using LocustHives.Systems.Logistics;
using LocustHives.Systems.Logistics.AccessMethods;
using LocustHives.Systems.Logistics.Core;
using LocustHives.Systems.Logistics.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace LocustHives.Game.Logistics
{
    // Making this a BlockEntity rather than a behavior in order to reuse BlockEntityContainer. I hate inheritance... >:(
    internal class BEHiveLattice : BlockEntityContainer, ILogisticsStorage
    {
        InventoryBase inventory;
        HashSet<LogisticsReservation> reservations;

        public override InventoryBase Inventory => inventory;
        IInventory ILogisticsStorage.Inventory => inventory;

        public override string InventoryClassName => "hivelattice";


        public IEnumerable<IStorageAccessMethod> AccessMethods
        {
            get
            {
                foreach (var face in BlockFacing.ALLFACES)
                {
                    if(Api.World.BlockAccessor.GetBlock(Pos.AddCopy(face)).Id == 0)
                    {
                        // Only faces that are airs
                        yield return new BlockFaceAccessible(Pos, face, 0, CanDo);
                    }
                }
            }
        }

        public override void Initialize(ICoreAPI api)
        {
            var quantitySlots = Block.Attributes["quantitySlots"].AsInt();
            inventory = new InventoryGeneric(quantitySlots, InventoryClassName + "-" + Pos, api);

            base.Initialize(api);

            // TODO: Inventory lock and weight stuff

            var tunableBehavior = GetBehavior<BEBehaviorLocustHiveTunable>();
            if (tunableBehavior != null)
            {
                tunableBehavior.OnTuned += (prevHive, hive) =>
                {
                    api.ModLoader.GetModSystem<LogisticsSystem>().UpdateLogisticsStorageMembership(this, hive);
                };
            }

            if (api is ICoreServerAPI)
            {
                inventory.SlotModified += (int obj) => MarkDirty(false);
                reservations = new HashSet<LogisticsReservation>();
            }
        }

        public LogisticsReservation TryReserve(ItemStack stack)
        {
            var available = CanDo(stack);
            if (available >= (uint)Math.Abs(stack.StackSize))
            {
                var reservation = new LogisticsReservation(stack, this);
                reservations.Add(reservation);
                reservation.ReleasedEvent += () =>
                {
                    reservations.Remove(reservation);
                };
                return reservation;
            }
            return null;
        }

        private uint CanDo(ItemStack stack)
        {
            var inventory = Inventory;
            if (inventory == null) return 0;
            bool isTake = stack.StackSize < 0;
            var reserved = (uint)reservations
                .Where(r => r.Stack.Satisfies(stack) && (r.Stack.StackSize < 0) == isTake)
                .Sum(r => Math.Abs(r.Stack.StackSize));
            var able = inventory.CanDo(stack);
            return (uint)Math.Max(0, (int)able - (int)reserved);
        }

        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder dsc)
        {
            base.GetBlockInfo(forPlayer, dsc);

            if (!inventory.Empty)
            {
                dsc.AppendLine($"Contains: \n{string.Join("\n", inventory
                    .Where(s => !s.Empty)
                    .Select(s => $"{s.Itemstack.StackSize}x {s.Itemstack.GetName()}"))}");
            }
            else
            {
                dsc.AppendLine(Lang.Get("Empty"));
            }
        }
    }
}
