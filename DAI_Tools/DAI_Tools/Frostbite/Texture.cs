using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAI_Tools.Frostbite
{
    public struct DDSPixelFormat
    {
        public int dwSize;
        public int dwFlags;
        public int dwFourCC;
        public int dwRGBBitCount;
        public uint dwRBitMask;
        public uint dwGBitMask;
        public uint dwBBitMask;
        public uint dwABitMask;
    }

    public struct TextureInfo
    {
        public uint pixelFormatID;
        public uint textureWidth;
        public uint textureHeight;
        public uint sizes;
        public List<uint> mipSizes;
        public DDSPixelFormat pixelFormat;
        public uint caps2;
    }

    public class DAITexture
    {
        public static Dictionary<uint, int> PixelFormatTypes = new Dictionary<uint, int>()
        {
            { 0x00, 0x31545844 },
            { 0x01, 0x31545844 },
            { 0x03, 0x35545844 },
            { 0x04, 0x31495441 },
            { 0x10, 0x74 },
            { 0x13, 0x32495441 },
            { 0x14, 0x53354342 },
        };

        public static void SetPixelFormatData(ref TextureInfo t, uint pixelFormatID)
        {
            t.caps2 = 0;
            t.pixelFormat.dwSize = 32;
            t.pixelFormat.dwFlags = 4;
            t.pixelFormat.dwFourCC = 0x31545844;
            t.pixelFormat.dwRGBBitCount = 0;
            t.pixelFormat.dwRBitMask = 0;
            t.pixelFormat.dwGBitMask = 0;
            t.pixelFormat.dwBBitMask = 0;
            t.pixelFormat.dwABitMask = 0;

            if (PixelFormatTypes.ContainsKey(pixelFormatID))
            {
                t.pixelFormat.dwFourCC = PixelFormatTypes[pixelFormatID];
                if (pixelFormatID == 0x01)
                {
                    t.pixelFormat.dwFlags |= 0x01;
                }
            }
            else
            {
                switch (pixelFormatID)
                {
                    case 0x0B:
                    case 0x36:
                        t.pixelFormat.dwFourCC = 0x00;
                        t.pixelFormat.dwRGBBitCount = 0x20;
                        t.pixelFormat.dwRBitMask = 0xFF;
                        t.pixelFormat.dwGBitMask = 0xFF00;
                        t.pixelFormat.dwBBitMask = 0xFF0000;
                        t.pixelFormat.dwABitMask = 0xFF000000;
                        t.pixelFormat.dwFlags = 0x41;
                        if (pixelFormatID == 0x36)
                        {
                            t.caps2 = 0xFE00;
                        }
                        break;
                    case 0x0C:
                        t.pixelFormat.dwFourCC = 0x00;
                        t.pixelFormat.dwRGBBitCount = 0x08;
                        t.pixelFormat.dwABitMask = 0xFF;
                        t.pixelFormat.dwFlags = 0x02;
                        break;
                    case 0x0D:
                        t.pixelFormat.dwFourCC = 0x00;
                        t.pixelFormat.dwRGBBitCount = 0x10;
                        t.pixelFormat.dwRBitMask = 0xFFFF;
                        t.pixelFormat.dwFlags = 0x20000;
                        break;
                }
            }
        }

        public static void WriteTextureHeader(TextureInfo textureInfo, BinaryWriter writer)
        {
            // DDS 4-byte header
            writer.Write(0x20534444);
            // DDS size;
            writer.Write(124);
            // DDS Flags
            writer.Write(0x000A1007);
            // DDS width/height
            writer.Write(textureInfo.textureHeight);
            writer.Write(textureInfo.textureWidth);
            // DDS pitch or linear size (size of first mipmap)
            writer.Write(textureInfo.mipSizes[0]);
            // DDS depth
            writer.Write(0);
            // DDS number of mipmaps
            writer.Write(textureInfo.mipSizes.Count);

            // DDS reserved
            for (int i = 0; i < 11; i++)
            {
                writer.Write(0);
            }

            writer.Write(textureInfo.pixelFormat.dwSize);
            writer.Write(textureInfo.pixelFormat.dwFlags);
            writer.Write(textureInfo.pixelFormat.dwFourCC);
            writer.Write(textureInfo.pixelFormat.dwRGBBitCount);
            writer.Write(textureInfo.pixelFormat.dwRBitMask);
            writer.Write(textureInfo.pixelFormat.dwGBitMask);
            writer.Write(textureInfo.pixelFormat.dwBBitMask);
            writer.Write(textureInfo.pixelFormat.dwABitMask);

            // DDS Caps 1-4
            writer.Write(0);
            writer.Write(textureInfo.caps2);
            writer.Write(0);
            writer.Write(0);
            // DDS Reserved2
            writer.Write(0);
        }
    }
}
