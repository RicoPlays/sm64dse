using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using System.Drawing;
using System.Globalization;
using System.Security.Cryptography;
using System.IO;
using SM64DSe.ImportExport;
using OpenTK.Graphics.OpenGL;

namespace SM64DSe.ImportExport.Loaders
{
    public abstract class AbstractModelLoader
    {
        public ModelBase m_Model;

        protected string m_ModelFileName;
        protected string m_ModelPath;

        protected MD5CryptoServiceProvider m_MD5;
        protected static CultureInfo USA = Helper.USA;

        public ModelBase LoadModel() { return LoadModel(new Vector3(1f, 1f, 1f)); }
        public abstract ModelBase LoadModel(Vector3 scale);
        public abstract Dictionary<string, ModelBase.MaterialDef> GetModelMaterials();

        public AbstractModelLoader(string modelFileName)
        {
            m_Model = new ModelBase(modelFileName);

            m_ModelFileName = modelFileName;
            m_ModelPath = Path.GetDirectoryName(m_ModelFileName);

            m_MD5 = new MD5CryptoServiceProvider();
        }

        protected void AddWhiteMat()
        {
            if (m_Model.m_Materials.ContainsKey("default_white"))
                return;
            ModelBase.MaterialDef mat = new ModelBase.MaterialDef("default_white", m_Model.m_Materials.Count);
            m_Model.m_Materials.Add("default_white", mat);
        }

        protected void AddWhiteMat(string bone)
        {
            AddWhiteMat();
            if (!m_Model.m_BoneTree.GetBoneByID(bone).GetRoot().m_MaterialsInBranch.Contains("default_white"))
                m_Model.m_BoneTree.GetBoneByID(bone).GetRoot().m_MaterialsInBranch.Add("default_white");
            if (!m_Model.m_BoneTree.GetBoneByID(bone).m_MaterialsInBranch.Contains("default_white"))
                m_Model.m_BoneTree.GetBoneByID(bone).m_MaterialsInBranch.Add("default_white");
        }

        protected void AddTexture(string texName, ModelBase.MaterialDef matDef)
        {
            Bitmap tex;
            try
            {
                tex = new Bitmap(m_ModelPath + Path.DirectorySeparatorChar + texName);

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

                matDef.m_HasTextures = true;

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

                string imghash = HexString(m_MD5.ComputeHash(map));
                if (m_Model.m_Textures.ContainsKey(imghash))
                {
                    ModelBase.MaterialDef mat2 = m_Model.m_Textures[imghash];
                    matDef.m_DiffuseMapName = mat2.m_DiffuseMapName;
                    matDef.m_DiffuseMapSize = mat2.m_DiffuseMapSize;
                    return;
                }

                matDef.m_DiffuseMapName = texName;
                m_Model.m_Textures.Add(imghash, matDef);

                matDef.m_DiffuseMapSize.X = tex.Width;
                matDef.m_DiffuseMapSize.Y = tex.Height;
            }
            catch
            {
                Console.WriteLine("Image not found: " + m_ModelPath + Path.DirectorySeparatorChar + texName);
            }
        }

        protected static string HexString(byte[] crap)
        {
            string ret = "";
            foreach (byte b in crap)
                ret += b.ToString("X2");
            return ret;
        }

    }
}
