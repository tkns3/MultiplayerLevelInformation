using HarmonyLib;
using IPA;
using IPA.Config.Stores;
using IPA.Loader;
using UnityEngine;
using IPALogger = IPA.Logging.Logger;

namespace MultiplayerLevelInformation
{
    [Plugin(RuntimeOptions.DynamicInit)]
    public class Plugin
    {
        private readonly PluginMetadata _metadata;
        private readonly Harmony _harmony;

        internal static Plugin Instance { get; private set; }
        internal static IPALogger Log { get; private set; }
        internal static string UserID { get; private set; }

        [Init]
        /// <summary>
        /// Called when the plugin is first loaded by IPA (either when the game starts or when the plugin is enabled if it starts disabled).
        /// [Init] methods that use a Constructor or called before regular methods like InitWithConfig.
        /// Only use [Init] with one Constructor.
        /// </summary>
        public Plugin(IPALogger logger, PluginMetadata metadata)
        {
            Instance = this;
            Log = logger;
            _metadata = metadata;
            _harmony = new Harmony("com.github.tkns3.MultiplayerLevelInformation");
            Log.Info("initialized.");
        }

        #region BSIPA Config
        //Uncomment to use BSIPA's config
        [Init]
        public void InitWithConfig(IPA.Config.Config conf)
        {
            Configuration.PluginConfig.Instance = conf.Generated<Configuration.PluginConfig>();
            Log.Debug("Config loaded");
        }
        #endregion

        [OnEnable]
        public void OnEnable()
        {
            if (PluginManager.GetPlugin("BeatSaberPlus_Multiplayer") != null)
            {
                Log.Info("OnEnable: for BeatTogether and MultiPlayer+.");
                _harmony.PatchAll(_metadata.Assembly);
            }
            else
            {
                Log.Info("OnEnable: for BeatTogether.");
                HarmonyPatches.Util.PatchForBeatTogether(_harmony);
            }
        }

        [OnDisable]
        public void OnDisable()
        {
            _harmony.UnpatchSelf();
        }

        [OnStart]
        public void OnApplicationStart()
        {
            Log.Debug("OnApplicationStart");
            new GameObject("MultiplayerLevelInformationController").AddComponent<MultiplayerLevelInformationController>();

            GetUserID();
        }

        public async void GetUserID()
        {
            var userInfo = await BS_Utils.Gameplay.GetUserInfo.GetUserAsync();
            UserID = userInfo.platformUserId;
        }

        [OnExit]
        public void OnApplicationQuit()
        {
            Log.Debug("OnApplicationQuit");
        }
    }
}