using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace RuneSharp.RunePackage
{
    public class OBJExporter
    {
        public static byte[] ExportOBJ(RSModel m, string mtl = null)
        {
            List<string> exportLines = new List<string>();
            exportLines.Add("# Runescape Model Format");
            exportLines.Add("# Exported to OBJ with RunePackage");
            if (mtl != null)
                exportLines.Add("mtllib " + mtl);
            for (int i = 0; i < m.numVertices; i++) // Vertices
                exportLines.Add("v " + m.verticesX[i] + " " + m.verticesY[i] + " " + m.verticesZ[i]);
            exportLines.Add("o runescapemodel");
            for (int i = 0; i < m.numTriangles; i++) // Faces
            {
                //exportLines.Add("g col" + i);
                exportLines.Add("usemtl color" + i);
                exportLines.Add("f " + (m.trianglePoints1[i] + 1) + " " + (m.trianglePoints2[i] + 1) + " " + (m.trianglePoints3[i] + 1));
                exportLines.Add("");
            }
            string dataTotal = "";
            foreach(string s in exportLines)
                dataTotal += s + "\n";
            return Encoding.UTF8.GetBytes(dataTotal.TrimEnd('\n'));
        }
        public static byte[] ExportMTL(RSModel m)
        {
            List<string> exportLines = new List<string>();
            string dataTotal = "";
            for (int i = 0; i < m.colorValues.Length; i++)
            {
                exportLines.Add("newmtl color" + i);
                System.Drawing.Color c = m.getColor(i);
                double r = Math.Round(c.R / 255.0, 3);
                double g = Math.Round(c.G / 255.0, 3);
                double b = Math.Round(c.B / 255.0, 3);
                exportLines.Add("\tKd " + r + " " + g + " " + b);
                if (m.triangleAlphaValues != null)
                    exportLines.Add("\td " + (255 - (m.triangleAlphaValues[i] / 255.0)));
            }
            foreach (string s in exportLines)
                dataTotal += s + "\n";
            return Encoding.UTF8.GetBytes(dataTotal.TrimEnd('\n'));
        }
    }
}
