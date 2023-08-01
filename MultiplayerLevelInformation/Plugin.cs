using IPA;
using IPA.Loader;
using MultiplayerLevelInformation.HarmonyPatches;
using MultiplayerLevelInformation.Utils;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using IPALogger = IPA.Logging.Logger;

namespace MultiplayerLevelInformation
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        private readonly PluginMetadata _metadata;
        private readonly Patcher _patcher;

        internal static IPALogger Log { get; private set; }
        internal static string OwnUserID { get; private set; }

        [Init]
        public Plugin(IPALogger logger, PluginMetadata metadata)
        {
            Log = logger;
            _metadata = metadata;
            _patcher = new Patcher("com.github.tkns3.MultiplayerLevelInformation");
            Log.Debug("initialized.");
        }

        #region BSIPA Config
        //Uncomment to use BSIPA's config
        /*
        [Init]
        public void InitWithConfig(IPA.Config.Config conf)
        {
        }
        */
        #endregion

        [OnEnable]
        public void OnEnable()
        {
            var mppPlugin = PluginManager.GetPlugin("BeatSaberPlus_Multiplayer");
            if (mppPlugin == null)
            {
                Log.Info("BeatSaberPlus_Multiplayer is Disable.");
            }
            else
            {
                Log.Info($"BeatSaberPlus_Multiplayer({mppPlugin.HVersion}) is Enable.");
                _patcher.PatchForMultiplayerPlus(new System.Version(mppPlugin.HVersion.ToString()));
            }

            var btgPlugin = PluginManager.GetPlugin("BeatTogether");
            if (btgPlugin == null)
            {
                Log.Info("BeatTogether is Disable.");
            }
            else
            {
                Log.Info($"BeatTogether({btgPlugin.HVersion}) is Enable.");
                _patcher.PatchForBeatTogether();
            }
        }

        [OnDisable]
        public void OnDisable()
        {
            _patcher.Unpatch();
        }

        [OnStart]
        public void OnApplicationStart()
        {
            Log.Debug("OnApplicationStart");

            async void getUserID()
            {
                var userInfo = await BS_Utils.Gameplay.GetUserInfo.GetUserAsync();
                OwnUserID = userInfo.platformUserId;
            }
            getUserID();

            new GameObject("MultiplayerLevelInformationController").AddComponent<MultiplayerLevelInformationController>();

            //テスト用 起動後のメニュー画面でUIを表示したい場合はコメントを外す
            //SceneManager.activeSceneChanged += OnActiveSceneChanged;
        }

        private void OnActiveSceneChanged(Scene prev, Scene next)
        {
            if (next.name == "MainMenu")
            {
                SceneManager.activeSceneChanged -= OnActiveSceneChanged;
                async void showUI()
                {
                    await Task.Delay(3000);
                    MultiplayerPlusHarmony.OnJoinLoby?.Invoke();
                }
                showUI();
            }
        }

        [OnExit]
        public void OnApplicationQuit()
        {
            Log.Debug("OnApplicationQuit");
        }
    }
}