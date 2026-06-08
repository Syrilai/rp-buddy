using Dalamud.Game.Chat;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.Command;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs;
using RpBuddy.Utils;
using RpBuddy.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RpBuddy;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;
    [PluginService] internal static IChatGui ChatGui { get; private set; } = null!;
    [PluginService] internal static IObjectTable ObjectTable { get; private set; } = null!;
    [PluginService] internal static IGameConfig GameConfig { get; private set; } = null!;

    public bool IsChatTwoEnabled = false;

    private const string CommandName = "/rpbuddy";

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("RP Buddy");
    private ConfigWindow ConfigWindow { get; init; }
    private MainWindow MainWindow { get; init; }

    public ChatColors ChatColors;

    public static Plugin Instance = null!;

    public Plugin()
    {
        Instance = this;

        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this);

        ChatColors = new();

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);

        PluginInterface.ActivePluginsChanged += ActivePluginsChanged;

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Shows the RP Buddy introduction"
        });

        PluginInterface.UiBuilder.Draw += WindowSystem.Draw;

        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUi;

        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUi;

        ChatGui.ChatMessage += ChatGui_ChatMessage;

        Log.Information($"Plugin created");

        CheckCompatibility();
    }

    internal void ActivePluginsChanged(IActivePluginsChangedEventArgs args)
    {
        if (args.AffectedInternalNames.Any(i => i.Equals("ChatTwo"))) {
            var newValue = args.Kind switch
            {
                PluginListInvalidationKind.Unloaded => false,
                PluginListInvalidationKind.Loaded 
                    or PluginListInvalidationKind.Update
                    or PluginListInvalidationKind.AutoUpdate => true,
                _ => false
            };

            IsChatTwoEnabled = newValue;
        }
    }

    internal void CheckCompatibility()
    {
        foreach (var installedPlugin in PluginInterface.InstalledPlugins)
        {
            if (!installedPlugin.InternalName.Equals("ChatTwo"))
                continue;

            IsChatTwoEnabled = installedPlugin.IsLoaded;
        }
    }

    internal static Lumina.Text.SeStringBuilder NewSeStringBuilder()
    {
        return new Lumina.Text.SeStringBuilder();
    }

    private void ChatGui_ChatMessage(IHandleableChatMessage message) {
        if (message.IsHandled)
            return;

        if (!Configuration.IsChatTypeEnabled(message.LogKind))
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
            var lp = ObjectTable.LocalPlayer;
            if ((lp != null && lp.Name.TextValue == message.OriginalSender.ExtractText()) || (Configuration.AlwaysAssumeLocalPlayer && lp != null))
            {
                var playerCharacter = PlayerManager.GetPlayerCharacterFromPayload(new PlayerPayload(lp.Name.TextValue, lp.HomeWorld.RowId));
                if (playerCharacter != null)
                {
                    isRoleplaying = playerCharacter.OnlineStatus.RowId == 22;
                }
            }
        }

        // Configuration checks
        if (Configuration.RequiresRoleplayingTag && !isRoleplaying)
        {
            return;
        }

        if (Configuration.ShowRoleplayTagInChat && isRoleplaying)
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
        var treatAsEmoteChatCheck = Configuration.TreatSayAsEmoteForEveryone
            ? Configuration.TreatSayAsEmote && treatAsEmoteChat
            : Configuration.TreatSayAsEmote && treatAsEmoteChat && isRoleplaying;

        var parser = new ChatParser(Log);
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
        PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi -= ToggleConfigUi;
        PluginInterface.UiBuilder.OpenMainUi -= ToggleMainUi;
        PluginInterface.ActivePluginsChanged -= ActivePluginsChanged;

        ChatGui.ChatMessage -= ChatGui_ChatMessage;
        
        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();
        MainWindow.Dispose();

        CommandManager.RemoveHandler(CommandName);
    }

    private void OnCommand(string command, string args)
    {
        MainWindow.Toggle();
    }
    
    public void ToggleConfigUi() => ConfigWindow.Toggle();
    public void ToggleMainUi() => MainWindow.Toggle();
}
