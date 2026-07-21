using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Enums;
using KamiToolKit.Nodes;
using KamiToolKit.Nodes.Simplified;
using KamiToolKit.UiOverlay;
using RpBuddy.Addons.Nodes.ItemTooltipOverlay;

namespace RpBuddy.Addons.Overlays;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable InconsistentNaming

public sealed partial class ItemTooltipOverlay : OverlayNode
{
    private readonly ResNode Container;
    private readonly WindowBackgroundTextureNode BackgroundTextureNode;

    // Header Group
    public readonly ResNode HeaderGroup;
    public readonly TextNode ItemNameText;
    public readonly IconNode ItemIcon;
    public readonly TextNode QuantityText;
    public readonly TextNode CategoryText;
    public readonly ResNode ItemFlagGroup;
    public readonly TextNode UntradableText;
    public readonly TextNode BindingText;
    public readonly TextNode UniqueText;

    // Description Group
    public readonly ResNode DescriptionGroup;
    public readonly SimpleNineGridNode DescriptionGroupDivider;
    public readonly TextNode DescriptionText;

    // Action Group
    public readonly ResNode ActionGroup;
    public readonly SimpleNineGridNode ActionGroupDivider;
    public readonly TextNode ActionText;

    public ItemTooltipOverlay()
    {
        Container = new ResNode
        {
            IsVisible = false,
        };
        Container.AttachNode(this);

        BackgroundTextureNode = new WindowBackgroundTextureNode(false, "ui/uld/WindowF_Bg")
        {
            NodeId = 11,
            Offsets = new Vector4(64.0f, 32.0f, 32.0f, 32.0f),
            NodeFlags = NodeFlags.AnchorTop | NodeFlags.AnchorLeft |
                        NodeFlags.Visible | NodeFlags.Enabled | NodeFlags.Fill | NodeFlags.EmitsEvents,
            PartsRenderType = 19
        };
        BackgroundTextureNode.AttachNode(Container);

        HeaderGroup = new ResNode
        {
            Size = new Vector2(TooltipLayout.Width, TooltipLayout.HeaderHeight)
        };
        HeaderGroup.AttachNode(Container);

        ItemNameText = new TextNode
        {
            IsVisible = true,
            Position = new Vector2(TooltipLayout.NameLeft, TooltipLayout.NameTop),
            Size = new Vector2(TooltipLayout.NameWidth, TooltipLayout.NameHeight),

            TextColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
            FontSize = 14,
            TextFlags = TextFlags.WordWrap | TextFlags.MultiLine
        };
        ItemNameText.AttachNode(HeaderGroup);

        ItemIcon = new IconNode
        {
            IconId = 0,
            Position = new Vector2(TooltipLayout.IconLeft, TooltipLayout.IconTop),
            Size = new Vector2(TooltipLayout.IconSize, TooltipLayout.IconSize + 4)
        };
        ItemIcon.AttachNode(HeaderGroup);

        QuantityText = new TextNode
        {
            Position = new Vector2(TooltipLayout.QuantityLeft, TooltipLayout.QuantityTop),
            Size = new Vector2(TooltipLayout.QuantityWidth, 21),
            AlignmentType = AlignmentType.Right,
            TextColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
            FontSize = 12,
            TextFlags = TextFlags.Emboss,
            String = ""
        };
        QuantityText.AttachNode(HeaderGroup);

        CategoryText = new TextNode
        {
            Position = new Vector2(TooltipLayout.CategoryLeft, TooltipLayout.CategoryTop),
            Size = new Vector2(TooltipLayout.CategoryWidth, 21),
            AlignmentType = AlignmentType.Left,
            TextColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
            FontSize = 12,
            TextFlags = TextFlags.Emboss | TextFlags.MultiLine | TextFlags.Ellipsis,
            String = ""
        };
        CategoryText.AttachNode(HeaderGroup);

        ItemFlagGroup = new ResNode
        {
            Position = new Vector2(TooltipLayout.FlagGroupLeft, TooltipLayout.FlagGroupTop),
            Size = new Vector2(TooltipLayout.FlagGroupWidth, 14)
        };
        ItemFlagGroup.AttachNode(HeaderGroup);

        UntradableText = CreateFlagText("Untradable", new Vector2(TooltipLayout.FlagTextLeft, 0), TooltipLayout.FlagTextWidth);
        UntradableText.AttachNode(ItemFlagGroup);

        BindingText = CreateFlagText("Binding", new Vector2(TooltipLayout.FlagTextLeft, 0), TooltipLayout.FlagTextWidth);
        BindingText.AttachNode(ItemFlagGroup);

        UniqueText = CreateFlagText("Unique", Vector2.Zero, TooltipLayout.UniqueTextWidth);
        UniqueText.AttachNode(ItemFlagGroup);

        DescriptionGroup = new ResNode
        {
            Position = new Vector2(0, TooltipLayout.HeaderHeight + TooltipLayout.SectionSpacing),
            Size = new Vector2(TooltipLayout.GroupWidth, 165)
        };
        DescriptionGroup.AttachNode(Container);

        DescriptionGroupDivider = new TooltipDividerNode();
        DescriptionGroupDivider.AttachNode(DescriptionGroup);

        DescriptionText = CreateSectionText();
        DescriptionText.AttachNode(DescriptionGroup);

        ActionGroup = new ResNode
        {
            Position = new Vector2(0, TooltipLayout.HeaderHeight + TooltipLayout.SectionSpacing),
            Size = new Vector2(TooltipLayout.GroupWidth, 0)
        };
        ActionGroup.AttachNode(Container);

        ActionGroupDivider = new TooltipDividerNode();
        ActionGroupDivider.AttachNode(ActionGroup);

        ActionText = CreateSectionText();
        ActionText.AttachNode(ActionGroup);
    }

    private static TextNode CreateSectionText()
    {
        return new TextNode
        {
            IsVisible = true,
            Position = new Vector2(TooltipLayout.SectionTextLeft, TooltipLayout.SectionTextTop),
            Size = new Vector2(TooltipLayout.SectionTextWidth, 40),

            TextColor = new Vector4(1, 1, 1, 1),
            AlignmentType = AlignmentType.TopLeft,
            FontSize = 12,
            TextFlags = TextFlags.Emboss | TextFlags.WordWrap | TextFlags.MultiLine
        };
    }

    private static TextNode CreateFlagText(string text, Vector2 position, float width)
    {
        return new TextNode
        {
            Position = position,
            Size = new Vector2(width, 14),
            String = text,
            FontType = FontType.MiedingerMed,
            FontSize = 12,
            TextColor = new Vector4(204 / 255f, 204 / 255f, 204 / 255f, 1),
            TextFlags = TextFlags.Emboss,
            IsVisible = false
        };
    }
}
