/* OBJLoader
 * 
 * Converts a Wavefront OBJ model into a ModelBase object for use in BMDImporter and KCLImporter.
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
using SM64DSe.ImportExport.Loaders;

namespace SM64DSe.ImportExport.Loaders.ExternalLoaders
{
    public class OBJLoader : AbstractModelLoader
    {
        public List<Vector4> m_Vertices;
        public List<Vector2> m_TexCoords;
        public List<Vector3> m_Normals;
        public List<Color> m_Colours;
        public List<int> m_VertexBoneIDs;
        private bool m_SketchupHack;

        public OBJLoader(string modelFileName) : 
            base(modelFileName)
        {
            m_Vertices = new List<Vector4>();
            m_TexCoords = new List<Vector2>();
            m_Normals = new List<Vector3>();
            m_Colours = new List<Color>();
            m_VertexBoneIDs = new List<int>();

            m_SketchupHack = false;
        }

        public override ModelBase LoadModel(Vector3 scale)
        {
            if (m_ModelFileName == null || "".Equals(m_ModelFileName))
                throw new SystemException("You must specify the filename of the model to load via the constructor before " +
                    "calling LoadModel()");

            Stream fs = File.OpenRead(m_ModelFileName);
            StreamReader sr = new StreamReader(fs);

            string curmaterial = null;

            bool foundObjects = LoadDefaultBones(m_ModelFileName);
            string currentBone = null;
            int currentBoneIndex = -1;
            if (!foundObjects)
            {
                currentBone = "default_bone_name";
                m_Model.m_BoneTree.AddRootBone(new ModelBase.BoneDef(currentBone));
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
                            LoadMaterials(m_ModelPath + Path.DirectorySeparatorChar + filename);
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
                        m_Model.m_BoneTree.GetBoneByID(currentBone).m_Geometries.Add(currentBone, new ModelBase.GeometryDef(currentBone));
                        currentBoneIndex = m_Model.m_BoneTree.GetBoneIndex(currentBone);
                        break;

                    case "usemtl": // material name
                        if (parts.Length < 2) continue;
                        curmaterial = parts[1];
                        if (!m_Model.m_Materials.ContainsKey(curmaterial))
                        {
                            curmaterial = "default_white";
                            AddWhiteMat(currentBone);
                        }
                        // The parent bone should have a list of all materials used by itself and its children
                        if (!m_Model.m_BoneTree.GetBoneByID(currentBone).GetRoot().m_MaterialsInBranch.Contains(curmaterial))
                            m_Model.m_BoneTree.GetBoneByID(currentBone).GetRoot().m_MaterialsInBranch.Add(curmaterial);
                        if (!m_Model.m_BoneTree.GetBoneByID(currentBone).m_MaterialsInBranch.Contains(curmaterial))
                            m_Model.m_BoneTree.GetBoneByID(currentBone).m_MaterialsInBranch.Add(curmaterial);
                        break;

                    case "v": // vertex
                        {
                            if (parts.Length < 4) continue;
                            float x = float.Parse(parts[1], USA);
                            float y = float.Parse(parts[2], USA);
                            float z = float.Parse(parts[3], USA);
                            float w = 1f; //(parts.Length < 5) ? 1f : float.Parse(parts[4], USA);

                            m_Vertices.Add(new Vector4(x, y, z, w));
                            m_VertexBoneIDs.Add(currentBoneIndex);
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

                            if (curmaterial != null)
                            {
                                // If a new object is defined but a material to use not set, we need to use the previous one and add 
                                // it to the current bone and its parent
                                if (!m_Model.m_BoneTree.GetBoneByID(currentBone).GetRoot().m_MaterialsInBranch.Contains(curmaterial))
                                    m_Model.m_BoneTree.GetBoneByID(currentBone).GetRoot().m_MaterialsInBranch.Add(curmaterial);
                                if (!m_Model.m_BoneTree.GetBoneByID(currentBone).m_MaterialsInBranch.Contains(curmaterial))
                                    m_Model.m_BoneTree.GetBoneByID(currentBone).m_MaterialsInBranch.Add(curmaterial);
                            }
                            else
                            {
                                // No "usemtl" command before declaring face
                                curmaterial = "default_white";
                                AddWhiteMat(currentBone);
                            }

                            ModelBase.BoneDef bone = m_Model.m_BoneTree.GetBoneByID(currentBone);
                            if (!bone.m_Geometries.Values.ElementAt(0).m_PolyLists.ContainsKey(curmaterial))
                            {
                                bone.m_Geometries.Values.ElementAt(0).m_PolyLists.Add(
                                    curmaterial, new ModelBase.PolyListDef(currentBone + "." + curmaterial, curmaterial));
                            }
                            ModelBase.PolyListDef polyList = bone.m_Geometries.Values.ElementAt(0).m_PolyLists[curmaterial];

                            ModelBase.FaceDef face = new ModelBase.FaceDef();
                            face.m_NumVertices = nvtx;
                            face.m_Vertices = new Vector3[nvtx];
                            face.m_TextureCoordinates = new Vector2?[nvtx];
                            face.m_Normals = new Vector3?[nvtx];
                            face.m_VertexColours = new Color?[nvtx];
                            face.m_VertexBoneIDs = new int[nvtx];

                            for (int i = 0; i < nvtx; i++)
                            {
                                string vtx = parts[i + 1];
                                string[] idxs = vtx.Split(new char[] { '/' });

                                face.m_Vertices[i] = m_Vertices[int.Parse(idxs[0]) - 1].Xyz;
                                if (m_Model.m_Materials[curmaterial].m_HasTextures && idxs.Length >= 2 && idxs[1].Length > 0)
                                    face.m_TextureCoordinates[i] = m_TexCoords[int.Parse(idxs[1]) - 1];
                                else
                                    face.m_TextureCoordinates[i] = null;
                                if (idxs.Length >= 3 && !idxs[2].Equals(""))
                                    face.m_Normals[i] = m_Normals[int.Parse(idxs[2]) - 1];
                                else
                                    face.m_Normals[i] = null;
                                // Vertex colours (non-standard "Extended OBJ" Blender plugin only)
                                if (idxs.Length >= 4 && !idxs[3].Equals(""))
                                    face.m_VertexColours[i] = m_Colours[int.Parse(idxs[3]) - 1];
                                else
                                    face.m_VertexColours[i] = null;
                                face.m_VertexBoneIDs[i] = currentBoneIndex;
                            }

                            polyList.m_Faces.Add(face);
                        }
                        break;
                }
            }

            sr.Close();

            m_Model.ScaleModel(scale);

            return m_Model;
        }

        private void LoadMaterials(string filename)
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

                            ModelBase.MaterialDef mat = new ModelBase.MaterialDef(curmaterial);
                            mat.m_Index = m_Model.m_Materials.Count;
                            mat.m_DiffuseColour = Color.White;
                            mat.m_Opacity = 255; // oops
                            mat.m_HasTextures = false;
                            mat.m_DiffuseMapName = "";
                            mat.m_DiffuseMapID = 0;
                            mat.m_DiffuseMapSize = new Vector2(0f, 0f);
                            mat.m_ColType = 0;
                            if (!m_Model.m_Materials.ContainsKey(curmaterial))
                                m_Model.m_Materials.Add(curmaterial, mat);
                        }
                        break;

                    case "d":
                    case "Tr": // opacity
                        {
                            if (parts.Length < 2) continue;
                            float o = float.Parse(parts[1], USA);
                            if (m_SketchupHack)
                                o *= 255;

                            ModelBase.MaterialDef mat = (ModelBase.MaterialDef)m_Model.m_Materials[curmaterial];
                            mat.m_Opacity = Math.Max(0, Math.Min(255, (int)(o * 255)));
                        }
                        break;

                    case "Kd": // diffuse colour
                        {
                            if (parts.Length < 4) continue;
                            float r = float.Parse(parts[1], USA);
                            float g = float.Parse(parts[2], USA);
                            float b = float.Parse(parts[3], USA);
                            Color col = Color.FromArgb((int)(r * 255), (int)(g * 255), (int)(b * 255));

                            ModelBase.MaterialDef mat = (ModelBase.MaterialDef)m_Model.m_Materials[curmaterial];
                            mat.m_DiffuseColour = col;
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

                                ModelBase.MaterialDef mat = (ModelBase.MaterialDef)m_Model.m_Materials[curmaterial];
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
                                if (m_Model.m_Textures.ContainsKey(imghash))
                                {
                                    ModelBase.MaterialDef mat2 = m_Model.m_Textures[imghash];
                                    mat.m_DiffuseMapName = mat2.m_DiffuseMapName;
                                    mat.m_DiffuseMapID = mat2.m_DiffuseMapID;
                                    mat.m_DiffuseMapSize = mat2.m_DiffuseMapSize;
                                    break;
                                }

                                mat.m_DiffuseMapName = texname;
                                m_Model.m_Textures.Add(imghash, mat);

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

        protected void LoadBoneDefinitionsForOBJ(string filename)
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

            m_Model.m_BoneTree.Clear();

            ModelBase.BoneDef bone = null;

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

                            bone = new ModelBase.BoneDef(parts[1]);
                        }
                        break;

                    case "parent_offset": // Offset in bones to parent bone (signed 16-bit. 0=no parent, -1=parent is the previous bone, ...)
                        {
                            if (parts.Length < 2) continue;
                            short parent_offset = short.Parse(parts[1]);

                            if ((parent_offset < 0 && m_Model.m_BoneTree.Count == 0) || parent_offset > 0)
                            {
                                throw new SystemException("Child bones cannot be defined before their parent in: " + filename);
                            }

                            if (parent_offset == 0)
                            {
                                m_Model.m_BoneTree.AddRootBone(bone);
                            }
                            else if (parent_offset < 0)
                            {
                                List<ModelBase.BoneDef> listOfBones = m_Model.m_BoneTree.GetAsList();
                                listOfBones[listOfBones.Count + parent_offset].AddChild(bone);
                            }
                        }
                        break;

                    case "has_children": // 1 if the bone has children, 0 otherwise
                        {
                            if (parts.Length < 2) continue;
                            bool has_children = (short.Parse(parts[1]) == 1);
                            // No longer needed
                        }
                        break;

                    case "sibling_offset": // Offset in bones to the next sibling bone (0=bone is last child of its parent)
                        {
                            if (parts.Length < 2) continue;
                            short sibling_offset = short.Parse(parts[1]);
                            // No longer needed
                        }
                        break;

                    case "scale": // Scale transformation
                        {
                            if (parts.Length < 4) continue;
                            uint[] scale = new uint[] { uint.Parse(parts[1], System.Globalization.NumberStyles.HexNumber), 
                                uint.Parse(parts[2], System.Globalization.NumberStyles.HexNumber), 
                                uint.Parse(parts[3], System.Globalization.NumberStyles.HexNumber) };

                            bone.SetScale(scale);
                        }
                        break;

                    case "rotation": // Rotation transformation
                        {
                            if (parts.Length < 4) continue;
                            ushort[] rotation = new ushort[] { ushort.Parse(parts[1], System.Globalization.NumberStyles.HexNumber), 
                                ushort.Parse(parts[2], System.Globalization.NumberStyles.HexNumber), 
                                ushort.Parse(parts[3], System.Globalization.NumberStyles.HexNumber) };

                            bone.SetRotation(rotation);
                        }
                        break;

                    case "translation": // Scale transformation
                        {
                            if (parts.Length < 4) continue;
                            uint[] translation = new uint[] { uint.Parse(parts[1], System.Globalization.NumberStyles.HexNumber), 
                                uint.Parse(parts[2], System.Globalization.NumberStyles.HexNumber), 
                                uint.Parse(parts[3], System.Globalization.NumberStyles.HexNumber) };

                            bone.SetTranslation(translation);
                        }
                        break;
                    case "billboard": // Always rendered facing camera (not yet implemented)
                        {
                            if (parts.Length < 2) continue;
                            bool billboard = (short.Parse(parts[1]) == 1);

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
        protected bool LoadDefaultBones(string modelFileName)
        {
            Stream fs = File.OpenRead(m_ModelFileName);
            StreamReader sr = new StreamReader(fs);

            bool foundObjects = false;

            ModelBase.BoneDef bone = null;

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
                            bone = new ModelBase.BoneDef(parts[1]);

                            if (m_Model.m_BoneTree.Count == 0)
                            {
                                m_Model.m_BoneTree.AddRootBone(bone);
                            }
                            else
                            {
                                m_Model.m_BoneTree.GetAsList()[m_Model.m_BoneTree.Count - 1].AddChild(bone);
                            }

                            foundObjects = true;
                        }
                        break;
                }
            }

            sr.Close();

            return foundObjects;
        }

        public override Dictionary<string, ModelBase.MaterialDef> GetModelMaterials()
        {
            if (m_Model.m_Materials.Count > 0)
                return m_Model.m_Materials;
            else
            {
                LoadModel();
                return m_Model.m_Materials;
            }
        }
    }
}
