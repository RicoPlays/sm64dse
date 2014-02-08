using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using OpenTK;

namespace SM64DSe.Exporters
{
    public static class KCL_Exporter
    {
        public static void ExportKCLToOBJ(List<ColFace> planes, List<Color> colours)
        {
            string output = "";
            string mtllib = "";
            CultureInfo usa = new CultureInfo("en-US");//Need to ensure 1.23 not 1,23 when floatVar.ToString() used - use floatVar.ToString(usa)
            SaveFileDialog saveOBJ = new SaveFileDialog();
            saveOBJ.FileName = "CollisionMap";//Default name
            saveOBJ.DefaultExt = ".obj";//Default file extension
            saveOBJ.Filter = "Wavefront OBJ (.obj)|*.obj";//Filter by .obj
            if (saveOBJ.ShowDialog() == DialogResult.Cancel)
                return;
            StreamWriter outfile = new StreamWriter(saveOBJ.FileName);
            StreamWriter outMTL = new StreamWriter(saveOBJ.FileName.Substring(0, saveOBJ.FileName.Length - 4) + ".mtl");
            string dir = Path.GetDirectoryName(saveOBJ.FileName);
            string filename = Path.GetFileNameWithoutExtension(saveOBJ.FileName);

            List<int> types = new List<int>();
            for (int i = 0; i < planes.Count; i++)
            {
                if (!types.Contains(planes[i].type))
                    types.Add(planes[i].type);
            }

            output += Program.AppTitle + " " + Program.AppVersion + " " + Program.AppDate + "\n\n";

            output += "mtllib " + filename + ".mtl" + "\n\n";

            for (int i = 0; i < types.Count; i++)
            {
                mtllib += "newmtl material_" + types[i] + "\n";
                mtllib += "Ka 1 1 1\n";
                mtllib += "Kd " + (colours[types[i]].R / 255.0f).ToString(usa) + " " + (colours[types[i]].G / 255.0f).ToString(usa) + " " +
                    (colours[types[i]].B / 255.0f).ToString(usa) + "\n";
                mtllib += "Ks 1 1 1\n";
                mtllib += "Ns 1\n";
                mtllib += "d 1\n";
                mtllib += "illum 2\n\n";
            }

            // Write all vertices and normals to file
            List<Vector3> outputVertices = new List<Vector3>();
            List<Vector3> outputNormals = new List<Vector3>();
            for (int i = 0; i < planes.Count; i++)
            {
                if (!outputVertices.Contains(planes[i].point1))
                    outputVertices.Add(planes[i].point1);
                if (!outputVertices.Contains(planes[i].point2))
                    outputVertices.Add(planes[i].point2);
                if (!outputVertices.Contains(planes[i].point3))
                    outputVertices.Add(planes[i].point3);

                if (!outputNormals.Contains(planes[i].normal))
                    outputNormals.Add(planes[i].normal);
            }
            for (int i = 0; i < outputVertices.Count; i++)
                output += "v " + outputVertices[i].X.ToString(usa) + " " + outputVertices[i].Y.ToString(usa) + " " + outputVertices[i].Z.ToString(usa) + "\n";

            for (int i = 0; i < outputNormals.Count; i++)
                output += "vn " + outputNormals[i].X.ToString(usa) + " " + outputNormals[i].Y.ToString(usa) + " " +
                    outputNormals[i].Z.ToString(usa) + "\n";

            // Write all faces to file per-collision type (indices start at 1)
            for (int i = 0; i < types.Count; i++)
            {
                output += "# Collision Type " + i + "\n";
                output += "usemtl material_" + i + "\n";
                for (int j = 0; j < planes.Count; j++)
                {
                    if (planes[j].type == types[i])
                    {
                        output += "f " + (outputVertices.IndexOf(planes[j].point1) + 1) + "//" + (outputNormals.IndexOf(planes[j].normal) + 1) + " " +
                            (outputVertices.IndexOf(planes[j].point2) + 1) + "//" + (outputNormals.IndexOf(planes[j].normal) + 1) + " " +
                            (outputVertices.IndexOf(planes[j].point3) + 1) + "//" + (outputNormals.IndexOf(planes[j].normal) + 1) + "\n";
                    }
                }
            }

            // Save file
            outfile.Write(output);
            outfile.Close();
            outMTL.Write(mtllib);
            outMTL.Close();
        }
    }
}
