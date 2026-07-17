using System;
using System.Collections.Generic;
using FFXIVClientStructs.Havok.Animation.Rig;
using Lumina.Text.ReadOnly;

namespace RpBuddy.Inventory;

[Serializable]
public sealed record CustomItem
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required uint IconId { get; init; }
    public ReadOnlySeString Description { get; init; } = string.Empty;
    public int MaxStackSize { get; init; } = 1;

    public string DO_NOT_USE__Category = "Other";
    public bool DO_NOT_USE__CanBeUsed { get; init; } = false;
    public List<string> DO_NOT_USE__UseActions { get; init; } = [];
}