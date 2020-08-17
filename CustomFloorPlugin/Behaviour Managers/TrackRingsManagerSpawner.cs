using BS_Utils.Utilities;
using System.Collections.Generic;
using UnityEngine;
using BSEvents = CustomFloorPlugin.Util.BSEvents;

namespace CustomFloorPlugin
{
    class TrackRingsManagerSpawner : MonoBehaviour
    {
        List<TrackRings> trackRingsDescriptors;
        public List<TrackLaneRingsManager> trackLaneRingsManagers;
        List<TrackLaneRingsRotationEffectSpawner> rotationSpawners;
        List<TrackLaneRingsPositionStepEffectSpawner> stepSpawners;
        
        private void OnEnable()
        {
            BSEvents.beatmapEvent += ProxyEvent;
            foreach (TrackLaneRingsPositionStepEffectSpawner spawner in stepSpawners)
            {
                BSEvents.beatmapEvent += spawner.HandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger;
            }
        }

        private void ProxyEvent(BeatmapEventData data)
        {
            foreach (TrackLaneRingsRotationEffectSpawner spawner in rotationSpawners)
            {
                ReflectionUtil.InvokeMethod(spawner, "HandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger", new object[] { data });
            }
        }

        private void Start()
        {
            // make sure the rings are parented to this transform
            foreach (var trackLaneRingsManager in trackLaneRingsManagers)
            {
                var rings = ReflectionUtil.GetPrivateField<TrackLaneRing[]>(trackLaneRingsManager, "_rings");
                foreach (var ring in rings)
                {
                    ring.transform.parent = transform;
                }
            }
        }

        private void OnDisable()
        {
            BSEvents.beatmapEvent -= ProxyEvent;
            foreach (TrackLaneRingsPositionStepEffectSpawner spawner in stepSpawners)
            {
                BSEvents.beatmapEvent -= spawner.HandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger;
            }
        }

        public void CreateTrackRings(GameObject go)
        {
            var _container = BSEvents.Instance.diContainer;

            if (rotationSpawners == null) rotationSpawners = new List<TrackLaneRingsRotationEffectSpawner>();
            if (stepSpawners == null) stepSpawners = new List<TrackLaneRingsPositionStepEffectSpawner>();
            if (trackLaneRingsManagers == null) trackLaneRingsManagers = new List<TrackLaneRingsManager>();
            if (trackRingsDescriptors == null) trackRingsDescriptors = new List<TrackRings>();
            
            TrackRings[] ringsDescriptors = go.GetComponentsInChildren<TrackRings>();

            foreach (TrackRings trackRingDesc in ringsDescriptors)
            {
                trackRingsDescriptors.Add(trackRingDesc);

                var ringsManager = trackRingDesc.gameObject.AddComponent<TrackLaneRingsManager>();
                trackLaneRingsManagers.Add(ringsManager);

                var ring = trackRingDesc.trackLaneRingPrefab.AddComponent<TrackLaneRing>();

                ReflectionUtil.SetPrivateField(ringsManager, "_trackLaneRingPrefab", ring);
                ReflectionUtil.SetPrivateField(ringsManager, "_ringCount", trackRingDesc.ringCount);
                ReflectionUtil.SetPrivateField(ringsManager, "_ringPositionStep", trackRingDesc.ringPositionStep);

                var bocc = new BeatmapObjectCallbackController();

                if (trackRingDesc.useRotationEffect)
                {
                    var rotationEffect = _container.InstantiateComponent<TrackLaneRingsRotationEffect>(trackRingDesc.gameObject);
                    //var rotationEffect = trackRingDesc.gameObject.AddComponent<TrackLaneRingsRotationEffect>();

                    ReflectionUtil.SetPrivateField(rotationEffect, "_trackLaneRingsManager", ringsManager);
                    ReflectionUtil.SetPrivateField(rotationEffect, "_startupRotationAngle", trackRingDesc.startupRotationAngle);
                    ReflectionUtil.SetPrivateField(rotationEffect, "_startupRotationStep", trackRingDesc.startupRotationStep);
                    ReflectionUtil.SetPrivateField(rotationEffect, "_startupRotationPropagationSpeed", trackRingDesc.startupRotationPropagationSpeed);
                    ReflectionUtil.SetPrivateField(rotationEffect, "_startupRotationFlexySpeed", trackRingDesc.startupRotationFlexySpeed);

                    //var rotationEffectSpawner = _container.InstantiateComponent<TrackLaneRingsRotationEffectSpawner>(trackRingDesc.gameObject);
                    var rotationEffectSpawner = trackRingDesc.gameObject.AddComponent<TrackLaneRingsRotationEffectSpawner>();
                    rotationSpawners.Add(rotationEffectSpawner);

                    ReflectionUtil.SetPrivateField(rotationEffectSpawner, "_beatmapEventType", (BeatmapEventType)trackRingDesc.rotationSongEventType);
                    ReflectionUtil.SetPrivateField(rotationEffectSpawner, "_rotationStep", trackRingDesc.rotationStep);
                    ReflectionUtil.SetPrivateField(rotationEffectSpawner, "_rotationPropagationSpeed", trackRingDesc.rotationPropagationSpeed);
                    ReflectionUtil.SetPrivateField(rotationEffectSpawner, "_rotationFlexySpeed", trackRingDesc.rotationFlexySpeed);
                    ReflectionUtil.SetPrivateField(rotationEffectSpawner, "_trackLaneRingsRotationEffect", rotationEffect);
                    ReflectionUtil.SetPrivateField(rotationEffectSpawner, "_beatmapObjectCallbackController", bocc);
                }
                if (trackRingDesc.useStepEffect)
                {
                    //var stepEffectSpawner = _container.InstantiateComponent<TrackLaneRingsPositionStepEffectSpawner>(trackRingDesc.gameObject);
                    var stepEffectSpawner = trackRingDesc.gameObject.AddComponent<TrackLaneRingsPositionStepEffectSpawner>();
                    stepSpawners.Add(stepEffectSpawner);

                    ReflectionUtil.SetPrivateField(stepEffectSpawner, "_trackLaneRingsManager", ringsManager);
                    ReflectionUtil.SetPrivateField(stepEffectSpawner, "_beatmapEventType", (BeatmapEventType)trackRingDesc.stepSongEventType);
                    ReflectionUtil.SetPrivateField(stepEffectSpawner, "_minPositionStep", trackRingDesc.minPositionStep);
                    ReflectionUtil.SetPrivateField(stepEffectSpawner, "_maxPositionStep", trackRingDesc.maxPositionStep);
                    ReflectionUtil.SetPrivateField(stepEffectSpawner, "_moveSpeed", trackRingDesc.moveSpeed);
                    ReflectionUtil.SetPrivateField(stepEffectSpawner, "_beatmapObjectCallbackController", bocc);
                }
            }
        }
    }
}
