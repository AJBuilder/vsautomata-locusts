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
using Vintagestory.API.Datastructures;
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
                foreach(var face in AvailableFaces())
                {
                    yield return new BlockFaceAccessible(Pos, face, 0, CanDo, TryTakeOut, TryPutInto);
                }

                foreach (var connected in TraverseConnected())
                {
                    foreach(var face in connected.AvailableFaces())
                    {
                        yield return new BlockFaceAccessible(connected.Pos, face, 0, CanDo, TryTakeOut, TryPutInto);
                    }
                }
            }
        }

        public override void Initialize(ICoreAPI api)
        {
            if(inventory == null) InitInventory();

            base.Initialize(api);


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
                reservations = new HashSet<LogisticsReservation>();
            }
        }

        protected void InitInventory()
        {
            var quantitySlots = Block.Attributes["quantitySlots"].AsInt();
            inventory = new InventoryGeneric(quantitySlots, null, null);
            // TODO: Inventory lock and weight stuff
            if (Api is ICoreServerAPI)
            {
                inventory.SlotModified += (int obj) => MarkDirty(false);
            }
        }

        private IEnumerable<BlockFacing> AvailableFaces()
        {
            foreach (var face in BlockFacing.ALLFACES)
            {
                if (Api.World.BlockAccessor.GetBlock(Pos.AddCopy(face)).Id == 0)
                {
                    // Only faces that are air
                    yield return face;
                }
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
            bool isTake = stack.StackSize < 0;
            var reserved = (uint)reservations
                .Where(r => r.Stack.Satisfies(stack) && (r.Stack.StackSize < 0) == isTake)
                .Sum(r => Math.Abs(r.Stack.StackSize));
            return Math.Max(0, inventory.CanDo(stack) - reserved); ;
        }

        private uint TryTakeOut(ItemStack stack, ItemSlot sinkSlot)
        {
            // This method doesn't acutally transfer at the one it is closest too!
            uint quantity = (uint)Math.Abs(stack.StackSize);
            return inventory.TryTakeMatching(Api.World, stack, sinkSlot, quantity);
        }

        private uint TryPutInto(ItemSlot sourceSlot, uint quantity)
        {
            return inventory.TryPutIntoBestSlots(Api.World, sourceSlot, quantity);
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

        public IEnumerable<BEHiveLattice> TraverseConnected(ISet<BlockPos> visited = null)
        {
            if (visited == null) visited = new HashSet<BlockPos>();
            visited.Add(Pos);

            var queue = new Queue<BEHiveLattice>();

            // Seed queue with immediate neighbors
            foreach (var face in BlockFacing.ALLFACES)
            {
                var be = Api.World.BlockAccessor.GetBlockEntity<BEHiveLattice>(Pos.AddCopy(face));
                if (be != null && visited.Add(be.Pos))
                {
                    queue.Enqueue(be);
                }
            }

            // BFS traversal
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                yield return current;

                foreach (var face in BlockFacing.ALLFACES)
                {
                    var be = Api.World.BlockAccessor.GetBlockEntity<BEHiveLattice>(current.Pos.AddCopy(face));
                    if (be != null && visited.Add(be.Pos))
                    {
                        queue.Enqueue(be);
                    }
                }
            }
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
        {
            if (inventory == null) InitInventory();
            base.FromTreeAttributes(tree, worldForResolving);
        }
    }
}
