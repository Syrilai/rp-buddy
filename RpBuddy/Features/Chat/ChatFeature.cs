using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dalamud.Game.Chat;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using RpBuddy.Interface;
using RpBuddy.Utils;
using Syrilib.Extensions.Dalamud;
using SeStringBuilder = Lumina.Text.SeStringBuilder;

namespace RpBuddy.Features.Chat;

public class ChatFeature : IFeature
{
    private readonly IChatGui _chatGui;
    private readonly IObjectTable _objectTable;
    private readonly ChatParser _parser;
    
    public ChatFeature()
    {
        _chatGui = IChatGui.Get();
        _objectTable = IObjectTable.Get();
        _parser = new ChatParser();

        _chatGui.ChatMessage += OnChatMessage;
    }
    
    public bool IsEnabled => true;
    
    public void Dispose()
    {
        _chatGui.ChatMessage -= OnChatMessage;
    }
    
    private unsafe void OnChatMessage(IHandleableChatMessage chatMessage)
    {
        if (chatMessage.IsHandled)
            return;

        var macroSender = NativeStringConverter.SeStringToMacroCode(chatMessage.Sender);
        var macroMessage = NativeStringConverter.SeStringToMacroCode(chatMessage.Message);

        var isSayChat = chatMessage.LogKind == XivChatType.Say;
        var isRoleplaying = false;
        var isTargeted = false;
        var hasNameChanges = false;
        var hasMessageChanges = false;
        uint onlineStatus = 0;

        var lp = _objectTable.LocalPlayer;
        var playerPayload = chatMessage.Sender.Payloads.OfType<PlayerPayload>().FirstOrDefault();
        if (playerPayload is null)
        {
            if (lp is { } localPlayer)
            {
                playerPayload = new PlayerPayload(localPlayer.Name.TextValue, localPlayer.HomeWorld.RowId);
            }
        }

        if (playerPayload is not null)
            if (PlayerManager.GetPlayerCharacterFromPayload(playerPayload) is { } playerCharacter)
            {
                onlineStatus = playerCharacter.OnlineStatus.RowId;
                isRoleplaying = playerCharacter.OnlineStatus.RowId == 22;
                if (lp is { } localPlayer)
                {
                    var character = (Character*)playerCharacter.Address;
                    var targetId = character->GetTargetId();
                    isTargeted = targetId.ObjectId == localPlayer.GameObjectId;
                }
            }

        if (Shared.Configuration.ChatFeature.ShowOnlineStatusInChat)
        {
            BitmapFontIcon icon = onlineStatus switch
            {
                4 => BitmapFontIcon.Meteor,
                11 => BitmapFontIcon.MentorPvE,
                12 => BitmapFontIcon.DoNotDisturb,
                15 => BitmapFontIcon.WatchingCutscene,
                17 => BitmapFontIcon.Away,
                18 => BitmapFontIcon.CameraMode,
                22 => BitmapFontIcon.RolePlaying,
                23 => BitmapFontIcon.LookingForParty,
                25 => BitmapFontIcon.WaitingForDutyFinder,
                26 => BitmapFontIcon.GroupFinder,
                27 => BitmapFontIcon.Mentor,
                28 => BitmapFontIcon.MentorPvE,
                29 => BitmapFontIcon.MentorCrafting,
                30 => BitmapFontIcon.MentorPvP,
                31 => BitmapFontIcon.Returner,
                32 => BitmapFontIcon.NewAdventurer,
                36 => BitmapFontIcon.PartyLeader,
                37 => BitmapFontIcon.PartyMember,
                38 => BitmapFontIcon.CrossWorldPartyLeader,
                39 => BitmapFontIcon.CrossWorldPartyMember,
                
                0 or 21 or 43 or 47 => BitmapFontIcon.AnyClass,
                #if DEBUG
                _ => BitmapFontIcon.Warning
                #else
                _ => BitmapFontIcon.AnyClass
                #endif
            };

            if (icon != BitmapFontIcon.AnyClass)
            {
                hasNameChanges = true;
                macroSender = new SeStringBuilder()
                    .AppendIcon((uint)icon)
                    .Append($"{(icon == BitmapFontIcon.Warning ? $"({onlineStatus})" : "")} ")
                    .AppendMacroString(macroSender)
                    .ToReadOnlySeString()
                    .ToMacroString();
            }
        }

        if (Shared.Configuration.ChatFeature.RequiresRoleplayingTag && !isRoleplaying)
        {
            if (Shared.Configuration.ChatFeature.ShowTargetedInChat && isTargeted)
            {
                hasNameChanges = true;
                macroSender = new SeStringBuilder()
                    .AppendIcon((uint)BitmapFontIcon.GreenDot)
                    .AppendMacroString(macroSender)
                    .ToReadOnlySeString()
                    .ToMacroString();
            }
            
            if (hasNameChanges)
                chatMessage.Sender = NativeStringConverter.MacroCodeToSeString(macroSender);
            return;
        }

        if (!Shared.Configuration.ChatFeature.ShowOnlineStatusInChat && Shared.Configuration.ChatFeature.ShowRoleplayTagInChat && isRoleplaying)
        {
            hasNameChanges = true;
            macroSender = new SeStringBuilder()
                .AppendIcon((uint)BitmapFontIcon.RolePlaying)
                .Append(" ")
                .AppendMacroString(macroSender)
                .ToReadOnlySeString()
                .ToMacroString();
        }
        
        if (Shared.Configuration.ChatFeature.ShowTargetedInChat && isTargeted)
        {
            hasNameChanges = true;
            macroSender = new SeStringBuilder()
                .AppendIcon((uint)BitmapFontIcon.GreenDot)
                .AppendMacroString(macroSender)
                .ToReadOnlySeString()
                .ToMacroString();
        }


        var trimmedMessage = macroMessage.Trim();
        var startsWithPipe = trimmedMessage.StartsWith("||") || trimmedMessage.StartsWith('|');

        if (startsWithPipe)
        {
            macroMessage = trimmedMessage.StartsWith("||") ? trimmedMessage[2..].TrimStart() : trimmedMessage[1..].TrimStart();
        }

        var couldBeEmoteChat = isSayChat || startsWithPipe;
        var treatAsEmoteChat =
            Shared.Configuration.ChatFeature.TreatSayAsEmoteForEveryone
                ? Shared.Configuration.ChatFeature.TreatSayAsEmote && couldBeEmoteChat
                : Shared.Configuration.ChatFeature.TreatSayAsEmote && couldBeEmoteChat && isRoleplaying;

        var tokens = _parser.Tokenize(macroMessage);

        var textOnly = new StringBuilder();
        foreach (var token in tokens)
        {
            if (token is TextToken textToken)
                textOnly.Append(textToken.Text);
        }

        var text = textOnly
            .ToString()
            .TrimStart();
        
        var hasRpPatterns = text.Contains('"') || text.Contains('*') ||
                            text.Contains('(') || text.Contains('[') ||
                            text.Contains("(d)") || text.Contains("(c)");

        if (IsEnabled && Shared.Configuration.ChatFeature.IsChatTypeEnabled(chatMessage.LogKind) && (hasRpPatterns || treatAsEmoteChat || startsWithPipe))
        {
            // Do we really need to say this here already?
            hasMessageChanges = true;
            var formattedTokens = new List<MacroToken>();

            if (startsWithPipe)
            {
                formattedTokens.Add(new MacroTagToken($"icon({(uint)BitmapFontIcon.ArrowDown})"));
                formattedTokens.Add(new TextToken("\n"));
            }
            
            if (treatAsEmoteChat)
                formattedTokens.Add(new MacroTagToken($"color({ChatParser.GetColorForMatchType(MatchType.Action)})"));

            var processedTokens = _parser.ApplyRpFormatting(tokens);
            formattedTokens.AddRange(processedTokens);
            
            if (treatAsEmoteChat)
                formattedTokens.Add(new MacroTagToken("color(stackcolor)"));

            macroMessage = _parser.SerializeTokens(formattedTokens);
        }

        if (hasNameChanges)
            chatMessage.Sender = NativeStringConverter.MacroCodeToSeString(macroSender);
        if (hasMessageChanges)
            chatMessage.Message = NativeStringConverter.MacroCodeToSeString(macroMessage);
    }

    private void ApplyStatusIcon(IHandleableChatMessage chatMessage)
    {
    }

    private void ApplyRoleplayMarkup(IHandleableChatMessage chatMessage)
    {
    }
}