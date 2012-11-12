using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace RuneSharp.RunePackage
{
    public class FileIO
    {
        public static int TotalRead = 0;
        public static int TotalWrite = 0;
        public static int CompleteWrite = 0;
        public static Boolean FileExists(string s)
        {
            return File.Exists(s);
        }
        public static byte[] ReadFile(string s)
        {
            try
            {
                byte[] readBytes;
                FileStream f = File.Open(s, FileMode.Open);
                readBytes = new byte[f.Length];
                f.Read(readBytes, 0, readBytes.Length);
                f.Close();
                TotalRead++;
                return readBytes;
            }
            catch (Exception e)
            {
                Logger.Log("File Read Error: " + e.Message + " on file "+s, LogType.Error);
                return null;
            }
        }
        public static void WriteFile(string s, byte[] writeBytes)
        {
            try
            {
                FileStream f = File.Open(s, FileMode.OpenOrCreate);
                f.Write(writeBytes, 0, writeBytes.Length);
                f.Close();
                TotalWrite++;
                CompleteWrite++;
            }
            catch (Exception e)
            {
                Logger.Log("File Write Error: " + e.Message + " on file " + s, LogType.Error);
                return;
            }
        }
        public static void WriteFile(string s, byte[] writeBytes, int len)
        {
            try
            {
                FileStream f = File.Open(s, FileMode.OpenOrCreate);
                f.Write(writeBytes, 0, len);
                f.Close();
                TotalWrite++;
                CompleteWrite++;
            }
            catch (Exception e)
            {
                Logger.Log("File Write Error: " + e.Message + " on file " + s, LogType.Error);
                return;
            }
        }
    }
}
