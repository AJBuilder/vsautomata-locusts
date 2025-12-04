using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;

namespace LocustLogistics.Logistics.Storage
{
    public class HiveStorageSettingsGui : GuiDialogBlockEntity
    {
        private int currentTab = 0;
        private bool requestIsQuotaMode = true;

        #region Callback Properties

        // Storage Tab Callbacks
        public Action<int, bool> OnStorageSlotToggled { get; set; }
        public Action<int> OnStorageSlotFilterClicked { get; set; }
        public Action<int, ItemStack> OnStorageSlotItemChanged { get; set; }

        // Request Tab Callbacks
        public Action<bool> OnRequestSubModeChanged { get; set; }
        public Func<List<ItemStack>> GetAvailableItems { get; set; }
        public Action OnAddRequestClicked { get; set; }
        public Action<int> OnRequestCleared { get; set; }
        public Action<int, bool> OnRequestToggled { get; set; }
        public Action<int, int> OnRequestQuantityChanged { get; set; }

        // Push Tab Callbacks
        public Action<bool> OnPushModeToggled { get; set; }

        #endregion

        private IInventory configInventory;

        public HiveStorageSettingsGui(BlockPos blockEntityPos, IInventory configInventory, ICoreClientAPI capi)
            : base(Lang.Get("locustlogistics:hive-storage-settings"), blockEntityPos, capi)
        {
            this.configInventory = configInventory;
        }

        public void SetupDialog()
        {
            Compose();
        }

        private void Compose()
        {
            // Dialog bounds
            ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
            bgBounds.BothSizing = ElementSizing.FitToChildren;

            ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog
                .WithAlignment(EnumDialogArea.CenterMiddle)
                .WithFixedAlignmentOffset(0, 0);

            // Tab bounds
            ElementBounds tabBounds = ElementBounds.Fixed(0, -24, 400, 25);

            var tabs = new GuiTab[]
            {
                new GuiTab() { Name = Lang.Get("locustlogistics:tab-storage"), DataInt = 0 },
                new GuiTab() { Name = Lang.Get("locustlogistics:tab-request"), DataInt = 1 },
                new GuiTab() { Name = Lang.Get("locustlogistics:tab-push"), DataInt = 2 }
            };

            CairoFont tabFont = CairoFont.WhiteDetailText();

            SingleComposer = capi.Gui
                .CreateCompo("hivestoragesettingsdialog-" + BlockEntityPosition, dialogBounds)
                .AddShadedDialogBG(bgBounds, true)
                .AddDialogTitleBar(DialogTitle, OnTitleBarClose)
                .AddHorizontalTabs(tabs, tabBounds, OnTabClicked, tabFont,
                    tabFont.Clone().WithColor(GuiStyle.ActiveButtonTextColor), "tabs")
                .BeginChildElements(bgBounds);

            // Compose the appropriate tab content
            switch (currentTab)
            {
                case 0:
                    ComposeStorageTab();
                    break;
                case 1:
                    ComposeRequestTab();
                    break;
                case 2:
                    ComposePushTab();
                    break;
            }

            SingleComposer.EndChildElements().Compose();

            // Set the active tab
            SingleComposer.GetHorizontalTabs("tabs").activeElement = currentTab;
        }

        private void OnTabClicked(int tabIndex)
        {
            if (currentTab != tabIndex)
            {
                currentTab = tabIndex;
                Compose();
            }
        }

        private void ComposeStorageTab()
        {
            // Calculate slot grid dimensions
            int cols = 4;
            int rows = (int)Math.Ceiling(configInventory.Count / (double)cols);

            // Slot grid bounds
            ElementBounds slotGridBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 0, 30, cols, rows);

            // Custom slot grid that intercepts clicks
            SingleComposer.AddInteractiveElement(
                new GuiElementStorageSlotGrid(capi, configInventory, SendInvPacket, cols, slotGridBounds, this),
                "storageslotgrid"
            );
        }

        private void ComposeRequestTab()
        {
            double y = 30;

            // Quota/Queue mode switch
            ElementBounds switchBounds = ElementBounds.Fixed(0, y, 35, 25);
            ElementBounds switchTextBounds = ElementBounds.Fixed(40, y + 3, 200, 25);

            SingleComposer
                .AddSwitch(OnRequestModeToggled, switchBounds, "requestmodeswitch", 25)
                .AddStaticText(Lang.Get("locustlogistics:request-mode-quota"), CairoFont.WhiteSmallText(), switchTextBounds);

            y += 40;

            // Add Request button
            ElementBounds addButtonBounds = ElementBounds.Fixed(0, y, 150, 30);
            SingleComposer.AddSmallButton(Lang.Get("locustlogistics:add-request"), OnAddRequestButtonClicked, addButtonBounds);

            y += 40;

            // Request list area (placeholder for now)
            ElementBounds requestListBounds = ElementBounds.Fixed(0, y, 400, 200);
            SingleComposer.AddStaticText(Lang.Get("locustlogistics:request-list-placeholder"),
                CairoFont.WhiteSmallText(), requestListBounds);

            // Set initial switch state
            SingleComposer.GetSwitch("requestmodeswitch").On = requestIsQuotaMode;
        }

        private void ComposePushTab()
        {
            double y = 30;

            // Push mode enable/disable switch
            ElementBounds switchBounds = ElementBounds.Fixed(0, y, 35, 25);
            ElementBounds switchTextBounds = ElementBounds.Fixed(40, y + 3, 200, 25);

            SingleComposer
                .AddSwitch(OnPushModeToggleInternal, switchBounds, "pushmodeswitch", 25)
                .AddStaticText(Lang.Get("locustlogistics:enable-push-mode"), CairoFont.WhiteSmallText(), switchTextBounds);
        }

        #region Internal Event Handlers

        private void OnRequestModeToggled(bool isQuota)
        {
            requestIsQuotaMode = isQuota;
            OnRequestSubModeChanged?.Invoke(isQuota);
        }

        private bool OnAddRequestButtonClicked()
        {
            OnAddRequestClicked?.Invoke();
            return true;
        }

        private void OnPushModeToggleInternal(bool enabled)
        {
            OnPushModeToggled?.Invoke(enabled);
        }

        #endregion

        #region Internal Methods Called by Custom Slot Grid

        internal void HandleStorageSlotLeftClick(int slotId)
        {
            OnStorageSlotToggled?.Invoke(slotId, true); // Will need state management
        }

        internal void HandleStorageSlotRightClick(int slotId)
        {
            OnStorageSlotFilterClicked?.Invoke(slotId);
        }

        #endregion

        private void OnTitleBarClose()
        {
            TryClose();
        }

        public override void OnGuiClosed()
        {
            SingleComposer?.GetSlotGrid("storageslotgrid")?.OnGuiClosed(capi);
            base.OnGuiClosed();
        }

        private void SendInvPacket(object packet)
        {
            capi.Network.SendPacketClient(packet);
        }
    }

    /// <summary>
    /// Custom slot grid that intercepts left and right clicks
    /// </summary>
    public class GuiElementStorageSlotGrid : GuiElementItemSlotGridBase
    {
        private HiveStorageSettingsGui parentGui;

        public GuiElementStorageSlotGrid(ICoreClientAPI capi, IInventory inventory, Action<object> sendPacket,
            int columns, ElementBounds bounds, HiveStorageSettingsGui parent)
            : base(capi, inventory, sendPacket, columns, bounds)
        {
            this.parentGui = parent;
        }

        public override void SlotClick(ICoreClientAPI api, int slotId, EnumMouseButton mouseButton, bool shiftPressed, bool ctrlPressed, bool altPressed)
        {
            // Intercept left and right clicks
            if (mouseButton == EnumMouseButton.Left)
            {
                parentGui.HandleStorageSlotLeftClick(slotId);
                // Don't call base - we're handling this ourselves
                return;
            }
            else if (mouseButton == EnumMouseButton.Right)
            {
                parentGui.HandleStorageSlotRightClick(slotId);
                // Don't call base - we're handling this ourselves
                return;
            }

            // For other mouse buttons, use default behavior
            base.SlotClick(api, slotId, mouseButton, shiftPressed, ctrlPressed, altPressed);
        }
    }
}
