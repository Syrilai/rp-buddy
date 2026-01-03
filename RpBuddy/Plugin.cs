using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using RpBuddy.Windows;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text;
using System.Numerics;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using System.Text.RegularExpressions;
using SimpleTweaksPlugin.ExtraPayloads;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.SubKinds;

namespace RpBuddy;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IPlayerState PlayerState { get; private set; } = null!;
    [PluginService] internal static IDataManager DataManager { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;
    [PluginService] internal static IChatGui ChatGui { get; private set; } = null!;
    [PluginService] internal static IGameConfig GameConfig { get; private set; } = null!;
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
            HelpMessage = "A useful message to display in /xlhelp"
        });

        PluginInterface.UiBuilder.Draw += WindowSystem.Draw;

        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUi;

        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUi;

        ChatGui.ChatMessage += ChatGui_ChatMessageV2;

        Log.Information($"Plugin created");
    }

    private void ChatGui_ChatMessageV2(XivChatType type, int timestamp, ref SeString sender, ref SeString message, ref bool isHandled)
    {
        var inAllowedChatType = new List<XivChatType>()
        {
            // Public Chats
            XivChatType.Say,
            XivChatType.Yell,
            XivChatType.CustomEmote,

            // Party/Alliance Chats
            XivChatType.Party,
            XivChatType.CrossParty,
            XivChatType.Alliance,

            // Free Company, although maybe rare but I don't want to take chances
            XivChatType.FreeCompany,

            // Literally all Cross-World Linkshells and Linkshells
            XivChatType.CrossLinkShell1,
            XivChatType.CrossLinkShell2,
            XivChatType.CrossLinkShell3,
            XivChatType.CrossLinkShell4,
            XivChatType.CrossLinkShell5,
            XivChatType.CrossLinkShell6,
            XivChatType.CrossLinkShell7,
            XivChatType.CrossLinkShell8,
            XivChatType.Ls1,
            XivChatType.Ls2,
            XivChatType.Ls3,
            XivChatType.Ls4,
            XivChatType.Ls5,
            XivChatType.Ls6,
            XivChatType.Ls7,
            XivChatType.Ls8,
            // (at this point it should be a blacklist and not a whitelist man..)

            // Tells
            XivChatType.TellIncoming,
            XivChatType.TellOutgoing,
        };

        if (!inAllowedChatType.Contains(type))
        {
            return;
        }

        if (message.Payloads.Any(p => p is not TextPayload))
        {
            // temporary fix as it lowkey breaks existing paylods until i can be bothered to fix it
            return;
        }

        var isSayChat = type == XivChatType.Say;
        var isRoleplayingStatus = false;

        var playerPayload = sender.Payloads.OfType<PlayerPayload>().FirstOrDefault();

        IPlayerCharacter? playerCharacter = null;

        if (playerPayload != null)
        {
            // Message from another player - find them in ObjectTable
            var player = ObjectTable.FirstOrDefault(obj => obj is IPlayerCharacter pc && pc.Name.TextValue == playerPayload.PlayerName && pc.HomeWorld.RowId == playerPayload.World.RowId);

            if (player != null)
            {
                playerCharacter = player as IPlayerCharacter;
            }
        }
        else
        {
            // No PlayerPayload - verify this is actually YOUR message before using LocalPlayer
            var localPlayer = ObjectTable.LocalPlayer;
            if (localPlayer != null)
            {
                // Get the sender name from the SeString
                var senderName = sender.TextValue;
                
                // Only use LocalPlayer if the sender name matches your character's name
                if (senderName == localPlayer.Name.TextValue)
                {
                    playerCharacter = localPlayer;
                }
            }
        }

        // Check RP status for either the other player or yourself
        if (playerCharacter != null)
        {
            var onlineStatus = playerCharacter.OnlineStatus;

            if (onlineStatus.RowId == 22)
            {
                isRoleplayingStatus = true;
            }
        }

        // Ignore everything if the source is not in the roleplaying status and the confi is enabled.
        if (Configuration.RequiresRoleplayingTag && !isRoleplayingStatus)
        {
            return;
        }

        if (isRoleplayingStatus && Configuration.ShowRoleplayTagInChat)
        {
            sender.Payloads.Insert(0, new IconPayload(BitmapFontIcon.RolePlaying));
        }

        var sayColor = GameConfig.UiConfig.GetUInt("ColorSay");
        var emColor = GameConfig.UiConfig.GetUInt("ColorEmoteUser");
        var echoColor = GameConfig.UiConfig.GetUInt("ColorEcho");

        var text = message.TextValue.TrimStart();

        

        // Check if message starts with | or ||
        var startsWithPipe = text.StartsWith("||") || text.StartsWith("|");
        var pipePrefix = text.StartsWith("||") ? "||" : text.StartsWith("|") ? "|" : "";

        Log.Information("Message: {message}", sender);

        // If it starts with pipe, strip the prefix
        if (startsWithPipe)
        {
            text = text.Substring(pipePrefix.Length).TrimStart(); // Also trim leading spaces after pipe
        }

        // If message starts with pipe, treat it as say chat for emote color purposes
        var treatAsEmoteChat = isSayChat || startsWithPipe;

        var treatAsEmoteChatCheck = Configuration.TreatSayAsEmoteForEveryone ? treatAsEmoteChat && Configuration.TreatSayAsEmote : treatAsEmoteChat && Configuration.TreatSayAsEmote && isRoleplayingStatus;
        // treatAsEmoteChat && Configuration.TreatSayAsEmote && isRoleplayingStatus

        // PRIORITY ORDER: Done > Continued > OOC > Quotes > Emotes

        // Pattern for (d) or (y/y) where both numbers are the same - Done indicator (HIGHEST PRIORITY)
        var donePattern = @"\(d\)|\((\d+)/\1\)";

        // Pattern for (c) or (x/y) where x and y are numbers - Combat/Dice rolls (HIGH PRIORITY)
        var continuedPattern = @"\(c\)|\(\d+/\d+\)";

        // Pattern for OOC text: (...) ((...)) [...] [[...]]
        var oocPattern = @"\(\([^)]+\)\)|\([^)]+\)|\[\[[^\]]+\]\]|\[[^\]]+\]";

        // Pattern for quoted text
        var quotePattern = @"""[^""]*""";

        // Pattern for quoted text italic parts
        var quoteItalicPattern = @"-\S+-|/\S+/|\*\S+\*";

        // Pattern for asterisk text (*text*) that is NOT inside quotes
        var asteriskPattern = @"\*(?=(?:[^""]*""[^""]*"")*[^""]*$)[^*]+\*(?=(?:[^""]*""[^""]*"")*[^""]*$)";

        // Pattern for angle bracket text (<text>) that is NOT inside quotes
        var angleBracketPattern = @"<(?=(?:[^""]*""[^""]*"")*[^""]*$)[^>]+>(?=(?:[^""]*""[^""]*"")*[^""]*$)";

        var hasDone = Regex.IsMatch(text, donePattern);
        var hasContinued = Regex.IsMatch(text, continuedPattern);
        var hasOoc = Regex.IsMatch(text, oocPattern);
        var hasQuotes = Regex.IsMatch(text, quotePattern);
        var hasAsterisks = Regex.IsMatch(text, asteriskPattern);
        var hasAngleBrackets = Regex.IsMatch(text, angleBracketPattern);

        if (hasQuotes || hasAsterisks || hasAngleBrackets || hasOoc || hasContinued || hasDone)
        {
            var newPayloads = new List<Payload>();

            // If message started with pipe, add icon and newline first
            if (startsWithPipe)
            {
                newPayloads.Add(new IconPayload(BitmapFontIcon.ArrowDown));
                newPayloads.Add(new TextPayload("\n"));
            }

            // We check here for the roleplaying status because otherwise we fuck up a lot of normal messages with this move, tehe
            // Start with emote color if it's say chat (or treated as such) that should be treated as emote
            if (treatAsEmoteChatCheck)
            {
                newPayloads.Add(new ColorPayload(GetColor(emColor)));
            }

            // Get done matches first (highest priority)
            var doneMatches = Regex.Matches(text, donePattern);
            
            // Build a list of done ranges to exclude from other pattern matching
            var doneRanges = new List<(int Start, int End)>();
            foreach (Match doneMatch in doneMatches)
            {
                doneRanges.Add((doneMatch.Index, doneMatch.Index + doneMatch.Length));
            }

            // Get continued matches second (high priority)
            var continuedMatches = Regex.Matches(text, continuedPattern).Cast<Match>()
                .Where(m => !IsInsideRange(m.Index, m.Length, doneRanges));
            
            // Build continued ranges
            var continuedRanges = new List<(int Start, int End)>(doneRanges);
            foreach (Match contMatch in continuedMatches)
            {
                continuedRanges.Add((contMatch.Index, contMatch.Index + contMatch.Length));
            }

            // Get OOC matches third, excluding done/continued ranges
            var oocMatches = Regex.Matches(text, oocPattern).Cast<Match>()
                .Where(m => !IsInsideRange(m.Index, m.Length, continuedRanges));
            
            // Build combined exclusion ranges (done + continued + OOC)
            var exclusionRanges = new List<(int Start, int End)>(continuedRanges);
            foreach (Match oocMatch in oocMatches)
            {
                exclusionRanges.Add((oocMatch.Index, oocMatch.Index + oocMatch.Length));
            }

            // Helper function to check if a match is inside any exclusion range
            bool IsInsideRange(int index, int length, List<(int Start, int End)> ranges)
            {
                var end = index + length;
                foreach (var range in ranges)
                {
                    if (index >= range.Start && end <= range.End)
                    {
                        return true;
                    }
                }
                return false;
            }

            // Get all other matches, excluding those inside done/continued/OOC
            var quoteMatches = Regex.Matches(text, quotePattern).Cast<Match>()
                .Where(m => !IsInsideRange(m.Index, m.Length, exclusionRanges));
            var asteriskMatches = Regex.Matches(text, asteriskPattern).Cast<Match>()
                .Where(m => !IsInsideRange(m.Index, m.Length, exclusionRanges));
            var angleBracketMatches = Regex.Matches(text, angleBracketPattern).Cast<Match>()
                .Where(m => !IsInsideRange(m.Index, m.Length, exclusionRanges));

            // Combine all matches and sort by position
            // MatchType: 0 = Quote, 1 = Emote, 2 = OOC, 3 = Continued, 4 = Done
            var allMatches = new List<(int Index, int Length, int MatchType, string Value)>();

            foreach (Match match in quoteMatches)
            {
                allMatches.Add((match.Index, match.Length, 0, match.Value));
            }

            foreach (Match match in asteriskMatches)
            {
                allMatches.Add((match.Index, match.Length, 1, match.Value));
            }

            foreach (Match match in angleBracketMatches)
            {
                allMatches.Add((match.Index, match.Length, 1, match.Value));
            }

            foreach (Match match in oocMatches)
            {
                allMatches.Add((match.Index, match.Length, 2, match.Value));
            }

            foreach (Match match in continuedMatches)
            {
                allMatches.Add((match.Index, match.Length, 3, match.Value));
            }

            foreach (Match match in doneMatches)
            {
                allMatches.Add((match.Index, match.Length, 4, match.Value));
            }

            // Sort by index to process in order
            allMatches = allMatches.OrderBy(m => m.Index).ToList();

            var lastIndex = 0;

            foreach (var match in allMatches)
            {
                // Add any text before this match
                if (match.Index > lastIndex)
                {
                    var beforeText = text.Substring(lastIndex, match.Index - lastIndex);
                    newPayloads.Add(new TextPayload(beforeText));
                }

                switch (match.MatchType)
                {
                    case 0: // Quote
                        Log.Information("Processing quoted text: {text}", match.Value);

                        var italicMatches = Regex.Matches(match.Value, quoteItalicPattern);

                        var subLastIndex = 0;

                        newPayloads.Add(new ColorPayload(GetColor(sayColor)));

                        foreach (Match subMatch in italicMatches)
                        {
                            if (subMatch.Index > subLastIndex)
                            {
                                var subBeforeText = match.Value.Substring(subLastIndex, subMatch.Index - subLastIndex);
                                newPayloads.Add(new TextPayload(subBeforeText));
                            }

                            newPayloads.Add(new EmphasisItalicPayload(true));
                            newPayloads.Add(new TextPayload(subMatch.Value[1..^1]));
                            newPayloads.Add(new EmphasisItalicPayload(false));

                            subLastIndex = subMatch.Index + subMatch.Length;
                        }

                        if (subLastIndex < match.Length)
                        {
                            var subRemainingText = match.Value.Substring(subLastIndex);
                            newPayloads.Add(new TextPayload(subRemainingText));
                        }

                        newPayloads.Add(new ColorEndPayload());
                        break;

                    case 1: // Custom Emotes (** or <>)
                        newPayloads.Add(new ColorPayload(GetColor(emColor)));
                        newPayloads.Add(new TextPayload(match.Value));
                        newPayloads.Add(new ColorEndPayload());
                        break;

                    case 2: // OOC
                        newPayloads.Add(new ColorPayload(GetColor(echoColor)));
                        newPayloads.Add(new TextPayload(match.Value));
                        newPayloads.Add(new ColorEndPayload());
                        break;

                    case 3: // Continued indicator
                        newPayloads.Add(new ColorPayload(GetColor(echoColor)));
                        newPayloads.Add(new TextPayload($"{match.Value} "));
                        newPayloads.Add(new ColorEndPayload());
                        newPayloads.Add(new ColorPayload(new Vector3(0.9921568627451f, 0.41176470588235f, 0.09411764705882f)));
                        newPayloads.Add(new TextPayload("")); // Clock "Unicode" from the game, looks the best imo
                        newPayloads.Add(new ColorEndPayload());
                        break;

                    case 4: // Done indicator
                        newPayloads.Add(new ColorPayload(GetColor(echoColor)));
                        newPayloads.Add(new TextPayload($"{match.Value} "));
                        newPayloads.Add(new ColorEndPayload());
                        newPayloads.Add(new ColorPayload(new Vector3(0.09411764705882f, 0.9921568627451f, 0.13725490196078f)));
                        newPayloads.Add(new TextPayload("✓")); // Its meh, but I can't be bothered to find something better and it's 5 am
                        newPayloads.Add(new ColorEndPayload());
                        break;
                }

                lastIndex = match.Index + match.Length;
            }

            // Add any remaining text after the last match
            if (lastIndex < text.Length)
            {
                var remainingText = text.Substring(lastIndex);
                newPayloads.Add(new TextPayload(remainingText));
            }

            // And one more time, so we can end the payload
            if (treatAsEmoteChatCheck)
            {
                newPayloads.Add(new ColorEndPayload());
            }

            message = new SeString(newPayloads);
        }
        else if (treatAsEmoteChatCheck)
        {
            // No matches found, but we still want to color the entire message as emote
            var newPayloads = new List<Payload>();
            
            // If message started with pipe(bomb), add icon and newline first
            if (startsWithPipe)
            {
                newPayloads.Add(new IconPayload(BitmapFontIcon.ArrowDown));
                newPayloads.Add(new TextPayload("\n"));
            }
            
            newPayloads.Add(new ColorPayload(GetColor(emColor)));
            // Add the stripped text as a new TextPayload since we modified it
            newPayloads.Add(new TextPayload(text));
            newPayloads.Add(new ColorEndPayload());
            
            message = new SeString(newPayloads);
        }
        else if (startsWithPipe)
        {
            // Message starts with pipe but no other RP patterns - still add icon, newline, and emote color if RP status
            var newPayloads = new List<Payload>
            {
                new IconPayload(BitmapFontIcon.ArrowDown),
                new TextPayload("\n")
            };
            
            // If user has RP status, treat piped messages as emote-colored
            if (isRoleplayingStatus && Configuration.TreatSayAsEmote)
            {
                newPayloads.Add(new ColorPayload(GetColor(emColor)));
                newPayloads.Add(new TextPayload(text));
                newPayloads.Add(new ColorEndPayload());
            }
            else
            {
                newPayloads.Add(new TextPayload(text));
            }
            
            message = new SeString(newPayloads);
        }
    }

    private static Vector3 GetColor(uint chatColor)
    {
        var fb = chatColor & 255;
        var fg = (chatColor >> 8) & 255;
        var fr = (chatColor >> 16) & 255;

        return new Vector3(fr / 255f, fg / 255f, fb / 255f);
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
