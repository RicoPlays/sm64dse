/* SM64DSe allows the importing of models to BMD format.
 * Currently supported:
 * 
 * COLLADA DAE:
 *  
 * Imports the modelFile from a COLLADA DAE modelFile complete with full joints and skinning (rigging).
 * 
 * Notes:
 * BMD does not support vertex weights, instead each vertex is only assigned to one bone - where a 
 * DAE modelFile uses multiple < 1.0 vertex weights, the largest value is used to assign the vertex to 
 * a joint.
 * 
 * This is the recommended format for importing as it matches the features of BMD almost exactly.
 * 
 * 
 * Wavefront OBJ:
 * 
 * Imports an OBJ modelFile.
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
using OpenTK;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Globalization;
using OpenTK.Graphics.OpenGL;
using System.Security.Cryptography;
using System.Xml;
using System.Text.RegularExpressions;
using SM64DSe.ImportExport.Loaders;
using SM64DSe.ImportExport.Writers.InternalWriters;
using SM64DSe.ImportExport.Writers;
using SM64DSe.ImportExport.Loaders.ExternalLoaders;

namespace SM64DSe.ImportExport
{
    public class BMDImporter
    {
        public ModelBase m_LoadedModel;

        public BMD ConvertModelToBMD(ref NitroFile modelFile, string fileName, bool save = true)
        {
            return ConvertModelToBMD(ref modelFile, fileName, new Vector3(1f, 1f, 1f), save);
        }

        public BMD ConvertModelToBMD(ref NitroFile modelFile, string fileName, Vector3 scale, bool save = true)
        {
            BMD importedModel = null;

            string modelFormat = fileName.Substring(fileName.Length - 3, 3).ToLower();
            switch (modelFormat)
            {
                case "obj":
                    importedModel = ConvertOBJToBMD(ref modelFile, fileName, scale, save);
                    break;
                case "dae":
                    importedModel = ConvertDAEToBMD(ref modelFile, fileName, scale, save);
                    break;
                default:
                    importedModel = ConvertOBJToBMD(ref modelFile, fileName, scale, save);
                    break;
            }

            return importedModel;
        }

        public BMD ConvertDAEToBMD(ref NitroFile modelFile, string fileName, bool save = true)
        {
            return ConvertDAEToBMD(ref modelFile, fileName, new Vector3(1f, 1f, 1f), save);
        }

        public BMD ConvertDAEToBMD(ref NitroFile modelFile, string fileName, Vector3 scale, bool save = true)
        {
            BMD importedModel = new BMD(modelFile);

            if (m_LoadedModel == null)
                m_LoadedModel = new DAELoader(fileName).LoadModel(scale);

            importedModel = CallBMDWriter(ref modelFile, m_LoadedModel, save);

            return importedModel;
        }

        public BMD ConvertOBJToBMD(ref NitroFile modelFile, string fileName, bool save = true)
        {
            return ConvertOBJToBMD(ref modelFile, fileName, new Vector3(1f, 1f, 1f), save);
        }

        public BMD ConvertOBJToBMD(ref NitroFile modelFile, string fileName, Vector3 scale, bool save = true)
        {
            BMD importedModel = new BMD(modelFile);

            if (m_LoadedModel == null)
                m_LoadedModel = new OBJLoader(fileName).LoadModel(scale);

            importedModel = CallBMDWriter(ref modelFile, m_LoadedModel, save);

            return importedModel;
        }

        public Dictionary<string, ModelBase.MaterialDef> GetModelMaterials(string fileName)
        {
            string modelFormat = fileName.Substring(fileName.Length - 3, 3).ToLower();
            switch (modelFormat)
            {
                case "obj":
                    return (m_LoadedModel != null) ? m_LoadedModel.m_Materials : new OBJLoader(fileName).GetModelMaterials();
                case "dae":
                    return (m_LoadedModel != null) ? m_LoadedModel.m_Materials : new DAELoader(fileName).GetModelMaterials();
                default:
                    return (m_LoadedModel != null) ? m_LoadedModel.m_Materials : new OBJLoader(fileName).GetModelMaterials();
            }
        }

        public BCA ConvertAnimatedDAEToBMDAndBCA(ref NitroFile animationFile, string fileName, bool save = true)
        {
            if (m_LoadedModel == null)
                m_LoadedModel = new DAELoader(fileName).LoadModel();

            BCA importedAnimation = CallBCAWriter(ref animationFile, m_LoadedModel, save);

            return importedAnimation;
        }

        protected BMD CallBMDWriter(ref NitroFile modelFile, ModelBase model, bool save = true)
        {
            AbstractModelWriter bmdWriter = new BMDWriter(model, ref modelFile);

            bmdWriter.WriteModel(save);

            return new BMD(modelFile);
        }

        protected BCA CallBCAWriter(ref NitroFile animationFile, ModelBase model, bool save = true)
        {
            AbstractModelWriter bcaWriter = new BCAWriter(model, ref animationFile);

            bcaWriter.WriteModel(save);

            return new BCA(animationFile);
        }

    }
}