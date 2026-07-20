// ReSharper disable MemberCanBeMadeStatic.Local
// ReSharper disable MemberCanBeMadeStatic.Global
// ReSharper disable MemberCanBePrivate.Global

using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game;
using Dalamud.Plugin.Services;
using Lumina.Data;
using Lumina.Excel;
using Lumina.Extensions;
using Syrilib.Extensions.Dalamud;

namespace Syrilib.Extensions.Lumina;

// ReSharper disable once InconsistentNaming
public static class IExcelRowExtensions
{
    extension<T>(IExcelRow<T> excelRow) where T : struct, IExcelRow<T>
    {
        public T WithLanguage(ClientLanguage language)
            => IDataManager.Get().GetExcelSheet<T>(language: language).GetRow(excelRow.RowId);

        public T WithLanguage(Language language)
            => IDataManager.Get().GetExcelSheet<T>(language: (ClientLanguage)language).GetRow(excelRow.RowId);

        public static IEnumerable<T> Rows => IDataManager.Get().GetExcelSheet<T>();

        public static RowRef<T> GetRowRef(uint id, Language? language = null)
            => new(IDataManager.Get().Excel, id, language);

        public static T GetRow(uint id, ClientLanguage? language = null)
            => IDataManager.Get().GetExcelSheet<T>(language: language).GetRow(id);

        public static bool TryGetRow(uint id, out T row, ClientLanguage? language = null) {
            if (IDataManager.Get().GetExcelSheet<T>(language: language).TryGetRow(id, out var r)) {
                row = r;
                return true;

            }
            else {
                row = default;
                return false;
            }
        }

        public static bool Any(Func<T, bool> predicate)
            => IDataManager.Get().GetExcelSheet<T>().Any(r => predicate(r));

        public static int Count(Func<T, bool> predicate)
            => IDataManager.Get().GetExcelSheet<T>().Count(r => predicate(r));

        public static bool All(Func<T, bool> predicate)
            => IDataManager.Get().GetExcelSheet<T>().All(r => predicate(r));

        public static T[] Where(Func<T, bool> predicate)
            => [.. IDataManager.Get().GetExcelSheet<T>().Where(r => predicate(r))];

        public static TResult[] Select<TResult>(Func<T, TResult> selector)
            => [.. IDataManager.Get().GetExcelSheet<T>().Select(selector)];

        public static T? FirstOrNull()
            => IDataManager.Get().GetExcelSheet<T>().FirstOrNull();

        public static T? FirstOrNull(Func<T, bool> predicate)
            => IDataManager.Get().GetExcelSheet<T>().Where(r => predicate(r)).FirstOrNull();
    }
}