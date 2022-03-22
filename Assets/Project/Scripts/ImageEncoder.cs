using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace AsyncReader
{
    public static class ImageConverter
    {
        public static byte[] Encode(Texture2D texture)
        {
            if (texture == null)
            {
                throw new ArgumentNullException();
            }
            
            byte[] data = texture.GetRawTextureData();

            byte[] widthData = BitConverter.GetBytes(texture.width);
            byte[] heightData = BitConverter.GetBytes(texture.height);
            byte[] formatData = BitConverter.GetBytes((int)texture.format);

            int headerSize = widthData.Length + heightData.Length + formatData.Length;
            byte[] result = new byte[headerSize + data.Length];
            
            // Write the header info.
            Array.Copy(widthData, 0, result, 0, widthData.Length);
            Array.Copy(heightData, 0, result, widthData.Length, heightData.Length);
            Array.Copy(formatData, 0, result, widthData.Length + heightData.Length, formatData.Length);

            // Write the data.
            Array.Copy(data, 0, result, headerSize, data.Length);

            return result;
        }

        public static unsafe Texture2D Decode(IntPtr pointer, int fileSize)
        {
            // A header includes 3 int values. Width, Height and Format.
            int headerSize = sizeof(int) * 3;

            byte[] data = new byte[headerSize];
            Marshal.Copy(pointer, data, 0, headerSize);

            int width = BitConverter.ToInt32(data, 0);
            int height = BitConverter.ToInt32(data, 4);
            TextureFormat format = (TextureFormat)BitConverter.ToInt32(data, 8);

            byte* p = (byte*)pointer;
            
            // Proceed the pointer position to under its header.
            p += 12;

            IntPtr texturePointer = (IntPtr)p;

            Texture2D texture = new Texture2D(width, height, format, false);
            try
            {
                texture.LoadRawTextureData(texturePointer, fileSize);
                texture.Apply();
            }
            catch (Exception e)
            {
                Debug.LogError($"[{nameof(ImageConverter)}] Failed to load a texture from a texture pointer. Make sure that the pointer has a header its format.");
                throw;
            }

            return texture;
        }
    }
}