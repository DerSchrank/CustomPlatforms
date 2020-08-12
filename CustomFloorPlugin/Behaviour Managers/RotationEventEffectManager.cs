using CustomFloorPlugin.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CustomFloorPlugin
{
    public class RotationEventEffectManager : MonoBehaviour
    {
        List<RotationEventEffect> effectDescriptors;
        List<LightRotationEventEffect> lightRotationEffects;
        
        private void OnEnable()
        {
            foreach (LightRotationEventEffect rotEffect in lightRotationEffects)
            {
                var action = BS_Utils.Utilities.ReflectionUtil.GetPrivateField<Action<BeatmapEventData>>(rotEffect, "HandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger");

                BSEvents.beatmapEvent += action;
            }
            BSEvents.menuSceneLoaded += HandleSceneChange;
            BSEvents.gameSceneLoaded += HandleSceneChange;
            HandleSceneChange();
        }
        
        private void OnDisable()
        {
            foreach (LightRotationEventEffect rotEffect in lightRotationEffects)
            {
                var action = BS_Utils.Utilities.ReflectionUtil.GetPrivateField<Action<BeatmapEventData>>(rotEffect, "HandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger");

                BSEvents.beatmapEvent -= action;
            }
            BSEvents.menuSceneLoaded -= HandleSceneChange;
            BSEvents.gameSceneLoaded -= HandleSceneChange;
        }
        
        private void HandleSceneChange()
        {
            foreach (LightRotationEventEffect rotEffect in lightRotationEffects)
            {
                rotEffect.transform.localRotation = BS_Utils.Utilities.ReflectionUtil.GetPrivateField<Quaternion>(rotEffect, "_startRotation");
                rotEffect.enabled = false;
            }
        }

        public void CreateEffects(GameObject go)
        {
            if(lightRotationEffects == null) lightRotationEffects = new List<LightRotationEventEffect>();
            if (effectDescriptors == null) effectDescriptors = new List<RotationEventEffect>();

            RotationEventEffect[] localDescriptors = go.GetComponentsInChildren<RotationEventEffect>(true);

            if (localDescriptors == null) return;

            foreach (RotationEventEffect effectDescriptor in effectDescriptors)
            {
                var rotEvent = effectDescriptor.gameObject.AddComponent<LightRotationEventEffect>();

                BS_Utils.Utilities.ReflectionUtil.SetPrivateField(rotEvent, "_event", (BeatmapEventType)effectDescriptor.eventType);
                BS_Utils.Utilities.ReflectionUtil.SetPrivateField(rotEvent, "_rotationVector", effectDescriptor.rotationVector);
                BS_Utils.Utilities.ReflectionUtil.SetPrivateField(rotEvent, "_transform", rotEvent.transform);
                BS_Utils.Utilities.ReflectionUtil.SetPrivateField(rotEvent, "_startRotation", rotEvent.transform.rotation);
                lightRotationEffects.Add(rotEvent);
                effectDescriptors.Add(effectDescriptor);
            }
        }
    }
}
