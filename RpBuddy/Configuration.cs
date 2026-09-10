using Dalamud.Configuration;
using Dalamud.Game.Text;
using System;
using System.Collections.Generic;
using RpBuddy.Inventory;

namespace RpBuddy;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 1;

    public ChatFeatureConfiguration     ChatFeature { get; set; } = new();
    public Dictionary<Guid, CustomItem> LocalItems { get; set; }  = [];
    public InventoryItem?[] LocalInventoryItems { get; set; } = new InventoryItem?[InventoryBase.Rows * InventoryBase.Columns];

    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}

[Serializable]
public class ChatFeatureConfiguration
{
    public bool         RequiresRoleplayingTag     { get; set; } = true;
    public bool         TreatSayAsEmote            { get; set; } = true;
    public bool         TreatSayAsEmoteForEveryone { get; set; }
    public bool         ShowRoleplayTagInChat      { get; set; } = true;
    public bool         ShowOnlineStatusInChat     { get; set; } = false;
    public bool         ShowTargetIndicator        { get; set; } = false;
    public HashSet<int> EnabledChatTypes           { get; set; } = GetDefaultChatTypes();
    
    public static HashSet<int> GetDefaultChatTypes()
    {
        return
        [
            (int)XivChatType.Say,
            (int)XivChatType.Yell,
            (int)XivChatType.CustomEmote,
            (int)XivChatType.Party,
            (int)XivChatType.CrossParty,
            (int)XivChatType.TellIncoming,
            (int)XivChatType.TellOutgoing,
            (int)XivChatType.Echo
        ];
    }
    
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
}