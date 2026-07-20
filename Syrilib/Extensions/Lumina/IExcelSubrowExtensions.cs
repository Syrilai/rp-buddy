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
public static class IExcelSubrowExtensions
{
    extension<T>(IExcelSubrow<T> row) where T : struct, IExcelSubrow<T>
    {
        public T? WithLanguage(ushort subRowId, ClientLanguage language)
            => IDataManager.Get().GetSubrowSheet<T>(language: language).GetSubrowOrDefault(row.RowId, subRowId);

        public T? WithLanguage(ushort subRowId, Language language)
            => IDataManager.Get().GetSubrowSheet<T>(language: (ClientLanguage)language).GetSubrowOrDefault(row.RowId, subRowId);

        public static IEnumerable<T> Rows => IDataManager.Get().GetSubrowSheet<T>().SelectMany(r => r);

        public static SubrowRef<T> GetSubrowRef(uint rowId, Language? language = null)
            => new(IDataManager.Get().Excel, rowId, language);

        public static T? GetSubrow(uint rowId, ushort subRowId, ClientLanguage? language = null)
            => IDataManager.Get().GetSubrowSheet<T>(language: language).GetSubrowOrDefault(rowId, subRowId);

        public static bool TryGetSubrow(uint rowId, ushort subRowId, out T subrow, ClientLanguage? language = null) {
            if (IDataManager.Get().GetSubrowSheet<T>(language: language).TryGetSubrow(rowId, subRowId, out var r)) {
                subrow = r;
                return true;
            }
            else {
                subrow = default;
                return false;
            }
        }

        public static bool TryGetSubrows(uint rowId, out SubrowCollection<T> subrows) {
            if (IDataManager.Get().TryGetSubrows<T>(rowId, out var r)) {
                subrows = r;
                return true;
            }
            else {
                subrows = [];
                return false;
            }
        }

        public static bool Any(Func<T, bool> predicate)
            => EnumerateSubrows<T>().Any(predicate);

        public static int Count(Func<T, bool> predicate)
            => EnumerateSubrows<T>().Count(predicate);

        public static bool All(Func<T, bool> predicate)
            => EnumerateSubrows<T>().All(predicate);

        public static T[] Where(Func<T, bool> predicate)
            => [.. EnumerateSubrows<T>().Where(predicate)];

        public static TResult[] Select<TResult>(Func<T, TResult> selector)
            => [.. EnumerateSubrows<T>().Select(selector)];

        public static T? FirstOrNull()
            => EnumerateSubrows<T>().FirstOrNull();

        public static T? FirstOrNull(Func<T, bool> predicate)
            => EnumerateSubrows<T>().Where(predicate).FirstOrNull();
    }
    
    private static IEnumerable<T> EnumerateSubrows<T>(ClientLanguage? language = null) where T : struct, IExcelSubrow<T>
        => IDataManager.Get().GetSubrowSheet<T>(language: language).SelectMany(r => r);
}