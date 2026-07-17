using KamiToolKit.UiOverlay;
using RpBuddy.Addons;
using RpBuddy.Addons.Overlays;
using RpBuddy.Inventory;
using RpBuddy.Windows;

namespace RpBuddy;

public static class Shared
{
    public static Configuration Configuration { get; set; } = null!;
    public static CustomItemCatalog ItemCatalog { get; set; } = null!;
    public static OverlayController OverlayController { get; set; } = null!;
    public static InventoryBase Inventory { get; set; } = null!;

    public static SharedWindows Windows { get; set; } = new();
    public static SharedAddons Addons { get; set; } = new();
}

public class SharedWindows
{
    public MainWindow Main { get; set; } = null!;
    public ConfigWindow Config { get; set; } = null!;
}

public class SharedAddons
{
    public RpInventoryAddon RpInventory { get; set; } = null!;
    public ContextMenuAddon ContextMenu { get; set; } = null!;

    public ItemTooltipOverlay ItemTooltip { get; set; } = null!;
}
