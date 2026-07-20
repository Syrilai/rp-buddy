// ReSharper disable MemberCanBeMadeStatic.Local
// ReSharper disable MemberCanBeMadeStatic.Global
// ReSharper disable MemberCanBePrivate.Global

using System.Linq;
using Lumina.Excel.Sheets;

namespace Syrilib.Extensions.Lumina;

public static class ClassJobCategoryExtensions
{
    extension(ClassJobCategory classJobCategory)
    {
        public bool HasJobsAtLevel(int level)
            => ClassJob.Where(classJob => classJobCategory.ContainsJob(classJob))
                .All(classJob => classJob.GetLevel() >= level);

        public bool HasAnyJobAtLevel(int level)
            => ClassJob.Where(classJob => classJobCategory.ContainsJob(classJob))
                .Any(classJob => classJob.GetLevel() >= level);

        public bool ContainsJob(ClassJob classJob)
            => classJobCategory.ExcelPage.ReadBool(classJobCategory.RowOffset + classJob.RowId + 4);
    }
}