// ReSharper disable MemberCanBeMadeStatic.Local
// ReSharper disable MemberCanBeMadeStatic.Global
// ReSharper disable MemberCanBePrivate.Global

using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace Syrilib.Extensions.Lumina;

public static unsafe class AnimaWeaponItemExtensions
{
    extension(AnimaWeaponItem row)
    {
        public unsafe Collection<RowRef<Item>> Items
            => new(row.ExcelPage, row.RowOffset, row.RowOffset, &ItemCtor, 14);
    }

    internal static RowRef<Item> ItemCtor(ExcelPage page, uint parentOffset, uint offset, uint i)
        => new(page.Module, page.ReadUInt32(offset + i * 4), page.Language);
}