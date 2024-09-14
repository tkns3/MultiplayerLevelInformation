﻿using HarmonyLib;

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
            }
            catch (System.Exception e)
            {
                Plugin.Log.Error($"PatchForBeatTogether Failed: BeatTogetherJoinLobyHarmony");
                Plugin.Log.Error($"{e}");
            }

            try
            {
                harmony.Patch(
                    BeatTogetherLeaveLobyHarmony.TargetMethod(),
                    postfix: new HarmonyMethod(typeof(BeatTogetherLeaveLobyHarmony).GetMethod("Postfix")));
            }
            catch (System.Exception e)
            {
                Plugin.Log.Error($"PatchForBeatTogether Failed: BeatTogetherLeaveLobyHarmony");
                Plugin.Log.Error($"{e}");
            }

            try
            {
                harmony.Patch(
                    BeatTogetherSetLocalPlayerLevelHarmony.TargetMethod(),
                    prefix: new HarmonyMethod(typeof(BeatTogetherSetLocalPlayerLevelHarmony).GetMethod("Prefix")));
            }
            catch (System.Exception e)
            {
                Plugin.Log.Error($"PatchForBeatTogether Failed: BeatTogetherSetLocalPlayerLevelHarmony");
                Plugin.Log.Error($"{e}");
            }

            try
            {
                harmony.Patch(
                    BeatTogetherSetPlayerLevelHarmony.TargetMethod(),
                    postfix: new HarmonyMethod(typeof(BeatTogetherSetPlayerLevelHarmony).GetMethod("Postfix")));
            }
            catch (System.Exception e)
            {
                Plugin.Log.Error($"PatchForBeatTogether Failed: BeatTogetherSetPlayerLevelHarmony");
                Plugin.Log.Error($"{e}");
            }

            try
            {
                harmony.Patch(
                    BeatTogetherSetPlayerIsPartyOwnerHarmony.TargetMethod(),
                    postfix: new HarmonyMethod(typeof(BeatTogetherSetPlayerIsPartyOwnerHarmony).GetMethod("Postfix")));
            }
            catch (System.Exception e)
            {
                Plugin.Log.Error($"PatchForBeatTogether Failed: BeatTogetherSetPlayerIsPartyOwnerHarmony");
                Plugin.Log.Error($"{e}");
            }
        }

        public void PatchForMultiplayerPlus(System.Version version)
        {
            MultiplayerPlusHarmony.SetVersion(version);

            try
            {
                harmony.Patch(
                    MultiplayerPlusRoomCreateHarmony.TargetMethod(),
                    postfix: new HarmonyMethod(typeof(MultiplayerPlusRoomCreateHarmony).GetMethod("Postfix")));
            }
            catch (System.Exception e)
            {
                Plugin.Log.Error($"PatchForMultiplayerPlus Failed: MultiplayerPlusRoomCreateHarmony");
                Plugin.Log.Error($"{e}");
            }

            try
            {
                harmony.Patch(
                    MultiplayerPlusRoomJoinHarmony.TargetMethod(),
                    postfix: new HarmonyMethod(typeof(MultiplayerPlusRoomJoinHarmony).GetMethod("Postfix")));
            }
            catch (System.Exception e)
            {
                Plugin.Log.Error($"PatchForMultiplayerPlus Failed: MultiplayerPlusRoomJoinHarmony");
                Plugin.Log.Error($"{e}");
            }

            try
            {
                harmony.Patch(
                    MultiplayerPlusRoomPlayerJoinedHarmony.TargetMethod(),
                    postfix: new HarmonyMethod(typeof(MultiplayerPlusRoomPlayerJoinedHarmony).GetMethod("Postfix")));
            }
            catch (System.Exception e)
            {
                Plugin.Log.Error($"PatchForMultiplayerPlus Failed: MultiplayerPlusRoomPlayerJoinedHarmony");
                Plugin.Log.Error($"{e}");
            }

            try
            {
                harmony.Patch(
                    MultiplayerPlusRoomPlayerLeavedHarmony.TargetMethod(),
                    postfix: new HarmonyMethod(typeof(MultiplayerPlusRoomPlayerLeavedHarmony).GetMethod("Postfix")));
            }
            catch (System.Exception e)
            {
                Plugin.Log.Error($"PatchForMultiplayerPlus Failed: MultiplayerPlusRoomPlayerLeavedHarmony");
                Plugin.Log.Error($"{e}");
            }

            try
            {
                harmony.Patch(
                    MultiplayerPlusNetworkStatusChangeHarmony.TargetMethod(),
                    postfix: new HarmonyMethod(typeof(MultiplayerPlusNetworkStatusChangeHarmony).GetMethod("Postfix")));
            }
            catch (System.Exception e)
            {
                Plugin.Log.Error($"PatchForMultiplayerPlus Failed: MultiplayerPlusNetworkStatusChangeHarmony");
                Plugin.Log.Error($"{e}");
            }

            try
            {
                harmony.Patch(
                    MultiplayerPlusRoomUpdateHarmony.TargetMethod(),
                    postfix: new HarmonyMethod(typeof(MultiplayerPlusRoomUpdateHarmony).GetMethod("Postfix")));
            }
            catch (System.Exception e)
            {
                Plugin.Log.Error($"PatchForMultiplayerPlus Failed: MultiplayerPlusRoomUpdateHarmony");
                Plugin.Log.Error($"{e}");
            }

            try
            {
                harmony.Patch(
                    MultiplayerPlusPlayerUpdateHarmony.TargetMethod(),
                    postfix: new HarmonyMethod(typeof(MultiplayerPlusPlayerUpdateHarmony).GetMethod("Postfix")));
            }
            catch (System.Exception e)
            {
                Plugin.Log.Error($"PatchForMultiplayerPlus Failed: MultiplayerPlusPlayerUpdateHarmony");
                Plugin.Log.Error($"{e}");
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
                Plugin.Log.Error($"GetPrivateFieldValue: {name}, {e}");
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
                Plugin.Log.Error($"GetPrivatePropertyValue: {name}, {e}");
                return default;
            }
        }

    }
}
