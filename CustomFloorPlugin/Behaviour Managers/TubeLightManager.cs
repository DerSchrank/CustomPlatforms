using System.Linq;
using UnityEngine;
using BS_Utils.Utilities;
using BSEvents = CustomFloorPlugin.Util.BSEvents;
using System;
using Zenject;
using System.Collections.Generic;

namespace CustomFloorPlugin
{
    public class TubeLightManager : MonoBehaviour
    {
        private static LightWithIdManager defaultLightWithIdManager = new LightWithIdManager();

        public static void CreateAdditionalLightSwitchControllers()
        {
            var templateSwitchEffect = Resources.FindObjectsOfTypeAll<LightSwitchEventEffect>().FirstOrDefault();

            for (int i = 6; i < 16; i++)
            {
                var newSwitchEffect = ReflectionUtil.CopyComponent(templateSwitchEffect, typeof(LightSwitchEventEffect), typeof(LightSwitchEventEffect), templateSwitchEffect.gameObject) as LightSwitchEventEffect;
                newSwitchEffect.SetPrivateField("_lightsID", i);
                newSwitchEffect.SetPrivateField("_event", (BeatmapEventType)(i-1));
            }
        }

        public static void SetColorToDefault(TubeLight tl, TubeBloomPrePassLightWithId tubeBloomLight)
        {
            tubeBloomLight.ColorWasSet(tl.color * 0.7f);
            //tubeBloomLight.Refresh();
        }

        public static Action Bind<T, U>(Action<T, U> func, T arg, U arg2)
        {
            return () => func(arg, arg2);
        }

        public static bool setup = false;
        public static int z = 0;
        private static Color off = new Color(0f, 0f, 0f, 0f);

        public static void SetupTubeLight(TubeLight tl, DiContainer _container)
        {
            var prefab = Resources.FindObjectsOfTypeAll<TubeBloomPrePassLight>().First(x => x.name == "Neon");

            // Don't init twice
            if (tl.tubeBloomLight != null) return;

            var tubeBloomLight = _container.InstantiatePrefabForComponent<TubeBloomPrePassLight>(prefab);
            tubeBloomLight.name = "Tube Light " + z++;

            tl.tubeBloomLight = tubeBloomLight;
            tubeBloomLight.transform.SetParent(tl.transform);
            tubeBloomLight.transform.localRotation = Quaternion.identity;
            tubeBloomLight.transform.localPosition = Vector3.zero;
            tubeBloomLight.transform.localScale = new Vector3(1 / tl.transform.lossyScale.x, 1 / tl.transform.lossyScale.y, 1 / tl.transform.lossyScale.z);

            var withId = tubeBloomLight.GetComponent<TubeBloomPrePassLightWithId>();

            if (tl.GetComponent<MeshFilter>().mesh.vertexCount == 0)
            {
                tl.GetComponent<MeshRenderer>().enabled = false;
            }
            else
            {
                // swap for MeshBloomPrePassLight
                tubeBloomLight.gameObject.SetActive(false);

                MeshBloomPrePassLight meshbloom = ReflectionUtil.CopyComponent(tubeBloomLight, typeof(TubeBloomPrePassLight), typeof(MeshBloomPrePassLight), tubeBloomLight.gameObject) as MeshBloomPrePassLight;
                meshbloom.Init(tl.GetComponent<Renderer>());

                MeshBloomPrePassLightWithId meshbloomid = ReflectionUtil.CopyComponent(withId, typeof(TubeBloomPrePassLightWithId), typeof(MeshBloomPrePassLightWithId), withId.gameObject) as MeshBloomPrePassLightWithId;
                Destroy(withId);
                withId = meshbloomid;

                tubeBloomLight.gameObject.SetActive(true);
                Destroy(tubeBloomLight);

                tubeBloomLight = meshbloom;
            }

            var setColorToDefault = Bind(SetColorToDefault, tl, withId);

            tl.onEnable += () => {
                BSEvents.menuSceneLoaded += setColorToDefault;
                BSEvents.menuSceneLoadedFresh += setColorToDefault;
                setColorToDefault();
            };

            tl.onDisable += () => {
                BSEvents.menuSceneLoaded -= setColorToDefault;
                BSEvents.menuSceneLoadedFresh -= setColorToDefault;
            };

            tubeBloomLight.SetPrivateField("_width", tl.width * 2);
            tubeBloomLight.SetPrivateField("_length", tl.length);
            tubeBloomLight.SetPrivateField("_center", tl.center);
            tubeBloomLight.SetPrivateField("_transform", tubeBloomLight.transform);
            var parabox = tubeBloomLight.GetComponentInChildren<ParametricBoxController>();
            tubeBloomLight.SetPrivateField("_parametricBoxController", parabox);
            var parasprite = tubeBloomLight.GetComponentInChildren<Parametric3SliceSpriteController>();
            tubeBloomLight.SetPrivateField("_dynamic3SliceSprite", parasprite);

            parasprite.InvokeMethod("Init");
            parasprite.GetComponent<MeshRenderer>().enabled = false;

            ReflectionUtil.SetPrivateField(withId, "_tubeBloomPrePassLight", tubeBloomLight);
            ReflectionUtil.SetPrivateField(withId, "_ID", (int)tl.lightsID, typeof(LightWithId));
            withId.SetPrivateField("_lightManager", defaultLightWithIdManager, typeof(LightWithId));

            tubeBloomLight.InvokeMethod("Refresh");
        }

        public void CreateTubeLights(GameObject go)
        {
            var _container = BSEvents.Instance.diContainer;
            foreach (TubeLight tubeLight in go.GetComponentsInChildren<TubeLight>(true))
            {
                SetupTubeLight(tubeLight, _container);
            }
        }

        public static void FixUnregisterErrors()
        {
            var managers = Resources.FindObjectsOfTypeAll<LightWithIdManager>();

            foreach (var m in managers)
            {
                var _lights = ReflectionUtil.GetPrivateField<List<LightWithId>[]>(m, "_lights");
                for (int i = 0; i < 16; i++)
                {
                    if (_lights[i] == null)
                    {
                        _lights[i] = new List<LightWithId>(10);
                    }
                }
            }
        }

        /*internal static void SetColorToDefault(CustomPlatform currentPlatform)
        {
            for (int i = 0; i < 16; i++)
            {
                defaultLightWithIdManager.SetColorForId(i, Color.green);
            }
        }*/

        public static void UpdateEventTubeLightList()
        {
            // For some reason the prefab *and* the DiContainer give us a manager that does nothing?
            var managers = Resources.FindObjectsOfTypeAll<LightWithIdManager>();

            var manager = managers.First(it => it.name.Equals("LightWithIdManager"));
            var lights = PlatformManager.Instance.GetComponentsInChildren<LightWithId>();
            foreach (var light in lights)
            {
                light.InvokeMethod("OnDisable");
                ReflectionUtil.SetPrivateField(light, "_lightManager", manager, typeof(LightWithId));
                light.InvokeMethod("OnEnable");
            }

            var bocc = Resources.FindObjectsOfTypeAll<BeatmapObjectCallbackController>().First();
            var rings = PlatformManager.Instance.GetComponentsInChildren<TrackLaneRingsRotationEffectSpawner>();
            foreach (var ring in rings)
            {
                ReflectionUtil.SetPrivateField(ring, "_beatmapObjectCallbackController", bocc);
            }

            var ringsP = PlatformManager.Instance.GetComponentsInChildren<TrackLaneRingsPositionStepEffectSpawner>();
            foreach (var ring in ringsP)
            {
                ReflectionUtil.SetPrivateField(ring, "_beatmapObjectCallbackController", bocc);
            }
        }
    }
}
