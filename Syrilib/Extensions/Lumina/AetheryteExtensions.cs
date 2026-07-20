// ReSharper disable MemberCanBeMadeStatic.Local
// ReSharper disable MemberCanBeMadeStatic.Global
// ReSharper disable MemberCanBePrivate.Global

using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Excel.Sheets;

namespace Syrilib.Extensions.Lumina;

public static unsafe class AetheryteExtensions
{
    extension(Aetheryte row)
    {
        public bool IsUnlocked => UIState.Instance()->IsAetheryteUnlocked(row.RowId);
    }
}