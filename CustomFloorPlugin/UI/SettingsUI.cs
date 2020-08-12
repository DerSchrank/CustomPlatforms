using HMUI;
using UnityEngine;
using CustomFloorPlugin.Util;
using BeatSaberMarkupLanguage.MenuButtons;
using BeatSaberMarkupLanguage;
using System.Linq;

namespace CustomFloorPlugin
{
    internal class SettingsUI
    {
        public static PlatformsFlowCoordinator platformsFlowCoordinator;

        public static bool created = false;

        public static void CreateMenu()
        {
            if (!created)
            {
                MenuButton menuButton = new MenuButton("Custom Platform", "Change Custom Platforms Here!", ShowPlatformFlow, true);
                MenuButtons.instance.RegisterButton(menuButton);

                created = true;
            }
        }

        public static void ShowPlatformFlow()
        {
            if (platformsFlowCoordinator == null)
            {
                platformsFlowCoordinator = BeatSaberUI.CreateFlowCoordinator<PlatformsFlowCoordinator>();
            }

            BeatSaberUI.MainFlowCoordinator.PresentFlowCoordinator(platformsFlowCoordinator, null, false, false);
        }
    }
}