using KamiToolKit.UiOverlay;
using RpBuddy.Addons;
using RpBuddy.Addons.Overlays;
using RpBuddy.Features.Chat;
using RpBuddy.Inventory;
using RpBuddy.Windows;

namespace RpBuddy;

public static class Shared
{
    public static Configuration Configuration { get; set; } = null!;
    public static CustomItemCatalog ItemCatalog { get; set; } = null!;
    public static OverlayController OverlayController { get; set; } = null!;
    public static InventoryBase Inventory { get; set; } = null!;

    public static SharedFeatures Features { get; private set; } = new();
    public static SharedWindows Windows { get; private set; } = new();
    public static SharedAddons Addons { get; private set; } = new();
}

public class SharedFeatures
{
    public ChatFeature Chat { get; set; } = null!;
}

public class SharedWindows
{
    public MainWindow Main { get; set; } = null!;
    public ConfigWindow Config { get; set; } = null!;
}

public class SharedAddons
{
    public RpInventoryAddon RpInventory { get; set; } = null!;
    public ContextMenuAddon AddonContextMenu { get; set; } = null!;

    public ItemTooltipOverlay ItemTooltip { get; set; } = null!;
    public ContextMenuOverlay ContextMenu { get; set; } = null!;
}
