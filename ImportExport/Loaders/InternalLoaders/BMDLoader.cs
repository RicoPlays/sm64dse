/* BMD Loader
 * 
 * Uses a BMD object to generate a ModelBase object for use in OBJWriter and DAEWriter (BMDExporter).
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using SM64DSe.ImportExport.Writers.InternalWriters;

namespace SM64DSe.ImportExport.Loaders.InternalLoaders
{
    public class BMDLoader : AbstractModelLoader
    {
        BMD m_BMD;

        public BMDLoader(string modelFileName, BMD bmd) :
            base(modelFileName) 
        {
            m_BMD = bmd;
        }

        public override ModelBase LoadModel(OpenTK.Vector3 scale)
        {
            // AFTER PARSING MAKE SURE TO CALL ModelBase.ApplyTransformations();

            throw new NotImplementedException();
        }

        public Bitmap ConvertBMDTextureToBitmap(BMD.Texture texture)
        {
            Bitmap lol = new Bitmap((int)texture.m_Width, (int)texture.m_Height);

            for (int y = 0; y < (int)texture.m_Height; y++)
            {
                for (int x = 0; x < (int)texture.m_Width; x++)
                {
                    lol.SetPixel(x, y, Color.FromArgb(texture.m_Data[((y * texture.m_Width) + x) * 4 + 3],
                     texture.m_Data[((y * texture.m_Width) + x) * 4 + 2],
                     texture.m_Data[((y * texture.m_Width) + x) * 4 + 1],
                     texture.m_Data[((y * texture.m_Width) + x) * 4]));
                }
            }

            return lol;
        }

        public override Dictionary<string, ModelBase.MaterialDef> GetModelMaterials()
        {
            throw new NotImplementedException();
        }
    }
}
