using System;
using System.Collections.Generic;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.BaseTypes;
using KamiToolKit.Enums;
using RpBuddy.Addons.Nodes;

namespace RpBuddy.Addons;

public unsafe class ContextMenuAddon : NativeAddon
{
    public ContextMenuAddon()
    {
        CreateWindowNode = () => new ContextMenuWindowNode();
    }

    private List<(string Text, Action Action)> _actions = [];
    private Vector2 _positionToUse = Vector2.Zero;

    protected ContextMenuWindowNode INeedANameForThis => (ContextMenuWindowNode)WindowNode!;
    
    protected override void OnSetup(AtkUnitBase* addon, Span<AtkValue> atkValueSpan)
    {
        base.OnSetup(addon, atkValueSpan);

        SetWindowSize(new Vector2(100, 100));
        SetWindowPosition(_positionToUse);
    }

    public void ShowMenu(List<(string Text, Action Action)> newActions, Vector2 position)
    {
        _actions = newActions;
        _positionToUse = position;
        Open();
        SetWindowPosition(position);
    }
}