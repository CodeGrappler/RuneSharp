using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;

namespace RuneSharp.RunePackage
{
    public class MidiParser
    {
        public List<byte[]> files;
        public MidiParser(CacheIndice midiIndice)
        {
            files = new List<byte[]>();
            foreach (byte[] file in midiIndice.files)
                if(file != null)
                    files.Add(file);
        }
        //What is all this junk for? I found someone else's parsing code that said that the 6-9th bytes are the length of the file,
        //And if the file starts with 2, then it's a midi and I should start the GZipping at 9 bytes from the start. I tried it,
        //and it worked - once. Then, it didn't. The input streams had the GZip magic numbers (31 and 139) at the beginning. 
        //So I added a check for both of these, just in case.
        public byte[] getFile(int index)
        {
            byte[] midiData = files[index];
            byte[] returnData = null;
            int length = 0;
            if(midiData[0] != 31)
                length = ((midiData[5] & 0xff) << 24) + ((midiData[6] & 0xff) << 16) + ((midiData[7] & 0xff) << 8) + midiData[8];
            if (length > 0 || midiData[0] == 31)
            {
                byte[] fileData = new byte[midiData.Length - 9];
                if (midiData[0] == 2 || midiData[0] == 31)
                {
                    if (midiData[0] == 2)
                        Array.Copy(midiData, 9, fileData, 0, midiData.Length - 9);
                    else
                        fileData = midiData;
                    using (GZipStream gz = new GZipStream(new MemoryStream(fileData), CompressionMode.Decompress))
                    {
                        int size = 4096;
                        byte[] buffer = new byte[size];
                        using (MemoryStream memory = new MemoryStream())
                        {
                            int count = 0;
                            do
                            {
                                count = gz.Read(buffer, 0, size);
                                if (count > 0)
                                    memory.Write(buffer, 0, count);
                            }
                            while (count > 0);
                            returnData = memory.ToArray();
                        }
                    }
                }
                else if (fileData[0] == 1) { Logger.Log("File isn't a midi!", LogType.Error); }
            }
            return returnData;
        }
    }
}
