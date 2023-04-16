using System;

namespace MultiplayerLevelInformation.HarmonyPatches
{
    public static class BeatTogetherHarmony
    {
        public static Action OnJoinLoby;
        public static Action OnLeaveLoby;
        public static Action<string, LevelOverview> OnPlayerSelectedLevelChanged;
        public static Action<string> OnHostChanged;
        public static Action<string> OnOwnUserIDNotify;

        public static bool m_isOwnerSetPlayerBeatmapLevel = false;
        public static string m_hostUserID = "";
    }

    public class BeatTogetherJoinLobyHarmony
    {
        public static System.Reflection.MethodBase TargetMethod()
        {
            return typeof(MultiplayerLobbyConnectionController).GetMethod("HandleMultiplayerSessionManagerConnected");
        }

        public static void Postfix()
        {
            Plugin.Log.Debug($"BeatTogetherHarmony: HandleMultiplayerSessionManagerConnected");
            BeatTogetherHarmony.OnJoinLoby?.Invoke();
        }
    }

    public class BeatTogetherLeaveLobyHarmony
    {
        public static System.Reflection.MethodBase TargetMethod()
        {
            return typeof(MultiplayerLobbyConnectionController).GetMethod("LeaveLobby");
        }

        public static void Postfix()
        {
            Plugin.Log.Debug($"BeatTogetherHarmony: LeaveLobby");
            BeatTogetherHarmony.OnLeaveLoby?.Invoke();
        }
    }

    public class BeatTogetherSetLocalPlayerLevelHarmony
    {
        public static System.Reflection.MethodBase TargetMethod()
        {
            return typeof(LobbyPlayersDataModel).GetMethod("SetLocalPlayerBeatmapLevel");
        }

        public static void Prefix(PreviewDifficultyBeatmap beatmapLevel)
        {
            if (beatmapLevel != null && beatmapLevel.beatmapLevel != null)
            {
                Plugin.Log.Debug($"BeatTogetherHarmony: SetLocalPlayerBeatmapLevel");
                BeatTogetherHarmony.m_isOwnerSetPlayerBeatmapLevel = true;
            }
        }
    }

    public class BeatTogetherSetPlayerLevelHarmony
    {
        public static System.Reflection.MethodBase TargetMethod()
        {
            return typeof(LobbyPlayersDataModel).GetMethod("SetPlayerBeatmapLevel");
        }

        public static void Postfix(string userId, PreviewDifficultyBeatmap beatmapLevel)
        {
            if (beatmapLevel != null && beatmapLevel.beatmapLevel != null)
            {
                Plugin.Log.Debug($"BeatTogetherHarmony: SetPlayerBeatmapLevel: {userId}, {beatmapLevel.beatmapLevel.levelID}, {beatmapLevel.beatmapDifficulty}, {beatmapLevel.beatmapCharacteristic.serializedName}");
                if (BeatTogetherHarmony.m_isOwnerSetPlayerBeatmapLevel)
                {
                    BeatTogetherHarmony.m_isOwnerSetPlayerBeatmapLevel = false;
                    BeatTogetherHarmony.OnOwnUserIDNotify?.Invoke(userId);
                }
                BeatTogetherHarmony.OnPlayerSelectedLevelChanged?.Invoke(userId, new LevelOverview(beatmapLevel));
            }
        }
    }

    public class BeatTogetherSetPlayerIsPartyOwnerHarmony
    {
        public static System.Reflection.MethodBase TargetMethod()
        {
            return typeof(LobbyPlayersDataModel).GetMethod("SetPlayerIsPartyOwner");
        }

        public static void Postfix(string userId, bool isPartyOwner, bool notifyChange)
        {
            Plugin.Log.Debug($"BeatTogetherHarmony: SetPlayerIsPartyOwner: {userId}, isPartyOwner={isPartyOwner}");
            if (isPartyOwner && userId != BeatTogetherHarmony.m_hostUserID)
            {
                BeatTogetherHarmony.m_hostUserID = userId;
                BeatTogetherHarmony.OnHostChanged?.Invoke(userId);
            }
        }
    }
}
