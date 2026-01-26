using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Lumina.Text;
using RpBuddy.Utils;

namespace RpBuddy.Windows;

public class MainWindow : Window, IDisposable
{
    private readonly Plugin plugin;

    public MainWindow(Plugin plugin)
        : base("RP Buddy Introduction##introduction", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        this.plugin = plugin;
    }

    public void Dispose() { }

    private static readonly string RpIcon = new SeStringBuilder()
        .Append("<")
        .AppendIcon(127)
        .Append(" John Fantasy> Epic.")
        .ToReadOnlySeString()
        .ToMacroString();
    private static readonly string TreatSayAsEmoteChat = new SeStringBuilder()
        .BeginMacro(Lumina.Text.Payloads.MacroCode.Color)
            .AppendGlobalNumberExpression((int)GlobalExpressions.ColorSay)
        .EndMacro()
        .Append("<John Fantasy> \"Those are some nice flowers you have there..\"")
        .PopColor()
        .BeginMacro(Lumina.Text.Payloads.MacroCode.Color)
            .AppendGlobalNumberExpression((int)GlobalExpressions.ColorEmoteUser)
        .EndMacro()
        .Append(", as he glances a table over.")
        .PopColor()
        .ToReadOnlySeString()
        .ToMacroString();
    private static readonly string ColoredMessages = new SeStringBuilder()
        .BeginMacro(Lumina.Text.Payloads.MacroCode.Color)
            .AppendGlobalNumberExpression((int)GlobalExpressions.ColorEmoteUser)
        .EndMacro()
        .Append("John Fantasy glances out of the window, looking at the sunset. ")
        .PopColor()
        .BeginMacro(Lumina.Text.Payloads.MacroCode.Color)
            .AppendGlobalNumberExpression((int)GlobalExpressions.ColorSay)
        .EndMacro()
        .Append("\"Another ")
        .AppendItalicized("peaceful")
        .Append(" day coming to an end...\"")
        .PopColor()
        .BeginMacro(Lumina.Text.Payloads.MacroCode.Color)
            .AppendGlobalNumberExpression((int)GlobalExpressions.ColorEmoteUser)
        .EndMacro()
        .Append(", he says and breathes out. ")
        .PopColor()
        .BeginMacro(Lumina.Text.Payloads.MacroCode.Color)
            .AppendGlobalNumberExpression((int)GlobalExpressions.ColorTell)
        .EndMacro()
        .Append("((It is late, it might be time I have to end here for now))")
        .PopColor()
        .ToReadOnlySeString()
        .ToMacroString();
    private static readonly string ColoredMessagesExplanation = new SeStringBuilder()
        .Append("Here is what counts as what:\n")
        .Append("Speech  \"Text\", \"Text with _")
        .AppendItalicized("emphasis")
        .Append("_ and *")
        .BeginMacro(Lumina.Text.Payloads.MacroCode.EdgeColor)
            .AppendGlobalNumberExpression((int)GlobalExpressions.ColorSay)
        .EndMacro()
        .Append("bold")
        .PopEdgeColor()
        .Append("*\"\nAction  ")
        .BeginMacro(Lumina.Text.Payloads.MacroCode.Color)
            .AppendGlobalNumberExpression((int)GlobalExpressions.ColorEmoteUser)
        .EndMacro()
        .Append("*Text*")
        .PopColor()
        .Append("\nOOC  ")
        .BeginMacro(Lumina.Text.Payloads.MacroCode.Color)
            .AppendGlobalNumberExpression((int)GlobalExpressions.ColorTell)
        .EndMacro()
        .Append("[Text], [[Text]], (Text), ((Text))")
        .PopColor()
        .ToReadOnlySeString()
        .ToMacroString();

    private static readonly string MarkersContinued = new SeStringBuilder()
        .Append("Continued\n (c)  ")
        .BeginMacro(Lumina.Text.Payloads.MacroCode.Color)
            .AppendGlobalNumberExpression((int)GlobalExpressions.ColorEcho)
        .EndMacro()
        .Append("(c) ")
        .PopColor()
        .BeginMacro(Lumina.Text.Payloads.MacroCode.Color)
            .AppendIntExpression(0xFD6918)
        .EndMacro()
        .Append("")
        .PopColor()
        .Append("\n (1/5)  ")
        .BeginMacro(Lumina.Text.Payloads.MacroCode.Color)
            .AppendGlobalNumberExpression((int)GlobalExpressions.ColorEcho)
        .EndMacro()
        .Append("(1/5) ")
        .PopColor()
        .BeginMacro(Lumina.Text.Payloads.MacroCode.Color)
            .AppendIntExpression(0xFD6918)
        .EndMacro()
        .Append("")
        .PopColor()
        .ToReadOnlySeString()
        .ToMacroString();

    private static readonly string MarkersDone = new SeStringBuilder()
        .Append("Done\n (d)  ")
        .BeginMacro(Lumina.Text.Payloads.MacroCode.Color)
            .AppendGlobalNumberExpression((int)GlobalExpressions.ColorEcho)
        .EndMacro()
        .Append("(d) ")
        .PopColor()
        .BeginMacro(Lumina.Text.Payloads.MacroCode.Color)
            .AppendIntExpression(0x18FD23)
        .EndMacro()
        .Append("✓")
        .PopColor()
        .Append("\n (5/5)  ")
        .BeginMacro(Lumina.Text.Payloads.MacroCode.Color)
            .AppendGlobalNumberExpression((int)GlobalExpressions.ColorEcho)
        .EndMacro()
        .Append("(5/5) ")
        .PopColor()
        .BeginMacro(Lumina.Text.Payloads.MacroCode.Color)
            .AppendIntExpression(0x18FD23)
        .EndMacro()
        .Append("✓")
        .PopColor()
        .ToReadOnlySeString()
        .ToMacroString();

    private static readonly string PipePreview = new SeStringBuilder()
        .Append("<John Fantasy> | As the snow starts falling more violently, the trees turn whiter and whiter.\n\n<John Fantasy> ")
        .AppendIcon(106)
        .Append("\n")
        .BeginMacro(Lumina.Text.Payloads.MacroCode.Color)
            .AppendGlobalNumberExpression((int)GlobalExpressions.ColorEmoteUser)
        .EndMacro()
        .Append("As the snow starts falling more violently, the trees turn whiter and whiter.")
        .PopColor()
        .ToReadOnlySeString()
        .ToMacroString();

    public override void Draw()
    {
        if (ImGui.Button("Show Settings"))
        {
            plugin.ToggleConfigUi();
        }

        ImGui.Spacing();

        using (var child = ImRaii.Child("FeatureList", Vector2.Zero, true))
        {
            if (child.Success)
            {
                ImGui.Text("Here are the features that RP Buddy supports:");

                if (ImGui.CollapsingHeader("Add an RP Icon in front of Player Names", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    ImGuiHelpers.CompileSeStringWrapped(RpIcon);
                }

                if (ImGui.CollapsingHeader("Treat Say as an Emote Chat", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    ImGuiHelpers.CompileSeStringWrapped(TreatSayAsEmoteChat);
                }

                if (ImGui.CollapsingHeader("Color parts of messages differently based on context", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    ImGuiHelpers.CompileSeStringWrapped(ColoredMessages);
                    ImGui.Separator();
                    ImGuiHelpers.CompileSeStringWrapped(ColoredMessagesExplanation);
                }

                if (ImGui.CollapsingHeader("Improved Indicators for continued/done markers", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    ImGuiHelpers.CompileSeStringWrapped(MarkersContinued);
                    ImGui.Separator();
                    ImGuiHelpers.CompileSeStringWrapped(MarkersDone);
                }

                if (ImGui.CollapsingHeader("Vertical Lines for in-depth RP focus", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    ImGuiHelpers.CompileSeStringWrapped(PipePreview);
                }
            }
        }
    }
}
