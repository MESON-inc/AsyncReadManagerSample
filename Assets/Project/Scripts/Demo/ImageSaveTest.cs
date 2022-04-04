using System.IO;
using UnityEngine;

namespace AsyncReader.Demo
{
    public static class ImageSaveTest
    {
        public static void Save(Texture2D texture, string path)
        {
            byte[] rawData = texture.GetRawTextureData();
            byte[] encoded = ImageConverter.Encode(rawData, texture.width, texture.height, texture.format);
            File.WriteAllBytes(path, encoded);
        }
    }
}