using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using GisSharpBlog.NetTopologySuite.IO;
namespace RuneSharp.RunePackage
{
    public class RSFont
    {
        public List<byte[]> glyphPixels;
        public int[] glyphWidth;
        public int[] glyphHeight;
        public int[] horizontalKerning;
        public int[] verticalKerning;
        public int[] charEffectiveWidth;
        public int charHeight;
        private Random random;
        public bool isStrikethrough;
        BigEndianBinaryReader dataReader;
        BigEndianBinaryReader indexReader;

        /// <summary>
        /// Runescape Font. Doesn't work!
        /// </summary>
        public RSFont(bool flag, string fontName, ArchiveParser titleArchive)
        {
            glyphPixels = new List<byte[]>();
            glyphWidth = new int[256];
            glyphHeight = new int[256];
            horizontalKerning = new int[256];
            verticalKerning = new int[256];
            charEffectiveWidth = new int[256];
            random = new Random();
            isStrikethrough = false;
            byte[] data = titleArchive.getFile(fontName + ".dat");
            byte[] index = titleArchive.getFile("index.dat");
            dataReader = new BigEndianBinaryReader(new MemoryStream(data));
            indexReader = new BigEndianBinaryReader(new MemoryStream(index));
            int pos = dataReader.ReadInt16() + 4;
            int k = indexReader.ReadByte();
            if (k > 0)
                pos += 3 * (k - 1);
            indexReader.BaseStream.Position = pos;
            for (int l = 0; l < 256; l++)
            {
                horizontalKerning[l] = indexReader.ReadChar();
                verticalKerning[l] = indexReader.ReadChar();
                int width = glyphWidth[l] = indexReader.ReadInt16();
                int height = glyphWidth[l] = indexReader.ReadInt16();
                int k1 = indexReader.ReadByte();
                int l1 = width * height;
                glyphPixels.Add(new byte[l1]);
                if (k1 == 0)
                    for (int i = 0; i < l1; i++)
                        glyphPixels[l][i] = dataReader.ReadByte();
                else
                    if (k1 == 1)
                        for (int j = 0; j < width; j++)
                            for (int l2 = 0; l2 < height; l2++)
                                glyphPixels[l][j + l2 * width] = dataReader.ReadByte();
                if (height > charHeight && l < 128)
                    charHeight = height;
                horizontalKerning[l] = 1;
                charEffectiveWidth[l] = width + 2;
                int k2 = 0;
                for (int i = height / 7; i < height; i++)
                    k2 += glyphPixels[l][i * width];
                if (k2 <= height / 7)
                {
                    charEffectiveWidth[l]--;
                    horizontalKerning[l] = 0;
                }
                k2 = 0;
                for (int j = height / 7; j < height; j++)
                    k2 += glyphPixels[l][(width - 1) + j * width];
                if (k2 <= height / 7)
                    charEffectiveWidth[l]--;
            }
            if (flag)
                charEffectiveWidth[32] = charEffectiveWidth[73];
            else
                charEffectiveWidth[32] = charEffectiveWidth[105];
        }
        public int getFormattedStringWidth(string s)
        {
            if (s == null) return 0;
            int width = 0;
            for (int k = 0; k < s.Length; k++)
                if (s[k] == '@' && k + 4 < s.Length && s[k + 4] == '@')
                    k += 4;
                else
                    width += charEffectiveWidth[s[k]];
            return width;
        }
        public int getStringWidth(string s)
        {
            if (s == null) return 0;
            int j = 0;
            for (int k = 0; k < s.Length; k++)
                j += charEffectiveWidth[s[k]];
            return j;
        }
        public int getColorByName(string s)
        {
            switch (s)
            {
                case "red":
                    return 0xff0000;
                case "gre":
                    return 0x00ff00;
                case "blu":
                    return 0x0000ff;
                case "yel":
                    return 0xffff00;
                case "cya":
                    return 0x00ffff;
                case "mag":
                    return 0xff00ff;
                case "whi":
                    return 0xffffff;
                case "bla":
                    return 0x000000;
                case "lre":
                    return 0xff9040;
                case "dre":
                    return 0x800000;
                case "dbl":
                    return 0x000080;
                case "or1":
                    return 0xffb000;
                case "or2":
                    return 0xff7000;
                case "or3":
                    return 0xff3000;
                case "gr1":
                    return 0xc0ff00;
                case "gr2":
                    return 0x80ff00;
                case "gr3":
                    return 0x40ff00;
                case "str":
                    isStrikethrough = true;
                    break;
                case "end":
                    isStrikethrough = false;
                    break;
            }
            return -1;
        }
    }
}
