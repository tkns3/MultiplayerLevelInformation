using System.Runtime.CompilerServices;
using IPA.Config.Stores;
using MultiplayerLevelInformation;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace MultiplayerLevelInformation.Configuration
{
    internal class PluginConfig
    {
        public static PluginConfig Instance { get; set; }
        public virtual float PositionX { get; set; } = 0f;
        public virtual float PositionY { get; set; } = 0.1f;
        public virtual float PositionZ { get; set; } = 1.3f;
        public virtual float EulerAnglesX { get; set; } = 90f;
        public virtual float EulerAnglesY { get; set; } = 0f;
        public virtual float EulerAnglesZ { get; set; } = 0f;

        /// <summary>
        /// This is called whenever BSIPA reads the config from disk (including when file changes are detected).
        /// </summary>
        public virtual void OnReload()
        {
            // Do stuff after config is read from disk.
            Plugin.Log.Info("OnReload");
            MultiplayerLevelInformationController.Instance.ConfigChanged();
        }

        /// <summary>
        /// Call this to force BSIPA to update the config file. This is also called by BSIPA if it detects the file was modified.
        /// </summary>
        public virtual void Changed()
        {
            // Do stuff when the config is changed.
            Plugin.Log.Info("Changed");
            MultiplayerLevelInformationController.Instance.ConfigChanged();
        }

        /// <summary>
        /// Call this to have BSIPA copy the values from <paramref name="other"/> into this config.
        /// </summary>
        public virtual void CopyFrom(PluginConfig other)
        {
            // This instance's members populated from other
            this.PositionX = other.PositionX;
            this.PositionY = other.PositionY;
            this.PositionZ = other.PositionZ;
            this.EulerAnglesX = other.EulerAnglesX;
            this.EulerAnglesY = other.EulerAnglesY;
            this.EulerAnglesZ = other.EulerAnglesZ;
        }
    }
}
