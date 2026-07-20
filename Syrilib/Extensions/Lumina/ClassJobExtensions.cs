// ReSharper disable MemberCanBeMadeStatic.Local
// ReSharper disable MemberCanBeMadeStatic.Global
// ReSharper disable MemberCanBePrivate.Global

using System.Collections.Generic;
using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;
using Syrilib.Extensions.Dalamud;

namespace Syrilib.Extensions.Lumina;

public static class ClassJobExtensions
{
    extension(ClassJob job)
    {
        public bool IsTank => job.JobType is 1;
        public bool IsDps => job.JobType is 3 or 4 or 5;
        public bool IsHealer => job.JobType is 2 or 6;

        public bool IsDoW => ClassJobCategory.GetRowRef(30).Value.ContainsJob(job);
        public bool IsDoM => ClassJobCategory.GetRowRef(31).Value.ContainsJob(job);
        public bool IsDoL => ClassJobCategory.GetRowRef(32).Value.ContainsJob(job);
        public bool IsDoH => ClassJobCategory.GetRowRef(33).Value.ContainsJob(job);
        public bool IsCombat => ClassJobCategory.GetRowRef(34).Value.ContainsJob(job);
        public bool IsTrade => ClassJobCategory.GetRowRef(35).Value.ContainsJob(job);
        public bool IsMelee => job.JobType is 3;
        public bool IsRanged => job.Role is 3;
        public bool IsPhysicalRanged => job.JobType is 4;
        public bool IsMagicalRanged => job.JobType is 5;
        public bool IsPureHealer => job.JobType is 2;
        public bool IsShieldHealer => job.JobType is 6;

        public short GetLevel() => IPlayerState.Get().GetClassJobLevel(job);

        public IReadOnlyList<Action> GetActions() =>
            Action.Where(action => action.ClassJobCategory.ValueNullable?.ContainsJob(job) ?? false);
    }
}