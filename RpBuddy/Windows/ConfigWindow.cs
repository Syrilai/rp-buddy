using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;

namespace RpBuddy.Windows;

public class ConfigWindow : Window, IDisposable
{
    private readonly Configuration configuration;

    public ConfigWindow(Plugin plugin) : base("RP Buddy Configuration###config")
    {
        Flags = ImGuiWindowFlags.NoCollapse;

        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(300, 230),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        configuration = plugin.Configuration;
    }

    public void Dispose() { }

    public override void PreDraw()
    {
        Flags &= ~ImGuiWindowFlags.NoMove;
    }

    public override void Draw()
    {
        var requiresRoleplayingTag = configuration.RequiresRoleplayingTag;
        if (ImGui.Checkbox("Requires Roleplaying Tag", ref requiresRoleplayingTag))
        {
            configuration.RequiresRoleplayingTag = requiresRoleplayingTag;
            configuration.Save();
        }

        var treatSayAsEmote = configuration.TreatSayAsEmote;
        if (ImGui.Checkbox("Treat Say as Emote", ref treatSayAsEmote))
        {
            configuration.TreatSayAsEmote = treatSayAsEmote;
            configuration.Save();
        }

        var treatSayAsEmoteForEveryone = configuration.TreatSayAsEmoteForEveryone;
        if (requiresRoleplayingTag | !treatSayAsEmote)
        {
            ImGui.BeginDisabled();
            // We show it as false because the top settings overwrite this
            treatSayAsEmoteForEveryone = false;
        }
        if (ImGui.Checkbox("Treat Say as Emote for everyone", ref treatSayAsEmoteForEveryone))
        {
            configuration.TreatSayAsEmoteForEveryone = treatSayAsEmoteForEveryone;
            configuration.Save();
        }
        if (requiresRoleplayingTag | !treatSayAsEmote)
        {
            ImGui.EndDisabled();
            ImGui.BeginGroup();
            ImGui.Text("This option has been disabled due to:");
            if (requiresRoleplayingTag)
            {
                ImGui.BulletText("Requires Roleplaying Tag");
            }
            if (!treatSayAsEmote)
            {
                ImGui.BulletText("Treat Say as Emote");
            }
        }

        var showRoleplayTagInChat = configuration.ShowRoleplayTagInChat;
        if (ImGui.Checkbox("Show Roleplay Tag in Chat", ref showRoleplayTagInChat))
        {
            configuration.ShowRoleplayTagInChat = showRoleplayTagInChat;
            configuration.Save();
        }
    }
}
