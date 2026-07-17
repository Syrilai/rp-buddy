using System;
using System.Collections.Generic;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.BaseTypes;
using KamiToolKit.Classes;
using KamiToolKit.Nodes;
using Lumina.Text.ReadOnly;
using RpBuddy.Inventory;

namespace RpBuddy.Addons;

// TODO Rewrite this to not use slotContents maybe
public class RpInventoryAddon : NativeAddon
{
    private const int Rows = InventoryBase.Rows;
    private const int Columns = InventoryBase.Columns;
    private const float SlotSize = 44f;
    private const float Padding = 2f;
    
    private readonly List<DragDropNode> slots = [];
    private readonly Dictionary<DragDropNode, InventoryItem?> slotContents = [];
    private readonly InventoryBase inventory;

    public RpInventoryAddon(InventoryBase inventory)
    {
        this.inventory = inventory;
    }

    private void UpdateItemsFromInventory()
    {
        for (var i = 0; i < InventoryBase.Rows * InventoryBase.Columns; i++)
        {
            var (_, item) = inventory.GetItem(i);
                
            SetSlot(i, item);
        }
    }
    
    protected override unsafe void OnSetup(AtkUnitBase* addon, Span<AtkValue> atkValueSpan)
    {
        base.OnSetup(addon, atkValueSpan);

        const float totalWidth = Columns * SlotSize + (Columns - 1) * Padding;
        const float totalHeight = Rows * SlotSize + (Rows - 1) * Padding;
        
        SetWindowSize(new Vector2(totalWidth + 25f, totalHeight + 64f));

        for (var row = 0; row < Rows; row++)
        {
            for (var column = 0; column < Columns; column++)
            {
                var position = ContentStartPosition + new Vector2(
                    column * (SlotSize + Padding),
                    row * (SlotSize + Padding)
                );
                
                var index = row * Columns + column;

                var slot = new DragDropNode
                {
                    Size = new Vector2(SlotSize),
                    Position = position,
                    IsDraggable = true,
                    IsClickable = true
                };
                slot.AttachNode(this);
                slot.IconId = 0;
                slot.AcceptedType = DragDropType.LetterEditor_Item;

                slot.OnBegin += _ =>
                {
                    Plugin.Instance.ItemTooltipOverlay.Close();
                };

                slot.OnPayloadAccepted += (targetSlot, payload) =>
                {
                    var sourceIndex = payload.Int1;
                    if (sourceIndex == index) return;
                    if (sourceIndex < 0 || sourceIndex >= slots.Count) return;
                    if (GetSlot(sourceIndex) is null) return;

                    inventory.MoveItem(sourceIndex, index);
                };

                slot.OnRollOver += _ =>
                {
                    if (slotContents[slot] is not { } inventoryItem) return;

                    slot.ShowTooltip();
                    var t = slot.ScreenPosition;
                    Plugin.Instance.ItemTooltipOverlay.Position = new Vector2(t.X + SlotSize + 4f, t.Y + SlotSize + 4f);
                    Plugin.Instance.ItemTooltipOverlay.SetContents(inventoryItem);
                    Plugin.Instance.ItemTooltipOverlay.Open();
                };

                slot.OnRollOut += _ =>
                {
                    Plugin.Instance.ItemTooltipOverlay.Close();
                    slot.HideTooltip();
                };

                slot.OnDiscard += _ =>
                {
                    inventory.DiscardItem(index);
                };
                
                slot.AddEvent(AtkEventType.DragDropClick, (thisPtr, eventType, eventParam, atkEvent, atkEventData) =>
                {
                    if (atkEventData->MouseData.ButtonId != 1) return;
                    if (slotContents[slot] is not { } inventoryItem) return;
                    
                    Plugin.Instance.ItemTooltipOverlay.Close();
                });
                
                slots.Add(slot);
                slotContents[slot] = null;
            }
        }

        inventory.Updated += UpdateItemsFromInventory;
        UpdateItemsFromInventory();
    }

    protected override unsafe void OnFinalize(AtkUnitBase* addon)
    {
        base.OnFinalize(addon);
        slots.Clear();
        slotContents.Clear();
        inventory.Updated -= UpdateItemsFromInventory;
    }

    public void SetSlot(int index, InventoryItem? inventoryItem)
    {
        if (index < 0 || index >= slots.Count)
        {
            Plugin.Log.Warning("Tried setting slot {index}, which is out of range.\n{index1} < 0 || {index2} >= {totalSlots}", index, index, index, slots.Count);
            return;
        }

        var slot = slots[index];
        slotContents[slot] = inventoryItem;

        Plugin.Log.Info("Setting contents for {slot} ({index}) with {item}", slot, index, inventoryItem?.Item.Name ?? "None");

        if (inventoryItem is null)
        {
            Plugin.Log.Warning("Slot {slot} ({index}) will be set to empty.", slot, index);
            slot.Clear();
            slot.QuantityString = string.Empty;
            slot.TextTooltip = default;
            return;
        }

        slot.IconId = inventoryItem.Item.IconId;
        slot.QuantityString = inventoryItem.Quantity > 1
            ? inventoryItem.Quantity.ToString()
            : string.Empty;
        slot.TextTooltip = BuildTooltipText(inventoryItem);
        slot.Payload = new DragDropPayload
        {
            Type = DragDropType.LetterEditor_Item,
            Int1 = index
        };
    }

    public InventoryItem? GetSlot(int index) => index >= 0 && index < slots.Count
        ? slotContents[slots[index]]
        : null;
    
    private static ReadOnlySeString BuildTooltipText(InventoryItem inventoryItem) {
        var name = inventoryItem.Item.Name;

        return name;
    }
}
