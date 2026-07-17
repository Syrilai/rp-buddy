namespace RpBuddy.Inventory;

public class LocalInventory : InventoryBase
{
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

        return (NetworkStatus.Success, true);
    }

    public override (NetworkStatus, bool) MoveItem(int currentSlot, int newSlot)
    {
        if (!IsSlotInRange(currentSlot) || !IsSlotInRange(newSlot))
            return (NetworkStatus.Success, false);

        (Items[currentSlot], Items[newSlot]) = (Items[newSlot], Items[currentSlot]);
        NotifyUpdated();

        return (NetworkStatus.Success, true);
    }

    public override (NetworkStatus, bool) DiscardItem(int slot)
    {
        if (!IsSlotInRange(slot) || Items[slot] is null)
            return (NetworkStatus.Success, false);

        Items[slot] = null;
        NotifyUpdated();
        
        return (NetworkStatus.Success, true);
    }

    public override (NetworkStatus, bool) UseItem(int slot)
    {
        throw new System.NotImplementedException();
    }
}