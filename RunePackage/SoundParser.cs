using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using GisSharpBlog.NetTopologySuite.IO;

namespace RuneSharp.RunePackage
{
    //NOT DONE!
    public class SoundParser
    {
        public static int[] anIntArray326;
        public static bool aBoolean321;
        public static bool aBoolean322;
        public static bool aBoolean323 = true;
        public static int anInt324;
        public static int anInt330;
        public static int anInt331; 
        public static void ParseSounds(int flag, byte[] data)
        {
            byte[] sampleData = new byte[441000]; //aByteArray327
            //samples per second of the sound.
            RSSound[] sounds = new RSSound[0x10000];
            if (flag != 0)
                aBoolean322 = !aBoolean322; //what does this do?
            BigEndianBinaryReader dataReader = new BigEndianBinaryReader(new MemoryStream(data));
            
            while (true)
            {
                //class30_sub2_sub2 is the reader for the sound file
                //class6 - what does it do?
                int soundID = dataReader.ReadUInt16(); //is this really the sound ID? 
                if (soundID == 65535)
                    break;
                sounds[soundID] = new RSSound();
                for (int i = 0; i < 10; i++)
                {
                    int j = dataReader.ReadByte();
                    if (j != 0)
                    {
                        sounds[soundID].anInt1406 -= 1;
                        //method169 - gets true and stream reader
                        //creates a class29 which it sends the same
                        sounds[soundID].anInt540 = dataReader.ReadByte();
                        sounds[soundID].anInt538 = dataReader.ReadInt32();
                        sounds[soundID].anInt539 = dataReader.ReadInt32();
                        //method326 gets -112 and the reader
                        //if -112 doesn't equal this.abyte532, this.aBoolean533 is inverted
                        //but this.aByte532 always equals -112
                        sounds[soundID].anInt535 = dataReader.ReadByte();
                        sounds[soundID].anIntArray536 = new int[sounds[soundID].anInt535];
                        sounds[soundID].anIntArray537 = new int[sounds[soundID].anInt535];
                        for (int k = 0; k < sounds[soundID].anInt535; k++)
                        {
                            sounds[soundID].anIntArray536[k] = dataReader.ReadUInt16();
                            sounds[soundID].anIntArray537[k] = dataReader.ReadUInt16();
                        }
                        //then another Class29 is created and the same thing is done again?
                        RSSound tempSound = new RSSound();
                        tempSound.anInt535 = dataReader.ReadByte();
                        tempSound.anIntArray536 = new int[tempSound.anInt535];
                        tempSound.anIntArray537 = new int[tempSound.anInt535];
                        for (int k = 0; k < tempSound.anInt535; k++)
                        {
                            tempSound.anIntArray536[k] = dataReader.ReadUInt16();
                            tempSound.anIntArray537[k] = dataReader.ReadUInt16();
                        }
                        int uFlag = dataReader.ReadByte();
                        if (uFlag != 0)
                        {
                            sounds[soundID].anInt1406 -= 1; //I don't think this is supposed to be in the sound class
                        }
                    }
                }
                anInt330 = dataReader.ReadUInt16();
                anInt331 = dataReader.ReadUInt16();
            }

        }
    }

    public class RSSound
    {
        public int anInt1389;
        public int anInt1390;
        public byte aByte1391;
        public int anInt1392;
        public int anInt1393;
        public byte aByte1394;
        public int anInt1395;
        public bool aBoolean1396;
        public int anInt1397;
        public sbyte aByte1398;
        public byte aByte1399;
        public byte aByte1400;
        public bool aBoolean1401;
        public int anInt1402;
        public bool aBoolean1403;
        public bool aBoolean1404;
        public byte[] sampleData; //aByteArray1405
        public int anInt1406;
        public int anInt1407;
        public bool aBoolean97;
        public int[] anIntArray106;
        public int[] anIntArray107;
        public int[] anIntArray108;
        public int anInt110;
        public int anInt113;
        public int anInt540;
        public int anInt538;
        public int anInt539;
        public int anInt535;
        public int[] anIntArray536;
        public int[] anIntArray537;

        public RSSound()
        {
            this.anInt1389 = 891;
            this.anInt1390 = 9;
            this.aByte1391 = 14;
            this.anInt1392 = -29508;
            this.anInt1393 = 881;
            this.aByte1394 = 8;
            this.anInt1395 = 657;
            this.aBoolean1396 = false;
            this.anInt1397 = -715;
            this.aByte1398 = -57;
            this.aByte1399 = 108;
            this.aByte1400 = 3;
            this.aBoolean1401 = false;
            this.anInt1402 = -373;
            this.aBoolean1403 = false;
            this.aBoolean1404 = true;
            this.anInt1406 = 0;
            this.aBoolean97 = true;
            this.anIntArray106 = new int[5];
            this.anIntArray107 = new int[5];
            this.anIntArray108 = new int[5];
            this.anInt110 = 100;
            this.anInt113 = 500;
        }
    }
}
