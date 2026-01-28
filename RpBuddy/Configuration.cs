using Dalamud.Configuration;
using Dalamud.Game.Text;
using System;
using System.Collections.Generic;

namespace RpBuddy;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 1;

    // General
    public bool RequiresRoleplayingTag { get; set; } = true;
    public bool TreatSayAsEmote { get; set; } = true;
    public bool TreatSayAsEmoteForEveryone { get; set; } = false;
    public bool ShowRoleplayTagInChat { get; set; } = true;

    // Chat Types
    public HashSet<int> EnabledChatTypes { get; set; } = GetDefaultChatTypes();

    public static HashSet<int> GetDefaultChatTypes()
    {
        return new HashSet<int>
        {
            (int)XivChatType.Say,
            (int)XivChatType.Yell,
            (int)XivChatType.CustomEmote,
            (int)XivChatType.Party,
            (int)XivChatType.CrossParty,
            (int)XivChatType.TellIncoming,
            (int)XivChatType.TellOutgoing,
            (int)XivChatType.Echo
        };
    }

    // Risky
    public bool AlwaysAssumeLocalPlayer {  get; set; } = false;

    public bool IsChatTypeEnabled(XivChatType chatType)
    {
        return EnabledChatTypes.Contains((int)chatType);
    }

    public void SetChatTypeEnabled(XivChatType chatType, bool enabled)
    {
        if (enabled)
        {
            EnabledChatTypes.Add((int)chatType);
        }
        else
        {
            EnabledChatTypes.Remove((int)chatType);
        }
    }

    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
