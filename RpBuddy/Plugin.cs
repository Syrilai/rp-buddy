using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using RpBuddy.Windows;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using System.Collections.Generic;
using System.Linq;
using RpBuddy.Utils;
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

    private const string CommandName = "/rpbuddy";

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("RP Buddy");
    private ConfigWindow ConfigWindow { get; init; }
    private MainWindow MainWindow { get; init; }

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this);

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Shows the RP Buddy introduction"
        });

        PluginInterface.UiBuilder.Draw += WindowSystem.Draw;

        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUi;

        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUi;

        ChatGui.ChatMessage += ChatGui_ChatMessageV3;

        Log.Information($"Plugin created");
    }

    internal static Lumina.Text.SeStringBuilder NewSeStringBuilder()
    {
        return new Lumina.Text.SeStringBuilder();
    }

    private void ChatGui_ChatMessageV3(XivChatType type, int timestamp, ref SeString sender, ref SeString message, ref bool isHandled)
    {
        // Check if chat type is enabled
        if (!Configuration.IsChatTypeEnabled(type))
        {
            Log.Debug("Aborting: {channel} is not enabled", type);
            return;
        }

        var macroSender = NativeStringConverter.SeStringToMacroCode(sender);
        var macroMessage = NativeStringConverter.SeStringToMacroCode(message);

        Log.Debug("(old) {sender}: {message}", macroSender, macroMessage);

        var isSayChat = type == XivChatType.Say;
        var isRoleplaying = false;
        var hasChanges = false;

        var playerPayload = sender.Payloads.OfType<PlayerPayload>().FirstOrDefault();
        if (playerPayload != null)
        {
            var playerCharacter = PlayerManager.GetPlayerCharacterFromPayload(playerPayload);
            if (playerCharacter != null)
            {
                isRoleplaying = playerCharacter.OnlineStatus.RowId == 22;
            }
        }
        else
        {
            var lp = ObjectTable.LocalPlayer;
            if (lp != null && lp.Name.TextValue == sender.TextValue)
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
            Log.Debug("Aborting: Requires RP Status but it is not available");
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
                formattedToken.Add(new MacroTagToken($"color(gnum{(int)GlobalExpressions.ColorEmoteUser})"));
            }

            var processedTokens = parser.ApplyRpFormatting(tokens);
            formattedToken.AddRange(processedTokens);

            if (treatAsEmoteChatCheck)
            {
                formattedToken.Add(new MacroTagToken("color(stackcolor)"));
            }

            macroMessage = parser.SerializeTokens(formattedToken);
        }

        Log.Debug("(new) {sender}: {message}", macroSender, macroMessage);

        if (hasChanges)
        {
            sender = NativeStringConverter.MacroCodeToSeString(macroSender);
            message = NativeStringConverter.MacroCodeToSeString(macroMessage);
        }
    }

    public void Dispose()
    {
        PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi -= ToggleConfigUi;
        PluginInterface.UiBuilder.OpenMainUi -= ToggleMainUi;
        
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
