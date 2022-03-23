using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace AsyncReader
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ImageHeader
    {
        public int width;
        public int height;
        public int formatType;

        public static int Size => sizeof(int) * 3;

        public TextureFormat Format => (TextureFormat)formatType;

        public static ImageHeader Parse(IntPtr dataPointer)
        {
            // A header includes 3 int values. Width, Height and Format.
            int headerSize = sizeof(int) * 3;
            byte[] data = new byte[headerSize];
            Marshal.Copy(dataPointer, data, 0, headerSize);

            int width = BitConverter.ToInt32(data, 0);
            int height = BitConverter.ToInt32(data, 4);
            int formatType = BitConverter.ToInt32(data, 8);

            return new ImageHeader
            {
                width = width,
                height = height,
                formatType = formatType,
            };
        }
    }

    public struct ImageInfo
    {
        public ImageHeader header;
        public IntPtr buffer;
        public int fileSize;
    }

    public static class ImageConverter
    {
        public static byte[] Encode(byte[] data, int width, int height, TextureFormat format)
        {
            if (data == null)
            {
                throw new ArgumentNullException();
            }

            byte[] widthData = BitConverter.GetBytes(width);
            byte[] heightData = BitConverter.GetBytes(height);
            byte[] formatData = BitConverter.GetBytes((int)format);

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

        public static unsafe ImageInfo Decode(IntPtr pointer, int fileSize)
        {
            ImageHeader header = ImageHeader.Parse(pointer);

            byte* p = (byte*)pointer;

            // Proceed the pointer position to under its header.
            p += ImageHeader.Size;

            return new ImageInfo
            {
                header = header,
                buffer = (IntPtr)p,
                fileSize = fileSize,
            };
        }

        public static unsafe ImageInfo Decode(byte[] rawData)
        {
            int fileSize = rawData.Length - ImageHeader.Size;

            fixed (byte* pinned = rawData)
            {
                IntPtr pointer = (IntPtr)pinned;

                return Decode(pointer, fileSize);
            }
        }
    }
}