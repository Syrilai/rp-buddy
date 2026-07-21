using System.Numerics;
using KamiToolKit.Enums;

namespace RpBuddy.Addons.Overlays;

public sealed partial class ItemTooltipOverlay
{
    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();
        Container.Size = Size;
        BackgroundTextureNode.Size = Size;
    }

    private void RecalculateLayout()
    {
        var nameTextSize = ItemNameText.GetTextDrawSize();
        ItemNameText.Size = new Vector2(TooltipLayout.NameWidth, nameTextSize.Y);

        var descTextSize = DescriptionText.GetTextDrawSize();
        DescriptionText.Size = new Vector2(TooltipLayout.SectionTextWidth, descTextSize.Y);

        var hasActions = ActionGroup.IsVisible;
        var actionTextSize = hasActions ? ActionText.GetTextDrawSize() : Vector2.Zero;

        var currentY = TooltipLayout.HeaderHeight + TooltipLayout.SectionSpacing;

        DescriptionGroup.Position = new Vector2(0, currentY);
        DescriptionGroup.Size = new Vector2(TooltipLayout.GroupWidth,
            TooltipLayout.SectionTextTop + descTextSize.Y);

        currentY += TooltipLayout.SectionTextTop + descTextSize.Y;

        ActionGroup.Position = new Vector2(0, currentY);
        ActionGroup.Size = new Vector2(TooltipLayout.GroupWidth,
            hasActions ? TooltipLayout.SectionTextTop + actionTextSize.Y : 0);

        if (hasActions)
        {
            ActionText.Size = new Vector2(TooltipLayout.SectionTextWidth, actionTextSize.Y);
            currentY += TooltipLayout.SectionTextTop + actionTextSize.Y;
        }
        else
        {
            ActionText.Size = new Vector2(TooltipLayout.SectionTextWidth, 0);
        }

        Size = new Vector2(TooltipLayout.Width, currentY + TooltipLayout.BottomPadding);
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
}
