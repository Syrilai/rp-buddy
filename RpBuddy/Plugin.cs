using System;
using Dalamud.Game.Chat;
using Dalamud.Game.Command;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using RpBuddy.Utils;
using RpBuddy.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KamiToolKit;
using KamiToolKit.UiOverlay;
using RpBuddy.Addons;
using RpBuddy.Addons.Overlays;
using RpBuddy.Extensions;
using RpBuddy.Inventory;

namespace RpBuddy;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;

    private const string CommandName = "/rpbuddy";

    private readonly WindowSystem _windowSystem = new("RP Buddy");

    public Plugin()
    {
        KamiToolKitLibrary.Initialize(PluginInterface, PluginInterface.InternalName);
        
        Shared.ItemCatalog = new CustomItemCatalog();
        Shared.Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        Shared.Inventory = new LocalInventory();

        Shared.Windows.Main = new MainWindow(this);
        Shared.Windows.Config = new ConfigWindow(this);

        _windowSystem.AddWindow(Shared.Windows.Main);
        _windowSystem.AddWindow(Shared.Windows.Config);

        Service<ICommandManager>.Get().AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Shows the RP Buddy introduction"
        });

        PluginInterface.UiBuilder.Draw += _windowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUi;
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUi;

        Service<IChatGui>.Get().ChatMessage += ChatGui_ChatMessage;
        Service<IPluginLog>.Get().Information("Plugin created");

        Shared.Addons.RpInventory = new RpInventoryAddon
        {
            InternalName = "RpInventory",
            Title = "RP Inventory"
        };
        Shared.Addons.ContextMenu = new ContextMenuAddon
        {
            InternalName = "ContextMenu",
            Title = ""
        };
        

        Service<IFramework>.Get().RunSafely(() =>
        {
            Shared.OverlayController = new OverlayController();

            Shared.Addons.ItemTooltip = new ItemTooltipOverlay();
            
            Shared.OverlayController.AddNode(Shared.Addons.ItemTooltip);
        });
        
        SeedInventory();
    }

    private void ChatGui_ChatMessage(IHandleableChatMessage message) {
        if (message.IsHandled)
            return;

        if (!Shared.Configuration.IsChatTypeEnabled(message.LogKind))
            return;

        var macroSender = NativeStringConverter.SeStringToMacroCode(message.Sender);
        var macroMessage = NativeStringConverter.SeStringToMacroCode(message.Message);

        var isSayChat = message.LogKind == XivChatType.Say;
        var isRoleplaying = false;
        var hasChanges = false;

        var playerPayload = message.Sender.Payloads.OfType<PlayerPayload>().FirstOrDefault();
        if (playerPayload != null) {
            var playerCharacter = PlayerManager.GetPlayerCharacterFromPayload(playerPayload);
            if (playerCharacter != null) {
                isRoleplaying = playerCharacter.OnlineStatus.RowId == 22;
            }
        }
        else
        {
            var lp = Service<IObjectTable>.Get().LocalPlayer;
            if ((lp != null && lp.Name.TextValue == message.OriginalSender.ExtractText()) || (Shared.Configuration.AlwaysAssumeLocalPlayer && lp != null))
            {
                var playerCharacter = PlayerManager.GetPlayerCharacterFromPayload(new PlayerPayload(lp.Name.TextValue, lp.HomeWorld.RowId));
                if (playerCharacter != null)
                {
                    isRoleplaying = playerCharacter.OnlineStatus.RowId == 22;
                }
            }
        }

        // Configuration checks
        if (Shared.Configuration.RequiresRoleplayingTag && !isRoleplaying)
        {
            return;
        }

        if (Shared.Configuration.ShowRoleplayTagInChat && isRoleplaying)
        {
            hasChanges = true;
            macroSender = $"<icon({(uint)BitmapFontIcon.RolePlaying})> " + macroSender;
        }

        // Check for pipe prefix - handle leading whitespace properly
        var trimmedMessage = macroMessage.TrimStart();
        var startsWithPipe = trimmedMessage.StartsWith("||") || trimmedMessage.StartsWith("|");

        if (startsWithPipe)
        {
            if (trimmedMessage.StartsWith("||"))
            {
                macroMessage = trimmedMessage.Substring(2).TrimStart();
            }
            else if (trimmedMessage.StartsWith("|"))
            {
                macroMessage = trimmedMessage.Substring(1).TrimStart();
            }
        }

        var treatAsEmoteChat = isSayChat || startsWithPipe;
        var treatAsEmoteChatCheck = Shared.Configuration.TreatSayAsEmoteForEveryone
            ? Shared.Configuration.TreatSayAsEmote && treatAsEmoteChat
            : Shared.Configuration.TreatSayAsEmote && treatAsEmoteChat && isRoleplaying;

        var parser = new ChatParser();
        var tokens = parser.Tokenize(macroMessage);

        var textOnly = new StringBuilder();
        foreach (var token in tokens)
        {
            if (token is TextToken textToken)
            {
                textOnly.Append(textToken.Text);
            }
        }

        var text = textOnly.ToString().TrimStart();

        var hasRpPatterns = text.Contains('"') || text.Contains('*') ||
                            text.Contains('(') || text.Contains('[') ||
                            text.Contains("(d)") || text.Contains("(c)");

        if (hasRpPatterns || treatAsEmoteChatCheck || startsWithPipe)
        {
            hasChanges = true;
            var formattedToken = new List<MacroToken>();

            if (startsWithPipe)
            {
                formattedToken.Add(new MacroTagToken($"icon({(uint)BitmapFontIcon.ArrowDown})"));
                formattedToken.Add(new TextToken("\n"));
            }

            if (treatAsEmoteChatCheck)
            {
                formattedToken.Add(new MacroTagToken($"color({ChatParser.GetColorForMatchType(MatchType.Action)})"));
            }

            var processedTokens = parser.ApplyRpFormatting(tokens);
            formattedToken.AddRange(processedTokens);

            if (treatAsEmoteChatCheck)
            {
                formattedToken.Add(new MacroTagToken("color(stackcolor)"));
            }

            macroMessage = parser.SerializeTokens(formattedToken);
        }

        if (hasChanges)
        {
            message.Sender = NativeStringConverter.MacroCodeToSeString(macroSender);
            message.Message = NativeStringConverter.MacroCodeToSeString(macroMessage);
        }
    }

    public void Dispose()
    {
        PluginInterface.UiBuilder.Draw -= _windowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi -= ToggleConfigUi;
        PluginInterface.UiBuilder.OpenMainUi -= ToggleMainUi;
        
        Service<IChatGui>.Get().ChatMessage -= ChatGui_ChatMessage;
        
        _windowSystem.RemoveAllWindows();

        Shared.Windows.Main.Dispose();
        Shared.Windows.Config.Dispose();

        Service<ICommandManager>.Get().RemoveHandler(CommandName);
        
        Shared.Addons.RpInventory.Dispose();
        Shared.Addons.ContextMenu.Dispose();
        Shared.Addons.ItemTooltip.Dispose();
        Shared.OverlayController.Dispose();
        KamiToolKitLibrary.Dispose();
    }

    private void OnCommand(string command, string args)
    {
        switch (args.Split(' ').First().ToLower())
        {
            case "inventory":
                Shared.Addons.RpInventory.Toggle();
                break;
            case "e":
                Shared.Addons.ContextMenu.Toggle();
                break;
            default:
                Shared.Windows.Main.Toggle();
                break;
        }
    }

    private void SeedInventory()
    {
        Shared.ItemCatalog.Register(new CustomItem
        {
            Id = Guid.Empty,
            Name = "Tropical Sunset",
            IconId = 24415,
            Description = "Freshly mixed watermelon juice, some lime and apple juice, topped off with a slice of lime.",
            MaxStackSize = 1
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
}
