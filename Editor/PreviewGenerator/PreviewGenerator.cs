﻿using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

namespace AffenCode
{
    public class PreviewGenerator
    {
        private IconBuilderSettings _settings;
        
        public PreviewGenerator(IconBuilderSettings settings)
        {
            _settings = settings;
        }
        
        public Texture2D Generate(GameObject prefab, PreviewSettings settings)
        {
            if (!prefab || settings == null)
            {
                return Texture2D.blackTexture;
            }       
            
            var previewTarget = CreateTarget(prefab);
            SetupTarget(previewTarget, settings);
            SetupLayers(previewTarget, settings);
            
            var previewCamera = CreateCamera(settings);
            SetupCamera(previewCamera, previewTarget, settings);

            var sceneLights = Object.FindObjectsOfType<Light>();
            ToggleSceneLights(sceneLights, false);

            var previewLight = CreateLight(settings);

            previewTarget.position = Vector3.zero;
            previewTarget.eulerAngles = Vector3.zero;

            var child = previewTarget.GetChild(0);
            child.localEulerAngles = new Vector3(0, 180f, 0);

            var result = settings.CreateResultTexture();
            
            var particleSystems = previewTarget.GetComponentsInChildren<ParticleSystem>();

            bool tempRenderSettingsFog = RenderSettings.fog;
            RenderSettings.fog = false;
            
            for (int i = 0; i < settings.GridSize.y; i++)
            {
                for (int j = 0; j < settings.GridSize.x; j++)
                {
                    var angle = (j + i * settings.GridSize.x) * settings.AnglesDelta;
                    previewTarget.eulerAngles = new Vector3(0, angle, 0);

                    foreach (var particleSystem in particleSystems)
                    {
                        if (particleSystem && particleSystem.main.playOnAwake)
                        {
                            particleSystem.Simulate(particleSystem.main.duration * 0.5f, true, true);
                        }
                    }
                    
                    var framePixels = GetRender(previewCamera, settings).GetPixels();
                    result.SetPixels(
                        j * settings.FrameSize.x,
                        i * settings.FrameSize.y,
                        settings.FrameSize.x,
                        settings.FrameSize.y,
                        framePixels);
                }
            }
            
            RenderSettings.fog = tempRenderSettingsFog;

            Object.DestroyImmediate(previewTarget.gameObject);
            Object.DestroyImmediate(previewLight.gameObject);
            Object.DestroyImmediate(previewCamera.gameObject);
            
            ToggleSceneLights(sceneLights, true);

            return result;
        }
        
        private Transform CreateTarget(GameObject prefab)
        {
            var gameObject = new GameObject("PREVIEW_TARGET");
            gameObject.hideFlags = HideFlags.HideAndDontSave;

            var transform = gameObject.transform;
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;

            var target = Object.Instantiate(prefab, transform).transform;
            target.localPosition = Vector3.zero;
            target.localEulerAngles = new Vector3(0f, 180f, 0f);

            return transform;
        }

        private void SetupTarget(Transform target, PreviewSettings settings)
        {
            var child = target.GetChild(0);
            var bounds = CalculateBounds(target, settings);
            var delta = bounds.center - child.position;
            child.localPosition -= delta;
            target.localEulerAngles = new Vector3(0f, 180f, 0f);
        }

        private void SetupLayers(Transform transform, PreviewSettings settings)
        {
            transform.gameObject.layer = settings.RenderLayer;

            for (int i = 0; i < transform.childCount; i++)
            {
                SetupLayers(transform.GetChild(i), settings);
            }
        }
        
        private void ToggleSceneLights(IEnumerable<Light> lights, bool activity)
        {
            if (lights == null)
            {
                return;
            }
            
            foreach (var light in lights)
            {
                if (light)
                {
                    light.enabled = activity;
                }
            }
        }

        private Light CreateLight(PreviewSettings settings)
        {
            var gameObject = new GameObject("PREVIEW_LIGHT");
            gameObject.layer = settings.RenderLayer;
            gameObject.hideFlags = HideFlags.HideAndDontSave;

            var transform = gameObject.transform;
            transform.position = Vector3.zero;
            transform.eulerAngles = new Vector3(_settings.LightingAngle.x, _settings.LightingAngle.y, _settings.LightingAngle.z);

            var light = transform.gameObject.AddComponent<Light>();
            light.cullingMask = 1 << settings.RenderLayer;
            light.type = LightType.Directional;
            light.shadows = LightShadows.Soft;
            light.shadowResolution = LightShadowResolution.VeryHigh;
            light.intensity = _settings.LightingIntensity;

            return light;
        }

        private Camera CreateCamera(PreviewSettings settings)
        {
            var gameObject = new GameObject("PREVIEW_CAMERA");
            gameObject.layer = settings.RenderLayer;
            gameObject.hideFlags = HideFlags.HideAndDontSave;

            var transform = gameObject.transform;
            transform.position = Vector3.zero;
            transform.eulerAngles = _settings.CameraAngle;

            var camera = gameObject.AddComponent<Camera>();
            camera.cullingMask = 1 << settings.RenderLayer;
            camera.orthographic = _settings.CameraOrthographic;
            camera.orthographicSize = _settings.CameraOrthographicSize;

            camera.nearClipPlane = 0.01f;
            camera.fieldOfView = _settings.CameraFieldOfView;

            var color = Color.white;

            if (settings.IsTransparent)
            {
                camera.clearFlags = CameraClearFlags.Color;
                color = Color.gray;
                color.a = 0f;
            }
            else
            {
                camera.clearFlags = CameraClearFlags.SolidColor;
            }

            camera.backgroundColor = color;

            return camera;
        }

        private void SetupCamera(Camera camera, Transform target, PreviewSettings settings)
        {
            var bounds = CalculateBounds(target, settings);
            float cameraDistance = _settings.CameraDistance;
            Vector3 objectSizes = bounds.max - bounds.min;
            float objectSize = Mathf.Max(objectSizes.x, objectSizes.y, objectSizes.z);
            float cameraView = 5.0f * Mathf.Tan(Mathf.Deg2Rad * camera.fieldOfView) / settings.BoundsSizeToCameraDistanceFactor;
            float distance = cameraDistance * objectSize / cameraView;
            distance += objectSize;
            camera.transform.position = bounds.center - distance * camera.transform.forward;
            camera.transform.localPosition += _settings.CameraOffset;
        }

        private Bounds CalculateBounds(Transform target, PreviewSettings settings)
        {
            var renderers = target.gameObject.GetComponentsInChildren<Renderer>();
            var rectTransforms = target.gameObject.GetComponentsInChildren<RectTransform>();
            rectTransforms = rectTransforms.Where(x => x.GetComponent<CanvasRenderer>()).ToArray();
            var particleSystems = target.GetComponentsInChildren<ParticleSystem>();

            var result = new Bounds(target.position, Vector3.zero);

            int framesCount = 15;
            for (int i = 0; i < framesCount; i++)
            {
                var angle = i * 360f / framesCount;
                target.eulerAngles = new Vector3(0, angle, 0);

                foreach (var renderer in renderers)
                {
                    result.Encapsulate(renderer.bounds);
                }

                foreach (var rectTransform in rectTransforms)
                {
                    var corners = new Vector3[4];
                    rectTransform.GetWorldCorners(corners);
                    foreach (var corner in corners)
                    {
                        result.Encapsulate(corner);
                    }
                }

                foreach (var particleSystem in particleSystems)
                {
                    if (particleSystem && particleSystem.main.playOnAwake)
                    {
                        particleSystem.Simulate(particleSystem.main.duration * 0.01f, true, true);
                        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[10];
                        int particlesCount = particleSystem.GetParticles(particles);
                        for (int j = 0; j < particlesCount; ++j)
                        {
                            result.Encapsulate(particles[j].position);
                        }
                    }
                }
            }

            return result;
        }

        private Texture2D GetRender(Camera camera, PreviewSettings settings)
        {
            var tempRenderTexture = RenderTexture.active;
            var renderTexture = RenderTexture.GetTemporary(settings.FrameSize.x, settings.FrameSize.y, settings.RenderDepth);
            RenderTexture.active = renderTexture;

            camera.targetTexture = renderTexture;
            camera.Render();

            var result = settings.CreateFrameTexture();
            
            result.ReadPixels(new Rect(0,
                    0,
                    settings.FrameSize.x,
                    settings.FrameSize.y),
                0,
                0,
                false);

            result.Apply(false, false);

            camera.targetTexture = null;
            RenderTexture.active = tempRenderTexture;
            RenderTexture.ReleaseTemporary(renderTexture);

            return result;
        }
    }
}