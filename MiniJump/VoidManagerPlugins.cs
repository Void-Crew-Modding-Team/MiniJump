using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;
using VoidManager;
using VoidManager.MPModChecks;

namespace MiniJump
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.USERS_PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInProcess("Void Crew.exe")]
    [BepInDependency(VoidManager.MyPluginInfo.PLUGIN_GUID)]
    public class BepinPlugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "N/A")]
        private void Awake()
        {
            Log = Logger;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), MyPluginInfo.PLUGIN_GUID);
            Configs.Load(this);
            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        }
    }


    public class VoidManagerPlugin : VoidPlugin
    {
        public VoidManagerPlugin()
        {
            VoidManager.Events.Instance.LateUpdate += Jump.CheckDoJump;
        }
        public override MultiplayerType MPType => MultiplayerType.Host;

        public override string Author => MyPluginInfo.PLUGIN_AUTHORS;

        public override string Description => MyPluginInfo.PLUGIN_DESCRIPTION;

        public override string ThunderstoreID => MyPluginInfo.PLUGIN_THUNDERSTORE_ID;
        public override SessionChangedReturn OnSessionChange(SessionChangedInput input)
        {
            switch (input.CallType)
            {
                case CallType.SessionEscalated:
                case CallType.HostStartSession:
                    VoidManager.Progression.ProgressionHandler.DisableProgression(MyPluginInfo.PLUGIN_GUID);
                    break;
                default:
                    break;
            }
            return base.OnSessionChange(input);
        }
    }
}