using System.Numerics;
using KamiToolKit.Enums;
using KamiToolKit.Nodes;
using KamiToolKit.UiOverlay;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Classes;

namespace RpBuddy.Addons.Overlays;

public unsafe class ContextMenuOverlay : OverlayNode
{
    private NineGridNode Background;
    
    public ContextMenuOverlay()
    {
        Background = new NineGridNode
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
        Background.AttachNode(this);
    }
    
    protected override void OnUpdate()
    {
        Size = new Vector2(200, 200);
        Position = new Vector2(100, 100);
    }

    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();
        Background.Size = Size;
    }

    public override OverlayLayer OverlayLayer { get; } = OverlayLayer.AboveUserInterface;
}