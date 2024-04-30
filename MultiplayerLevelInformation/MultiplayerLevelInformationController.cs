using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.FloatingScreen;
using MultiplayerLevelInformation.APIs;
using MultiplayerLevelInformation.HarmonyPatches;
using MultiplayerLevelInformation.Utils;
using MultiplayerLevelInformation.Views;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace MultiplayerLevelInformation
{
    /// <summary>
    /// Monobehaviours (scripts) are added to GameObjects.
    /// For a full list of Messages a Monobehaviour can receive from the game, see https://docs.unity3d.com/ScriptReference/MonoBehaviour.html.
    /// </summary>
    public class MultiplayerLevelInformationController : MonoBehaviour
    {
        private GameObject m_RootGameObject = null;
        private FloatingScreen m_FloatingScreen = null;
        private InformationViewController m_ViewController = null;
        private Material m_FloatingScreenHandleMaterial = null;

        private Vector3 m_MPScreenPositionDefault = new Vector3(0.0f, 0.1f, 1.6f);
        private Vector3 m_MPScreenPositionLast = new Vector3(0.0f, 0.1f, 1.6f);
        private Quaternion m_MPScreenRotationDefault = Quaternion.Euler(90, 0, 0);
        private Quaternion m_MPScreenRotationLast = Quaternion.Euler(90, 0, 0);

        private Vector3 m_BTScreenPositionDefault = new Vector3(0.0f, 0.1f, 1.25f);
        private Vector3 m_BTScreenPositionLast = new Vector3(0.0f, 0.1f, 1.25f);
        private Quaternion m_BTScreenRotationDefault = Quaternion.Euler(90, 0, 0);
        private Quaternion m_BTScreenRotationLast = Quaternion.Euler(90, 0, 0);

        private static Dictionary<string, APIs.BeatSaver.MapDetail> m_mapDetailCaches = new Dictionary<string, APIs.BeatSaver.MapDetail>();
        private static Dictionary<string, LevelOverview> m_lastSelectedLevel = new Dictionary<string, LevelOverview>();

        private static MultiMode m_multiMode = MultiMode.None;
        private static SelectedByTarget m_selectedByTarget = SelectedByTarget.Own;
        private string m_ownUserID = "";
        private string m_hostUserID = "";
        private bool m_handleGrabbing = false;
        private bool m_handleEntering = false;

        private string TargetUserID
        {
            get
            {
                return (m_selectedByTarget == SelectedByTarget.Own) ? m_ownUserID : m_hostUserID;
            }
        }

        #region Monobehaviour Messages
        /// <summary>
        /// Only ever called once, mainly used to initialize variables.
        /// </summary>
        private void Awake()
        {
            GameObject.DontDestroyOnLoad(this); // Don't destroy this object on scene changes
            Plugin.Log.Debug($"{name}: Awake()");
        }

        /// <summary>
        /// Only ever called once on the first frame the script is Enabled. Start is called after any other script's Awake() and before Update().
        /// </summary>
        private void Start()
        {
            MultiplayerPlusHarmony.OnJoinLoby += OnMultiplayerPlusJoinLoby;
            MultiplayerPlusHarmony.OnLeaveLoby += OnMultiplayerPlusLeaveLoby;
            MultiplayerPlusHarmony.OnOwnUserIDNotify += OnOwnUserIDNotify;
            MultiplayerPlusHarmony.OnHostChanged += OnHostChanged;
            MultiplayerPlusHarmony.OnPlayerSelectedLevelChanged += OnPlayerSelectedLevelChanged;

            BeatTogetherHarmony.OnJoinLoby += OnBeatTogetherJoinLoby;
            BeatTogetherHarmony.OnLeaveLoby += OnBeatTogetherLeaveLoby;
            BeatTogetherHarmony.OnOwnUserIDNotify += OnOwnUserIDNotify;
            BeatTogetherHarmony.OnHostChanged += OnHostChanged;
            BeatTogetherHarmony.OnPlayerSelectedLevelChanged += OnPlayerSelectedLevelChanged;
        }

        /// <summary>
        /// Called when the script is being destroyed.
        /// </summary>
        private void OnDestroy()
        {
            Plugin.Log.Debug($"{name}: OnDestroy()");

            MultiplayerPlusHarmony.OnJoinLoby -= OnMultiplayerPlusJoinLoby;
            MultiplayerPlusHarmony.OnLeaveLoby -= OnMultiplayerPlusLeaveLoby;
            MultiplayerPlusHarmony.OnOwnUserIDNotify -= OnOwnUserIDNotify;
            MultiplayerPlusHarmony.OnHostChanged -= OnHostChanged;
            MultiplayerPlusHarmony.OnPlayerSelectedLevelChanged -= OnPlayerSelectedLevelChanged;

            BeatTogetherHarmony.OnJoinLoby -= OnBeatTogetherJoinLoby;
            BeatTogetherHarmony.OnLeaveLoby -= OnBeatTogetherLeaveLoby;
            BeatTogetherHarmony.OnOwnUserIDNotify -= OnOwnUserIDNotify;
            BeatTogetherHarmony.OnHostChanged -= OnHostChanged;
            BeatTogetherHarmony.OnPlayerSelectedLevelChanged -= OnPlayerSelectedLevelChanged;

            SceneManager.activeSceneChanged -= OnActiveSceneChanged;

            CleanupView();
        }
        #endregion

        private void SetupView(MultiMode multiMode)
        {
            CleanupView();

            if (m_RootGameObject == null)
            {
                m_RootGameObject = new GameObject("MultiplayerLevelInformation.FloatingScreen_Root");
                GameObject.DontDestroyOnLoad(m_RootGameObject);

                m_FloatingScreenHandleMaterial = GameObject.Instantiate(UINoGlowMaterial);
                m_FloatingScreenHandleMaterial.color = Color.clear;

                m_ViewController = BeatSaberUI.CreateViewController<InformationViewController>();

                m_FloatingScreen = FloatingScreen.CreateFloatingScreen(new Vector2(85, 40), true, Vector3.zero, Quaternion.identity);
                m_FloatingScreen.SetRootViewController(m_ViewController, HMUI.ViewController.AnimationType.None);
                m_FloatingScreen.transform.SetParent(m_RootGameObject.transform);
                m_FloatingScreen.gameObject.name = "MultiplayerLevelInformation.FloatingScreen";
                m_FloatingScreen.HandleSide = FloatingScreen.Side.Bottom;
                m_FloatingScreen.handle.transform.localScale = Vector3.one * 5.0f;
                m_FloatingScreen.handle.transform.localPosition = new Vector3(-7.6f * 3, -16.8f, 0.0f);
                m_FloatingScreen.handle.gameObject.GetComponent<Renderer>().material = m_FloatingScreenHandleMaterial;
                m_FloatingScreen.HandleGrabbed += OnHandleGrabbed;
                m_FloatingScreen.HandleReleased += OnHandleReleased;
                m_FloatingScreen.transform.localScale = Vector3.one * 0.02f;
                m_FloatingScreen.transform.localPosition = (multiMode == MultiMode.MultiplayerPlus) ? m_MPScreenPositionLast : m_BTScreenPositionLast;
                m_FloatingScreen.transform.localRotation = (multiMode == MultiMode.MultiplayerPlus) ? m_MPScreenRotationLast : m_BTScreenRotationLast;

                var pointerHandler = m_FloatingScreen.handle.AddComponent<PointerHandler>();
                pointerHandler.OnPointerEnter1 += OnHandlePointerEnter;
                pointerHandler.OnPointerExit1 += OnHandlePointerExit;

                m_ViewController.transform.localScale = Vector3.one;
                m_ViewController.transform.localEulerAngles = Vector3.zero;
                m_ViewController.OnSelectedByChanged += OnSelectedByChanged;
            }
            else
            {
                m_FloatingScreen.transform.localPosition = (multiMode == MultiMode.MultiplayerPlus) ? m_MPScreenPositionLast : m_BTScreenPositionLast;
                m_FloatingScreen.transform.localRotation = (multiMode == MultiMode.MultiplayerPlus) ? m_MPScreenRotationLast : m_BTScreenRotationLast;
            }
        }

        private void OnHandleReleased(object sender, FloatingScreenHandleEventArgs args)
        {
            m_handleGrabbing = false;

            if (m_FloatingScreen.transform.localPosition.y < 0f)
            {
                // 床の下で離した場合は位置リセット
                m_FloatingScreen.transform.localPosition = (m_multiMode == MultiMode.MultiplayerPlus) ? m_MPScreenPositionDefault : m_BTScreenPositionDefault;
                m_FloatingScreen.transform.localRotation = (m_multiMode == MultiMode.MultiplayerPlus) ? m_MPScreenRotationDefault : m_BTScreenRotationDefault;
            }

            if (m_multiMode == MultiMode.MultiplayerPlus)
            {
                m_MPScreenPositionLast = m_FloatingScreen.transform.localPosition;
                m_MPScreenRotationLast = m_FloatingScreen.transform.localRotation;
            }
            else
            {
                m_BTScreenPositionLast = m_FloatingScreen.transform.localPosition;
                m_BTScreenRotationLast = m_FloatingScreen.transform.localRotation;
            }

            m_ViewController.SetMoveArrowColor(m_handleEntering ? Color.white : Color.gray);
        }

        private void OnHandleGrabbed(object sender, FloatingScreenHandleEventArgs args)
        {
            m_handleGrabbing = true;
            m_ViewController.SetMoveArrowColor(Color.green);
        }

        public class PointerHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
        {
            public Action OnPointerEnter1;
            public Action OnPointerExit1;

            public void OnPointerEnter(PointerEventData eventData)
            {
                OnPointerEnter1?.Invoke();
            }

            public void OnPointerExit(PointerEventData eventData)
            {
                OnPointerExit1?.Invoke();
            }
        }

        private void OnHandlePointerEnter()
        {
            m_handleEntering = true;
            if (!m_handleGrabbing)
            {
                m_ViewController.SetMoveArrowColor(Color.white);
            }
        }

        private void OnHandlePointerExit()
        {
            m_handleEntering = false;
            if (!m_handleGrabbing)
            {
                m_ViewController.SetMoveArrowColor(Color.gray);
            }
        }

        private void CleanupView()
        {
            if (m_RootGameObject != null)
            {
                m_FloatingScreen.SetRootViewController(null, HMUI.ViewController.AnimationType.None);
                m_ViewController.OnSelectedByChanged -= OnSelectedByChanged;
                GameObject.Destroy(m_ViewController);
                GameObject.Destroy(m_FloatingScreen);
                GameObject.Destroy(m_RootGameObject);
                m_ViewController = null;
                m_FloatingScreen = null;
                m_RootGameObject = null;
            }
        }

        private void ChangeViewParameter(LevelOverview selectedLevel)
        {
            async void wrapper()
            {
                var mapType = selectedLevel.GetMapType();
                if (mapType == LevelOverview.MapType.NotSelect)
                {
                    m_ViewController.SetNotSelected();
                }
                else if (mapType == LevelOverview.MapType.Official)
                {
                    m_ViewController.SetNotSupportOfficial(selectedLevel);
                }
                else
                {
                    try
                    {
                        APIs.BeatSaver.MapDetail map;
                        if (!m_mapDetailCaches.TryGetValue(selectedLevel.hash, out map))
                        {
                            m_ViewController.SetDownloading(selectedLevel);
                            map = await APIs.BeatSaver.GetMapDetail(selectedLevel.hash);

                            try
                            {
                                // ★の数値はScoreSaberのほうを使う
                                var diff = map.GetDifficulty(selectedLevel.mapDifficulty, selectedLevel.mapMode);
                                var leaderboard = await APIs.ScoreSaber.GetLeaderboard(selectedLevel.hash, diff.mapDifficulty, diff.mapMode);
                                diff.stars = leaderboard.stars;
                            }
                            catch (Exception ex)
                            {
                                // Publish直後とかだと404 Not Foundが返ってくることもあるのでScoreSaberからの取得失敗は無視
                                Plugin.Log.Debug($"{ex.Message}");
                            }
                        }

                        var levelDetail = new LevelDetail().Assign(selectedLevel, map);
                        _ = m_mapDetailCaches.TryAdd(selectedLevel.hash, map);
                        m_ViewController.SetDownloaded(selectedLevel, levelDetail);
                    }
                    catch (Exception ex)
                    {
                        Plugin.Log.Debug($"{ex.Message}");
                        m_ViewController.SetFailed(selectedLevel, ex.Message);
                    }
                }
            }
            wrapper();
        }

        private LevelOverview GetLastSelectedLevel(string userID)
        {
            if (!m_lastSelectedLevel.ContainsKey(userID))
            {
                m_lastSelectedLevel.Add(userID, new LevelOverview());
            }
            return m_lastSelectedLevel[userID];
        }

        private void SetLastSelectedLevel(string userID, LevelOverview level)
        {
            m_lastSelectedLevel[userID] = level;
        }

        private void OnActiveSceneChanged(Scene prevScene, Scene nextScene)
        {
            Plugin.Log.Debug($"prev={prevScene.name}, naxt={nextScene.name}");
            switch (nextScene.name)
            {
                case "MainMenu":
                    OnMainMenuSceneActive(prevScene);
                    break;
                default:
                    m_FloatingScreen.gameObject.SetActive(false);
                    // MP+で投票がONになっている場合、プレイする譜面はホスト選択じゃない可能性あるけど実装面倒なのでさぼる
                    m_ViewController.LastPlayedLevelHash = GetLastSelectedLevel(m_hostUserID).hash;
                    break;
            }
        }

        private void OnMainMenuSceneActive(Scene prevScene)
        {
            switch (m_multiMode)
            {
                case MultiMode.None:
                    // do nothing
                    break;
                case MultiMode.BeatTogether:
                    m_lastSelectedLevel.Clear();
                    m_ViewController.SetNotSelected();
                    m_FloatingScreen.gameObject.SetActive(true);
                    break;
                case MultiMode.MultiplayerPlus:
                    m_FloatingScreen.gameObject.SetActive(true);
                    break;
            }
        }

        private void OnSelectedByChanged(SelectedByTarget target)
        {
            Plugin.Log.Debug($"OnSelectedByChanged: {target}");

            if (target == SelectedByTarget.Own)
            {

            }
            m_selectedByTarget = target;
            var lastSelectedLevel = GetLastSelectedLevel(TargetUserID);
            ChangeViewParameter(lastSelectedLevel);
        }

        private void OnBeatTogetherJoinLoby()
        {
            Plugin.Log.Debug($"OnBeatTogetherJoinLoby: preMode={m_multiMode}");

            if (m_multiMode == MultiMode.MultiplayerPlus)
            {
                OnMultiplayerPlusLeaveLoby();
            }

            if (m_multiMode != MultiMode.BeatTogether)
            {
                SetupView(MultiMode.BeatTogether);
                SceneManager.activeSceneChanged += OnActiveSceneChanged;
                m_lastSelectedLevel.Clear();
                m_selectedByTarget = SelectedByTarget.Own;
                m_ownUserID = "";
                m_hostUserID = "";
                m_ViewController.SetTargetSelectMode(enableTargetSelect: true, ownSelected: true);
                m_ViewController.SetNotSelected();
                m_FloatingScreen.gameObject.SetActive(true);

                m_multiMode = MultiMode.BeatTogether;
            }
        }

        private void OnBeatTogetherLeaveLoby()
        {
            Plugin.Log.Debug($"OnBeatTogetherLeaveLoby: preMode={m_multiMode}");

            if (m_multiMode != MultiMode.None)
            {
                SceneManager.activeSceneChanged -= OnActiveSceneChanged;
                m_lastSelectedLevel.Clear();
                m_ownUserID = "";
                m_hostUserID = "";
                m_FloatingScreen.gameObject.SetActive(false);

                m_multiMode = MultiMode.None;

                CleanupView();
            }
        }

        private IEnumerator JoinLoby()
        {
            yield return new WaitForSeconds(0f);
            Plugin.Log.Debug($"OnMultiplayerPlusJoinLoby: preMode={m_multiMode}");

            if (m_multiMode == MultiMode.BeatTogether)
            {
                OnBeatTogetherLeaveLoby();
            }

            if (m_multiMode != MultiMode.MultiplayerPlus)
            {
                SetupView(MultiMode.MultiplayerPlus);
                SceneManager.activeSceneChanged += OnActiveSceneChanged;
                m_lastSelectedLevel.Clear();
                m_selectedByTarget = SelectedByTarget.Own;
                m_ownUserID = "";
                m_hostUserID = "";
                m_ViewController.SetTargetSelectMode(enableTargetSelect: true, ownSelected: true);
                m_ViewController.SetNotSelected();
                m_FloatingScreen.gameObject.SetActive(true);

                m_multiMode = MultiMode.MultiplayerPlus;
            }
        }

        private void OnMultiplayerPlusJoinLoby()
        {
            StartCoroutine(JoinLoby());
        }

        private IEnumerator LeaveLoby()
        {
            yield return new WaitForSeconds(0f);
            Plugin.Log.Debug($"OnMultiplayerPlusLeaveLoby: preMode={m_multiMode}");

            if (m_multiMode != MultiMode.None)
            {
                SceneManager.activeSceneChanged -= OnActiveSceneChanged;
                m_lastSelectedLevel.Clear();
                m_ownUserID = "";
                m_hostUserID = "";
                m_FloatingScreen.gameObject.SetActive(false);

                m_multiMode = MultiMode.None;

                CleanupView();
            }
        }

        private void OnMultiplayerPlusLeaveLoby()
        {
            StartCoroutine(LeaveLoby());
        }

        private IEnumerator SelectLevel(string userID, LevelOverview level)
        {
            yield return new WaitForSeconds(0f);
            Plugin.Log.Debug($"OnPlayerSelectedLevelChanged: {userID}, {level.levelID}, {level.mapDifficulty}, {level.mapMode}");

            var lastLevel = GetLastSelectedLevel(userID);
            if (lastLevel != level)
            {
                SetLastSelectedLevel(userID, level);

                if (userID == TargetUserID)
                {
                    ChangeViewParameter(level);
                }
            }
        }

        private void OnPlayerSelectedLevelChanged(string userID, LevelOverview level)
        {
            StartCoroutine(SelectLevel(userID, level));
        }

        private IEnumerator ChangeUserID(string nextUserID)
        {
            yield return new WaitForSeconds(0f);
            Plugin.Log.Debug($"OnOwnUserIDNotify: pre={m_ownUserID}, next={nextUserID}");

            if (m_ownUserID == nextUserID) yield break;

            m_ownUserID = nextUserID;

            if (m_selectedByTarget == SelectedByTarget.Own)
            {
                var lastSelectedLevel = GetLastSelectedLevel(TargetUserID);
                ChangeViewParameter(lastSelectedLevel);
            }
        }

        private void OnOwnUserIDNotify(string nextUserID)
        {
            StartCoroutine(ChangeUserID(nextUserID));
        }

        private IEnumerator ChangeHost(string nextHostUserID)
        {
            yield return new WaitForSeconds(0f);
            Plugin.Log.Debug($"OnHostChanged: pre={m_hostUserID}, next={nextHostUserID}");

            if (m_hostUserID == nextHostUserID) yield break;

            m_hostUserID = nextHostUserID;

            if (m_selectedByTarget == SelectedByTarget.Host)
            {
                var lastSelectedLevel = GetLastSelectedLevel(TargetUserID);
                ChangeViewParameter(lastSelectedLevel);
            }
        }

        private void OnHostChanged(string nextHostUserID)
        {
            StartCoroutine(ChangeHost(nextHostUserID));
        }

        private static UnityEngine.Material m_UINoGlowMaterial;
        private static UnityEngine.Material UINoGlowMaterial
        {
            get
            {
                if (m_UINoGlowMaterial == null)
                {
                    m_UINoGlowMaterial = UnityEngine.Resources.FindObjectsOfTypeAll<UnityEngine.Material>().Where(x => x.name == "UINoGlow").FirstOrDefault();

                    if (m_UINoGlowMaterial != null)
                    {
                        m_UINoGlowMaterial = UnityEngine.Material.Instantiate(m_UINoGlowMaterial);
                    }
                }

                return m_UINoGlowMaterial;
            }
        }
    }

    public class LevelDetail
    {
        public string levelID;

        public string hash;

        public string imageUrl;

        public BeatmapDifficulty beatmapDifficulty;

        public MapDifficulty mapDifficulty;

        public string difficultyLabel;

        public string beatmapCharacteristic;

        public MapMode mapMode;

        public string key;

        public string songName;

        public string songSubName;

        public string songAuthorName;

        public string songFullName;

        public string mapperName;

        public int duration;

        public double bpm;

        public double nps;

        public int notes;

        public int ob;

        public int bombs;

        public double offset;

        public double njs;

        public double jd;

        public double rt;

        public double star;

        public int parityErrors;

        public int parityWarns;

        public int parityResets;

        public LevelDetail Assign(LevelOverview level, APIs.BeatSaver.MapDetail map)
        {
            if (map == null)
            {
                Plugin.Log.Debug("BeatSaver.MapDetail is invalid. map == null");
                throw new Exception($"Failed to get map data from Beat Server.");
            }
            if (map.versions == null)
            {
                Plugin.Log.Debug("BeatSaver.MapDetail is invalid. map.versions == null");
                throw new Exception($"Failed to get map data from Beat Server.");
            }
            if (map.versions.Length == 0)
            {
                Plugin.Log.Debug("BeatSaver.MapDetail is invalid. map.versions.Length == 0");
                throw new Exception($"Failed to get map data from Beat Server.");
            }
            if (map.versions[0].diffs == null)
            {
                Plugin.Log.Debug("BeatSaver.MapDetail is invalid. map.versions[0].diffs == null");
                throw new Exception($"Failed to get map data from Beat Server.");
            }
            if (map.id == null)
            {
                Plugin.Log.Debug("BeatSaver.MapDetail is invalid. map.id == null");
                throw new Exception($"Failed to get map data from Beat Server.");
            }
            if (map.metadata == null)
            {
                Plugin.Log.Debug("BeatSaver.MapDetail is invalid. map.metadata == null");
                throw new Exception($"Failed to get map data from Beat Server.");
            }
            if (map.metadata.levelAuthorName == null)
            {
                Plugin.Log.Debug("BeatSaver.MapDetail is invalid. map.metadata.levelAuthorName == null");
                throw new Exception($"Failed to get map data from Beat Server.");
            }
            try
            {
                var diff = map.GetDifficulty(level.mapDifficulty, level.mapMode);
                levelID = level.levelID;
                hash = map.versions[0].hash.ToUpper();
                imageUrl = map.versions[0].coverURL;
                beatmapDifficulty = level.beatmapDifficulty;
                mapDifficulty = level.mapDifficulty;
                difficultyLabel = diff.label;
                beatmapCharacteristic = level.beatmapCharacteristic;
                mapMode = level.mapMode;
                key = map.id;
                songName = map.metadata.songName;
                songSubName = map.metadata.songSubName;
                songAuthorName = map.metadata.songAuthorName;
                songFullName = $"{songName} {songSubName} / {songAuthorName}";
                mapperName = map.metadata.levelAuthorName;
                duration = map.metadata.duration;
                bpm = map.metadata.bpm;
                nps = diff.nps;
                notes = diff.notes;
                ob = diff.obstacles;
                bombs = diff.bombs;
                offset = diff.offset;
                njs = diff.njs;
                jd = (bpm > 0) ? BeatmapUtils.GetJd(bpm, njs, offset) : 0;
                rt = (bpm > 0 && njs > 0) ? BeatmapUtils.GetRT(bpm, njs, offset) * 1000 : 0;
                star = diff.stars;
                parityErrors = (diff.paritySummary != null) ? diff.paritySummary.errors : 0;
                parityWarns = (diff.paritySummary != null) ? diff.paritySummary.warns : 0;
                parityResets = (diff.paritySummary != null) ? diff.paritySummary.resets : 0;
                return this;
            }
            catch (Exception e)
            {
                Plugin.Log.Debug($"BeatSaver.MapDetail is invalid. {level.hash}/{level.mapDifficulty}/{level.mapMode} is not found. {e}");
                throw new Exception($"Failed to get map data from Beat Server.");
            }
        }

        public override string ToString()
        {
            string s = $"HASH=[{hash}]\nKey=[{key}] Mapper=[{mapperName}]\n";
            s += $"DURATION=[{(int)duration / 60}:{(int)duration % 60:00}] BPM=[{bpm}]\n";
            s += $"NPS=[{nps:0.##}] NOTE=[{notes}] OB=[{ob}] BOMB=[{bombs}]\n";
            s += $"NJS=[{njs:0.##}] JD=[{jd:0.##}] OFFSET=[{offset:0.##}] RT=[{rt:0.##}]";
            return s;
        }
    }

    public class LevelOverview
    {
        public string levelID;

        public BeatmapDifficulty beatmapDifficulty;

        public MapDifficulty mapDifficulty;

        public string beatmapCharacteristic;

        public MapMode mapMode;

        public string hash = "";

        public enum MapType
        {
            NotSelect,
            Official,
            Custom,
        }

        public LevelOverview()
        {
            levelID = "";
            mapDifficulty = MapDifficulty.Easy;
            mapMode = MapMode.Standard;
        }

        public void Initialize(string levelID, BeatmapDifficulty difficulty, string beatmapCharacteristic)
        {
            this.levelID = levelID;
            beatmapDifficulty = difficulty;
            switch (difficulty)
            {
                case BeatmapDifficulty.Easy: mapDifficulty = MapDifficulty.Easy; break;
                case BeatmapDifficulty.Normal: mapDifficulty = MapDifficulty.Normal; break;
                case BeatmapDifficulty.Hard: mapDifficulty = MapDifficulty.Hard; break;
                case BeatmapDifficulty.Expert: mapDifficulty = MapDifficulty.Expert; break;
                case BeatmapDifficulty.ExpertPlus: mapDifficulty = MapDifficulty.ExpertPlus; break;
            }
            this.beatmapCharacteristic = beatmapCharacteristic;
            switch (beatmapCharacteristic)
            {
                case "Standard": mapMode = MapMode.Standard; break;
                case "OneSaber": mapMode = MapMode.OneSaber; break;
                case "NoArrows": mapMode = MapMode.NoArrows; break;
                case "Lightshow": mapMode = MapMode.Lightshow; break;
                case "90Degree": mapMode = MapMode.Degree90; break;
                case "360Degree": mapMode = MapMode.Degree360; break;
                case "Lawless": mapMode = MapMode.Lawless; break;
                default: mapMode = MapMode.Unknown; break;
            }
            if (this.levelID.Length == 53)
            {
                this.hash = levelID.Substring(13).ToUpper();
            }
        }

        public LevelOverview(string levelID, BeatmapDifficulty difficulty, string beatmapCharacteristic)
        {
            Initialize(levelID, difficulty, beatmapCharacteristic);
        }

        public LevelOverview(PreviewDifficultyBeatmap beatmap)
        {
            Initialize(beatmap.beatmapLevel.levelID, beatmap.beatmapDifficulty, beatmap.beatmapCharacteristic.serializedName);
        }

        public static bool operator ==(LevelOverview lhs, LevelOverview rhs)
        {
            if (ReferenceEquals(lhs, rhs))
            {
                return true;
            }

            if (lhs is null || rhs is null)
            {
                return false;
            }

            return lhs.Equals(rhs);
        }

        public static bool operator !=(LevelOverview lhs, LevelOverview rhs)
        {
            return !(lhs == rhs);
        }

        public override bool Equals(object o)
        {
            if (o == null || GetType() != o.GetType())
            {
                return false;
            }

            LevelOverview other = (LevelOverview)o;
            return levelID == other.levelID && mapDifficulty == other.mapDifficulty && mapMode == other.mapMode && hash == other.hash;
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 31 + (levelID == null ? 0 : levelID.GetHashCode());
            hash = hash * 31 + mapDifficulty.GetHashCode();
            hash = hash * 31 + mapMode.GetHashCode();
            hash = hash * 31 + (this.hash == null ? 0 : this.hash.GetHashCode());
            return hash;
        }

        public MapType GetMapType()
        {
            if (levelID.Length == 0)
            {
                return MapType.NotSelect;
            }
            else if (levelID.Length != 53)
            {
                return MapType.Official;
            }
            else
            {
                return MapType.Custom;
            }
        }
    }

    public enum SelectedByTarget
    {
        Own,
        Host,
    }

    public enum MultiMode
    {
        None,
        BeatTogether,
        MultiplayerPlus,
    }
}
