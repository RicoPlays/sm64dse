﻿/* DAEWriter
 * 
 * Given a ModelBase object, produces a COLLADA 1.4.1 DAE model.
 * 
 * Supports joints, skinning and animations and plain static meshes. This is the recommended format for model 
 * exporting as it represents the original BMD model almost exactly, with the only difference being in texture 
 * options limitations.
 */ 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using OpenTK;
using System.Drawing;

namespace SM64DSe.ImportExport.Writers.ExternalWriters
{
    public class DAEWriter : AbstractModelWriter
    {
        bool m_UseSRTMatricesForTransforms = false;

        public DAEWriter(ModelBase model, string modelFileName, bool useSRTMatricesForTransforms = false) :
            base(model, modelFileName) 
        {
            m_UseSRTMatricesForTransforms = useSRTMatricesForTransforms;
        }

        public override void WriteModel(bool save = true)
        {
            string dir = m_ModelPath;

            List<string> exportedTextures = new List<string>();

            // Export textures to PNG
            foreach (ModelBase.MaterialDef material in m_Model.m_Materials.Values)
            {
                if (material.m_DiffuseMapName != null && !material.m_DiffuseMapName.Equals(""))
                {
                    string textureName = material.m_DiffuseMapName;
                    if (exportedTextures.Contains(textureName)) 
                        continue;
                    exportedTextures.Add(textureName);

                    if (!material.m_DiffuseMapInMemory)
                        ExportTextureToPNG(dir, textureName, m_ModelPath + Path.DirectorySeparatorChar + textureName);
                    else
                        ExportTextureToPNG(dir, textureName, m_Model.m_ConvertedTexturesBitmap[textureName]);
                }
            }

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "\t";
            settings.NewLineChars = "\r\n";
            settings.NewLineHandling = NewLineHandling.Replace;
            using (XmlWriter writer = XmlWriter.Create(m_ModelFileName, settings))
            {
                writer.WriteStartDocument();
                writer.WriteComment(Program.AppTitle + " " + Program.AppVersion + " " + Program.AppDate);
                writer.WriteStartElement("COLLADA", "http://www.collada.org/2005/11/COLLADASchema");
                writer.WriteAttributeString("version", "1.4.1");

                WriteDAE_Asset(writer);

                // Animation
                if (m_Model.m_Animations != null && m_Model.m_Animations.Count > 0)
                {
                    WriteDAE_LibraryAnimations(writer, m_Model, m_UseSRTMatricesForTransforms);
                }

                WriteDAE_LibraryImages(writer, exportedTextures, dir);

                WriteDAE_LibraryEffects(writer, m_Model.m_Materials);

                WriteDAE_LibraryMaterials(writer, m_Model.m_Materials);

                WriteDAE_LibraryGeometries(writer, m_Model);

                // DAE supports proper joints and skinning

                WriteDAE_LibraryControllers(writer, m_Model);

                WriteDAE_LibraryVisualScenes(writer, m_Model, m_UseSRTMatricesForTransforms);

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

        private static void WriteDAE_LibraryImages(XmlWriter writer, List<string> exportedTextures, string dir)
        {
            writer.WriteStartElement("library_images");

            foreach (string texName in exportedTextures)
            {
                if (texName == null)
                    continue;

                writer.WriteStartElement("image");

                writer.WriteAttributeString("id", texName + "-img");
                writer.WriteAttributeString("name", texName + "-img");

                writer.WriteElementString("init_from", texName + ".png");

                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        private static void WriteDAE_LibraryEffects(XmlWriter writer, Dictionary<string, ModelBase.MaterialDef> materials)
        {
            writer.WriteStartElement("library_effects");

            foreach (ModelBase.MaterialDef mat in materials.Values)
            {
                writer.WriteStartElement("effect");
                writer.WriteAttributeString("id", mat.m_ID + "-effect");
                writer.WriteStartElement("profile_COMMON");

                if (mat.m_HasTextures)
                {
                    writer.WriteStartElement("newparam");
                    writer.WriteAttributeString("sid", mat.m_DiffuseMapName + "-img-surface");

                    writer.WriteStartElement("surface");
                    writer.WriteAttributeString("type", "2D");
                    writer.WriteElementString("init_from", mat.m_DiffuseMapName + "-img");
                    writer.WriteEndElement();// surface

                    writer.WriteEndElement();// newparam

                    writer.WriteStartElement("newparam");
                    writer.WriteAttributeString("sid", mat.m_DiffuseMapName + "-img-sampler");

                    writer.WriteStartElement("sampler2D");
                    writer.WriteElementString("source", mat.m_DiffuseMapName + "-img-surface");
                    writer.WriteEndElement();// sampler2D

                    writer.WriteEndElement();// newparam
                }

                writer.WriteStartElement("technique");
                writer.WriteAttributeString("sid", "common");
                writer.WriteStartElement("phong");

                writer.WriteStartElement("emission");
                writer.WriteStartElement("color");
                writer.WriteAttributeString("sid", "emission");
                writer.WriteString((mat.m_EmissionColour.R / 255.0f).ToString(usa) + " " + (mat.m_EmissionColour.G / 255.0f).ToString(usa) + " " +
                    (mat.m_EmissionColour.B / 255.0f).ToString(usa) + " " + "1.0");
                writer.WriteEndElement();// color
                writer.WriteEndElement();// emission

                writer.WriteStartElement("ambient");
                writer.WriteStartElement("color");
                writer.WriteAttributeString("sid", "ambient");
                writer.WriteString((mat.m_AmbientColour.R / 255.0f).ToString(usa) + " " + (mat.m_AmbientColour.G / 255.0f).ToString(usa) + " " +
                    (mat.m_AmbientColour.B / 255.0f).ToString(usa) + " " + "1.0");
                writer.WriteEndElement();// color
                writer.WriteEndElement();// ambient

                writer.WriteStartElement("diffuse");
                if (mat.m_HasTextures)
                {
                    writer.WriteStartElement("texture");
                    writer.WriteAttributeString("texture", mat.m_DiffuseMapName + "-img-sampler");
                    writer.WriteAttributeString("texcoord", "UVMap");

                    writer.WriteEndElement();// texture
                }
                else
                {
                    writer.WriteStartElement("color");
                    writer.WriteAttributeString("sid", "diffuse");
                    writer.WriteString((mat.m_DiffuseColour.R / 255.0f).ToString(usa) + " " + (mat.m_DiffuseColour.G / 255.0f).ToString(usa) + " " +
                        (mat.m_DiffuseColour.B / 255.0f).ToString(usa) + " " + "1.0");
                    writer.WriteEndElement();// color
                }
                writer.WriteEndElement();// diffuse

                writer.WriteStartElement("specular");
                writer.WriteStartElement("color");
                writer.WriteAttributeString("sid", "specular");
                writer.WriteString((mat.m_SpecularColour.R / 255.0f).ToString(usa) + " " + (mat.m_SpecularColour.G / 255.0f).ToString(usa) + " " +
                    (mat.m_SpecularColour.B / 255.0f).ToString(usa) + " " + "1.0");
                writer.WriteEndElement();// color
                writer.WriteEndElement();// specular

                writer.WriteStartElement("transparency");
                writer.WriteStartElement("float");
                writer.WriteAttributeString("sid", "transparency");
                writer.WriteString((mat.m_Opacity / 255.0f).ToString(usa));
                writer.WriteEndElement();// float
                writer.WriteEndElement();// transparency

                writer.WriteEndElement();// phong
                writer.WriteEndElement();// technique

                if (mat.m_IsDoubleSided)
                {
                    writer.WriteStartElement("extra");
                    writer.WriteStartElement("technique");
                    writer.WriteAttributeString("profile", "GOOGLEEARTH");
                    writer.WriteElementString("double_sided", "1");
                    writer.WriteEndElement();// technique
                    writer.WriteEndElement();// extra
                }

                writer.WriteEndElement();// profile_COMMON
                if (mat.m_IsDoubleSided)
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

        private static void WriteDAE_LibraryMaterials(XmlWriter writer, Dictionary<string, ModelBase.MaterialDef> materials)
        {
            writer.WriteStartElement("library_materials");

            foreach (ModelBase.MaterialDef mat in materials.Values)
            {
                writer.WriteStartElement("material");
                writer.WriteAttributeString("id", mat.m_ID + "-material");
                writer.WriteAttributeString("name", mat.m_ID);
                writer.WriteStartElement("instance_effect");
                writer.WriteAttributeString("url", "#" + mat.m_ID + "-effect");
                writer.WriteEndElement();// instance_effect
                writer.WriteEndElement();// material
            }

            writer.WriteEndElement();
        }

        private static void WriteDAE_LibraryGeometries(XmlWriter writer, ModelBase model)
        {
            writer.WriteStartElement("library_geometries");

            IEnumerable<ModelBase.BoneDef> rootBones = model.m_BoneTree.GetRootBones();

            foreach (ModelBase.BoneDef root in rootBones)
            {
                List<ModelBase.BoneDef> bonesInBranch = root.GetBranch().Values.ToList();

                List<ModelBase.VertexDef> verticesInBranch = new List<ModelBase.VertexDef>();
                List<Vector3> positionsInBranch = new List<Vector3>();
                List<Vector3?> normalsInBranch = new List<Vector3?>();
                List<Vector2?> texCoordsInBranch = new List<Vector2?>();
                List<Color> vColoursInBranch = new List<Color>();
                foreach (ModelBase.BoneDef bone in bonesInBranch)
                {
                    foreach (ModelBase.GeometryDef geometry in bone.m_Geometries.Values)
                    {
                        foreach (ModelBase.PolyListDef polyList in geometry.m_PolyLists.Values)
                        {
                            foreach (ModelBase.FaceDef face in polyList.m_Faces)
                            {
                                foreach (ModelBase.VertexDef vert in face.m_Vertices)
                                {
                                    if (!verticesInBranch.Contains(vert))
                                        verticesInBranch.Add(vert);
                                }
                            }
                        }
                    }
                }

                foreach (ModelBase.VertexDef vert in verticesInBranch)
                {
                    positionsInBranch.Add(vert.m_Position);
                    texCoordsInBranch.Add(vert.m_TextureCoordinate);
                    if (vert.m_Normal != null)
                        normalsInBranch.Add(vert.m_Normal);
                    vColoursInBranch.Add(vert.m_VertexColour);
                }

                writer.WriteStartElement("geometry");
                writer.WriteAttributeString("id", root.m_ID + "-mesh");
                writer.WriteAttributeString("name", root.m_ID + "-mesh");
                writer.WriteStartElement("mesh");

                WriteDAE_Source_positions(writer, root, positionsInBranch);
                if (normalsInBranch.Count > 0)
                    WriteDAE_Source_normals(writer, root, normalsInBranch);
                if (texCoordsInBranch.Count > 0)
                    WriteDAE_Source_map(writer, root, texCoordsInBranch);
                if (vColoursInBranch.Count > 0)
                    WriteDAE_Source_colors(writer, root, vColoursInBranch);

                writer.WriteStartElement("vertices");
                writer.WriteAttributeString("id", root.m_ID + "-mesh-vertices");
                writer.WriteStartElement("input");
                writer.WriteAttributeString("semantic", "POSITION");
                writer.WriteAttributeString("source", "#" + root.m_ID + "-mesh-positions");
                writer.WriteEndElement();// input
                writer.WriteEndElement();// vertices

                foreach (string matName in root.m_MaterialsInBranch)
                {
                    List<ModelBase.FaceDef> faces = new List<ModelBase.FaceDef>();

                    foreach (ModelBase.BoneDef bone in bonesInBranch)
                    {
                        foreach (ModelBase.GeometryDef geometry in bone.m_Geometries.Values)
                        {
                            IEnumerable<KeyValuePair<string, ModelBase.PolyListDef>> polyListForMat =
                                geometry.m_PolyLists.Where(pl => pl.Value.m_MaterialName.Equals(matName));
                            foreach (KeyValuePair<string, ModelBase.PolyListDef> polyList in polyListForMat)
                                faces.AddRange(polyList.Value.m_Faces);
                        }
                    }

                    WriteDAE_Polylist(writer, root.m_ID, matName, faces,
                        positionsInBranch, 
                        ((normalsInBranch.Count > 0) ? normalsInBranch : null), 
                        ((model.m_Materials[matName].m_HasTextures) ? texCoordsInBranch : null), 
                        vColoursInBranch);
                }

                writer.WriteEndElement();// mesh
                writer.WriteEndElement();// geometry
            }

            writer.WriteEndElement();
        }

        private static void WriteDAE_Source_positions(XmlWriter writer, ModelBase.BoneDef parent, List<Vector3> positionsInBranch)
        {
            writer.WriteStartElement("source");
            writer.WriteAttributeString("id", parent.m_ID + "-mesh-positions");
            writer.WriteStartElement("float_array");
            writer.WriteAttributeString("id", parent.m_ID + "-mesh-positions-array");
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

            WriteDAE_TechniqueCommon_Accessor_float(writer, new string[] { "X", "Y", "Z" }, parent.m_ID + "-mesh-positions-array",
                positionsInBranch.Count, 3);

            writer.WriteEndElement();// source
        }

        private static void WriteDAE_Source_normals(XmlWriter writer, ModelBase.BoneDef parent, List<Vector3?> normalsInBranch)
        {
            writer.WriteStartElement("source");
            writer.WriteAttributeString("id", parent.m_ID + "-mesh-normals");
            writer.WriteStartElement("float_array");
            writer.WriteAttributeString("id", parent.m_ID + "-mesh-normals-array");
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

            WriteDAE_TechniqueCommon_Accessor_float(writer, new string[] { "X", "Y", "Z" }, parent.m_ID + "-mesh-normals-array",
                normalsInBranch.Count, 3);

            writer.WriteEndElement();// source
        }

        private static void WriteDAE_Source_map(XmlWriter writer, ModelBase.BoneDef parent, List<Vector2?> texCoordsInBranch)
        {
            writer.WriteStartElement("source");
            writer.WriteAttributeString("id", parent.m_ID + "-mesh-map-0");
            writer.WriteStartElement("float_array");
            writer.WriteAttributeString("id", parent.m_ID + "-mesh-map-0-array");
            writer.WriteAttributeString("count", (texCoordsInBranch.Count * 2).ToString());
            StringBuilder sb = new StringBuilder();
            foreach (Vector2 texCoord in texCoordsInBranch)
            {
                sb.Append(texCoord.X.ToString(usa) + " " + texCoord.Y.ToString(usa) + " ");
            }
            sb.Remove(sb.Length - 1, 1);// Remove extra space character at end
            writer.WriteString(sb.ToString());
            writer.WriteEndElement();// float_array

            WriteDAE_TechniqueCommon_Accessor_float(writer, new string[] { "S", "T" }, parent.m_ID + "-mesh-map-0-array",
                texCoordsInBranch.Count, 2);

            writer.WriteEndElement();// source
        }

        private static void WriteDAE_Source_colors(XmlWriter writer, ModelBase.BoneDef parent, List<Color> vColoursInBranch)
        {
            writer.WriteStartElement("source");
            writer.WriteAttributeString("id", parent.m_ID + "-mesh-colors");
            writer.WriteStartElement("float_array");
            writer.WriteAttributeString("id", parent.m_ID + "-mesh-colors-array");
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

            WriteDAE_TechniqueCommon_Accessor_float(writer, new string[] { "R", "G", "B" }, parent.m_ID + "-mesh-colors-array",
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

        private static void WriteDAE_Polylist(XmlWriter writer, string boneName, string matName, List<ModelBase.FaceDef> faces,
            List<Vector3> positionsInBranch, List<Vector3?> normalsInBranch,
            List<Vector2?> texCoordsInBranch, List<Color> vColoursInBranch)
        {
            writer.WriteStartElement("polylist");
            writer.WriteAttributeString("material", matName + "-material");
            writer.WriteAttributeString("count", "" + faces.Count);

            int offset = 0;

            writer.WriteStartElement("input");
            writer.WriteAttributeString("semantic", "VERTEX");
            writer.WriteAttributeString("source", "#" + boneName + "-mesh-vertices");
            writer.WriteAttributeString("offset", "" + offset++);
            writer.WriteEndElement();// input

            if (normalsInBranch != null)
            {
                writer.WriteStartElement("input");
                writer.WriteAttributeString("semantic", "NORMAL");
                writer.WriteAttributeString("source", "#" + boneName + "-mesh-normals");
                writer.WriteAttributeString("offset", "" + offset++);
                writer.WriteEndElement();// input
            }

            if (texCoordsInBranch != null)
            {
                writer.WriteStartElement("input");
                writer.WriteAttributeString("semantic", "TEXCOORD");
                writer.WriteAttributeString("source", "#" + boneName + "-mesh-map-0");
                writer.WriteAttributeString("offset", "" + offset++);
                writer.WriteAttributeString("set", "0");
                writer.WriteEndElement();// input
            }

            writer.WriteStartElement("input");
            writer.WriteAttributeString("semantic", "COLOR");
            writer.WriteAttributeString("source", "#" + boneName + "-mesh-colors");
            writer.WriteAttributeString("offset", "" + offset++);
            writer.WriteEndElement();// input

            writer.WriteStartElement("vcount");
            StringBuilder sb = new StringBuilder();
            foreach (ModelBase.FaceDef face in faces)
            {
                sb.Append(face.m_NumVertices + " ");
            }
            sb.Remove(sb.Length - 1, 1);// Remove extra space character at end
            writer.WriteString(sb.ToString());
            writer.WriteEndElement();// vcount

            writer.WriteStartElement("p");
            sb = new StringBuilder();
            foreach (ModelBase.FaceDef face in faces)
            {
                foreach (ModelBase.VertexDef vert in face.m_Vertices)
                {
                    sb.Append(positionsInBranch.IndexOf(vert.m_Position) + " " + 
                        ((normalsInBranch != null) ? normalsInBranch.IndexOf((Vector3)vert.m_Normal) + " " : "") +
                        ((texCoordsInBranch != null) ? texCoordsInBranch.IndexOf((Vector2)vert.m_TextureCoordinate) + " " : "") + 
                        vColoursInBranch.IndexOf((Color)vert.m_VertexColour) + " ");
                }
            }
            sb.Remove(sb.Length - 1, 1);// Remove extra space character at end
            writer.WriteString(sb.ToString());
            writer.WriteEndElement();// p

            writer.WriteEndElement();// polylist
        }

        private static void WriteDAE_LibraryControllers(XmlWriter writer, ModelBase model)
        {
            writer.WriteStartElement("library_controllers");

            IEnumerable<ModelBase.BoneDef> rootBones = model.m_BoneTree.GetRootBones();

            foreach (ModelBase.BoneDef root in rootBones)
            {
                IEnumerable<ModelBase.BoneDef> bonesInBranch = root.GetBranch().Values;

                List<ModelBase.VertexDef> verticesInBranch = new List<ModelBase.VertexDef>();
                foreach (ModelBase.BoneDef bone in bonesInBranch)
                {
                    foreach (ModelBase.GeometryDef geometry in bone.m_Geometries.Values)
                    {
                        foreach (ModelBase.PolyListDef polyList in geometry.m_PolyLists.Values)
                        {
                            foreach (ModelBase.FaceDef face in polyList.m_Faces)
                            {
                                foreach (ModelBase.VertexDef vert in face.m_Vertices)
                                {
                                    if (!verticesInBranch.Contains(vert))
                                        verticesInBranch.Add(vert);
                                }
                            }
                        }
                    }
                }

                writer.WriteStartElement("controller");
                writer.WriteAttributeString("id", root.m_ID + "-skin");
                writer.WriteAttributeString("name", "skinCluster_" + root.m_ID);

                writer.WriteStartElement("skin");
                writer.WriteAttributeString("source", "#" + root.m_ID + "-mesh");
                writer.WriteElementString("bind_shape_matrix", "1 0 0 0 0 1 0 0 0 0 1 0 0 0 0 1");

                // Source Joints
                writer.WriteStartElement("source");
                writer.WriteAttributeString("id", root.m_ID + "-skin-joints");
                StringBuilder boneNamesArray = new StringBuilder();
                foreach (ModelBase.BoneDef bone in bonesInBranch)
                    boneNamesArray.Append(bone.m_ID + " ");
                boneNamesArray.Remove(boneNamesArray.Length - 1, 1);// Remove extra space character at end
                writer.WriteStartElement("Name_array");
                writer.WriteAttributeString("id", root.m_ID + "-skin-joints-array");
                writer.WriteAttributeString("count", "" + bonesInBranch.Count());
                writer.WriteString(boneNamesArray.ToString());
                writer.WriteEndElement();// Name_array
                writer.WriteStartElement("technique_common");
                writer.WriteStartElement("accessor");
                writer.WriteAttributeString("source", "#" + root.m_ID + "-skin-joints-array");
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
                writer.WriteAttributeString("id", root.m_ID + "-skin-bind_poses");

                List<Matrix4> invMatrices = new List<Matrix4>();
                foreach (ModelBase.BoneDef bone in bonesInBranch)
                {
                    invMatrices.Add(bone.m_GlobalInverseTransformation);
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
                writer.WriteAttributeString("id", root.m_ID + "-skin-bind_poses-array");
                writer.WriteAttributeString("count", "" + (bonesInBranch.Count() * 16));
                writer.WriteString(sb.ToString());
                writer.WriteEndElement();// float_array
                writer.WriteStartElement("technique_common");
                writer.WriteStartElement("accessor");
                writer.WriteAttributeString("source", "#" + root.m_ID + "-skin-bind_poses-array");
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
                writer.WriteAttributeString("id", root.m_ID + "-skin-weights");
                writer.WriteComment("The BMD format does not have any concept of 'weights', instead each vertex is assigned to one bone");
                writer.WriteStartElement("float_array");
                writer.WriteAttributeString("id", root.m_ID + "-skin-weights-array");
                writer.WriteAttributeString("count", "1");
                writer.WriteString("1");
                writer.WriteEndElement();// float_array
                writer.WriteStartElement("technique_common");
                writer.WriteStartElement("accessor");
                writer.WriteAttributeString("source", "#" + root.m_ID + "-skin-weights-array");
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
                writer.WriteAttributeString("source", "#" + root.m_ID + "-skin-joints");
                writer.WriteEndElement();// input
                writer.WriteStartElement("input");
                writer.WriteAttributeString("semantic", "INV_BIND_MATRIX");
                writer.WriteAttributeString("source", "#" + root.m_ID + "-skin-bind_poses");
                writer.WriteEndElement();// input
                writer.WriteEndElement();// joints

                writer.WriteComment("Each vertex will be assigned to only one bone with a weight of 1");
                writer.WriteStartElement("vertex_weights");
                writer.WriteAttributeString("count", "" + verticesInBranch.Count);
                writer.WriteStartElement("input");
                writer.WriteAttributeString("semantic", "JOINT");
                writer.WriteAttributeString("source", "#" + root.m_ID + "-skin-joints");
                writer.WriteAttributeString("offset", "0");
                writer.WriteEndElement();// input
                writer.WriteStartElement("input");
                writer.WriteAttributeString("semantic", "WEIGHT");
                writer.WriteAttributeString("source", "#" + root.m_ID + "-skin-weights");
                writer.WriteAttributeString("offset", "1");
                writer.WriteEndElement();// input
                writer.WriteStartElement("vcount");
                StringBuilder vcount = new StringBuilder();
                foreach (ModelBase.VertexDef vert in verticesInBranch)
                    vcount.Append("1 ");
                vcount.Remove(vcount.Length - 1, 1);// Remove extra space character at end
                writer.WriteString(vcount.ToString());
                writer.WriteEndElement();// vcount
                writer.WriteComment("This list contains two values for each vertex, the first is the bone ID and the second " +
                    "is the index of the vertex weight '1' (0)");
                writer.WriteStartElement("v");
                StringBuilder v = new StringBuilder();
                foreach (ModelBase.VertexDef vert in verticesInBranch)
                    v.Append(vert.m_VertexBoneID + " 0 ");
                v.Remove(v.Length - 1, 1);// Remove extra space character at end
                writer.WriteString(v.ToString());
                writer.WriteEndElement();// v
                writer.WriteEndElement();// vertex_weights

                writer.WriteEndElement();// skin

                writer.WriteEndElement();// controller
            }

            writer.WriteEndElement();// library_controllers
        }

        private static void WriteDAE_LibraryVisualScenes(XmlWriter writer, ModelBase model, bool useSRTMatricesForTransforms)
        {
            writer.WriteStartElement("library_visual_scenes");
            writer.WriteStartElement("visual_scene");
            writer.WriteAttributeString("id", "Scene");
            writer.WriteAttributeString("name", "Scene");

            IEnumerable<ModelBase.BoneDef> rootBones = model.m_BoneTree.GetRootBones();

            foreach (ModelBase.BoneDef root in rootBones)
            {
                IEnumerable<ModelBase.BoneDef> bonesInBranch = root.GetBranch().Values;

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
                writer.WriteAttributeString("id", root.m_ID + "-node");
                writer.WriteAttributeString("name", root.m_ID + "-node");
                writer.WriteAttributeString("type", "NODE");

                writer.WriteStartElement("instance_controller");
                writer.WriteAttributeString("url", "#" + root.m_ID + "-skin");
                writer.WriteElementString("skeleton", "#" + root.m_ID);
                writer.WriteStartElement("bind_material");
                writer.WriteStartElement("technique_common");

                foreach (string matName in root.m_MaterialsInBranch)
                {
                    writer.WriteStartElement("instance_material");
                    writer.WriteAttributeString("symbol", matName + "-material");
                    writer.WriteAttributeString("target", "#" + matName + "-material");

                    writer.WriteStartElement("bind_vertex_input");
                    //writer.WriteAttributeString("semantic", parent.m_ID + "-mesh-map-0");
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

        private static void WriteDAE_Node_joint(XmlWriter writer, ModelBase.BoneDef parent, bool useSRTMatricesForTransforms)
        {
            writer.WriteStartElement("node");

            writer.WriteAttributeString("id", parent.m_ID);

            writer.WriteAttributeString("name", parent.m_ID);
            writer.WriteAttributeString("sid", parent.m_ID);
            writer.WriteAttributeString("type", "JOINT");

            if (useSRTMatricesForTransforms)
                WriteDAE_Node_transformationMatrix(writer, parent);
            else
                WriteDAE_Node_transformationSRT(writer, parent);

            foreach (ModelBase.BoneDef bone in parent.GetChildren().Values)
            {
                WriteDAE_Node_joint(writer, bone, useSRTMatricesForTransforms);
            }

            writer.WriteEndElement();// node
        }

        private static void WriteDAE_Node_transformationMatrix(XmlWriter writer, ModelBase.BoneDef node)
        {
            writer.WriteStartElement("matrix");
            writer.WriteAttributeString("sid", "transform");
            StringBuilder sb = new StringBuilder();
            Matrix4 matrix = node.m_GlobalTransformation;
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

        private static void WriteDAE_Node_transformationSRT(XmlWriter writer, ModelBase.BoneDef node)
        {
            Vector3 scale = node.m_Scale;
            Vector3 rotRad = node.m_Rotation;
            // COLLADA uses degrees for angles
            Vector3 rotDeg = new Vector3(rotRad.X * Helper.Rad2Deg, rotRad.Y * Helper.Rad2Deg, rotRad.Z * Helper.Rad2Deg);
            Vector3 trans = node.m_Translation;

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

        private static void WriteDAE_LibraryAnimations(XmlWriter writer, ModelBase model, bool useSRTMatricesForTransforms)
        {
            writer.WriteStartElement("library_animations");

            // For each joint, write and <animation> each for translate, rotationZ Y and X and scale
            int ind = 0;
            foreach (ModelBase.AnimationDef animation in model.m_Animations.Values)
            {
                ModelBase.BoneDef bone = model.m_BoneTree.GetBoneByID(animation.m_BoneID);

                if (useSRTMatricesForTransforms)
                {
                    writer.WriteStartElement("animation");
                    writer.WriteAttributeString("id", bone.m_ID + "_transform");
                    WriteDAE_Animation_Source_time(writer, animation, bone, "transform");
                    WriteDAE_Animation_Source_matrixOutput(writer, animation, bone);
                    WriteDAE_Animation_Source_interpolations(writer, animation, bone, "transform");
                    WriteDAE_Animation_Sampler(writer, bone, "transform");
                    WriteDAE_Animation_Channel(writer, bone, "transform", "transform");
                    writer.WriteEndElement();// animation
                }
                else
                {
                    writer.WriteStartElement("animation");
                    writer.WriteAttributeString("id", bone.m_ID + "_translate");
                    WriteDAE_Animation_Source_time(writer, animation, bone, "translate");
                    WriteDAE_Animation_Source_translationOutput(writer, animation, bone);
                    WriteDAE_Animation_Source_interpolations(writer, animation, bone, "translate");
                    WriteDAE_Animation_Sampler(writer, bone, "translate");
                    WriteDAE_Animation_Channel(writer, bone, "translate", "translate");
                    writer.WriteEndElement();// animation

                    writer.WriteStartElement("animation");
                    writer.WriteAttributeString("id", bone.m_ID + "_rotationZ");
                    WriteDAE_Animation_Source_time(writer, animation, bone, "rotationZ");
                    WriteDAE_Animation_Source_rotationOutput(writer, animation, bone, "Z");
                    WriteDAE_Animation_Source_interpolations(writer, animation, bone, "rotationZ");
                    WriteDAE_Animation_Sampler(writer, bone, "rotationZ");
                    WriteDAE_Animation_Channel(writer, bone, "rotationZ", "rotationZ.ANGLE");
                    writer.WriteEndElement();// animation

                    writer.WriteStartElement("animation");
                    writer.WriteAttributeString("id", bone.m_ID + "_rotationY");
                    WriteDAE_Animation_Source_time(writer, animation, bone, "rotationY");
                    WriteDAE_Animation_Source_rotationOutput(writer, animation, bone, "Y");
                    WriteDAE_Animation_Source_interpolations(writer, animation, bone, "rotationY");
                    WriteDAE_Animation_Sampler(writer, bone, "rotationY");
                    WriteDAE_Animation_Channel(writer, bone, "rotationY", "rotationY.ANGLE");
                    writer.WriteEndElement();// animation

                    writer.WriteStartElement("animation");
                    writer.WriteAttributeString("id", bone.m_ID + "_rotationX");
                    WriteDAE_Animation_Source_time(writer, animation, bone, "rotationX");
                    WriteDAE_Animation_Source_rotationOutput(writer, animation, bone, "X");
                    WriteDAE_Animation_Source_interpolations(writer, animation, bone, "rotationX");
                    WriteDAE_Animation_Sampler(writer, bone, "rotationX");
                    WriteDAE_Animation_Channel(writer, bone, "rotationX", "rotationX.ANGLE");
                    writer.WriteEndElement();// animation

                    writer.WriteStartElement("animation");
                    writer.WriteAttributeString("id", bone.m_ID + "_scale");
                    WriteDAE_Animation_Source_time(writer, animation, bone, "scale");
                    WriteDAE_Animation_Source_scaleOutput(writer, animation, bone);
                    WriteDAE_Animation_Source_interpolations(writer, animation, bone, "scale");
                    WriteDAE_Animation_Sampler(writer, bone, "scale");
                    WriteDAE_Animation_Channel(writer, bone, "scale", "scale");
                    writer.WriteEndElement();// animation
                }

                ind++;
            }

            writer.WriteEndElement();// library_animations
        }

        private static void WriteDAE_Animation_Source_matrixOutput(XmlWriter writer, ModelBase.AnimationDef animation, ModelBase.BoneDef bone)
        {
            writer.WriteStartElement("source");
            writer.WriteAttributeString("id", bone.m_ID + "_transform-output");
            writer.WriteStartElement("float_array");
            writer.WriteAttributeString("id", bone.m_ID + "_transform-output-array");
            writer.WriteAttributeString("count", "" + (animation.m_NumFrames * 16));
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < animation.m_NumFrames; i++)
            {
                Matrix4 matrix = animation.m_AnimationFrames[i].GetTransformation();
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
            writer.WriteAttributeString("source", "#" + bone.m_ID + "_transform-output-array");
            writer.WriteAttributeString("stride", "16");
            WriteDAE_Animation_Param_nameType(writer, "TRANSFORM", "float4x4");
            writer.WriteEndElement();// accessor
            writer.WriteEndElement();// technique_common
            writer.WriteEndElement();// source
        }
        /* 
         * EXPORING ANIMATIONS AS SEPARATE SCALE, ROTATION AND TRANSLATION NOT CURRENTLY WORKING
         */
        private static void WriteDAE_Animation_Source_translationOutput(XmlWriter writer, ModelBase.AnimationDef animation, 
            ModelBase.BoneDef bone)
        {
            writer.WriteStartElement("source");
            writer.WriteAttributeString("id", bone.m_ID + "_translate-output");
            writer.WriteStartElement("float_array");
            writer.WriteAttributeString("id", bone.m_ID + "_translate-output-array");
            writer.WriteAttributeString("count", "" + (animation.m_NumFrames * 3));
            StringBuilder trans = new StringBuilder();
            for (int i = 0; i < animation.m_NumFrames; i++)
            {
                float x = animation.m_AnimationFrames[i].GetTranslation().X;
                float y = animation.m_AnimationFrames[i].GetTranslation().Y;
                float z = animation.m_AnimationFrames[i].GetTranslation().Z;

                trans.Append(x.ToString(usa) + " " + y.ToString(usa) + " " + z.ToString(usa) + " ");
            }
            trans.Remove(trans.Length - 1, 1);// Remove extra space character at end
            writer.WriteString(trans.ToString());
            writer.WriteEndElement();// float_array
            writer.WriteStartElement("technique_common");
            writer.WriteStartElement("accessor");
            writer.WriteAttributeString("count", "" + animation.m_NumFrames);
            writer.WriteAttributeString("offset", "0");
            writer.WriteAttributeString("source", "#" + bone.m_ID + "_translate-output-array");
            writer.WriteAttributeString("stride", "3");
            WriteDAE_Animation_Param_nameType(writer, "X", "float");
            WriteDAE_Animation_Param_nameType(writer, "Y", "float");
            WriteDAE_Animation_Param_nameType(writer, "Z", "float");
            writer.WriteEndElement();// accessor
            writer.WriteEndElement();// technique_common
            writer.WriteEndElement();// source
        }

        private static void WriteDAE_Animation_Source_rotationOutput(XmlWriter writer, ModelBase.AnimationDef animation,
            ModelBase.BoneDef bone, string component)
        {
            writer.WriteStartElement("source");
            writer.WriteAttributeString("id", bone.m_ID + "_rotation" + component + "-output");
            writer.WriteStartElement("float_array");
            writer.WriteAttributeString("id", bone.m_ID + "_rotation" + component + "-output-array");
            writer.WriteAttributeString("count", "" + animation.m_NumFrames);
            StringBuilder rot = new StringBuilder();
            for (int i = 0; i < animation.m_NumFrames; i++)
            {
                float angle = 0.0f;
                Vector3 rotDeg = animation.m_AnimationFrames[i].GetRotationInDegrees();

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
            writer.WriteAttributeString("source", "#" + bone.m_ID + "_rotation" + component + "-output-array");
            writer.WriteAttributeString("stride", "1");
            WriteDAE_Animation_Param_nameType(writer, "ANGLE", "float");
            writer.WriteEndElement();// accessor
            writer.WriteEndElement();// technique_common
            writer.WriteEndElement();// source
        }

        private static void WriteDAE_Animation_Source_scaleOutput(XmlWriter writer, ModelBase.AnimationDef animation,
            ModelBase.BoneDef bone)
        {
            writer.WriteStartElement("source");
            writer.WriteAttributeString("id", bone.m_ID + "_scale-output");
            writer.WriteStartElement("float_array");
            writer.WriteAttributeString("id", bone.m_ID + "_scale-output-array");
            writer.WriteAttributeString("count", "" + (animation.m_NumFrames * 3));
            StringBuilder scale = new StringBuilder();
            for (int i = 0; i < animation.m_NumFrames; i++)
            {
                float x = animation.m_AnimationFrames[i].GetScale().X;
                float y = animation.m_AnimationFrames[i].GetScale().Y;
                float z = animation.m_AnimationFrames[i].GetScale().Z;

                scale.Append(x.ToString(usa) + " " + y.ToString(usa) + " " + z.ToString(usa) + " ");
            }
            scale.Remove(scale.Length - 1, 1);// Remove extra space character at end
            writer.WriteString(scale.ToString());
            writer.WriteEndElement();// float_array
            writer.WriteStartElement("technique_common");
            writer.WriteStartElement("accessor");
            writer.WriteAttributeString("count", "" + animation.m_NumFrames);
            writer.WriteAttributeString("offset", "0");
            writer.WriteAttributeString("source", "#" + bone.m_ID + "_scale-output-array");
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

        private static void WriteDAE_Animation_Source_time(XmlWriter writer, ModelBase.AnimationDef animation, 
            ModelBase.BoneDef bone, string transformationName)
        {
            writer.WriteStartElement("source");
            writer.WriteAttributeString("id", bone.m_ID + "_" + transformationName + "-input");
            writer.WriteStartElement("float_array");
            writer.WriteAttributeString("id", bone.m_ID + "_" + transformationName + "-input-array");
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
            writer.WriteAttributeString("source", "#" + bone.m_ID + "_" + transformationName + "-input-array");
            writer.WriteAttributeString("stride", "1");
            WriteDAE_Animation_Param_nameType(writer, "TIME", "float");
            writer.WriteEndElement();// accessor
            writer.WriteEndElement();// technique_common
            writer.WriteEndElement();// source
        }

        private static void WriteDAE_Animation_Source_interpolations(XmlWriter writer, ModelBase.AnimationDef animation,
            ModelBase.BoneDef bone, string transformationName)
        {
            writer.WriteStartElement("source");
            writer.WriteAttributeString("id", bone.m_ID + "_" + transformationName + "-interpolations");
            writer.WriteStartElement("Name_array");
            writer.WriteAttributeString("id", bone.m_ID + "_" + transformationName + "-interpolations-array");
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
            writer.WriteAttributeString("source", "#" + bone.m_ID + "_" + transformationName + "-interpolations-array");
            writer.WriteAttributeString("stride", "1");
            WriteDAE_Animation_Param_nameType(writer, "INTERPOLATION", "Name");
            writer.WriteEndElement();// accessor
            writer.WriteEndElement();// technique_common
            writer.WriteEndElement();// source
        }

        private static void WriteDAE_Animation_Sampler(XmlWriter writer, ModelBase.BoneDef bone, string transformationName)
        {
            writer.WriteStartElement("sampler");
            writer.WriteAttributeString("id", bone.m_ID + "_" + transformationName + "-sampler");
            writer.WriteStartElement("input");
            writer.WriteAttributeString("semantic", "INPUT");
            writer.WriteAttributeString("source", "#" + bone.m_ID + "_" + transformationName + "-input");
            writer.WriteEndElement();// input
            writer.WriteStartElement("input");
            writer.WriteAttributeString("semantic", "OUTPUT");
            writer.WriteAttributeString("source", "#" + bone.m_ID + "_" + transformationName + "-output");
            writer.WriteEndElement();// input
            writer.WriteStartElement("input");
            writer.WriteAttributeString("semantic", "INTERPOLATION");
            writer.WriteAttributeString("source", "#" + bone.m_ID + "_" + transformationName + "-interpolations");
            writer.WriteEndElement();// input
            writer.WriteEndElement();// sampler
        }

        private static void WriteDAE_Animation_Channel(XmlWriter writer, ModelBase.BoneDef bone, string samplerSuffix, string transformationName)
        {
            writer.WriteStartElement("channel");
            writer.WriteAttributeString("source", "#" + bone.m_ID + "_" + samplerSuffix + "-sampler");
            writer.WriteAttributeString("target", bone.m_ID + "/" + transformationName);
            writer.WriteEndElement();// channel
        }
    }
}
