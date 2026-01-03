using Dalamud.Configuration;
using System;

namespace RpBuddy;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public bool RequiresRoleplayingTag { get; set; } = true;
    public bool TreatSayAsEmote { get; set; } = true;
    public bool TreatSayAsEmoteForEveryone { get; set; } = false;
    public bool ShowRoleplayTagInChat { get; set; } = true;

    // The below exists just to make saving less cumbersome
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
