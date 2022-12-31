using HarmonyLib;
using IPA;
using IPA.Config;
using IPA.Config.Stores;
using IPA.Loader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
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
            Log.Info("MultiplayerLevelInformation initialized.");
        }

        #region BSIPA Config
        //Uncomment to use BSIPA's config
        [Init]
        public void InitWithConfig(Config conf)
        {
            Configuration.PluginConfig.Instance = conf.Generated<Configuration.PluginConfig>();
            Log.Debug("Config loaded");
        }
        #endregion

        [OnEnable]
        public void OnEnable()
        {
            _harmony.PatchAll(_metadata.Assembly);
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
        }

        [OnExit]
        public void OnApplicationQuit()
        {
            Log.Debug("OnApplicationQuit");

        }
    }
}