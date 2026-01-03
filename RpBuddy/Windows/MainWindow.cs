using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;

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

    public override void Draw()
    {
        if (ImGui.Button("Show Settings"))
        {
            plugin.ToggleConfigUi();
        }

        ImGui.Spacing();

        using (var child = ImRaii.Child("IntroductionText", Vector2.Zero, true))
        {
            if (child.Success)
            {
                var playerState = Plugin.PlayerState;
                if (!playerState.IsLoaded)
                {
                    ImGui.Text("Our local player is currently not logged in.");
                    return;
                }
                
                if (!playerState.ClassJob.IsValid)
                {
                    ImGui.Text("Our current job is currently not valid.");
                    return;
                }

                ImGuiHelpers.CompileSeStringWrapped("Here is what RP Buddy currently offers:\n\n" +
                    "• Adds an RP Icon in front of Player Names\n" +
                    "\\<<icon(127)>John Fantasy\\> Epic.\n\n" +
                    "• Treat Say as an Emote Chat\n" +
                    "<color(0xFFFFFF)>\\<John Fantasy\\> \"Those are some nice flowers you have there\", as he glances a table over.<color(stackcolor)>\n" +
                    "\n" +
                    "<color(0xFFFFFF)>\\<John Fantasy\\> \"Those are some nice flowers you have there\"<color(0xFF94C0FF)>, as he glances a table over.<color(stackcolor)>\n\n" +
                    "• Color parts of messages differently based on what it is (Actual colors are based on your log colors)\n" +
                    "<color(0xFF94C0FF)>John Fantasy glances out of the window, looking at the sunset. \"Another _peaceful_ day coming to an end...\", he says and breathes out. ((It is late now, it might be time I have to end here for now))<color(stackcolor)>\n" +
                    "\n" +
                    "<color(0xFF94C0FF)><icon(127)>John Fantasy glances out of the window, looking at the sunset. <color(stackcolor)><color(0xFFFFFF)>\"Another <italic(1)>peaceful<italic(0)> day coming to an end...\"<color(stackcolor)><color(0xFF94C0FF)>, he says and breathes out. <color(stackcolor)><color(0xCCCCCC)>((It is late now, it might be time I have to end here for now))<color(stackcolor)>\n" +
                    "Here is what counts as what:\n" +
                    " Speech  \"Text\", \"Text with <italic(1)>_emphasis_<italic(0)>\"\n" +
                    " Action  <color(0xFF94C0FF)>\\<Text\\><color(stackcolor)>, <color(0xFF94C0FF)>*Text*<color(stackcolor)>\n" +
                    " OOC  <color(0xCCCCCC)>[Text]<color(stackcolor)>, <color(0xCCCCCC)>[[Text]]<color(stackcolor)>, <color(0xCCCCCC)>(Text)<color(stackcolor)>, <color(0xCCCCCC)>((Text))<color(stackcolor)>\n\n" +
                    "• \"Improved\" Indicators for continued/done markers\n" +
                    " Continued\n" +
                    "  (c)  <color(0xCCCCCC)>(c)<color(stackcolor)> <color(0xFD6918)><color(stackcolor)>\n" +
                    "  (1/5)  <color(0xCCCCCC)>(1/5)<color(stackcolor)> <color(0xFD6918)><color(stackcolor)>\n" +
                    " Done\n" +
                    "  (d)  <color(0xCCCCCC)>(d)<color(stackcolor)> <color(0x18FD23)>✓<color(stackcolor)>\n" +
                    "  (5/5)  <color(0xCCCCCC)>(5/5)<color(stackcolor)> <color(0x18FD23)>✓<color(stackcolor)>\n\n" +
                    "• Vertical Lines for changes in the scenery or such\n" +
                    "<color(0xFFFFFF)>\\<John Fantasy\\> | As the snow starts falling more violently, the trees turn whiter and whiter.<color(stackcolor)>\n" +
                    "\n" +
                    "<color(0xFFFFFF)>\\<John Fantasy\\> <color(stackcolor)><icon(106)>\n" +
                    "<color(0xFF94C0FF)>  As the snow starts falling more violently, the trees turn whiter and whiter.<color(stackcolor)>");
            }
        }
    }
}
