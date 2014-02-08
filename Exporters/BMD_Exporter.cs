using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Globalization;
using System.IO;
using OpenTK;
using System.Drawing;

namespace SM64DSe.Exporters
{
    public static class BMD_Exporter
    {
        /* Exports the BMD model as a Wavefront OBJ;
         * Each bone is exported as a separate mesh/object using the o command, eg.:
         * o bone0
         * 
         * First a list of bones is created, these hold the materials used by the bone which then hold the 
         * geometry using that material. Within the BMD model a vertexlist (list of faces) often contains vertices 
         * from multiple bones. The faces must be extracted individually and assigned to the corresponding bone.
         * 
         * A list of bones and their offsets to their parent bone is included at the beginning of the file, this 
         * is non-standard and will need to be re-added when importing back into the editor. If not found, the 
         * assumption will be made in the importer that all bones are parent bones as in the level models.
         * 
        */
        public static void ExportBMDToOBJ(BMD model)
        {
            string output = "";
            string mtllib = "";
            string outbones = "";
            CultureInfo usa = new CultureInfo("en-US");//Need to ensure 1.23 not 1,23 when floatVar.ToString() used - use floatVar.ToString(usa)
            SaveFileDialog saveOBJ = new SaveFileDialog();
            saveOBJ.FileName = "SM64DS_Model";//Default name
            saveOBJ.DefaultExt = ".obj";//Default file extension
            saveOBJ.Filter = "Wavefront OBJ (.obj)|*.obj";//Filter by .obj
            if (saveOBJ.ShowDialog() == DialogResult.Cancel)
                return;
            StreamWriter outfile = new StreamWriter(saveOBJ.FileName);
            StreamWriter outMTL = new StreamWriter(saveOBJ.FileName.Substring(0, saveOBJ.FileName.Length - 4) + ".mtl");
            StreamWriter outBoneLib = new StreamWriter(saveOBJ.FileName.Substring(0, saveOBJ.FileName.Length - 4) + ".bones");
            string dir = Path.GetDirectoryName(saveOBJ.FileName);
            string filename = Path.GetFileNameWithoutExtension(saveOBJ.FileName);
            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> texCoords = new List<Vector2>();
            List<BMD.Texture> textures = new List<BMD.Texture>();
            output += Program.AppTitle + " " + Program.AppVersion + " " + Program.AppDate + "\n\n";
            output += "mtllib " + filename + ".mtl" + "\n\n";//Specify name of material library
            output += "bonelib " + filename + ".bones" + "\n\n";// Specify name of bones list

            // Apply transforms to bones
            applyBoneTransforms(model, 1f);

            // Write mtllib
            for (int i = 0; i < model.m_ModelChunks.Length; i++)
            {
                for (int j = 0; j < model.m_ModelChunks[i].m_MatGroups.Length; j++)
                {
                    //For every texture,
                    BMD.Texture currentTexture = model.m_ModelChunks[i].m_MatGroups[j].m_Texture;
                    textures.Add(currentTexture);
                    //Create new material
                    mtllib += "newmtl " /*+ ((i * 2) + j)*/ + model.m_ModelChunks[i].m_MatGroups[j].m_Name + "\n";
                    //Specify ambient colour - RGB 0-1
                    mtllib += "Ka " + (model.m_ModelChunks[i].m_MatGroups[j].m_AmbientColor.R / 255.0f).ToString(usa) +
                        " " + (model.m_ModelChunks[i].m_MatGroups[j].m_AmbientColor.G / 255.0f).ToString(usa) +
                        " " + (model.m_ModelChunks[i].m_MatGroups[j].m_AmbientColor.B / 255.0f).ToString(usa) + "\n";
                    //Specify diffuse colour - RGB 0-1
                    mtllib += "Kd " + (model.m_ModelChunks[i].m_MatGroups[j].m_DiffuseColor.R / 255.0f).ToString(usa) +
                        " " + (model.m_ModelChunks[i].m_MatGroups[j].m_DiffuseColor.G / 255.0f).ToString(usa) +
                        " " + (model.m_ModelChunks[i].m_MatGroups[j].m_DiffuseColor.B / 255.0f).ToString(usa) + "\n";
                    //Specify specular colour - RGB 0-1
                    mtllib += "Ks " + (model.m_ModelChunks[i].m_MatGroups[j].m_SpecularColor.R / 255.0f).ToString(usa) +
                        " " + (model.m_ModelChunks[i].m_MatGroups[j].m_SpecularColor.G / 255.0f).ToString(usa) +
                        " " + (model.m_ModelChunks[i].m_MatGroups[j].m_SpecularColor.B / 255.0f).ToString(usa) + "\n";
                    //Specify specular colour co-efficient - RGB 0-1
                    mtllib += "Ns " + model.m_ModelChunks[i].m_MatGroups[j].m_SpeEmiColors.ToString(usa) + "\n";
                    //Specify transparency - RGB Alpha channel 0-1
                    mtllib += "d " + (model.m_ModelChunks[i].m_MatGroups[j].m_Geometry[0].m_VertexList[0].m_Color.A / 255.0f).ToString(usa) + "\n";
                    //Specify texture type 0 - 10
                    //uint textype = (currentTexture.m_Params >> 26) & 0x7;
                    mtllib += "illum 2\n";
                    if (currentTexture != null)
                    {
                        //Specify name of texture image
                        mtllib += "map_Kd " + currentTexture.m_TexName + ".png" + "\n\n";
                        //Export the current texture to .PNG
                        Bitmap lol = new Bitmap((int)currentTexture.m_Width, (int)currentTexture.m_Height);

                        for (int y = 0; y < (int)currentTexture.m_Height; y++)
                        {
                            for (int x = 0; x < (int)currentTexture.m_Width; x++)
                            {
                                lol.SetPixel(x, y, Color.FromArgb(currentTexture.m_Data[((y * currentTexture.m_Width) + x) * 4 + 3],
                                 currentTexture.m_Data[((y * currentTexture.m_Width) + x) * 4 + 2],
                                 currentTexture.m_Data[((y * currentTexture.m_Width) + x) * 4 + 1],
                                 currentTexture.m_Data[((y * currentTexture.m_Width) + x) * 4]));
                            }
                        }
                        try
                        {
                            lol.Save(dir + "/" + currentTexture.m_TexName + ".png", System.Drawing.Imaging.ImageFormat.Png);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show("An error occurred while trying to save texture " + currentTexture.m_TexName + ".\n\n " +
                                e.Message + "\n" + e.Data + "\n" + e.StackTrace + "\n" + e.Source);
                        }
                    }
                    else
                        mtllib += "\n\n";
                }
            }

            // Get a list of the model's bones, having extracted each face and assigned it to its bone 
            // as defined in its Matrix ID, grouped by material, to child bones as well as parent ones
            List<BoneForExport> bones = getBonesForExport(model);

            // Write information about each bone, whether it's a parent or child to file
            // This is specific to this editor and will need re-added when re-importing if model modified
            for (int i = 0; i < bones.Count; i++)
            {
                outbones += "newbone " + bones[i].m_Name + "\n";
                // Offset in bones to parent bone (signed 16-bit. 0=no parent, -1=parent is the previous bone, ...)
                outbones += "parent_offset " + bones[i].m_ParentOffset + "\n";
                // 1 if the bone has children, 0 otherwise
                outbones += "has_children " + ((bones[i].m_HasChildren) ? "1" : "0") + "\n";
                // Offset in bones to the next sibling bone (0=bone is last child of its parent)
                outbones += "sibling_offset " + bones[i].m_SiblingOffset + "\n";
                // Scale matrix
                outbones += "scale " + bones[i].m_Scale[0].ToString("X8") + " " + bones[i].m_Scale[1].ToString("X8") + " " + bones[i].m_Scale[2].ToString("X8") + "\n";
                // Rotation matrix
                outbones += "rotation " + bones[i].m_Rotation[0].ToString("X4") + " " + bones[i].m_Rotation[1].ToString("X4") + " " + bones[i].m_Rotation[2].ToString("X4") + "\n";
                // Translation matrix
                outbones += "translation " + bones[i].m_Translation[0].ToString("X8") + " " + bones[i].m_Translation[1].ToString("X8") + " " + bones[i].m_Translation[2].ToString("X8") + "\n\n";
            }

            // Write each bone to file as a separate mesh/object using o command
            List<Vector3> verts = new List<Vector3>();
            List<Vector2> textureCoords = new List<Vector2>();
            for (int i = 0; i < bones.Count; i++)
            {
                output += "o " + bones[i].m_Name + "\n";

                // Get a list of all verices and texture co-ordinates for the current bone
                List<Vector3> vertsCurBone = new List<Vector3>();
                List<Vector2> textureCoordsCurBone = new List<Vector2>();

                for (int j = 0; j < bones[i].m_Materials.Count; j++)
                {
                    for (int k = 0; k < bones[i].m_Materials.Values.ToList<MaterialForExport>()[j].m_Faces.Count; k++)
                    {
                        for (int m = 0; m < bones[i].m_Materials.Values.ToList<MaterialForExport>()[j].m_Faces[k].m_Vertices.Count; m++)
                        {
                            if (!vertsCurBone.Contains(bones[i].m_Materials.Values.ToList<MaterialForExport>()[j].m_Faces[k].m_Vertices[m].m_Position))
                            {
                                vertsCurBone.Add(bones[i].m_Materials.Values.ToList<MaterialForExport>()[j].m_Faces[k].m_Vertices[m].m_Position);
                                verts.Add(bones[i].m_Materials.Values.ToList<MaterialForExport>()[j].m_Faces[k].m_Vertices[m].m_Position);
                            }
                            if (!textureCoordsCurBone.Contains(bones[i].m_Materials.Values.ToList<MaterialForExport>()[j].m_Faces[k].m_Vertices[m].m_TextureCoord))
                            {
                                textureCoordsCurBone.Add(bones[i].m_Materials.Values.ToList<MaterialForExport>()[j].m_Faces[k].m_Vertices[m].m_TextureCoord);
                                textureCoords.Add(bones[i].m_Materials.Values.ToList<MaterialForExport>()[j].m_Faces[k].m_Vertices[m].m_TextureCoord);
                            }
                        }
                    }
                }

                // Print a list of all vertices and texture co-ordinates
                foreach (Vector3 vert in vertsCurBone)
                    output += "v " + vert.X.ToString(usa) + " " + vert.Y.ToString(usa) + " " + vert.Z.ToString(usa) + "\n";
                foreach (Vector2 textureCoord in textureCoordsCurBone)
                    output += "vt " + textureCoord.X.ToString(usa) + " " + textureCoord.Y.ToString(usa) + "\n";

                // For each material used in the current bone, print all faces
                for (int j = 0; j < bones[i].m_Materials.Count; j++)
                {
                    if (bones[i].m_Materials.Values.ToList<MaterialForExport>()[j].m_Faces.Count == 0)
                        continue;

                    output += "usemtl " + bones[i].m_Materials.Values.ToList<MaterialForExport>()[j].m_MaterialName + "\n";

                    for (int k = 0; k < bones[i].m_Materials.Values.ToList<MaterialForExport>()[j].m_Faces.Count; k++)
                    {
                        // Each face is a triangle or a quad, as they have already been extracted individually from
                        // the vertex lists
                        // Note: Indices start at 1 in OBJ
                        int numVerticesInFace = bones[i].m_Materials.Values.ToList<MaterialForExport>()[j].m_Faces[k].m_Vertices.Count;

                        output += "f ";
                        for (int m = 0; m < numVerticesInFace; m++)
                        {
                            output += (lastIndexOfV3(verts, bones[i].m_Materials.Values.ToList<MaterialForExport>()[j].m_Faces[k].m_Vertices[m].m_Position) + 1) +
                            "/" + (lastIndexOfV2(textureCoords, bones[i].m_Materials.Values.ToList<MaterialForExport>()[j].m_Faces[k].m_Vertices[m].m_TextureCoord) + 1) + " ";
                        }
                        output += "\n";
                    }
                }
            }


            outfile.Write(output);
            outfile.Close();
            outMTL.Write(mtllib);
            outMTL.Close();
            outBoneLib.Write(outbones);
            outBoneLib.Close();
        }//End Method

        /* Returns a list of bones containing only their own geometry separated by material.
         * Go through each vertexlist in the BMD file and split it into separate faces, then assign 
         * that face to its bone as defined by its Matrix ID. 
         * Necessary as a vertexlist can contain faces from multiple bones and geometry is currently assigned 
         * only to the parent bone.
        */
        private static List<BoneForExport> getBonesForExport(BMD model)
        {
            Dictionary<uint, BoneForExport> bones = new Dictionary<uint, BoneForExport>();
            uint numBones = model.m_NumModelChunks;

            for (int i = 0; i < model.m_ModelChunks.Length; i++)
            {
                bones.Add(model.m_ModelChunks[i].m_ID,
                    new BoneForExport(model.m_ModelChunks[i].m_Name, model.m_ModelChunks[i].m_ParentOffset,
                        model.m_ModelChunks[i].m_SiblingOffset, model.m_ModelChunks[i].m_HasChildren,
                        model.m_ModelChunks[i].m_Scale, model.m_ModelChunks[i].m_Rotation, model.m_ModelChunks[i].m_Translation));
            }

            // Go through each vertexlist in the BMD file and split it into separate faces, then assign 
            // that face to its bone as defined by its Matrix ID
            for (int i = 0; i < model.m_ModelChunks.Length; i++)
            {
                // If it's a parent bone with no children, ignore the Matrix ID when assigning geometry and assign all 
                // geometry to this parent bone
                bool parentNoChildren = (model.m_ModelChunks[i].m_ParentOffset == 0 && !model.m_ModelChunks[i].m_HasChildren);

                for (int j = 0; j < model.m_ModelChunks[i].m_MatGroups.Length; j++)
                {
                    if (!bones[model.m_ModelChunks[i].m_ID].m_Materials.ContainsKey(model.m_ModelChunks[i].m_MatGroups[j].m_Name))
                        bones[model.m_ModelChunks[i].m_ID].m_Materials.Add(model.m_ModelChunks[i].m_MatGroups[j].m_Name,
                            new MaterialForExport(model.m_ModelChunks[i].m_MatGroups[j].m_Name, model.m_ModelChunks[i].m_Name));

                    for (int k = 0; k < model.m_ModelChunks[i].m_MatGroups[j].m_Geometry.Count; k++)
                    {
                        uint polyType = model.m_ModelChunks[i].m_MatGroups[j].m_Geometry[k].m_PolyType;
                        List<BMD.Vertex> vtxList = model.m_ModelChunks[i].m_MatGroups[j].m_Geometry[k].m_VertexList;
                        uint currentBoneID = 0;

                        switch (polyType)
                        {
                            case 0://Separate Triangles
                                {
                                    if (vtxList.Count <= 3)//Just 1 triangle
                                    {
                                        // Get the corresponding Bone ID from the vertex's Matrix ID, unless it's a parent bone with no children 
                                        // in which case ignore the Matrix ID (applicable to level models)
                                        currentBoneID = (parentNoChildren == true) ? (uint)i : model.m_ModelChunks[i].m_MatGroups[j].m_BoneIDs[vtxList[0].m_MatrixID];

                                        List<FaceVertex> verts = new List<FaceVertex>();
                                        verts.Add(new FaceVertex(vtxList[0].m_Position, vtxList[0].m_TexCoord, vtxList[0].m_Color));
                                        verts.Add(new FaceVertex(vtxList[1].m_Position, vtxList[1].m_TexCoord, vtxList[1].m_Color));
                                        verts.Add(new FaceVertex(vtxList[2].m_Position, vtxList[2].m_TexCoord, vtxList[2].m_Color));
                                        ModelFace face = new ModelFace(verts, vtxList[0].m_Normal, bones[currentBoneID].m_Name);

                                        if (!bones[currentBoneID].m_Materials.ContainsKey(model.m_ModelChunks[i].m_MatGroups[j].m_Name))
                                            bones[currentBoneID].m_Materials.Add(model.m_ModelChunks[i].m_MatGroups[j].m_Name,
                                                new MaterialForExport(model.m_ModelChunks[i].m_MatGroups[j].m_Name, bones[currentBoneID].m_Name));

                                        bones[currentBoneID].m_Materials[model.m_ModelChunks[i].m_MatGroups[j].m_Name].m_Faces.Add(face);
                                    }
                                    else if (vtxList.Count > 3 && (float)vtxList.Count % 3 == 0.0f)//Eg. 9 vertices in 3 triangles
                                    {
                                        int numFaces = vtxList.Count / 3;
                                        for (int a = 0, b = 0; a < numFaces; a++, b = b + 3)
                                        {
                                            currentBoneID = (parentNoChildren == true) ? (uint)i : model.m_ModelChunks[i].m_MatGroups[j].m_BoneIDs[vtxList[b + 2].m_MatrixID];
                                            List<FaceVertex> verts = new List<FaceVertex>();
                                            verts.Add(new FaceVertex(vtxList[b].m_Position, vtxList[b].m_TexCoord, vtxList[b].m_Color));
                                            verts.Add(new FaceVertex(vtxList[b + 1].m_Position, vtxList[b + 1].m_TexCoord, vtxList[b + 1].m_Color));
                                            verts.Add(new FaceVertex(vtxList[b + 2].m_Position, vtxList[b + 2].m_TexCoord, vtxList[b + 2].m_Color));
                                            ModelFace face = new ModelFace(verts, vtxList[b].m_Normal, bones[currentBoneID].m_Name);

                                            if (!bones[currentBoneID].m_Materials.ContainsKey(model.m_ModelChunks[i].m_MatGroups[j].m_Name))
                                                bones[currentBoneID].m_Materials.Add(model.m_ModelChunks[i].m_MatGroups[j].m_Name,
                                                    new MaterialForExport(model.m_ModelChunks[i].m_MatGroups[j].m_Name, bones[currentBoneID].m_Name));

                                            bones[currentBoneID].m_Materials[model.m_ModelChunks[i].m_MatGroups[j].m_Name].m_Faces.Add(face);
                                        }
                                    }
                                    break;
                                }
                            case 1://Separate Quadrilaterals
                                {
                                    if (vtxList.Count <= 4)//Just 1 quadrilateral
                                    {
                                        currentBoneID = (parentNoChildren == true) ? (uint)i : model.m_ModelChunks[i].m_MatGroups[j].m_BoneIDs[vtxList[1].m_MatrixID];
                                        List<FaceVertex> verts = new List<FaceVertex>();
                                        verts.Add(new FaceVertex(vtxList[0].m_Position, vtxList[0].m_TexCoord, vtxList[0].m_Color));
                                        verts.Add(new FaceVertex(vtxList[1].m_Position, vtxList[1].m_TexCoord, vtxList[1].m_Color));
                                        verts.Add(new FaceVertex(vtxList[2].m_Position, vtxList[2].m_TexCoord, vtxList[2].m_Color));
                                        verts.Add(new FaceVertex(vtxList[3].m_Position, vtxList[3].m_TexCoord, vtxList[3].m_Color));
                                        ModelFace face = new ModelFace(verts, vtxList[0].m_Normal, bones[currentBoneID].m_Name);

                                        if (!bones[currentBoneID].m_Materials.ContainsKey(model.m_ModelChunks[i].m_MatGroups[j].m_Name))
                                            bones[currentBoneID].m_Materials.Add(model.m_ModelChunks[i].m_MatGroups[j].m_Name,
                                                new MaterialForExport(model.m_ModelChunks[i].m_MatGroups[j].m_Name, bones[currentBoneID].m_Name));

                                        bones[currentBoneID].m_Materials[model.m_ModelChunks[i].m_MatGroups[j].m_Name].m_Faces.Add(face);
                                    }
                                    else if (vtxList.Count > 4 && (float)vtxList.Count % 4 == 0.0f)//Eg. 8 vertices in 2 quadrilaterals
                                    {
                                        int numFaces = vtxList.Count / 4;
                                        for (int a = 0, b = 0; a < numFaces; a++, b = b + 4)
                                        {
                                            currentBoneID = (parentNoChildren == true) ? (uint)i : model.m_ModelChunks[i].m_MatGroups[j].m_BoneIDs[vtxList[b + 1].m_MatrixID];
                                            List<FaceVertex> verts = new List<FaceVertex>();
                                            verts.Add(new FaceVertex(vtxList[b].m_Position, vtxList[b].m_TexCoord, vtxList[b].m_Color));
                                            verts.Add(new FaceVertex(vtxList[b + 1].m_Position, vtxList[b + 1].m_TexCoord, vtxList[b + 1].m_Color));
                                            verts.Add(new FaceVertex(vtxList[b + 2].m_Position, vtxList[b + 2].m_TexCoord, vtxList[b + 2].m_Color));
                                            verts.Add(new FaceVertex(vtxList[b + 3].m_Position, vtxList[b + 3].m_TexCoord, vtxList[b + 3].m_Color));
                                            ModelFace face = new ModelFace(verts, vtxList[b].m_Normal, bones[currentBoneID].m_Name);

                                            if (!bones[currentBoneID].m_Materials.ContainsKey(model.m_ModelChunks[i].m_MatGroups[j].m_Name))
                                                bones[currentBoneID].m_Materials.Add(model.m_ModelChunks[i].m_MatGroups[j].m_Name,
                                                    new MaterialForExport(model.m_ModelChunks[i].m_MatGroups[j].m_Name, bones[currentBoneID].m_Name));

                                            bones[currentBoneID].m_Materials[model.m_ModelChunks[i].m_MatGroups[j].m_Name].m_Faces.Add(face);
                                        }
                                    }
                                    break;
                                }
                            case 2://Triangle Strips
                                {
                                    //3+(N-1) vertices per N triangles
                                    //(N-3)+1 Triangles per N Vertices
                                    int numFaces = vtxList.Count - 2;
                                    if (vtxList.Count < 3)//Should never be
                                        break;
                                    //Convert all faces with more than 3 vertices to ones with only 3
                                    for (int n = 0; n < numFaces; n++)
                                    {
                                        if (n % 2 == 0)
                                        {
                                            currentBoneID = (parentNoChildren == true) ? (uint)i : model.m_ModelChunks[i].m_MatGroups[j].m_BoneIDs[vtxList[n + 0].m_MatrixID];
                                            List<FaceVertex> verts = new List<FaceVertex>();
                                            verts.Add(new FaceVertex(vtxList[n].m_Position, vtxList[n].m_TexCoord, vtxList[n].m_Color));
                                            verts.Add(new FaceVertex(vtxList[n + 1].m_Position, vtxList[n + 1].m_TexCoord, vtxList[n + 1].m_Color));
                                            verts.Add(new FaceVertex(vtxList[n + 2].m_Position, vtxList[n + 2].m_TexCoord, vtxList[n + 2].m_Color));
                                            ModelFace face = new ModelFace(verts, vtxList[n].m_Normal, bones[currentBoneID].m_Name);

                                            if (!bones[currentBoneID].m_Materials.ContainsKey(model.m_ModelChunks[i].m_MatGroups[j].m_Name))
                                                bones[currentBoneID].m_Materials.Add(model.m_ModelChunks[i].m_MatGroups[j].m_Name,
                                                    new MaterialForExport(model.m_ModelChunks[i].m_MatGroups[j].m_Name, bones[currentBoneID].m_Name));

                                            bones[currentBoneID].m_Materials[model.m_ModelChunks[i].m_MatGroups[j].m_Name].m_Faces.Add(face);
                                        }
                                        else
                                        {
                                            currentBoneID = (parentNoChildren == true) ? (uint)i : model.m_ModelChunks[i].m_MatGroups[j].m_BoneIDs[vtxList[n + 1].m_MatrixID];
                                            List<FaceVertex> verts = new List<FaceVertex>();
                                            verts.Add(new FaceVertex(vtxList[n + 2].m_Position, vtxList[n + 2].m_TexCoord, vtxList[n + 2].m_Color));
                                            verts.Add(new FaceVertex(vtxList[n + 1].m_Position, vtxList[n + 1].m_TexCoord, vtxList[n + 1].m_Color));
                                            verts.Add(new FaceVertex(vtxList[n].m_Position, vtxList[n].m_TexCoord, vtxList[n].m_Color));
                                            ModelFace face = new ModelFace(verts, vtxList[n + 2].m_Normal, bones[currentBoneID].m_Name);

                                            if (!bones[currentBoneID].m_Materials.ContainsKey(model.m_ModelChunks[i].m_MatGroups[j].m_Name))
                                                bones[currentBoneID].m_Materials.Add(model.m_ModelChunks[i].m_MatGroups[j].m_Name,
                                                    new MaterialForExport(model.m_ModelChunks[i].m_MatGroups[j].m_Name, bones[currentBoneID].m_Name));

                                            bones[currentBoneID].m_Materials[model.m_ModelChunks[i].m_MatGroups[j].m_Name].m_Faces.Add(face);
                                        }
                                        //Because of how normals are defined in triangle strips, every 2nd triangle is clockwise, whereas all others are anti-clockwise
                                    }
                                    break;
                                }
                            case 3://Quadrilateral Strips
                                {
                                    //4+(N-1)*2 vertices per N quads
                                    //((N-4)/2) + 1 Quads. per N Vertices
                                    int numFaces = ((vtxList.Count - 4) / 2) + 1;
                                    if (vtxList.Count < 4)//Should never be
                                        break;
                                    for (int n = 0, p = 0; n < numFaces; n++, p = p + 2)
                                    {
                                        currentBoneID = (parentNoChildren == true) ? (uint)i : model.m_ModelChunks[i].m_MatGroups[j].m_BoneIDs[vtxList[p].m_MatrixID];
                                        List<FaceVertex> verts = new List<FaceVertex>();
                                        verts.Add(new FaceVertex(vtxList[p].m_Position, vtxList[p].m_TexCoord, vtxList[p].m_Color));
                                        verts.Add(new FaceVertex(vtxList[p + 1].m_Position, vtxList[p + 1].m_TexCoord, vtxList[p + 1].m_Color));
                                        verts.Add(new FaceVertex(vtxList[p + 3].m_Position, vtxList[p + 3].m_TexCoord, vtxList[p + 3].m_Color));
                                        verts.Add(new FaceVertex(vtxList[p + 2].m_Position, vtxList[p + 2].m_TexCoord, vtxList[p + 2].m_Color));
                                        ModelFace face = new ModelFace(verts, vtxList[0].m_Normal, bones[currentBoneID].m_Name);

                                        if (!bones[currentBoneID].m_Materials.ContainsKey(model.m_ModelChunks[i].m_MatGroups[j].m_Name))
                                            bones[currentBoneID].m_Materials.Add(model.m_ModelChunks[i].m_MatGroups[j].m_Name,
                                                new MaterialForExport(model.m_ModelChunks[i].m_MatGroups[j].m_Name, bones[currentBoneID].m_Name));

                                        bones[currentBoneID].m_Materials[model.m_ModelChunks[i].m_MatGroups[j].m_Name].m_Faces.Add(face);
                                    }
                                    break;
                                }
                            default: MessageBox.Show("Unknown polygon type."); break;
                        }//End polyType switch
                    }
                }
            }

            return bones.Values.ToList<BoneForExport>();
        }

        private static void applyBoneTransforms(BMD model, float scale)
        {
            for (int i = 0; i < model.m_ModelChunks.Length; i++)
            {
                for (int j = 0; j < model.m_ModelChunks[i].m_MatGroups.Length; j++)
                {
                    if (model.m_ModelChunks[i].m_MatGroups[j].m_Geometry.Count <= 0)
                        continue;

                    for (int k = 0; k < model.m_ModelChunks[i].m_MatGroups[j].m_Geometry.Count; k++)
                    {
                        for (int m = 0; m < model.m_ModelChunks[i].m_MatGroups[j].m_Geometry[k].m_VertexList.Count; m++)
                        {

                            BMD.Vertex finalvtx = model.m_ModelChunks[i].m_MatGroups[j].m_Geometry[k].m_VertexList[m];
                            Matrix4 bonemtx =
                                model.m_ModelChunks[i].m_MatGroups[j].m_BoneMatrices[model.m_ModelChunks[i].m_MatGroups[j].m_Geometry[k].m_VertexList[m].m_MatrixID];

                            Vector3.Transform(ref finalvtx.m_Position, ref bonemtx, out finalvtx.m_Position);
                            Vector3.Multiply(ref finalvtx.m_Position, scale, out finalvtx.m_Position);

                            model.m_ModelChunks[i].m_MatGroups[j].m_Geometry[k].m_VertexList[m] = finalvtx;
                        }
                    }
                }
            }
        }

        private static int lastIndexOfV3(List<Vector3> listIn, Vector3 vectorIn)
        {
            int index = 0;
            for (int i = 0; i < listIn.Count; i++)
            {
                if (listIn[i] == vectorIn)
                    index = i;
            }
            return index;
        }

        private static int lastIndexOfV2(List<Vector2> listIn, Vector2 vectorIn)
        {
            int index = 0;
            for (int i = 0; i < listIn.Count; i++)
            {
                if (listIn[i] == vectorIn)
                    index = i;
            }
            return index;
        }

        private static int firstIndexOfV3(List<Vector3> listIn, Vector3 vectorIn)
        {
            int index = -1;
            for (int i = 0; i < listIn.Count; i++)
            {
                if (listIn[i] == vectorIn)
                    return i;
            }
            return index;
        }

        private static int firstIndexOfV2(List<Vector2> listIn, Vector2 vectorIn)
        {
            int index = -1;
            for (int i = 0; i < listIn.Count; i++)
            {
                if (listIn[i] == vectorIn)
                    return i;
            }
            return index;
        }

        public class BoneForExport
        {
            public Dictionary<String, MaterialForExport> m_Materials;
            public String m_Name;
            public short m_ParentOffset;
            public short m_SiblingOffset;
            public bool m_HasChildren;
            public uint[] m_Scale;
            public ushort[] m_Rotation;
            public uint[] m_Translation;

            public BoneForExport(String name, short parentOffset, short siblingOffset, bool hasChildren)
            {
                m_Materials = new Dictionary<String, MaterialForExport>();
                m_Name = name;
                m_ParentOffset = parentOffset;
                m_SiblingOffset = siblingOffset;
                m_HasChildren = hasChildren;
                m_Scale = new uint[] { 0x00001000, 0x00001000, 0x00001000 };
                m_Rotation = new ushort[] { 0x0000, 0x0000, 0x0000 };
                m_Translation = new uint[] { 0, 0, 0 };
            }

            public BoneForExport(String name, short parentOffset, short siblingOffset, bool hasChildren, uint[] scale, ushort[] rotation, uint[] translation)
            {
                m_Materials = new Dictionary<String, MaterialForExport>();
                m_Name = name;
                m_ParentOffset = parentOffset;
                m_SiblingOffset = siblingOffset;
                m_HasChildren = hasChildren;
                m_Scale = scale;
                m_Rotation = rotation;
                m_Translation = translation;
            }
        }

        public class MaterialForExport
        {
            public List<ModelFace> m_Faces;
            public String m_MaterialName;
            public String m_BoneName;

            public MaterialForExport(String name, String boneName)
            {
                m_Faces = new List<ModelFace>();
                m_MaterialName = name;
                m_BoneName = boneName;
            }
        }

        public class ModelFace
        {
            public List<FaceVertex> m_Vertices;
            public Vector3 m_Normal;
            public String m_BoneName;

            public ModelFace(List<FaceVertex> vertices, Vector3 normal, String boneName)
            {
                m_Vertices = vertices;
                m_Normal = normal;
                m_BoneName = boneName;
            }
        }

        public class FaceVertex
        {
            public Vector3 m_Position;
            public Vector2 m_TextureCoord;
            public Color m_VertexColour;

            public FaceVertex(Vector3 position, Vector2 textureCoord)
            {
                m_Position = position;
                m_TextureCoord = textureCoord;
            }

            public FaceVertex(Vector3 position, Vector2 textureCoord, Color vertexColour)
            {
                m_Position = position;
                m_TextureCoord = textureCoord;
                m_VertexColour = vertexColour;
            }
        }
    }
}
