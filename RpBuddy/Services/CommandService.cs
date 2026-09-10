using System;
using System.Linq;
using Dalamud.Game.Command;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using Lumina.Excel.Sheets;
using RpBuddy.Inventory;
using Syrilib.Extensions.Dalamud;
using Syrilib.Extensions.Lumina;

namespace RpBuddy.Services;

public class CommandService : IDisposable
{
    private const string CommandName = "/rpbuddy";
    
    public CommandService()
    {
        ICommandManager.Get().AddHandler(
            command: CommandName,
            info: new CommandInfo(OnCommand)
            {
                AllowedInMacros = false,
                DisplayOrder = 0,
                HelpMessage = "Help Message",
                ShowInHelp = true
            }
        );
    }
    
    public void Dispose()
    {
        ICommandManager.Get().RemoveHandler(
            command: CommandName
        );
    }

    private static void OnCommand(string command, string args)
    {
        switch (args.Split(' ').First().ToLower())
        {
            case "inventory":
                Shared.Addons.RpInventory.Toggle();
                break;
            case "additem":
                var rawItemId = args.Split(' ').Skip(1).First();
                if (!Guid.TryParse(rawItemId, out var guid))
                    break;

                if (!Shared.ItemCatalog.TryGet(guid, out var item))
                    break;

                if (!int.TryParse(args.Split(' ').Skip(2).First(), out var amount))
                    break;

                Shared.Inventory.AddItem(new InventoryItem
                {
                    ItemId = guid,
                    Quantity = amount
                });
                
                IChatGui.Get().Print($"Added {amount}x {item.Name}");
                break;
            case "test":
                Test();
                break;
            case "catalog":
                Shared.Windows.ItemCatalog.Toggle();
                break;
            default:
                Shared.Windows.Main.Toggle();
                break;
        }
    }

    private static void Test()
    {
        var unlockState = IUnlockState.Get();
        var rows = Emote.Rows.Where(row => row.Icon > 0).ToList();
        var unlocked = rows.Count(unlockState.IsEmoteUnlocked);
        IChatGui.Get().Print($"State: {unlocked}/{rows.Count}");
    }
}