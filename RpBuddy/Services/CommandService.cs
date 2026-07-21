using System;
using System.Linq;
using Dalamud.Game.Command;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using RpBuddy.Inventory;
using Syrilib.Extensions.Dalamud;

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
                    Item = item,
                    Quantity = amount
                });
                
                IChatGui.Get().Print($"Added {amount}x {item.Name}");
                break;
            default:
                Shared.Windows.Main.Toggle();
                break;
        }
    }
}