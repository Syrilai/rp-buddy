using Dalamud.Game.Text.SeStringHandling;
using Lumina.Text.ReadOnly;
using Dalamud.Utility;

namespace RpBuddy.Utils
{
    internal class NativeStringConverter
    {
        public static string SeStringToMacroCode(SeString message)
        {
            var payloadBytes = message.Encode();

            var seString = new ReadOnlySeStringSpan(payloadBytes);

            return seString.ToMacroString();
        }

        public static SeString MacroCodeToSeString(string macroCode)
        {
            var seStringBuilder = new Lumina.Text.SeStringBuilder()
                .AppendMacroString(macroCode)
                .ToReadOnlySeString();

            return seStringBuilder.ToDalamudString();
        }
    }
}
