/* BCALoader
 * 
 * Given a BCA object and a ModelBase object, adds animations to the ModelBase object for use in the 
 * Writer classes.
 */ 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SM64DSe.ImportExport.Loaders.InternalLoaders
{
    public class BCALoader : AbstractModelLoader
    {
        BCA m_BCA;

        public BCALoader(ModelBase model, BCA animation, string modelFileName) :
            base(modelFileName)
        {
            m_BCA = animation;
            m_Model = model;
        }

        public override ModelBase LoadModel(OpenTK.Vector3 scale)
        {
            List<ModelBase.BoneDef> flatBoneList = m_Model.m_BoneTree.GetAsList();
            for (int i = 0; i < m_BCA.m_AnimationData.Length; i++)
            {
                string boneID = flatBoneList[i].m_ID;
                ModelBase.AnimationDef animation = new ModelBase.AnimationDef(boneID + "-animation", boneID);
                m_Model.m_Animations.Add(animation.m_ID, animation);
            }

            int numBones = flatBoneList.Count;
            for (int i = 0; i < m_BCA.m_NumFrames; i++)
            {
                BCA.SRTContainer[] transformations = m_BCA.GetAllLocalSRTValuesForFrame(numBones, i);

                for (int j = 0; j < m_Model.m_Animations.Count; j++)
                {
                    ModelBase.AnimationFrameDef frame = new ModelBase.AnimationFrameDef();
                    frame.SetScale(transformations[j].m_Scale);
                    frame.SetRotation(transformations[j].m_Rotation);
                    frame.SetTranslation(transformations[j].m_Translation);

                    m_Model.m_Animations.Values.ElementAt(j).m_AnimationFrames.Add(frame);
                }
            }

            return m_Model;
        }

        public override Dictionary<string, ModelBase.MaterialDef> GetModelMaterials()
        {
            return m_Model.m_Materials;
        }
    }
}
