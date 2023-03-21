using HarmonyLib;

namespace MultiplayerLevelInformation.HarmonyPatches
{
    public static class Util
    {
        public static T GetPrivateFieldValue<T>(object obj, string name)
        {
            try
            {
                //Plugin.Log.Info($"GetPrivateFieldValue: {name}");
                var type = obj.GetType();
                var field = type.GetField(name, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                return (T)field.GetValue(obj);
            }
            catch (System.Exception e)
            {
                Plugin.Log.Info($"GetPrivateFieldValue: {name}, {e}");
                return default;
            }
        }

        public static T GetPrivatePropertyValue<T>(object obj, string name)
        {
            try
            {
                //Plugin.Log.Info($"GetPrivatePropertyValue: {name}");
                var type = obj.GetType();
                var property = type.GetProperty(name, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                return (T)property.GetValue(obj);
            }
            catch (System.Exception e)
            {
                Plugin.Log.Info($"GetPrivatePropertyValue: {name}, {e}");
                return default;
            }
        }

        public static void PatchForBeatTogether(Harmony harmony)
        {
            
            harmony.Patch(typeof(LobbyPlayersDataModel).GetMethod("SetLocalPlayerBeatmapLevel"), postfix: new HarmonyMethod(typeof(LobbyPlayersDataModelSetLocalPlayerBeatmapLevelPatch).GetMethod("Postfix")));
            harmony.Patch(typeof(MultiplayerLobbyCenterScreenLayoutAnimator).GetMethod("StartCountdown"), postfix: new HarmonyMethod(typeof(MultiplayerLobbyCenterScreenLayoutAnimatorStartCountdownPatch).GetMethod("Postfix")));
            harmony.Patch(typeof(MultiplayerLobbyConnectionController).GetMethod("LeaveLobby"), postfix: new HarmonyMethod(typeof(MultiplayerLobbyConnectionControllerLeaveLobbyPatch).GetMethod("Postfix")));
        }
    }

    [HarmonyPatch(typeof(LobbyPlayersDataModel), nameof(LobbyPlayersDataModel.SetLocalPlayerBeatmapLevel))]
    public class LobbyPlayersDataModelSetLocalPlayerBeatmapLevelPatch
    {
        public static void Postfix(PreviewDifficultyBeatmap beatmapLevel)
        {
            if (beatmapLevel != null && beatmapLevel.beatmapLevel != null)
            {
                MultiplayerLevelInformationController.Instance.ShowBeatmapInformation(beatmapLevel);
            }
        }
    }

    [HarmonyPatch(typeof(MultiplayerLobbyCenterScreenLayoutAnimator), nameof(MultiplayerLobbyCenterScreenLayoutAnimator.StartCountdown))]
    public class MultiplayerLobbyCenterScreenLayoutAnimatorStartCountdownPatch
    {
        public static void Postfix()
        {
            MultiplayerLevelInformationController.Instance.HideBeatmapInformation();
        }
    }

    [HarmonyPatch(typeof(MultiplayerLobbyConnectionController), nameof(MultiplayerLobbyConnectionController.LeaveLobby))]
    public class MultiplayerLobbyConnectionControllerLeaveLobbyPatch
    {
        public static void Postfix()
        {
            MultiplayerLevelInformationController.Instance.HideBeatmapInformation();
        }
    }

    [HarmonyPatch]
    class BeatSaberPlus_Multiplayer__UI__MultiplayerPRoomView__NetworkManager_OnRoomPlayerUpdated
    {
        static System.Reflection.MethodBase TargetMethod()
        {
            var type = System.Type.GetType("BeatSaberPlus_Multiplayer.UI.MultiplayerPRoomView, BeatSaberPlus_Multiplayer");
            var m = AccessTools.DeclaredMethod(type, "NetworkManager_OnRoomPlayerUpdated");
            return m;
        }
        static void Postfix(object p_PlayerData)
        {
            var userID = Util.GetPrivateFieldValue<string>(p_PlayerData, "UserID");
            object selectedLevel = Util.GetPrivateFieldValue<object>(p_PlayerData, "SelectedLevel");
            var levelID = Util.GetPrivatePropertyValue<string>(selectedLevel, "LevelID");
            var difficulty = Util.GetPrivatePropertyValue<byte>(selectedLevel, "Diff");
            var mode = Util.GetPrivatePropertyValue<string>(selectedLevel, "CharacteristicSO");

            Plugin.Log.Info($"BeatSaberPlus_Multiplayer.UI.MultiplayerPRoomView.NetworkManager_OnRoomPlayerUpdated: {userID}, {levelID}, {difficulty}, {mode}");
            if (levelID.Length > 0 && mode.Length > 0 && userID.Equals(Plugin.UserID))
            {
                BeatmapDifficulty beatmapDifficulty;
                switch (difficulty)
                {
                    case 0: beatmapDifficulty = BeatmapDifficulty.Easy; break;
                    case 1: beatmapDifficulty = BeatmapDifficulty.Normal; break;
                    case 2: beatmapDifficulty = BeatmapDifficulty.Hard; break;
                    case 3: beatmapDifficulty = BeatmapDifficulty.Expert; break;
                    case 4: beatmapDifficulty = BeatmapDifficulty.ExpertPlus; break;
                    default: beatmapDifficulty = BeatmapDifficulty.ExpertPlus; break;
                }
                MultiplayerLevelInformationController.Instance.ShowBeatmapInformation(levelID, beatmapDifficulty, mode);
            }
        }
    }

    [HarmonyPatch]
    class BeatSaberPlus_Multiplayer__Network__NetworkManager__Close
    {
        static System.Reflection.MethodBase TargetMethod()
        {
            var type = System.Type.GetType("BeatSaberPlus_Multiplayer.Network.NetworkManager, BeatSaberPlus_Multiplayer");
            var m = AccessTools.DeclaredMethod(type, "Close");
            return m;
        }
        static void Postfix()
        {
            Plugin.Log.Info($"BeatSaberPlus_Multiplayer.Network.NetworkManager.Close");
            MultiplayerLevelInformationController.Instance.HideBeatmapInformation();
        }
    }

    [HarmonyPatch]
    class BeatSaberPlus_Multiplayer__Network__NetworkManager__Handle_SMsgRoomState
    {
        static System.Reflection.MethodBase TargetMethod()
        {
            var type = System.Type.GetType("BeatSaberPlus_Multiplayer.Network.NetworkManager, BeatSaberPlus_Multiplayer");
            var m = AccessTools.DeclaredMethod(type, "Handle_SMsgRoomState");
            return m;
        }
        static void Postfix(ref object p_Packet)
        {
            var state = Util.GetPrivateFieldValue<byte>(p_Packet, "State");
            Plugin.Log.Info($"BeatSaberPlus_Multiplayer.Network.NetworkManager.Handle_SMsgRoomState: {state}");
            bool isHide;
            switch (state)
            {
                case 0: // None
                    isHide = true;
                    break;
                case 1: // SelectingSOng
                    isHide = false;
                    break;
                case 2: // WarmingUp
                    isHide = false;
                    break;
                case 3: // Playing
                    isHide = true;
                    break;
                case 4: // Results
                    isHide = false;
                    break;
                default:
                    isHide = true;
                    break;
            }
            if (isHide)
            {
                MultiplayerLevelInformationController.Instance.HideBeatmapInformation();
            }
            else
            {
                MultiplayerLevelInformationController.Instance.ShowBeatmapInformation();
            }
        }
    }
}
