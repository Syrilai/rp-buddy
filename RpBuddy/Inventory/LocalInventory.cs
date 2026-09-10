using System.Linq;
using Dalamud.Game.Text;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Lumina.Text;
using RpBuddy.Addons;
using Syrilib.Extensions.Dalamud;
using SeString = Dalamud.Game.Text.SeStringHandling.SeString;

namespace RpBuddy.Inventory;

public class LocalInventory : InventoryBase
{
    public void LoadFromConfig()
    {
        var saved = Shared.Configuration.LocalInventoryItems;
        for (var i = 0; i < Items.Length && i < saved.Length; i++)
            Items[i] = saved[i];

        foreach (var inventoryItem in Items.Where(i => i is not null))
            inventoryItem?.NormalizeQuantity();
        
        NotifyUpdated();
    }
    
    private void Persist()
    {
        Shared.Configuration.LocalInventoryItems = Items.ToArray();
        Shared.Configuration.Save();
    }
    
    public override (NetworkStatus, InventoryItem?) GetItem(int slot)
    {
        return !IsSlotInRange(slot) ? (NetworkStatus.Success, null) : (NetworkStatus.Success, Items[slot]);
    }

    public override (NetworkStatus, bool, int?) AddItem(InventoryItem item)
    {
        var slot = -1;
        for (var i = 0; i < Rows * Columns; i++)
        {
            var isFree = GetItem(i).Item2 is null;
            if (!isFree) continue;
            slot = i;
            break;
        }

        if (slot == -1)
            return (NetworkStatus.Success, false, null);

        var (_, setItemStatus) = SetItem(slot, item);

        return (NetworkStatus.Success, setItemStatus, slot);
    }

    public override (NetworkStatus, bool) SetItem(int slot, InventoryItem item)
    {
        if (!IsSlotInRange(slot))
            return (NetworkStatus.Success, false);

        Items[slot] = item;
        NotifyUpdated();
        Persist();

        return (NetworkStatus.Success, true);
    }

    public override (NetworkStatus, bool) MoveItem(int currentSlot, int newSlot)
    {
        if (!IsSlotInRange(currentSlot) || !IsSlotInRange(newSlot))
            return (NetworkStatus.Success, false);

        (Items[currentSlot], Items[newSlot]) = (Items[newSlot], Items[currentSlot]);
        NotifyUpdated();
        Persist();

        return (NetworkStatus.Success, true);
    }

    public override (NetworkStatus, bool) DiscardItem(int slot)
    {
        if (!IsSlotInRange(slot) || Items[slot] is null)
            return (NetworkStatus.Success, false);
        
        var parsedSlot = Items[slot]!;
        var quantity = parsedSlot.Quantity;
        var item = parsedSlot.Item;

        var promptText = new SeStringBuilder()
                         .PushColorType(508)
                         .PushEdgeColorType(509)
                         .Append($"Discard {(quantity > 1 ? quantity.ToString() : "the")} ")
                         .PushColorType(549)
                         .PushEdgeColorType(550)
                         .Append(item.Name)
                         .PopEdgeColorType()
                         .PopColorType()
                         .Append("?")
                         .PopEdgeColorType()
                         .PopColorType()
                         .ToReadOnlySeString();

        Shared.Addons.YesNo.QueueSelect(
            new YesNoAddonConfig
            {
                PromptText = promptText,
                OnConfirm = () => InternalDiscardItem(slot),
            }
        );
        
        return (NetworkStatus.Pending, true);
    }

    private (NetworkStatus, bool) InternalDiscardItem(int slot, bool showChatMessage = true)
    {
        if (!IsSlotInRange(slot) || Items[slot] is null)
            return (NetworkStatus.Success, false);
        
        var parsedSlot = Items[slot]!;
        var quantity = parsedSlot.Quantity;
        var item = parsedSlot.Item;
        
        Items[slot] = null;
        NotifyUpdated();
        Persist();

        if (!showChatMessage) return (NetworkStatus.Success, true);
        var chatText = new SeStringBuilder()
                       .Append("You throw away ")
                       .Append(quantity > 1 ? quantity.ToString() : "a")
                       .Append(" ")
                       .PushColorType(500)
                       .PushEdgeColorType(501)
                       .Append(SeIconChar.LinkMarker.ToIconChar())
                       .PopEdgeColorType()
                       .PopColorType()
                       .PushColorType(549)
                       .PushEdgeColorType(550)
                       .Append(item.Name)
                       .PopEdgeColorType()
                       .PopColorType()
                       .Append(".")
                       .ToReadOnlySeString();
        
        IChatGui.Get().Print(new XivChatEntry
        {
            Type = XivChatType.SystemMessage,
            MessageBytes = chatText.Data.ToArray(),
        });

        return (NetworkStatus.Success, true);
    }

    public override (NetworkStatus, bool) UseItem(int slot)
    {
        // TODO Add logic here, for now just discard
        var (_, item) = GetItem(slot);
        if (item is null)
            return (NetworkStatus.Success, false);
        
        if (!item.Item.CanBeUsed)
            return (NetworkStatus.Success, false);
        
        foreach (var action in item.Item.UseActions)
            action.Execute();
        
        return InternalDiscardItem(slot, false);
    }
}