/* DAELoader
 * 
 * Using the 'csharpcollada' library to parse COLLADA DAE model, converts a COLLADA DAE model into a 
 * ModelBase object for use in BMDImporter and KCLImporter.
 */ 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using System.Drawing;
using System.Xml;

namespace SM64DSe.ImportExport.Loaders.ExternalLoaders
{
    public class DAELoader : AbstractModelLoader
    {
        XmlDocument m_COLLADA;

        public DAELoader(string modelFileName) : 
            base(modelFileName)
        {
            
        }

        public override ModelBase LoadModel(Vector3 scale)
        {
            LoadMaterials();

            return m_Model;
        }

        private void LoadMaterials()
        {
            
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
