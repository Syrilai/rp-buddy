// ReSharper disable MemberCanBeMadeStatic.Local
// ReSharper disable MemberCanBeMadeStatic.Global
// ReSharper disable MemberCanBePrivate.Global

using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace Syrilib.Extensions.Dalamud;

// ReSharper disable once InconsistentNaming
public static class IConditionExtensions
{
    extension(ICondition condition)
    {
        public unsafe bool HasPermission(uint id) => Conditions.Instance()->HasPermission(id);
        public unsafe bool HasPermission(IEnumerable<uint> ids) =>
            ids.All(id => Conditions.Instance()->HasPermission(id));

        public bool CanQueue() => condition.HasPermission([119, 120]);
        
        public bool CanMoveItems() => condition.HasPermission(134);
        
        public bool CanLowerItemQuality() => condition.HasPermission(135);

        public bool IsBoundByDuty() => condition.Any(
            ConditionFlag.BoundByDuty,
            ConditionFlag.BoundByDuty56,
            ConditionFlag.BoundByDuty95
        );

        public bool IsInCombat() => condition.Any(ConditionFlag.InCombat);

        public bool IsInCutscene() => condition.Any(
            ConditionFlag.OccupiedInCutSceneEvent,
            ConditionFlag.WatchingCutscene,
            ConditionFlag.WatchingCutscene78
        );

        public bool IsBetweenAreas() => condition.Any(
            ConditionFlag.BetweenAreas,
            ConditionFlag.BetweenAreas51
        );

        public bool IsCrafting() => condition.Any(
            ConditionFlag.Crafting,
            ConditionFlag.ExecutingCraftingAction,
            ConditionFlag.PreparingToCraft
        );

        public bool IsGathering() => condition.Any(
            ConditionFlag.Gathering,
            ConditionFlag.ExecutingGatheringAction
        );

        public bool IsLockedIn() => condition.Any(
            ConditionFlag.Occupied,
            ConditionFlag.Occupied30,
            ConditionFlag.Occupied33,
            ConditionFlag.Occupied38,
            ConditionFlag.Occupied39,
            ConditionFlag.OccupiedInCutSceneEvent,
            ConditionFlag.OccupiedInEvent,
            ConditionFlag.OccupiedInQuestEvent,
            ConditionFlag.OccupiedSummoningBell,
            ConditionFlag.WatchingCutscene,
            ConditionFlag.WatchingCutscene78,
            ConditionFlag.BetweenAreas,
            ConditionFlag.BetweenAreas51,
            ConditionFlag.InThatPosition,
            ConditionFlag.PreparingToCraft,
            ConditionFlag.BeingMoved,
            ConditionFlag.RidingPillion,
            ConditionFlag.Fishing
        );

        public bool IsUnavailable() => condition.IsLockedIn() || condition.Any(
            ConditionFlag.TradeOpen,
            ConditionFlag.Crafting,
            ConditionFlag.ExecutingCraftingAction,
            ConditionFlag.Unconscious,
            ConditionFlag.MeldingMateria,
            ConditionFlag.Gathering,
            ConditionFlag.OperatingSiegeMachine,
            ConditionFlag.CarryingItem,
            ConditionFlag.CarryingObject,
            ConditionFlag.Mounting,
            ConditionFlag.Mounting71,
            ConditionFlag.ParticipatingInCustomMatch,
            ConditionFlag.PlayingLordOfVerminion,
            ConditionFlag.ChocoboRacing,
            ConditionFlag.PlayingMiniGame,
            ConditionFlag.Performing,
            ConditionFlag.Transformed,
            ConditionFlag.UsingHousingFunctions
        );
    }
}