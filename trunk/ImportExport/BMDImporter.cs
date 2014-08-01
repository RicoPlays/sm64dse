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
        public static BMD ConvertModelToBMD(NitroFile modelFile, string fileName, bool save = true)
        {
            return ConvertModelToBMD(modelFile, fileName, new Vector3(1f, 1f, 1f), save);
        }

        public static BMD ConvertModelToBMD(NitroFile modelFile, string fileName, Vector3 scale, bool save = true)
        {
            BMD importedModel = null;

            string modelFormat = fileName.Substring(fileName.Length - 3, 3).ToLower();
            switch (modelFormat)
            {
                case "obj":
                    importedModel = ConvertOBJToBMD(modelFile, fileName, new Vector3(1f, 1f, 1f), save);
                    break;
                case "dae":
                    importedModel = ConvertDAEToBMD(modelFile, fileName, new Vector3(1f, 1f, 1f), save);
                    break;
                default:
                    importedModel = ConvertOBJToBMD(modelFile, fileName, new Vector3(1f, 1f, 1f), save);
                    break;
            }

            return importedModel;
        }

        public static BMD ConvertDAEToBMD(NitroFile modelFile, string fileName, bool save = true)
        {
            return ConvertDAEToBMD(modelFile, fileName, new Vector3(1f, 1f, 1f), save);
        }

        public static BMD ConvertDAEToBMD(NitroFile modelFile, string fileName, Vector3 scale, bool save = true)
        {
            BMD importedModel = new BMD(modelFile);

            ModelBase model = new DAELoader(fileName).LoadModel();

            importedModel = CallBMDWriter(modelFile, model, save);

            if (save)
                importedModel.m_File.SaveChanges();

            return importedModel;
        }

        public static BMD ConvertOBJToBMD(NitroFile modelFile, string fileName, bool save = true)
        {
            return ConvertOBJToBMD(modelFile, fileName, new Vector3(1f, 1f, 1f), save);
        }

        public static BMD ConvertOBJToBMD(NitroFile modelFile, string fileName, Vector3 scale, bool save = true)
        {
            BMD importedModel = new BMD(modelFile);

            ModelBase model = new OBJLoader(fileName).LoadModel(scale);

            importedModel = CallBMDWriter(modelFile, model, save);

            if (save)
                importedModel.m_File.SaveChanges();

            return importedModel;
        }

        public static Dictionary<string, ModelBase.MaterialDef> GetModelMaterials(string fileName)
        {
            string modelFormat = fileName.Substring(fileName.Length - 3, 3).ToLower();
            switch (modelFormat)
            {
                case "obj":
                    return new OBJLoader(fileName).GetModelMaterials();
                case "dae":
                    return new DAELoader(fileName).GetModelMaterials();
                default:
                    return new OBJLoader(fileName).GetModelMaterials();
            }
        }

        public static Dictionary<string, ModelBase.MaterialDef> GetDAEMaterials(string fileName)
        {
            return new DAELoader(fileName).GetModelMaterials();
        }

        public static Dictionary<string, ModelBase.MaterialDef> GetOBJMaterials(string fileName)
        {
            return new OBJLoader(fileName).GetModelMaterials();
        }

        public static BCA ConvertDAEToBCA(NitroFile animation, string fileName, bool save = true)
        {
            BCA importedAnimation = null;

            if (save)
                importedAnimation.m_File.SaveChanges();

            return importedAnimation;
        }

        protected static BMD CallBMDWriter(NitroFile modelFile, ModelBase model, bool save = true)
        {
            AbstractModelWriter bmdWriter = new BMDWriter(model, modelFile);

            bmdWriter.WriteModel(save);

            return new BMD(modelFile);
        }

    }
}