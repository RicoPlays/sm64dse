using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System.Globalization;
using OpenTK;

namespace SM64DSe.ImportExport.Writers.ExternalWriters
{
    public class OBJWriter : AbstractModelWriter
    {
        public OBJWriter(ModelBase model, string modelFileName) :
            base(model, modelFileName)
        {
            StringBuilder output = new StringBuilder();
            StringBuilder mtllib = new StringBuilder();
            StreamWriter outfile = new StreamWriter(modelFileName);
            StreamWriter outMTL = new StreamWriter(modelFileName.Substring(0, modelFileName.Length - 4) + ".mtl");
            string dir = Path.GetDirectoryName(modelFileName);
            string baseFileName = Path.GetFileNameWithoutExtension(modelFileName);
            List<BMD.Texture> textures = new List<BMD.Texture>();
            output.Append("#" + Program.AppTitle + " " + Program.AppVersion + " " + Program.AppDate + "\n\n");
            output.Append("mtllib " + baseFileName + ".mtl" + "\n\n"); // Specify name of material library
            output.Append("bonelib " + baseFileName + ".bones" + "\n\n"); // Specify name of bones list

            // Write mtllib
            foreach (ModelBase.MaterialDef material in model.m_Materials.Values)
            {
                //For every texture,
                string textureName = material.m_DiffuseMapName;
                //Create new material
                mtllib.Append("newmtl " /*+ ((i * 2) + j)*/ + material.m_ID + "\n");
                //Specify ambient colour - RGB 0-1
                mtllib.Append("Ka " + (material.m_AmbientColour.R / 255.0f).ToString(usa) +
                    " " + (material.m_AmbientColour.G / 255.0f).ToString(usa) +
                    " " + (material.m_AmbientColour.B / 255.0f).ToString(usa) + "\n");
                //Specify diffuse colour - RGB 0-1
                mtllib.Append("Kd " + (material.m_DiffuseColour.R / 255.0f).ToString(usa) +
                    " " + (material.m_DiffuseColour.G / 255.0f).ToString(usa) +
                    " " + (material.m_DiffuseColour.B / 255.0f).ToString(usa) + "\n");
                //Specify specular colour - RGB 0-1
                mtllib.Append("Ks " + (material.m_SpecularColour.R / 255.0f).ToString(usa) +
                    " " + (material.m_SpecularColour.G / 255.0f).ToString(usa) +
                    " " + (material.m_SpecularColour.B / 255.0f).ToString(usa) + "\n");
                //Specify specular colour co-efficient - RGB 0-1
                //mtllib += "Ns " + material.m_SpeEmiColors.ToString(usa) + "\n";
                //Specify transparency - RGB Alpha channel 0-1
                mtllib.Append("d " + (material.m_Opacity / 255.0f).ToString(usa) + "\n");
                //Specify texture type 0 - 10
                //uint textype = (currentTexture.m_Params >> 26) & 0x7;
                mtllib.Append("illum 2\n");
                if (textureName != null)
                {
                    //Specify name of texture image
                    mtllib.Append("map_Kd " + textureName + ".png" + "\n\n");
                    ExportTextureToPNG(dir, textureName, model.m_ConvertedTexturesBitmap[textureName]);
                }
                else
                    mtllib.Append("\n\n");
            }

            WriteBonesFileOBJ(modelFileName.Substring(0, modelFileName.Length - 4) + ".bones", model.m_BoneTree);

            // Write each bone to file as a separate mesh/object using o command
            List<Vector3> verts = new List<Vector3>();
            List<Vector2> textureCoords = new List<Vector2>();
            List<Color> vertexColours = new List<Color>();
            foreach (ModelBase.BoneDef bone in model.m_BoneTree)
            {
                output.Append("o " + bone.m_ID + "\n");

                // Get a list of all verices and texture co-ordinates for the current bone
                List<Vector3> vertsCurBone = new List<Vector3>();
                List<Vector2> textureCoordsCurBone = new List<Vector2>();
                List<Color> vertexColoursCurBone = new List<Color>();

                foreach (ModelBase.GeometryDef geometry in bone.m_Geometries.Values)
                {
                    foreach (ModelBase.PolyListDef polyList in geometry.m_PolyLists.Values)
                    {
                        foreach (ModelBase.FaceDef face in polyList.m_Faces)
                        {
                            for (int m = 0; m < face.m_NumVertices; m++)
                            {
                                if (!vertsCurBone.Contains(face.m_Vertices[m]))
                                {
                                    vertsCurBone.Add(face.m_Vertices[m]);
                                    verts.Add(face.m_Vertices[m]);
                                }
                                if (face.m_TextureCoordinates[m] != null && !textureCoordsCurBone.Contains((Vector2)face.m_TextureCoordinates[m]))
                                {
                                    textureCoordsCurBone.Add((Vector2)face.m_TextureCoordinates[m]);
                                    textureCoords.Add((Vector2)face.m_TextureCoordinates[m]);
                                }
                                if (face.m_VertexColours[m] != null && !vertexColoursCurBone.Contains((Color)face.m_VertexColours[m]))
                                {
                                    vertexColoursCurBone.Add((Color)face.m_VertexColours[m]);
                                    vertexColours.Add((Color)face.m_VertexColours[m]);
                                }
                            }
                        }
                    }
                }

                // Print a list of all vertices, texture co-ordinates and vertex colours
                foreach (Vector3 vert in vertsCurBone)
                    output.Append("v " + vert.X.ToString(usa) + " " + vert.Y.ToString(usa) + " " + vert.Z.ToString(usa) + "\n");
                foreach (Vector2 textureCoord in textureCoordsCurBone)
                    output.Append("vt " + textureCoord.X.ToString(usa) + " " + textureCoord.Y.ToString(usa) + "\n");
                foreach (Color vColour in vertexColoursCurBone)
                    output.Append("vc " + (vColour.R / 255.0f).ToString(usa) + " " + (vColour.G / 255.0f).ToString(usa) + " " +
                        (vColour.B / 255.0f).ToString(usa) + "\n");

                // For each material used in the current bone, print all faces
                foreach (ModelBase.GeometryDef geometry in bone.m_Geometries.Values)
                {
                    foreach (ModelBase.PolyListDef polyList in geometry.m_PolyLists.Values)
                    {
                        output.Append("usemtl " + polyList.m_MaterialName + "\n");

                        foreach (ModelBase.FaceDef face in polyList.m_Faces)
                        {
                            // Each face is a triangle or a quad, as they have already been extracted individually from
                            // the vertex lists
                            // Note: Indices start at 1 in OBJ
                            int numVerticesInFace = face.m_NumVertices;

                            output.Append("f ");
                            for (int m = 0; m < numVerticesInFace; m++)
                            {
                                output.Append((verts.LastIndexOf(face.m_Vertices[m]) + 1) +
                                "/" + (textureCoords.LastIndexOf((Vector2)face.m_TextureCoordinates[m]) + 1) +
                                "//" + (vertexColours.LastIndexOf((Color)face.m_VertexColours[m]) + 1) + " ");
                            }
                            output.Append("\n");
                        }
                    }
                }
            }

            outfile.Write(output);
            outfile.Close();
            outMTL.Write(mtllib);
            outMTL.Close();
        }

        private static void WriteBonesFileOBJ(string filename, ModelBase.BoneDefRoot boneTree)
        {
            // Write information about each bone, whether it's a parent or child to file
            // This is specific to this editor and will need re-added when re-importing if model modified

            StreamWriter writer = new StreamWriter(filename);

            StringBuilder outbones = new StringBuilder();
            foreach (ModelBase.BoneDef bone in boneTree)
            {
                outbones.Append("newbone " + bone.m_ID + "\n");
                // Offset in bones to parent bone (signed 16-bit. 0=no parent, -1=parent is the previous bone, ...)
                outbones.Append("parent_offset " + boneTree.GetParentOffset(bone) + "\n");
                // 1 if the bone has children, 0 otherwise
                outbones.Append("has_children " + ((bone.m_HasChildren) ? "1" : "0") + "\n");
                // Offset in bones to the next sibling bone (0=bone is last child of its parent)
                outbones.Append("sibling_offset " + boneTree.GetNextSiblingOffset(bone) + "\n");
                // Scale component
                outbones.Append("scale " + bone.m_20_12Scale[0].ToString("X8") + " " + bone.m_20_12Scale[1].ToString("X8") + " " + 
                    bone.m_20_12Scale[2].ToString("X8") + "\n");
                // Rotation component
                outbones.Append("rotation " + bone.m_4_12Rotation[0].ToString("X4") + " " + bone.m_4_12Rotation[1].ToString("X4") + " " + 
                    bone.m_4_12Rotation[2].ToString("X4") + "\n");
                // Translation component
                outbones.Append("translation " + bone.m_20_12Translation[0].ToString("X8") + " " + bone.m_20_12Translation[0].ToString("X8") + 
                    " " + bone.m_20_12Translation[2].ToString("X8") + "\n\n");
            }

            writer.Write(outbones);
            writer.Close();
        }

        private static void ExportTextureToPNG(string dir, string name, Bitmap lol)
        {
            //Export the current texture to .PNG
            try
            {
                lol.Save(dir + "/" + name + ".png", System.Drawing.Imaging.ImageFormat.Png);
            }
            catch (Exception e)
            {
                MessageBox.Show("An error occurred while trying to save texture " + name + ".\n\n " +
                    e.Message + "\n" + e.Data + "\n" + e.StackTrace + "\n" + e.Source);
            }
        }

        public override void WriteModel(bool save = true)
        {
            throw new NotImplementedException();
        }

        protected CultureInfo usa = Helper.USA;
    }
}
