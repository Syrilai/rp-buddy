// ReSharper disable MemberCanBeMadeStatic.Local
// ReSharper disable MemberCanBeMadeStatic.Global
// ReSharper disable MemberCanBePrivate.Global

using System;
using System.Linq;
using FFXIVClientStructs.FFXIV.Client.System.Input;
using FFXIVClientStructs.FFXIV.Client.UI;
using Lumina.Excel.Sheets;

namespace Syrilib.Extensions.Lumina;

public static class ConfigKeyExtensions
{
    extension(ConfigKey configKey)
    {
        public bool TryGetInputId(out InputId inputId)
        {
            inputId = Enum.GetValues<InputId>().FirstOrDefault(i => Enum.GetName(i) == configKey.Label.ToString(), InputId.NotFound);
            return inputId != InputId.NotFound;
        }

        public unsafe bool IsDown() => configKey.TryGetInputId(out var inputId) && UIInputData.Instance()->IsInputIdDown(inputId);
        public unsafe bool IsHeld() => configKey.TryGetInputId(out var inputId) && UIInputData.Instance()->IsInputIdHeld(inputId);
        public unsafe bool IsPressed() => configKey.TryGetInputId(out var inputId) && UIInputData.Instance()->IsInputIdPressed(inputId);
        public unsafe bool IsReleased() => configKey.TryGetInputId(out var inputId) && UIInputData.Instance()->IsInputIdReleased(inputId);
    }
}