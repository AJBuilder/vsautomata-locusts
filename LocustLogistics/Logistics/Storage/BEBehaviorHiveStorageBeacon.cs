using LocustLogistics.Core;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace LocustLogistics.Logistics.Storage
{
    public enum EnumStorageBeaconPacketId
    {
        OpenGui = 1000,
        CloseGui = 1001,
        SlotToggled = 1002,
        FilterChanged = 1003,
        RequestModeChanged = 1004,
        RequestAdded = 1005,
        RequestCleared = 1006,
        PushModeChanged = 1007
    }

    public class BEBehaviorHiveStorageBeacon : BlockEntityBehavior, IHiveStorage
    {
        private InventoryGeneric configInventory;
        private HiveStorageSettingsGui settingsDialog;

        public Vec3d Position => Blockentity.Pos.ToVec3d();

        public IInventory Inventory
        {
            get
            {
                BlockPos targetPos = Pos.DownCopy();
                return (Api.World.BlockAccessor.GetBlockEntity(targetPos) as IBlockEntityContainer)?.Inventory;
            }
        }

        public BEBehaviorHiveStorageBeacon(BlockEntity blockentity) : base(blockentity)
        {
            configInventory = new InventoryGeneric(12, "storagebeacon-config", null, null);
        }

        public override void Initialize(ICoreAPI api, JsonObject properties)
        {
            base.Initialize(api, properties);

            configInventory.LateInitialize("storagebeacon-config-" + Pos, api);

            var tunableBehavior = Blockentity.GetBehavior<BEBehaviorHiveTunable>();
            if (tunableBehavior != null)
            {
                tunableBehavior.OnTuned += (prevHive, hive) =>
                {
                    api.ModLoader.GetModSystem<StorageModSystem>().UpdateStorageHiveMembership(this, prevHive, hive);
                };
            }
        }

        public bool OnPlayerRightClick(IPlayer byPlayer, BlockSelection blockSel)
        {
            if (Api.Side == EnumAppSide.Client)
            {
                ToggleGuiClient(byPlayer);
            }
            return true;
        }

        private void ToggleGuiClient(IPlayer byPlayer)
        {
            if (settingsDialog == null)
            {
                ICoreClientAPI capi = Api as ICoreClientAPI;
                settingsDialog = new HiveStorageSettingsGui(Pos, configInventory, capi);
                WireGUICallbacks();
                settingsDialog.SetupDialog();

                settingsDialog.OnClosed += () =>
                {
                    settingsDialog = null;
                    capi.Network.SendBlockEntityPacket(Pos, (int)EnumStorageBeaconPacketId.CloseGui, null);
                };

                settingsDialog.TryOpen();
                capi.Network.SendBlockEntityPacket(Pos, (int)EnumStorageBeaconPacketId.OpenGui, null);
            }
            else
            {
                settingsDialog.TryClose();
            }
        }

        private void WireGUICallbacks()
        {
            if (settingsDialog == null) return;

            // Storage tab callbacks
            settingsDialog.OnStorageSlotToggled = (slotId, enabled) =>
            {
                // TODO: Implement slot toggle logic
                // Send packet to server with slot state change
            };

            settingsDialog.OnStorageSlotFilterClicked = (slotId) =>
            {
                // Open item picker for filter selection
                var availableItems = settingsDialog.GetAvailableItems?.Invoke() ?? new List<ItemStack>();
                var picker = new GuiDialogItemPicker(Api as ICoreClientAPI, availableItems, (selectedItem) =>
                {
                    if (selectedItem != null && slotId >= 0 && slotId < configInventory.Count)
                    {
                        configInventory[slotId].Itemstack = selectedItem.Clone();
                        configInventory[slotId].MarkDirty();
                    }
                });
                picker.SetupDialog();
                picker.TryOpen();
            };

            // Request tab callbacks
            settingsDialog.OnRequestSubModeChanged = (isQuota) =>
            {
                // TODO: Handle quota/queue mode change
            };

            settingsDialog.OnAddRequestClicked = () =>
            {
                // Open item picker for adding request
                var availableItems = settingsDialog.GetAvailableItems?.Invoke() ?? new List<ItemStack>();
                var picker = new GuiDialogItemPicker(Api as ICoreClientAPI, availableItems, (selectedItem) =>
                {
                    // TODO: Add selected item to request list
                });
                picker.SetupDialog();
                picker.TryOpen();
            };

            // Push tab callbacks
            settingsDialog.OnPushModeToggled = (enabled) =>
            {
                // TODO: Handle push mode toggle
            };

            // Provide available items callback
            settingsDialog.GetAvailableItems = () =>
            {
                // Get list of all unique items in proxied inventory
                var proxiedInv = Inventory;
                var items = new List<ItemStack>();

                if (proxiedInv != null)
                {
                    foreach (var slot in proxiedInv)
                    {
                        if (!slot.Empty)
                        {
                            items.Add(slot.Itemstack);
                        }
                    }
                }

                return items;
            };
        }

        public override void OnBlockRemoved()
        {
            base.OnBlockRemoved();
            if (settingsDialog?.IsOpened() == true)
            {
                settingsDialog?.TryClose();
            }
            settingsDialog?.Dispose();
        }

        public override void OnBlockUnloaded()
        {
            base.OnBlockUnloaded();
            if (settingsDialog?.IsOpened() == true)
            {
                settingsDialog?.TryClose();
            }
            settingsDialog?.Dispose();
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
            ITreeAttribute configTree = new TreeAttribute();
            configInventory.ToTreeAttributes(configTree);
            tree["configInventory"] = configTree;
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
        {
            base.FromTreeAttributes(tree, worldForResolving);
            ITreeAttribute configTree = tree.GetTreeAttribute("configInventory");
            if (configTree != null)
            {
                configInventory.FromTreeAttributes(configTree);
            }
            configInventory.ResolveBlocksOrItems();
        }

        public override void OnReceivedClientPacket(IPlayer player, int packetid, byte[] data)
        {
            base.OnReceivedClientPacket(player, packetid, data);

            if (packetid == (int)EnumStorageBeaconPacketId.OpenGui)
            {
                // Player opened GUI
            }
            else if (packetid == (int)EnumStorageBeaconPacketId.CloseGui)
            {
                // Player closed GUI
            }
            // TODO: Handle other packet types (slot toggle, filter change, etc.)
        }

        public override void OnReceivedServerPacket(int packetid, byte[] data)
        {
            base.OnReceivedServerPacket(packetid, data);

            if (packetid == (int)EnumStorageBeaconPacketId.CloseGui)
            {
                if (settingsDialog?.IsOpened() == true)
                {
                    settingsDialog?.TryClose();
                }
                settingsDialog?.Dispose();
                settingsDialog = null;
            }
            // TODO: Handle other packet types for syncing state from server
        }
    }
}
