using System.Numerics;
using KamiToolKit.Nodes.Simplified;
using RpBuddy.Addons.Overlays;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace RpBuddy.Addons.Nodes.ItemTooltipOverlay;

public sealed class TooltipDividerNode : SimpleNineGridNode
{
    public TooltipDividerNode()
    {
        NodeId = 41;
        Position = new Vector2(TooltipLayout.DividerLeft, TooltipLayout.DividerTop);
        Size = new Vector2(TooltipLayout.DividerWidth, TooltipLayout.DividerHeight);
        TexturePath = "ui/uld/WindowA_Line.tex";
        TextureCoordinates = Vector2.Zero;
        TextureSize = new Vector2(32.0f, 4.0f);
        LeftOffset = 12.0f;
        RightOffset = 12.0f;
        NodeFlags = NodeFlags.AnchorTop | NodeFlags.AnchorLeft | NodeFlags.AnchorRight |
                    NodeFlags.Visible | NodeFlags.Enabled | NodeFlags.EmitsEvents;
    }
}
