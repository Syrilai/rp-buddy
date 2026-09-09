using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dalamud.Game.Chat;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Lumina.Excel.Sheets;
using RpBuddy.Interface;
using RpBuddy.Utils;
using Syrilib.Enums.Lumina;
using Syrilib.Extensions.Dalamud;

namespace RpBuddy.Features.Chat;

public class ChatFeature : IFeature
{
    private readonly IChatGui     _chatGui;
    private readonly IObjectTable _objectTable;
    private readonly ChatParser   _parser;

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

    private void OnChatMessage(IHandleableChatMessage chatMessage)
    {
        if (chatMessage.IsHandled)
            return;
        
        ApplyStatusIcon(chatMessage);
        ApplyTargetIndicator(chatMessage);
        ApplyRoleplayMarkup(chatMessage);
    }

    private void ApplyStatusIcon(IHandleableChatMessage chatMessage)
    {
        if (!(Shared.Configuration.ChatFeature.ShowOnlineStatusInChat ||
              Shared.Configuration.ChatFeature.ShowRoleplayTagInChat))
            return;
        
        var playerInfo = GetPlayerInfo(chatMessage);
        if (playerInfo?.OnlineStatus is not { Value: var onlineStatus })
            return;
        
        var showOnline = Shared.Configuration.ChatFeature.ShowOnlineStatusInChat;
        var showRpOnly = Shared.Configuration.ChatFeature.ShowRoleplayTagInChat;
        var isRp = onlineStatus.Enum == OnlineStatusEnum.RolePlaying;

        if (!showOnline && !(showRpOnly && isRp))
            return;
        
        if (GetOnlineStatusIcon(onlineStatus) is not {  } icon) return;

        var senderNameIndex = FindSenderNameIndex(chatMessage);
        var iconPayload = new IconPayload(icon);
        var paddingPayload = new TextPayload(" ");
        
        chatMessage.Sender.Payloads.Insert(senderNameIndex, paddingPayload);
        chatMessage.Sender.Payloads.Insert(senderNameIndex, iconPayload);
    }

    private unsafe void ApplyTargetIndicator(IHandleableChatMessage chatMessage)
    {
        // TODO Add target indicator back
        /*
         * var character = (Character*)playerCharacter.Address;
                var targetId = character->GetTargetId();
                var lp = _objectTable.LocalPlayer;
         */
        if (!Shared.Configuration.ChatFeature.ShowTargetIndicator)
            return;
        
        var playerInfo = GetPlayerInfo(chatMessage);
        if (playerInfo?.PlayerCharacter is not {  } senderPlayerCharacter || _objectTable.LocalPlayer is not { } localPlayer)
            return;

        var senderCharacter = (Character*)senderPlayerCharacter.Address;
        var senderTargetId = senderCharacter->GetTargetId();
        var localCharacter = (Character*)localPlayer.Address;
        var localTargetId = localCharacter->GetTargetId();

        // I will get so confused on this later on, so future me, enjoy
        // V if the sender targets the local player
        var isSelfTarget = senderTargetId == localPlayer.GameObjectId;
        // V if the local player targets the sender
        var isOtherTarget = localTargetId == senderPlayerCharacter.GameObjectId;
        
        /* I am honestly unsure where to put this.
         * At first, it was at the start of the sender payloads, like so:
         * [Target][Status][Original]
         * As of v0.3, base format has changed to
         * [Prefix][Status][Name][World]
         * where Prefix and World is what comes before and after the name, my new idea would be like
         * [Prefix][Status][Name][World][Target]
         * but I am not sure if it'd look so epic if it comes *after* the world suffix, or if I should put it before?
         */
        
        if (isSelfTarget)
        {
            var selfTargetIconPayload = new IconPayload(BitmapFontIcon.ArrowDown);
            chatMessage.Sender.Payloads.Add(selfTargetIconPayload);
        }
        
        if (isOtherTarget)
        {
            var otherTargetIconPayload = new IconPayload(BitmapFontIcon.ArrowUp);
            chatMessage.Sender.Payloads.Add(otherTargetIconPayload);
        }
    }

    private int FindSenderNameIndex(IHandleableChatMessage chatMessage)
    {
        var payloads = chatMessage.Sender.Payloads;
        var playerPayload = payloads.OfType<PlayerPayload>().FirstOrDefault();

        if (playerPayload is null) return payloads.FindLastIndex(p => p is TextPayload);
        
        var name = playerPayload.PlayerName;

        var index = payloads.FindIndex(p => p is TextPayload text && text.Text == name);

        return index >= 0 ? index : payloads.FindLastIndex(p => p is TextPayload);
    }

    private void ApplyRoleplayMarkup(IHandleableChatMessage chatMessage)
    {
        if (!IsEnabled || !Shared.Configuration.ChatFeature.IsChatTypeEnabled(chatMessage.LogKind))
            return;

        var macroMessage = NativeStringConverter.SeStringToMacroCode(chatMessage.Message);

        var trimmedMessage = macroMessage.Trim();
        var startsWithPipe = trimmedMessage.StartsWith("||") || trimmedMessage.StartsWith('|');

        if (startsWithPipe)
        {
            macroMessage = trimmedMessage.StartsWith("||")
                               ? trimmedMessage[2..].TrimStart()
                               : trimmedMessage[1..].TrimStart();
        }

        var isRoleplaying = false;

        var playerPayload = chatMessage.Sender.Payloads.OfType<PlayerPayload>().FirstOrDefault();
        if (playerPayload is null)
        {
            var lp = _objectTable.LocalPlayer;
            if (lp != null)
                playerPayload = new PlayerPayload(lp.Name.TextValue, lp.HomeWorld.RowId);
        }

        if (playerPayload is not null)
            if (PlayerManager.GetPlayerCharacterFromPayload(playerPayload) is { } playerCharacter)
            {
                isRoleplaying = playerCharacter.OnlineStatus.RowId == 22;
            }

        if (Shared.Configuration.ChatFeature.RequiresRoleplayingTag && !isRoleplaying)
            return;

        var couldBeEmoteChat = chatMessage.LogKind == XivChatType.Say || startsWithPipe;
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

        var text = textOnly.ToString().TrimStart();

        var hasRpPatterns = text.Contains('"') || text.Contains('*') ||
                            text.Contains('(') || text.Contains('[') ||
                            text.Contains("(d)") || text.Contains("(c)");

        if (!(hasRpPatterns || treatAsEmoteChat || startsWithPipe))
            return;

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

        chatMessage.Message = ChatParser.RebuildFromTokens(formattedTokens);
    }

    private BitmapFontIcon? GetOnlineStatusIcon(OnlineStatus onlineStatus)
    {
        return onlineStatus.Enum switch
        {
            OnlineStatusEnum.EventParticipant       => BitmapFontIcon.Meteor,
            OnlineStatusEnum.BattleMentor           => BitmapFontIcon.MentorPvE,
            OnlineStatusEnum.Busy                   => BitmapFontIcon.DoNotDisturb,
            OnlineStatusEnum.ViewingCutscene        => BitmapFontIcon.WatchingCutscene,
            OnlineStatusEnum.AwayFromKeyboard       => BitmapFontIcon.Away,
            OnlineStatusEnum.CameraMode             => BitmapFontIcon.CameraMode,
            OnlineStatusEnum.RolePlaying            => BitmapFontIcon.RolePlaying,
            OnlineStatusEnum.LookingForParty        => BitmapFontIcon.LookingForParty,
            OnlineStatusEnum.WaitingForDutyFinder   => BitmapFontIcon.WaitingForDutyFinder,
            OnlineStatusEnum.RecruitingPartyMembers => BitmapFontIcon.GroupFinder,
            OnlineStatusEnum.Mentor                 => BitmapFontIcon.Mentor,
            OnlineStatusEnum.PvEMentor              => BitmapFontIcon.MentorPvE,
            OnlineStatusEnum.TradeMentor            => BitmapFontIcon.MentorCrafting,
            OnlineStatusEnum.PvPMentor              => BitmapFontIcon.MentorPvP,
            OnlineStatusEnum.Returner               => BitmapFontIcon.Returner,
            OnlineStatusEnum.NewAdventurer          => BitmapFontIcon.NewAdventurer,
            OnlineStatusEnum.PartyLeader            => BitmapFontIcon.PartyLeader,
            OnlineStatusEnum.PartyMember            => BitmapFontIcon.PartyMember,
            OnlineStatusEnum.CrossWorldPartyLeader  => BitmapFontIcon.CrossWorldPartyLeader,
            OnlineStatusEnum.CrossWorldPartyMember  => BitmapFontIcon.CrossWorldPartyMember,

            _ => null
        };
    }

    private PlayerInfo? GetPlayerInfo(IHandleableChatMessage chatMessage)
    {
        var playerPayload = chatMessage.Sender.Payloads
                                       .OfType<PlayerPayload>()
                                       .FirstOrDefault();
        if (playerPayload is null)
        {
            var textPayload = chatMessage.Sender.Payloads
                                         .OfType<TextPayload>()
                                         .Reverse()
                                         .FirstOrDefault();
            if (textPayload is null)
                return null;

            var localPlayer = _objectTable.LocalPlayer;
            if (localPlayer is null)
                return null;

            return string.Equals(textPayload.Text, localPlayer.Name.TextValue)
                       ? new PlayerInfo(
                           characterName: localPlayer.Name.TextValue,
                           worldId: localPlayer.HomeWorld.RowId,
                           playerCharacter: localPlayer,
                           onlineStatus: localPlayer.OnlineStatus
                       )
                       : null;
        }

        if (PlayerManager.GetPlayerInfoFromPayload(playerPayload) is { } playerInfo)
            return playerInfo;

        return null;
    }
}