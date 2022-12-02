using UnityEngine;

namespace AffenCode
{
    public class PreviewSettings 
    {
        public int RenderLayer { get; }

        public Vector2Int FrameSize { get; }

        public Vector2Int GridSize { get; }

        public int RenderDepth { get; }

        public TextureFormat TextureFormat { get; }
        
        public bool IsTransparent { get; }

        public bool ExportAsJpg { get; }

        public int FramesCount => GridSize.x * GridSize.y;

        public float AnglesDelta { get; }
        
        public float BoundsSizeToCameraDistanceFactor { get; }

        public Texture2D CreateResultTexture()
        {
            return new Texture2D(
                FrameSize.x * GridSize.x,
                FrameSize.y * GridSize.y,
                TextureFormat,
                false);
        }

        public Texture2D CreateFrameTexture()
        {
            return new Texture2D(
                FrameSize.x,
                FrameSize.y,
                TextureFormat,
                false);
        }

        public byte[] GetBytes(Texture2D texture)
        {
            if (ExportAsJpg)
            {
                return texture.EncodeToJPG();
            }
            else
            {
                return texture.EncodeToPNG();
            }
        }

        public PreviewSettings(Vector2Int frameSize, Vector2Int gridSize, bool isTransparent, bool exportAsJpg)
        {
            RenderLayer = 30;
            FrameSize = frameSize;
            GridSize = gridSize;
            RenderDepth = 16;
            
            AnglesDelta = 360f / FramesCount;
            BoundsSizeToCameraDistanceFactor = (float) frameSize.x / frameSize.y;

            TextureFormat = TextureFormat.RGBA32;
            IsTransparent = isTransparent;
            ExportAsJpg = exportAsJpg;

            if (exportAsJpg)
            {
                IsTransparent = false;
            }
        }
    }
}