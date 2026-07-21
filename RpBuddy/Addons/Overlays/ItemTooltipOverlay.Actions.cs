using System;
using System.Collections.Generic;
using Dalamud.Game.Text.SeStringHandling;
using Lumina.Text.Payloads;
using Lumina.Text.ReadOnly;
using RpBuddy.Inventory;
using RpBuddy.Inventory.Actions;
using SeStringBuilder = Lumina.Text.SeStringBuilder;

namespace RpBuddy.Addons.Overlays;

public sealed partial class ItemTooltipOverlay
{
    public void SetContents(InventoryItem inventoryItem)
    {
        ItemNameText.String = inventoryItem.Item.Name;
        ItemIcon.IconId = inventoryItem.Item.IconId;
        QuantityText.String = $"{inventoryItem.Quantity}/{inventoryItem.Item.MaxStackSize}";
        CategoryText.String = inventoryItem.Item.Category.Value.Name;

        var hasActions = inventoryItem.Item is { CanBeUsed: true, UseActions.Count: > 0 };

        DescriptionText.String = inventoryItem.Item.Description;
        ActionText.String = hasActions ? BuildActionsString(inventoryItem.Item.UseActions) : "";
        ActionGroup.IsVisible = hasActions;

        RecalculateLayout();
    }

    private static ReadOnlySeString BuildActionsString(IReadOnlyList<ItemActionBase> actions)
    {
        var actionString = new SeStringBuilder();
        actionString
            .BeginMacro(MacroCode.Color)
            .AppendIntExpression(0xFF5959)
            .EndMacro()
            .Append("This item has custom actions!")
            .PopColor()
            .AppendNewLine();

        foreach (var action in actions)
            AddActionText(actionString, action, 0, "");

        return actionString.ToReadOnlySeString();
    }

    private static SeStringBuilder AddActionText(SeStringBuilder text, ItemActionBase action, int spacing = 0, string prefix = "", bool appendNewLine = true)
    {
        if (appendNewLine)
            text.AppendNewLine();

        text
            .Append(new string(' ', spacing * 4))
            .Append(prefix);

        switch (action)
        {
            case ItemDelayedAction delayedAction:
                text.Append($" Delayed Actions: ({Math.Floor(delayedAction.Delay / 1000f)}s)");
                foreach (var actionToExecute in delayedAction.ActionsToExecute)
                    AddActionText(text, actionToExecute, spacing + 1, " ");
                break;
            case ItemCommandAction commandAction:
                text
                    .AppendIcon((uint)BitmapFontIcon.Warning)
                    .Append(" ")
                    .AppendItalicized($"/{commandAction.Command}");
                break;
            default:
                text
                    .AppendIcon((uint)BitmapFontIcon.NoCircle)
                    .Append(" Unknown Action");
                break;
        }

        return text;
    }
}
