/* Imports an OBJ model to BMD.
 * 
 * Notes: 
 * Supports the Blender-specific "Extended OBJ" plugin's vertex colours. 
 * OBJ does not support joints so each object, "o" command is treated as a bone with a custom *.bones file 
 * used to read the hierarchy and properties.
 * 
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Security.Cryptography;

namespace SM64DSe.Importers
{
    public class BMD_Importer_OBJ : BMD_Importer_Base
    {

        private void OBJ_LoadMTL(String filename)
        {
            Stream fs;
            try
            {
                fs = File.OpenRead(filename);
            }
            catch
            {
                MessageBox.Show("Material library not found:\n\n" + filename + "\n\nA default white material will be used instead.");
                AddWhiteMat();
                return;
            }
            StreamReader sr = new StreamReader(fs);

            string curmaterial = "";

            string imagesNotFound = "";

            string curline;
            while ((curline = sr.ReadLine()) != null)
            {
                curline = curline.Trim();

                // skip empty lines and comments
                if (curline.Length < 1) continue;
                if (curline[0] == '#')
                {
                    if (curline == "#Materials exported from Google Sketchup")
                        m_SketchupHack = true;

                    continue;
                }

                string[] parts = curline.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 1) continue;

                switch (parts[0])
                {
                    case "newmtl": // new material definition
                        {
                            if (parts.Length < 2) continue;
                            curmaterial = parts[1];

                            MaterialDef mat = new MaterialDef();
                            mat.m_ID = m_Materials.Count;
                            mat.m_Name = curmaterial;
                            mat.m_Faces = new List<FaceDef>();
                            mat.m_DiffuseColor = Color.White;
                            mat.m_Opacity = 255; // oops
                            mat.m_HasTextures = false;
                            mat.m_DiffuseMapName = "";
                            mat.m_DiffuseMapID = 0;
                            mat.m_DiffuseMapSize = new Vector2(0f, 0f);
                            mat.m_ColType = 0;
                            try
                            {
                                m_Materials.Add(curmaterial, mat);
                            }
                            catch
                            {
                                //Duplicate material
                            }
                        }
                        break;

                    case "d":
                    case "Tr": // opacity
                        {
                            if (parts.Length < 2) continue;
                            float o = float.Parse(parts[1], USA);
                            if (m_SketchupHack)
                                o *= 255;

                            MaterialDef mat = (MaterialDef)m_Materials[curmaterial];
                            mat.m_Opacity = Math.Max(0, Math.Min(255, (int)(o * 255)));
                        }
                        break;

                    case "Kd": // diffuse color
                        {
                            if (parts.Length < 4) continue;
                            float r = float.Parse(parts[1], USA);
                            float g = float.Parse(parts[2], USA);
                            float b = float.Parse(parts[3], USA);
                            Color col = Color.FromArgb((int)(r * 255), (int)(g * 255), (int)(b * 255));

                            MaterialDef mat = (MaterialDef)m_Materials[curmaterial];
                            mat.m_DiffuseColor = col;
                        }
                        break;

                    case "map_Kd":
                    case "mapKd": // diffuse map (texture)
                        {
                            string texname = curline.Substring(parts[0].Length + 1).Trim();
                            Bitmap tex;
                            try
                            {
                                tex = new Bitmap(m_ModelPath + Path.DirectorySeparatorChar + texname);

                                int width = 8, height = 8;
                                while (width < tex.Width) width *= 2;
                                while (height < tex.Height) height *= 2;

                                // cheap resizing for textures whose dimensions aren't power-of-two
                                if ((width != tex.Width) || (height != tex.Height))
                                {
                                    Bitmap newbmp = new Bitmap(width, height);
                                    Graphics g = Graphics.FromImage(newbmp);
                                    g.DrawImage(tex, new Rectangle(0, 0, width, height));
                                    tex = newbmp;
                                }

                                MaterialDef mat = (MaterialDef)m_Materials[curmaterial];
                                mat.m_HasTextures = true;

                                byte[] map = new byte[tex.Width * tex.Height * 4];
                                for (int y = 0; y < tex.Height; y++)
                                {
                                    for (int x = 0; x < tex.Width; x++)
                                    {
                                        Color pixel = tex.GetPixel(x, y);
                                        int pos = ((y * tex.Width) + x) * 4;

                                        map[pos] = pixel.B;
                                        map[pos + 1] = pixel.G;
                                        map[pos + 2] = pixel.R;
                                        map[pos + 3] = pixel.A;
                                    }
                                }
                                //System.Drawing.Imaging.BitmapData lol = tex.LockBits(new Rectangle(0, 0, tex.Width, tex.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                                //System.Runtime.InteropServices.Marshal.Copy(lol.Scan0, map, 0, tex.Width * tex.Height * 4);

                                string imghash = HexString(m_MD5.ComputeHash(map));
                                if (m_Textures.ContainsKey(imghash))
                                {
                                    MaterialDef mat2 = m_Textures[imghash];
                                    mat.m_DiffuseMapName = mat2.m_DiffuseMapName;
                                    mat.m_DiffuseMapID = mat2.m_DiffuseMapID;
                                    mat.m_DiffuseMapSize = mat2.m_DiffuseMapSize;
                                    break;
                                }

                                mat.m_DiffuseMapName = texname;
                                m_Textures.Add(imghash, mat);

                                mat.m_DiffuseMapSize.X = tex.Width;
                                mat.m_DiffuseMapSize.Y = tex.Height;

                                mat.m_DiffuseMapID = GL.GenTexture();
                                GL.BindTexture(TextureTarget.Texture2D, mat.m_DiffuseMapID);
                                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Four, tex.Width, tex.Height,
                                    0, PixelFormat.Bgra, PixelType.UnsignedByte, map);

                                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

                                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
                            }
                            catch
                            {
                                imagesNotFound += m_ModelPath + Path.DirectorySeparatorChar + texname + "\n";
                            }
                            break;
                        }
                }
            }

            if (!imagesNotFound.Equals(""))
                MessageBox.Show("The following images were not found:\n\n" + imagesNotFound);

            sr.Close();
        }

        protected void LoadBoneDefinitionsForOBJ(String filename)
        {
            Stream fs;
            try
            {
                fs = File.OpenRead(filename);
            }
            catch
            {
                MessageBox.Show("Specified Bone definitions not found:\n\n" + filename + "\n\nUsing default values.");
                return;
            }
            StreamReader sr = new StreamReader(fs);

            string currentBone = "";
            string currentRootBone = "";// Parent (type 0) bone, as opposed to a child bone that has children bones

            m_Bones.Clear();

            string curline;
            while ((curline = sr.ReadLine()) != null)
            {
                curline = curline.Trim();

                // skip empty lines and comments
                if (curline.Length < 1) continue;
                if (curline[0] == '#')
                {
                    continue;
                }

                string[] parts = curline.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 1) continue;

                switch (parts[0])
                {
                    case "newbone": // new bone definition
                        {
                            if (parts.Length < 2) continue;
                            currentBone = parts[1];

                            BoneForImport bone = new BoneForImport();
                            bone.m_Name = currentBone;

                            try
                            {
                                m_Bones.Add(currentBone, bone);
                            }
                            catch
                            {
                                //Duplicate bone name
                                bone.m_Name += "_";
                                m_Bones.Add(currentBone, bone);
                            }
                        }
                        break;

                    case "parent_offset": // Offset in bones to parent bone (signed 16-bit. 0=no parent, -1=parent is the previous bone, ...)
                        {
                            if (parts.Length < 2) continue;
                            short parent_offset = short.Parse(parts[1]);

                            BoneForImport bone = (BoneForImport)m_Bones[currentBone];
                            bone.m_ParentOffset = parent_offset;

                            if (parent_offset == 0)
                            {
                                currentRootBone = currentBone;
                            }

                            bone.m_RootBone = currentRootBone;
                            bone.m_ParentBone = m_Bones.Values.ElementAt(GetBoneIndex(bone.m_Name) + bone.m_ParentOffset).m_Name;
                        }
                        break;

                    case "has_children": // 1 if the bone has children, 0 otherwise
                        {
                            if (parts.Length < 2) continue;
                            bool has_children = (short.Parse(parts[1]) == 1);

                            BoneForImport bone = (BoneForImport)m_Bones[currentBone];
                            bone.m_HasChildren = has_children;
                        }
                        break;

                    case "sibling_offset": // Offset in bones to the next sibling bone (0=bone is last child of its parent)
                        {
                            if (parts.Length < 2) continue;
                            short sibling_offset = short.Parse(parts[1]);

                            BoneForImport bone = (BoneForImport)m_Bones[currentBone];
                            bone.m_SiblingOffset = sibling_offset;
                        }
                        break;

                    case "scale": // Scale transformation
                        {
                            if (parts.Length < 4) continue;
                            uint[] scale = new uint[] { uint.Parse(parts[1], System.Globalization.NumberStyles.HexNumber), 
                                uint.Parse(parts[2], System.Globalization.NumberStyles.HexNumber), 
                                uint.Parse(parts[3], System.Globalization.NumberStyles.HexNumber) };

                            BoneForImport bone = (BoneForImport)m_Bones[currentBone];
                            bone.m_Scale = scale;
                        }
                        break;

                    case "rotation": // Rotation transformation
                        {
                            if (parts.Length < 4) continue;
                            ushort[] rotation = new ushort[] { ushort.Parse(parts[1], System.Globalization.NumberStyles.HexNumber), 
                                ushort.Parse(parts[2], System.Globalization.NumberStyles.HexNumber), 
                                ushort.Parse(parts[3], System.Globalization.NumberStyles.HexNumber) };

                            BoneForImport bone = (BoneForImport)m_Bones[currentBone];
                            bone.m_Rotation = rotation;
                        }
                        break;

                    case "translation": // Scale transformation
                        {
                            if (parts.Length < 4) continue;
                            uint[] translation = new uint[] { uint.Parse(parts[1], System.Globalization.NumberStyles.HexNumber), 
                                uint.Parse(parts[2], System.Globalization.NumberStyles.HexNumber), 
                                uint.Parse(parts[3], System.Globalization.NumberStyles.HexNumber) };

                            BoneForImport bone = (BoneForImport)m_Bones[currentBone];
                            bone.m_Translation = translation;
                        }
                        break;
                    case "billboard": // Always rendered facing camera
                        {
                            if (parts.Length < 2) continue;
                            bool billboard = (short.Parse(parts[1]) == 1);

                            BoneForImport bone = (BoneForImport)m_Bones[currentBone];
                            bone.m_Billboard = billboard;
                        }
                        break;
                }
            }
            sr.Close();
        }

        /* Creates a list of bones based on object names found in OBJ file and assigns them 
         * default values. These will be replaced if a bone definition file is found.
         * By default there is one root parent bone and every other bone is a child bone with one child (until the end)
         */
        protected bool OBJ_LoadDefaultBones(String modelFileName)
        {
            Stream fs = File.OpenRead(m_ModelFileName);
            StreamReader sr = new StreamReader(fs);

            bool foundObjects = false;

            string currentBone = "";
            string rootBone = "";

            string curline;
            while ((curline = sr.ReadLine()) != null)
            {
                curline = curline.Trim();

                // skip empty lines and comments
                if (curline.Length < 1) continue;
                if (curline[0] == '#') continue;

                string[] parts = curline.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 1) continue;

                switch (parts[0])
                {
                    case "o": // object (bone)
                        {
                            currentBone = parts[1];
                            m_Bones.Add(currentBone, new BoneForImport(currentBone, 0, currentBone, currentBone, 0, false));
                            short parent_offset = -1;
                            if ("".Equals(rootBone))
                            {
                                rootBone = currentBone;
                                parent_offset = 0;
                            }
                            m_Bones[currentBone].m_RootBone = rootBone;
                            m_Bones[currentBone].m_ParentOffset = parent_offset;
                            if (parent_offset < 0)
                            {
                                m_Bones[currentBone].m_ParentBone = m_Bones.Values.ElementAt(GetBoneIndex(currentBone) + parent_offset).m_Name;
                                m_Bones[m_Bones[currentBone].m_ParentBone].m_HasChildren = true;// All but last bone set to has children
                            }
                            foundObjects = true;
                        }
                        break;
                }
            }

            sr.Close();

            return foundObjects;
        }

        protected override BMD Import(BMD model, String modelFileName, Vector3 scale)
        {
            return LoadModel_OBJ(model, modelFileName, scale);
        }

        /*
         * Main method used when importing an OBJ model. Loads an OBJ model and returns an imported BMD model.
         */
        protected BMD LoadModel_OBJ(BMD model, String modelFileName, Vector3 scale)
        {
            m_ImportedModel = model;

            m_Vertices = new List<Vector4>();
            m_TexCoords = new List<Vector2>();
            m_Normals = new List<Vector3>();
            m_Colours = new List<Color>();
            m_VertexBoneIDs = new List<int>();
            m_Materials = new Dictionary<string, MaterialDef>();
            m_Textures = new Dictionary<string, MaterialDef>();
            m_Bones = new Dictionary<string, BoneForImport>();
            m_SketchupHack = false;

            m_ModelFileName = modelFileName;
            m_ModelPath = Path.GetDirectoryName(m_ModelFileName);

            m_MD5 = new MD5CryptoServiceProvider();

            Stream fs = File.OpenRead(m_ModelFileName);
            StreamReader sr = new StreamReader(fs);

            string curmaterial = "";

            bool foundObjects = OBJ_LoadDefaultBones(modelFileName);
            string currentBone = "";
            int currentBoneID = 0;
            if (!foundObjects)
            {
                currentBone = "default_bone_name";
                m_Bones.Add(currentBone, new BoneForImport(currentBone, 0, currentBone, currentBone, 0, false));
            }

            string curline;
            while ((curline = sr.ReadLine()) != null)
            {
                curline = curline.Trim();

                // skip empty lines and comments
                if (curline.Length < 1) continue;
                if (curline[0] == '#') continue;

                string[] parts = curline.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 1) continue;

                switch (parts[0])
                {
                    case "mtllib": // material lib file
                        {
                            string filename = curline.Substring(parts[0].Length + 1).Trim();
                            OBJ_LoadMTL(m_ModelPath + Path.DirectorySeparatorChar + filename);
                        }
                        break;

                    case "bonelib": // bone definitions file
                        {
                            string filename = curline.Substring(parts[0].Length + 1).Trim();
                            LoadBoneDefinitionsForOBJ(m_ModelPath + Path.DirectorySeparatorChar + filename);
                        }
                        break;

                    case "o": // object (bone)
                        if (parts.Length < 2) continue;
                        currentBone = parts[1];
                        m_Bones[currentBone].m_Name = currentBone;
                        currentBoneID = GetBoneIndex(currentBone);
                        break;

                    case "usemtl": // material name
                        if (parts.Length < 2) continue;
                        curmaterial = parts[1];
                        if (!m_Materials.ContainsKey(curmaterial))
                        {
                            curmaterial = "default_white";
                            AddWhiteMat(currentBone);
                        }
                        // The parent bone should have a list of all materials used by itself and its children
                        if (!m_Bones[m_Bones[currentBone].m_RootBone].m_Materials.ContainsKey(curmaterial))
                            m_Bones[m_Bones[currentBone].m_RootBone].m_Materials.Add(curmaterial, m_Materials[curmaterial].copyAllButFaces());
                        if (!m_Bones[currentBone].m_Materials.ContainsKey(curmaterial))
                            m_Bones[currentBone].m_Materials.Add(curmaterial, m_Materials[curmaterial].copyAllButFaces());
                        break;

                    case "v": // vertex
                        {
                            if (parts.Length < 4) continue;
                            float x = float.Parse(parts[1], USA);
                            float y = float.Parse(parts[2], USA);
                            float z = float.Parse(parts[3], USA);
                            float w = 1f; //(parts.Length < 5) ? 1f : float.Parse(parts[4], USA);

                            m_Vertices.Add(new Vector4(x, y, z, w));
                            m_VertexBoneIDs.Add(currentBoneID);
                        }
                        break;

                    case "vt": // texcoord
                        {
                            if (parts.Length < 2) continue;
                            float s = float.Parse(parts[1], USA);
                            float t = (parts.Length < 3) ? 0f : float.Parse(parts[2], USA);

                            m_TexCoords.Add(new Vector2(s, t));
                        }
                        break;

                    case "vn": // normal
                        {
                            if (parts.Length < 4) continue;
                            float x = float.Parse(parts[1], USA);
                            float y = float.Parse(parts[2], USA);
                            float z = float.Parse(parts[3], USA);

                            Vector3 vec = new Vector3(x, y, z);
                            vec.Normalize();
                            m_Normals.Add(vec);
                        }
                        break;

                    case "vc": // vertex colour (non-standard "Extended OBJ" Blender plugin only)
                        {
                            if (parts.Length < 4) continue;
                            float r = float.Parse(parts[1], USA);
                            float g = float.Parse(parts[2], USA);
                            float b = float.Parse(parts[3], USA);

                            Color vcolour = Color.FromArgb((int)(r * 255.0f), (int)(g * 255.0f), (int)(b * 255.0f));
                            m_Colours.Add(vcolour);
                        }
                        break;

                    case "f": // face
                        {
                            if (parts.Length < 4) continue;
                            int nvtx = parts.Length - 1;

                            MaterialDef mat = new MaterialDef();
                            if (!curmaterial.Equals(""))
                            {
                                // If a new object is defined but a material to use not set, we need to use the previous one and add 
                                // it to the current bone and its parent
                                if (!m_Bones[m_Bones[currentBone].m_RootBone].m_Materials.ContainsKey(curmaterial))
                                    m_Bones[m_Bones[currentBone].m_RootBone].m_Materials.Add(curmaterial, m_Materials[curmaterial].copyAllButFaces());
                                if (!m_Bones[currentBone].m_Materials.ContainsKey(curmaterial))
                                    m_Bones[currentBone].m_Materials.Add(curmaterial, m_Materials[curmaterial].copyAllButFaces());

                                mat = (MaterialDef)m_Bones[currentBone].m_Materials[curmaterial];
                            }
                            else
                            {
                                // No "usemtl" command before declaring face
                                curmaterial = "default_white";
                                AddWhiteMat(currentBone);
                                mat = (MaterialDef)m_Bones[currentBone].m_Materials[curmaterial];
                            }
                            FaceDef face = new FaceDef();
                            face.m_MatName = curmaterial;
                            face.m_VtxIndices = new int[nvtx];
                            face.m_TxcIndices = new int[nvtx];
                            face.m_NrmIndices = new int[nvtx];
                            face.m_ColIndices = new int[nvtx];
                            face.m_BoneIDs = new int[nvtx];

                            for (int i = 0; i < nvtx; i++)
                            {
                                string vtx = parts[i + 1];
                                string[] idxs = vtx.Split(new char[] { '/' });

                                face.m_VtxIndices[i] = int.Parse(idxs[0]) - 1;
                                face.m_TxcIndices[i] = (mat.m_HasTextures && idxs.Length >= 2 && idxs[1].Length > 0)
                                    ? (int.Parse(idxs[1]) - 1) : -1;
                                face.m_NrmIndices[i] = (idxs.Length >= 3 && !idxs[2].Equals("")) ? (int.Parse(idxs[2]) - 1) : -1;
                                // Vertex colours (non-standard "Extended OBJ" Blender plugin only)
                                face.m_ColIndices[i] = (idxs.Length >= 4 && !idxs[3].Equals("")) ? (int.Parse(idxs[3]) - 1) : -1;
                                face.m_BoneIDs[i] = currentBoneID;
                            }

                            m_Bones[currentBone].m_Materials[curmaterial] = mat;
                            m_Bones[currentBone].m_Materials[curmaterial].m_Faces.Add(face);
                        }
                        break;
                }
            }

            sr.Close();

            GenerateBMD(scale, false);

            return m_ImportedModel;
        }

    }
}