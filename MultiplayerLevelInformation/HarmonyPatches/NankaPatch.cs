using HarmonyLib;

namespace MultiplayerLevelInformation.HarmonyPatches
{
    [HarmonyPatch(typeof(LobbyPlayersDataModel), nameof(LobbyPlayersDataModel.SetLocalPlayerBeatmapLevel))]
    internal class BeatmapSelectionViewSetBeatmapPatch
    {
        private static void Postfix(PreviewDifficultyBeatmap beatmapLevel)
        {
            if (beatmapLevel != null && beatmapLevel.beatmapLevel != null)
            {
                MultiplayerLevelInformationController.Instance.ShowBeatmapInformation(beatmapLevel);
            }
        }
    }

    [HarmonyPatch(typeof(CenterStageScreenController), nameof(CenterStageScreenController.SetNextGameplaySetupData))]
    internal class CenterStageScreenControllerSetNextGameplaySetupDataPatch
    {
        private static void Postfix(ILevelGameplaySetupData levelGameplaySetupData)
        {
            MultiplayerLevelInformationController.Instance.HideBeatmapInformation();
        }
    }

    [HarmonyPatch(typeof(MultiplayerLobbyConnectionController), nameof(MultiplayerLobbyConnectionController.LeaveLobby))]
    internal class MultiplayerLobbyConnectionControllerLeaveLobbyPatch
    {
        private static void Postfix()
        {
            MultiplayerLevelInformationController.Instance.HideBeatmapInformation();
        }
    }
}
