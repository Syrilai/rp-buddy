using System;

namespace RpBuddy.Inventory;

public abstract class InventoryBase
{
    public const int Rows = 5;
    public const int Columns = 5;

    protected InventoryItem?[] Items { get; } = new InventoryItem?[Rows * Columns];

    public event Action? Updated;

    internal bool IsSlotInRange(int slot)
    {
        return slot is >= 0 and < Rows * Columns;
    }

    internal void NotifyUpdated()
    {
        Updated?.Invoke();
    }

    public abstract (NetworkStatus, InventoryItem?) GetItem(int slot);
    public abstract (NetworkStatus, bool, int?) AddItem(InventoryItem item);
    public abstract (NetworkStatus, bool) SetItem(int slot, InventoryItem item);
    public abstract (NetworkStatus, bool) MoveItem(int currentSlot, int newSlot);
    public abstract (NetworkStatus, bool) DiscardItem(int slot);
    public abstract (NetworkStatus, bool) UseItem(int slot);
}

public enum NetworkStatus
{
    Success,
    Failure
}