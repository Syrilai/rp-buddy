// ReSharper disable MemberCanBeMadeStatic.Local
// ReSharper disable MemberCanBeMadeStatic.Global
// ReSharper disable MemberCanBePrivate.Global

using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace Syrilib.Extensions.Lumina;

public static unsafe class EquipRaceCategoryExtensions
{
    extension(EquipRaceCategory row)
    {
        public Collection<bool> Races
            => new(row.ExcelPage, row.RowOffset, row.RowOffset, &RaceCtor, row.ExcelPage.Module.GetSheet<Race>().Count);

        public Collection<bool> Sexes
            => new(row.ExcelPage, row.RowOffset, row.RowOffset + (uint)row.ExcelPage.Module.GetSheet<Race>().Count,
                &SexCtor, 2);

        public bool CanEquip => row.Races[PlayerState.Instance()->Race - 1] && row.Sexes[PlayerState.Instance()->Sex];
    }

    private static bool RaceCtor(ExcelPage page, uint parentOffset, uint offset, uint i)
        => page.ReadBool(offset + i);

    private static bool SexCtor(ExcelPage page, uint parentOffset, uint offset, uint i)
        => page.ReadPackedBool(offset, (byte)i);
}