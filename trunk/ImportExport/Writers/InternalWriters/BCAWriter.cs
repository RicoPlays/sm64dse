/* BCAWriter
 * 
 * Given a ModelBase object with animations, produces a BCA file.
 */ 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SM64DSe.ImportExport.Writers.InternalWriters
{
    public class BCAWriter : AbstractModelWriter
    {
        public NitroFile m_ModelFile;

        public BCAWriter(ModelBase model, ref NitroFile modelFile) :
            base(model, modelFile.m_Name)
        {
            m_ModelFile = modelFile;
        }

        public override void WriteModel(bool save = true)
        {
            ModelBase.BoneDefRoot boneTree = m_Model.m_BoneTree;
            Dictionary<string, ModelBase.AnimationDef> animations = m_Model.m_Animations;

            if (animations.Count == 0) return;

            NitroFile bca = m_ModelFile;
            bca.Clear();

            uint dataoffset = 0x00;
            uint headersize = 0x18;
            int numAnimations = boneTree.Count;
            int numFrames = animations.ElementAt(0).Value.m_AnimationFrames.Count;

            int numTransformations = 0;
            foreach (ModelBase.BoneDef bone in boneTree)
            {
                if (animations.ContainsKey(bone.m_ID))
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

            foreach (ModelBase.BoneDef bone in boneTree)
            {
                boneDataValuesOffset.Add(bone.m_ID, dataValuesOffset);

                if (animations.ContainsKey(bone.m_ID))
                {
                    ModelBase.AnimationDef anim = animations[bone.m_ID];

                    foreach (ModelBase.AnimationFrameDef frame in anim.m_AnimationFrames)
                    {
                        bca.Write32(dataoffset, (uint)(frame.GetScale().X * 4096.0f));
                        dataoffset += 4;
                    }
                    foreach (ModelBase.AnimationFrameDef frame in anim.m_AnimationFrames)
                    {
                        bca.Write32(dataoffset, (uint)(frame.GetScale().Y * 4096.0f));
                        dataoffset += 4;
                    }
                    foreach (ModelBase.AnimationFrameDef frame in anim.m_AnimationFrames)
                    {
                        bca.Write32(dataoffset, (uint)(frame.GetScale().Z * 4096.0f));
                        dataoffset += 4;
                    }

                    dataValuesOffset += (anim.m_AnimationFrames.Count * 3);
                }
                else
                {
                    // Write bone's scale values
                    bca.Write32(dataoffset, bone.m_20_12Scale[0]);
                    dataoffset += 4;
                    bca.Write32(dataoffset, bone.m_20_12Scale[1]);
                    dataoffset += 4;
                    bca.Write32(dataoffset, bone.m_20_12Scale[2]);
                    dataoffset += 4;

                    dataValuesOffset += 3;
                }
            }

            dataoffset = rotationValuesOffset;

            // Rotation is in Radians
            foreach (ModelBase.BoneDef bone in boneTree)
            {
                if (animations.ContainsKey(bone.m_ID))
                {
                    ModelBase.AnimationDef anim = animations[bone.m_ID];

                    foreach (ModelBase.AnimationFrameDef frame in anim.m_AnimationFrames)
                    {
                        bca.Write16(dataoffset, (ushort)(float)((frame.GetRotation().X * 2048.0f) / Math.PI));
                        dataoffset += 2;
                    }
                    foreach (ModelBase.AnimationFrameDef frame in anim.m_AnimationFrames)
                    {
                        bca.Write16(dataoffset, (ushort)(float)((frame.GetRotation().Y * 2048.0f) / Math.PI));
                        dataoffset += 2;
                    }
                    foreach (ModelBase.AnimationFrameDef frame in anim.m_AnimationFrames)
                    {
                        bca.Write16(dataoffset, (ushort)(float)((frame.GetRotation().Z * 2048.0f) / Math.PI));
                        dataoffset += 2;
                    }
                }
                else
                {
                    // Write bone's rotation values
                    bca.Write16(dataoffset, bone.m_4_12Rotation[0]);
                    dataoffset += 2;
                    bca.Write16(dataoffset, bone.m_4_12Rotation[1]);
                    dataoffset += 2;
                    bca.Write16(dataoffset, bone.m_4_12Rotation[2]);
                    dataoffset += 2;
                }
            }

            dataoffset = translationValuesOffset;

            foreach (ModelBase.BoneDef bone in boneTree)
            {
                if (animations.ContainsKey(bone.m_ID))
                {
                    ModelBase.AnimationDef anim = animations[bone.m_ID];

                    foreach (ModelBase.AnimationFrameDef frame in anim.m_AnimationFrames)
                    {
                        bca.Write32(dataoffset, (uint)(frame.GetTranslation().X * 4096.0f));
                        dataoffset += 4;
                    }
                    foreach (ModelBase.AnimationFrameDef frame in anim.m_AnimationFrames)
                    {
                        bca.Write32(dataoffset, (uint)(frame.GetTranslation().Y * 4096.0f));
                        dataoffset += 4;
                    }
                    foreach (ModelBase.AnimationFrameDef frame in anim.m_AnimationFrames)
                    {
                        bca.Write32(dataoffset, (uint)(frame.GetTranslation().Z * 4096.0f));
                        dataoffset += 4;
                    }
                }
                else
                {
                    // Write bone's translation values
                    bca.Write32(dataoffset, bone.m_20_12Translation[0]);
                    dataoffset += 4;
                    bca.Write32(dataoffset, bone.m_20_12Translation[1]);
                    dataoffset += 4;
                    bca.Write32(dataoffset, bone.m_20_12Translation[2]);
                    dataoffset += 4;
                }
            }

            dataoffset = animationDataOffset;

            // For each bone, write the animation descriptor for each transformation component
            foreach (ModelBase.BoneDef bone in boneTree)
            {
                if (animations.ContainsKey(bone.m_ID))
                {
                    ModelBase.AnimationDef anim = animations[bone.m_ID];

                    // Scale X
                    WriteBCAAnimationDescriptor(bca, dataoffset, true, (boneDataValuesOffset[bone.m_ID] + (numFrames * 0)));
                    dataoffset += 4;

                    // Scale Y
                    WriteBCAAnimationDescriptor(bca, dataoffset, true, (boneDataValuesOffset[bone.m_ID] + (numFrames * 1)));
                    dataoffset += 4;

                    // Scale Z
                    WriteBCAAnimationDescriptor(bca, dataoffset, true, (boneDataValuesOffset[bone.m_ID] + (numFrames * 2)));
                    dataoffset += 4;

                    // Rotation X
                    WriteBCAAnimationDescriptor(bca, dataoffset, true, (boneDataValuesOffset[bone.m_ID] + (numFrames * 0)));
                    dataoffset += 4;

                    // Rotation Y
                    WriteBCAAnimationDescriptor(bca, dataoffset, true, (boneDataValuesOffset[bone.m_ID] + (numFrames * 1)));
                    dataoffset += 4;

                    // Rotation Z
                    WriteBCAAnimationDescriptor(bca, dataoffset, true, (boneDataValuesOffset[bone.m_ID] + (numFrames * 2)));
                    dataoffset += 4;

                    // Translation X
                    WriteBCAAnimationDescriptor(bca, dataoffset, true, (boneDataValuesOffset[bone.m_ID] + (numFrames * 0)));
                    dataoffset += 4;

                    // Translation Y
                    WriteBCAAnimationDescriptor(bca, dataoffset, true, (boneDataValuesOffset[bone.m_ID] + (numFrames * 1)));
                    dataoffset += 4;

                    // Translation Z
                    WriteBCAAnimationDescriptor(bca, dataoffset, true, (boneDataValuesOffset[bone.m_ID] + (numFrames * 2)));
                    dataoffset += 4;
                }
                else
                {
                    // Set to use constant values (the bone's transformation as there's no animation)

                    // Scale X
                    WriteBCAAnimationDescriptor(bca, dataoffset, false, (boneDataValuesOffset[bone.m_ID] + (1 * 0)));
                    dataoffset += 4;

                    // Scale Y
                    WriteBCAAnimationDescriptor(bca, dataoffset, false, (boneDataValuesOffset[bone.m_ID] + (1 * 1)));
                    dataoffset += 4;

                    // Scale Z
                    WriteBCAAnimationDescriptor(bca, dataoffset, false, (boneDataValuesOffset[bone.m_ID] + (1 * 2)));
                    dataoffset += 4;

                    // Rotation X
                    WriteBCAAnimationDescriptor(bca, dataoffset, false, (boneDataValuesOffset[bone.m_ID] + (1 * 0)));
                    dataoffset += 4;

                    // Rotation Y
                    WriteBCAAnimationDescriptor(bca, dataoffset, false, (boneDataValuesOffset[bone.m_ID] + (1 * 1)));
                    dataoffset += 4;

                    // Rotation Z
                    WriteBCAAnimationDescriptor(bca, dataoffset, false, (boneDataValuesOffset[bone.m_ID] + (1 * 2)));
                    dataoffset += 4;

                    // Translation X
                    WriteBCAAnimationDescriptor(bca, dataoffset, false, (boneDataValuesOffset[bone.m_ID] + (1 * 0)));
                    dataoffset += 4;

                    // Translation Y
                    WriteBCAAnimationDescriptor(bca, dataoffset, false, (boneDataValuesOffset[bone.m_ID] + (1 * 1)));
                    dataoffset += 4;

                    // Translation Z
                    WriteBCAAnimationDescriptor(bca, dataoffset, false, (boneDataValuesOffset[bone.m_ID] + (1 * 2)));
                    dataoffset += 4;
                }
            }

            if (save)
                bca.SaveChanges();
        }

        private static void WriteBCAAnimationDescriptor(NitroFile bca, uint offset, bool indexIncrements, int startIndex)
        {
            bca.Write8(offset + 0x00, 0);// Use interpolation
            bca.Write8(offset + 0x01, (indexIncrements == true) ? (byte)1 : (byte)0);// Index increments with each frame
            bca.Write16(offset + 0x02, (ushort)startIndex);// Starting index
        }
    }
}
