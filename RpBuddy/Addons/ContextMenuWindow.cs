using System;
using System.Collections.Generic;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.BaseTypes;
using KamiToolKit.Nodes;
using RpBuddy.Addons.Nodes;

namespace RpBuddy.Addons;

public unsafe class ContextMenuWindow : NativeAddon
{
    public ContextMenuWindow()
    {
        CreateWindowNode = () => new ContextMenuWindowNode();
    }

    private List<(string Text, Action action)> actions = [];

    protected ContextMenuWindowNode INeedANameForThis => (ContextMenuWindowNode)WindowNode!;
    
    protected override void OnSetup(AtkUnitBase* addon, Span<AtkValue> atkValueSpan)
    {
        base.OnSetup(addon, atkValueSpan);

        SetWindowSize(new Vector2(100, 100));
    }

    protected override void OnFinalize(AtkUnitBase* addon)
    {
        base.OnFinalize(addon);
    }

    public void ShowMenu(List<(string Text, Action action)> newActions)
    {
        actions = newActions;
        Open();
    }
}