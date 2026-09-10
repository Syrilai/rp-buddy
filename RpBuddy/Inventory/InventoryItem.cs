using System;
using Newtonsoft.Json;

namespace RpBuddy.Inventory;

[Serializable]
public sealed class InventoryItem
{
    public required Guid ItemId { get; init; }
    private int _quantity = 0;

    [JsonIgnore]
    public CustomItem Item
    {
        get
        {
            if (Shared.ItemCatalog is null)
            {
                throw new InvalidOperationException("Item catalog has not been initialized.");
            }

            return Shared.ItemCatalog.Get(ItemId)
                   ?? throw new InvalidOperationException($"Item with ID {ItemId} not found in catalog.");
        }
    }
    
    public int Quantity
    {
        get => _quantity;
        set => _quantity = Math.Max(value, 0);
    }

    [JsonIgnore]
    public bool IsFull => Quantity >= Item.MaxStackSize;

    public void NormalizeQuantity()
    {
        _quantity = Math.Clamp(_quantity, 0, Item.MaxStackSize);
    }

    public int Add(int amount)
    {
        if (amount <= 0)
        {
            return 0;
        }

        NormalizeQuantity();

        var spaceLeft = Item.MaxStackSize - Quantity;
        var added = Math.Min(spaceLeft, amount);

        _quantity += added;

        return amount - added;
    }
}