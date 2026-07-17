using System;

namespace RpBuddy.Inventory;

public sealed class InventoryItem
{
    public required CustomItem Item { get; init; }
    private int _quantity = 0;

    public int Quantity
    {
        get => _quantity;
        set => _quantity = Math.Clamp(value, 0, Item.MaxStackSize);
    }

    public bool IsFull => Quantity >= Item.MaxStackSize;

    public int Add(int amount)
    {
        var spaceLeft = Item.MaxStackSize - Quantity;
        var added = Math.Min(spaceLeft, amount);
        Quantity += added;

        return amount - added;
    }
}