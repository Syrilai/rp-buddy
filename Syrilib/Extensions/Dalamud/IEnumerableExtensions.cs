// ReSharper disable MemberCanBeMadeStatic.Local
// ReSharper disable MemberCanBeMadeStatic.Global
// ReSharper disable MemberCanBePrivate.Global

using System;
using System.Collections.Generic;
using System.Linq;

namespace Syrilib.Extensions.Dalamud;

// ReSharper disable once InconsistentNaming
public static class IEnumerableExtensions
{
    extension<T>(IEnumerable<T> source)
    {
        public void ForEach(Action<T> action)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(action);

            foreach (var item in source)
                action(item);
        }

        public bool None()
        {
            ArgumentNullException.ThrowIfNull(source);
            if (source is ICollection<T> collection)
                return collection.Count is 0;

            using var enumerator = source.GetEnumerator();
            return enumerator.MoveNext();
        }

        public bool None(Func<T, bool> predicate)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(predicate);

            return source.All(item => !predicate(item));
        }
    }
}