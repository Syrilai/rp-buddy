using System;
using System.Collections.Generic;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.Havok.Animation.Rig;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;
using RpBuddy.Inventory.Actions;
using Syrilib.Extensions.Dalamud;
using Syrilib.Extensions.Lumina;

namespace RpBuddy.Inventory;

[Serializable]
public sealed record CustomItem
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required uint IconId { get; init; }
    public ReadOnlySeString Description { get; init; } = string.Empty;
    public int MaxStackSize { get; init; } = 1;

    private uint _categoryId = 0;
    public bool CanBeUsed { get; init; } = false;
    public List<ItemActionBase> UseActions { get; init; } = [];

    public RowRef<ItemUICategory> Category
    {
        get => ItemUICategory.GetRowRef(_categoryId);
        set => _categoryId = value.RowId;
    }
}