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
            ModelBase.MaterialDef mat = new ModelBase.MaterialDef("default_white");
            mat.m_Index = m_Model.m_Materials.Count;
            mat.m_DiffuseColour = Color.White;
            mat.m_Opacity = 255;
            mat.m_HasTextures = false;
            mat.m_DiffuseMapName = "";
            mat.m_DiffuseMapID = 0;
            mat.m_DiffuseMapSize = new Vector2(0f, 0f);
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

        protected static string HexString(byte[] crap)
        {
            string ret = "";
            foreach (byte b in crap)
                ret += b.ToString("X2");
            return ret;
        }

    }
}
