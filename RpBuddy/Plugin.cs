using System;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using RpBuddy.Windows;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KamiToolKit;
using KamiToolKit.UiOverlay;
using Lumina.Excel.Sheets;
using RpBuddy.Addons;
using RpBuddy.Addons.Overlays;
using RpBuddy.Features.Chat;
using RpBuddy.Inventory;
using RpBuddy.Inventory.Actions;
using RpBuddy.Services;
using Syrilib;
using Syrilib.Extensions.Dalamud;
using Syrilib.Extensions.Lumina;

namespace RpBuddy;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class Plugin : IAsyncDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;

    private readonly WindowSystem _windowSystem = new("RP Buddy");
    private CommandService? _commandService;
    
    public async Task LoadAsync(CancellationToken cancellationToken)
    {
        await KamiToolKitLibrary.InitializeAsync(PluginInterface);
        SyrilibMain.Initialize(PluginInterface);
        
        cancellationToken.ThrowIfCancellationRequested();
        
        Shared.ItemCatalog = new CustomItemCatalog();
        Shared.Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        Shared.Inventory = new LocalInventory();

        Shared.Features.Chat = new ChatFeature();

        Shared.Windows.Main = new MainWindow(this);
        Shared.Windows.Config = new ConfigWindow(this);

        _windowSystem.AddWindow(Shared.Windows.Main);
        _windowSystem.AddWindow(Shared.Windows.Config);

        PluginInterface.UiBuilder.Draw += _windowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUi;
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUi;

        _commandService = new CommandService();

        await IFramework.Get().RunOnTick(() =>
        {
            Shared.OverlayController = new OverlayController();

            Shared.Addons.ItemTooltip = new ItemTooltipOverlay();

            Shared.OverlayController.AddNode(Shared.Addons.ItemTooltip);
            
            Shared.Addons.RpInventory = new RpInventoryAddon
            {
                InternalName = "RpBuddyRpInventory",
                Title = "RP Inventory"
            };
            Shared.Addons.YesNo = new YesNoAddon
            {
                InternalName = "RpBuddyYesNo",
                Title = "YesNo"
            };
        }, cancellationToken: cancellationToken);
        
        SeedInventory();
    }

    private void SeedInventory()
    {
        Shared.ItemCatalog.Register(new CustomItem
        {
            Id = Guid.Empty,
            Name = "Tropical Sunset",
            IconId = 24415,
            Description = "Freshly mixed watermelon juice, some lime and apple juice, topped off with a slice of lime.",
            MaxStackSize = 1,
            Category = ItemUICategory.GetRowRef(44),
            CanBeUsed = true,
            UseActions = [
                new ItemCommandAction("delighted"),
                new ItemCommandAction("em", "swiftly empties the glass of Tropical Sunset."),
                new ItemDelayedAction(6000, [
                    new ItemCommandAction("stagger"),
                    new ItemCommandAction("em", "seems to be hit with a wave of tipsiness. Maybe drinking it so swiftly wasn't a great idea after all..?")
                ]),
            ]
        });
        
        foreach (var invItem in Shared.ItemCatalog.GetAll().Select(customItem => new InventoryItem
                 {
                     Item = customItem,
                     Quantity = 1
                 }))
        {
            Shared.Inventory.AddItem(invItem);
        }
    }
    
    public void ToggleConfigUi() => Shared.Windows.Config.Toggle();
    public void ToggleMainUi() => Shared.Windows.Main.Toggle();
    
    public async ValueTask DisposeAsync()
    {
        PluginInterface.UiBuilder.Draw -= _windowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi -= ToggleConfigUi;
        PluginInterface.UiBuilder.OpenMainUi -= ToggleMainUi;

        _windowSystem.RemoveAllWindows();
        _commandService?.Dispose();

        Shared.Windows.Main.Dispose();
        Shared.Windows.Config.Dispose();
        

        await Shared.Addons.RpInventory.DisposeAsync();
        await Shared.Addons.YesNo.DisposeAsync();
        await IFramework.Get().RunOnTick(async () =>
        {
            Shared.OverlayController.Dispose();
            
            await KamiToolKitLibrary.DisposeAsync();
        });

        SyrilibMain.Dispose();
    }
}
