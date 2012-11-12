using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Text;
using GisSharpBlog.NetTopologySuite.IO;

namespace RuneSharp.RunePackage
{
    public class CacheIndice
    {
        public List<byte[]> files = new List<byte[]>();
        private static byte[] readBuffer = new byte[520];
        private BigEndianBinaryReader dataReader;
        private BigEndianBinaryReader indexReader;
        int cacheNum;
        int maxFileSize;
        public CacheIndice(ref BigEndianBinaryReader dataFile, ref BigEndianBinaryReader indexFile, int cacheNum)
        {
            maxFileSize = 6500000;
            this.cacheNum = cacheNum;
            this.dataReader = dataFile;
            this.indexReader = indexFile;
            for (int i = 0; i < indexFile.BaseStream.Length / 6; i++)
            {
                Logger.Log("Getting file: " + i, LogType.Message);
                byte[] data = getFile(i);
                if(data != null)
                    Logger.Log("Found file: " + i + ", Length: " + data.Length + " bytes in cache "+cacheNum, LogType.Success);
                files.Add(data);
            }
        }
        public int getNumFiles()
        {
            return files.Count;
        }
        public byte[] getFile(int fileNumber)
        {
            this.indexReader.Seek(fileNumber * 6);
            int readCycle;
            for (int totalRead = 0; totalRead < 6; totalRead += readCycle)
            {
                readCycle = indexReader.Read(readBuffer, totalRead, 6 - totalRead);
                if (readCycle == -1)
                    Logger.Log("Error reading cache file", LogType.Error);
            }
            int totalFileSize = ((readBuffer[0] & 0xff) << 16) + ((readBuffer[1] & 0xff) << 8) + (readBuffer[2] & 0xff);
            int nextSectorID = ((readBuffer[3] & 0xff) << 16) + ((readBuffer[4] & 0xff) << 8) + (readBuffer[5] & 0xff);
            if (totalFileSize < 0 || totalFileSize > maxFileSize)
                Logger.Log("File too large or too small!", LogType.Error);
            if (nextSectorID <= 0 || nextSectorID > dataReader.BaseStream.Length / 520L)
                Logger.Log("Sector " + nextSectorID + " exceeds file!", LogType.Error);
            byte[] fileBuffer = new byte[totalFileSize];
            int dataRead = 0;
            int expectedFileNum = 0;
            while(totalFileSize > dataRead)
            {
                if(nextSectorID == 0)
                    Logger.Log("Invalid Sector ID: "+nextSectorID, LogType.Error);
                this.dataReader.Seek(nextSectorID*520);
                int totalRead = 0;
                int amountToRead = totalFileSize - dataRead;
                if (amountToRead > 512)
                    amountToRead = 512;
                for (; totalRead < amountToRead + 8; totalRead += readCycle)
                {
                    readCycle = dataReader.Read(readBuffer, totalRead, (amountToRead + 8) - totalRead);
                    if (readCycle == -1)
                        Logger.Log("Error reading file", LogType.Error);
                }
                int nextSectorsFileNum = ((readBuffer[0] & 0xff) << 8) + (readBuffer[1] & 0xff);
                int filePartitionNum = ((readBuffer[2] & 0xff) << 8) + (readBuffer[3] & 0xff);
                int nextSectorsID = ((readBuffer[4] & 0xff) << 16) + ((readBuffer[5] & 0xff) << 8) + (readBuffer[6] & 0xff);
                int nextSectorsCacheNum = readBuffer[7] & 0xff;
                if (nextSectorsFileNum != fileNumber)
                    Logger.Log("Sector file num didn't match expected result!", LogType.Error);
                else if (filePartitionNum != expectedFileNum)
                    Logger.Log("Sector file part number didn't match expected result!", LogType.Error);
                else if (nextSectorsCacheNum != cacheNum)
                    Logger.Log("Sector cache number didn't match expected result!", LogType.Error);
                if (nextSectorsID < 0 || nextSectorsID > dataReader.BaseStream.Length / 520)
                    Logger.Log("Sector exceeds cache limits!", LogType.Error);
                for (int c = 0; c < amountToRead; c++)
                    fileBuffer[dataRead++] = readBuffer[c + 8];
                nextSectorID = nextSectorsID;
                expectedFileNum++;
            }
            if (fileBuffer.Length == 0) return null;
            return fileBuffer;
        }
    }
    public static class BigEndianBinaryReaderExtensions
    {
        public static void Seek(this BigEndianBinaryReader br, int pos)
        {
            if (pos < 0 || pos > 0x3c00000)
            {
                Logger.Log("Bad Seek: at pos " + pos, LogType.Error);
                pos = 0x3c00000;
                try { Thread.Sleep(1000); }
                catch (Exception ignored) { }
            }
            br.BaseStream.Position = 0;
            br.BaseStream.Seek(pos, SeekOrigin.Current);
        }
    }
}
