using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using GisSharpBlog.NetTopologySuite.IO;

namespace RuneSharp.RunePackage
{
    public class ImageArchive
    {
        private static string[] knownExceptions = new string[] { "index.dat", "title.dat" }; //skip these files
        private ArchiveParser jagArchive;
        List<ImageGroup> images = new List<ImageGroup>();

        public ImageArchive(ref ArchiveParser jagArchive)
        {
            this.jagArchive = jagArchive;
            byte[] indexData = jagArchive.getFile("index.dat");
            for (int i = 0; i < jagArchive.getTotalFiles(); i++)
            {
                int hash = jagArchive.getIdentifierAt(i);
                if (validImage(hash))
                    images.Add(new ImageGroup(indexData, jagArchive.getFileAt(i), true));
            }
        }
        public bool validImage(int hash)
        {
            foreach(string s in knownExceptions)
                if(hash == jagArchive.getHash(s))
                    return false;
            return true;
        }
        public ImageGroup getImageGroup(int i)
        {
            return images[i];
        }
    }
    public class ImageGroup
    {
        public int maxWidth, maxHeight;
        private int[] colorMap;
        private List<ImageBean> images = new List<ImageBean>();
        private int colorCount;
        private BigEndianBinaryReader indexReader;
        private BigEndianBinaryReader dataReader;
        private int indexOffset = 0;
        private int packType = 0;
        public ImageGroup()
        {
            maxHeight = 0;
            maxWidth = 0;
            colorCount = 2;
            colorMap = new int[] { 0, 1 };
        }
        public ImageGroup(byte[] index, byte[] data, bool unpack)
        {
            indexReader = new BigEndianBinaryReader(new MemoryStream(index));
            dataReader = new BigEndianBinaryReader(new MemoryStream(data));
            indexReader.BaseStream.Position = dataReader.ReadUInt16();
            indexOffset = (int)indexReader.BaseStream.Position;
            maxWidth = indexReader.ReadUInt16();
            maxHeight = indexReader.ReadUInt16();
            colorCount = indexReader.ReadByte();
            colorMap = new int[colorCount];
            for (int x = 0; x < colorCount - 1; x++)
            {
                colorMap[x + 1] = indexReader.ReadUInt24();
                if (colorMap[x + 1] == 0)
                    colorMap[x + 1] = 1;
            }
            if(unpack)
                unpackImages();
        }
        public void unpackImages()
        {
            int origIndexOffset = (int)indexReader.BaseStream.Position;
            int origDataOffset = (int)dataReader.BaseStream.Position;
            while (dataReader.BaseStream.Position < dataReader.BaseStream.Length)
            {
                int drawOffsetX = indexReader.ReadByte();
                int drawOffsetY = indexReader.ReadByte();
                int width = indexReader.ReadUInt16();
                int height = indexReader.ReadUInt16();
                packType = indexReader.ReadByte();
                int numPixels = width * height;
                int[] pixels = new int[numPixels];
                if (packType == 0)
                    for (int x = 0; x < numPixels; x++)
                        pixels[x] = colorMap[dataReader.ReadByte()];
                else if (packType == 1)
                    for (int x = 0; x < width; x++)
                        for (int y = 0; y < height; y++)
                            pixels[x + y * width] = colorMap[dataReader.ReadByte()];
                images.Add(new ImageBean(drawOffsetX, drawOffsetY, width, height, pixels));
            }
            indexReader.BaseStream.Position = origIndexOffset;
            dataReader.BaseStream.Position = origDataOffset;
        }
        public Image getImage(int i)
        {
            ImageBean b = images[i];
            Bitmap img = new Bitmap(b.width, b.height);
            int[] pixels = b.pixels;
            for(int x=0;x<img.Width;x++)
                for (int y = 0; y < img.Height; y++)
                {
                    int rgb = pixels[x + y * img.Width];
                    if (rgb == 0)
                        rgb = 0xFF00FF;
                    img.SetPixel(x, y, toRGB(rgb));
                }
            return img;
        }
        public Color toRGB(int rgb)
        {
            int red = (rgb >> 16) & 255;
            int green = (rgb >> 8) & 255;
            int blue = (rgb) & 255;
            return Color.FromArgb(red, green, blue);
        }
    }
    public class ImageBean
    {
        public int drawOffsetX, drawOffsetY;
        public int width, height;
        public int[] pixels;
        public ImageBean(int drawOffsetX, int drawOffsetY, int width, int height, int[] pixels)
        {
            this.drawOffsetX = drawOffsetX;
            this.drawOffsetY = drawOffsetY;
            this.width = width;
            this.height = height;
            this.pixels = pixels;
        }
    }
}
