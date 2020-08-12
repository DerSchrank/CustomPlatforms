using UnityEngine;
using System.Linq;
using CustomFloorPlugin.Util;
using BeatSaberMarkupLanguage.Attributes;
using System.Collections.Generic;

namespace CustomFloorPlugin
{
    public class PlatformManager : MonoBehaviour
    {
        public static PlatformManager Instance;

        private EnvironmentHider menuEnvHider;
        private EnvironmentHider gameEnvHider;

        private PlatformLoader platformLoader;

        private CustomPlatform[] platforms;

        public static void OnLoad()
        {
            if (Instance != null) return;
            GameObject go = new GameObject("Platform Manager");
            go.AddComponent<PlatformManager>();
        }

        private void Awake()
        {
            if (Instance != null) return;
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            EnvironmentArranger.arrangement = (EnvironmentArranger.Arrangement)Plugin.config.GetInt("Settings", "EnvironmentArrangement", 0, true);
            EnvironmentSceneOverrider.overrideMode = (EnvironmentSceneOverrider.EnvOverrideMode)Plugin.config.GetInt("Settings", "EnvironmentOverrideMode", 0, true);
            EnvironmentSceneOverrider.GetSceneInfos();
            EnvironmentSceneOverrider.OverrideEnvironmentScene();

            menuEnvHider = new EnvironmentHider();
            gameEnvHider = new EnvironmentHider();
            platformLoader = new PlatformLoader();

            BSEvents.gameSceneLoaded += HandleGameSceneLoaded;
            BSEvents.menuSceneLoadedFresh += HandleMenuSceneLoadedFresh;
            BSEvents.menuSceneLoaded += HandleMenuSceneLoaded;
            
            RefreshPlatforms();

            HandleMenuSceneLoadedFresh();
            
            //PlatformUI.OnLoad();
        }

        public CustomPlatform AddPlatform(string path)
        {
            CustomPlatform newPlatform = platformLoader.LoadPlatformBundle(path, transform);
            if(newPlatform != null)
            {
                var platList = platforms.ToList();
                platList.Add(newPlatform);
                platforms = platList.ToArray();
            }
            return newPlatform;
        }

        public void RefreshPlatforms()
        {
            if (platforms != null)
            {
                foreach (CustomPlatform platform in platforms)
                {
                    Destroy(platform.gameObject);
                }
            }
            platforms = platformLoader.CreateAllPlatforms(transform);

            PlatformFromUserPrefs();
        }

        public void PlatformFromUserPrefs()
        {
            // Retrieve saved path from player prefs if it exists
            if (Plugin.config.HasKey("Data", "CustomPlatformPath"))
            {
                string savedPath = Plugin.config.GetString("Data", "CustomPlatformPath");
                // Check if this path was loaded and update our platform index
                for (int i = 0; i < platforms.Length; i++)
                {
                    if (savedPath == platforms[i].platName + platforms[i].platAuthor)
                    {
                        ChangeToPlatform(i);
                        break;
                    }
                }
            }
        }

        private void HandleGameSceneLoaded()
        {
            gameEnvHider.FindEnvironment();
            gameEnvHider.HideObjectsForPlatform(currentPlatform);

            EnvironmentArranger.RearrangeEnvironment();
            TubeLightManager.CreateAdditionalLightSwitchControllers();
            //TubeLightManager.UpdateEventTubeLightList();
        }

        private void HandleMenuSceneLoadedFresh()
        {   
            menuEnvHider.FindEnvironment();
            HandleMenuSceneLoaded();
        }

        private void HandleMenuSceneLoaded()
        {
            Debug.Log("PlatformFromUserPrefs?");
            menuEnvHider.HideObjectsForPlatform(currentPlatform);
            PlatformFromUserPrefs();
        }
        
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                NextPlatform();
            }
        }

        public int currentPlatformIndex { get; private set; } = 0;
        public static int CurrentPlatformIndex { get { return Instance.currentPlatformIndex; } }

        public CustomPlatform currentPlatform { get { return platforms[currentPlatformIndex]; } }

        public static List<CustomPlatform> AllPlatforms { get => Instance.platforms.ToList(); }
        public CustomPlatform[] GetPlatforms() => platforms;

        public CustomPlatform GetPlatform(int i)
        {
            return platforms.ElementAt(i);
        }
        public void NextPlatform()
        {
            ChangeToPlatform(currentPlatformIndex + 1);
        }

        public void PrevPlatform()
        {
            ChangeToPlatform(currentPlatformIndex - 1);
        }

        public static void TempChangeToPlatform(int index)
        {
            Instance.ChangeToPlatform(index, false);
        }

        public void ChangeToPlatform(int index, bool save = true)
        {
            Debug.Log($"Change from {currentPlatformIndex} to {index}");

            // Hide current Platform
            currentPlatform.gameObject.SetActive(false);

            // Increment index
            currentPlatformIndex = index % platforms.Length;
            
            // Save path into ModPrefs
            if (save)
                Plugin.config.SetString("Data", "CustomPlatformPath", currentPlatform.platName + currentPlatform.platAuthor);
            
            // Show new platform
            currentPlatform.gameObject.SetActive(true);

            // Hide environment for new platform
            menuEnvHider.HideObjectsForPlatform(currentPlatform);
            gameEnvHider.HideObjectsForPlatform(currentPlatform);

            // Update lightSwitchEvent TubeLight references
            //TubeLightManager.UpdateEventTubeLightList();
        }
    }
}