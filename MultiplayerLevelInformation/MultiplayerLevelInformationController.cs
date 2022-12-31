using MultiplayerLevelInformation.Configuration;
using HMUI;
using MultiplayerLevelInformation.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using System.Runtime.CompilerServices;

namespace MultiplayerLevelInformation
{
    /// <summary>
    /// Monobehaviours (scripts) are added to GameObjects.
    /// For a full list of Messages a Monobehaviour can receive from the game, see https://docs.unity3d.com/ScriptReference/MonoBehaviour.html.
    /// </summary>
    public class MultiplayerLevelInformationController : MonoBehaviour
    {
        public static MultiplayerLevelInformationController Instance { get; private set; }

        private static Dictionary<string, APIs.BeatSaver.MapDetail> _mapDetailCaches = new Dictionary<string, APIs.BeatSaver.MapDetail>();

        // These methods are automatically called by Unity, you should remove any you aren't using.
        #region Monobehaviour Messages
        /// <summary>
        /// Only ever called once, mainly used to initialize variables.
        /// </summary>
        private void Awake()
        {
            // For this particular MonoBehaviour, we only want one instance to exist at any time, so store a reference to it in a static property
            //   and destroy any that are created while one already exists.
            if (Instance != null)
            {
                Plugin.Log?.Warn($"Instance of {GetType().Name} already exists, destroying.");
                GameObject.DestroyImmediate(this);
                return;
            }
            GameObject.DontDestroyOnLoad(this); // Don't destroy this object on scene changes
            Instance = this;
            Plugin.Log?.Debug($"{name}: Awake()");
        }
        /// <summary>
        /// Only ever called once on the first frame the script is Enabled. Start is called after any other script's Awake() and before Update().
        /// </summary>
        private void Start()
        {
            gameObject.AddComponent<Canvas>();
            CurvedTextMeshPro textMesh = new GameObject("Text").AddComponent<CurvedTextMeshPro>();
            textMesh.transform.SetParent(transform);
            textMesh.alignment = TextAlignmentOptions.Center;
            textMesh.transform.eulerAngles = new Vector3(PluginConfig.Instance.EulerAnglesX, PluginConfig.Instance.EulerAnglesY, PluginConfig.Instance.EulerAnglesZ);
            textMesh.transform.position = new Vector3(PluginConfig.Instance.PositionX, PluginConfig.Instance.PositionY, PluginConfig.Instance.PositionZ);
            textMesh.color = Color.white;
            textMesh.fontSize = 0.05f;
            textMesh.text = "";
        }

        /// <summary>
        /// Called every frame if the script is enabled.
        /// </summary>
        private void Update()
        {

        }

        public void HideBeatmapInformation()
        {
            Plugin.Log.Info($"HideBeatmapInformation");
            CurvedTextMeshPro textMesh = gameObject.GetComponentInChildren<CurvedTextMeshPro>();
            textMesh.text = "";
        }

        public void ShowBeatmapInformation(PreviewDifficultyBeatmap beatmap)
        {
            Plugin.Log.Info($"ShowBeatmapInformation {beatmap.beatmapLevel.levelID}");

            CurvedTextMeshPro textMesh = gameObject.GetComponentInChildren<CurvedTextMeshPro>();
            if (textMesh == null)
            {
                return;
            }

            if (beatmap.beatmapLevel.levelID.Length != 53)
            {
                textMesh.text = $"{beatmap.beatmapLevel.levelID} is not custom level.";
                Plugin.Log.Info(textMesh.text);
                return;
            }

            string hash = beatmap.beatmapLevel.levelID.Substring(13);

            if (_mapDetailCaches.TryGetValue(hash, out var _map))
            {
                textMesh.text = LevelInformationText(hash, _map, beatmap.beatmapDifficulty, beatmap.beatmapCharacteristic.serializedName);
            }
            else
            {
                async void wrapper()
                {
                    try
                    {
                        textMesh.text = $"Downloading Map Information from Beat Saver...";
                        var map = await APIs.BeatSaver.GetMapDetail(hash);
                        if (map != null)
                        {
                            if (!_mapDetailCaches.ContainsKey(hash))
                            {
                                _mapDetailCaches.Add(hash, map);
                            }
                            textMesh.text = LevelInformationText(hash, map, beatmap.beatmapDifficulty, beatmap.beatmapCharacteristic.serializedName);
                        }
                        else
                        {
                            textMesh.text = $"failed.";
                        }
                    }
                    catch (Exception e)
                    {
                        textMesh.text = e.ToString();
                    }
                }
                wrapper();
            }
        }

        static string LevelInformationText(string hash, APIs.BeatSaver.MapDetail map, BeatmapDifficulty difficulty, string mode)
        {
            Plugin.Log.Info($"LevelInformationText {hash}, {difficulty}, {mode}");
            if (map.versions == null)
            {
                return "map.versions == null";
            }
            if (map.versions.Length == 0)
            {
                return "map.versions.Length == 0";
            }
            if (map.versions[0].diffs == null)
            {
                return "map.versions[0].diffs == null";
            }
            if (map.id == null)
            {
                return "map.id == null";
            }
            if (map.metadata == null)
            {
                return "map.metadata == null";
            }
            if (map.metadata.levelAuthorName == null)
            {
                return "map.metadata.levelAuthorNam == null";
            }
            if (!map.versions[0].hash.ToLower().Equals(hash.ToLower()))
            {
                return $"Selected HASH=[{hash}]\nLatest HASH=[{map.versions[0].hash}]\nKey=[{map.id}] Mapper=[{map.metadata.levelAuthorName}]\nThis Map is Re-Pubslished !!!";
            }
            try
            {
                var diff = map.versions[0].diffs.Single(d => d.difficulty != null && d.difficulty.Equals(difficulty.ToString()) && d.characteristic != null && d.characteristic.Equals(mode));
                var key = map.id;
                var mapper = map.metadata.levelAuthorName;
                var duration = map.metadata.duration;
                var bpm = map.metadata.bpm;
                var nps = diff.nps;
                var notes = diff.notes;
                var ob = diff.obstacles;
                var bombs = diff.bombs;
                var offset = diff.offset;
                var njs = diff.njs;
                var jd = (bpm > 0) ? BeatmapUtils.GetJd(bpm, njs, offset) : 0;
                var rt = (bpm > 0 && njs > 0) ? BeatmapUtils.GetRT(bpm, njs, offset) * 1000 : 0;
                string s = $"HASH=[{hash}]\nKey=[{key}] Mapper=[{mapper}]\n";
                s += $"DURATION=[{(int)duration / 60}:{(int)duration % 60:00}] BPM=[{bpm}]\n";
                s += $"NPS=[{nps:#.##}] NOTE=[{notes}] OB=[{ob}] BOMB=[{bombs}]\n";
                s += $"NJS=[{njs}] JD=[{jd:#.##}] OFFSET=[{offset}] RT=[{rt:#.##}]";
                Plugin.Log.Info(s);
                return s;
            }
            catch (Exception)
            {
                return $"Selected HASH=[{hash}]\nLatest HASH=[{map.versions[0].hash}]\nKey=[{map.id}] Mapper=[{map.metadata.levelAuthorName}]\nThis Map is Re-Pubslished !!!";
            }
        }

        public void ConfigChanged()
        {
            Plugin.Log.Info("ConfigChanged");
            CurvedTextMeshPro textMesh = gameObject.GetComponentInChildren<CurvedTextMeshPro>();
            textMesh.transform.eulerAngles = new Vector3(PluginConfig.Instance.EulerAnglesX, PluginConfig.Instance.EulerAnglesY, PluginConfig.Instance.EulerAnglesZ);
            textMesh.transform.position = new Vector3(PluginConfig.Instance.PositionX, PluginConfig.Instance.PositionY, PluginConfig.Instance.PositionZ);
        }

        /// <summary>
        /// Called every frame after every other enabled script's Update().
        /// </summary>
        private void LateUpdate()
        {

        }

        /// <summary>
        /// Called when the script becomes enabled and active
        /// </summary>
        private void OnEnable()
        {

        }

        /// <summary>
        /// Called when the script becomes disabled or when it is being destroyed.
        /// </summary>
        private void OnDisable()
        {

        }

        /// <summary>
        /// Called when the script is being destroyed.
        /// </summary>
        private void OnDestroy()
        {
            Plugin.Log?.Debug($"{name}: OnDestroy()");
            if (Instance == this)
                Instance = null; // This MonoBehaviour is being destroyed, so set the static instance property to null.

        }
        #endregion
    }
}
