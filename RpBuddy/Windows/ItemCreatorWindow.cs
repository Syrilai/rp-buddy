using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.ImGuiNotification;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Newtonsoft.Json;
using RpBuddy.Inventory;
using RpBuddy.Inventory.Actions;
using Syrilib.Extensions.Dalamud;
using Syrilib.Extensions.Lumina;

namespace RpBuddy.Windows;

/*
 * basically the item creator window for the beta
 * im (trying) to hide any method that allows to open it manually
 */
public class ItemCreatorWindow : Window
{
    private new bool IsOpen => base.IsOpen;
    
    private Guid _itemId = Guid.Empty;
    private string _itemName = string.Empty;
    private uint _iconId = 0;
    private string _description = string.Empty;
    private int _maxStackSize = 1;
    private bool _canBeUsed = false;
    private uint _categoryId;
    private List<IItemActionBase> _actions = [];

    private CustomItem? _customItem;
    
    public ItemCreatorWindow() : base("Item Creator###item-creator")
    {
        _categoryId = 44;
    }

    public void OpenItem(CustomItem item)
    {
        _itemId = item.Id;
        _itemName = item.Name;
        _iconId = item.IconId;
        _description = item.Description.ToString();
        _maxStackSize = item.MaxStackSize;
        _canBeUsed = item.CanBeUsed;
        _categoryId = item.CategoryId;
        _actions = item.UseActions;
        _customItem = item;
        
        base.IsOpen = true;
    }

    public void OpenBlank()
    {
        _itemId = Guid.Empty;
        _itemName = string.Empty;
        _iconId = 0;
        _description = string.Empty;
        _maxStackSize = 1;
        _canBeUsed = false;
        _categoryId = 44;
        _actions = [];
        _customItem = null;
        
        base.IsOpen = true;
    }

    public new void Toggle()
    {
        throw new MethodAccessException("Cannot toggle this window manually.");
    }

    public override void Draw()
    {
        ImGui.Text(_itemId.ToString());
        if (ImGui.SmallButton("Copy"))
        {
            ImGui.SetClipboardText(_itemId.ToString());
        }
        
        ImGui.InputText("Item Name", ref _itemName);
        ImGui.InputUInt("Icon ID", ref _iconId);
        ImGui.InputTextMultiline("Description", ref _description);
        ImGui.InputInt("Max Stack Size", ref _maxStackSize);
        ImGui.Checkbox("Can Be Used", ref _canBeUsed);

        using (var combo = ImRaii.Combo("Category", ItemUICategory.GetRow(_categoryId).Name.ToString()))
        {
            if (combo)
                foreach (var row in ItemUICategory.Rows
                                                  .Where(row => !row.Name.IsEmpty))
                {
                    if (ImGui.Selectable(row.Name.ToString()))
                        _categoryId = row.RowId;
                }
        }
        
        // Now we break any and all laws probably
        ImGui.Spacing();

        DrawActionList(_actions, "action");

        if (_itemId != Guid.Empty)
        {
            if (ImGui.Button("Save"))
            {
                Shared.ItemCatalog.Update(_itemId, item =>
                {
                    item.Name = _itemName;
                    item.IconId = _iconId;
                    item.Description = _description;
                    item.MaxStackSize = _maxStackSize;
                    item.CanBeUsed = _canBeUsed;
                    item.Category = ItemUICategory.GetRowRef(_categoryId);
                    item.UseActions = _actions;
                });
                base.IsOpen = false;
            }
        }
        else
        {
            if (ImGui.Button("Create"))
            {
                var item = new CustomItem
                {
                    Id = Guid.CreateVersion7(),
                    Name = _itemName,
                    IconId = _iconId,
                    MacroDescription = _description,
                    MaxStackSize = _maxStackSize,
                    CanBeUsed = _canBeUsed,
                    CategoryId = _categoryId,
                    UseActions = _actions
                };
                
                Shared.ItemCatalog.Register(item);
                base.IsOpen = false;
            }
        }

        if (_customItem is not null)
        {
            ImGui.SameLine();
            if (ImGui.Button("Export"))
            {
                var json = JsonConvert.SerializeObject(_customItem);
                var bytes = Encoding.UTF8.GetBytes(json);
                ImGui.SetClipboardText(Convert.ToBase64String(bytes));
                INotificationManager.Get().AddNotification(
                    new Notification
                    {
                        Content = "Copied item to clipboard.",
                        Type = NotificationType.Info,
                        Title = "Export Success"
                    }
                );
                base.IsOpen = false;
            }
        }
        
        ImGui.SameLine();
        if (ImGui.Button("Import"))
        {
            var clipboard = ImGui.GetClipboardText();
            try
            {
                var bytes = Convert.FromBase64String(clipboard);
                var json = Encoding.UTF8.GetString(bytes);
                var item = JsonConvert.DeserializeObject<CustomItem>(json);
                if (item is null)
                {
                    INotificationManager.Get().AddNotification(
                        new Notification
                        {
                            Content = "Failed to import item: Invalid JSON.",
                            Type = NotificationType.Error,
                            Title = "Import Error"
                        }
                    );
                    return;
                }
                
                Shared.ItemCatalog.Register(item);
                base.IsOpen = false;
            }
            catch (Exception exception)
            {
                INotificationManager.Get().AddNotification(
                    new Notification
                    {
                        Content = "Failed to import item: " + exception.Message,
                        Type = NotificationType.Error,
                        Title = "Import Error"
                    }
                );
            }
        }
    }
    
    private void DrawActionList(List<IItemActionBase> actions, string idPrefix, float indent = 0f)
    {
        ImGui.Indent(indent);

        int? removeAt = null;

        for (var i = 0; i < actions.Count; i++)
        {
            var action = actions[i];
            var id = $"{idPrefix}-{i}";

            using var _ = ImRaii.PushId(id);
            ImGui.Separator();

            switch (action)
            {
                case ItemCommandAction commandAction:
                    DrawCommandAction(commandAction);
                    break;
                case ItemDelayedAction delayedAction:
                    DrawDelayedAction(delayedAction, id);
                    break;
            }

            ImGui.SameLine();
            if (ImGui.SmallButton("Remove"))
                removeAt = i;
        }

        if (removeAt.HasValue)
            actions.RemoveAt(removeAt.Value);

        ImGui.Spacing();

        if (ImGui.SmallButton("Add Command Action"))
            actions.Add(new ItemCommandAction(""));

        ImGui.SameLine();
        if (ImGui.SmallButton("Add Delayed Action"))
            actions.Add(new ItemDelayedAction(1000, []));

        ImGui.Unindent(indent);
    }
    
    private void DrawCommandAction(ItemCommandAction commandAction)
    {
        ImGui.SetNextItemWidth(200f);
        var command = commandAction.Command;
        if (ImGui.InputText("Command", ref command, 64))
            commandAction.Command = command;

        ImGui.SameLine();
        ImGui.SetNextItemWidth(200f);
        var arguments = commandAction.Arguments ?? string.Empty;
        if (ImGui.InputText("Arguments", ref arguments, 128))
            commandAction.Arguments = string.IsNullOrEmpty(arguments) ? "" : arguments;
    }

    private void DrawDelayedAction(ItemDelayedAction delayedAction, string id)
    {
        ImGui.SetNextItemWidth(120f);
        var delay = delayedAction.Delay;
        if (ImGui.InputInt("Delay (ms)", ref delay))
            delayedAction.Delay = Math.Max(0, delay);

        ImGui.TextDisabled("Runs after delay:");
        DrawActionList(delayedAction.ActionsToExecute, $"{id}-child", indent: 20f);
    }
}