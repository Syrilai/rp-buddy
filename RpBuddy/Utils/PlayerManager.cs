using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using System;

namespace RpBuddy.Utils
{
    internal readonly struct PlayerInfo
    {
        public readonly string CharacterName;
        public readonly uint WorldId;

        public readonly uint OnlineStatus;

        // Managed Fields
        public readonly string CharacterFirstName;
        public readonly string CharacterLastName;

        public PlayerInfo(string characterName, uint worldId, uint onlineStatus)
        {
            CharacterName = characterName;
            WorldId = worldId;
            OnlineStatus = onlineStatus;

            var nameParts = characterName.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            CharacterFirstName = nameParts.Length > 0 ? nameParts[0] : string.Empty;
            CharacterLastName = nameParts.Length > 1 ? nameParts[1] : string.Empty;
        }
    }

    internal class PlayerManager
    {
        public static IPlayerCharacter? GetPlayerCharacterFromPayload(PlayerPayload payload)
        {
            var objectTable = Plugin.ObjectTable;

            // Check if the payload is not actually us (sus)
            if (objectTable.LocalPlayer != null &&
                objectTable.LocalPlayer.HomeWorld.RowId == payload.World.RowId &&
                objectTable.LocalPlayer.Name.TextValue == payload.PlayerName)
            {
                return objectTable.LocalPlayer;
            }

            // And then actually check who is near us (or at least try to)
            foreach (var obj in objectTable)
            {
                if (obj is IPlayerCharacter pc &&
                    pc.HomeWorld.RowId == payload.World.RowId &&
                    pc.Name.TextValue == payload.PlayerName)
                {
                    return pc;
                }
            }

            return null;
        }

        public static PlayerInfo? GetPlayerInfoFromPayload(PlayerPayload payload)
        {
            var playerCharacter = GetPlayerCharacterFromPayload(payload);
            if (playerCharacter == null) return null;

            return new(playerCharacter.Name.TextValue, playerCharacter.HomeWorld.RowId, playerCharacter.OnlineStatus.RowId);
        }
    }
}
