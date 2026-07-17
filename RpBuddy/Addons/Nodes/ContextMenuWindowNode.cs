using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Classes;
using KamiToolKit.Enums;
using KamiToolKit.Nodes;
using KamiToolKit.Nodes.Simplified;
using KamiToolKit.Timelines;
using Lumina.Text.ReadOnly;

namespace RpBuddy.Addons.Nodes;

public unsafe class ContextMenuWindowNode : WindowNodeBase {

    /// <summary>
    /// Not intended for public use, but it's here if you absolutely need it.
    /// </summary>
    public ImageNode BackgroundImageNode { get; }

    /// <summary>
    /// Not intended for public use, but it's here if you absolutely need it.
    /// </summary>
    public NineGridNode BackgroundTextureNode { get; }

    /// <summary>
    /// Not intended for public use, but it's here if you absolutely need it.
    /// </summary>
    public CollisionNode HeaderCollisionNode { get; }

    /// <summary>
    /// Not intended for public use, but it's here if you absolutely need it.
    /// </summary>
    public ResNode HeaderContainerNode { get; }

    /// <summary>
    /// Gets or sets the reference to the owning addon.
    /// </summary>
    public AtkUnitBase* OwnerAddon {
        get => Component->OwnerUnitBase;
        set => Component->OwnerUnitBase = value;
    }

    /// <inheritdoc/>
    public override float HeaderHeight
        => HeaderContainerNode.Height;

    /// <inheritdoc/>
    public override Vector2 ContentSize
        => new(BackgroundImageNode.Width, BackgroundImageNode.Height - HeaderHeight);

    /// <inheritdoc/>
    public override Vector2 ContentStartPosition
        => new(BackgroundImageNode.X, BackgroundImageNode.Y + HeaderHeight);

    /// <inheritdoc/>
    public override ResNode WindowHeaderFocusNode
        => HeaderContainerNode;

    /// <summary>
    /// Constructs a new <see cref="WindowNode"/>
    /// </summary>
    public ContextMenuWindowNode() {
        NodeFlags = NodeFlags.AnchorTop | NodeFlags.AnchorLeft | NodeFlags.Visible | NodeFlags.Enabled | NodeFlags.EmitsEvents;

        CollisionNode.NodeId = 13;
        CollisionNode.NodeFlags = NodeFlags.AnchorTop | NodeFlags.AnchorLeft | NodeFlags.Visible | NodeFlags.Enabled | NodeFlags.Fill | NodeFlags.HasCollision | NodeFlags.EmitsEvents;

        Component->ShowFlags = 1;

        HeaderCollisionNode = new CollisionNode {
            Uses = 2,
            NodeId = 12,
            Size = new Vector2(0.0f, 0.0f),
            Position = new Vector2(8.0f, 8.0f),
            NodeFlags = NodeFlags.AnchorTop | NodeFlags.AnchorLeft | NodeFlags.AnchorRight |
                        NodeFlags.Visible | NodeFlags.Enabled | NodeFlags.HasCollision | NodeFlags.RespondToMouse | NodeFlags.EmitsEvents,
        };
        HeaderCollisionNode.AttachNode(this);
        
        BackgroundTextureNode = new NineGridNode
        {
            NodeId = 2,
            NodeFlags = NodeFlags.AnchorTop | NodeFlags.AnchorLeft |
                        NodeFlags.Visible | NodeFlags.Enabled | NodeFlags.Fill | NodeFlags.EmitsEvents,
            
            Parts = [
                new Part { TextureCoordinates = new Vector2(0.0f, 0.0f), Size = new Vector2(16.0f, 16.0f), Id = 0, TexturePath = "ui/uld/WindowH_Bg_Corner.tex" },
                new Part { TextureCoordinates = new Vector2(0.0f, 0.0f), Size = new Vector2(16.0f, 16.0f), Id = 1, TexturePath = "ui/uld/WindowH_Bg_H.tex" },
                new Part { TextureCoordinates = new Vector2(16.0f, 0.0f), Size = new Vector2(16.0f, 16.0f), Id = 2, TexturePath = "ui/uld/WindowH_Bg_Corner.tex" },
                new Part { TextureCoordinates = new Vector2(0.0f, 0.0f), Size = new Vector2(16.0f, 8.0f), Id = 3, TexturePath = "ui/uld/WindowH_Bg_V.tex" },
                new Part { TextureCoordinates = new Vector2(0.0f, 0.0f), Size = new Vector2(16.0f, 8.0f), Id = 4, TexturePath = "ui/uld/WindowH_Bg_HV.tex" },
                new Part { TextureCoordinates = new Vector2(16.0f, 0.0f), Size = new Vector2(16.0f, 8.0f), Id = 5, TexturePath = "ui/uld/WindowH_Bg_V.tex" },
                new Part { TextureCoordinates = new Vector2(0.0f, 16.0f), Size = new Vector2(16.0f, 16.0f), Id = 6, TexturePath = "ui/uld/WindowH_Bg_Corner.tex" },
                new Part { TextureCoordinates = new Vector2(0.0f, 16.0f), Size = new Vector2(16.0f, 16.0f), Id = 7, TexturePath = "ui/uld/WindowH_Bg_H.tex" },
                new Part { TextureCoordinates = new Vector2(16.0f, 16.0f), Size = new Vector2(16.0f, 16.0f), Id = 8, TexturePath = "ui/uld/WindowH_Bg_Corner.tex" },
            ],
            PartsRenderType = 53
        };
        BackgroundTextureNode.AttachNode(this);

        BackgroundImageNode = new SimpleImageNode {
            NodeId = 9,
            WrapMode = WrapMode.Stretch,
            NodeFlags = NodeFlags.AnchorTop | NodeFlags.AnchorLeft |NodeFlags.AnchorRight | NodeFlags.AnchorBottom |
                        NodeFlags.Visible | NodeFlags.Enabled | NodeFlags.EmitsEvents,
            TexturePath = "ui/uld/WindowA_Gradation.tex",
            TextureCoordinates = new Vector2(6.0f, 2.0f),
            TextureSize = new Vector2(24.0f, 24.0f),
        };
        BackgroundImageNode.AttachNode(this);

        HeaderContainerNode = new ResNode {
            NodeId = 2,
            Size = new Vector2(0.0f, 38.0f),
            NodeFlags = NodeFlags.AnchorTop | NodeFlags.AnchorLeft | NodeFlags.AnchorRight |
                        NodeFlags.Visible | NodeFlags.Enabled | NodeFlags.EmitsEvents,
        };
        HeaderContainerNode.AttachNode(this);

        Data->ShowCloseButton = 1;
        Data->ShowConfigButton = 0;
        Data->ShowHelpButton = 0;
        Data->ShowHeader = 1;
        Data->Nodes[0] = 0;
        Data->Nodes[1] = 0;
        Data->Nodes[2] = 0;
        Data->Nodes[3] = 0;
        Data->Nodes[4] = 0;
        Data->Nodes[5] = 0;
        Data->Nodes[6] = HeaderContainerNode.NodeId;
        Data->Nodes[7] = 0;

        LoadTimelines();

        InitializeComponentEvents();
    }

    /// <inheritdoc />
    protected override void OnSizeChanged() {
        base.OnSizeChanged();

        HeaderContainerNode.Width = Width;
        HeaderCollisionNode.Width = Width - 14.0f;
        BackgroundTextureNode.Size = Size;
        BackgroundImageNode.Size = new Vector2(Width - 8.0f, Height - 16.0f);
        BackgroundImageNode.Position = new Vector2(4.0f, 4.0f);
    }

    private void LoadTimelines() {
        AddTimeline(new TimelineBuilder()
            .BeginFrameSet(1, 29)
            .AddLabelPair(1, 9, 17)
            .AddLabelPair(10, 19, 18)
            .AddLabelPair(20, 29, 7)
            .EndFrameSet()
            .Build());

        BackgroundTextureNode.AddTimeline(new TimelineBuilder()
            .AddFrameSetWithFrame(1, 9, 1, multiplyColor: new Vector3(100.0f))
            .AddFrameSetWithFrame(10, 19, 10, multiplyColor: new Vector3(100.0f))
            .AddFrameSetWithFrame(20, 29, 20, multiplyColor: new Vector3(50.0f))
            .Build());
    }
}