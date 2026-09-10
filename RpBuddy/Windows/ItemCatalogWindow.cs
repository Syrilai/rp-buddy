using System;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using Syrilib.Extensions.Dalamud;

namespace RpBuddy.Windows;

public class ItemCatalogWindow : Window, IDisposable
{
    public ItemCatalogWindow() : base("Item Catalog###item-catalog")
    {
    }

    public override void Draw()
    {
        const float ItemSize = 64f;
        const float RowPadding = 4f;
        const float LabelGap = 8f;
        const float LineGap = 2f;

        var items = Shared.ItemCatalog.All.ToArray();
        var itemCount = items.Length;
        var rowHeight = ItemSize + RowPadding;
        
        if (ImGui.Button("Open Blank Item Creator"))
        {
            Shared.Windows.ItemCreator.OpenBlank();
        }

        var textureProvider = ITextureProvider.Get();
        var dl = ImGui.GetWindowDrawList();
        var origin = ImGui.GetCursorPos();
        var itemSizeVec = new Vector2(ItemSize);
        var availWidth = ImGui.GetContentRegionAvail().X;
        var rowSize = new Vector2(availWidth, ItemSize);
        var fontSize = ImGui.GetFontSize();
        var totalTextHeight = fontSize * 2f + LineGap;
        var blockTop = rowSize.Y / 2f - totalTextHeight / 2f;
        
        dl.ChannelsSplit(2);
        dl.ChannelsSetCurrent(1);

        var log = IPluginLog.Get();

        unsafe
        {
            var clipper = new ImGuiListClipperPtr(ImGuiNative.ImGuiListClipper());
            clipper.Begin(itemCount, rowHeight);

            while (clipper.Step())
            {
                for (var idx = clipper.DisplayStart; idx < clipper.DisplayEnd; idx++)
                {
                    DrawItem(idx);
                }
            }

            clipper.End();
            clipper.Destroy();
        }
        
        dl.ChannelsMerge();

        void DrawItem(int index)
        {
            var item = items[index];

            using (ImRaii.PushId($"item-{item.Id}"))
            {
                ImGui.SetCursorPos(origin + new Vector2(0, index * rowHeight));
                var screenPos = ImGui.GetCursorScreenPos();
                var iconEnd = screenPos + itemSizeVec;

                if (textureProvider.TryGetFromGameIcon(new GameIconLookup(item.IconId, hiRes: true), out var texture)
                    && texture.TryGetWrap(out var wrap, out _))
                {
                    dl.AddImageRounded(
                        wrap.Handle,
                        screenPos,
                        iconEnd,
                        Vector2.Zero,
                        Vector2.One,
                        ImGui.GetColorU32(Vector4.One),
                        8f
                    );

                    

                    var itemNamePos = screenPos + new Vector2(ItemSize + LabelGap, blockTop);
                    var itemActionsPos = screenPos + new Vector2(ItemSize + LabelGap, blockTop + fontSize + LineGap);
                    dl.AddText(itemNamePos, ImGui.GetColorU32(ImGuiCol.Text), item.Name);
                    dl.AddText(itemActionsPos, ImGui.GetColorU32(ImGuiCol.TextDisabled), $"{item.UseActions.Count} Actions");
                }
                else
                {
                    ImGui.SetCursorScreenPos(screenPos);
                    ImGui.Text($"Fallback\n{item.IconId}");
                }

                // Hit area spans the full row width, not just the icon
                if (ImGui.InvisibleButton("button", rowSize))
                {
                    Shared.Windows.ItemCreator.OpenItem(item);
                }

                if (ImGui.IsItemHovered())
                {
                    ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                    dl.ChannelsSetCurrent(0);
                    dl.AddRectFilled(screenPos, screenPos + rowSize, ImGui.GetColorU32(ImGuiCol.FrameBgHovered), 8f);
                    dl.ChannelsSetCurrent(1);
                }
            }
        }
    }

    public void Dispose()
    {
    }
}