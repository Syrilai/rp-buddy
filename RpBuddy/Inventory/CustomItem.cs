using System;
using System.Collections.Generic;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;
using Newtonsoft.Json;
using RpBuddy.Inventory.Actions;
using RpBuddy.Utils;
using Syrilib.Extensions.Lumina;

namespace RpBuddy.Inventory;

[Serializable]
public sealed record CustomItem
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required uint IconId { get; init; }
    public string MacroDescription { get; init; } = string.Empty;
    public int MaxStackSize { get; init; } = 1;
    public bool CanBeUsed { get; init; } = false;
    [JsonProperty(ItemConverterType = typeof(ItemActionConverter))]
    public List<IItemActionBase> UseActions { get; init; } = [];
    public uint CategoryId { get; init; } = 0;

    [JsonIgnore]
    public RowRef<ItemUICategory> Category => ItemUICategory.GetRowRef(CategoryId);

    [JsonIgnore]
    public ReadOnlySeString Description => ReadOnlySeString.FromMacroString(MacroDescription);
}