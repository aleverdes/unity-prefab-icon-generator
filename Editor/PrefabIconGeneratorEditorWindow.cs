using System;
using System.Drawing;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AffenCode
{
    public class PrefabIconGeneratorEditorWindow : EditorWindow
    {
        private const string DestinationFolderPathPrefKey = "AffenCode.PrefabIconGenerator.DestinationFolderPath";
        private const string CameraOffsetXPrefKey = "AffenCode.PrefabIconGenerator.CameraOffset.X";
        private const string CameraOffsetYPrefKey = "AffenCode.PrefabIconGenerator.CameraOffset.Y";
        private const string CameraOffsetZPrefKey = "AffenCode.PrefabIconGenerator.CameraOffset.Z";
        private const string CameraAngleXPrefKey = "AffenCode.PrefabIconGenerator.CameraAngle.X";
        private const string CameraAngleYPrefKey = "AffenCode.PrefabIconGenerator.CameraAngle.Y";
        private const string CameraAngleZPrefKey = "AffenCode.PrefabIconGenerator.CameraAngle.Z";
        private const string LightingAngleXPrefKey = "AffenCode.PrefabIconGenerator.LightingAngle.X";
        private const string LightingAngleYPrefKey = "AffenCode.PrefabIconGenerator.LightingAngle.Y";
        private const string LightingAngleZPrefKey = "AffenCode.PrefabIconGenerator.LightingAngle.Z";
        private const string LightingIntensityPrefKey = "AffenCode.PrefabIconGenerator.LightingIntensity.Z";
        private const string CameraDistancePrefKey = "AffenCode.PrefabIconGenerator.CameraDistance";
        private const string CameraFieldOfViewPrefKey = "AffenCode.PrefabIconGenerator.CameraFieldOfView";
        private const string CameraOrthographicPrefKey = "AffenCode.PrefabIconGenerator.CameraOrthographic";
        private const string CameraOrthographicSizePrefKey = "AffenCode.PrefabIconGenerator.CameraOrthographicSize";
        private const string IconWidthPrefKey = "AffenCode.PrefabIconGenerator.IconWidth";
        private const string IconHeightPrefKey = "AffenCode.PrefabIconGenerator.IconHeight";

        private const float Epsilon = 0.00000001f;
        
        private string _destinationFolderPath;
        private GameObject _prefab;
        private Texture2D _preview;

        private bool _cameraFoldout = true;
        private bool _lightingFoldout = true;

        private Vector3 _cameraAnglePrev;
        private Vector3 _cameraAngle = new Vector3(15f, 45f, 0f);

        private Vector3 _lightingAnglePrev;
        private Vector3 _lightingAngle = new Vector3(50f, 30f, 0f);
        
        private float _lightingIntensityPrev;
        private float _lightingIntensity = 1f;

        private Vector3 _cameraOffsetPrev;
        private Vector3 _cameraOffset = new Vector3(0, 0, 0f);
        
        private float _cameraDistancePrev;
        private float _cameraDistance = 5f;
        
        private float _cameraFieldOfViewPrev;
        private float _cameraFieldOfView = 30f;
        
        private bool _cameraOrthographicPrev;
        private bool _cameraOrthographic;
        
        private float _cameraOrthographicSizePrev;
        private float _cameraOrthographicSize = 1f;
        
        private bool _iconFoldout = true;
        
        private int _iconWidthPrev;
        private int _iconWidth = 256;
        
        private int _iconHeightPrev;
        private int _iconHeight = 256;

        private bool _liveUpdate;

        public string PathToIcon => Path.Combine(_destinationFolderPath, $"{_prefab.name}.png");
        
        [MenuItem("Tools/Generate Icon for Prefab")]
        private static void ShowWindow()
        {
            var window = GetWindow<PrefabIconGeneratorEditorWindow>();
            window.minSize = new Vector2(256f, 740f);
            window.maxSize = window.minSize;
        }

        private void Awake()
        {
            if (Selection.activeObject && Selection.activeObject is GameObject gameObject)
            {
                _prefab = gameObject;
            }

            _destinationFolderPath = EditorPrefs.GetString(DestinationFolderPathPrefKey, "Assets/Icons");
            
            var cameraAngleX = EditorPrefs.GetFloat(CameraAngleXPrefKey, 15f);
            var cameraAngleY = EditorPrefs.GetFloat(CameraAngleYPrefKey, 45f);
            var cameraAngleZ = EditorPrefs.GetFloat(CameraAngleZPrefKey, 0);
            _cameraAngle = new Vector3(cameraAngleX, cameraAngleY, cameraAngleZ);
            
            var lightingAngleX = EditorPrefs.GetFloat(LightingAngleXPrefKey, 50f);
            var lightingAngleY = EditorPrefs.GetFloat(LightingAngleYPrefKey, 30f);
            var lightingAngleZ = EditorPrefs.GetFloat(LightingAngleZPrefKey, 0);
            _lightingAngle = new Vector3(lightingAngleX, lightingAngleY, lightingAngleZ);
            
            _lightingIntensity = EditorPrefs.GetFloat(LightingIntensityPrefKey, 1f);
            
            var cameraOffsetX = EditorPrefs.GetFloat(CameraOffsetXPrefKey, 0);
            var cameraOffsetY = EditorPrefs.GetFloat(CameraOffsetYPrefKey, 0);
            var cameraOffsetZ = EditorPrefs.GetFloat(CameraOffsetZPrefKey, 0);
            _cameraOffset = new Vector3(cameraOffsetX, cameraOffsetY, cameraOffsetZ);
            
            _cameraDistance = EditorPrefs.GetFloat(CameraDistancePrefKey, 5f);
            _cameraFieldOfView = EditorPrefs.GetFloat(CameraFieldOfViewPrefKey, 30f);
            _cameraOrthographic = EditorPrefs.GetInt(CameraOrthographicPrefKey, 0) > 0;
            _cameraOrthographicSize = EditorPrefs.GetFloat(CameraOrthographicSizePrefKey, 1f);
            
            _iconWidth = EditorPrefs.GetInt(IconWidthPrefKey, 256);
            _iconHeight = EditorPrefs.GetInt(IconHeightPrefKey, 256);
            
            if (_prefab && File.Exists(PathToIcon))
            {
                _preview = AssetDatabase.LoadAssetAtPath<Texture2D>(PathToIcon);
            }
        }

        private void OnGUI()
        {
            var changed = false;
            
            DrawDestinationFolderPath();
            _prefab = (GameObject) EditorGUILayout.ObjectField(_prefab, typeof(GameObject), true);

            if (!_prefab)
            {
                return;
            }
            
            EditorGUILayout.Space();

            _cameraFoldout = EditorGUILayout.Foldout(_cameraFoldout, "Camera Settings");
            if (_cameraFoldout)
            {
                EditorGUI.indentLevel++;

                _cameraAnglePrev = _cameraAngle;
                _cameraAngle = EditorGUILayout.Vector3Field("Angle", _cameraAngle);
                changed |= _cameraAngle != _cameraAnglePrev;

                _cameraOffsetPrev = _cameraOffset;
                _cameraOffset = EditorGUILayout.Vector3Field("Offset", _cameraOffset);
                changed |= _cameraOffset != _cameraOffsetPrev;
                
                _cameraOrthographicPrev = _cameraOrthographic;
                _cameraOrthographic = EditorGUILayout.Toggle("Orthographic", _cameraOrthographic);
                changed |= _cameraOrthographic != _cameraOrthographicPrev;
                
                if (_cameraOrthographic)
                {
                    _cameraOrthographicSizePrev = _cameraOrthographicSize;
                    _cameraOrthographicSize = EditorGUILayout.FloatField("Orthographic Size", _cameraOrthographicSize);
                    changed |= Mathf.Abs(_cameraOrthographicSize - _cameraOrthographicSizePrev) > Epsilon;
                }
                else
                {
                    _cameraDistancePrev = _cameraDistance;
                    _cameraDistance = EditorGUILayout.FloatField("Distance", _cameraDistance);
                    changed |= Mathf.Abs(_cameraDistance - _cameraDistancePrev) > Epsilon;
                    
                    _cameraFieldOfViewPrev = _cameraFieldOfView;
                    _cameraFieldOfView = EditorGUILayout.FloatField("Field Of View", _cameraFieldOfView);
                    changed |= Mathf.Abs(_cameraFieldOfView - _cameraFieldOfViewPrev) > Epsilon;
                }

                if (GUILayout.Button("Center"))
                {
                    var meshRenderers = _prefab.GetComponentsInChildren<MeshRenderer>();
                    var bounds = new Bounds();
                    var center = Vector3.zero;
                    int count = 0;
                    foreach (var meshRenderer in meshRenderers)
                    {
                        var meshRendererBounds = meshRenderer.bounds;
                        center += meshRendererBounds.center;
                        bounds.Expand(meshRendererBounds.size);
                        count++;
                    }

                    center = (1f / count) * center;
                    center -= _prefab.transform.position;
                    center.x = -center.x;
                    _cameraOffset = center;
                }
                
                EditorGUILayout.Space();
                EditorGUI.indentLevel--;
            }

            _lightingFoldout = EditorGUILayout.Foldout(_lightingFoldout, "Lighting Settings");
            if (_lightingFoldout)
            {
                EditorGUI.indentLevel++;
                
                _lightingAnglePrev = _lightingAngle;
                _lightingAngle = EditorGUILayout.Vector3Field("Angle", _lightingAngle);
                changed |= _lightingAngle != _lightingAnglePrev;
                
                _lightingIntensityPrev = _lightingIntensity;
                _lightingIntensity = EditorGUILayout.FloatField("Intensity", _lightingIntensity);
                changed |= Mathf.Abs(_lightingIntensity - _lightingIntensityPrev) > Epsilon;
                    
                EditorGUILayout.Space();
                EditorGUI.indentLevel--;
            }
            
            _iconFoldout = EditorGUILayout.Foldout(_iconFoldout, "Icon Settings");
            if (_iconFoldout)
            {
                EditorGUI.indentLevel++;
                
                _iconWidthPrev = _iconWidth;
                _iconWidth = EditorGUILayout.IntField("Width", _iconWidth);
                changed |= _iconWidth != _iconWidthPrev;
                
                _iconHeightPrev = _iconHeight;
                _iconHeight = EditorGUILayout.IntField("Height", _iconHeight);
                changed |= _iconHeight != _iconHeightPrev;
                
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            _liveUpdate = EditorGUILayout.Toggle("Live Update", _liveUpdate);

            if (_liveUpdate)
            {
                if (changed)
                {
                    BuildIcon();
                    _preview = AssetDatabase.LoadAssetAtPath<Texture2D>(PathToIcon);
                }
            }
            else
            {
                EditorGUILayout.Space();
            
                if (GUILayout.Button("Generate"))
                {
                    BuildIcon();
                    _preview = AssetDatabase.LoadAssetAtPath<Texture2D>(PathToIcon);
                }
            }
            
            if (_prefab && File.Exists(PathToIcon))
            {
                if (!_preview)
                {
                    _preview = AssetDatabase.LoadAssetAtPath<Texture2D>(PathToIcon);
                }

                EditorGUILayout.ObjectField(_preview, typeof(Texture2D), false);
                var rect = EditorGUILayout.GetControlRect(false, 256f);
                rect.width = 256f;
                EditorGUI.DrawTextureTransparent(rect, _preview);
            }

            if (changed)
            {
                Save();
            }
        }

        private void BuildIcon()
        {
            var iconBuilder = new IconBuilder();
            iconBuilder.Build(new IconBuilderSettings
            {
                CameraOffset = _cameraOffset,
                CameraDistance = _cameraDistance,
                CameraFieldOfView = _cameraFieldOfView,
                CameraAngle = _cameraAngle,
                CameraOrthographic = _cameraOrthographic,
                CameraOrthographicSize = _cameraOrthographicSize,
                LightingAngle = _lightingAngle,
                LightingIntensity = _lightingIntensity,
                Size = new Vector2Int(_iconWidth, _iconHeight),
                DestinationFolderPath = _destinationFolderPath,
                Prefab = _prefab,
                Format = IconFormat.PNG
            });
        }

        private void Save()
        {
            EditorPrefs.SetFloat(CameraOffsetXPrefKey, _cameraOffset.x);
            EditorPrefs.SetFloat(CameraOffsetYPrefKey, _cameraOffset.y);
            EditorPrefs.SetFloat(CameraOffsetZPrefKey, _cameraOffset.z);
            
            EditorPrefs.SetFloat(LightingAngleXPrefKey, _lightingAngle.x);
            EditorPrefs.SetFloat(LightingAngleYPrefKey, _lightingAngle.y);
            EditorPrefs.SetFloat(LightingAngleZPrefKey, _lightingAngle.z);

            EditorPrefs.SetFloat(LightingIntensityPrefKey, _lightingIntensity);
            
            EditorPrefs.SetFloat(CameraAngleXPrefKey, _cameraAngle.x);
            EditorPrefs.SetFloat(CameraAngleYPrefKey, _cameraAngle.y);
            EditorPrefs.SetFloat(CameraAngleZPrefKey, _cameraAngle.z);
            
            EditorPrefs.SetFloat(CameraDistancePrefKey, _cameraDistance);
            EditorPrefs.SetFloat(CameraFieldOfViewPrefKey, _cameraFieldOfView);
            
            EditorPrefs.SetFloat(CameraOrthographicPrefKey, _cameraOrthographic ? 1 : 0);
            EditorPrefs.SetFloat(CameraOrthographicSizePrefKey, _cameraOrthographicSize);
            
            EditorPrefs.SetFloat(IconWidthPrefKey, _iconWidth);
            EditorPrefs.SetFloat(IconHeightPrefKey, _iconHeight);
        }

        private void DrawDestinationFolderPath()
        {
            var prevPath = _destinationFolderPath;
            
            var controlRect = EditorGUILayout.GetControlRect();
            
            var labelRect = new Rect(controlRect)
            {
                size = CalcSize("Destination Folder Path: ", EditorStyles.miniLabel)
            };
            EditorGUI.LabelField(labelRect, "Destination Folder Path: ", EditorStyles.miniLabel);

            var pathRectLabelSize = CalcSize(_destinationFolderPath + " ", EditorStyles.textField);
            var pathRectWidth = Mathf.Min(pathRectLabelSize.x, 256f - CalcSize("...", EditorStyles.miniButton).x);
            var pathRectHeight = pathRectLabelSize.y;
            var pathRect = new Rect(controlRect)
            {
                y = labelRect.height + 5, 
                size = new Vector2(pathRectWidth, pathRectHeight)
            };
            _destinationFolderPath = EditorGUI.TextField(pathRect, _destinationFolderPath);
            
            var buttonRect = new Rect(pathRect.x + pathRect.width, pathRect.y - 1, 22, labelRect.height);
            if (GUI.Button(buttonRect, "...", EditorStyles.miniButton))
            {
                _destinationFolderPath = EditorUtility.OpenFolderPanel("Destination Folder", _destinationFolderPath, "").Trim().TrimEnd('/');

                if (string.IsNullOrEmpty(_destinationFolderPath))
                {
                    _destinationFolderPath = prevPath;
                }
                else
                {
                    if (_destinationFolderPath.IndexOf(UnityProject.Path, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        _destinationFolderPath = _destinationFolderPath.Replace(UnityProject.Path, "").TrimStart('/');
                    }
                    else
                    {
                        _destinationFolderPath = prevPath;
                        EditorUtility.DisplayDialog("Error!", "Object Destination Folder must be in unity project's Assets folder", "OK");
                    }
                }
            }
            
            if (prevPath != _destinationFolderPath)
            {
                EditorPrefs.SetString(DestinationFolderPathPrefKey, _destinationFolderPath);
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
        }
        
        public static Vector2 CalcSize(string label, GUIStyle guiStyle)
        {
            var content = new GUIContent(label);

            return guiStyle.CalcSize(content);
        }
    }
}