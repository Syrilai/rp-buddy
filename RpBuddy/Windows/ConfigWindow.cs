using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.Text;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using Lumina.Text;

namespace RpBuddy.Windows;

public class ConfigWindow : Window, IDisposable
{
    private readonly Configuration configuration;

    private static readonly Dictionary<string, List<(XivChatType ChatType, string Name, int ColorIndex)>> ChatTypeCategories = new()
    {
        ["Public Chat"] =
        [
            (XivChatType.Say, "Say", 13),
            (XivChatType.Yell, "Yell", 32),
            (XivChatType.Shout, "Shout", 14),
            (XivChatType.CustomEmote, "Custom Emote", 30)
        ],
        ["Party & Alliance"] =
        [
            (XivChatType.Party, "Party", 16),
            (XivChatType.CrossParty, "Cross-World Party", 16),
            (XivChatType.Alliance, "Alliance", 17)
        ],
        ["Free Company"] =
        [
            (XivChatType.FreeCompany, "Free Company", 26)
        ],
        ["Linkshells"] =
        [
            (XivChatType.Ls1, "Linkshell 1", 18),
            (XivChatType.Ls2, "Linkshell 2", 19),
            (XivChatType.Ls3, "Linkshell 3", 20),
            (XivChatType.Ls4, "Linkshell 4", 21),
            (XivChatType.Ls5, "Linkshell 5", 22),
            (XivChatType.Ls6, "Linkshell 6", 23),
            (XivChatType.Ls7, "Linkshell 7", 24),
            (XivChatType.Ls8, "Linkshell 8", 25)
        ],
        ["Cross-World Linkshells"] =
        [
            (XivChatType.CrossLinkShell1, "CWLS 1", 35),
            (XivChatType.CrossLinkShell2, "CWLS 2", 84),
            (XivChatType.CrossLinkShell3, "CWLS 3", 85),
            (XivChatType.CrossLinkShell4, "CWLS 4", 86),
            (XivChatType.CrossLinkShell5, "CWLS 5", 87),
            (XivChatType.CrossLinkShell6, "CWLS 6", 88),
            (XivChatType.CrossLinkShell7, "CWLS 7", 89),
            (XivChatType.CrossLinkShell8, "CWLS 8", 90),
        ],
        ["Direct Messages"] =
        [
            (XivChatType.TellIncoming, "Incoming Tells", 15),
            (XivChatType.TellOutgoing, "Outgoing Tells", 15),
        ],
        ["Other"] =
        [
            (XivChatType.Echo, "Echo (Testing)", 43),
        ]
    };

    public ConfigWindow(Plugin plugin) : base("RP Buddy Configuration###config")
    {
        Flags = ImGuiWindowFlags.NoCollapse;

        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(500, 450),
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
        if (ImGui.BeginTabBar("ConfigTabs"))
        {
            if (ImGui.BeginTabItem("General"))
            {
                DrawGeneralSettings();
                ImGui.EndTabItem();
            }
            if (ImGui.BeginTabItem("Chat Channels"))
            {
                DrawChatChannelSettings();
                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }
    }

    private void DrawGeneralSettings()
    {
        var requiresRoleplayingTag = configuration.RequiresRoleplayingTag;
        if (ImGui.Checkbox("Requires Roleplaying Tag", ref requiresRoleplayingTag))
        {
            configuration.RequiresRoleplayingTag = requiresRoleplayingTag;
            configuration.Save();
        }
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("Only process messages from players with the Roleplaying status");
        }

        var treatSayAsEmote = configuration.TreatSayAsEmote;
        if (ImGui.Checkbox("Treat Say as Emote", ref treatSayAsEmote))
        {
            configuration.TreatSayAsEmote = treatSayAsEmote;
            configuration.Save();
        }
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("Apply emoter color to Say chat messages");
        }

        var treatSayAsEmoteForEveryone = configuration.TreatSayAsEmoteForEveryone;
        if (requiresRoleplayingTag | !treatSayAsEmote)
        {
            ImGui.BeginDisabled();
            treatSayAsEmoteForEveryone = false;
        }
        if (ImGui.Checkbox("Treat Say as Emote for everyone", ref treatSayAsEmoteForEveryone))
        {
            configuration.TreatSayAsEmoteForEveryone = treatSayAsEmoteForEveryone;
            configuration.Save();
        }
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("Apply emote color regardless of RP status");
        }
        if (requiresRoleplayingTag | !treatSayAsEmote)
        {
            ImGui.EndDisabled();
            ImGui.BeginGroup();
            ImGui.TextColored(new Vector4(1, 0.5f, 0, 1), "This option has been disabled due to:");
            if (requiresRoleplayingTag)
            {
                ImGui.BulletText("Requires Roleplaying Tag");
            }
            if (!treatSayAsEmote)
            {
                ImGui.BulletText("Treat Say as Emote");
            }
            ImGui.EndGroup();
        }

        ImGui.Separator();

        var showRoleplayTagInChat = configuration.ShowRoleplayTagInChat;
        if (ImGui.Checkbox("Show Roleplay Tag in Chat", ref showRoleplayTagInChat))
        {
            configuration.ShowRoleplayTagInChat = showRoleplayTagInChat;
            configuration.Save();
        }
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("Show the RP icon next to player names in chat");
        }
    }

    private void DrawChatChannelSettings()
    {
        ImGui.TextWrapped("Select which chat channels should have RP formatting applied:");
        ImGui.Separator();

        if (ImGui.BeginChild("ChatChannelList", new Vector2(0, -30), true))
        {
            foreach (var (category, chatTypes) in ChatTypeCategories)
            {
                if (ImGui.CollapsingHeader(category, ImGuiTreeNodeFlags.DefaultOpen))
                {
                    ImGui.Indent();

                    foreach (var (chatType, name, colorIndex) in chatTypes)
                    {
                        var isEnabled = configuration.IsChatTypeEnabled(chatType);
                        if (ImGui.Checkbox("", ref isEnabled))
                        {
                            configuration.SetChatTypeEnabled(chatType, isEnabled);
                            configuration.Save();
                        }
                        ImGui.SameLine();
                        var text = new SeStringBuilder()
                            .BeginMacro(Lumina.Text.Payloads.MacroCode.Color)
                                .AppendGlobalNumberExpression(colorIndex)
                            .EndMacro()
                            .Append(name)
                            .PopColor()
                            .ToReadOnlySeString()
                            .ToMacroString();
                        ImGuiHelpers.CompileSeStringWrapped(text);
                    }

                    ImGui.Unindent();
                    ImGui.Spacing();
                }
            }
            ImGui.EndChild();
        }

        ImGui.Separator();

        if (ImGui.Button("Enable All"))
        {
            foreach (var chatTypes in ChatTypeCategories.Values)
            {
                foreach (var (chatType, _, _) in chatTypes)
                {
                    configuration.SetChatTypeEnabled(chatType, true);
                }
            }
            configuration.Save();
        }

        ImGui.SameLine();

        if (ImGui.Button("Disable All"))
        {
            foreach (var chatTypes in ChatTypeCategories.Values)
            {
                foreach (var (chatType, _, _) in chatTypes)
                {
                    configuration.SetChatTypeEnabled(chatType, false);
                }
            }
            configuration.Save();
        }

        ImGui.SameLine();

        if (ImGui.Button("Reset to Defaults"))
        {
            configuration.EnabledChatTypes = Configuration.GetDefaultChatTypes();
            configuration.Save();
        }
    }
}
