using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FFXIVClientStructs.FFXIV.Client.Game.WKS;

namespace RpBuddy.Inventory;

[Serializable]
public sealed class CustomItemCatalog
{
    public Dictionary<Guid, CustomItem> Items { get; set; } = [];

    public CustomItem Register(CustomItem item)
    {
        Items[item.Id] = item;
        
        Shared.Configuration.Save();
        
        return item;
    }
    
    public bool TryGet(Guid id, [NotNullWhen(true)] out CustomItem? item)
    {
        return Items.TryGetValue(id, out item!);
    }
    public CustomItem? Get(Guid id) => Items.GetValueOrDefault(id);
    public List<CustomItem> GetAll() => Items.Select(item => item.Value).ToList();
    public bool Remove(Guid id) => Items.Remove(id);
    public IReadOnlyCollection<CustomItem> All => Items.Values;
}
