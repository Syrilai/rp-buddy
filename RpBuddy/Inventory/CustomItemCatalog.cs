using System;
using System.Collections.Generic;
using System.Linq;

namespace RpBuddy.Inventory;

[Serializable]
public sealed class CustomItemCatalog
{
    public Dictionary<Guid, CustomItem> Items { get; set; } = [];

    public CustomItem Register(CustomItem item)
    {
        Items[item.Id] = item;
        
        Plugin.Instance.Configuration.Save();
        
        return item;
    }
    
    public CustomItem? Get(Guid id) => Items.GetValueOrDefault(id);
    public List<CustomItem> GetAll() => Items.Select(item => item.Value).ToList();
    public bool Remove(Guid id) => Items.Remove(id);
    public IReadOnlyCollection<CustomItem> All => Items.Values;
}
