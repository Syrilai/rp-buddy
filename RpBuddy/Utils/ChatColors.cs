using System;
using System.Collections.Generic;
using System.Text;

namespace RpBuddy.Utils
{
    public class ChatColors
    {

        public static uint ColorSay = 0xFFFFFFFF;
        public static uint ColorEmoteUser = 0xBCFFF0;
        public static uint ColorTell = 0xFFB8E0;
        public static uint ColorEcho = 0xCCCCCC;

        public ChatColors()
        {
            RefreshColors();
        }
        
        public void RefreshColors()
        {
            if (Plugin.GameConfig.UiConfig.TryGetUInt("ColorSay", out var colorSay))
            {
                ColorSay = colorSay;
            }
            if (Plugin.GameConfig.UiConfig.TryGetUInt("ColorEmoteUser", out var colorEmoteUser))
            {
                ColorEmoteUser = colorEmoteUser;
            }
            if (Plugin.GameConfig.UiConfig.TryGetUInt("ColorTell", out var colorTell))
            {
                ColorTell = colorTell;
            }
            if (Plugin.GameConfig.UiConfig.TryGetUInt("ColorEcho", out var colorEcho))
            {
                ColorEcho = colorEcho;
            }
        }
    }
}
