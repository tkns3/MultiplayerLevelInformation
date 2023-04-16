using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace MultiplayerLevelInformation.HarmonyPatches
{
    public static class MultiplayerPlusHarmony
    {
        public static Action OnJoinLoby;
        public static Action OnLeaveLoby;
        public static Action<string, LevelOverview> OnPlayerSelectedLevelChanged;
        public static Action<string> OnHostChanged;
        public static Action<string> OnOwnUserIDNotify;

        internal static int m_networkStatus = 0;
        internal static MultiplayerPlusClass.PlayerData m_Host = new MultiplayerPlusClass.PlayerData();
        internal static Dictionary<uint, MultiplayerPlusClass.PlayerData> m_RoomPlayers = new Dictionary<uint, MultiplayerPlusClass.PlayerData>();

        internal static BeatmapDifficulty ToBeatmapDifficulty(byte diff)
        {
            switch (diff)
            {
                case 0: return BeatmapDifficulty.Easy;
                case 1: return BeatmapDifficulty.Normal;
                case 2: return BeatmapDifficulty.Hard;
                case 3: return BeatmapDifficulty.Expert;
                case 4: return BeatmapDifficulty.ExpertPlus;
                default: return BeatmapDifficulty.ExpertPlus;
            }
        }

        internal static LevelOverview CreateLevelOverview(MultiplayerPlusClass.Level level)
        {
            return new LevelOverview(level.LevelID, ToBeatmapDifficulty(level.Diff), level.CharacteristicSO);
        }
    }

    class MultiplayerPlusClass
    {
        internal class Level
        {
            public string LevelID;
            public string CharacteristicSO;
            public byte Diff;

            public static Level Parse(object levelObject)
            {
                var m = new Level();
                m.LevelID = Patcher.GetPrivatePropertyValue<string>(levelObject, "LevelID");
                m.CharacteristicSO = Patcher.GetPrivatePropertyValue<string>(levelObject, "CharacteristicSO");
                m.Diff = Patcher.GetPrivatePropertyValue<byte>(levelObject, "Diff");
                return m;
            }

            public static Level Parse(object parentObject, string levelFieldName)
            {
                var levelObject = Patcher.GetPrivateFieldValue<object>(parentObject, levelFieldName);
                if (levelObject != null)
                {
                    return Parse(levelObject);
                }
                return null;
            }
        }

        internal class PlayerData
        {
            public uint LUID;
            public string UserID;
            public string UserName;
            public Level SelectedLevel;

            public static PlayerData Parse(object playerDataObject)
            {
                var m = new PlayerData();
                m.LUID = Patcher.GetPrivateFieldValue<uint>(playerDataObject, "LUID");
                m.UserID = Patcher.GetPrivateFieldValue<string>(playerDataObject, "UserID");
                m.UserName = Patcher.GetPrivateFieldValue<string>(playerDataObject, "UserName");
                m.SelectedLevel = Level.Parse(playerDataObject, "SelectedLevel");
                return m;
            }

            public static PlayerData Parse(object parentObject, string playerDataFieldName)
            {
                var playerDataObject = Patcher.GetPrivateFieldValue<object>(parentObject, playerDataFieldName);
                if (playerDataObject != null)
                {
                    return Parse(playerDataObject);
                }
                return null;
            }
        }

        internal class PlayerDataList
        {
            public static List<PlayerData> Parse(object playerDataListObject)
            {
                Type objType = playerDataListObject.GetType();
                Type elementTypes = objType.GetGenericArguments()[0];
                MethodInfo methodInfo = objType.GetMethod("GetEnumerator");
                IEnumerator enumerator = (IEnumerator)methodInfo.Invoke(playerDataListObject, null);

                var list = new List<PlayerData>();
                while (enumerator.MoveNext())
                {
                    object element = enumerator.Current;
                    var playerData = PlayerData.Parse(element);
                    list.Add(playerData);
                }
                return list;
            }

            public static List<PlayerData> Parse(object parentObject, string playerDataListFieldName)
            {
                var playerDataObjectList = Patcher.GetPrivateFieldValue<object>(parentObject, playerDataListFieldName);
                if (playerDataObjectList != null)
                {
                    return Parse(playerDataObjectList);
                }
                return null;
            }
        }

        internal class RoomData
        {
            public uint HostLUID;
            public byte State;
            public Level SelectedLevel;

            public static RoomData Parse(object roomDataObject)
            {
                var m = new RoomData();
                m.HostLUID = Patcher.GetPrivateFieldValue<uint>(roomDataObject, "HostLUID");
                m.State = Patcher.GetPrivateFieldValue<byte>(roomDataObject, "State");
                m.SelectedLevel = Level.Parse(roomDataObject, "SelectedLevel");
                return m;
            }

            public static RoomData Parse(object parentObject, string roomDataFieldName)
            {
                var roomDataObject = Patcher.GetPrivateFieldValue<object>(parentObject, roomDataFieldName);
                if (roomDataObject != null)
                {
                    return Parse(roomDataObject);
                }
                return null;
            }
        }

        internal class SMsgRoomCreateResult
        {
            public RoomData RoomInfo;
            public PlayerData PlayerInfo;

            public static SMsgRoomCreateResult Parse(object sMsgRoomCreateResultObject)
            {
                var m = new SMsgRoomCreateResult();
                m.RoomInfo = RoomData.Parse(sMsgRoomCreateResultObject, "RoomInfo");
                m.PlayerInfo = PlayerData.Parse(sMsgRoomCreateResultObject, "PlayerInfo");
                return m;
            }
        }

        internal class SMsgRoomJoinResult
        {
            public RoomData RoomData;
            public List<PlayerData> Players;

            public static SMsgRoomJoinResult Parse(object sMsgRoomJoinResultObject)
            {
                var m = new SMsgRoomJoinResult();
                m.RoomData = RoomData.Parse(sMsgRoomJoinResultObject, "RoomData");
                m.Players = PlayerDataList.Parse(sMsgRoomJoinResultObject, "Players");
                return m;
            }
        }

        internal class SMsgRoomPlayerJoined
        {
            public PlayerData PlayerData;

            public static SMsgRoomPlayerJoined Parse(object sMsgRoomPlayerJoinedObject)
            {
                var m = new SMsgRoomPlayerJoined();
                m.PlayerData = PlayerData.Parse(sMsgRoomPlayerJoinedObject, "PlayerData");
                return m;
            }
        }

        internal class SMsgRoomPlayerLeaved
        {
            public uint LUID;

            public static SMsgRoomPlayerLeaved Parse(object sMsgRoomPlayerLeavedObject)
            {
                var m = new SMsgRoomPlayerLeaved();
                m.LUID = Patcher.GetPrivateFieldValue<uint>(sMsgRoomPlayerLeavedObject, "LUID");
                return m;
            }
        }

        internal class SMsgRoomUpdated
        {
            public RoomData RoomData;

            public static SMsgRoomUpdated Parse(object sMsgRoomUpdatedObject)
            {
                var m = new SMsgRoomUpdated();
                m.RoomData = RoomData.Parse(sMsgRoomUpdatedObject, "RoomData");
                return m;
            }
        }
    }

    public class MultiplayerPlusRoomCreateHarmony
    {
        public static System.Reflection.MethodBase TargetMethod()
        {
            var type = System.Type.GetType("BeatSaberPlus_Multiplayer.Network.NetworkManager, BeatSaberPlus_Multiplayer");
            var method = AccessTools.DeclaredMethod(type, "Handle_SMsgRoomCreateResult");
            return method;
        }

        public static void Postfix(ref object p_Packet)
        {
            try
            {
                var packet = MultiplayerPlusClass.SMsgRoomCreateResult.Parse(p_Packet);
                if (packet != null && packet.RoomInfo != null)
                {
                    Plugin.Log.Debug($"MultiplayerPlusHarmony: RoomCreateResult");
                    MultiplayerPlusHarmony.m_RoomPlayers.Clear();
                    MultiplayerPlusHarmony.m_RoomPlayers.Add(packet.PlayerInfo.LUID, packet.PlayerInfo);
                    MultiplayerPlusHarmony.m_Host = packet.PlayerInfo;

                    MultiplayerPlusHarmony.OnJoinLoby?.Invoke();
                    MultiplayerPlusHarmony.OnOwnUserIDNotify?.Invoke(Plugin.OwnUserID);
                    MultiplayerPlusHarmony.OnHostChanged?.Invoke(MultiplayerPlusHarmony.m_Host.UserID);
                }
            }
            catch (System.Exception e)
            {
                Plugin.Log.Critical($"MultiplayerPlusHarmony: RoomCreateResult Postfix: {e}");
            }
        }
    }

    public class MultiplayerPlusRoomJoinHarmony
    {
        public static System.Reflection.MethodBase TargetMethod()
        {
            var type = System.Type.GetType("BeatSaberPlus_Multiplayer.Network.NetworkManager, BeatSaberPlus_Multiplayer");
            var method = AccessTools.DeclaredMethod(type, "Handle_SMsgRoomJoinResult");
            return method;
        }

        public static void Postfix(ref object p_Packet)
        {
            try
            {
                var packet = MultiplayerPlusClass.SMsgRoomJoinResult.Parse(p_Packet);
                if (packet != null && packet.RoomData != null && packet.Players != null)
                {
                    Plugin.Log.Debug($"MultiplayerPlusHarmony: RoomJoinResult: {packet.RoomData.HostLUID}");
                    MultiplayerPlusHarmony.m_RoomPlayers.Clear();
                    foreach (var player in packet.Players)
                    {
                        Plugin.Log.Debug($"MultiplayerPlusHarmony: RoomJoinResult: {player.LUID}, {player.UserID}");
                        _ = MultiplayerPlusHarmony.m_RoomPlayers.TryAdd(player.LUID, player);
                        if (player.LUID == packet.RoomData.HostLUID)
                        {
                            MultiplayerPlusHarmony.m_Host = player;
                        }
                    }

                    MultiplayerPlusHarmony.OnJoinLoby?.Invoke();
                    MultiplayerPlusHarmony.OnOwnUserIDNotify?.Invoke(Plugin.OwnUserID);
                    MultiplayerPlusHarmony.OnHostChanged?.Invoke(MultiplayerPlusHarmony.m_Host.UserID);
                    foreach (var player in MultiplayerPlusHarmony.m_RoomPlayers.Values)
                    {
                        MultiplayerPlusHarmony.OnPlayerSelectedLevelChanged?.Invoke(player.UserID, MultiplayerPlusHarmony.CreateLevelOverview(player.SelectedLevel));
                    }
                }
            }
            catch (System.Exception e)
            {
                Plugin.Log.Critical($"MultiplayerPlusHarmony: RoomJoinResult Postfix: {e}");
            }
        }
    }

    public class MultiplayerPlusRoomPlayerJoinedHarmony
    {
        public static System.Reflection.MethodBase TargetMethod()
        {
            var type = System.Type.GetType("BeatSaberPlus_Multiplayer.Network.NetworkManager, BeatSaberPlus_Multiplayer");
            var method = AccessTools.DeclaredMethod(type, "Handle_SMsgRoomPlayerJoined");
            return method;
        }

        public static void Postfix(ref object p_Packet)
        {
            try
            {
                var packet = MultiplayerPlusClass.SMsgRoomPlayerJoined.Parse(p_Packet);
                if (packet != null)
                {
                    Plugin.Log.Debug($"MultiplayerPlusHarmony: RoomPlayerJoined: {packet.PlayerData.LUID}, {packet.PlayerData.UserID}");
                    _ = MultiplayerPlusHarmony.m_RoomPlayers.TryAdd(packet.PlayerData.LUID, packet.PlayerData);
                }
            }
            catch (System.Exception e)
            {
                Plugin.Log.Critical($"MultiplayerPlusHarmony: RoomPlayerJoined Postfix: {e}");
            }
        }
    }

    public class MultiplayerPlusRoomPlayerLeavedHarmony
    {
        public static System.Reflection.MethodBase TargetMethod()
        {
            var type = System.Type.GetType("BeatSaberPlus_Multiplayer.Network.NetworkManager, BeatSaberPlus_Multiplayer");
            var method = AccessTools.DeclaredMethod(type, "Handle_SMsgRoomPlayerLeaved");
            return method;
        }

        public static void Postfix(ref object p_Packet)
        {
            try
            {
                var packet = MultiplayerPlusClass.SMsgRoomPlayerLeaved.Parse(p_Packet);
                if (packet != null)
                {
                    Plugin.Log.Debug($"MultiplayerPlusHarmony: RoomPlayerLeaved: {packet.LUID}");
                    _ = MultiplayerPlusHarmony.m_RoomPlayers.Remove(packet.LUID);
                }
            }
            catch (System.Exception e)
            {
                Plugin.Log.Critical($"MultiplayerPlusHarmony: RoomPlayerLeaved Postfix: {e}");
            }
        }
    }

    public class MultiplayerPlusNetworkStatusChangeHarmony
    {
        public static System.Reflection.MethodBase TargetMethod()
        {
            var type = System.Type.GetType("BeatSaberPlus_Multiplayer.Managers.RoomManager, BeatSaberPlus_Multiplayer");
            var enumType = System.Type.GetType("BeatSaberPlus_Multiplayer.Network.ENetworkStatus, BeatSaberPlus_Multiplayer");
            var method = AccessTools.DeclaredMethod(type, "NetworkManager_OnStatusChange", new System.Type[] { enumType });
            return method;
        }

        public static void Postfix(object p_Status)
        {
            try
            {
                // 0: Discconected, 1: FailedToConnect, 2: Connecting, 3: Connected
                // 4: Authing, 5: Authed, 6: InRoom
                int networkStatus = (int)p_Status;
                Plugin.Log.Debug($"MultiplayerPlusHarmony: NetworkStatusChange: {networkStatus}");

                if (MultiplayerPlusHarmony.m_networkStatus == networkStatus)
                {
                    return;
                }

                if (networkStatus == 0)
                {
                    MultiplayerPlusHarmony.m_Host = new MultiplayerPlusClass.PlayerData();
                    MultiplayerPlusHarmony.m_RoomPlayers.Clear();
                    MultiplayerPlusHarmony.OnLeaveLoby?.Invoke();
                }

                MultiplayerPlusHarmony.m_networkStatus = networkStatus;
            }
            catch (System.Exception e)
            {
                Plugin.Log.Critical($"MultiplayerPlusHarmony: NetworkStatusChange Postfix: {e}");
            }
        }
    }

    public class MultiplayerPlusRoomUpdateHarmony
    {
        public static System.Reflection.MethodBase TargetMethod()
        {
            var type = System.Type.GetType("BeatSaberPlus_Multiplayer.Network.NetworkManager, BeatSaberPlus_Multiplayer");
            var method = AccessTools.DeclaredMethod(type, "Handle_SMsgRoomUpdated");
            return method;
        }

        public static void Postfix(ref object p_Packet)
        {
            try
            {
                var packet = MultiplayerPlusClass.SMsgRoomUpdated.Parse(p_Packet);
                if (packet != null && packet.RoomData != null)
                {
                    //Plugin.Log.Debug($"MultiplayerPlusHarmony: RoomUpdate: {packet.RoomData.HostLUID}, {packet.RoomData.State}, {packet.RoomData.SelectedLevel.LevelID}, {packet.RoomData.SelectedLevel.Diff}, {packet.RoomData.SelectedLevel.CharacteristicSO}");
                    if (MultiplayerPlusHarmony.m_Host.LUID != packet.RoomData.HostLUID)
                    {
                        if (MultiplayerPlusHarmony.m_RoomPlayers.TryGetValue(packet.RoomData.HostLUID, out var player))
                        {
                            MultiplayerPlusHarmony.m_Host = player;
                            MultiplayerPlusHarmony.OnHostChanged?.Invoke(MultiplayerPlusHarmony.m_Host.UserID);
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Plugin.Log.Critical($"MultiplayerPlusHarmony: RoomUpdate Postfix: {e}");
            }
        }
    }

    public class MultiplayerPlusPlayerUpdateHarmony
    {

        public static System.Reflection.MethodBase TargetMethod()
        {
            var type = System.Type.GetType("BeatSaberPlus_Multiplayer.UI.MultiplayerPRoomView, BeatSaberPlus_Multiplayer");
            var method = AccessTools.DeclaredMethod(type, "NetworkManager_OnRoomPlayerUpdated");
            return method;
        }

        public static void Postfix(object p_PlayerData)
        {
            try
            {
                var playerData = MultiplayerPlusClass.PlayerData.Parse(p_PlayerData);
                if (playerData != null && playerData.SelectedLevel != null)
                {
                    //Plugin.Log.Debug($"MultiplayerPlusHarmony: RoomPlayerUpdated: {playerData.LUID}, {playerData.UserID}, {playerData.SelectedLevel.LevelID}, {playerData.SelectedLevel.Diff}, {playerData.SelectedLevel.CharacteristicSO}");
                    if (MultiplayerPlusHarmony.m_RoomPlayers.TryGetValue(playerData.LUID, out var pre))
                    {
                        var preLevel = MultiplayerPlusHarmony.CreateLevelOverview(pre.SelectedLevel);
                        var nextLevel = MultiplayerPlusHarmony.CreateLevelOverview(playerData.SelectedLevel);

                        if (preLevel != nextLevel)
                        {
                            MultiplayerPlusHarmony.OnPlayerSelectedLevelChanged?.Invoke(playerData.UserID, nextLevel);
                        }

                        MultiplayerPlusHarmony.m_RoomPlayers[playerData.LUID] = playerData;
                    }
                }
            }
            catch (System.Exception e)
            {
                Plugin.Log.Critical($"MultiplayerPlusHarmony: RoomPlayerUpdated Postfix: {e}");
            }
        }
    }
}
