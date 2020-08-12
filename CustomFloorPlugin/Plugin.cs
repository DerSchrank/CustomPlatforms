using IPA;
using CustomFloorPlugin.Util;

namespace CustomFloorPlugin
{
    [Plugin(RuntimeOptions.DynamicInit)]
    public class Plugin
    {
        public static BS_Utils.Utilities.Config config;
        public static IPA.Logging.Logger logger;
        private bool init = false;

        [Init]
        public void Init(object thisWillBeNull, IPA.Logging.Logger logger)
        {
            Plugin.logger = logger;
        }

        [OnStart]
        public void OnApplicationStart()
        {
            //Instance = this;
            BSEvents.OnLoad();
            SettingsUI.CreateMenu();
            BSEvents.menuSceneLoadedFresh += OnMenuSceneLoadedFresh;
        }

        private void OnMenuSceneLoadedFresh()
        {
            if(!init){ 
                init = true;
                config = new BS_Utils.Utilities.Config("Custom Platforms");
                PlatformManager.OnLoad();
            }
        }

        [OnExit]
        public void OnApplicationQuit()
        {
            BSEvents.menuSceneLoadedFresh -= OnMenuSceneLoadedFresh;
        }
    }
}