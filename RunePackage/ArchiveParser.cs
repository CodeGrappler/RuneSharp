using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ICSharpCode.SharpZipLib.BZip2;
using GisSharpBlog.NetTopologySuite.IO;

namespace RuneSharp.RunePackage
{
    public class ArchiveParser
    {
        byte[] data;
        byte[] finalBuffer;
        List<byte[]> files = new List<byte[]>();
        List<int> identifiers = new List<int>();
        int totalFiles;
        List<int> decompressedSizes = new List<int>();
        List<int> compressedSizes = new List<int>();
        List<int> startOffsets = new List<int>();
        bool compressedAsWhole;
        BigEndianBinaryReader archiveReader;
        public int DecompressedSize, CompressedSize;
        public ArchiveParser(byte[] archive)
        {
            this.data = archive;
            archiveReader = new BigEndianBinaryReader(new MemoryStream(this.data));
            DecompressedSize = archiveReader.ReadUInt24();
            CompressedSize = archiveReader.ReadUInt24();
            if (CompressedSize != DecompressedSize)
            {
                byte[] input = new byte[DecompressedSize - 6];
                Array.Copy(this.data, 6, input, 0, DecompressedSize - 6);
                byte[] dec = new byte[DecompressedSize - 6];
                BZip2.Decompress(new MemoryStream(input), new MemoryStream(dec), true);
                finalBuffer = dec;
                archiveReader.Close();
                archiveReader = new BigEndianBinaryReader(new MemoryStream(finalBuffer));
                compressedAsWhole = true;
            }
            else
            {
                finalBuffer = this.data;
                compressedAsWhole = false;
            }
            totalFiles = archiveReader.ReadUInt16();
            int offset = 8 + totalFiles * 10;
            for (int i = 0; i < totalFiles; i++)
            {
                identifiers.Add(archiveReader.ReadInt32());
                decompressedSizes.Add(archiveReader.ReadUInt24());
                compressedSizes.Add(archiveReader.ReadUInt24());
                startOffsets.Add(offset);
                Logger.Log("Found file: " + identifiers[i] + " at " + offset + ", size: " + compressedSizes[i], LogType.Success);
                offset += compressedSizes[i];
                files.Add(getFileAt(i));
            }
        }
        public byte[] getFileAt(int at)
        {
            byte[] dataBuffer = new byte[decompressedSizes[at]];
            byte[] finalBuffer2 = new byte[compressedSizes[at]];
            if (!compressedAsWhole)
            {
                Array.Copy(finalBuffer, startOffsets[at], finalBuffer2, 0, compressedSizes[at]);
                BZip2.Decompress(new MemoryStream(finalBuffer2), new MemoryStream(dataBuffer), true);
            }
            else
                Array.Copy(finalBuffer, startOffsets[at], dataBuffer, 0, decompressedSizes[at]);
            return dataBuffer;
        }
        public byte[] getFileFromList(int id)
        {
            return files[id];
        }
        public byte[] getFile(int identifier)
        {
            for(int k=0;k<totalFiles;k++)
                if(identifiers[k] == identifier)
                    return getFileAt(k);
            return null;
        }
        public int getIdentifierAt(int at)
        {
            return identifiers[at];
        }
        public int getDecompressedSize(int at)
        {
            return decompressedSizes[at];
        }
        public int getTotalFiles()
        {
            return totalFiles;
        }
        public byte[] getFile(string identStr)
        {
            int id = 0;
            identStr = identStr.ToUpper();
            for (int j = 0; j < identStr.Length; j++)
                id = (id * 61 + identStr[j]) - 32;
            return getFile(id);
        }
        public int getHash(string s)
        {
            int id = 0;
            s = s.ToUpper();
            for (int j = 0; j < s.Length; j++)
                id = (id * 61 + s[j]) - 32;
            return id;
        }
        public static string GetArchiveName(int archive)
        {
            if (archive > ArchiveNames.ArchiveJagNames.Length - 1 || archive < 0)
                return null;
            return ArchiveNames.ArchiveJagNames[archive];
        }
        public string GetArchiveFileName(int archive, int file)
        {
            if (archive > ArchiveNames.ArchiveNamesIndex.Count - 1 || archive < 0)
                return null;
            if (file > ArchiveNames.ArchiveNamesIndex[archive].Length - 1 || archive < 0)
                return null;
            string name = ArchiveNames.ArchiveNamesIndex[archive][file];
            if (name == null)
                name = ""+getIdentifierAt(file);
            return name;
        }
        public static string[] GetArchiveFileList(int archive)
        {
            if (archive > ArchiveNames.ArchiveNamesIndex.Count - 1 || archive < 0)
                return null;
            return ArchiveNames.ArchiveNamesIndex[archive];
        }
        public static void GetNames()
        {
            new ArchiveNames();
        }
    }
    public static class BigEndianBinaryReaderExtensions2
    {
        public static int ReadUInt24(this BigEndianBinaryReader br)
        {
            byte[] data = br.ReadBytes(3);
            //return data[2] + (data[1] << 8) + (data[0] << 16);
            return ((data[0] & 0xff) << 16) + ((data[1] & 0xff) << 8) + (data[2] & 0xff);
        }
        public static int ReadSmart(this BigEndianBinaryReader br, byte[] buffer)
        {
            int i = buffer[br.BaseStream.Position];
            if (i < 128) return br.ReadByte() - 64;
            return br.ReadUInt16() - 49152;
        }
    }
    public class ArchiveNames
    {
        private static string[] Archive1;
        private static string[] Archive2;
        private static string[] Archive3;
        private static string[] Archive4;
        private static string[] Archive5;
        private static string[] Archive6;
        private static string[] Archive7;
        private static string[] Archive8;
        public static string[] ArchiveJagNames = new string[] { "title.jag", "config.jag", "interface.jag", "media.jag", "versionlist.jag", "textures.jag", "wordenc.jag", "sounds.jag" };
        public static List<string[]> ArchiveNamesIndex;
        public ArchiveNames()
        {
            Archive1 = new string[] { "p11_full.dat", "p12_full.dat", "b12_full.dat", "q8_full.dat", "logo.dat", "title.dat", "titlebox.dat", "titlebutton.dat", "runes.dat", "index.dat" };
            Archive2 = new string[] { "npc.dat", "flo.dat", null, "idk.dat", null, "loc.dat", "obj.dat", "loc.idx", null, "npc.idx", "seq.dat", "spotanim.dat", null, "varbit.dat", null, null, "varp.dat", null, null, null, "obj.idx", null, null, null };
            Archive3 = new string[] { "data" };
            Archive4 = new string[] { "backbase1.dat", "backbase2.dat", "backhmid1.dat", "backhmid2.dat", "backleft1.dat", "backleft2.dat", "backright1.dat", "backright2.dat", "backtop1.dat", "backvmid1.dat", "backvmid2.dat", "backvmid3.dat", "mapback.dat", "chatback.dat", "invback.dat", "magicon.dat", "magicoff.dat", "prayeron.dat", "prayeroff.dat", "prayerglow.dat", "wornicons.dat", "sideicons.dat", "compass.dat", "miscgraphics.dat", "miscgraphics2.dat", "miscgraphics3.dat", "staticons.dat", "staticons2.dat", "combaticons.dat", "combaticons2.dat", "combaticons3.dat", "combatboxes.dat", "tradebacking.dat", "hitmarks.dat", "cross.dat", "mapdots.dat", "sworddecor.dat", "redstone1.dat", "redstone2.dat", "redstone3.dat", "leftarrow.dat", "rightarrow.dat", "steelborder.dat", "steelborder2.dat", "scrollbar.dat", "mapscene.dat", "mapfunction.dat", "magicon2.dat", "magicoff2.dat", "mapmarker.dat", "mod_icons.dat", "mapedge.dat", "chest.dat", "coins.dat", "keys.dat", "headicons_pk.dat", "headicons_prayer.dat", "headicons_hint.dat", "overlay_multiway.dat", "tex_brown.dat", "tex_red.dat", "number_button.dat", "index.dat" };
            Archive5 = new string[] { "anim_index", "anim_crc", "model_version", "anim_version", "map_crc", "map_index", "map_version", "midi_crc", "midi_index", "midi_version", "model_crc", "model_index" };
            Archive6 = new string[] { "0.dat", "1.dat", "2.dat", "3.dat", "4.dat", "5.dat", "6.dat", "7.dat", "8.dat", "9.dat", "10.dat", "11.dat", "12.dat", "13.dat", "14.dat", "15.dat", "16.dat", "17.dat", "18.dat", "19.dat", "20.dat", "21.dat", "22.dat", "23.dat", "24.dat", "25.dat", "26.dat", "27.dat", "28.dat", "29.dat", "30.dat", "31.dat", "32.dat", "33.dat", "34.dat", "35.dat", "36.dat", "37.dat", "38.dat", "39.dat", "40.dat", "41.dat", "42.dat", "43.dat", "44.dat", "45.dat", "46.dat", "47.dat", "48.dat", "49.dat", "index.dat" };
            Archive7 = new string[] { "badenc.txt", "fragmentsenc.txt", "tldlist.txt", "domainenc.txt" };
            Archive8 = new string[] { "sounds.dat" };
            ArchiveNamesIndex = new List<string[]>();
            ArchiveNamesIndex.Add(Archive1);
            ArchiveNamesIndex.Add(Archive2);
            ArchiveNamesIndex.Add(Archive3);
            ArchiveNamesIndex.Add(Archive4);
            ArchiveNamesIndex.Add(Archive5);
            ArchiveNamesIndex.Add(Archive6);
            ArchiveNamesIndex.Add(Archive7);
            ArchiveNamesIndex.Add(Archive8);
        }
    }
    public class CensorParser
    {
        public static string[] ParseBadEnc(byte[] badenc)
        {
            BigEndianBinaryReader br = new BigEndianBinaryReader(new MemoryStream(badenc));
            int length = br.ReadInt32();
            List<char[]> bads = new List<char[]>();
            for (int i = 0; i < length; i++)
            {
                char[] val = new char[br.ReadChar()];
                for (int charid = 0; charid < val.Length; charid++)
                    val[charid] = (char)br.ReadChar();
                bads.Add(val);
                byte[,] b1 = new byte[br.ReadByte(), 2];
                for (int l = 0; l < b1.Length/2; l++)
                {
                    b1[l, 0] = (byte)br.ReadByte();
                    b1[l, 1] = (byte)br.ReadByte();
                }
            }
            string[] s = new string[bads.Count];
            for(int c=0;c<s.Length;c++)
                s[c] = new string(bads[c]);
            return s;
        }
        public static string[] ParseDomainEnc(byte[] domainenc)
        {
            BigEndianBinaryReader br = new BigEndianBinaryReader(new MemoryStream(domainenc));
            int length = br.ReadInt32();
            List<char[]> domains = new List<char[]>();
            for (int j = 0; j < length; j++)
            {
                char[] val = new char[br.ReadByte()];
                for (int k = 0; k < val.GetLength(0); k++)
                    val[k] = (char)br.ReadChar();
                domains.Add(val);
            }
            string[] s = new string[domains.Count];
            for (int c = 0; c < s.Length; c++)
                s[c] = new string(domains[c]);
            return s;
        }
        public static int[] ParseFragmentsEnc(byte[] fragenc)
        {
            BigEndianBinaryReader br = new BigEndianBinaryReader(new MemoryStream(fragenc));
            int length = br.ReadInt32();
            int[] fragmentsEnc = new int[length];
            for (int i = 0; i < length; i++)
                fragmentsEnc[i] = br.ReadInt16();
            return fragmentsEnc;
        }
        public static string[] ParseTldList(byte[] tldenc)
        {
            BigEndianBinaryReader br = new BigEndianBinaryReader(new MemoryStream(tldenc));
            int length = br.ReadInt32();
            List<char[]> tldList = new List<char[]>();
            int[] tldArray = new int[length];
            for(int id = 0; id < length; id++)
            {
                tldArray[id] = br.ReadByte();
                char[] tld = new char[br.ReadByte()];
                for (int charID = 0; charID < tld.Length; charID++)
                    tld[charID] = (char)br.ReadByte();
                tldList.Add(tld);
            }
            string[] s = new string[tldList.Count];
            for (int c = 0; c < s.Length; c++)
                s[c] = new string(tldList[c]);
            return s;
        }
    }
}
