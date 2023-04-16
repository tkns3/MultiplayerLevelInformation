using HarmonyLib;

namespace MultiplayerLevelInformation.HarmonyPatches
{
    public class Patcher
    {
        string harmoneyId;
        Harmony harmony;

        public Patcher(string id)
        {
            harmoneyId = id;
            harmony = new Harmony(id);
        }

        public void PatchForBeatTogether()
        {
            try
            {
                harmony.Patch(
                    BeatTogetherJoinLobyHarmony.TargetMethod(),
                    postfix: new HarmonyMethod(typeof(BeatTogetherJoinLobyHarmony).GetMethod("Postfix")));

                harmony.Patch(
                    BeatTogetherLeaveLobyHarmony.TargetMethod(),
                    postfix: new HarmonyMethod(typeof(BeatTogetherLeaveLobyHarmony).GetMethod("Postfix")));

                harmony.Patch(
                    BeatTogetherSetLocalPlayerLevelHarmony.TargetMethod(),
                    prefix: new HarmonyMethod(typeof(BeatTogetherSetLocalPlayerLevelHarmony).GetMethod("Prefix")));

                harmony.Patch(
                    BeatTogetherSetPlayerLevelHarmony.TargetMethod(),
                    postfix: new HarmonyMethod(typeof(BeatTogetherSetPlayerLevelHarmony).GetMethod("Postfix")));

                harmony.Patch(
                    BeatTogetherSetPlayerIsPartyOwnerHarmony.TargetMethod(),
                    postfix: new HarmonyMethod(typeof(BeatTogetherSetPlayerIsPartyOwnerHarmony).GetMethod("Postfix")));
            }
            catch (System.Exception e)
            {
                Plugin.Log.Critical($"PatchForBeatTogether Failed: {e}");
            }
        }

        public void PatchForMultiplayerPlus()
        {
            try
            {
                harmony.Patch(
                    MultiplayerPlusRoomCreateHarmony.TargetMethod(),
                    postfix: new HarmonyMethod(typeof(MultiplayerPlusRoomCreateHarmony).GetMethod("Postfix")));

                harmony.Patch(
                    MultiplayerPlusRoomJoinHarmony.TargetMethod(),
                    postfix: new HarmonyMethod(typeof(MultiplayerPlusRoomJoinHarmony).GetMethod("Postfix")));

                harmony.Patch(
                    MultiplayerPlusRoomPlayerJoinedHarmony.TargetMethod(),
                    postfix: new HarmonyMethod(typeof(MultiplayerPlusRoomPlayerJoinedHarmony).GetMethod("Postfix")));

                harmony.Patch(
                    MultiplayerPlusRoomPlayerLeavedHarmony.TargetMethod(),
                    postfix: new HarmonyMethod(typeof(MultiplayerPlusRoomPlayerLeavedHarmony).GetMethod("Postfix")));

                harmony.Patch(
                    MultiplayerPlusNetworkStatusChangeHarmony.TargetMethod(),
                    postfix: new HarmonyMethod(typeof(MultiplayerPlusNetworkStatusChangeHarmony).GetMethod("Postfix")));

                harmony.Patch(
                    MultiplayerPlusRoomUpdateHarmony.TargetMethod(),
                    postfix: new HarmonyMethod(typeof(MultiplayerPlusRoomUpdateHarmony).GetMethod("Postfix")));

                harmony.Patch(
                    MultiplayerPlusPlayerUpdateHarmony.TargetMethod(),
                    postfix: new HarmonyMethod(typeof(MultiplayerPlusPlayerUpdateHarmony).GetMethod("Postfix")));
            }
            catch (System.Exception e)
            {
                Plugin.Log.Critical($"PatchForMultiplayerPlus Failed: {e}");
            }
        }

        public void Unpatch()
        {
            harmony.UnpatchSelf();
        }

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
                Plugin.Log.Critical($"GetPrivateFieldValue: {name}, {e}");
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
                Plugin.Log.Critical($"GetPrivatePropertyValue: {name}, {e}");
                return default;
            }
        }

    }
}
