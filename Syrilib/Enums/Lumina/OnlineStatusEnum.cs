using Lumina.Excel;
using Lumina.Excel.Sheets;
using Syrilib.Extensions.Lumina;

namespace Syrilib.Enums.Lumina;

public enum OnlineStatusEnum
{
    Empty = 0,
    GameMasterQualityAssurance = 1,
    GameMaster = 2,
    GameMasterEvent = 3,
    EventParticipant = 4,
    Disconnected = 5,
    WaitingForFriendListApproval = 6,
    WaitingForLinkshellApproval = 7,
    WaitingForFreeCompanyApproval = 8,
    NotFound = 9,
    Offline = 10,
    BattleMentor = 11,
    Busy = 12,
    PvP = 13,
    PlayingTripleTriad = 14,
    ViewingCutscene = 15,
    UsingAChocoboPorter = 16,
    AwayFromKeyboard = 17,
    CameraMode = 18,
    LookingForRepairs = 19,
    LookingToRepair = 20,
    LookingToMeldMateria = 21,
    RolePlaying = 22,
    LookingForParty = 23,
    SwordForHire = 24,
    WaitingForDutyFinder = 25,
    RecruitingPartyMembers = 26,
    Mentor = 27,
    PvEMentor = 28,
    TradeMentor = 29,
    PvPMentor = 30,
    Returner = 31,
    NewAdventurer = 32,
    AllianceLeader = 33,
    AlliancePartyLeader = 34,
    AlliancePartyMember = 35,
    PartyLeader = 36,
    PartyMember = 37,
    CrossWorldPartyLeader = 38,
    CrossWorldPartyMember = 39,
    AnotherWorld = 40,
    SharingDuty = 41,
    SimilarDuty = 42,
    InDuty = 43,
    TrialAdventurer = 44,
    FreeCompany = 45,
    GrandCompany = 46,
    Online = 47
}

public static class OnlineStatusExtensions
{
    extension(OnlineStatusEnum onlineStatusEnum)
    {
        public OnlineStatus Row => OnlineStatus.GetRow((uint)onlineStatusEnum);
        
        public RowRef<OnlineStatus> RowRef => OnlineStatus.GetRowRef((uint)onlineStatusEnum);

        private static OnlineStatusEnum FromRow(OnlineStatus onlineStatus)
            => (OnlineStatusEnum)onlineStatus.RowId;

        private static OnlineStatusEnum FromRowRef(RowRef<OnlineStatus> rowRef)
            => (OnlineStatusEnum)rowRef.RowId;
    }

    extension(OnlineStatus onlineStatus)
    {
        public OnlineStatusEnum Enum => OnlineStatusEnum.FromRow(onlineStatus);
    }

    extension(RowRef<OnlineStatus> rowRef)
    {
        public OnlineStatusEnum Enum => OnlineStatusEnum.FromRowRef(rowRef);
    }
}