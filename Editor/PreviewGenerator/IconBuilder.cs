using System.IO;
using UnityEditor;
using UnityEngine;

namespace AffenCode
{
    public class IconBuilder 
    {
        public void Build(IconBuilderSettings settings)
        {           
            var previewSettings = new PreviewSettings(settings.Size, Vector2Int.one, settings.Format == IconFormat.PNG, settings.Format == IconFormat.JPG);
            var previewGenerator = new PreviewGenerator(settings);
            
            var texture = previewGenerator.Generate(settings.Prefab, previewSettings);
            
            var bytes = previewSettings.GetBytes(texture);

            if (!Directory.Exists(settings.DestinationFolderPath))
            {
                Directory.CreateDirectory(settings.DestinationFolderPath);
            }
            
            var path = Path.Combine(settings.DestinationFolderPath, $"{settings.Prefab.name}.png");
            
            File.WriteAllBytes(path, bytes);
            AssetDatabase.ImportAsset(path);
        }
    }
        
    public struct IconBuilderSettings
    {
        public GameObject Prefab;
        public string DestinationFolderPath;
        public Vector2Int Size;
        public float CameraDistance;
        public float CameraFieldOfView;
        public Vector3 CameraAngle;
        public Vector3 CameraOffset;
        public bool CameraOrthographic;
        public float CameraOrthographicSize;
        public IconFormat Format;
    }

    public enum IconFormat
    {
        JPG,
        PNG
    }
}