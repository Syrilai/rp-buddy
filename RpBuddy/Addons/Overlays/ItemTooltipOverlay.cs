using System.Numerics;
using Dalamud.Bindings.ImGui;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Enums;
using KamiToolKit.Nodes;
using KamiToolKit.Nodes.Simplified;
using KamiToolKit.UiOverlay;
using RpBuddy.Inventory;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable InconsistentNaming

namespace RpBuddy.Addons.Overlays;

public sealed class ItemTooltipOverlay : OverlayNode
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

    private const float Width = 376.0f;

    private const float HeaderHeight = 78.0f;

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

        HeaderGroup = new ResNode();
        HeaderGroup.AttachNode(Container);

        ItemNameText = new TextNode
        {
            IsVisible = true,
            Position = new Vector2(66.0f, 16.0f),
            Size = new Vector2(Width - 66.0f, 42.0f),
            
            TextColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
            FontSize = 14,
            TextFlags = TextFlags.WordWrap | TextFlags.MultiLine
        };
        ItemNameText.AttachNode(HeaderGroup);

        ItemIcon = new IconNode
        {
            IconId = 0,
            Position = new Vector2(15, 14),
            Size = new Vector2(44, 48)
        };
        ItemIcon.AttachNode(HeaderGroup);

        QuantityText = new TextNode
        {
            Position = new Vector2(234, 58),
            Size = new Vector2(130, 21),
            AlignmentType = AlignmentType.Right,
            TextColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
            FontSize = 12,
            TextFlags = TextFlags.Emboss,
            String = ""
        };
        QuantityText.AttachNode(HeaderGroup);
        
        CategoryText = new TextNode
        {
            Position = new Vector2(16, 58),
            Size = new Vector2(218, 21),
            AlignmentType = AlignmentType.Left,
            TextColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
            FontSize = 12,
            TextFlags = TextFlags.Emboss | TextFlags.MultiLine | TextFlags.Ellipsis,
            String = ""
        };
        CategoryText.AttachNode(HeaderGroup);

        ItemFlagGroup = new ResNode
        {
            Position = new Vector2(73, 4),
            Size = new Vector2(200, 14)
        };
        ItemFlagGroup.AttachNode(HeaderGroup);

        UntradableText = new TextNode
        {
            Position = new Vector2(86, 0),
            Size = new Vector2(114, 14),
            String = "Untradable",
            FontType = FontType.MiedingerMed,
            FontSize = 12,
            TextColor = new Vector4(204 / 255f, 204 / 255f, 204 / 255f, 1),
            TextFlags = TextFlags.Emboss,
            IsVisible = false
        };
        UntradableText.AttachNode(ItemFlagGroup);
        
        BindingText = new TextNode
        {
            Position = new Vector2(86, 0),
            Size = new Vector2(114, 14),
            String = "Binding",
            FontType = FontType.MiedingerMed,
            FontSize = 12,
            TextColor = new Vector4(204 / 255f, 204 / 255f, 204 / 255f, 1),
            TextFlags = TextFlags.Emboss,
            IsVisible = false
        };
        BindingText.AttachNode(ItemFlagGroup);
        
        UniqueText = new TextNode
        {
            Position = new Vector2(0, 0),
            Size = new Vector2(71, 14),
            String = "Unique",
            FontType = FontType.MiedingerMed,
            FontSize = 12,
            TextColor = new Vector4(204 / 255f, 204 / 255f, 204 / 255f, 1),
            TextFlags = TextFlags.Emboss,
            IsVisible = false
        };
        UniqueText.AttachNode(ItemFlagGroup);

        DescriptionGroup = new ResNode
        {
            Position = new Vector2(0, 79),
            Size = new Vector2(374, 165)
        };
        DescriptionGroup.AttachNode(Container);

        DescriptionGroupDivider = new SimpleNineGridNode
        {
            NodeId = 41,
            Position = new Vector2(15, 4),
            Size = new Vector2(346, 4),
            TexturePath = "ui/uld/WindowA_Line.tex",
            TextureCoordinates = Vector2.Zero,
            TextureSize = new Vector2(32.0f, 4.0f),
            LeftOffset = 12.0f,
            RightOffset = 12.0f,
            NodeFlags = NodeFlags.AnchorTop | NodeFlags.AnchorLeft | NodeFlags.AnchorRight |
                        NodeFlags.Visible | NodeFlags.Enabled | NodeFlags.EmitsEvents
        };
        DescriptionGroupDivider.AttachNode(DescriptionGroup);

        DescriptionText = new TextNode
        {
            IsVisible = true,
            Position = new Vector2(17, 8),
            Size = new Vector2(342, 40),

            TextColor = new Vector4(1, 1, 1, 1),
            AlignmentType = AlignmentType.TopLeft,
            FontSize = 12,
            TextFlags = TextFlags.Emboss | TextFlags.WordWrap | TextFlags.MultiLine
        };
        DescriptionText.AttachNode(DescriptionGroup);
    }

    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();
        Container.Size = Size;
        BackgroundTextureNode.Size = Size;
    }

    protected override void OnUpdate()
    {
        
    }

    public override OverlayLayer OverlayLayer => OverlayLayer.AboveUserInterface;

    public void Open()
    {
        Container.IsVisible = true;
    }

    public void Close()
    {
        Container.IsVisible = false;
    }

    public void SetContents(InventoryItem inventoryItem)
    {
        ItemNameText.String = inventoryItem.Item.Name;
        ItemIcon.IconId = inventoryItem.Item.IconId;
        QuantityText.String = $"{inventoryItem.Quantity}/{inventoryItem.Item.MaxStackSize}";
        CategoryText.String = "Other";
        DescriptionText.String = inventoryItem.Item.Description;

        var descriptionTextSize = DescriptionText.GetTextDrawSize();

        Size = new Vector2(Width, HeaderHeight + DescriptionText.Position.Y + descriptionTextSize.Y + 12.0f);
    }
}