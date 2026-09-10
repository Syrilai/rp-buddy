using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FFXIVClientStructs.FFXIV.Client.Game.WKS;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;
using RpBuddy.Inventory.Actions;

namespace RpBuddy.Inventory;

public sealed class CustomItemCatalog
{
    private Dictionary<Guid, CustomItem> _localItems = [];
    private Dictionary<Guid, CustomItem> _remoteItems = [];

    public CustomItem Register(CustomItem item)
    {
        _localItems[item.Id] = item;
        Persist();
        return item;
    }

    public void SetRemoteItems(IEnumerable<CustomItem> items)
    {
        // TODO: Don't clear it, as we want to "stream" the items in the user requests
        _remoteItems.Clear();
        foreach (var item in items)
            _remoteItems[item.Id] = item;
    }

    public bool TryGet(Guid id, [NotNullWhen(true)] out CustomItem? item)
    {
        return _localItems.TryGetValue(id, out item) || _remoteItems.TryGetValue(id, out item);
    }

    public CustomItem? Get(Guid id) => TryGet(id, out var item) ? item : null;

    public List<CustomItem> GetAll() => All.ToList();

    public bool Remove(Guid id)
    {
        var removed = _localItems.Remove(id);
        if (removed) Persist();
        return removed;
    }
    
    public CustomItem Update(Guid id, Action<CustomItemUpdate> configure)
    {
        if (!_localItems.TryGetValue(id, out var existing))
            throw new InvalidOperationException($"Item {id} not found");

        var update = new CustomItemUpdate(existing);
        configure(update);

        var updated = update.Apply();
        _localItems[id] = updated;
        Persist();

        return updated;
    }
    
    public IReadOnlyCollection<CustomItem> All =>
        _remoteItems.Values
                       .Concat(_localItems.Values)
                       .GroupBy(i => i.Id)
                       .Select(g => g.Last())
                       .ToList();

    private void Persist()
    {
        Shared.Configuration.LocalItems = _localItems;
        Shared.Configuration.Save();
    }

    public void LoadFromConfig()
    {
        _localItems.Clear();
        foreach (var (id, item) in Shared.Configuration.LocalItems)
            _localItems[id] = item;
    }
}

// A scratch object the edit UI fills in; only the catalog knows how to turn it into a real CustomItem.
public sealed class CustomItemUpdate
{
    private readonly CustomItem _original;

    public string? Name { get; set; }
    public uint? IconId { get; set; }
    public ReadOnlySeString? Description { get; set; }
    public int? MaxStackSize { get; set; }
    public bool? CanBeUsed { get; set; }
    public RowRef<ItemUICategory>? Category { get; set; }
    public List<IItemActionBase>? UseActions { get; set; }

    internal CustomItemUpdate(CustomItem original) => _original = original;

    internal CustomItem Apply() => _original with
    {
        Name = Name ?? _original.Name,
        IconId = IconId ?? _original.IconId,
        MacroDescription = Description?.ToMacroString() ?? _original.MacroDescription,
        MaxStackSize = MaxStackSize ?? _original.MaxStackSize,
        CanBeUsed = CanBeUsed ?? _original.CanBeUsed,
        CategoryId = Category?.RowId ?? _original.CategoryId,
        UseActions = UseActions ?? _original.UseActions
    };
}