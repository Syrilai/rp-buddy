// ReSharper disable MemberCanBeMadeStatic.Local
// ReSharper disable MemberCanBeMadeStatic.Global
// ReSharper disable MemberCanBePrivate.Global

using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel.Sheets;

namespace Syrilib.Extensions.Lumina;

public static unsafe class ActionExtensions
{
    extension(Action row)
    {
        public bool IsOnCooldown(ActionType type = ActionType.Action)
        {
            var group = row.GetRecastGroup();
            if (group is -1) return false;
            var recast = ActionManager.Instance()->GetRecastGroupDetail(group);
            return recast->Total - recast->Elapsed > 0;
        }

        public bool IsAvailable(ActionType type = ActionType.Action)
            => row.GetActionStatus(type) == 0 && !row.IsOnCooldown(type);

        public int GetRecastGroup(ActionType type = ActionType.Action)
            => ActionManager.Instance()->GetRecastGroup((int)type, row.RowId);

        public uint GetActionStatus(ActionType type = ActionType.Action)
            => ActionManager.Instance()->GetActionStatus(type, row.RowId);
    }
}