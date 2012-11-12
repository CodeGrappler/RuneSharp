using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GisSharpBlog.NetTopologySuite.IO;

namespace RuneSharp.RunePackage
{
    public class CacheReader
    {
        public static string CacheDir = "cache/";
        int uid;
        BigEndianBinaryReader cacheReader;
        FileStream cacheStream;
        FileStream[] cacheIndexStreams;
        BigEndianBinaryReader[] cacheIndexReaders;
        List<CacheIndice> cacheIndices;
        public CacheReader(int cacheNum=-1)
        {
            if (!File.Exists(CacheDir+"main_file_cache.dat"))
                Logger.Log("Couldn't find cache file: "+CacheDir+"main_file_cache.dat", LogType.Fatal);
            cacheStream = File.Open(CacheDir + "main_file_cache.dat", FileMode.Open);
            cacheReader = new BigEndianBinaryReader(cacheStream);
            uid = (int)(new Random().NextDouble() * 99999999D);
            cacheIndexStreams = new FileStream[5];
            cacheIndexReaders = new BigEndianBinaryReader[5];
            cacheIndices = new List<CacheIndice>();
            if (cacheNum == -1)
                for (int i = 0; i < 5; i++)
                    ReadCache(i);
            else if (cacheNum > -1 && cacheNum < 5)
                ReadCache(cacheNum);
            else
                throw new Exception("Invalid cache num!");
        }
        public void ReadCache(int cacheNum)
        {
            if (!File.Exists(CacheDir + "main_file_cache.idx" + cacheNum))
                Logger.Log("Couldn't find index file #" + cacheNum + " in " + CacheDir, LogType.Fatal);
            cacheIndexStreams[cacheNum] = File.Open(CacheDir + "main_file_cache.idx" + cacheNum, FileMode.Open);
            cacheIndexReaders[cacheNum] = new BigEndianBinaryReader(cacheIndexStreams[cacheNum]);
            Logger.Log("Reading indice #" + cacheNum);
            cacheIndices.Add(new CacheIndice(ref cacheReader, ref cacheIndexReaders[cacheNum], cacheNum + 1));
            Logger.Log("Read indice #" + cacheNum, LogType.Success);
        }
        public CacheIndice getArchiveCache() { return cacheIndices[0]; }
        public CacheIndice getModelCache() { return cacheIndices[1]; }
        public CacheIndice getAnimationCache() { return cacheIndices[2]; }
        public CacheIndice getAudioCache() { return cacheIndices[3]; }
        public CacheIndice getMapCache() { return cacheIndices[4]; }
    }
}
