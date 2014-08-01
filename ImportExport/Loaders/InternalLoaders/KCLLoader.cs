/* KCLLoader
 * 
 * Uses a KCL object to generate a ModelBase object for use in OBJWriter and DAEWriter (KCLExporter).
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SM64DSe.ImportExport.Loaders.InternalLoaders
{
    public class KCLLoader : AbstractModelLoader
    {
        KCL m_KCL;

        public KCLLoader(string modelFileName, KCL kcl) :
            base(modelFileName) 
        {
            m_KCL = kcl;
        }

        public override ModelBase LoadModel(OpenTK.Vector3 scale)
        {
            throw new NotImplementedException();
        }

        public override Dictionary<string, ModelBase.MaterialDef> GetModelMaterials()
        {
            throw new NotImplementedException();
        }
    }
}
