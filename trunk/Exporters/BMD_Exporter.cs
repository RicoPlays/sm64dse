/*
* COLLADA DAE (BMD and BCA):
* 
* Exports the model as a COLLADA DAE model complete with full joints and skinning and animation.
* 
* This is the recommended format for exporting as it matches the original BMD almost exactly.
* 
* A .bones file is NOT needed as the COLLADA format fully supports joints and skinning.
* 
* Wavefront OBJ (BMD only):
* 
* Exports the BMD model as a Wavefront OBJ with vertex colours as used in the Blender-specific "Extended OBJ" plugin;
* Each bone is exported as a separate mesh/object using the o command, eg.:
* o bone0
* 
* First a list of bones is created, these hold the materials used by the bone which then hold the 
* geometry using that material. Within the BMD model a vertexlist (list of faces) often contains vertices 
* from multiple bones. The faces must be extracted individually and assigned to the corresponding bone.
* 
* As OBJ does not have any support for bones, a list of bones and their offsets to their parent bone is included at 
* the beginning of the file, this is non-standard and will need to be re-added when importing back into the editor. If not 
* found, the assumption will be made in the importer that all bones are parent bones as in the level models.
* 
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Globalization;
using System.IO;
using OpenTK;
using System.Drawing;
using System.Xml;

namespace SM64DSe.Exporters
{
    public static class BMD_Exporter
    {
        public static void ExportBMD(BMD model)
        {
            SaveFileDialog saveModel = new SaveFileDialog();
            saveModel.FileName = "SM64DS_Model";//Default name
            saveModel.DefaultExt = ".dae";//Default file extension
            saveModel.Filter = "COLLADA DAE (.dae)|*.dae|Wavefront OBJ (.obj)|*.obj";//Filter by .DAE and .OBJ
            if (saveModel.ShowDialog() == DialogResult.Cancel)
                return;

            string ext = Path.GetExtension(saveModel.FileName);

            if (ext.ToLowerInvariant().Equals(".dae"))
                ExportBMDToDAE(model, saveModel.FileName);
            else if (ext.ToLowerInvariant().Equals(".obj"))
                ExportBMDToOBJ(model, saveModel.FileName);
            else
                ExportBMDToDAE(model, saveModel.FileName);
        }

        public static void ExportBMDAndBCA(BMD model, BCA animation)
        {
            SaveFileDialog saveModel = new SaveFileDialog();
            saveModel.FileName = "SM64DS_Animated_Model_" + 
                model.m_FileName.Substring(model.m_FileName.LastIndexOf("/") + 1) + ".DAE";//Default name
            saveModel.DefaultExt = ".dae";//Default file extension
            saveModel.Filter = "COLLADA DAE (.dae)|*.dae";//Filter by .DAE
            if (saveModel.ShowDialog() == DialogResult.Cancel)
                return;

            ExportBMDAndBCAToDAE(model, animation, saveModel.FileName);
        }
        
        public static void ExportBMDToOBJ(BMD model, string fileName)
        {
            string output = "";
            string mtllib = "";
            StreamWriter outfile = new StreamWriter(fileName);
            StreamWriter outMTL = new StreamWriter(fileName.Substring(0, fileName.Length - 4) + ".mtl");
            string dir = Path.GetDirectoryName(fileName);
            string baseFileName = Path.GetFileNameWithoutExtension(fileName);
            List<BMD.Texture> textures = new List<BMD.Texture>();
            output += "#" + Program.AppTitle + " " + Program.AppVersion + " " + Program.AppDate + "\n\n";
            output += "mtllib " + baseFileName + ".mtl" + "\n\n";//Specify name of material library
            output += "bonelib " + baseFileName + ".bones" + "\n\n";// Specify name of bones list

            // Apply transforms to bones
            ApplyBoneTransformations(model, 1f);

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
                        ExportTextureToPNG(dir, currentTexture);
                    }
                    else
                        mtllib += "\n\n";
                }
            }

            // Get a list of the model's bones, having extracted each face and assigned it to its bone 
            // as defined in its Matrix ID, grouped by material, to child bones as well as parent ones
            List<BoneForExport> bones = GetBonesForExport(model);

            WriteBonesFileOBJ(fileName.Substring(0, fileName.Length - 4) + ".bones", bones);

            // Write each bone to file as a separate mesh/object using o command
            List<Vector3> verts = new List<Vector3>();
            List<Vector2> textureCoords = new List<Vector2>();
            List<Color> vertexColours = new List<Color>();
            for (int i = 0; i < bones.Count; i++)
            {
                output += "o " + bones[i].m_Name + "\n";

                // Get a list of all verices and texture co-ordinates for the current bone
                List<Vector3> vertsCurBone = new List<Vector3>();
                List<Vector2> textureCoordsCurBone = new List<Vector2>();
                List<Color> vertexColoursCurBone = new List<Color>();

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
                            if (!vertexColoursCurBone.Contains(bones[i].m_Materials.Values.ToList<MaterialForExport>()[j].m_Faces[k].m_Vertices[m].m_VertexColour))
                            {
                                vertexColoursCurBone.Add(bones[i].m_Materials.Values.ToList<MaterialForExport>()[j].m_Faces[k].m_Vertices[m].m_VertexColour);
                                vertexColours.Add(bones[i].m_Materials.Values.ToList<MaterialForExport>()[j].m_Faces[k].m_Vertices[m].m_VertexColour);
                            }
                        }
                    }
                }

                // Print a list of all vertices, texture co-ordinates and vertex colours
                foreach (Vector3 vert in vertsCurBone)
                    output += "v " + vert.X.ToString(usa) + " " + vert.Y.ToString(usa) + " " + vert.Z.ToString(usa) + "\n";
                foreach (Vector2 textureCoord in textureCoordsCurBone)
                    output += "vt " + textureCoord.X.ToString(usa) + " " + textureCoord.Y.ToString(usa) + "\n";
                foreach (Color vColour in vertexColoursCurBone)
                    output += "vc " + (vColour.R / 255.0f).ToString(usa) + " " + (vColour.G / 255.0f).ToString(usa) + " " + 
                        (vColour.B / 255.0f).ToString(usa) + "\n";

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
                            output += (verts.LastIndexOf(bones[i].m_Materials.Values.ToList<MaterialForExport>()[j].m_Faces[k].m_Vertices[m].m_Position) + 1) +
                            "/" + (textureCoords.LastIndexOf(bones[i].m_Materials.Values.ToList<MaterialForExport>()[j].m_Faces[k].m_Vertices[m].m_TextureCoord) + 1) +
                            "//" + (vertexColours.LastIndexOf(bones[i].m_Materials.Values.ToList<MaterialForExport>()[j].m_Faces[k].m_Vertices[m].m_VertexColour) + 1) + " ";
                        }
                        output += "\n";
                    }
                }
            }

            outfile.Write(output);
            outfile.Close();
            outMTL.Write(mtllib);
            outMTL.Close();
        }//End Method

        private static void WriteBonesFileOBJ(String filename, List<BoneForExport> bones)
        {
            // Write information about each bone, whether it's a parent or child to file
            // This is specific to this editor and will need re-added when re-importing if model modified

            StreamWriter writer = new StreamWriter(filename);

            string outbones = "";
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

            writer.Write(outbones);
            writer.Close();
        }

        private static void ExportTextureToPNG(string dir, BMD.Texture currentTexture)
        {
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
        
        private static CultureInfo usa = Helper.USA;//Need to ensure 1.23 not 1,23 when floatVar.ToString() used - use floatVar.ToString(usa)

        public static void ExportBMDToDAE(BMD model, string fileName)
        {
            ExportBMDAndBCAToDAE(model, null, fileName);
        }

        public static void ExportBMDAndBCAToDAE(BMD model, BCA animation, string fileName, bool useSRTMatricesForTransforms = false)
        {
            string dir = Path.GetDirectoryName(fileName);
            string filename = Path.GetFileNameWithoutExtension(fileName);

            List<BMD.Texture> textures = new List<BMD.Texture>();

            // Apply transforms to bones
            ApplyBoneTransformations(model, 1f);

            // Get a list of the model's bones, having extracted each face and assigned it to its bone 
            // as defined in its Matrix ID, grouped by material, to child bones as well as parent ones
            List<BoneForExport> bones = GetBonesForExport(model);

            // Export textures to PNG
            for (int i = 0; i < model.m_ModelChunks.Length; i++)
            {
                for (int j = 0; j < model.m_ModelChunks[i].m_MatGroups.Length; j++)
                {
                    //For every texture,
                    BMD.Texture currentTexture = model.m_ModelChunks[i].m_MatGroups[j].m_Texture;

                    if (!textures.Contains(currentTexture))
                    {
                        textures.Add(currentTexture);

                        if (currentTexture != null)
                        {
                            ExportTextureToPNG(dir, currentTexture);
                        }
                    }
                }
            }

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "\t";
            settings.NewLineChars = "\r\n";
            settings.NewLineHandling = NewLineHandling.Replace;
            using (XmlWriter writer = XmlWriter.Create(fileName, settings))
            {
                writer.WriteStartDocument();
                writer.WriteComment(Program.AppTitle + " " + Program.AppVersion + " " + Program.AppDate);
                writer.WriteStartElement("COLLADA", "http://www.collada.org/2005/11/COLLADASchema");
                writer.WriteAttributeString("version", "1.4.1");

                WriteDAE_Asset(writer);

                // Animation, library_animations has to be first library_* tag
                if (animation != null)
                {
                    WriteDAE_LibraryAnimations(writer, model.m_ModelChunks, bones, animation, useSRTMatricesForTransforms);
                }

                WriteDAE_LibraryImages(writer, textures, dir);

                List<BMD.MaterialGroup> matGroups = new List<BMD.MaterialGroup>();
                for (int i = 0; i < model.m_ModelChunks.Length; i++)
                {
                    for (int j = 0; j < model.m_ModelChunks[i].m_MatGroups.Length; j++)
                    {
                        IEnumerable<BMD.MaterialGroup> matchingNameMats = matGroups.Where(mat0 => mat0.m_Name.Equals(
                            model.m_ModelChunks[i].m_MatGroups[j].m_Name));

                        if (matchingNameMats.Count() < 1)
                            matGroups.Add(model.m_ModelChunks[i].m_MatGroups[j]);
                    }
                }

                WriteDAE_LibraryEffects(writer, matGroups);

                WriteDAE_LibraryMaterials(writer, matGroups);

                WriteDAE_LibraryGeometries(writer, bones);

                // DAE supports proper joints and skinning

                WriteDAE_LibraryControllers(writer, bones);

                WriteDAE_LibraryVisualScenes(writer, bones, useSRTMatricesForTransforms);

                writer.WriteEndElement();
                writer.WriteEndDocument();

                writer.Close();
            }
        }

        private static void WriteDAE_Asset(XmlWriter writer)
        {
            writer.WriteStartElement("asset");

            writer.WriteStartElement("contributor");
            writer.WriteElementString("author", "Fiachra");
            writer.WriteElementString("authoring_tool", Program.AppTitle + " " + Program.AppVersion + " " + Program.AppDate);
            writer.WriteEndElement();

            writer.WriteElementString("created", DateTime.UtcNow.ToString("s"));
            writer.WriteElementString("modified", DateTime.UtcNow.ToString("s"));
            writer.WriteStartElement("unit");
            writer.WriteAttributeString("name", "meter");
            writer.WriteAttributeString("meter", "1");
            writer.WriteEndElement();// unit
            writer.WriteElementString("up_axis", "Y_UP");

            writer.WriteEndElement();
        }

        private static void WriteDAE_LibraryImages(XmlWriter writer, List<BMD.Texture> textures, string dir)
        {
            writer.WriteStartElement("library_images");

            foreach (BMD.Texture tex in textures)
            {
                if (tex == null)
                    continue;

                writer.WriteStartElement("image");

                writer.WriteAttributeString("id", tex.m_TexName + "-img");
                writer.WriteAttributeString("name", tex.m_TexName + "-img");

                writer.WriteElementString("init_from", tex.m_TexName + ".png");

                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        private static void WriteDAE_LibraryEffects(XmlWriter writer, List<BMD.MaterialGroup> matGroups)
        {
            writer.WriteStartElement("library_effects");

            foreach (BMD.MaterialGroup mat in matGroups)
            {
                writer.WriteStartElement("effect");
                writer.WriteAttributeString("id", mat.m_Name + "-effect");
                writer.WriteStartElement("profile_COMMON");

                if (mat.m_Texture != null)
                {
                    writer.WriteStartElement("newparam");
                    writer.WriteAttributeString("sid", mat.m_Texture.m_TexName + "-img-surface");

                    writer.WriteStartElement("surface");
                    writer.WriteAttributeString("type", "2D");
                    writer.WriteElementString("init_from", mat.m_Texture.m_TexName + "-img");
                    writer.WriteEndElement();// surface

                    writer.WriteEndElement();// newparam

                    writer.WriteStartElement("newparam");
                    writer.WriteAttributeString("sid", mat.m_Texture.m_TexName + "-img-sampler");

                    writer.WriteStartElement("sampler2D");
                    writer.WriteElementString("source", mat.m_Texture.m_TexName + "-img-surface");
                    writer.WriteEndElement();// sampler2D

                    writer.WriteEndElement();// newparam
                }

                writer.WriteStartElement("technique");
                writer.WriteAttributeString("sid", "common");
                writer.WriteStartElement("phong");

                writer.WriteStartElement("emission");
                writer.WriteStartElement("color");
                writer.WriteAttributeString("sid", "emission");
                writer.WriteString((mat.m_EmissionColor.R / 255.0f).ToString(usa) + " " + (mat.m_EmissionColor.G / 255.0f).ToString(usa) + " " +
                    (mat.m_EmissionColor.B / 255.0f).ToString(usa) + " "  + "1.0");
                writer.WriteEndElement();// color
                writer.WriteEndElement();// emission

                writer.WriteStartElement("ambient");
                writer.WriteStartElement("color");
                writer.WriteAttributeString("sid", "ambient");
                writer.WriteString((mat.m_AmbientColor.R / 255.0f).ToString(usa) + " "  + (mat.m_AmbientColor.G / 255.0f).ToString(usa) + " "  +
                    (mat.m_AmbientColor.B / 255.0f).ToString(usa) + " "  + "1.0");
                writer.WriteEndElement();// color
                writer.WriteEndElement();// ambient

                writer.WriteStartElement("diffuse");
                if (mat.m_Texture != null)
                {
                    writer.WriteStartElement("texture");
                    writer.WriteAttributeString("texture", mat.m_Texture.m_TexName + "-img-sampler");
                    writer.WriteAttributeString("texcoord", "UVMap");

                    writer.WriteEndElement();// texture
                }
                else
                {
                    writer.WriteStartElement("color");
                    writer.WriteAttributeString("sid", "diffuse");
                    writer.WriteString((mat.m_DiffuseColor.R / 255.0f).ToString(usa) + " "  + (mat.m_DiffuseColor.G / 255.0f).ToString(usa) + " "  +
                        (mat.m_DiffuseColor.B / 255.0f).ToString(usa) + " "  + "1.0");
                    writer.WriteEndElement();// color
                }
                writer.WriteEndElement();// diffuse

                writer.WriteStartElement("specular");
                writer.WriteStartElement("color");
                writer.WriteAttributeString("sid", "specular");
                writer.WriteString((mat.m_SpecularColor.R / 255.0f).ToString(usa) + " "  + (mat.m_SpecularColor.G / 255.0f).ToString(usa) + " "  +
                    (mat.m_SpecularColor.B / 255.0f).ToString(usa) + " "  + "1.0");
                writer.WriteEndElement();// color
                writer.WriteEndElement();// specular

                writer.WriteStartElement("transparency");
                writer.WriteStartElement("float");
                writer.WriteAttributeString("sid", "transparency");
                writer.WriteString((mat.m_DiffuseColor.A / 255.0f).ToString(usa));
                writer.WriteEndElement();// float
                writer.WriteEndElement();// transparency

                writer.WriteEndElement();// phong
                writer.WriteEndElement();// technique

                if ((mat.m_PolyAttribs & 0xC0) == 0xC0)
                {
                    writer.WriteStartElement("extra");
                    writer.WriteStartElement("technique");
                    writer.WriteAttributeString("profile", "GOOGLEEARTH");
                    writer.WriteElementString("double_sided", "1");
                    writer.WriteEndElement();// technique
                    writer.WriteEndElement();// extra
                }

                writer.WriteEndElement();// profile_COMMON
                if ((mat.m_PolyAttribs & 0xC0) == 0xC0)
                {
                    writer.WriteStartElement("extra");
                    writer.WriteStartElement("technique");
                    writer.WriteAttributeString("profile", "MAX3D");
                    writer.WriteElementString("double_sided", "1");
                    writer.WriteEndElement();// technique
                    writer.WriteEndElement();// extra
                }
                writer.WriteEndElement();// effect
            }

            writer.WriteEndElement();
        }

        private static void WriteDAE_LibraryMaterials(XmlWriter writer, List<BMD.MaterialGroup> matGroups)
        {
            writer.WriteStartElement("library_materials");

            foreach (BMD.MaterialGroup mat in matGroups)
            {
                writer.WriteStartElement("material");
                writer.WriteAttributeString("id", mat.m_Name + "-material");
                writer.WriteAttributeString("name", mat.m_Name);
                writer.WriteStartElement("instance_effect");
                writer.WriteAttributeString("url", "#" + mat.m_Name + "-effect");
                writer.WriteEndElement();// instance_effect
                writer.WriteEndElement();// material
            }

            writer.WriteEndElement();
        }

        private static void WriteDAE_LibraryGeometries(XmlWriter writer, List<BoneForExport> bones)
        {
            writer.WriteStartElement("library_geometries");

            IEnumerable<BoneForExport> rootBones = bones.Where(bone0 => bone0.m_ParentOffset == 0);

            foreach (BoneForExport root in rootBones)
            {
                IEnumerable<BoneForExport> bonesInBranch = bones.Where(bone0 => bone0.m_RootName == root.m_Name);

                List<FaceVertex> verticesInBranch = new List<FaceVertex>();
                List<Vector3> positionsInBranch = new List<Vector3>();
                List<Vector3> normalsInBranch = new List<Vector3>();
                List<Vector2> texCoordsInBranch = new List<Vector2>();
                List<Color> vColoursInBranch = new List<Color>();
                foreach (BoneForExport bone in bonesInBranch)
                {
                    foreach (MaterialForExport mat in bone.m_Materials.Values)
                    {
                        foreach (ModelFace face in mat.m_Faces)
                        {
                            if (!normalsInBranch.Contains(face.m_Normal))
                                normalsInBranch.Add(face.m_Normal);

                            foreach (FaceVertex vert in face.m_Vertices)
                            {
                                if (!verticesInBranch.Contains(vert))
                                    verticesInBranch.Add(vert);
                            }
                        }
                    }
                }

                foreach (FaceVertex vert in verticesInBranch)
                {
                    positionsInBranch.Add(vert.m_Position);
                    texCoordsInBranch.Add(vert.m_TextureCoord);
                    vColoursInBranch.Add(vert.m_VertexColour);
                }

                writer.WriteStartElement("geometry");
                writer.WriteAttributeString("id", root.m_Name + "-mesh");
                writer.WriteAttributeString("name", root.m_Name + "-mesh");
                writer.WriteStartElement("mesh");

                WriteDAE_Source_positions(writer, root, positionsInBranch);
                WriteDAE_Source_normals(writer, root, normalsInBranch);
                WriteDAE_Source_map(writer, root, texCoordsInBranch);
                WriteDAE_Source_colors(writer, root, vColoursInBranch);

                writer.WriteStartElement("vertices");
                writer.WriteAttributeString("id", root.m_Name + "-mesh-vertices");
                writer.WriteStartElement("input");
                writer.WriteAttributeString("semantic", "POSITION");
                writer.WriteAttributeString("source", "#" + root.m_Name + "-mesh-positions");
                writer.WriteEndElement();// input
                writer.WriteEndElement();// vertices

                foreach (MaterialForExport mat in root.m_Materials.Values)
                {
                    List<ModelFace> faces = new List<ModelFace>();

                    foreach (BoneForExport bone in bonesInBranch)
                    {
                        if (bone.m_Materials.ContainsKey(mat.m_MaterialName))
                            faces.AddRange(bone.m_Materials[mat.m_MaterialName].m_Faces);
                    }

                    WriteDAE_Polylist(writer, root.m_Name, mat.m_MaterialName, faces, 
                        positionsInBranch, normalsInBranch, texCoordsInBranch, vColoursInBranch);
                }

                writer.WriteEndElement();// mesh
                writer.WriteEndElement();// geometry
            }

            writer.WriteEndElement();
        }

        private static void WriteDAE_Source_positions(XmlWriter writer, BoneForExport parent, List<Vector3> positionsInBranch)
        {
            writer.WriteStartElement("source");
            writer.WriteAttributeString("id", parent.m_Name + "-mesh-positions");
            writer.WriteStartElement("float_array");
            writer.WriteAttributeString("id", parent.m_Name + "-mesh-positions-array");
            writer.WriteAttributeString("count", (positionsInBranch.Count * 3).ToString());
            StringBuilder sb = new StringBuilder();
            foreach (Vector3 vert in positionsInBranch)
            {
                sb.Append(vert.X.ToString(usa) + " " + vert.Y.ToString(usa) + " " +
                    vert.Z.ToString(usa) + " ");
            }
            sb.Remove(sb.Length - 1, 1);// Remove extra space character at end
            writer.WriteString(sb.ToString());
            writer.WriteEndElement();// float_array

            WriteDAE_TechniqueCommon_Accessor_float(writer, new string[] { "X", "Y", "Z" }, parent.m_Name + "-mesh-positions-array",
                positionsInBranch.Count, 3);

            writer.WriteEndElement();// source
        }

        private static void WriteDAE_Source_normals(XmlWriter writer, BoneForExport parent, List<Vector3> normalsInBranch)
        {
            writer.WriteStartElement("source");
            writer.WriteAttributeString("id", parent.m_Name + "-mesh-normals");
            writer.WriteStartElement("float_array");
            writer.WriteAttributeString("id", parent.m_Name + "-mesh-normals-array");
            writer.WriteAttributeString("count", (normalsInBranch.Count * 3).ToString());
            StringBuilder sb = new StringBuilder();
            foreach (Vector3 normal in normalsInBranch)
            {
                sb.Append(normal.X.ToString(usa) + " " + normal.Y.ToString(usa) + " " +
                    normal.Z.ToString(usa) + " ");
            }
            sb.Remove(sb.Length - 1, 1);// Remove extra space character at end
            writer.WriteString(sb.ToString());
            writer.WriteEndElement();// float_array

            WriteDAE_TechniqueCommon_Accessor_float(writer, new string[] { "X", "Y", "Z" }, parent.m_Name + "-mesh-normals-array",
                normalsInBranch.Count, 3);

            writer.WriteEndElement();// source
        }

        private static void WriteDAE_Source_map(XmlWriter writer, BoneForExport parent, List<Vector2> texCoordsInBranch)
        {
            writer.WriteStartElement("source");
            writer.WriteAttributeString("id", parent.m_Name + "-mesh-map-0");
            writer.WriteStartElement("float_array");
            writer.WriteAttributeString("id", parent.m_Name + "-mesh-map-0-array");
            writer.WriteAttributeString("count", (texCoordsInBranch.Count * 2).ToString());
            StringBuilder sb = new StringBuilder();
            foreach (Vector2 texCoord in texCoordsInBranch)
            {
                sb.Append(texCoord.X.ToString(usa) + " " + texCoord.Y.ToString(usa) + " ");
            }
            sb.Remove(sb.Length - 1, 1);// Remove extra space character at end
            writer.WriteString(sb.ToString());
            writer.WriteEndElement();// float_array

            WriteDAE_TechniqueCommon_Accessor_float(writer, new string[] { "S", "T" }, parent.m_Name + "-mesh-map-0-array",
                texCoordsInBranch.Count, 2);

            writer.WriteEndElement();// source
        }

        private static void WriteDAE_Source_colors(XmlWriter writer, BoneForExport parent, List<Color> vColoursInBranch)
        {
            writer.WriteStartElement("source");
            writer.WriteAttributeString("id", parent.m_Name + "-mesh-colors");
            writer.WriteStartElement("float_array");
            writer.WriteAttributeString("id", parent.m_Name + "-mesh-colors-array");
            writer.WriteAttributeString("count", (vColoursInBranch.Count * 3).ToString());
            StringBuilder sb = new StringBuilder();
            foreach (Color vColour in vColoursInBranch)
            {
                sb.Append((vColour.R / 255.0f).ToString(usa) + " " + (vColour.G / 255.0f).ToString(usa) + " " +
                    (vColour.B / 255.0f).ToString(usa) + " ");
            }
            sb.Remove(sb.Length - 1, 1);// Remove extra space character at end
            writer.WriteString(sb.ToString());
            writer.WriteEndElement();// float_array

            WriteDAE_TechniqueCommon_Accessor_float(writer, new string[] { "R", "G", "B" }, parent.m_Name + "-mesh-colors-array",
                vColoursInBranch.Count, 3);

            writer.WriteEndElement();// source
        }

        private static void WriteDAE_TechniqueCommon_Accessor_float(XmlWriter writer, string[] paramNames, string source, int count, int stride)
        {
            writer.WriteStartElement("technique_common");
            writer.WriteStartElement("accessor");
            writer.WriteAttributeString("source", "#" + source);
            writer.WriteAttributeString("count", "" + count);
            writer.WriteAttributeString("stride", "" + stride);

            foreach (string param in paramNames)
            {
                writer.WriteStartElement("param");
                writer.WriteAttributeString("name", param);
                writer.WriteAttributeString("type", "float");
                writer.WriteEndElement();// param
            }

            writer.WriteEndElement();// accessor
            writer.WriteEndElement();// technique_common
        }

        private static void WriteDAE_Polylist(XmlWriter writer, string boneName, string matName, List<ModelFace> faces, 
            List<Vector3> positionsInBranch, List<Vector3> normalsInBranch, 
            List<Vector2> texCoordsInBranch, List<Color> vColoursInBranch)
        {
            writer.WriteStartElement("polylist");
            writer.WriteAttributeString("material", matName + "-material");
            writer.WriteAttributeString("count", "" + faces.Count);

            writer.WriteStartElement("input");
            writer.WriteAttributeString("semantic", "VERTEX");
            writer.WriteAttributeString("source", "#" + boneName + "-mesh-vertices");
            writer.WriteAttributeString("offset", "0");
            writer.WriteEndElement();// input

            writer.WriteStartElement("input");
            writer.WriteAttributeString("semantic", "NORMAL");
            writer.WriteAttributeString("source", "#" + boneName + "-mesh-normals");
            writer.WriteAttributeString("offset", "1");
            writer.WriteEndElement();// input

            writer.WriteStartElement("input");
            writer.WriteAttributeString("semantic", "TEXCOORD");
            writer.WriteAttributeString("source", "#" + boneName + "-mesh-map-0");
            writer.WriteAttributeString("offset", "2");
            writer.WriteAttributeString("set", "0");
            writer.WriteEndElement();// input

            writer.WriteStartElement("input");
            writer.WriteAttributeString("semantic", "COLOR");
            writer.WriteAttributeString("source", "#" + boneName + "-mesh-colors");
            writer.WriteAttributeString("offset", "3");
            writer.WriteEndElement();// input

            writer.WriteStartElement("vcount");
            StringBuilder sb = new StringBuilder();
            foreach (ModelFace face in faces)
            {
                sb.Append(face.m_Vertices.Count + " ");
            }
            sb.Remove(sb.Length - 1, 1);// Remove extra space character at end
            writer.WriteString(sb.ToString());
            writer.WriteEndElement();// vcount

            writer.WriteStartElement("p");
            sb = new StringBuilder();
            foreach (ModelFace face in faces)
            {
                foreach (FaceVertex vert in face.m_Vertices)
                {
                    sb.Append(positionsInBranch.IndexOf(vert.m_Position) + " " + normalsInBranch.IndexOf(face.m_Normal) + " " + 
                        texCoordsInBranch.IndexOf(vert.m_TextureCoord) + " " + vColoursInBranch.IndexOf(vert.m_VertexColour) + " ");
                }
            }
            sb.Remove(sb.Length - 1, 1);// Remove extra space character at end
            writer.WriteString(sb.ToString());
            writer.WriteEndElement();// p

            writer.WriteEndElement();// polylist
        }

        private static void WriteDAE_LibraryControllers(XmlWriter writer, List<BoneForExport> bones)
        {
            writer.WriteStartElement("library_controllers");

            IEnumerable<BoneForExport> rootBones = bones.Where(bone0 => bone0.m_ParentOffset == 0);

            foreach (BoneForExport root in rootBones)
            {
                IEnumerable<BoneForExport> bonesInBranch = bones.Where(bone0 => bone0.m_RootName == root.m_Name);

                List<FaceVertex> verticesInBranch = new List<FaceVertex>();
                foreach (BoneForExport bone in bonesInBranch)
                {
                    foreach (MaterialForExport mat in bone.m_Materials.Values)
                    {
                        foreach (ModelFace face in mat.m_Faces)
                        {
                            foreach (FaceVertex vert in face.m_Vertices)
                            {
                                if (!verticesInBranch.Contains(vert))
                                    verticesInBranch.Add(vert);
                            }
                        }
                    }
                }

                writer.WriteStartElement("controller");
                writer.WriteAttributeString("id", root.m_Name + "-skin");
                writer.WriteAttributeString("name", "skinCluster_" + root.m_ParentName);

                writer.WriteStartElement("skin");
                writer.WriteAttributeString("source", "#" + root.m_Name + "-mesh");
                writer.WriteElementString("bind_shape_matrix", "1 0 0 0 0 1 0 0 0 0 1 0 0 0 0 1");

                // Source Joints
                writer.WriteStartElement("source");
                writer.WriteAttributeString("id", root.m_Name + "-skin-joints");
                StringBuilder boneNamesArray = new StringBuilder();
                foreach (BoneForExport bone in bonesInBranch)
                    boneNamesArray.Append(bone.m_Name + " ");
                boneNamesArray.Remove(boneNamesArray.Length - 1, 1);// Remove extra space character at end
                writer.WriteStartElement("Name_array");
                writer.WriteAttributeString("id", root.m_Name + "-skin-joints-array");
                writer.WriteAttributeString("count", "" + bonesInBranch.Count());
                writer.WriteString(boneNamesArray.ToString());
                writer.WriteEndElement();// Name_array
                writer.WriteStartElement("technique_common");
                writer.WriteStartElement("accessor");
                writer.WriteAttributeString("source", "#" + root.m_Name + "-skin-joints-array");
                writer.WriteAttributeString("count", "" + bonesInBranch.Count());
                writer.WriteAttributeString("stride", "1");
                writer.WriteStartElement("param");
                writer.WriteAttributeString("name", "JOINT");
                writer.WriteAttributeString("type", "Name");
                writer.WriteEndElement();// param
                writer.WriteEndElement();// accessor
                writer.WriteEndElement();// technique_common
                writer.WriteEndElement();// source

                // Source Inverse Bind Poses
                writer.WriteComment("The inverse bind pose for a joint is given by the inverse bind pose of the parent joint (where present) " +
                    "multiplied by the inverse of the current joint's transformation matrix.");
                writer.WriteStartElement("source");
                writer.WriteAttributeString("id", root.m_Name + "-skin-bind_poses");

                List<Matrix4> invMatrices = new List<Matrix4>();
                foreach (BoneForExport bone in bonesInBranch)
                {
                    invMatrices.Add(bone.m_TotalReverseTransform);
                }
                StringBuilder sb = new StringBuilder();
                foreach (Matrix4 matrix in invMatrices)
                {
                    sb.Append(matrix.Column0.X.ToString(usa) + " " + matrix.Column0.Y.ToString(usa) + " " +
                        matrix.Column0.Z.ToString(usa) + " " + matrix.Column0.W.ToString(usa) + " ");
                    sb.Append(matrix.Column1.X.ToString(usa) + " " + matrix.Column1.Y.ToString(usa) + " " +
                        matrix.Column1.Z.ToString(usa) + " " + matrix.Column1.W.ToString(usa) + " ");
                    sb.Append(matrix.Column2.X.ToString(usa) + " " + matrix.Column2.Y.ToString(usa) + " " +
                        matrix.Column2.Z.ToString(usa) + " " + matrix.Column2.W.ToString(usa) + " ");
                    sb.Append(matrix.Column3.X.ToString(usa) + " " + matrix.Column3.Y.ToString(usa) + " " +
                        matrix.Column3.Z.ToString(usa) + " " + matrix.Column3.W.ToString(usa) + " ");
                }
                sb.Remove(sb.Length - 1, 1);// Remove extra space character at end

                writer.WriteStartElement("float_array");
                writer.WriteAttributeString("id", root.m_Name + "-skin-bind_poses-array");
                writer.WriteAttributeString("count", "" + (bonesInBranch.Count() * 16));
                writer.WriteString(sb.ToString());
                writer.WriteEndElement();// float_array
                writer.WriteStartElement("technique_common");
                writer.WriteStartElement("accessor");
                writer.WriteAttributeString("source", "#" + root.m_Name + "-skin-bind_poses-array");
                writer.WriteAttributeString("count", "" + bonesInBranch.Count());
                writer.WriteAttributeString("stride", "16");
                writer.WriteStartElement("param");
                writer.WriteAttributeString("name", "TRANSFORM");
                writer.WriteAttributeString("type", "float4x4");
                writer.WriteEndElement();// param
                writer.WriteEndElement();// accessor
                writer.WriteEndElement();// technique_common
                writer.WriteEndElement();// source

                // Source Vertex Joint Weights
                writer.WriteStartElement("source");
                writer.WriteAttributeString("id", root.m_Name + "-skin-weights");
                writer.WriteComment("The BMD format does not have any concept of 'weights', instead each vertex is assigned to one bone");
                writer.WriteStartElement("float_array");
                writer.WriteAttributeString("id", root.m_Name + "-skin-weights-array");
                writer.WriteAttributeString("count", "1");
                writer.WriteString("1");
                writer.WriteEndElement();// float_array
                writer.WriteStartElement("technique_common");
                writer.WriteStartElement("accessor");
                writer.WriteAttributeString("source", "#" + root.m_Name + "-skin-weights-array");
                writer.WriteAttributeString("count", "1");
                writer.WriteAttributeString("stride", "1");
                writer.WriteStartElement("param");
                writer.WriteAttributeString("name", "WEIGHT");
                writer.WriteAttributeString("type", "float");
                writer.WriteEndElement();// param
                writer.WriteEndElement();// accessor
                writer.WriteEndElement();// technique_common
                writer.WriteEndElement();// source

                writer.WriteStartElement("joints");
                writer.WriteStartElement("input");
                writer.WriteAttributeString("semantic", "JOINT");
                writer.WriteAttributeString("source", "#" + root.m_Name + "-skin-joints");
                writer.WriteEndElement();// input
                writer.WriteStartElement("input");
                writer.WriteAttributeString("semantic", "INV_BIND_MATRIX");
                writer.WriteAttributeString("source", "#" + root.m_Name + "-skin-bind_poses");
                writer.WriteEndElement();// input
                writer.WriteEndElement();// joints

                writer.WriteComment("Each vertex will be assigned to only one bone with a weight of 1");
                writer.WriteStartElement("vertex_weights");
                writer.WriteAttributeString("count", "" + verticesInBranch.Count);
                writer.WriteStartElement("input");
                writer.WriteAttributeString("semantic", "JOINT");
                writer.WriteAttributeString("source", "#" + root.m_Name + "-skin-joints");
                writer.WriteAttributeString("offset", "0");
                writer.WriteEndElement();// input
                writer.WriteStartElement("input");
                writer.WriteAttributeString("semantic", "WEIGHT");
                writer.WriteAttributeString("source", "#" + root.m_Name + "-skin-weights");
                writer.WriteAttributeString("offset", "1");
                writer.WriteEndElement();// input
                writer.WriteStartElement("vcount");
                StringBuilder vcount = new StringBuilder();
                foreach (FaceVertex vert in verticesInBranch)
                    vcount.Append("1 ");
                vcount.Remove(vcount.Length - 1, 1);// Remove extra space character at end
                writer.WriteString(vcount.ToString());
                writer.WriteEndElement();// vcount
                writer.WriteComment("This list contains two values for each vertex, the first is the bone ID and the second " +
                    "is the index of the vertex weight '1' (0)");
                writer.WriteStartElement("v");
                StringBuilder v = new StringBuilder();
                foreach (FaceVertex vert in verticesInBranch)
                    v.Append(vert.m_BoneID + " 0 ");
                v.Remove(v.Length - 1, 1);// Remove extra space character at end
                writer.WriteString(v.ToString());
                writer.WriteEndElement();// v
                writer.WriteEndElement();// vertex_weights

                writer.WriteEndElement();// skin

                writer.WriteEndElement();// controller
            }

            writer.WriteEndElement();// library_controllers
        }

        private static void WriteDAE_LibraryVisualScenes(XmlWriter writer, List<BoneForExport> bones, bool useSRTMatricesForTransforms)
        {
            writer.WriteStartElement("library_visual_scenes");
            writer.WriteStartElement("visual_scene");
            writer.WriteAttributeString("id", "Scene");
            writer.WriteAttributeString("name", "Scene");

            IEnumerable<BoneForExport> rootBones = bones.Where(bone0 => bone0.m_ParentOffset == 0);

            foreach (BoneForExport root in rootBones)
            {
                IEnumerable<BoneForExport> bonesInBranch = bones.Where(bone0 => bone0.m_RootName == root.m_Name);

                if (useSRTMatricesForTransforms)
                {
                    writer.WriteComment("This lists each of the 'bones' in the BMD model, listing each bone's individual " +
                        "Scale, Rotation (XYZ), Translation transformation as an SRT matrix (multiplied in that order). " +
                        "This will then be multiplied recursively by the parents' transformations to get " +
                        "the final transformation for this joint.");
                }
                else
                {
                    writer.WriteComment("This lists each of the 'bones' in the BMD model, listing each bone's individual " +
                        "Scale, Rotation, Translation transformation in the reverse of the order in which they should be multiplied to form an SRT " +
                        " matrix. This will then be multiplied recursively by the parents' transformations to get " +
                        "the final transformation for this joint.");
                }

                WriteDAE_Node_joint(writer, root, useSRTMatricesForTransforms);

                writer.WriteStartElement("node");
                writer.WriteAttributeString("id", root.m_Name + "-node");
                writer.WriteAttributeString("name", root.m_Name + "-node");
                writer.WriteAttributeString("type", "NODE");

                writer.WriteStartElement("instance_controller");
                writer.WriteAttributeString("url", "#" + root.m_Name + "-skin");
                writer.WriteElementString("skeleton", "#" + root.m_Name);
                writer.WriteStartElement("bind_material");
                writer.WriteStartElement("technique_common");

                foreach (MaterialForExport mat in root.m_Materials.Values)
                {
                    writer.WriteStartElement("instance_material");
                    writer.WriteAttributeString("symbol", mat.m_MaterialName + "-material");
                    writer.WriteAttributeString("target", "#" + mat.m_MaterialName + "-material");

                    writer.WriteStartElement("bind_vertex_input");
                    //writer.WriteAttributeString("semantic", parent.m_Name + "-mesh-map-0");
                    writer.WriteAttributeString("semantic", "UVMap");
                    writer.WriteAttributeString("input_semantic", "TEXCOORD");
                    writer.WriteAttributeString("input_set", "0");

                    writer.WriteEndElement();// bind_vertex_input
                    writer.WriteEndElement();// instance_material
                }

                writer.WriteEndElement();// technique_common
                writer.WriteEndElement();// bind_material
                writer.WriteEndElement();// instance_controller

                writer.WriteEndElement();// node
            }

            writer.WriteEndElement();// visual_scene
            writer.WriteEndElement();// library_visual_scenes

            writer.WriteStartElement("scene");
            writer.WriteStartElement("instance_visual_scene");
            writer.WriteAttributeString("url", "#Scene");

            writer.WriteEndElement();// instance_visual_scene
            writer.WriteEndElement();// scene
        }

        private static void WriteDAE_Node_joint(XmlWriter writer, BoneForExport parent, bool useSRTMatricesForTransforms)
        {
            writer.WriteStartElement("node");

            writer.WriteAttributeString("id", parent.m_Name);

            writer.WriteAttributeString("name", parent.m_Name);
            writer.WriteAttributeString("sid", parent.m_Name);
            writer.WriteAttributeString("type", "JOINT");

            if (useSRTMatricesForTransforms)
                WriteDAE_Node_transformationMatrix(writer, parent);
            else
                WriteDAE_Node_transformationSRT(writer, parent);

            foreach (BoneForExport bone in parent.m_Children)
            {
                WriteDAE_Node_joint(writer, bone, useSRTMatricesForTransforms);
            }

            writer.WriteEndElement();// node
        }

        private static void WriteDAE_Node_transformationMatrix(XmlWriter writer, BoneForExport parent)
        {
            writer.WriteStartElement("matrix");
            writer.WriteAttributeString("sid", "transform");
            StringBuilder sb = new StringBuilder();
            Matrix4 matrix = parent.m_TotalReverseTransform;
            matrix.Invert();
            sb.Append(matrix.Column0.X.ToString(usa) + " " + matrix.Column0.Y.ToString(usa) + " " +
                matrix.Column0.Z.ToString(usa) + " " + matrix.Column0.W.ToString(usa) + " ");
            sb.Append(matrix.Column1.X.ToString(usa) + " " + matrix.Column1.Y.ToString(usa) + " " +
                matrix.Column1.Z.ToString(usa) + " " + matrix.Column1.W.ToString(usa) + " ");
            sb.Append(matrix.Column2.X.ToString(usa) + " " + matrix.Column2.Y.ToString(usa) + " " +
                matrix.Column2.Z.ToString(usa) + " " + matrix.Column2.W.ToString(usa) + " ");
            sb.Append(matrix.Column3.X.ToString(usa) + " " + matrix.Column3.Y.ToString(usa) + " " +
                matrix.Column3.Z.ToString(usa) + " " + matrix.Column3.W.ToString(usa) + " ");
            sb.Remove(sb.Length - 1, 1);// Remove extra space character at end
            writer.WriteString(sb.ToString());
            writer.WriteEndElement();// matrix
        }

        private static void WriteDAE_Node_transformationSRT(XmlWriter writer, BoneForExport parent)
        {
            Vector3 scale = new Vector3((float)(int)parent.m_Scale[0] / 4096.0f, (float)(int)parent.m_Scale[1] / 4096.0f, (float)(int)parent.m_Scale[2] / 4096.0f);
            Vector3 rotRad = new Vector3(((float)(short)parent.m_Rotation[0] * (float)Math.PI) / 2048.0f, ((float)(short)parent.m_Rotation[1] * (float)Math.PI) / 2048.0f, ((float)(short)parent.m_Rotation[2] * (float)Math.PI) / 2048.0f);
            // COLLADA uses degrees for angles
            Vector3 rotDeg = new Vector3(rotRad.X * Helper.Rad2Deg, rotRad.Y * Helper.Rad2Deg, rotRad.Z * Helper.Rad2Deg);
            Vector3 trans = new Vector3((float)(int)parent.m_Translation[0] / 4096.0f, (float)(int)parent.m_Translation[1] / 4096.0f, (float)(int)parent.m_Translation[2] / 4096.0f);

            writer.WriteStartElement("translate");
            writer.WriteAttributeString("sid", "translate");
            writer.WriteString(trans.X.ToString(usa) + " " + trans.Y.ToString(usa) + " " + trans.Z.ToString(usa));
            writer.WriteEndElement();// translate

            writer.WriteStartElement("rotate");
            writer.WriteAttributeString("sid", "rotationZ");
            writer.WriteString("0 0 1 " + rotDeg.Z);
            writer.WriteEndElement();// rotate

            writer.WriteStartElement("rotate");
            writer.WriteAttributeString("sid", "rotationY");
            writer.WriteString("0 1 0 " + rotDeg.Y);
            writer.WriteEndElement();// rotate

            writer.WriteStartElement("rotate");
            writer.WriteAttributeString("sid", "rotationX");
            writer.WriteString("1 0 0 " + rotDeg.X);
            writer.WriteEndElement();// rotate

            writer.WriteStartElement("scale");
            writer.WriteAttributeString("sid", "scale");
            writer.WriteString(scale.X.ToString(usa) + " " + scale.Y.ToString(usa) + " " + scale.Z.ToString(usa));
            writer.WriteEndElement();// scale
        }

        private static void WriteDAE_LibraryAnimations(XmlWriter writer, BMD.ModelChunk[] chunks, List<BoneForExport> bones,
            BCA animation, bool useSRTMatricesForTransforms)
        {
            writer.WriteStartElement("library_animations");

            List<BCA.SRTContainer[]> localSRTValues = new List<BCA.SRTContainer[]>();
            List<Matrix4[]> localSRTMatrices = new List<Matrix4[]>();
            for (int i = 0; i < animation.m_NumFrames; i++)
            {
                localSRTValues.Add(animation.GetAllLocalSRTValuesForFrame(chunks, i));
                localSRTMatrices.Add(animation.GetAllLocalMatricesForFrame(chunks, i));
            }

            // For each joint, write and <animation> each for translate, rotationZ Y and X and scale
            int ind = 0;
            foreach (BoneForExport bone in bones)
            {
                if (useSRTMatricesForTransforms)
                {
                    writer.WriteStartElement("animation");
                    writer.WriteAttributeString("id", bone.m_Name + "_transform");
                    WriteDAE_Animation_Source_time(writer, animation, bone, "transform");
                    WriteDAE_Animation_Source_matrixOutput(writer, animation, localSRTMatrices, ind, bone);
                    WriteDAE_Animation_Source_interpolations(writer, animation, bone, "transform");
                    WriteDAE_Animation_Sampler(writer, bone, "transform");
                    WriteDAE_Animation_Channel(writer, bone, "transform", "transform");
                    writer.WriteEndElement();// animation
                }
                else
                {
                    writer.WriteStartElement("animation");
                    writer.WriteAttributeString("id", bone.m_Name + "_translate");
                    WriteDAE_Animation_Source_time(writer, animation, bone, "translate");
                    WriteDAE_Animation_Source_translationOutput(writer, animation, localSRTValues, ind, bone);
                    WriteDAE_Animation_Source_interpolations(writer, animation, bone, "translate");
                    WriteDAE_Animation_Sampler(writer, bone, "translate");
                    WriteDAE_Animation_Channel(writer, bone, "translate", "translate");
                    writer.WriteEndElement();// animation

                    writer.WriteStartElement("animation");
                    writer.WriteAttributeString("id", bone.m_Name + "_rotationZ");
                    WriteDAE_Animation_Source_time(writer, animation, bone, "rotationZ");
                    WriteDAE_Animation_Source_rotationOutput(writer, animation, localSRTValues, ind, bone, "Z");
                    WriteDAE_Animation_Source_interpolations(writer, animation, bone, "rotationZ");
                    WriteDAE_Animation_Sampler(writer, bone, "rotationZ");
                    WriteDAE_Animation_Channel(writer, bone, "rotationZ", "rotationZ.ANGLE");
                    writer.WriteEndElement();// animation

                    writer.WriteStartElement("animation");
                    writer.WriteAttributeString("id", bone.m_Name + "_rotationY");
                    WriteDAE_Animation_Source_time(writer, animation, bone, "rotationY");
                    WriteDAE_Animation_Source_rotationOutput(writer, animation, localSRTValues, ind, bone, "Y");
                    WriteDAE_Animation_Source_interpolations(writer, animation, bone, "rotationY");
                    WriteDAE_Animation_Sampler(writer, bone, "rotationY");
                    WriteDAE_Animation_Channel(writer, bone, "rotationY", "rotationY.ANGLE");
                    writer.WriteEndElement();// animation

                    writer.WriteStartElement("animation");
                    writer.WriteAttributeString("id", bone.m_Name + "_rotationX");
                    WriteDAE_Animation_Source_time(writer, animation, bone, "rotationX");
                    WriteDAE_Animation_Source_rotationOutput(writer, animation, localSRTValues, ind, bone, "X");
                    WriteDAE_Animation_Source_interpolations(writer, animation, bone, "rotationX");
                    WriteDAE_Animation_Sampler(writer, bone, "rotationX");
                    WriteDAE_Animation_Channel(writer, bone, "rotationX", "rotationX.ANGLE");
                    writer.WriteEndElement();// animation

                    writer.WriteStartElement("animation");
                    writer.WriteAttributeString("id", bone.m_Name + "_scale");
                    WriteDAE_Animation_Source_time(writer, animation, bone, "scale");
                    WriteDAE_Animation_Source_scaleOutput(writer, animation, localSRTValues, ind, bone);
                    WriteDAE_Animation_Source_interpolations(writer, animation, bone, "scale");
                    WriteDAE_Animation_Sampler(writer, bone, "scale");
                    WriteDAE_Animation_Channel(writer, bone, "scale", "scale");
                    writer.WriteEndElement();// animation
                }

                ind++;
            }

            writer.WriteEndElement();// library_animations
        }

        private static void WriteDAE_Animation_Source_matrixOutput(XmlWriter writer, BCA animation, List<Matrix4[]> localSRTMatrices,
            int ind, BoneForExport bone)
        {
            writer.WriteStartElement("source");
            writer.WriteAttributeString("id", bone.m_Name + "_transform-output");
            writer.WriteStartElement("float_array");
            writer.WriteAttributeString("id", bone.m_Name + "_transform-output-array");
            writer.WriteAttributeString("count", "" + (animation.m_NumFrames * 16));
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < animation.m_NumFrames; i++)
            {
                Matrix4 matrix = localSRTMatrices[i][ind];
                sb.Append(matrix.Column0.X.ToString(usa) + " " + matrix.Column0.Y.ToString(usa) + " " +
                    matrix.Column0.Z.ToString(usa) + " " + matrix.Column0.W.ToString(usa) + " ");
                sb.Append(matrix.Column1.X.ToString(usa) + " " + matrix.Column1.Y.ToString(usa) + " " +
                    matrix.Column1.Z.ToString(usa) + " " + matrix.Column1.W.ToString(usa) + " ");
                sb.Append(matrix.Column2.X.ToString(usa) + " " + matrix.Column2.Y.ToString(usa) + " " +
                    matrix.Column2.Z.ToString(usa) + " " + matrix.Column2.W.ToString(usa) + " ");
                sb.Append(matrix.Column3.X.ToString(usa) + " " + matrix.Column3.Y.ToString(usa) + " " +
                    matrix.Column3.Z.ToString(usa) + " " + matrix.Column3.W.ToString(usa) + " ");
            }
            sb.Remove(sb.Length - 1, 1);// Remove extra space character at end
            writer.WriteString(sb.ToString());
            writer.WriteEndElement();// float_array
            writer.WriteStartElement("technique_common");
            writer.WriteStartElement("accessor");
            writer.WriteAttributeString("count", "" + animation.m_NumFrames);
            writer.WriteAttributeString("offset", "0");
            writer.WriteAttributeString("source", "#" + bone.m_Name + "_transform-output-array");
            writer.WriteAttributeString("stride", "16");
            WriteDAE_Animation_Param_nameType(writer, "TRANSFORM", "float4x4");
            writer.WriteEndElement();// accessor
            writer.WriteEndElement();// technique_common
            writer.WriteEndElement();// source
        }
        /* 
         * EXPORING ANIMATIONS AS SEPARATE SCALE, ROTATION AND TRANSLATION NOT CURRENTLY WORKING
         */ 
        private static void WriteDAE_Animation_Source_translationOutput(XmlWriter writer, BCA animation, List<BCA.SRTContainer[]> localSRTValues,
            int ind, BoneForExport bone)
        {
            writer.WriteStartElement("source");
            writer.WriteAttributeString("id", bone.m_Name + "_translate-output");
            writer.WriteStartElement("float_array");
            writer.WriteAttributeString("id", bone.m_Name + "_translate-output-array");
            writer.WriteAttributeString("count", "" + (animation.m_NumFrames * 3));
            StringBuilder trans = new StringBuilder();
            for (int i = 0; i < animation.m_NumFrames; i++)
            {
                float x = localSRTValues[i][ind].m_Translation.X;
                float y = localSRTValues[i][ind].m_Translation.Y;
                float z = localSRTValues[i][ind].m_Translation.Z;

                trans.Append(x.ToString(usa) + " " + y.ToString(usa) + " " + z.ToString(usa) + " ");
            }
            trans.Remove(trans.Length - 1, 1);// Remove extra space character at end
            writer.WriteString(trans.ToString());
            writer.WriteEndElement();// float_array
            writer.WriteStartElement("technique_common");
            writer.WriteStartElement("accessor");
            writer.WriteAttributeString("count", "" + animation.m_NumFrames);
            writer.WriteAttributeString("offset", "0");
            writer.WriteAttributeString("source", "#" + bone.m_Name + "_translate-output-array");
            writer.WriteAttributeString("stride", "3");
            WriteDAE_Animation_Param_nameType(writer, "X", "float");
            WriteDAE_Animation_Param_nameType(writer, "Y", "float");
            WriteDAE_Animation_Param_nameType(writer, "Z", "float");
            writer.WriteEndElement();// accessor
            writer.WriteEndElement();// technique_common
            writer.WriteEndElement();// source
        }

        private static void WriteDAE_Animation_Source_rotationOutput(XmlWriter writer, BCA animation, List<BCA.SRTContainer[]> localSRTValues,
            int ind, BoneForExport bone, string component)
        {
            writer.WriteStartElement("source");
            writer.WriteAttributeString("id", bone.m_Name + "_rotation" + component + "-output");
            writer.WriteStartElement("float_array");
            writer.WriteAttributeString("id", bone.m_Name + "_rotation" + component + "-output-array");
            writer.WriteAttributeString("count", "" + animation.m_NumFrames);
            StringBuilder rot = new StringBuilder();
            for (int i = 0; i < animation.m_NumFrames; i++)
            {
                float angle = 0.0f;
                Vector3 rotDeg = localSRTValues[i][ind].GetRotationInDegrees();

                switch (component.ToUpperInvariant())
                {
                    case "X":
                        angle = rotDeg.X;
                        break;
                    case "Y":
                        angle = rotDeg.Y;
                        break;
                    case "Z":
                        angle = rotDeg.Z;
                        break;
                }

                rot.Append(angle.ToString(usa) + " ");
            }
            rot.Remove(rot.Length - 1, 1);// Remove extra space character at end
            writer.WriteString(rot.ToString());
            writer.WriteEndElement();// float_array
            writer.WriteStartElement("technique_common");
            writer.WriteStartElement("accessor");
            writer.WriteAttributeString("count", "" + animation.m_NumFrames);
            writer.WriteAttributeString("offset", "0");
            writer.WriteAttributeString("source", "#" + bone.m_Name + "_rotation" + component + "-output-array");
            writer.WriteAttributeString("stride", "1");
            WriteDAE_Animation_Param_nameType(writer, "ANGLE", "float");
            writer.WriteEndElement();// accessor
            writer.WriteEndElement();// technique_common
            writer.WriteEndElement();// source
        }

        private static void WriteDAE_Animation_Source_scaleOutput(XmlWriter writer, BCA animation, List<BCA.SRTContainer[]> localSRTValues,
            int ind, BoneForExport bone)
        {
            writer.WriteStartElement("source");
            writer.WriteAttributeString("id", bone.m_Name + "_scale-output");
            writer.WriteStartElement("float_array");
            writer.WriteAttributeString("id", bone.m_Name + "_scale-output-array");
            writer.WriteAttributeString("count", "" + (animation.m_NumFrames * 3));
            StringBuilder scale = new StringBuilder();
            for (int i = 0; i < animation.m_NumFrames; i++)
            {
                float x = localSRTValues[i][ind].m_Scale.X;
                float y = localSRTValues[i][ind].m_Scale.Y;
                float z = localSRTValues[i][ind].m_Scale.Z;

                scale.Append(x.ToString(usa) + " " + y.ToString(usa) + " " + z.ToString(usa) + " ");
            }
            scale.Remove(scale.Length - 1, 1);// Remove extra space character at end
            writer.WriteString(scale.ToString());
            writer.WriteEndElement();// float_array
            writer.WriteStartElement("technique_common");
            writer.WriteStartElement("accessor");
            writer.WriteAttributeString("count", "" + animation.m_NumFrames);
            writer.WriteAttributeString("offset", "0");
            writer.WriteAttributeString("source", "#" + bone.m_Name + "_scale-output-array");
            writer.WriteAttributeString("stride", "3");
            WriteDAE_Animation_Param_nameType(writer, "X", "float");
            WriteDAE_Animation_Param_nameType(writer, "Y", "float");
            WriteDAE_Animation_Param_nameType(writer, "Z", "float");
            writer.WriteEndElement();// accessor
            writer.WriteEndElement();// technique_common
            writer.WriteEndElement();// source
        }

        private static void WriteDAE_Animation_Param_nameType(XmlWriter writer, string name, string type)
        {
            writer.WriteStartElement("param");
            writer.WriteAttributeString("name", name);
            writer.WriteAttributeString("type", type);
            writer.WriteEndElement();// param
        }

        private static void WriteDAE_Animation_Source_time(XmlWriter writer, BCA animation, BoneForExport bone, string transformationName)
        {
            writer.WriteStartElement("source");
            writer.WriteAttributeString("id", bone.m_Name + "_" + transformationName + "-input");
            writer.WriteStartElement("float_array");
            writer.WriteAttributeString("id", bone.m_Name + "_" + transformationName + "-input-array");
            writer.WriteAttributeString("count", "" + animation.m_NumFrames);
            StringBuilder time = new StringBuilder();
            for (int i = 0; i < animation.m_NumFrames; i++)
            {
                time.Append(((float)(i * (1.0f / 30.0f))).ToString(usa) + " ");
            }
            time.Remove(time.Length - 1, 1);// Remove extra space character at end
            writer.WriteString(time.ToString());
            writer.WriteEndElement();// float_array
            writer.WriteStartElement("technique_common");
            writer.WriteStartElement("accessor");
            writer.WriteAttributeString("count", "" + animation.m_NumFrames);
            writer.WriteAttributeString("offset", "0");
            writer.WriteAttributeString("source", "#" + bone.m_Name + "_" + transformationName + "-input-array");
            writer.WriteAttributeString("stride", "1");
            WriteDAE_Animation_Param_nameType(writer, "TIME", "float");
            writer.WriteEndElement();// accessor
            writer.WriteEndElement();// technique_common
            writer.WriteEndElement();// source
        }

        private static void WriteDAE_Animation_Source_interpolations(XmlWriter writer, BCA animation, 
            BoneForExport bone, string transformationName)
        {
            writer.WriteStartElement("source");
            writer.WriteAttributeString("id", bone.m_Name + "_" + transformationName + "-interpolations");
            writer.WriteStartElement("Name_array");
            writer.WriteAttributeString("id", bone.m_Name + "_" + transformationName + "-interpolations-array");
            writer.WriteAttributeString("count", "" + animation.m_NumFrames);
            StringBuilder interp = new StringBuilder();
            for (int i = 0; i < animation.m_NumFrames; i++)
            {
                interp.Append("LINEAR ");
            }
            interp.Remove(interp.Length - 1, 1);// Remove extra space character at end
            writer.WriteString(interp.ToString());
            writer.WriteEndElement();// Name_array
            writer.WriteStartElement("technique_common");
            writer.WriteStartElement("accessor");
            writer.WriteAttributeString("count", "" + animation.m_NumFrames);
            writer.WriteAttributeString("offset", "0");
            writer.WriteAttributeString("source", "#" + bone.m_Name + "_" + transformationName + "-interpolations-array");
            writer.WriteAttributeString("stride", "1");
            WriteDAE_Animation_Param_nameType(writer, "INTERPOLATION", "Name");
            writer.WriteEndElement();// accessor
            writer.WriteEndElement();// technique_common
            writer.WriteEndElement();// source
        }

        private static void WriteDAE_Animation_Sampler(XmlWriter writer, BoneForExport bone, string transformationName)
        {
            writer.WriteStartElement("sampler");
            writer.WriteAttributeString("id", bone.m_Name + "_" + transformationName + "-sampler");
            writer.WriteStartElement("input");
            writer.WriteAttributeString("semantic", "INPUT");
            writer.WriteAttributeString("source", "#" + bone.m_Name + "_" + transformationName + "-input");
            writer.WriteEndElement();// input
            writer.WriteStartElement("input");
            writer.WriteAttributeString("semantic", "OUTPUT");
            writer.WriteAttributeString("source", "#" + bone.m_Name + "_" + transformationName + "-output");
            writer.WriteEndElement();// input
            writer.WriteStartElement("input");
            writer.WriteAttributeString("semantic", "INTERPOLATION");
            writer.WriteAttributeString("source", "#" + bone.m_Name + "_" + transformationName + "-interpolations");
            writer.WriteEndElement();// input
            writer.WriteEndElement();// sampler
        }

        private static void WriteDAE_Animation_Channel(XmlWriter writer, BoneForExport bone, string samplerSuffix, string transformationName)
        {
            writer.WriteStartElement("channel");
            writer.WriteAttributeString("source", "#" + bone.m_Name + "_" + samplerSuffix + "-sampler");
            writer.WriteAttributeString("target", bone.m_Name + "/" + transformationName);
            writer.WriteEndElement();// channel
        }

        /* Returns a list of bones containing only their own geometry separated by material.
         * Go through each vertexlist in the BMD file and split it into separate faces, then assign 
         * that face to its bone as defined by its Matrix ID. 
         * Necessary as a vertexlist can contain faces from multiple bones and geometry is currently assigned 
         * only to the parent bone.
        */
        private static List<BoneForExport> GetBonesForExport(BMD model)
        {
            Dictionary<uint, BoneForExport> bones = new Dictionary<uint, BoneForExport>();
            uint numBones = model.m_NumModelChunks;

            for (int i = 0; i < model.m_ModelChunks.Length; i++)
            {
                bones.Add(model.m_ModelChunks[i].m_ID,
                    new BoneForExport(model.m_ModelChunks[i].m_Name, model.m_ModelChunks[i].m_ParentOffset,
                        model.m_ModelChunks[i].m_SiblingOffset, model.m_ModelChunks[i].m_HasChildren,
                        model.m_ModelChunks[i].m_20_12Scale, model.m_ModelChunks[i].m_4_12Rotation, model.m_ModelChunks[i].m_20_12Translation));
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
                                        verts.Add(new FaceVertex(vtxList[0].m_Position, vtxList[0].m_TexCoord, vtxList[0].m_Color,
                                            model.m_ModelChunks[i].m_MatGroups[j].m_BoneIDs[vtxList[0].m_MatrixID]));
                                        verts.Add(new FaceVertex(vtxList[1].m_Position, vtxList[1].m_TexCoord, vtxList[1].m_Color,
                                            model.m_ModelChunks[i].m_MatGroups[j].m_BoneIDs[vtxList[1].m_MatrixID]));
                                        verts.Add(new FaceVertex(vtxList[2].m_Position, vtxList[2].m_TexCoord, vtxList[2].m_Color,
                                            model.m_ModelChunks[i].m_MatGroups[j].m_BoneIDs[vtxList[2].m_MatrixID]));
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
                                            verts.Add(new FaceVertex(vtxList[b].m_Position, vtxList[b].m_TexCoord, vtxList[b].m_Color,
                                                model.m_ModelChunks[i].m_MatGroups[j].m_BoneIDs[vtxList[b].m_MatrixID]));
                                            verts.Add(new FaceVertex(vtxList[b + 1].m_Position, vtxList[b + 1].m_TexCoord, vtxList[b + 1].m_Color,
                                                model.m_ModelChunks[i].m_MatGroups[j].m_BoneIDs[vtxList[b + 1].m_MatrixID]));
                                            verts.Add(new FaceVertex(vtxList[b + 2].m_Position, vtxList[b + 2].m_TexCoord, vtxList[b + 2].m_Color,
                                                model.m_ModelChunks[i].m_MatGroups[j].m_BoneIDs[vtxList[b + 2].m_MatrixID]));
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
                                        verts.Add(new FaceVertex(vtxList[0].m_Position, vtxList[0].m_TexCoord, vtxList[0].m_Color,
                                            model.m_ModelChunks[i].m_MatGroups[j].m_BoneIDs[vtxList[0].m_MatrixID]));
                                        verts.Add(new FaceVertex(vtxList[1].m_Position, vtxList[1].m_TexCoord, vtxList[1].m_Color,
                                            model.m_ModelChunks[i].m_MatGroups[j].m_BoneIDs[vtxList[1].m_MatrixID]));
                                        verts.Add(new FaceVertex(vtxList[2].m_Position, vtxList[2].m_TexCoord, vtxList[2].m_Color,
                                            model.m_ModelChunks[i].m_MatGroups[j].m_BoneIDs[vtxList[2].m_MatrixID]));
                                        verts.Add(new FaceVertex(vtxList[3].m_Position, vtxList[3].m_TexCoord, vtxList[3].m_Color,
                                            model.m_ModelChunks[i].m_MatGroups[j].m_BoneIDs[vtxList[3].m_MatrixID]));
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
                                            verts.Add(new FaceVertex(vtxList[b].m_Position, vtxList[b].m_TexCoord, vtxList[b].m_Color,
                                                model.m_ModelChunks[i].m_MatGroups[j].m_BoneIDs[vtxList[b].m_MatrixID]));
                                            verts.Add(new FaceVertex(vtxList[b + 1].m_Position, vtxList[b + 1].m_TexCoord, vtxList[b + 1].m_Color,
                                                model.m_ModelChunks[i].m_MatGroups[j].m_BoneIDs[vtxList[b + 1].m_MatrixID]));
                                            verts.Add(new FaceVertex(vtxList[b + 2].m_Position, vtxList[b + 2].m_TexCoord, vtxList[b + 2].m_Color,
                                                model.m_ModelChunks[i].m_MatGroups[j].m_BoneIDs[vtxList[b + 2].m_MatrixID]));
                                            verts.Add(new FaceVertex(vtxList[b + 3].m_Position, vtxList[b + 3].m_TexCoord, vtxList[b + 3].m_Color,
                                                model.m_ModelChunks[i].m_MatGroups[j].m_BoneIDs[vtxList[b + 3].m_MatrixID]));
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
                                            verts.Add(new FaceVertex(vtxList[n].m_Position, vtxList[n].m_TexCoord, vtxList[n].m_Color,
                                                model.m_ModelChunks[i].m_MatGroups[j].m_BoneIDs[vtxList[n].m_MatrixID]));
                                            verts.Add(new FaceVertex(vtxList[n + 1].m_Position, vtxList[n + 1].m_TexCoord, vtxList[n + 1].m_Color,
                                                model.m_ModelChunks[i].m_MatGroups[j].m_BoneIDs[vtxList[n + 1].m_MatrixID]));
                                            verts.Add(new FaceVertex(vtxList[n + 2].m_Position, vtxList[n + 2].m_TexCoord, vtxList[n + 2].m_Color,
                                                model.m_ModelChunks[i].m_MatGroups[j].m_BoneIDs[vtxList[n + 2].m_MatrixID]));
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
                                            verts.Add(new FaceVertex(vtxList[n + 2].m_Position, vtxList[n + 2].m_TexCoord, vtxList[n + 2].m_Color,
                                                model.m_ModelChunks[i].m_MatGroups[j].m_BoneIDs[vtxList[n + 2].m_MatrixID]));
                                            verts.Add(new FaceVertex(vtxList[n + 1].m_Position, vtxList[n + 1].m_TexCoord, vtxList[n + 1].m_Color,
                                                model.m_ModelChunks[i].m_MatGroups[j].m_BoneIDs[vtxList[n + 1].m_MatrixID]));
                                            verts.Add(new FaceVertex(vtxList[n].m_Position, vtxList[n].m_TexCoord, vtxList[n].m_Color,
                                                model.m_ModelChunks[i].m_MatGroups[j].m_BoneIDs[vtxList[n].m_MatrixID]));
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
                                        verts.Add(new FaceVertex(vtxList[p].m_Position, vtxList[p].m_TexCoord, vtxList[p].m_Color,
                                            model.m_ModelChunks[i].m_MatGroups[j].m_BoneIDs[vtxList[p].m_MatrixID]));
                                        verts.Add(new FaceVertex(vtxList[p + 1].m_Position, vtxList[p + 1].m_TexCoord, vtxList[p + 1].m_Color,
                                            model.m_ModelChunks[i].m_MatGroups[j].m_BoneIDs[vtxList[p + 1].m_MatrixID]));
                                        verts.Add(new FaceVertex(vtxList[p + 3].m_Position, vtxList[p + 3].m_TexCoord, vtxList[p + 3].m_Color,
                                            model.m_ModelChunks[i].m_MatGroups[j].m_BoneIDs[vtxList[p + 3].m_MatrixID]));
                                        verts.Add(new FaceVertex(vtxList[p + 2].m_Position, vtxList[p + 2].m_TexCoord, vtxList[p + 2].m_Color,
                                            model.m_ModelChunks[i].m_MatGroups[j].m_BoneIDs[vtxList[p + 2].m_MatrixID]));
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

            string currentRoot = "";
            for (int i = 0; i < bones.Values.Count; i++)
            {
                BoneForExport bone = bones.Values.ElementAt(i);

                if (bone.m_ParentOffset == 0)
                {
                    currentRoot = bone.m_Name;
                    bone.m_ParentName = bone.m_Name;
                    bone.m_RootName = bone.m_Name;
                }
                else
                {
                    bones.Values.ElementAt(i + bone.m_ParentOffset).m_Children.Add(bones.Values.ElementAt(i));
                    bone.m_ParentName = bones.Values.ElementAt(i + bone.m_ParentOffset).m_Name;
                    bone.m_RootName = currentRoot;
                }
            }

            for (int i = 0; i < bones.Count; i++)
            {
                BoneForExport bone = bones.Values.ElementAt(i);

                Vector3 invScale = new Vector3((float)(1f / (((int)bone.m_Scale[0]) / 4096.0f)), (float)(1f / (((int)bone.m_Scale[1]) / 4096.0f)), (float)(1f / (((int)bone.m_Scale[2]) / 4096.0f)));
                Vector3 invRot = new Vector3(((float)(short)bone.m_Rotation[0] * (float)Math.PI) / -2048.0f, ((float)(short)bone.m_Rotation[1] * (float)Math.PI) / -2048.0f, ((float)(short)bone.m_Rotation[2] * (float)Math.PI) / -2048.0f);
                Vector3 invTrans = new Vector3((float)(((int)bone.m_Translation[0]) / -4096.0f) * (1f), (float)(((int)bone.m_Translation[1]) / -4096.0f) * (1f), (float)(((int)bone.m_Translation[2]) / -4096.0f) * (1f));

                Matrix4 ret = Matrix4.Identity;

                Matrix4 mscale = Matrix4.CreateScale(invScale);
                Matrix4 mxrot = Matrix4.CreateRotationX(invRot.X);
                Matrix4 myrot = Matrix4.CreateRotationY(invRot.Y);
                Matrix4 mzrot = Matrix4.CreateRotationZ(invRot.Z);
                Matrix4 mtrans = Matrix4.CreateTranslation(invTrans);

                Matrix4.Mult(ref ret, ref mtrans, out ret);
                Matrix4.Mult(ref ret, ref mzrot, out ret);
                Matrix4.Mult(ref ret, ref myrot, out ret);
                Matrix4.Mult(ref ret, ref mxrot, out ret);
                Matrix4.Mult(ref ret, ref mscale, out ret);

                bone.m_LocalReverseTransform = ret;

                if (bone.m_ParentOffset == 0)
                {
                    bone.m_TotalReverseTransform = ret;
                }
            }

            for (int i = 0; i < bones.Count; i++)
            {
                BoneForExport bone = bones.Values.ElementAt(i);

                // If a child bone, apply its parent's transformations as well
                if (bone.m_ParentOffset < 0)
                {
                    Matrix4 ret = Matrix4.Identity;

                    BoneForExport parent_bone = bones.Values.Where(bone0 => bone0.m_Name.Equals(bone.m_ParentName)).ElementAt(0);

                    Matrix4.Mult(ref ret, ref parent_bone.m_TotalReverseTransform, out ret);
                    Matrix4.Mult(ref ret, ref bone.m_LocalReverseTransform, out bone.m_TotalReverseTransform);
                }
            }

            return bones.Values.ToList<BoneForExport>();
        }

        private static void ApplyBoneTransformations(BMD model, float scale)
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

        public class BoneForExport
        {
            public Dictionary<String, MaterialForExport> m_Materials;
            public String m_Name;
            public String m_ParentName;
            public String m_RootName;
            public List<BoneForExport> m_Children = new List<BoneForExport>();
            public short m_ParentOffset;
            public short m_SiblingOffset;
            public bool m_HasChildren;
            public uint[] m_Scale;
            public ushort[] m_Rotation;
            public uint[] m_Translation;
            public Matrix4 m_LocalReverseTransform;
            public Matrix4 m_TotalReverseTransform;

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
            public uint m_BoneID;

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

            public FaceVertex(Vector3 position, Vector2 textureCoord, Color vertexColour, uint boneID)
            {
                m_Position = position;
                m_TextureCoord = textureCoord;
                m_VertexColour = vertexColour;
                m_BoneID = boneID;
            }

            public override bool Equals(object obj)
            {
                var fv = obj as FaceVertex;
                if (fv == null)
                    return false;

                if (!(fv.m_Position.X == m_Position.X && fv.m_Position.Y == m_Position.Y && fv.m_Position.Z == this.m_Position.Z))
                    return false;

                if (!(fv.m_TextureCoord.X == m_TextureCoord.X && fv.m_TextureCoord.Y == m_TextureCoord.Y))
                    return false;

                if (!(fv.m_VertexColour.R == m_VertexColour.R && fv.m_VertexColour.G == m_VertexColour.G &&
                    fv.m_VertexColour.B == m_VertexColour.B))
                    return false;

                if (!(fv.m_BoneID == m_BoneID))
                    return false;

                return true;
            }
        }
    }
}
