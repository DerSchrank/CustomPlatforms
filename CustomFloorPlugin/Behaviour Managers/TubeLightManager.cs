using System.Linq;
using UnityEngine;
using BS_Utils.Utilities;

namespace CustomFloorPlugin
{
    public class TubeLightManager : MonoBehaviour
    {
        public static void CreateAdditionalLightSwitchControllers()
        {
            var templateSwitchEffect = Resources.FindObjectsOfTypeAll<LightSwitchEventEffect>().FirstOrDefault();

            for (int i = 6; i < 16; i++)
            {
                var newSwitchEffect = ReflectionUtil.CopyComponent(templateSwitchEffect, typeof(LightSwitchEventEffect), typeof(LightSwitchEventEffect), templateSwitchEffect.gameObject) as LightSwitchEventEffect;
                newSwitchEffect.SetPrivateField("_lightsID", i);
                newSwitchEffect.SetPrivateField("_event", (BeatmapEventType)(i-1));
            }
            //UpdateEventTubeLightList();
        }
        
        /*public static void UpdateEventTubeLightList()
        {

            LightSwitchEventEffect[] lightSwitchEvents = Resources.FindObjectsOfTypeAll<LightSwitchEventEffect>();
            foreach (LightSwitchEventEffect switchEffect in lightSwitchEvents)
            {
                ReflectionUtil.SetPrivateField(
                    switchEffect,
                    "_lights",
                    BSLight.GetLightsWithID(switchEffect.LightsID)
                );
            }
            
        }*/
    }
}
