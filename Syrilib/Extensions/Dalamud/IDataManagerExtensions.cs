// ReSharper disable MemberCanBeMadeStatic.Local
// ReSharper disable MemberCanBeMadeStatic.Global
// ReSharper disable MemberCanBePrivate.Global

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using Lumina.Data.Files;
using Lumina.Data.Parsing.Layer;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Lumina.Extensions;

namespace Syrilib.Extensions.Dalamud;

// ReSharper disable once InconsistentNaming
public static class IDataManagerExtensions
{
    public record NpcInfo(ulong Id, Vector3 Location, uint ShopId);
    
    extension(IDataManager dataManager)
    {
        public NpcInfo? GetNpcInfo(uint eNpcId, uint territoryId, uint itemId = 0)
        {
            var scene = dataManager.GetRow<TerritoryType>(territoryId)!.Value.Bg.ToString();
            var filenameStart = scene.LastIndexOf('/') + 1;
            var planeventLayerGroup = $"bg/${scene[0..filenameStart]}planevent.lgb";
            var lvb = dataManager.GetFile<LgbFile>(planeventLayerGroup);
            if (lvb is null) return null;
            foreach (var layer in lvb.Layers)
            foreach (var instance in layer.InstanceObjects)
            {
                if (instance.AssetType != LayerEntryType.EventNPC)
                    continue;
                var baseId = ((LayerCommon.ENPCInstanceObject)instance.Object).ParentData.ParentData.BaseId;
                if (baseId != eNpcId) continue;
                var npcId = (1ul << 32) | instance.InstanceId;
                var npcLocation = new Vector3(
                    instance.Transform.Translation.X,
                    instance.Transform.Translation.Y,
                    instance.Transform.Translation.Z
                );

                if (itemId == 0) return new NpcInfo(npcId, npcLocation, 0);
                var vendor = dataManager.FindVendorItem(baseId, itemId);
                return vendor.itemIndex >= 0 ? new NpcInfo(npcId, npcLocation, vendor.shopId) : new NpcInfo(npcId, npcLocation, 0);
            }

            return null;
        }

        public RowRef<T> GetRef<T>(uint rowId, ClientLanguage? language = null) where T : struct, IExcelRow<T>
            => new(dataManager.Excel, rowId, (language ?? IClientState.Get().ClientLanguage).ToLumina());

        public ExcelSheet<T> GetSheet<T>(ClientLanguage? language = null) where T : struct, IExcelRow<T>
            => dataManager.GetExcelSheet<T>(language ?? IClientState.Get().ClientLanguage);

        public ExcelSheet<T> GetSheet<T>(string sheetName, ClientLanguage? language = null)
            where T : struct, IExcelRow<T>
            => dataManager.GetExcelSheet<T>(language ?? IClientState.Get().ClientLanguage, sheetName);

        public T? GetRow<T>(uint rowId, ClientLanguage? language = null) where T : struct, IExcelRow<T>
            => dataManager.GetSheet<T>(language).GetRowOrDefault(rowId);

        public bool TryGetRow<T>(uint rowId, out T row) where T : struct, IExcelRow<T>
            => dataManager.TryGetRow(rowId, null, out row);

        public bool TryGetRow<T>(uint rowId, ClientLanguage? language, out T row) where T : struct, IExcelRow<T>
            => dataManager.GetSheet<T>(language ?? IClientState.Get().ClientLanguage).TryGetRow(rowId, out row);

        public bool TryGetRow<T>(string sheetName, uint rowId, out T row) where T : struct, IExcelRow<T>
            => dataManager.TryGetRow(sheetName, rowId, null, out row);

        public bool TryGetRow<T>(string sheetName, uint rowId, ClientLanguage? language, out T row) where T : struct, IExcelRow<T>
            => dataManager.GetSheet<T>(sheetName, language ?? IClientState.Get().ClientLanguage).TryGetRow(rowId, out row);

        public bool TryFindRow<T>(string sheetName, Predicate<T> predicate, out T row) where T : struct, IExcelRow<T>
            => dataManager.TryFindRow(sheetName, predicate, null, out row);

        public bool TryFindRow<T>(string sheetName, Predicate<T> predicate, ClientLanguage? language, out T row)
            where T : struct, IExcelRow<T>
            => dataManager.GetSheet<T>(sheetName, language ?? IClientState.Get().ClientLanguage).TryGetFirst(predicate, out row);

        public bool TryFindRow<T>(Predicate<T> predicate, out T row) where T : struct, IExcelRow<T>
            => dataManager.TryFindRow(predicate, null, out row);

        public bool TryFindRow<T>(Predicate<T> predicate, ClientLanguage? language, out T row)
            where T : struct, IExcelRow<T>
            => dataManager.GetSheet<T>(language ?? IClientState.Get().ClientLanguage).TryGetFirst(predicate, out row);

        public T? FindRow<T>(Func<T, bool> predicate) where T : struct, IExcelRow<T>
            => dataManager.GetSheet<T>().FirstOrNull(row => predicate(row));

        public IReadOnlyList<T> FindRows<T>(Predicate<T> predicate, ClientLanguage? language = null) where T : struct, IExcelRow<T>
            => [.. dataManager.GetSheet<T>(language ?? IClientState.Get().ClientLanguage).Where(row => predicate(row))];

        public bool TryFindRows<T>(Predicate<T> predicate, out IReadOnlyList<T> rows) where T : struct, IExcelRow<T>
            => dataManager.TryFindRows(predicate, null, out rows);

        public bool TryFindRows<T>(Predicate<T> predicate, ClientLanguage? language, out IReadOnlyList<T> rows)
            where T : struct, IExcelRow<T>
        {
            rows = [.. dataManager.GetSheet<T>(language).Where(row => predicate(row))];
            return rows.Count != 0;
        }
        
        public SubrowRef<T> GetSubRef<T>(uint rowId, ClientLanguage? language = null) where T : struct, IExcelSubrow<T>
            => new(dataManager.Excel, rowId, (language ?? IClientState.Get().ClientLanguage).ToLumina());

        public SubrowExcelSheet<T> GetSubrowSheet<T>(ClientLanguage? language = null) where T : struct, IExcelSubrow<T>
            => dataManager.GetSubrowExcelSheet<T>(language ?? IClientState.Get().ClientLanguage);

        public SubrowExcelSheet<T> GetSubrowSheet<T>(string sheetName, ClientLanguage? language = null) where T : struct, IExcelSubrow<T>
            => dataManager.GetSubrowExcelSheet<T>(language ?? IClientState.Get().ClientLanguage, sheetName);

        public T? GetRow<T>(uint rowId, ushort subRowId, ClientLanguage? language = null) where T : struct, IExcelSubrow<T>
            => dataManager.GetSubrowSheet<T>(language).GetSubrowOrDefault(rowId, subRowId);

        public bool TryGetSubrows<T>(uint rowId, out SubrowCollection<T> rows) where T : struct, IExcelSubrow<T>
            => dataManager.TryGetSubrows(rowId, null, out rows);

        public bool TryGetSubrows<T>(uint rowId, ClientLanguage? language, out SubrowCollection<T> rows) where T : struct, IExcelSubrow<T>
            => dataManager.GetSubrowSheet<T>(language ?? IClientState.Get().ClientLanguage).TryGetRow(rowId, out rows);

        public bool TryGetSubrow<T>(uint rowId, int subRowIndex, out T row) where T : struct, IExcelSubrow<T>
            => dataManager.TryGetSubrow(rowId, subRowIndex, null, out row);
        
        public bool TryGetSubrow<T>(uint rowId, int subRowIndex, ClientLanguage? language, out T row) where T : struct, IExcelSubrow<T> {
            if (!dataManager.GetSubrowSheet<T>(language ?? IClientState.Get().ClientLanguage).TryGetRow(rowId, out var rows) || subRowIndex < rows.Count) {
                row = default;
                return false;
            }

            row = rows[subRowIndex];
            return true;
        }

        public bool TryFindSubrow<T>(Predicate<T> predicate, out T subrow) where T : struct, IExcelSubrow<T>
            => dataManager.TryFindSubrow(predicate, null, out subrow);
        
        public bool TryFindSubrow<T>(Predicate<T> predicate, ClientLanguage? language, out T subrow) where T : struct, IExcelSubrow<T> {
            foreach (var isubrow in from irow in dataManager.GetSubrowSheet<T>(language ?? IClientState.Get().ClientLanguage) from isubrow in irow where predicate(isubrow) select isubrow)
            {
                subrow = isubrow;
                return true;
            }

            subrow = default;
            return false;
        }
        
        public bool TryGetRawRow(string sheetName, uint rowId, out RawRow rawRow)
            => dataManager.TryGetRow(sheetName, rowId, out rawRow);

        public bool TryGetRawRow(string sheetName, uint rowId, ClientLanguage? language, out RawRow rawRow)
            => dataManager.TryGetRow(sheetName, rowId, language, out rawRow);

        private (uint shopId, int itemIndex) FindVendorItem(uint enpcId, uint itemId) {
            var enpcBase = dataManager.GetRow<ENpcBase>(enpcId);
            if (enpcBase == null)
                return (0, -1);

            foreach (var handler in enpcBase.Value.ENpcData.Where(handler => handler.RowId >> 16 == (uint)EventHandlerContent.Shop))
            {
                if (!dataManager.TryGetSubrows<GilShopItem>(handler.RowId, out var items)) continue;
                for (var i = 0; i < items.Count; ++i) {
                    var shopItem = items[i];
                    if (shopItem.Item.RowId == itemId)
                        return (handler.RowId, i);
                }
            }
            return (0, -1);
        }
    }
}