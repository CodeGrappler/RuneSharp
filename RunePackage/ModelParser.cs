using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using GisSharpBlog.NetTopologySuite.IO;

namespace RuneSharp.RunePackage
{
    public class RSModel
    {
        private ModelValues model;
        private static int[] colorMap;
        private static Dictionary<int, int> map = new Dictionary<int, int>();
        public int numVertices;
        public int[] verticesX;
        public int[] verticesY;
        public int[] verticesZ;
        public int numTriangles;
        public int[] trianglePoints1;
        public int[] trianglePoints2;
        public int[] trianglePoints3;
        public int[] texturePointers;
        public int[] trianglePriorities;
        public int[] triangleAlphaValues;
        public int[] colorValues;
        public int numTexTriangles;
        public int[] texTrianglePointsX;
        public int[] texTrianglePointsY;
        public int[] texTrianglePointsZ;
        public int[] vertexSkins;
        public int[] triangleSkinValues;

        //I got this from Tom's code, who probably got it from Jagex.
        //I refuse to attempt to rewrite this, it's a mess
        //Luckily it didn't use much of the Java standard library,
        //so it was pretty easy to port.
        public static void genColorMap()
        {
            if (colorMap == null) colorMap = new int[0x10000];
            int j = 0;
            for (int k = 0; k < 512; k++)
            {
                double d1 = (double)(k / 8) / 64D + 0.0078125D;
                double d2 = (double)(k & 7) / 8D + 0.0625D;
                for (int k1 = 0; k1 < 128; k1++)
                {
                    double d3 = (double)k1 / 128D;
                    double d4 = d3;
                    double d5 = d3;
                    double d6 = d3;
                    if (d2 != 0.0D)
                    {
                        double d7;
                        if (d3 < 0.5D)
                            d7 = d3 * (1.0D + d2);
                        else
                            d7 = (d3 + d2) - d3 * d2;
                        double d8 = 2D * d3 - d7;
                        double d9 = d1 + 0.33333333333333331D;
                        if (d9 > 1.0D)
                            d9--;
                        double d10 = d1;
                        double d11 = d1 - 0.33333333333333331D;
                        if (d11 < 0.0D)
                            d11++;
                        if (6D * d9 < 1.0D)
                            d4 = d8 + (d7 - d8) * 6D * d9;
                        else if (2D * d9 < 1.0D)
                            d4 = d7;
                        else if (3D * d9 < 2D)
                            d4 = d8 + (d7 - d8) * (0.66666666666666663D - d9) * 6D;
                        else
                            d4 = d8;
                        if (6D * d10 < 1.0D)
                            d5 = d8 + (d7 - d8) * 6D * d10;
                        else if (2D * d10 < 1.0D)
                            d5 = d7;
                        else if (3D * d10 < 2D)
                            d5 = d8 + (d7 - d8) * (0.66666666666666663D - d10) * 6D;
                        else
                            d5 = d8;
                        if (6D * d11 < 1.0D)
                            d6 = d8 + (d7 - d8) * 6D * d11;
                        else if (2D * d11 < 1.0D)
                            d6 = d7;
                        else if (3D * d11 < 2D)
                            d6 = d8 + (d7 - d8) * (0.66666666666666663D - d11) * 6D;
                        else
                            d6 = d8;
                    }
                    int l1 = (int)(d4 * 256D); // R
                    int i2 = (int)(d5 * 256D); // G
                    int j2 = (int)(d6 * 256D); // B
                    int k2 = (l1 << 16) + (i2 << 8) + j2; // RGB
                    colorMap[j] = k2;
                    map.Add(j++, k2); //Why is this even here?
                }
            }
            //sortMap();
        }
        /*public static void sortMap()
        {
            List<int> mapKeys = new List<int>();
            List<int> mapValues = new List<int>();
            foreach (int i in map.Values)
                mapValues.Add(i);
            foreach (int k in map.Keys)
                mapKeys.Add(k);
            map.Clear();
            int[] values = mapValues.ToArray();
            for (int i = 0; i < values.Length; i++)
            {
                int indexOf = mapValues.IndexOf(values[i]);
                if (values[i] == 0 || indexOf < i) continue;
                map.Add(mapKeys.Find(item => item == indexOf), values[i]);
            }
            return;
        }*/
        public int getColor(Color c)
        {
            int rgbValue = (c.R << 16) + (c.G << 8) + (c.B);
            int current = 0;
            for (int i = 0; i < colorMap.Length; i++)
                if (Math.Abs(colorMap[i] - rgbValue) < Math.Abs(colorMap[current] - rgbValue))
                    current = i;
            return current;
        }
        public int getColor0(Color c)
        {
            int rgbValue = (c.R << 16) + (c.G << 8) + (c.B);
            int x = 0;
            int current = 0;
            int[] valueArray = new int[map.Values.Count];
            map.Values.CopyTo(valueArray, 0);
            while (rgbValue > current && x < map.Count)
            {
                x++;
                current = valueArray[x];
            }
            return x;
        }
        public void readHeader()
        {
            Logger.Log("Reading model header");
            BigEndianBinaryReader headerReader = new BigEndianBinaryReader(new MemoryStream(model.modelData));
            headerReader.BaseStream.Position = model.modelData.Length - 18;
            model.numVertices = headerReader.ReadUInt16();
            model.numTriangles = headerReader.ReadUInt16();
            model.numTexTriangles = headerReader.ReadByte();
            int useTextures = headerReader.ReadByte();
            int useTriPriority = headerReader.ReadByte();
            int useTransparency = headerReader.ReadByte();
            int useTriSkinning = headerReader.ReadByte();
            int useVertSkinning = headerReader.ReadByte();
            int xDataLen = headerReader.ReadUInt16();
            int yDataLen = headerReader.ReadUInt16();
            int zDataLen = headerReader.ReadUInt16();
            int triDataLen = headerReader.ReadUInt16();
            int tmpOffset = 0;
            model.vertexDirectionOffset = tmpOffset;
            tmpOffset += model.numVertices;
            model.triangleTypeOffset = tmpOffset;
            tmpOffset += model.numTriangles;
            model.trianglePriorityOffset = tmpOffset;
            if (useTriPriority == 255)
                tmpOffset += model.numTriangles;
            else
                model.trianglePriorityOffset = -useTriPriority - 1;
            model.triangleSkinOffset = tmpOffset;
            if (useTriSkinning == 1)
                tmpOffset += model.numTriangles;
            else
                model.triangleSkinOffset = -1;
            model.texturePointerOffset = tmpOffset;
            if (useTextures == 1)
                tmpOffset += model.numTriangles;
            else
                model.texturePointerOffset = -1;
            model.vertexSkinOffset = tmpOffset;
            if (useVertSkinning == 1)
                tmpOffset += model.numVertices;
            else
                model.vertexSkinOffset = -1;
            if (useTransparency == 1)
                tmpOffset += model.numTriangles;
            else
                model.triangleAlphaOffset = -1;
            model.triDataOffset = tmpOffset;
            tmpOffset += triDataLen;
            model.colorDataOffset = tmpOffset;
            tmpOffset += model.numTriangles * 2;
            model.uvMapTrianglesOffset = tmpOffset;
            tmpOffset += model.numTexTriangles * 6;
            model.xDataOffset = tmpOffset;
            tmpOffset += xDataLen;
            model.yDataOffset = tmpOffset;
            tmpOffset += yDataLen;
            model.zDataOffset = tmpOffset;
            tmpOffset += zDataLen;
        }

        public RSModel(byte[] modelData)
        {
            if (modelData[0] == 31 && (modelData[1] & 0xff) == 139)
                using (GZipStream gz = new GZipStream(new MemoryStream(modelData), CompressionMode.Decompress))
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
                        modelData = memory.ToArray();
                    }
                }
            model = new ModelValues();
            model.modelData = modelData;
            readHeader();
            numVertices = model.numVertices;
            numTriangles = model.numTriangles;
            numTexTriangles = model.numTexTriangles;
            verticesX = new int[numVertices];
            verticesY = new int[numVertices];
            verticesZ = new int[numVertices];
            trianglePoints1 = new int[numTriangles];
            trianglePoints2 = new int[numTriangles];
            trianglePoints3 = new int[numTriangles];
            texTrianglePointsX = new int[numTexTriangles];
            texTrianglePointsY = new int[numTexTriangles];
            texTrianglePointsZ = new int[numTexTriangles];
            if (model.vertexSkinOffset >= 0)
                vertexSkins = new int[numVertices];
            if (model.texturePointerOffset >= 0)
                texturePointers = new int[numTriangles];
            if (model.trianglePriorityOffset >= 0)
                trianglePriorities = new int[numTriangles];
            if (model.triangleAlphaOffset >= 0)
                triangleAlphaValues = new int[numTriangles];
            if (model.triangleSkinOffset >= 0)
                triangleSkinValues = new int[numTriangles];
            colorValues = new int[numTriangles];
            BigEndianBinaryReader directionReader = new BigEndianBinaryReader(new MemoryStream(model.modelData));
            directionReader.BaseStream.Position = model.vertexDirectionOffset;
            BigEndianBinaryReader xDataReader = new BigEndianBinaryReader(new MemoryStream(model.modelData));
            xDataReader.BaseStream.Position = model.xDataOffset;
            BigEndianBinaryReader yDataReader = new BigEndianBinaryReader(new MemoryStream(model.modelData));
            yDataReader.BaseStream.Position = model.yDataOffset;
            BigEndianBinaryReader zDataReader = new BigEndianBinaryReader(new MemoryStream(model.modelData));
            zDataReader.BaseStream.Position = model.zDataOffset;
            BigEndianBinaryReader skinReader = new BigEndianBinaryReader(new MemoryStream(model.modelData));
            if(vertexSkins != null) skinReader.BaseStream.Position = model.vertexSkinOffset;
            int baseOffsetX = 0;
            int baseOffsetY = 0;
            int baseOffsetZ = 0;
            for (int currentVertex = 0; currentVertex < numVertices; currentVertex++)
            {
                int flag = directionReader.ReadByte();
                int currentOffsetX = 0;
                if ((flag & 1) != 0) 
                    currentOffsetX = xDataReader.ReadSmart(model.modelData);
                int currentOffsetY = 0;
                if ((flag & 2) != 0)
                    currentOffsetY = yDataReader.ReadSmart(model.modelData);
                int currentOffsetZ = 0;
                if ((flag & 4) != 0)
                    currentOffsetZ = zDataReader.ReadSmart(model.modelData);
                verticesX[currentVertex] = baseOffsetX + currentOffsetX;
                verticesY[currentVertex] = baseOffsetY + currentOffsetY;
                verticesZ[currentVertex] = baseOffsetZ + currentOffsetZ;
                baseOffsetX = verticesX[currentVertex];
                baseOffsetY = verticesY[currentVertex];
                baseOffsetZ = verticesZ[currentVertex];
                if (vertexSkins != null)
                    vertexSkins[currentVertex] = skinReader.ReadByte();
            }
            if (vertexSkins != null) 
                directionReader.BaseStream.Position = model.colorDataOffset;
            if(texturePointers != null)
                xDataReader.BaseStream.Position = model.texturePointerOffset;
            if(trianglePriorities != null)
                yDataReader.BaseStream.Position = model.trianglePriorityOffset;
            if(triangleAlphaValues != null)
                zDataReader.BaseStream.Position = model.triangleAlphaOffset;
            if(triangleSkinValues != null)
                skinReader.BaseStream.Position = model.triangleSkinOffset;
            for (int currentTriangle = 0; currentTriangle < numTriangles; currentTriangle++)
            {
                colorValues[currentTriangle] = directionReader.ReadUInt16();
                if (texturePointers != null)
                    texturePointers[currentTriangle] = xDataReader.ReadByte();
                if (trianglePriorities != null)
                    trianglePriorities[currentTriangle] = yDataReader.ReadByte();
                if (triangleAlphaValues != null)
                    triangleAlphaValues[currentTriangle] = zDataReader.ReadByte();
                if (triangleSkinValues != null)
                    triangleSkinValues[currentTriangle] = skinReader.ReadByte();
            }
            directionReader.BaseStream.Position = model.triDataOffset;
            xDataReader.BaseStream.Position = model.triangleTypeOffset;
            int pointOffset1 = 0;
            int pointOffset2 = 0;
            int pointOffset3 = 0;
            int baseValue = 0;
            for (int currentTriangle = 0; currentTriangle < numTriangles; currentTriangle++)
            {
                int type = xDataReader.ReadByte();
                if (type == 1)
                {
                    pointOffset1 = directionReader.ReadSmart(model.modelData) + baseValue;
                    baseValue = pointOffset1;
                    pointOffset2 = directionReader.ReadSmart(model.modelData) + baseValue;
                    baseValue = pointOffset2;
                    pointOffset3 = directionReader.ReadSmart(model.modelData) + baseValue;
                    baseValue = pointOffset3;
                }
                if (type == 2)
                {
                    pointOffset2 = pointOffset3;
                    pointOffset3 = directionReader.ReadSmart(model.modelData) + baseValue;
                    baseValue = pointOffset3;
                }
                if (type == 3)
                {
                    pointOffset1 = pointOffset3;
                    pointOffset3 = directionReader.ReadSmart(model.modelData) + baseValue;
                    baseValue = pointOffset3;
                } 
                if(type == 4)
                {
                    int origPointOffset1 = pointOffset1;
                    pointOffset1 = pointOffset2;
                    pointOffset2 = origPointOffset1;
                    pointOffset3 = directionReader.ReadSmart(model.modelData) + baseValue;
                    baseValue = pointOffset3;
                }
                if (type < 5 && type > 0)
                {
                    trianglePoints1[currentTriangle] = pointOffset1;
                    trianglePoints2[currentTriangle] = pointOffset2;
                    trianglePoints3[currentTriangle] = pointOffset3;
                }
            }
            xDataReader.BaseStream.Position = model.uvMapTrianglesOffset;
            for (int currentTexTriangle = 0; currentTexTriangle < numTexTriangles; currentTexTriangle++)
            {
                texTrianglePointsX[currentTexTriangle] = directionReader.ReadUInt16();
                texTrianglePointsY[currentTexTriangle] = directionReader.ReadUInt16();
                texTrianglePointsZ[currentTexTriangle] = directionReader.ReadUInt16();
            }
        }
        public Color getColor(int color)
        {
            int hsl = colorMap[color];
            if (hsl == 0)
                hsl = 1;
            int red = (hsl >> 16) & 255;
            int green = (hsl >> 8) & 255;
            int blue = (hsl) & 255;
            return Color.FromArgb(red, green, blue);
        }
    }

    public class ModelValues
    {
        public byte[] modelData;
        public int numVertices;
        public int numTriangles;
        public int numTexTriangles;
        public int vertexDirectionOffset;
        public int xDataOffset;
        public int yDataOffset;
        public int zDataOffset;
        public int vertexSkinOffset;
        public int triDataOffset;
        public int triangleTypeOffset;
        public int colorDataOffset;
        public int texturePointerOffset;
        public int trianglePriorityOffset;
        public int triangleAlphaOffset;
        public int triangleSkinOffset;
        public int uvMapTrianglesOffset;
    }
}
