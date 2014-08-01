/* COLLADA DAE:
 *  
 * Imports rigged and animated COLLADA DAE models to BMD and BCA.
 * 
 * Notes:
 * BMD does not support vertex weights, instead each vertex is only assigned to one bone - where a 
 * DAE model uses multiple < 1.0 vertex weights, the largest value is used to assign the vertex to 
 * a joint.
 * 
 * This is the recommended format for importing as it matches the features of BMD almost exactly.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using System.Drawing;
using OpenTK;
using System.Text.RegularExpressions;
using OpenTK.Graphics.OpenGL;
using System.Security.Cryptography;

namespace SM64DSe.Importers
{
    public class BMD_BCA_Importer_DAE : BMD_Importer_Base
    {
        // COLLADA DAE specific
        protected BiDictionaryOneToOne<string, string> m_DAEGeometryToRootBoneMap;
        protected Dictionary<string, string> m_DAEGeometryPositionsListNames;
        protected Dictionary<string, string> m_DAEControllerToGeometryNameMap;
        protected BiDictionaryOneToOne<string, string> m_DAEBoneIDToBoneSIDMap;
        protected Dictionary<string, Dictionary<string, TransformationType>> m_DAEBoneTransformsList;
        protected Dictionary<string, Dictionary<string, string>> m_DAEGeometryInstanceMaterialNameMap;

        protected Dictionary<string, Dictionary<string, AnimationForImport>> m_Animations;

        protected void DAE_LoadMTL(String filename)
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

            Dictionary<string, string> textureNames = new Dictionary<string, string>();

            // Get a list of texture names first, in case a meterial references one not yet read in
            using (XmlReader reader = XmlReader.Create(filename))
            {
                reader.MoveToContent();
                reader.ReadToFollowing("library_images");

                while (reader.Read())
                {
                    if (reader.NodeType.Equals(XmlNodeType.Element))
                    {
                        switch (reader.LocalName)
                        {
                            case "image":
                                {
                                    String name = reader.GetAttribute("id");
                                    String value = "";
                                    while (reader.Read())
                                    {
                                        reader.MoveToContent();
                                        if (reader.LocalName.Equals("init_from"))
                                        {
                                            value = reader.ReadElementContentAsString();
                                        }
                                        else
                                            break;
                                    }
                                    textureNames.Add(name, value);
                                }
                                break;
                        }
                    }
                }
            }

            // Map effects to materials
            BiDictionaryOneToOne<string, string> effectMaterialMap = new BiDictionaryOneToOne<string, string>();

            using (XmlReader reader = XmlReader.Create(filename))
            {
                reader.MoveToContent();
                reader.ReadToFollowing("library_materials");

                string material = "";
                string effect = "";

                while (reader.Read())
                {
                    if (reader.NodeType.Equals(XmlNodeType.Element))
                    {
                        switch (reader.LocalName)
                        {
                            case "material":
                                material = reader.GetAttribute("id");
                                break;
                            case "instance_effect":
                                effect = reader.GetAttribute("url").Replace("#", "");
                                effectMaterialMap.Add(material, effect);
                                break;
                        }
                    }
                }
            }

            using (XmlReader reader = XmlReader.Create(filename))
            {
                reader.MoveToContent();
                reader.ReadToFollowing("library_effects");

                Dictionary<string, string> surfaces = new Dictionary<string, string>();
                Dictionary<string, string> samplers = new Dictionary<string, string>();

                while (reader.Read())
                {
                    if (reader.NodeType.Equals(XmlNodeType.Element))
                    {
                        switch (reader.LocalName)
                        {
                            case "effect":
                                {
                                    string effect = reader.GetAttribute("id");
                                    string material_name = effectMaterialMap.GetBySecond(effect);
                                    curmaterial = material_name;

                                    MaterialDef mat = new MaterialDef();
                                    mat.m_ID = m_Materials.Count;
                                    mat.m_Name = curmaterial;
                                    mat.m_Faces = new List<FaceDef>();
                                    mat.m_DiffuseColor = Color.White;
                                    mat.m_Opacity = 255;
                                    mat.m_HasTextures = false;
                                    mat.m_DiffuseMapName = "";
                                    mat.m_DiffuseMapID = 0;
                                    mat.m_DiffuseMapSize = new Vector2(0f, 0f);
                                    mat.m_ColType = 0;
                                    if (!m_Materials.ContainsKey(curmaterial))
                                        m_Materials.Add(curmaterial, mat);
                                }
                                break;
                            case "newparam":
                                {
                                    string id = ((id = reader.GetAttribute("sid")) != null) ? id : reader.GetAttribute("id");
                                    while (reader.Read())
                                    {
                                        reader.MoveToContent();
                                        if (reader.LocalName.Equals("surface") && reader.NodeType.Equals(XmlNodeType.Element))
                                        {
                                            while (reader.Read())
                                            {
                                                reader.MoveToContent();
                                                if (reader.LocalName.Equals("init_from") && reader.NodeType.Equals(XmlNodeType.Element))
                                                {
                                                    string value = reader.ReadString();
                                                    if (!surfaces.ContainsKey(id))
                                                        surfaces.Add(id, value);
                                                }
                                                else
                                                    break;
                                            }
                                        }
                                        else if (reader.LocalName.Equals("sampler2D") && reader.NodeType.Equals(XmlNodeType.Element))
                                        {
                                            while (reader.Read())
                                            {
                                                reader.MoveToContent();
                                                if (reader.LocalName.Equals("source") && reader.NodeType.Equals(XmlNodeType.Element))
                                                {
                                                    string value = reader.ReadString();
                                                    if (!samplers.ContainsKey(id))
                                                        samplers.Add(id, value);
                                                }
                                                else
                                                    break;
                                            }
                                        }
                                        else
                                            break;
                                    }
                                }
                                break;
                            case "diffuse":
                                while (reader.Read())
                                {
                                    reader.MoveToContent();
                                    if (reader.LocalName.Equals("color"))
                                    {
                                        String value = reader.ReadElementContentAsString();
                                        String[] rgba = value.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                        float r = float.Parse(rgba[0], USA);
                                        float g = float.Parse(rgba[1], USA);
                                        float b = float.Parse(rgba[2], USA);
                                        float a = float.Parse(rgba[3], USA);
                                        Color col = Color.FromArgb((int)(a * 255), (int)(r * 255), (int)(g * 255), (int)(b * 255));

                                        MaterialDef mat = (MaterialDef)m_Materials[curmaterial];
                                        mat.m_DiffuseColor = col;
                                    }
                                    else if (reader.LocalName.Equals("texture"))
                                    {
                                        string textureAttr = reader.GetAttribute("texture");
                                        string texname = (textureNames.ContainsKey(Regex.Replace(textureAttr, @"-sampler$", String.Empty))) ? 
                                            textureNames[Regex.Replace(reader.GetAttribute("texture"), @"-sampler$", String.Empty)] :
                                            textureNames[surfaces[samplers[textureAttr]]];
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
                                    else
                                        break;
                                }
                                break;
                            case "transparency":
                                while (reader.Read())
                                {
                                    reader.MoveToContent();
                                    if (reader.LocalName.Equals("float"))
                                    {
                                        float o = float.Parse(reader.ReadElementContentAsString(), USA);

                                        MaterialDef mat = (MaterialDef)m_Materials[curmaterial];
                                        mat.m_Opacity = Math.Max(0, Math.Min(255, (int)(o * 255)));
                                    }
                                    else
                                        break;
                                }
                                break;
                        }
                    }
                }
            }

            if (!imagesNotFound.Equals(""))
                MessageBox.Show("The following images were not found:\n\n" + imagesNotFound);

            sr.Close();
        }

        protected override BMD Import(BMD model, String modelFileName, Vector3 scale)
        {
            return LoadModel_DAE(model, modelFileName, scale);
        }

        protected override BCA ImportAnimation(BCA animation, string modelFileName)
        {
            return LoadModel_DAE_Animation(animation, modelFileName);
        }

        /* 
         * Main method used to return converted BMD model
         */ 
        public BMD LoadModel_DAE(BMD model, String modelFileName, Vector3 scale)
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

            m_ModelFileName = modelFileName;
            m_ModelPath = Path.GetDirectoryName(m_ModelFileName);

            currentBone = "";
            currentBoneID = -1;
            curmaterial = "";

            m_MD5 = new MD5CryptoServiceProvider();

            bool foundObjects = DAE_LoadBones(modelFileName);
            if (!foundObjects)
            {
                //throw new Exception("This model contains no geometry.");
                currentBone = "default_bone_name";
                currentBoneID = 0;
                m_Bones.Add(currentBone, new BoneForImport(currentBone, 0, currentBone, currentBone, 0, false));
            }
            geometryVertexOffsets = new int[m_Bones.Count];

            DAE_LoadMTL(modelFileName);

            // Get the name of the vertices list first - needed to distinguish between positions and normals
            m_DAEGeometryPositionsListNames = ReadDAEGeometryPositionsListNames(modelFileName);

            using (XmlReader reader = XmlReader.Create(modelFileName))
            {
                reader.MoveToContent();

                while (reader.Read())
                {
                    if (reader.NodeType.Equals(XmlNodeType.Element))
                    {
                        if (reader.LocalName.Equals("geometry"))
                        {
                            ReadDAE_Geometry(reader);
                        }
                    }
                    else if (reader.NodeType.Equals(XmlNodeType.EndElement))
                    {
                        if (reader.LocalName.Equals("COLLADA"))
                        {
                            break;
                        }
                    }
                }
            }
            using (XmlReader reader = XmlReader.Create(modelFileName))
            {
                reader.MoveToContent();

                while (reader.Read())
                {
                    if (reader.NodeType.Equals(XmlNodeType.Element))
                    {
                        if (reader.LocalName.Equals("library_controllers"))
                        {
                            ReadDAE_LibraryControllers(reader);
                        }
                    }
                    else if (reader.NodeType.Equals(XmlNodeType.EndElement))
                    {
                        if (reader.LocalName.Equals("COLLADA"))
                        {
                            break;
                        }
                    }
                }
            }

            GenerateBMD(scale, false);

            return m_ImportedModel;
        }

        /* 
         * Main method called to return imported BCA for the imported model
         */ 
        public BCA LoadModel_DAE_Animation(BCA animation, String modelFileName)
        {
            m_ImportedAnimation = animation;

            using (XmlReader reader = XmlReader.Create(modelFileName))
            {
                reader.MoveToContent();

                while (reader.Read())
                {
                    if (reader.NodeType.Equals(XmlNodeType.Element))
                    {
                        if (reader.LocalName.Equals("library_animations"))
                        {
                            m_Animations = new Dictionary<string, Dictionary<string, AnimationForImport>>();
                            ReadDAE_LibraryAnimations(reader);
                        }
                    }
                    else if (reader.NodeType.Equals(XmlNodeType.EndElement))
                    {
                        if (reader.LocalName.Equals("COLLADA"))
                        {
                            break;
                        }
                    }
                }
            }

            ProcessAnimationFrames();

            GenerateBCA(false);

            return m_ImportedAnimation;
        }

        private const float FRAMES_PER_SECOND = 30.0f;
        private const float INTERVAL = 1.0f / FRAMES_PER_SECOND;
        protected void ProcessAnimationFrames()
        {
            Dictionary<string, List<Tuple<float[], TransformationType>>> finalAnimationFrameValues =
                new Dictionary<string, List<Tuple<float[], TransformationType>>>();

            int boneInd = 0;
            foreach (Dictionary<string, AnimationForImport> dict in m_Animations.Values)
            {
                string boneName = m_Animations.Keys.ElementAt(boneInd);
                finalAnimationFrameValues.Add(boneName, new List<Tuple<float[], TransformationType>>());

                foreach (AnimationForImport anim in dict.Values)
                {
                    foreach (AnimationSamplerChannelPairForImport sampler in anim.m_SamplerChannelPairs.Values)
                    {
                        List<Tuple<float[], TransformationType>> alignedFrameValuesList = new List<Tuple<float[], TransformationType>>();

                        List<Tuple<float[], TransformationType>> outputValuesList = new List<Tuple<float[], TransformationType>>();

                        if (sampler.m_Type == TransformationType.None)
                        {
                            continue;
                        }
                        else if (sampler.m_Type == TransformationType.TransformationMatrix)
                        {
                            float[] floatVals = anim.m_Sources[sampler.m_OutputSourceID].m_FloatArray;
                            int count = anim.m_Sources[sampler.m_OutputSourceID].m_Count;
                            int stride = anim.m_Sources[sampler.m_OutputSourceID].m_Stride;
                            Matrix4[] matrices = new Matrix4[count];
                            List<float> Sx = new List<float>();
                            List<float> Sy = new List<float>();
                            List<float> Sz = new List<float>();
                            List<float> Rx = new List<float>();
                            List<float> Ry = new List<float>();
                            List<float> Rz = new List<float>();
                            List<float> Tx = new List<float>();
                            List<float> Ty = new List<float>();
                            List<float> Tz = new List<float>();
                            for (int ind = 0; ind < floatVals.Length; ind += stride)
                            {
                                float[] tmp = new float[stride];
                                Array.Copy(floatVals, ind, tmp, 0, stride);
                                Matrix4 matrix = Helper.FloatArrayToMatrix4(tmp);
                                Vector3 scale = new Vector3();
                                Vector3 rotation = new Vector3();
                                Vector3 translation = new Vector3();
                                Helper.DecomposeSRTMatrix1(matrix, out scale, out rotation, out translation);
                                Sx.Add(scale.X); Sy.Add(scale.Y); Sz.Add(scale.Z);
                                Rx.Add(rotation.X); Ry.Add(rotation.Y); Rz.Add(rotation.Z);
                                Tx.Add(translation.X); Ty.Add(translation.Y); Tz.Add(translation.Z);
                            }
                            outputValuesList.Add(new Tuple<float[], TransformationType>(Sx.ToArray(), TransformationType.ScaleX));
                            outputValuesList.Add(new Tuple<float[], TransformationType>(Sy.ToArray(), TransformationType.ScaleY));
                            outputValuesList.Add(new Tuple<float[], TransformationType>(Sz.ToArray(), TransformationType.ScaleZ));
                            outputValuesList.Add(new Tuple<float[], TransformationType>(Rx.ToArray(), TransformationType.RotationX));
                            outputValuesList.Add(new Tuple<float[], TransformationType>(Ry.ToArray(), TransformationType.RotationY));
                            outputValuesList.Add(new Tuple<float[], TransformationType>(Rz.ToArray(), TransformationType.RotationZ));
                            outputValuesList.Add(new Tuple<float[], TransformationType>(Tx.ToArray(), TransformationType.TranslationX));
                            outputValuesList.Add(new Tuple<float[], TransformationType>(Ty.ToArray(), TransformationType.TranslationY));
                            outputValuesList.Add(new Tuple<float[], TransformationType>(Tz.ToArray(), TransformationType.TranslationZ));
                        }
                        else if (sampler.m_Type == TransformationType.TranslationXYZ || sampler.m_Type == TransformationType.ScaleXYZ)
                        {
                            float[] tmp = anim.m_Sources[sampler.m_OutputSourceID].m_FloatArray;
                            int count = anim.m_Sources[sampler.m_OutputSourceID].m_Count;
                            int stride = anim.m_Sources[sampler.m_OutputSourceID].m_Stride;
                            float[] x = new float[count];
                            float[] y = new float[count];
                            float[] z = new float[count];
                            int tmpCount = 0;
                            for (int ind = anim.m_Sources[sampler.m_OutputSourceID].m_StartIndexOffset;
                                ind < anim.m_Sources[sampler.m_OutputSourceID].m_StartIndexOffset + tmp.Length; ind += stride)
                            {
                                x[tmpCount] = tmp[ind + 0];
                                y[tmpCount] = tmp[ind + 1];
                                z[tmpCount] = tmp[ind + 2];
                                tmpCount++;
                            }
                            outputValuesList.Add(new Tuple<float[], TransformationType>(x,
                                (sampler.m_Type == TransformationType.TranslationXYZ) ? TransformationType.TranslationX : TransformationType.ScaleX));
                            outputValuesList.Add(new Tuple<float[], TransformationType>(y,
                                (sampler.m_Type == TransformationType.TranslationXYZ) ? TransformationType.TranslationY : TransformationType.ScaleY));
                            outputValuesList.Add(new Tuple<float[], TransformationType>(z,
                                (sampler.m_Type == TransformationType.TranslationXYZ) ? TransformationType.TranslationZ : TransformationType.ScaleZ));
                        }
                        else if (sampler.m_Type == TransformationType.RotationX || sampler.m_Type == TransformationType.RotationY ||
                            sampler.m_Type == TransformationType.RotationZ)
                        {
                            // COLLADA stores rotation in Radians, need to convert to Degrees
                            for (int i = 0; i < anim.m_Sources[sampler.m_OutputSourceID].m_FloatArray.Length; i++)
                            {
                                anim.m_Sources[sampler.m_OutputSourceID].m_FloatArray[i] =
                                    anim.m_Sources[sampler.m_OutputSourceID].m_FloatArray[i] * Helper.Deg2Rad;
                            }
                            outputValuesList.Add(new Tuple<float[], TransformationType>(
                                anim.m_Sources[sampler.m_OutputSourceID].m_FloatArray, sampler.m_Type));
                        }
                        else // Shouldn't happen
                        {
                            outputValuesList.Add(new Tuple<float[], TransformationType>(
                                anim.m_Sources[sampler.m_OutputSourceID].m_FloatArray, sampler.m_Type));
                        }

                        foreach (Tuple<float[], TransformationType> outputValues in outputValuesList)
                        {
                            alignedFrameValuesList.Add(new Tuple<float[], TransformationType>(
                                InterpolateFramesAndExtractOneOverFrameRateFPS(anim, sampler, outputValues.Item1),
                                outputValues.Item2));
                        }
                        finalAnimationFrameValues[boneName].AddRange(alignedFrameValuesList);
                    }
                }

                string finalAnimationName = "FinalAnimation";
                while (m_Animations[boneName].ContainsKey(finalAnimationName))
                    finalAnimationName += "0";
                m_Animations[boneName].Add(finalAnimationName, new AnimationForImport(finalAnimationName, true));

                boneInd++;
            }

            int numBones = m_Animations.Count;

            for (int i = 0; i < numBones; i++)
            {
                List<Tuple<float[], TransformationType>> alignedFrameValuesList = finalAnimationFrameValues.Values.ElementAt(i);
                AnimationForImport anim = null;
                if (m_Animations[finalAnimationFrameValues.Keys.ElementAt(i)].Values.
                    ElementAt(m_Animations[finalAnimationFrameValues.Keys.ElementAt(i)].Values.Count - 1).m_IsTotal)
                {
                    anim = m_Animations[finalAnimationFrameValues.Keys.ElementAt(i)].Values.
                        ElementAt(m_Animations[finalAnimationFrameValues.Keys.ElementAt(i)].Values.Count - 1);
                }
                else
                {
                    foreach (AnimationForImport afi in m_Animations[finalAnimationFrameValues.Keys.ElementAt(i)].Values)
                    {
                        if (afi.m_IsTotal)
                        {
                            anim = afi;
                            break;
                        }
                    }
                }

                for (int f = 0; f < alignedFrameValuesList.ElementAt(0).Item1.Length; f++)
                {
                    anim.m_Frames.Add(new AnimationFrameForImport());
                }

                foreach (Tuple<float[], TransformationType> alignedFrameValues in alignedFrameValuesList)
                {
                    for (int f = 0; f < alignedFrameValues.Item1.Length; f++)
                    {
                        AnimationFrameForImport frame = anim.m_Frames.ElementAt(f);

                        switch (alignedFrameValues.Item2)
                        {
                            case TransformationType.ScaleX:
                                frame.m_Scale.X = alignedFrameValues.Item1[f];
                                break;
                            case TransformationType.ScaleY:
                                frame.m_Scale.Y = alignedFrameValues.Item1[f];
                                break;
                            case TransformationType.ScaleZ:
                                frame.m_Scale.Z = alignedFrameValues.Item1[f];
                                break;
                            case TransformationType.RotationX:
                                frame.m_Rotation.X = alignedFrameValues.Item1[f];
                                break;
                            case TransformationType.RotationY:
                                frame.m_Rotation.Y = alignedFrameValues.Item1[f];
                                break;
                            case TransformationType.RotationZ:
                                frame.m_Rotation.Z = alignedFrameValues.Item1[f];
                                break;
                            case TransformationType.TranslationX:
                                frame.m_Translation.X = alignedFrameValues.Item1[f];
                                break;
                            case TransformationType.TranslationY:
                                frame.m_Translation.Y = alignedFrameValues.Item1[f];
                                break;
                            case TransformationType.TranslationZ:
                                frame.m_Translation.Z = alignedFrameValues.Item1[f];
                                break;
                        }
                    }
                }
            }

        }

        protected float[] InterpolateFramesAndExtractOneOverFrameRateFPS(AnimationForImport anim,
            AnimationSamplerChannelPairForImport sampler, float[] outputValues)
        {
            float[] time = anim.m_Sources[sampler.m_InputSourceID].m_FloatArray;
            int numFrames = (int)Math.Ceiling(time[time.Length - 1] * FRAMES_PER_SECOND) + 1;
            float[] alignedFrameValues = new float[numFrames];

            float smallestDifference = GetSmallestDifference(time);
            float smallestInterval = (smallestDifference < INTERVAL) ? smallestDifference : INTERVAL;

            float lastFrameTime = time[time.Length - 1];
            int numInterpolatedFrames = (int)Math.Round(((lastFrameTime / smallestInterval) + 1), 2);
            List<float> interpolatedFrames = new List<float>();

            List<float> interpolatedTime = new List<float>();

            // Interpolate and then convert to (1 / FRAMES_PER_SECOND) frames using closest to keyframe time
            for (int kf = 0; kf < time.Length - 1; kf++)
            {
                float time_diff = time[kf + 1] - time[kf];
                if (time_diff < 0)
                    time_diff *= (-1);
                float value_diff = outputValues[kf + 1] - outputValues[kf];

                int numFramesInGap = (int)Math.Round((time_diff / smallestInterval), 2);
                List<float> interp = new List<float>();

                if (numFramesInGap == 1)
                {
                    interp.Add(outputValues[kf]);
                    interpolatedTime.Add(time[kf]);
                }
                else
                {
                    for (int inf = 0; inf < numFramesInGap; inf++)
                    {
                        interp.Add(outputValues[kf] + (inf * (value_diff / numFramesInGap)));
                        interpolatedTime.Add(time[kf] + (inf * (time_diff / numFramesInGap)));
                    }
                }
                interpolatedFrames.AddRange(interp);
            }
            interpolatedFrames.Add(outputValues[outputValues.Length - 1]);
            interpolatedTime.Add(time[time.Length - 1]);

            float[] tmpTime = interpolatedTime.ToArray();
            for (int kf = 0; kf < numFrames; kf++)
            {
                float frameTime = (float)kf * INTERVAL;
                int indexClosest = GetIndexClosest(tmpTime, frameTime);
                alignedFrameValues[kf] = interpolatedFrames[indexClosest];
            }
            //alignedFrameValues[numFrames - 1] = interpolatedFrames[numInterpolatedFrames - 1];

            return alignedFrameValues;
        }

        private static int GetIndexClosest(float[] values, float target)
        {
            int index = -1;
            float closest = int.MaxValue;
            float minDifference = int.MaxValue;
            for (int i = 0; i < values.Length; i++)
            {
                double difference = Math.Abs(values[i] - target);
                if (minDifference > difference)
                {
                    minDifference = (float)difference;
                    closest = values[i];
                    index = i;
                }
            }

            return index;
        }

        private static float GetSmallestDifference(float[] values)
        {
            float smallest = float.MaxValue;

            for (int i = 0; i < values.Length - 1; i++)
            {
                float difference = values[i + 1] - values[i];
                if (difference < 0)
                    difference *= (-1);

                if (difference < smallest)
                    smallest = difference;
            }

            return smallest;
        }

        protected void ReadDAE_LibraryAnimations(XmlReader reader)
        {
            reader.MoveToContent();
            while (reader.Read())
            {
                if (reader.NodeType.Equals(XmlNodeType.Element))
                {
                    if (reader.LocalName.Equals("animation"))
                    {
                        string id = reader.GetAttribute("id");
                        AnimationForImport anim = new AnimationForImport(id);

                        ReadDAE_Animation(reader, anim);
                    }
                }
                else if (reader.NodeType.Equals(XmlNodeType.EndElement))
                {
                    if (reader.LocalName.Equals("library_animations"))
                    {
                        break;
                    }
                }
            }
        }

        protected void ReadDAE_Animation(XmlReader reader, AnimationForImport anim)
        {
            string boneID = null;

            reader.MoveToContent();
            while (reader.Read())
            {
                if (reader.NodeType.Equals(XmlNodeType.Element))
                {
                    if (reader.LocalName.Equals("source"))
                    {
                        SourceForImport source = ReadDAE_Source(reader);
                        anim.m_Sources.Add(source.m_ID, source);
                    }
                    else if (reader.LocalName.Equals("sampler"))
                    {
                        AnimationSamplerChannelPairForImport sampler = ReadDAE_Animation_Sampler(reader);
                        anim.m_SamplerChannelPairs.Add(sampler.m_ID, sampler);
                    }
                    else if (reader.LocalName.Equals("channel"))
                    {
                        boneID = ReadDAE_Animation_Channel(reader, anim.m_SamplerChannelPairs);
                    }
                }
                else if (reader.NodeType.Equals(XmlNodeType.EndElement))
                {
                    if (reader.LocalName.Equals("animation"))
                    {
                        if (boneID != null)
                        {
                            if (!m_Animations.ContainsKey(boneID))
                                m_Animations.Add(boneID, new Dictionary<string, AnimationForImport>());

                            m_Animations[boneID].Add(anim.m_ID, anim);
                        }
                        break;
                    }
                }
            }
        }

        protected SourceForImport ReadDAE_Source(XmlReader reader)
        {
            string id = reader.GetAttribute("id");
            SourceForImport src = new SourceForImport(id);

            reader.MoveToContent();
            while (reader.Read())
            {
                if (reader.NodeType.Equals(XmlNodeType.Element))
                {
                    if (reader.LocalName.Equals("float_array"))
                    {
                        string[] tmpStringArr = reader.ReadString().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        src.m_FloatArray = new float[tmpStringArr.Length];
                        for (int i = 0; i < tmpStringArr.Length; i++)
                        {
                            src.m_FloatArray[i] = float.Parse(tmpStringArr[i], USA);
                        }
                    }
                    else if (reader.LocalName.Equals("Name_array"))
                    {
                        src.m_NameArray = reader.ReadString().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    }
                    else if (reader.LocalName.Equals("IDREF_array"))
                    {
                        src.m_IDREFArray = reader.ReadString().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    }
                    else if (reader.LocalName.Equals("accessor"))
                    {
                        src.m_Count = int.Parse(reader.GetAttribute("count"));
                        string strideAttr = reader.GetAttribute("stride");
                        src.m_Stride = (strideAttr == null) ? 1 : int.Parse(strideAttr);
                        string offset = reader.GetAttribute("offset");
                        src.m_StartIndexOffset = (offset == null) ? 0 : int.Parse(offset);
                    }
                    else if (reader.LocalName.Equals("param"))
                    {
                        string type = reader.GetAttribute("type");
                        if (type == null)
                            type = "float";
                        src.m_ElementType = type;
                    }
                }
                else if (reader.NodeType.Equals(XmlNodeType.EndElement))
                {
                    if (reader.LocalName.Equals("source"))
                    {
                        break;
                    }
                }
            }

            return src;
        }

        protected AnimationSamplerChannelPairForImport ReadDAE_Animation_Sampler(XmlReader reader)
        {
            string id = reader.GetAttribute("id");
            AnimationSamplerChannelPairForImport sampler = new AnimationSamplerChannelPairForImport(id);

            reader.MoveToContent();
            while (reader.Read())
            {
                if (reader.NodeType.Equals(XmlNodeType.Element))
                {
                    if (reader.LocalName.Equals("input"))
                    {
                        string semantic = reader.GetAttribute("semantic");
                        string source = reader.GetAttribute("source").Replace("#", "");

                        switch (semantic.ToUpperInvariant())
                        {
                            case "INPUT":
                                sampler.m_InputSourceID = source;
                                break;
                            case "OUTPUT":
                                sampler.m_OutputSourceID = source;
                                break;
                            case "INTERPOLATION":
                                sampler.m_InterpolationSourceID = source;
                                break;
                        }
                    }
                }
                else if (reader.NodeType.Equals(XmlNodeType.EndElement))
                {
                    if (reader.LocalName.Equals("sampler"))
                    {
                        break;
                    }
                }
            }

            return sampler;
        }

        protected string ReadDAE_Animation_Channel(XmlReader reader, Dictionary<string, AnimationSamplerChannelPairForImport> samplers)
        {
            reader.MoveToContent();

            string samplerID = reader.GetAttribute("source").Replace("#", "");
            string target = reader.GetAttribute("target");

            string boneID = target.Substring(0, target.IndexOf("/"));
            string transformationSID = target.Substring(target.IndexOf("/") + 1);
            string targetParam = (transformationSID.IndexOf('.') != -1) ? transformationSID.Substring(transformationSID.IndexOf('.') + 1) :
                null;
            transformationSID = (targetParam != null) ? transformationSID.Replace("." + targetParam, "") : transformationSID;

            if (!m_Bones.ContainsKey(boneID) || !m_DAEBoneTransformsList[boneID].ContainsKey(transformationSID))
                return null;// We don't want this animation included as it does not apply to a joint or a valid joint transformation

            if (targetParam == null)
            {
                samplers[samplerID].m_Type = m_DAEBoneTransformsList[boneID][transformationSID];
            }
            else
            {
                // Eg. joint has <translate sid="translation>1 2 3</translate> and animation has 
                // ...target="JOINTNAME/translation.X"/>
                if (m_DAEBoneTransformsList[boneID][transformationSID] == TransformationType.TranslationXYZ)
                {
                    switch (target.ToUpperInvariant())
                    {
                        case "X":
                            samplers[samplerID].m_Type = TransformationType.TranslationX;
                            break;
                        case "Y":
                            samplers[samplerID].m_Type = TransformationType.TranslationY;
                            break;
                        case "Z":
                            samplers[samplerID].m_Type = TransformationType.TranslationZ;
                            break;
                    }
                }
                else if (m_DAEBoneTransformsList[boneID][transformationSID] == TransformationType.ScaleXYZ)
                {
                    switch (target.ToUpperInvariant())
                    {
                        case "X":
                            samplers[samplerID].m_Type = TransformationType.ScaleX;
                            break;
                        case "Y":
                            samplers[samplerID].m_Type = TransformationType.ScaleY;
                            break;
                        case "Z":
                            samplers[samplerID].m_Type = TransformationType.ScaleZ;
                            break;
                    }
                }
                else
                {
                    samplers[samplerID].m_Type = m_DAEBoneTransformsList[boneID][transformationSID];
                }
            }

            return boneID;
        }

        /* 
         * Creates a list of bones based on joints defined as skeletons or, if none are found, the <geometry> 
         * elements are used to create bones with default values.
         */
        protected bool DAE_LoadBones(String modelFileName)
        {
            m_DAEGeometryToRootBoneMap = new BiDictionaryOneToOne<string, string>();
            m_DAEControllerToGeometryNameMap = new Dictionary<string, string>();
            m_DAEBoneIDToBoneSIDMap = new BiDictionaryOneToOne<string, string>();

            using (XmlReader reader = XmlReader.Create(modelFileName))
            {
                reader.MoveToContent();

                while (reader.Read())
                {
                    if (reader.NodeType.Equals(XmlNodeType.Element))
                    {
                        if (reader.LocalName.Equals("library_controllers"))
                        {
                            m_DAEControllerToGeometryNameMap = ReadDAECreateControllerIDToGeometryIDMap(reader);
                            break;
                        }
                    }
                }
            }

            using (XmlReader reader = XmlReader.Create(modelFileName))
            {
                reader.MoveToContent();

                while (reader.Read())
                {
                    if (reader.NodeType.Equals(XmlNodeType.Element))
                    {
                        if (reader.LocalName.Equals("library_visual_scenes"))
                        {
                            ReadDAE_LibraryVisualScenes(reader, m_DAEControllerToGeometryNameMap);
                        }
                    }
                }
            }

            return m_DAEGeometryToRootBoneMap.Count > 0;
        }

        public static Dictionary<string, string> ReadDAECreateControllerIDToGeometryIDMap(XmlReader reader)
        {
            Dictionary<string, string> controllerToGeometryNameMap = new Dictionary<string, string>();

            string currentController = "";
            string currentSkinSource = "";

            reader.MoveToContent();
            while (reader.Read())
            {
                if (reader.NodeType.Equals(XmlNodeType.Element))
                {
                    if (reader.LocalName.Equals("controller"))
                    {
                        currentController = reader.GetAttribute("id");
                    }
                    if (reader.LocalName.Equals("skin"))
                    {
                        currentSkinSource = reader.GetAttribute("source").Replace("#", "");
                    }
                }
                else if (reader.NodeType.Equals(XmlNodeType.EndElement))
                {
                    if (reader.LocalName.Equals("controller"))
                        controllerToGeometryNameMap.Add(currentController, currentSkinSource);
                    else if (reader.LocalName.Equals("library_controllers"))
                        break;
                }
            }

            return controllerToGeometryNameMap;
        }

        protected void ReadDAE_LibraryVisualScenes(XmlReader reader, Dictionary<string, string> controllerToGeometryNameMap)
        {
            m_DAEBoneTransformsList = new Dictionary<string, Dictionary<string, TransformationType>>();
            m_DAEGeometryInstanceMaterialNameMap = new Dictionary<string, Dictionary<string, string>>();

            reader.MoveToContent();
            while (reader.Read())
            {
                if (reader.NodeType.Equals(XmlNodeType.Element))
                {
                    if (reader.LocalName.Equals("node"))
                    {
                        string id = reader.GetAttribute("id");
                        string type = reader.GetAttribute("type");
                        if (type == null)
                            type = "NODE";

                        if (type.ToUpperInvariant().Equals("NODE"))
                            ReadDAE_Node_NODE(reader, controllerToGeometryNameMap);
                        else if (type.ToUpperInvariant().Equals("JOINT"))
                            ReadDAE_Node_JOINT(reader, 0, true, controllerToGeometryNameMap);
                    }
                }
                else if (reader.NodeType.Equals(XmlNodeType.EndElement))
                {
                    if (reader.LocalName.Equals("library_visual_scenes"))
                        break;
                }
            }
        }

        protected void ReadDAE_Node_NODE(XmlReader reader, Dictionary<string, string> controllerToGeometryNameMap, int childJointParentID = 0)
        {
            string mesh_name = "";

            reader.MoveToContent();
            while (reader.Read())
            {
                if (reader.NodeType.Equals(XmlNodeType.Element))
                {
                    if (reader.LocalName.Equals("instance_geometry"))
                    {
                        // Create a default bone with the name of the <geometry> element
                        mesh_name = reader.GetAttribute("url").Replace("#", "");
                        m_Bones.Add(mesh_name, new BoneForImport(mesh_name, 0, mesh_name, mesh_name, 0, false));
                        short parent_offset = 0;
                        m_Bones[mesh_name].m_RootBone = mesh_name;
                        m_Bones[mesh_name].m_ParentBone = mesh_name;
                        m_Bones[mesh_name].m_ParentOffset = parent_offset;
                        m_DAEGeometryToRootBoneMap.Add(mesh_name, mesh_name);
                    }
                    else if (reader.LocalName.Equals("instance_controller"))
                    {
                        // Read the <skeleton> tag to get the name of the root joint and use the <controller> ID to 
                        // <geometry> ID map to get the name of the <geometry>
                        mesh_name = controllerToGeometryNameMap[reader.GetAttribute("url").Replace("#", "")];
                        reader.ReadToFollowing("skeleton");
                        string rootBoneName = reader.ReadString().Replace("#", "");
                        m_DAEGeometryToRootBoneMap.Add(mesh_name, rootBoneName);
                    }
                    else if (reader.LocalName.Equals("instance_material"))
                    {
                        string symbol = reader.GetAttribute("symbol");
                        string target = reader.GetAttribute("target").Replace("#", "");
                        if (!m_DAEGeometryInstanceMaterialNameMap.ContainsKey(mesh_name))
                            m_DAEGeometryInstanceMaterialNameMap.Add(mesh_name, new Dictionary<string, string>());
                        m_DAEGeometryInstanceMaterialNameMap[mesh_name].Add(symbol, target);
                    }
                    else if (reader.LocalName.Equals("node"))
                    {
                        string id = reader.GetAttribute("id");
                        string type = reader.GetAttribute("type");
                        if (type == null)
                            type = "NODE";

                        if (type.ToUpperInvariant().Equals("NODE"))
                            ReadDAE_Node_NODE(reader, controllerToGeometryNameMap);
                        else if (type.ToUpperInvariant().Equals("JOINT"))
                            ReadDAE_Node_JOINT(reader, childJointParentID, (childJointParentID == 0), controllerToGeometryNameMap);
                    }
                }
                else if (reader.NodeType.Equals(XmlNodeType.EndElement))
                {
                    if (reader.LocalName.Equals("node"))
                        break;
                }
            }
        }

        public enum TransformationType
        {
            TranslationXYZ,
            TranslationX,
            TranslationY,
            TranslationZ,
            RotationX,
            RotationY,
            RotationZ,
            ScaleXYZ,
            ScaleX,
            ScaleY,
            ScaleZ,
            TransformationMatrix,
            None
        };

        protected void ReadDAE_Node_JOINT(XmlReader reader, int parentID, bool root, 
            Dictionary<string, string> controllerToGeometryNameMap)
        {
            Vector3 scale = new Vector3(1f, 1f, 1f);
            Vector3 rotation = new Vector3(0f, 0f, 0f);
            Vector3 translation = new Vector3(0f, 0f, 0f);

            string id = reader.GetAttribute("id");
            string name = reader.GetAttribute("name");
            string boneSid = reader.GetAttribute("sid");
            if (boneSid == null) boneSid = id;
            m_DAEBoneIDToBoneSIDMap.Add(id, boneSid);
            bool hasChildren = false;

            // Add new bone for current node with basic information, not all can be added until finished reading in node
            int currentIndex = m_Bones.Values.Count;
            short parentOffset = (short)((-1) * (currentIndex - parentID));
            string parentBoneName = (root) ? id : m_Bones.Keys.ElementAt(currentIndex + parentOffset);
            string rootBoneName = (root) ? id : m_Bones.Values.Last(bone0 => bone0.m_ParentOffset == 0).m_Name;
            int rootBoneID = (root) ? currentIndex : GetBoneIndex(rootBoneName);

            List<BoneForImport> bonesInBranch = new List<BoneForImport>();
            for (int i = rootBoneID; i < m_Bones.Values.Count; i++)
                bonesInBranch.Add(m_Bones.Values.ElementAt(i));

            // sibling offset, unlike parent offset should not be negative, bones should only point forward to their next sibling
            IEnumerable<BoneForImport> siblings = bonesInBranch.Where(bone0 => bone0.m_ParentOffset != 0 &&
                bone0.m_ParentBone.Equals(parentBoneName));
            if (siblings.Count() > 0)
            {
                siblings.ElementAt(siblings.Count() - 1).m_SiblingOffset =
                    (short)(currentIndex - GetBoneIndex(siblings.ElementAt(siblings.Count() - 1).m_Name));
            }

            BoneForImport node = new BoneForImport(id, parentOffset, parentBoneName,
                rootBoneName, 0, false);

            m_Bones.Add(id, node);
            // end block

            m_DAEBoneTransformsList.Add(id, new Dictionary<string, TransformationType>());
            List<Tuple<Vector3, TransformationType>> transforms = new List<Tuple<Vector3, TransformationType>>();

            reader.MoveToContent();
            while (reader.Read())
            {
                if (reader.NodeType.Equals(XmlNodeType.Element))
                {
                    if (reader.LocalName.Equals("matrix"))
                    {
                        string sid = reader.GetAttribute("sid");
                        string[] vals = reader.ReadString().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        Matrix4 matrix = Helper.StringArrayToMatrix4(vals);
                        Helper.DecomposeSRTMatrix1(matrix, out scale, out rotation, out translation);

                        m_DAEBoneTransformsList[id].Add(sid, TransformationType.TransformationMatrix);
                    }
                    else if (reader.LocalName.Equals("translate"))
                    {
                        string sid = reader.GetAttribute("sid");
                        string[] vals = reader.ReadString().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        Vector3 tmp = new Vector3(float.Parse(vals[0], USA), float.Parse(vals[1], USA), float.Parse(vals[2], USA));

                        transforms.Add(new Tuple<Vector3, TransformationType>(tmp, TransformationType.TranslationXYZ));
                        m_DAEBoneTransformsList[id].Add(sid, TransformationType.TranslationXYZ);
                    }
                    else if (reader.LocalName.Equals("rotate"))
                    {
                        /*
                         * <rotate sid="rotateZ">0 0 1 0</rotate>
                         * <rotate sid="rotateY">0 1 0 45</rotate>
                         * <rotate sid="rotateX">1 0 0 0</rotate>
                         */
                        string sid = reader.GetAttribute("sid");
                        string[] vals = reader.ReadString().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        int ind = -1;
                        for (int i = 0; i < 3; i++)
                        {
                            if (float.Parse(vals[i]) == 1.0f)
                            {
                                ind = i;
                                break;
                            }
                        }
                        switch (ind)
                        {
                            case 0:
                                transforms.Add(new Tuple<Vector3, TransformationType>(new Vector3(float.Parse(vals[3]) * Helper.Deg2Rad, 0.0f, 0.0f),
                                    TransformationType.RotationX));
                                m_DAEBoneTransformsList[id].Add(sid, TransformationType.RotationX);
                                break;
                            case 1:
                                transforms.Add(new Tuple<Vector3, TransformationType>(new Vector3(0.0f, float.Parse(vals[3]) * Helper.Deg2Rad, 0.0f),
                                    TransformationType.RotationY));
                                m_DAEBoneTransformsList[id].Add(sid, TransformationType.RotationY);
                                break;
                            case 2:
                                transforms.Add(new Tuple<Vector3, TransformationType>(new Vector3(0.0f, 0.0f, float.Parse(vals[3]) * Helper.Deg2Rad),
                                    TransformationType.RotationZ));
                                m_DAEBoneTransformsList[id].Add(sid, TransformationType.RotationZ);
                                break;
                        }
                    }
                    else if (reader.LocalName.Equals("scale"))
                    {
                        string sid = reader.GetAttribute("sid");
                        string[] vals = reader.ReadString().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        Vector3 tmp = new Vector3(float.Parse(vals[0], USA), float.Parse(vals[1], USA), float.Parse(vals[2], USA));

                        transforms.Add(new Tuple<Vector3, TransformationType>(tmp, TransformationType.ScaleXYZ));
                        m_DAEBoneTransformsList[id].Add(sid, TransformationType.ScaleXYZ);
                    }
                    else if (reader.LocalName.Equals("node"))
                    {
                        string type = reader.GetAttribute("type");
                        if (type.ToUpperInvariant().Equals("JOINT"))
                        {
                            hasChildren = true;
                            ReadDAE_Node_JOINT(reader, currentIndex, false, controllerToGeometryNameMap);
                        }
                        else if (type == null || type.ToUpperInvariant().Equals("NODE"))
                        {
                            hasChildren = true;// Probably
                            ReadDAE_Node_NODE(reader, controllerToGeometryNameMap, currentIndex);
                        }
                    }
                }
                else if (reader.NodeType.Equals(XmlNodeType.EndElement))
                {
                    if (reader.LocalName.Equals("node"))
                    {
                        // If matrix not used, multiply each of the transformations in the reverse of the order they appear.
                        // IMPORTANT NOTE: The order must be Scale, Rotation, Translation (appear in file TRzRyRxS)
                        if (transforms.Count > 0)
                        {
                            if (transforms.Count == 5 && transforms[0].Item2.Equals(TransformationType.TranslationXYZ) &&
                                transforms[1].Item2.Equals(TransformationType.RotationZ) &&
                                transforms[2].Item2.Equals(TransformationType.RotationY) &&
                                transforms[3].Item2.Equals(TransformationType.RotationX) &&
                                transforms[4].Item2.Equals(TransformationType.ScaleXYZ))
                            {
                                /* If transformations appear as follows:
                                 * <translate sid="translate">0.1 1.2 0.55</translate>
					             * <rotate sid="rotationZ">0 0 1 0</rotate>
					             * <rotate sid="rotationY">0 1 0 -180</rotate>
					             * <rotate sid="rotationX">1 0 0 0</rotate>
					             * <scale sid="scale">1 1 1</scale>
                                 * then we can just extract the values directly
                                 */
                                scale = transforms[4].Item1;
                                rotation.X = transforms[3].Item1.X;
                                rotation.Y = transforms[2].Item1.Y;
                                rotation.Z = transforms[1].Item1.Z;
                                translation = transforms[0].Item1;
                            }
                            else if (transforms.Count < 5)
                            {
                                // Making an assumption that the order is SRT if less than 5 tranformations, if it's greater than 
                                // 5 or there are 5 but not in the order in above condition, use matrix decomposition method.
                                if (transforms.Where(tfm => tfm.Item2 == TransformationType.TranslationXYZ).Count() == 1 && 
                                    transforms.ElementAt(transforms.Count - 1).Item2 == TransformationType.TranslationXYZ)
                                {
                                    translation = transforms.Where(tfm => tfm.Item2 == TransformationType.TranslationXYZ).ElementAt(0).Item1;
                                }
                                if (transforms.Where(tfm => tfm.Item2 == TransformationType.RotationX).Count() == 1)
                                {
                                    rotation.X = transforms.Where(tfm => tfm.Item2 == TransformationType.RotationX).ElementAt(0).Item1.X;
                                }
                                if (transforms.Where(tfm => tfm.Item2 == TransformationType.RotationY).Count() == 1)
                                {
                                    rotation.Y = transforms.Where(tfm => tfm.Item2 == TransformationType.RotationY).ElementAt(0).Item1.Y;
                                }
                                if (transforms.Where(tfm => tfm.Item2 == TransformationType.RotationZ).Count() == 1)
                                {
                                    rotation.Z = transforms.Where(tfm => tfm.Item2 == TransformationType.RotationZ).ElementAt(0).Item1.Z;
                                }
                                if (transforms.Where(tfm => tfm.Item2 == TransformationType.ScaleXYZ).Count() == 1 &&
                                    transforms.ElementAt(0).Item2 == TransformationType.ScaleXYZ)
                                {
                                    scale = transforms.Where(tfm => tfm.Item2 == TransformationType.ScaleXYZ).ElementAt(0).Item1;
                                }
                            }
                            else
                            {
                                // If not we need to multiply the matrices and then decompose the final matrix
                                Matrix4 result = Matrix4.Identity;
                                for (int i = transforms.Count - 1; i >= 0; i--)
                                {
                                    Tuple<Vector3, TransformationType> current = transforms.ElementAt(i);
                                    Vector3 vec = current.Item1;
                                    switch (current.Item2)
                                    {
                                        case TransformationType.ScaleXYZ:
                                            Matrix4 mscale = Matrix4.CreateScale(vec);
                                            Matrix4.Mult(ref result, ref mscale, out result);
                                            break;
                                        case TransformationType.RotationX:
                                            Matrix4 mxrot = Matrix4.CreateRotationX(vec.X);
                                            Matrix4.Mult(ref result, ref mxrot, out result);
                                            break;
                                        case TransformationType.RotationY:
                                            Matrix4 myrot = Matrix4.CreateRotationY(vec.Y);
                                            Matrix4.Mult(ref result, ref myrot, out result);
                                            break;
                                        case TransformationType.RotationZ:
                                            Matrix4 mzrot = Matrix4.CreateRotationZ(vec.Z);
                                            Matrix4.Mult(ref result, ref mzrot, out result);
                                            break;
                                        case TransformationType.TranslationXYZ:
                                            Matrix4 mtrans = Matrix4.CreateTranslation(vec);
                                            Matrix4.Mult(ref result, ref mtrans, out result);
                                            break;
                                    }
                                }

                                Helper.DecomposeSRTMatrix1(result, out scale, out rotation, out translation);
                            }
                        }

                        node = m_Bones[id];
                        node.m_HasChildren = hasChildren;
                        node.m_Scale = new uint[] { (uint)(scale.X * 4096.0f), 
                            (uint)(scale.Y * 4096.0f), (uint)(scale.Z * 4096.0f) };
                        node.m_Rotation = new ushort[] { (ushort)((rotation.X * 2048.0f) / Math.PI), 
                            (ushort)((rotation.Y * 2048.0f) / Math.PI), (ushort)((rotation.Z * 2048.0f) / Math.PI) };
                        node.m_Translation = new uint[] { (uint)(translation.X * 4096.0f), 
                            (uint)(translation.Y * 4096.0f), (uint)(translation.Z * 4096.0f) };

                        break;
                    }
                }
            }
        }

        protected void ReadDAE_LibraryControllers(XmlReader reader)
        {
            reader.MoveToContent();

            while (reader.Read())
            {
                if (reader.NodeType.Equals(XmlNodeType.Element))
                {
                    if (reader.LocalName.Equals("controller"))
                    {
                        ReadDAE_Controller(reader);
                    }
                }
                else if (reader.NodeType.Equals(XmlNodeType.EndElement))
                {
                    if (reader.LocalName.Equals("library_controllers"))
                    {
                        break;
                    }
                }
            }
        }

        protected void ReadDAE_Controller(XmlReader reader)
        {
            reader.MoveToContent();

            while (reader.Read())
            {
                if (reader.NodeType.Equals(XmlNodeType.Element))
                {
                    if (reader.LocalName.Equals("skin"))
                    {
                        ReadDAE_Controller_Skin(reader);
                    }
                }
                else if (reader.NodeType.Equals(XmlNodeType.EndElement))
                {
                    if (reader.LocalName.Equals("controller"))
                    {
                        break;
                    }
                }
            }
        }

        protected void ReadDAE_Controller_Skin(XmlReader reader)
        {
            string geometryName = reader.GetAttribute("source").Replace("#", "");
            string[] jointNames = new string[0];
            Matrix4[] invBindPoses = new Matrix4[0];

            Dictionary<string, SourceForImport> sources = new Dictionary<string, SourceForImport>();

            reader.MoveToContent();
            while (reader.Read())
            {
                if (reader.NodeType.Equals(XmlNodeType.Element))
                {
                    if (reader.LocalName.Equals("source"))
                    {
                        string idAttr = reader.GetAttribute("id");
                        SourceForImport source = ReadDAE_Source(reader);
                        sources.Add(idAttr, source);
                    }
                    else if (reader.LocalName.Equals("joints"))
                    {
                        // Assuming:
                        // <input semantic="JOINT" source="#[Joint names source ID]" />
                        // <input semantic="INV_BIND_MATRIX" source="#[Inverse bind poses source ID]" />
                        for (int i = 0; i < 2; i++)
                        {
                            reader.ReadToFollowing("input");
                            string semantic = reader.GetAttribute("semantic");
                            string sourceAttr = reader.GetAttribute("source").Replace("#", "");
                            if (semantic.ToUpperInvariant().Equals("JOINT"))
                            {
                                SourceForImport joint = sources[sourceAttr];

                                jointNames = new string[joint.m_Count];
                                int indexJoint = 0;//GetDictionaryStringKeyIndex(paramsList.Keys.ToList<Object>(), "JOINT");
                                int tmpCount = 0;
                                for (int j = 0; j < (joint.m_Count * joint.m_Stride); j += joint.m_Stride)
                                {
                                    jointNames[tmpCount] = (joint.m_NameArray != null) ? 
                                        m_DAEBoneIDToBoneSIDMap.GetBySecond(joint.m_NameArray[j + indexJoint]) :
                                        joint.m_IDREFArray[j + indexJoint];
                                    tmpCount++;
                                }
                            }
                            else if (semantic.ToUpperInvariant().Equals("INV_BIND_MATRIX"))
                            {
                                SourceForImport invBindMatrix = sources[sourceAttr];

                                invBindPoses = new Matrix4[invBindMatrix.m_Count];
                                int indexTransform = 0;
                                int tmpCount = 0;
                                for (int j = 0; j < (invBindMatrix.m_Count * invBindMatrix.m_Stride); j += invBindMatrix.m_Stride)
                                {
                                    int ind = indexTransform + j;
                                    Vector4 row0 = new Vector4(invBindMatrix.m_FloatArray[ind + 0], invBindMatrix.m_FloatArray[ind + 4],
                                        invBindMatrix.m_FloatArray[ind + 8], invBindMatrix.m_FloatArray[ind + 12]);
                                    Vector4 row1 = new Vector4(invBindMatrix.m_FloatArray[ind + 1], invBindMatrix.m_FloatArray[ind + 5],
                                        invBindMatrix.m_FloatArray[ind + 9], invBindMatrix.m_FloatArray[ind + 13]);
                                    Vector4 row2 = new Vector4(invBindMatrix.m_FloatArray[ind + 2], invBindMatrix.m_FloatArray[ind + 6],
                                        invBindMatrix.m_FloatArray[ind + 10], invBindMatrix.m_FloatArray[ind + 14]);
                                    Vector4 row3 = new Vector4(invBindMatrix.m_FloatArray[ind + 3], invBindMatrix.m_FloatArray[ind + 7],
                                        invBindMatrix.m_FloatArray[ind + 11], invBindMatrix.m_FloatArray[ind + 15]);
                                    Matrix4 matrix = new Matrix4(row0, row1, row2, row3);

                                    invBindPoses[tmpCount] = matrix;
                                    tmpCount++;
                                }
                            }
                        }
                    }
                    else if (reader.LocalName.Equals("vertex_weights"))
                    {
                        ReadDAE_Controller_Skin_vertexWeights(reader, geometryName, jointNames, invBindPoses, sources);
                    }
                }
                else if (reader.NodeType.Equals(XmlNodeType.EndElement))
                {
                    if (reader.LocalName.Equals("skin"))
                    {
                        break;
                    }
                }
            }
        }

        /* 
         * BMD does not support vertex weights, instead it allows you to assign each vertex to one bone ("joint") only. This
         * method reads all the influences (joint/weight pairs) and uses the hightest weighting to determine the bone to which 
         * the vertex should be assigned.
         */
        protected void ReadDAE_Controller_Skin_vertexWeights(XmlReader reader, string geometryName, string[] jointNames,
            Matrix4[] invBindPoses, Dictionary<string, SourceForImport> sources)
        {
            float[] weights = new float[0];
            int offsetJoint = -1;
            int offsetWeight = -1;
            int[] vcount = new int[0];
            int geometryVertexOffset =
                geometryVertexOffsets[GetDictionaryStringKeyIndex(m_DAEGeometryPositionsListNames.Keys.ToList<Object>(), geometryName)];

            reader.MoveToContent();
            while (reader.Read())
            {
                if (reader.NodeType.Equals(XmlNodeType.Element))
                {
                    if (reader.LocalName.Equals("input"))
                    {
                        string semantic = reader.GetAttribute("semantic");
                        int offset = int.Parse(reader.GetAttribute("offset"));
                        if (semantic.Equals("JOINT"))
                        {
                            offsetJoint = offset;
                        }
                        else if (semantic.Equals("WEIGHT"))
                        {
                            offsetWeight = offset;

                            string sourceAttr = reader.GetAttribute("source").Replace("#", "");
                            SourceForImport weight = sources[sourceAttr];
                            weights = new float[weight.m_Count];
                            int indexWeight = 0;// GetDictionaryStringKeyIndex(paramsList.Keys.ToList<Object>(), "WEIGHT");
                            int tmpCount = 0;
                            for (int i = 0; i < (weight.m_Count * weight.m_Stride); i += weight.m_Stride)
                            {
                                weights[tmpCount] = weight.m_FloatArray[i + indexWeight];
                                tmpCount++;
                            }
                        }
                    }
                    else if (reader.LocalName.Equals("vcount"))
                    {
                        string[] tmpArr = reader.ReadString().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        vcount = new int[tmpArr.Length];
                        for (int i = 0; i < tmpArr.Length; i++)
                        {
                            vcount[i] = int.Parse(tmpArr[i]);
                        }
                    }
                    else if (reader.LocalName.Equals("v"))
                    {
                        string[] tmpArr = reader.ReadString().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        int currentVertexIndex = 0;
                        for (int i = 0; i < tmpArr.Length; )
                        {
                            int numInfluences = vcount[currentVertexIndex];
                            List<Tuple<int, float>> influences = new List<Tuple<int, float>>();

                            for (int j = 0; j < numInfluences; j++)
                            {
                                int indexJoint = int.Parse(tmpArr[i + (j * 2) + offsetJoint]);
                                int indexWeight = int.Parse(tmpArr[i + (j * 2) + offsetWeight]);

                                int mappedBoneID = GetDictionaryStringKeyIndex(m_Bones.Keys.ToList<Object>(), jointNames[indexJoint]);
                                float weight = weights[indexWeight];

                                influences.Add(new Tuple<int, float>(mappedBoneID, weight));
                            }

                            int highestWeightJoint = 0;
                            float highestWeightValue = 0.0f;
                            for (int j = 0; j < numInfluences; j++)
                            {
                                if (influences[j].Item2 > highestWeightValue)
                                {
                                    highestWeightValue = influences[j].Item2;
                                    highestWeightJoint = influences[j].Item1;
                                }
                            }

                            m_VertexBoneIDs[geometryVertexOffset + currentVertexIndex] = highestWeightJoint;

                            i += (vcount[currentVertexIndex] * 2);
                            currentVertexIndex++;
                        }
                    }
                }
                else if (reader.NodeType.Equals(XmlNodeType.EndElement))
                {
                    if (reader.LocalName.Equals("vertex_weights"))
                    {
                        BoneForImport currentBone = m_Bones[m_DAEGeometryToRootBoneMap.GetByFirst(geometryName)];
                        for (int i = 0; i < currentBone.m_Materials.Count; i++)
                        {
                            for (int j = 0; j < currentBone.m_Materials.Values.ElementAt(i).m_Faces.Count; j++)
                            {
                                for (int k = 0; k < currentBone.m_Materials.Values.ElementAt(i).m_Faces[j].m_VtxIndices.Count(); k++)
                                {
                                    int vtxInd = currentBone.m_Materials.Values.ElementAt(i).m_Faces[j].m_VtxIndices[k];
                                    int boneID = m_VertexBoneIDs[vtxInd];
                                    currentBone.m_Materials.Values.ElementAt(i).m_Faces[j].m_BoneIDs[k] = boneID;
                                }
                            }
                        }

                        break;
                    }
                }
            }
        }

        protected Dictionary<string, string> ReadDAE_Accessor_Params(XmlReader reader)
        {
            Dictionary<string, string> paramsList = new Dictionary<string, string>();

            reader.MoveToContent();
            while (reader.Read())
            {
                if (reader.NodeType.Equals(XmlNodeType.Element))
                {
                    if (reader.LocalName.Equals("param"))
                    {
                        string name = reader.GetAttribute("name");
                        string type = reader.GetAttribute("type");

                        paramsList.Add(name, type);
                    }
                }
                else if (reader.NodeType.Equals(XmlNodeType.EndElement))
                {
                    if (reader.LocalName.Equals("accessor"))
                    {
                        break;
                    }
                }
            }

            return paramsList;
        }

        protected Dictionary<string, string> ReadDAEGeometryPositionsListNames(string modelFileName)
        {
            Dictionary<string, string> geometryPositionsListNames = new Dictionary<string, string>();

            using (XmlReader reader = XmlReader.Create(modelFileName))
            {
                reader.MoveToContent();

                while (reader.Read())
                {
                    reader.ReadToFollowing("geometry");
                    string currentGeometry = reader.GetAttribute("id");

                    if (!reader.NodeType.Equals(XmlNodeType.Element))
                        continue;

                    reader.ReadToFollowing("vertices");
                    reader.ReadToFollowing("input");
                    string semantic = reader.GetAttribute("semantic");
                    while (semantic.ToLowerInvariant() != "position")
                    {
                        reader.ReadToFollowing("input");
                        semantic = reader.GetAttribute("semantic");
                    }
                    string positionsName = reader.GetAttribute("source").Replace("#", "");
                    geometryPositionsListNames.Add(currentGeometry, positionsName);
                }
            }

            return geometryPositionsListNames;
        }

        protected void ReadDAE_Geometry(XmlReader reader)
        {
            currentBone = m_DAEGeometryToRootBoneMap.GetByFirst(reader.GetAttribute("id"));
            currentBoneID = GetBoneIndex(currentBone);
            currentVertices = new List<float>();
            currentTexCoords = new List<float>();
            currentNormals = new List<float>();
            currentColours = new List<float>();
            vertexIndex = -1;
            normalIndex = -1;
            texCoordIndex = -1;
            colourIndex = -1;

            // Number of colour components - should be 3: R, G and B and their offsets - should be 0, 1, 2 
            // This is because 3D editors use non-standard variations such 3DS Max which exports RGBA
            vtxColourStrideAndRGBOffsets = new int[] { 3, 0, 1, 2 };

            reader.MoveToContent();
            while (reader.Read())
            {
                if (reader.NodeType.Equals(XmlNodeType.Element))
                {
                    if (reader.LocalName.Equals("source"))
                    {
                        ReadDAE_Geometry_Source(reader);
                    }
                    else if (reader.LocalName.Equals("polylist"))
                    {
                        ReadDAE_PolyList(reader);
                    }
                    else if (reader.LocalName.Equals("triangles"))
                    {
                        ReadDAE_PolyList(reader);
                    }
                    else if (reader.LocalName.Equals("polygons"))
                    {
                        ReadDAE_PolyList(reader, true);
                    }
                }
                else if (reader.NodeType.Equals(XmlNodeType.EndElement) && reader.LocalName.Equals("geometry"))
                {
                    // Build vertices, normals, texture co-ordinates and colours from the 
                    // component values read earlier
                    geometryVertexOffsets[currentBoneID] = m_Vertices.Count;

                    for (int i = 0; i < currentVertices.Count; i += 3)
                    {
                        m_Vertices.Add(new Vector4(currentVertices[i], currentVertices[i + 1], currentVertices[i + 2], 1f));
                        m_VertexBoneIDs.Add(currentBoneID);
                    }
                    for (int i = 0; i < currentNormals.Count; i += 3)
                    {
                        Vector3 vec = new Vector3(currentNormals[i], currentNormals[i + 1], currentNormals[i + 2]);
                        vec.Normalize();
                        m_Normals.Add(vec);
                    }
                    for (int i = 0; i < currentTexCoords.Count; i += 2)
                    {
                        m_TexCoords.Add(new Vector2(currentTexCoords[i], currentTexCoords[i + 1]));
                    }
                    for (int i = 0; i < currentColours.Count; i += vtxColourStrideAndRGBOffsets[0])
                    {
                        Color vtxColour = Color.FromArgb((int)(currentColours[i + vtxColourStrideAndRGBOffsets[1]] * 255f),
                            (int)(currentColours[i + vtxColourStrideAndRGBOffsets[2]] * 255f),
                            (int)(currentColours[i + vtxColourStrideAndRGBOffsets[3]] * 255f));
                        m_Colours.Add(vtxColour);
                    }

                    return;
                }
            }

            return;
        }

        protected void ReadDAE_PolyList(XmlReader reader, bool isPolygons = false)
        {
            string materialAttr = reader.GetAttribute("material");
            if (materialAttr == null) { AddWhiteMat(currentBone); curmaterial = "default_white"; }
            else { curmaterial = m_DAEGeometryInstanceMaterialNameMap[m_DAEGeometryToRootBoneMap.GetBySecond(currentBone)][materialAttr]; }

            // The parent bone should have a list of all materials used by itself and its children
            if (!m_Bones[m_Bones[currentBone].m_RootBone].m_Materials.ContainsKey(curmaterial))
                m_Bones[m_Bones[currentBone].m_RootBone].m_Materials.Add(curmaterial, m_Materials[curmaterial].copyAllButFaces());
            if (!m_Bones[currentBone].m_Materials.ContainsKey(curmaterial))
                m_Bones[currentBone].m_Materials.Add(curmaterial, m_Materials[curmaterial].copyAllButFaces());

            int numFaces = int.Parse(reader.GetAttribute("count"));
            int[] vcount = null;

            string element = reader.LocalName;

            reader.MoveToContent();
            while (reader.Read())
            {
                if (reader.NodeType.Equals(XmlNodeType.Element))
                {
                    if (reader.LocalName.Equals("input"))
                    {
                        ReadDAE_Geometry_Input(reader);
                    }
                    else if (reader.LocalName.Equals("vcount"))
                    {
                        vcount = ReadDAE_Geometry_VCount(reader, numFaces);
                    }
                    else if (reader.LocalName.Equals("p"))
                    {
                        ReadDAE_Geometry_P(reader, vcount, numFaces, isPolygons);
                    }
                }
                else if (reader.NodeType.Equals(XmlNodeType.EndElement) && reader.LocalName.Equals(element))
                {
                    return;
                }
            }

            return;
        }

        protected void ReadDAE_Geometry_Source(XmlReader reader)
        {
            string id = reader.GetAttribute("id");
            reader.MoveToContent();
            while (reader.Read())
            {
                if (reader.NodeType.Equals(XmlNodeType.Element))
                {
                    if (reader.LocalName.Equals("float_array"))
                    {
                        ReadDAE_Geometry_FloatArray(reader, id);
                    }
                }
                else if (reader.NodeType.Equals(XmlNodeType.EndElement) && reader.LocalName.Equals("source"))
                {
                    return;
                }
            }
            return;
        }

        protected void ReadDAE_Geometry_FloatArray(XmlReader reader, string sourceID)
        {
            string id = reader.GetAttribute("id");
            string values = reader.ReadElementContentAsString();
            string[] split = values.Split(new string[] { " ", "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            float[] float_values = new float[split.Length];
            for (int i = 0; i < split.Length; i++)
                float_values[i] = float.Parse(split[i], USA);

            reader.ReadToFollowing("accessor");
            int stride = int.Parse(reader.GetAttribute("stride"));

            List<string> paramNames = new List<string>();

            while (reader.Read())
            {
                if (reader.NodeType.Equals(XmlNodeType.Element) && reader.LocalName.Equals("param"))
                {
                    paramNames.Add(reader.GetAttribute("name"));
                }
                else if (reader.NodeType.Equals(XmlNodeType.EndElement) && reader.LocalName.Equals("accessor"))
                {
                    break;
                }
            }

            if (stride == 2)
                currentTexCoords.AddRange(float_values);
            else if (paramNames.Contains("R") || paramNames.Contains("G") || paramNames.Contains("B"))
            {
                currentColours.AddRange(float_values);
                vtxColourStrideAndRGBOffsets[0] = stride;
                vtxColourStrideAndRGBOffsets[1] = paramNames.IndexOf("R");
                vtxColourStrideAndRGBOffsets[2] = paramNames.IndexOf("G");
                vtxColourStrideAndRGBOffsets[3] = paramNames.IndexOf("B");
            }
            else
            {
                // Positions
                if (sourceID.Contains(m_DAEGeometryPositionsListNames[m_DAEGeometryToRootBoneMap.GetBySecond(currentBone)]))
                    currentVertices.AddRange(float_values);
                // Normals
                else
                    currentNormals.AddRange(float_values);
            }

        }

        protected void ReadDAE_Geometry_Input(XmlReader reader)
        {
            string semantic = reader.GetAttribute("semantic");

            switch (semantic)
            {
                case "VERTEX":
                    vertexIndex = int.Parse(reader.GetAttribute("offset"));
                    break;
                case "NORMAL":
                    normalIndex = int.Parse(reader.GetAttribute("offset"));
                    break;
                case "TEXCOORD":
                    texCoordIndex = int.Parse(reader.GetAttribute("offset"));
                    break;
                case "COLOR":
                    colourIndex = int.Parse(reader.GetAttribute("offset"));
                    break;
            }
        }

        protected int[] ReadDAE_Geometry_VCount(XmlReader reader, int numFaces)
        {
            int[] vcount = new int[numFaces];

            String values = reader.ReadElementContentAsString();
            String[] split = values.Split(new string[] { " ", "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < split.Length; i++)
            {
                vcount[i] = int.Parse(split[i]);
            }

            return vcount;
        }

        protected void ReadDAE_Geometry_P(XmlReader reader, int[] vcount, int numFaces, bool isPolygons = false)
        {
            String values = reader.ReadElementContentAsString().Trim();
            String[] split = values.Split(new string[] { " ", "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            int vtxInd = 0;
            int vcountInd = 0;
            // Faces must include vertices, normals and texture co-ordinates with
            // optional colours
            int inputCount = 0;
            if (vertexIndex != -1) inputCount++; if (normalIndex != -1) inputCount++;
            if (texCoordIndex != -1) inputCount++; if (colourIndex != -1) inputCount++;
            while (vtxInd < split.Length)
            {
                MaterialDef mat = (MaterialDef)m_Bones[currentBone].m_Materials[curmaterial];
                int nvtx = (vcount != null) ? vcount[vcountInd] : (split.Length / (inputCount * numFaces));
                if (isPolygons)
                    nvtx = (split.Length / inputCount);

                FaceDef face = new FaceDef();
                face.m_MatName = curmaterial;
                face.m_VtxIndices = new int[nvtx];
                face.m_TxcIndices = new int[nvtx];
                face.m_NrmIndices = new int[nvtx];
                face.m_ColIndices = new int[nvtx];
                face.m_BoneIDs = new int[nvtx];

                // For each vertex defined in face
                for (int i = 0; i < nvtx; i += 1)
                {
                    face.m_VtxIndices[i] = int.Parse(split[vtxInd + vertexIndex]) + m_Vertices.Count;
                    face.m_NrmIndices[i] = (normalIndex != -1) ? int.Parse(split[vtxInd + normalIndex]) + m_Normals.Count : -1;
                    face.m_TxcIndices[i] = (texCoordIndex != -1) ? int.Parse(split[vtxInd + texCoordIndex]) + m_TexCoords.Count : -1;
                    face.m_ColIndices[i] = (colourIndex != -1) ? int.Parse(split[vtxInd + colourIndex]) + m_Colours.Count : -1;
                    face.m_BoneIDs[i] = currentBoneID;// May or may not be overridden later by skinning information

                    vtxInd += inputCount;
                }
                /* Example of a typical triangle
                 * 0  0  0      2  1  3     3  2  2
                 * V1 N1 T1     V2 N2 T2    V3 N3 T3
                 * 
                 * V<n> are the indices of the triangles vertex positions,
                 * N<n> are its normal indices
                 * and T<n> are its texture co-ordinate indices
                 */

                // If the material has no textures set the texture co-ordinate indices to -1
                if (!m_Bones[currentBone].m_Materials[curmaterial].m_HasTextures)
                    face.m_TxcIndices = Enumerable.Repeat(-1, nvtx).ToArray();

                m_Bones[currentBone].m_Materials[curmaterial] = mat;
                m_Bones[currentBone].m_Materials[curmaterial].m_Faces.Add(face);

                vcountInd++;
            }
        }

        /*
         * Constructs a BCA file from the loaded animations.
         * This method must only be called AFTER loading a DAE model
         */
        protected void GenerateBCA(bool save)
        {
            NitroFile bca = m_ImportedAnimation.m_File;
            bca.Clear();

            uint dataoffset = 0x00;
            uint headersize = 0x18;
            int numAnimations = m_Bones.Count;
            int numFrames = 0;
            if (m_Animations.Values.ElementAt(0).Values.ElementAt(m_Animations.Values.ElementAt(0).Values.Count - 1).m_IsTotal)
            {
                numFrames = m_Animations.Values.ElementAt(0).Values.ElementAt(m_Animations.Values.ElementAt(0).Values.Count - 1).m_Frames.Count;
            }
            else
            {
                foreach (AnimationForImport afi in m_Animations.Values.ElementAt(0).Values)
                {
                    numFrames = afi.m_Frames.Count;
                    break;
                }
            }

            int numTransformations = 0;
            foreach (string boneName in m_Bones.Keys)
            {
                if (m_Animations.ContainsKey(boneName))
                {
                    numTransformations += (numFrames * 3);// X, Y and Z for each frame
                }
                else
                {
                    numTransformations += 3;// Just one X, Y and Z for the bone's transformations
                }
            }
            int numScale = numTransformations;
            int numRotation = numTransformations;
            int numTranslation = numTransformations;
            uint scaleValuesOffset = headersize;
            uint rotationValuesOffset = scaleValuesOffset + (uint)(numScale * 4);
            uint translationValuesOffset = (uint)(((rotationValuesOffset + (uint)(numRotation * 2)) + 3) & ~3);
            uint animationDataOffset = translationValuesOffset + (uint)(numTranslation * 4);

            bca.Write16(0x00, (ushort)numAnimations);// Number of bones to be handled (should match the number of bones in the BMD)
            bca.Write16(0x02, (ushort)numFrames);// Number of animation frames
            bca.Write32(0x04, 0);// Unknown, either 0 or 1
            bca.Write32(0x08, scaleValuesOffset);// Offset to scale values section
            bca.Write32(0x0C, rotationValuesOffset);// Offset to rotation values section
            bca.Write32(0x10, translationValuesOffset);// Offset to translation values section
            bca.Write32(0x14, animationDataOffset);// Offset to animation section

            dataoffset = scaleValuesOffset;

            // Holds the indices at which the scale, rotation and translations values are stored for each bone, 
            // only needs set during writing of scale values as it'll be the same for rotation and translation as 
            // we are not making using of constant values or interpolation for animated bones
            int dataValuesOffset = 0;
            Dictionary<string, int> boneDataValuesOffset = new Dictionary<string, int>();

            foreach (string boneName in m_Bones.Keys)
            {
                boneDataValuesOffset.Add(boneName, dataValuesOffset);

                if (m_Animations.ContainsKey(boneName))
                {
                    AnimationForImport anim = null;
                    Dictionary<string, AnimationForImport> dict = m_Animations[boneName];
                    if (dict.Values.ElementAt(dict.Values.Count - 1).m_IsTotal)
                    {
                        anim = dict.Values.ElementAt(dict.Values.Count - 1);
                    }
                    else
                    {
                        foreach (AnimationForImport afi in dict.Values)
                        {
                            anim = afi;
                            break;
                        }
                    }

                    foreach (AnimationFrameForImport frame in anim.m_Frames)
                    {
                        bca.Write32(dataoffset, (uint)(frame.m_Scale.X * 4096.0f));
                        dataoffset += 4;
                    }
                    foreach (AnimationFrameForImport frame in anim.m_Frames)
                    {
                        bca.Write32(dataoffset, (uint)(frame.m_Scale.Y * 4096.0f));
                        dataoffset += 4;
                    }
                    foreach (AnimationFrameForImport frame in anim.m_Frames)
                    {
                        bca.Write32(dataoffset, (uint)(frame.m_Scale.Z * 4096.0f));
                        dataoffset += 4;
                    }

                    dataValuesOffset += (anim.m_Frames.Count * 3);
                }
                else
                {
                    // Write bone's scale values
                    bca.Write32(dataoffset, m_Bones[boneName].m_Scale[0]);
                    dataoffset += 4;
                    bca.Write32(dataoffset, m_Bones[boneName].m_Scale[1]);
                    dataoffset += 4;
                    bca.Write32(dataoffset, m_Bones[boneName].m_Scale[2]);
                    dataoffset += 4;

                    dataValuesOffset += 3;
                }
            }

            dataoffset = rotationValuesOffset;

            // Rotation is in Radians
            foreach (string boneName in m_Bones.Keys)
            {
                if (m_Animations.ContainsKey(boneName))
                {
                    AnimationForImport anim = null;
                    Dictionary<string, AnimationForImport> dict = m_Animations[boneName];
                    if (dict.Values.ElementAt(dict.Values.Count - 1).m_IsTotal)
                    {
                        anim = dict.Values.ElementAt(dict.Values.Count - 1);
                    }
                    else
                    {
                        foreach (AnimationForImport afi in dict.Values)
                        {
                            anim = afi;
                            break;
                        }
                    }

                    foreach (AnimationFrameForImport frame in anim.m_Frames)
                    {
                        bca.Write16(dataoffset, (ushort)(float)((frame.m_Rotation.X * 2048.0f) / Math.PI));
                        dataoffset += 2;
                    }
                    foreach (AnimationFrameForImport frame in anim.m_Frames)
                    {
                        bca.Write16(dataoffset, (ushort)(float)((frame.m_Rotation.Y * 2048.0f) / Math.PI));
                        dataoffset += 2;
                    }
                    foreach (AnimationFrameForImport frame in anim.m_Frames)
                    {
                        bca.Write16(dataoffset, (ushort)(float)((frame.m_Rotation.Z * 2048.0f) / Math.PI));
                        dataoffset += 2;
                    }
                }
                else
                {
                    // Write bone's rotation values
                    bca.Write16(dataoffset, m_Bones[boneName].m_Rotation[0]);
                    dataoffset += 2;
                    bca.Write16(dataoffset, m_Bones[boneName].m_Rotation[1]);
                    dataoffset += 2;
                    bca.Write16(dataoffset, m_Bones[boneName].m_Rotation[2]);
                    dataoffset += 2;
                }
            }

            dataoffset = translationValuesOffset;

            foreach (string boneName in m_Bones.Keys)
            {
                if (m_Animations.ContainsKey(boneName))
                {
                    AnimationForImport anim = null;
                    Dictionary<string, AnimationForImport> dict = m_Animations[boneName];
                    if (dict.Values.ElementAt(dict.Values.Count - 1).m_IsTotal)
                    {
                        anim = dict.Values.ElementAt(dict.Values.Count - 1);
                    }
                    else
                    {
                        foreach (AnimationForImport afi in dict.Values)
                        {
                            anim = afi;
                            break;
                        }
                    }

                    foreach (AnimationFrameForImport frame in anim.m_Frames)
                    {
                        bca.Write32(dataoffset, (uint)(frame.m_Translation.X * 4096.0f));
                        dataoffset += 4;
                    }
                    foreach (AnimationFrameForImport frame in anim.m_Frames)
                    {
                        bca.Write32(dataoffset, (uint)(frame.m_Translation.Y * 4096.0f));
                        dataoffset += 4;
                    }
                    foreach (AnimationFrameForImport frame in anim.m_Frames)
                    {
                        bca.Write32(dataoffset, (uint)(frame.m_Translation.Z * 4096.0f));
                        dataoffset += 4;
                    }
                }
                else
                {
                    // Write bone's translation values
                    bca.Write32(dataoffset, m_Bones[boneName].m_Translation[0]);
                    dataoffset += 4;
                    bca.Write32(dataoffset, m_Bones[boneName].m_Translation[1]);
                    dataoffset += 4;
                    bca.Write32(dataoffset, m_Bones[boneName].m_Translation[2]);
                    dataoffset += 4;
                }
            }

            dataoffset = animationDataOffset;

            // For each bone
            foreach (string boneName in m_Bones.Keys)
            {
                if (m_Animations.ContainsKey(boneName))
                {
                    AnimationForImport anim = null;
                    Dictionary<string, AnimationForImport> dict = m_Animations[boneName];
                    if (dict.Values.ElementAt(dict.Values.Count - 1).m_IsTotal)
                    {
                        anim = dict.Values.ElementAt(dict.Values.Count - 1);
                    }
                    else
                    {
                        foreach (AnimationForImport afi in dict.Values)
                        {
                            anim = afi;
                            break;
                        }
                    }

                    // Scale X
                    WriteBCAAnimationDescriptor(bca, dataoffset, true, (boneDataValuesOffset[boneName] + (numFrames * 0)));
                    dataoffset += 4;

                    // Scale Y
                    WriteBCAAnimationDescriptor(bca, dataoffset, true, (boneDataValuesOffset[boneName] + (numFrames * 1)));
                    dataoffset += 4;

                    // Scale Z
                    WriteBCAAnimationDescriptor(bca, dataoffset, true, (boneDataValuesOffset[boneName] + (numFrames * 2)));
                    dataoffset += 4;

                    // Rotation X
                    WriteBCAAnimationDescriptor(bca, dataoffset, true, (boneDataValuesOffset[boneName] + (numFrames * 0)));
                    dataoffset += 4;

                    // Rotation Y
                    WriteBCAAnimationDescriptor(bca, dataoffset, true, (boneDataValuesOffset[boneName] + (numFrames * 1)));
                    dataoffset += 4;

                    // Rotation Z
                    WriteBCAAnimationDescriptor(bca, dataoffset, true, (boneDataValuesOffset[boneName] + (numFrames * 2)));
                    dataoffset += 4;

                    // Translation X
                    WriteBCAAnimationDescriptor(bca, dataoffset, true, (boneDataValuesOffset[boneName] + (numFrames * 0)));
                    dataoffset += 4;

                    // Translation Y
                    WriteBCAAnimationDescriptor(bca, dataoffset, true, (boneDataValuesOffset[boneName] + (numFrames * 1)));
                    dataoffset += 4;

                    // Translation Z
                    WriteBCAAnimationDescriptor(bca, dataoffset, true, (boneDataValuesOffset[boneName] + (numFrames * 2)));
                    dataoffset += 4;
                }
                else
                {
                    // Set to use constant values (the bone's transformation as there's no animation)

                    // Scale X
                    WriteBCAAnimationDescriptor(bca, dataoffset, false, (boneDataValuesOffset[boneName] + (1 * 0)));
                    dataoffset += 4;

                    // Scale Y
                    WriteBCAAnimationDescriptor(bca, dataoffset, false, (boneDataValuesOffset[boneName] + (1 * 1)));
                    dataoffset += 4;

                    // Scale Z
                    WriteBCAAnimationDescriptor(bca, dataoffset, false, (boneDataValuesOffset[boneName] + (1 * 2)));
                    dataoffset += 4;

                    // Rotation X
                    WriteBCAAnimationDescriptor(bca, dataoffset, false, (boneDataValuesOffset[boneName] + (1 * 0)));
                    dataoffset += 4;

                    // Rotation Y
                    WriteBCAAnimationDescriptor(bca, dataoffset, false, (boneDataValuesOffset[boneName] + (1 * 1)));
                    dataoffset += 4;

                    // Rotation Z
                    WriteBCAAnimationDescriptor(bca, dataoffset, false, (boneDataValuesOffset[boneName] + (1 * 2)));
                    dataoffset += 4;

                    // Translation X
                    WriteBCAAnimationDescriptor(bca, dataoffset, false, (boneDataValuesOffset[boneName] + (1 * 0)));
                    dataoffset += 4;

                    // Translation Y
                    WriteBCAAnimationDescriptor(bca, dataoffset, false, (boneDataValuesOffset[boneName] + (1 * 1)));
                    dataoffset += 4;

                    // Translation Z
                    WriteBCAAnimationDescriptor(bca, dataoffset, false, (boneDataValuesOffset[boneName] + (1 * 2)));
                    dataoffset += 4;
                }
            }

            if (save)
                bca.SaveChanges();

            m_ImportedAnimation = new BCA(bca);
        }

        private void WriteBCAAnimationDescriptor(NitroFile bca, uint offset, bool indexIncrements, int startIndex)
        {
            bca.Write8(offset + 0x00, 0);// Use interpolation
            bca.Write8(offset + 0x01, (indexIncrements == true) ? (byte)1 : (byte)0);// Index increments with each frame
            bca.Write16(offset + 0x02, (ushort)startIndex);// Starting index
        }

        public class AnimationForImport
        {
            public string m_ID;
            public Dictionary<string, SourceForImport> m_Sources = new Dictionary<string, SourceForImport>();
            public Dictionary<string, AnimationSamplerChannelPairForImport> m_SamplerChannelPairs =
                new Dictionary<string, AnimationSamplerChannelPairForImport>();
            public List<AnimationFrameForImport> m_Frames = new List<AnimationFrameForImport>();
            public bool m_IsTotal;// Whether this bone contains the merged animations for the bone

            public AnimationForImport(string id)
            {
                m_ID = id;
                m_IsTotal = false;
            }

            public AnimationForImport(string id, bool isTotal)
            {
                m_ID = id;
                m_IsTotal = isTotal;
            }
        }

        public class AnimationFrameForImport
        {
            public Vector3 m_Scale;
            public Vector3 m_Rotation;
            public Vector3 m_Translation;

            public AnimationFrameForImport()
            {
                m_Scale = new Vector3(1f, 1f, 1f);
                m_Rotation = new Vector3(0f, 0f, 0f);
                m_Translation = new Vector3(0f, 0f, 0f);
            }

            public AnimationFrameForImport(Vector3 scale, Vector3 rotation, Vector3 tranlation)
            {
                m_Scale = scale;
                m_Rotation = rotation;
                m_Translation = tranlation;
            }
        }

        public class AnimationSamplerChannelPairForImport
        {
            public string m_ID;
            public TransformationType m_Type;
            public string m_InputSourceID;
            public string m_OutputSourceID;
            public string m_InterpolationSourceID;

            public AnimationSamplerChannelPairForImport(string id)
            {
                m_ID = id;
                m_Type = TransformationType.None;// To be updated when a corresponding <channel> is read in
            }
        }

        public class SourceForImport
        {
            public string m_ID;
            public int m_Count;
            public int m_Stride;
            public float[] m_FloatArray;
            public string[] m_NameArray;
            public string[] m_IDREFArray;
            public string m_ElementType;
            public int m_StartIndexOffset;

            public SourceForImport(string id)
            {
                m_ID = id;
            }
        }

    }
}