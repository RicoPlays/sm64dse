/* DAELoader
 * 
 * Using the classes produced by the XSD tool from the COLLADA 1.4.1 specification to parse COLLADA DAE models, converts a 
 * COLLADA DAE model into a ModelBase object for use by the Writer classes.
 * 
 * Supports joints, skinning and animations (see below notes) as well as plain static meshes.
 * 
 * Animation support is currently limited to models whose transformations are defined using separate scale, rotation and 
 * translation components. Models whose transformations are defined using a transformation matrix are only supported for 
 * joints with a depth of 2 (root and its child nodes only).
 */ 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using System.Drawing;
using System.Xml;
using Collada141;
using System.Windows.Forms;
using System.IO;

namespace SM64DSe.ImportExport.Loaders.ExternalLoaders
{
    public class DAELoader : AbstractModelLoader
    {
        COLLADA m_COLLADAModel;

        library_images library_images;
        library_effects library_effects;
        library_materials library_materials;
        library_geometries library_geometries;
        library_controllers library_controllers;
        library_visual_scenes library_visual_scenes;
        library_animations library_animations;

        public DAELoader(string modelFileName) : 
            base(modelFileName)
        {
            m_COLLADAModel = COLLADA.Load(modelFileName);
        }

        public override ModelBase LoadModel(Vector3 scale)
        {
            foreach (var item in m_COLLADAModel.Items)
            {
                if (item.GetType().Equals(typeof(library_images))) this.library_images = item as library_images;
                if (item.GetType().Equals(typeof(library_effects))) this.library_effects = item as library_effects;
                if (item.GetType().Equals(typeof(library_materials))) this.library_materials = item as library_materials;
                if (item.GetType().Equals(typeof(library_geometries))) this.library_geometries = item as library_geometries;
                if (item.GetType().Equals(typeof(library_controllers))) this.library_controllers = item as library_controllers;
                if (item.GetType().Equals(typeof(library_visual_scenes))) this.library_visual_scenes = item as library_visual_scenes;
                if (item.GetType().Equals(typeof(library_animations))) this.library_animations = item as library_animations;
            }

            ReadMaterials();

            ReadVisualScenes();

            ReadAnimations();

            m_Model.ScaleModel(scale);

            return m_Model;
        }

        private void ReadMaterials()
        {
            if (this.library_materials == null || this.library_materials.material.Length == 0)
            {
                AddWhiteMat();
                return;
            }

            foreach (material mat in this.library_materials.material)
            {
                string id = mat.id;
                string effectID = mat.instance_effect.url.Replace("#", "");

                ModelBase.MaterialDef matDef = new ModelBase.MaterialDef(id, m_Model.m_Materials.Count);

                ReadMaterialEffect(matDef, effectID);

                m_Model.m_Materials.Add(id, matDef);
            }

            return;
        }

        private void ReadMaterialEffect(ModelBase.MaterialDef matDef, string effectID)
        {
            effect matEffect = this.library_effects.effect.Where(eff => eff.id.Equals(effectID)).ElementAt(0);

            foreach (var profileCommon in matEffect.Items)
            {
                if (profileCommon.technique == null || profileCommon.technique.Item == null) continue;

                common_color_or_texture_type diffuse = null;
                common_float_or_param_type transparency = null;

                if ((profileCommon.technique.Item as effectFx_profile_abstractProfile_COMMONTechniquePhong) != null)
                {
                    diffuse = (profileCommon.technique.Item as effectFx_profile_abstractProfile_COMMONTechniquePhong).diffuse;
                    transparency = (profileCommon.technique.Item as effectFx_profile_abstractProfile_COMMONTechniquePhong).transparency;
                }
                else if ((profileCommon.technique.Item as effectFx_profile_abstractProfile_COMMONTechniqueLambert) != null)
                {
                    diffuse = (profileCommon.technique.Item as effectFx_profile_abstractProfile_COMMONTechniqueLambert).diffuse;
                    transparency = (profileCommon.technique.Item as effectFx_profile_abstractProfile_COMMONTechniqueLambert).transparency;
                }
                else if ((profileCommon.technique.Item as effectFx_profile_abstractProfile_COMMONTechniqueBlinn) != null)
                {
                    diffuse = (profileCommon.technique.Item as effectFx_profile_abstractProfile_COMMONTechniqueBlinn).diffuse;
                    transparency = (profileCommon.technique.Item as effectFx_profile_abstractProfile_COMMONTechniqueBlinn).transparency;
                }
                else if ((profileCommon.technique.Item as effectFx_profile_abstractProfile_COMMONTechniqueConstant) != null)
                {
                    transparency = (profileCommon.technique.Item as effectFx_profile_abstractProfile_COMMONTechniqueConstant).transparency;
                }

                if (diffuse != null && diffuse.Item != null)
                {
                    if ((diffuse.Item as common_color_or_texture_typeColor) != null)
                    {
                        var diffuseColour = diffuse.Item as common_color_or_texture_typeColor;

                        matDef.m_DiffuseColour = Color.FromArgb((int)(diffuseColour.Values[0] * 255f),
                            (int)(diffuseColour.Values[1] * 255f), (int)(diffuseColour.Values[2] * 255f));

                    }
                    else if ((diffuse.Item as common_color_or_texture_typeTexture) != null)
                    {
                        var diffuseTexture = diffuse.Item as common_color_or_texture_typeTexture;

                        string samplerID = diffuseTexture.texture;
                        string surfaceID = null;
                        foreach (var item0 in profileCommon.Items)
                        {
                            var newparam = item0 as common_newparam_type;
                            if (newparam == null || !newparam.sid.Equals(samplerID)) continue;

                            surfaceID = (newparam.Item as fx_sampler2D_common).source;
                            break;
                        }
                        string imageID = null;
                        foreach (var item0 in profileCommon.Items)
                        {
                            var newparam = item0 as common_newparam_type;
                            if (newparam == null || !newparam.sid.Equals(surfaceID)) continue;

                            imageID = (newparam.Item as fx_surface_common).init_from[0].Value;
                            break;
                        }
                        string texName = (string)this.library_images.image.Where(img => img.id.Equals(imageID)).ElementAt(0).Item;
                        if (texName.Contains(m_ModelPath)) 
                            texName.Replace(m_ModelPath, "");
                        AddTexture(texName, matDef);

                        matDef.m_DiffuseColour = Color.White;
                    }
                }

                if (transparency != null && transparency.Item != null)
                {
                    var value = (transparency.Item as common_float_or_param_typeFloat).Value;
                    matDef.m_Opacity = (int)(value * 255f);
                }

                break;
            }
        }

        private void ReadVisualScenes()
        {
            foreach (visual_scene visualScene in this.library_visual_scenes.visual_scene)
            {
                foreach (node nodeNODE in visualScene.node.Where(node0 => node0.type.Equals(NodeType.NODE)))
                {
                    ReadNode(nodeNODE, nodeNODE, false);
                }

                break; // Only going to read first visual_scene
            }
        }

        private void ReadNode(node joint, node parent, bool inSkeleton)
        {
            string id = joint.id;

            Vector3 nodeScale = Vector3.One;
            Vector3 nodeRotation = Vector3.Zero;
            Vector3 nodeTranslation = Vector3.Zero;

            ReadNodeTransformations(joint, ref nodeScale, ref nodeRotation, ref nodeTranslation);

            if (joint.instance_geometry != null && joint.instance_geometry.Length > 0)
            {
                // Making an assumption that <instance_geometry> will never appear within a skeleton, not sure if it can?

                ModelBase.BoneDef rootBone = new ModelBase.BoneDef(id);

                rootBone.SetScale(nodeScale);
                rootBone.SetRotation(nodeRotation);
                rootBone.SetTranslation(nodeTranslation);

                m_Model.m_BoneTree.AddRootBone(rootBone);

                foreach (instance_geometry instanceGeometry in joint.instance_geometry)
                {
                    string geometryID = instanceGeometry.url.Replace("#", "");

                    Dictionary<string, string> bindMaterials = new Dictionary<string, string>();

                    if (instanceGeometry.bind_material != null)
                    {
                        foreach (instance_material instanceMaterial in instanceGeometry.bind_material.technique_common)
                        {
                            bindMaterials.Add(instanceMaterial.symbol, instanceMaterial.target.Replace("#", ""));
                        }
                    }

                    ModelBase.GeometryDef geometry = ReadGeometry(geometryID, id, bindMaterials);
                    rootBone.m_Geometries.Add(geometryID, geometry);
                }
            }
            else if (joint.instance_controller != null && joint.instance_controller.Length > 0)
            {
                // Making an assumption that <instance_controller> will never appear within a skeleton, not sure if it can?

                instance_controller instanceController = joint.instance_controller[0];

                string controllerID = instanceController.url.Replace("#", "");
                if (instanceController.skeleton != null && instanceController.skeleton.Length > 0)
                {

                    /*string skeletonRoot = null;
                    foreach (string skel in instanceController.skeleton)
                    {
                        string skeleton = skel.Replace("#", "");
                        if (skeletonRoot == null) skeletonRoot = skeleton;
                        if (m_Model.m_BoneTree.GetBoneByID(skeleton) != null) continue;

                        ReadSkeleton(skeletonRoot);
                    }
                    // WRONG */

                    string skeletonRoot = instanceController.skeleton[0].Replace("#", "");
                    ReadSkeleton(skeletonRoot);

                    controller cntl = this.library_controllers.controller.Where(cntl0 => cntl0.id.Equals(controllerID)).ElementAt(0);
                    if (cntl.Item as skin != null)
                    {

                        string geometryID = (cntl.Item as skin).source1.Replace("#", "");
                        int[] vertexBoneIDs = ReadSkinController(controllerID, skeletonRoot);

                        Dictionary<string, string> bindMaterials = new Dictionary<string, string>();
                        if (instanceController.bind_material != null)
                        {
                            foreach (instance_material instanceMaterial in instanceController.bind_material.technique_common)
                            {
                                bindMaterials.Add(instanceMaterial.symbol, instanceMaterial.target.Replace("#", ""));
                            }
                        }

                        ModelBase.GeometryDef geomDef = ReadGeometry(geometryID, skeletonRoot, bindMaterials, vertexBoneIDs);
                        m_Model.m_BoneTree.GetBoneByID(skeletonRoot).m_Geometries.Add(geometryID, geomDef);
                    }
                }
            }
            else if (inSkeleton)
            {
                ModelBase.BoneDef boneDef = new ModelBase.BoneDef(joint.id);

                if (joint == parent)
                {
                    m_Model.m_BoneTree.AddRootBone(boneDef);
                }
                else
                {
                    ModelBase.BoneDef parentBone = m_Model.m_BoneTree.GetBoneByID(parent.id);
                    parentBone.AddChild(boneDef);
                }

                boneDef.SetScale(nodeScale);
                boneDef.SetRotation(nodeRotation);
                boneDef.SetTranslation(nodeTranslation);

                if (joint.node1 == null || joint.node1.Length == 0)
                    return;
                foreach (node child in joint.node1)
                {
                    if (child.type.Equals(NodeType.NODE))
                    {
                        Console.WriteLine("Warning: node: " + joint.id + " has a child of type \"NODE\" within a skeleton, failure likely");
                    }

                    ReadNode(child, joint, true);
                }
            }
        }

        private void ReadSkeleton(string rootNodeID)
        {
            node skeletonRoot = null;
            node skeletonRootParent = null;

            foreach (visual_scene visualScene in this.library_visual_scenes.visual_scene)
            {
                foreach (node node0 in visualScene.node)
                {
                    node result = FindNodeInTree(node0, rootNodeID);
                    if (result != null)
                    {
                        skeletonRoot = result;

                        if (result != node0)
                        {
                            bool foundRoot = IsRootNodeOfChild(node0, skeletonRoot);
                            if (foundRoot)
                                skeletonRootParent = node0;
                        }

                        break;
                    }
                }
            }

            ReadNode(skeletonRoot, skeletonRoot, true);

            m_Model.m_BoneTree.GetBoneByID(skeletonRoot.id).CalculateBranchTransformations();
        }

        private int[] ReadSkinController(string id, string skeletonRoot)
        {
            controller controller = this.library_controllers.controller.Where(cntl => cntl.id.Equals(id)).ElementAt(0);

            if (controller.Item as skin == null)
                return null;

            skin skin = controller.Item as skin;

            string geometryID = skin.source1.Replace("#", "");

            string[] jointNames = new string[0];
            Matrix4[] inverseBindPoses = new Matrix4[0];

            foreach (InputLocal input in skin.joints.input)
            {
                if (input.semantic.Equals("JOINT"))
                {
                    string sourceID = input.source.Replace("#", "");
                    source jointNamesSource = skin.source.Where(src => src.id.Equals(sourceID)).ElementAt(0);
                    if (jointNamesSource.Item as Name_array != null)
                    {
                        jointNames = (jointNamesSource.Item as Name_array).Values;
                        for (int i = 0; i < jointNames.Length; i++)
                        {
                            string jointID = FindIDFromSIDInSkeleton(skeletonRoot, jointNames[i]);
                            jointNames[i] = jointID;
                        }
                    }
                    else if (jointNamesSource.Item as IDREF_array != null)
                    {
                        jointNames = (jointNamesSource.Item as IDREF_array).Value.
                            Split(new string[] { " ", "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    }
                }
                else if (input.semantic.Equals("INV_BIND_MATRIX"))
                {
                    string sourceID = input.source.Replace("#", "");
                    source invBindMatrixSource = skin.source.Where(src => src.id.Equals(sourceID)).ElementAt(0);
                    if (invBindMatrixSource.Item as float_array != null)
                    {
                        float[] vals = Array.ConvertAll<double, float>((invBindMatrixSource.Item as float_array).Values, Convert.ToSingle);
                        if (invBindMatrixSource.technique_common != null && invBindMatrixSource.technique_common.accessor != null)
                        {
                            accessor acc = invBindMatrixSource.technique_common.accessor;
                            ulong count = acc.count;
                            ulong stride = acc.stride;
                            inverseBindPoses = new Matrix4[count];

                            int matrixIndex = 0;
                            for (ulong i = 0; i < (count * stride); i += stride, matrixIndex++)
                            {
                                float[] tmp = new float[16];
                                Array.Copy(vals, (int)i, tmp, 0, 16);
                                Matrix4 invBindMatrix = Helper.FloatArrayToMatrix4(tmp);
                                inverseBindPoses[matrixIndex] = invBindMatrix;
                            }
                        }
                    }
                }
            }

            //for (int i = 0; i < jointNames.Length; i++)
            //{
            //    m_Model.m_BoneTree.GetBoneByID(jointNames[i]).m_GlobalInverseTransformation = inverseBindPoses[i];
            //}

            float[] weights = new float[0];
            long offsetJoint = -1;
            long offsetWeight = -1;
            int[] vcount = new int[0];
            int[] v = new int[0];

            int[] vertexBoneIDs = new int[skin.vertex_weights.count];

            foreach (InputLocalOffset input in skin.vertex_weights.input)
            {
                if (input.semantic.Equals("JOINT"))
                {
                    offsetJoint = (long)input.offset;
                }
                else if (input.semantic.Equals("WEIGHT"))
                {
                    offsetWeight = (long)input.offset;

                    string sourceID = input.source.Replace("#", "");
                    source weightSource = skin.source.Where(src => src.id.Equals(sourceID)).ElementAt(0);
                    if (weightSource.Item as float_array != null)
                    {
                        weights = Array.ConvertAll<double, float>((weightSource.Item as float_array).Values, Convert.ToSingle);
                    }
                }
            }

            vcount = Array.ConvertAll<string, int>(
                skin.vertex_weights.vcount.Split(new string[] { " ", "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries),
                Convert.ToInt32);

            v = Array.ConvertAll<string, int>(
                skin.vertex_weights.v.Split(new string[] { " ", "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries),
                Convert.ToInt32);
            int currentVertexIndex = 0;

            // For each vertex, <v> contains n number of joint ID-weight pairs where n is the value specified in <vcount> 
            // for the current vertex. BMD does not support weights, instead each vertex is assigned to a single joint. 
            // When there is more than one weight for a vertex, the vertex is assigned to the joint with the highest weighting:
            // eg. <v>0 0 1 1 2 3 3 5</v>
            // contains the following pairs, (0,0), (1,1), (2,3), (3,5) for a vertex
            // where the first index is an index into the joint names source > bone0, bone1, bone2, bone3
            // and the second index is an index into the weights source > 0.5, 0.25, 0.15, 0.1
            // In this case, the first pair, (0,0) has the highest weighting so this vertex will be assigned to bone0

            for (int i = 0; i < v.Length; )
            {
                int numInfluences = vcount[currentVertexIndex];
                List<Tuple<int, float>> influences = new List<Tuple<int, float>>();

                for (int j = 0; j < numInfluences; j++)
                {
                    int indexJoint = v[i + (j * 2) + offsetJoint];
                    int indexWeight = v[i + (j * 2) + offsetWeight];

                    int mappedBoneID = m_Model.m_BoneTree.GetBoneIndex(jointNames[indexJoint]);
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

                vertexBoneIDs[currentVertexIndex] = highestWeightJoint;

                i += (vcount[currentVertexIndex] * 2);
                currentVertexIndex++;
            }

            return vertexBoneIDs;
        }

        private static void ReadNodeTransformations(node nodeNODE, ref Vector3 nodeScale, ref Vector3 nodeRotation, ref Vector3 nodeTranslation)
        {
            List<Tuple<Vector3, TransformationType>> transforms = new List<Tuple<Vector3, TransformationType>>();
            bool matrixBacked = false;
            for (int i = 0; i < ((nodeNODE.Items != null) ? nodeNODE.Items.Length : 0); i++)
            {
                var item = nodeNODE.Items[i];

                if (nodeNODE.ItemsElementName[i].Equals(ItemsChoiceType2.matrix))
                {
                    Matrix4 nodeMatrix = Helper.DoubleArrayToMatrix4((item as matrix).Values);
                    Helper.DecomposeSRTMatrix1(nodeMatrix, out nodeScale, out nodeRotation, out nodeTranslation);
                    matrixBacked = true;
                }
                else
                {
                    if (nodeNODE.ItemsElementName[i].Equals(ItemsChoiceType2.translate))
                    {
                        var translate = item as TargetableFloat3;
                        Vector3 tmp = new Vector3((float)translate.Values[0], (float)translate.Values[1], (float)translate.Values[2]);
                        transforms.Add(new Tuple<Vector3, TransformationType>(tmp, TransformationType.TranslationXYZ));
                    }
                    else if (nodeNODE.ItemsElementName[i].Equals(ItemsChoiceType2.rotate))
                    {
                        /*
                         * <rotate sid="rotateZ">0 0 1 0</rotate>
                         * <rotate sid="rotateY">0 1 0 45</rotate>
                         * <rotate sid="rotateX">1 0 0 0</rotate>
                         */
                        var rotate = item as rotate;
                        TransformationType rotType = GetRotationType(rotate);
                        Vector3 rot = Vector3.Zero;
                        switch (rotType)
                        {
                            case TransformationType.RotationX:
                                rot = new Vector3((float)(rotate.Values[3] * Helper.Deg2Rad), 0.0f, 0.0f);
                                break;
                            case TransformationType.RotationY:
                                rot = new Vector3(0.0f, (float)(rotate.Values[3] * Helper.Deg2Rad), 0.0f);
                                break;
                            case TransformationType.RotationZ:
                                rot = new Vector3(0.0f, 0.0f, (float)(rotate.Values[3] * Helper.Deg2Rad));
                                break;
                        }
                        transforms.Add(new Tuple<Vector3, TransformationType>(rot, rotType));
                    }
                    else if (nodeNODE.ItemsElementName[i].Equals(ItemsChoiceType2.scale))
                    {
                        var scale = item as TargetableFloat3;
                        Vector3 tmp = new Vector3((float)scale.Values[0], (float)scale.Values[1], (float)scale.Values[2]);
                        transforms.Add(new Tuple<Vector3, TransformationType>(tmp, TransformationType.ScaleXYZ));
                    }
                }
            }
            if (!matrixBacked)
            {
                // If matrix not used, multiply each of the transformations in the reverse of the order they appear.
                // IMPORTANT NOTE: The order must be Scale, Rotation, Translation (appear in file TRzRyRxS)
                List<Tuple<Vector3, TransformationType>> scale =
                    transforms.Where(tran => tran.Item2.Equals(TransformationType.ScaleXYZ)).ToList();
                List<Tuple<Vector3, TransformationType>> rotate =
                    transforms.Where(tran => tran.Item2.Equals(TransformationType.RotationX) ||
                        tran.Item2.Equals(TransformationType.RotationY) || tran.Item2.Equals(TransformationType.RotationZ)).ToList();
                List<Tuple<Vector3, TransformationType>> translate =
                    transforms.Where(tran => tran.Item2.Equals(TransformationType.TranslationXYZ)).ToList();

                if (scale.Count <= 1 && rotate.Count <= 3 && translate.Count <= 1)
                {
                    // Making an assumption that the order is SRT if less than 5 tranformations, if it's greater than 
                    // 5 or there are 5 but not in the order in above condition, use matrix decomposition method.
                    nodeScale = (scale.Count == 1) ? scale[0].Item1 : nodeScale;
                    foreach (Tuple<Vector3, TransformationType> rot in rotate)
                    {
                        if (rot.Item2 == TransformationType.RotationX)
                        {
                            nodeRotation.X = rot.Item1.X;
                        }
                        if (rot.Item2 == TransformationType.RotationY)
                        {
                            nodeRotation.Y = rot.Item1.Y;
                        }
                        if (rot.Item2 == TransformationType.RotationZ)
                        {
                            nodeRotation.Z = rot.Item1.Z;
                        }
                    }
                    nodeTranslation = (translate.Count == 1) ? translate[0].Item1 : nodeTranslation;
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

                    Helper.DecomposeSRTMatrix1(result, out nodeScale, out nodeRotation, out nodeTranslation);
                }
            }
        }

        private static TransformationType GetRotationType(rotate rotate)
        {
            int ind = -1;
            for (int j = 0; j < 3; j++)
            {
                if (rotate.Values[j] == 1.0)
                {
                    ind = j;
                    break;
                }
            }
            switch (ind)
            {
                case 0:
                    return TransformationType.RotationX;
                case 1:
                    return TransformationType.RotationY;
                case 2:
                    return TransformationType.RotationZ;
            }
            return TransformationType.None;
        }

        private ModelBase.GeometryDef ReadGeometry(string id, string boneID, Dictionary<string, string> bindMaterials)
        {
            return ReadGeometry(id, boneID, bindMaterials, null);
        }

        private ModelBase.GeometryDef ReadGeometry(string id, string boneID, Dictionary<string, string> bindMaterials, int[] vertexBoneIDs)
        {
            ModelBase.GeometryDef geomDef = new ModelBase.GeometryDef(id);
            int boneIndex = m_Model.m_BoneTree.GetBoneIndex(boneID);
            geometry geom = library_geometries.geometry.Where(geom0 => geom0.id.Equals(id)).ElementAt(0);

            Dictionary<string, source> sources = new Dictionary<string, source>();

            if (geom.Item as mesh != null)
            {
                mesh geomMesh = geom.Item as mesh;
                Dictionary<string, string> geometryVertices = new Dictionary<string,string>();
                geometryVertices.Add(geomMesh.vertices.id, 
                    geomMesh.vertices.input.Where(input0 => input0.semantic.Equals("POSITION")).ElementAt(0).source.Replace("#", ""));

                foreach (source src in geomMesh.source)
                {
                    string sourceID = src.id;
                    if (src.Item as float_array != null)
                        sources.Add(sourceID, src);
                }
                foreach (var item in geomMesh.Items)
                {
                    if ((item as triangles != null) || (item as polylist != null) || (item as polygons != null))
                    {
                        ModelBase.PolyListDef polyListDef;
                        string material;
                        ulong count;
                        InputLocalOffset[] inputs;
                        int[] vcount;
                        int[] p;

                        if (item as triangles != null)
                        {
                            triangles tris = item as triangles;
                            string matAttr = (tris.material != null) ? tris.material : "default_white";
                            material = (bindMaterials != null && bindMaterials.Count > 0 && bindMaterials.ContainsKey(matAttr)) ?
                                bindMaterials[matAttr] : matAttr;
                            count = tris.count;
                            inputs = tris.input;
                            vcount = new int[count];
                            for (ulong i = 0; i < count; i++) vcount[i] = 3;
                            p = Array.ConvertAll<string, int>
                                (tris.p.Split(new string[] { " ", "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries), Convert.ToInt32);
                        }
                        else if (item as polylist != null)
                        {
                            polylist plist = item as polylist;
                            string matAttr = (plist.material != null) ? plist.material : "default_white";
                            material = (bindMaterials != null && bindMaterials.Count > 0 && bindMaterials.ContainsKey(matAttr)) ?
                                bindMaterials[matAttr] : matAttr;
                            count = plist.count;
                            inputs = plist.input;
                            vcount = Array.ConvertAll<string, int>
                                (plist.vcount.Split(new string[] { " ", "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries), Convert.ToInt32);
                            p = Array.ConvertAll<string, int>
                                (plist.p.Split(new string[] { " ", "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries), Convert.ToInt32);
                        }
                        else
                        {
                            polygons pgons = item as polygons;
                            string matAttr = (pgons.material != null) ? pgons.material : "default_white";
                            material = (bindMaterials != null && bindMaterials.Count > 0 && bindMaterials.ContainsKey(matAttr)) ?
                                bindMaterials[matAttr] : matAttr;
                            count = pgons.count;
                            inputs = pgons.input;
                            vcount = new int[count];
                            p = new int[count];
                            int counter = 0;
                            for (int i = 0; i < pgons.Items.Length; i++)
                            {
                                var element = pgons.Items[i];
                                if (element as string != null)
                                {
                                    int[] tmp = Array.ConvertAll<string, int>
                                        ((element as string).Split(new string[] { " ", "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries), 
                                        Convert.ToInt32);
                                    vcount[i] = tmp.Length / inputs.Length;
                                    Array.Copy(tmp, 0, p, counter, vcount[i]);
                                    counter += tmp.Length;
                                }
                            }
                        }

                        polyListDef = new ModelBase.PolyListDef(id + "." + material, material);

                        // The parent (root) bone should have a list of all materials used by itself and its children
                        if (!m_Model.m_BoneTree.GetBoneByID(boneID).GetRoot().m_MaterialsInBranch.Contains(material))
                            m_Model.m_BoneTree.GetBoneByID(boneID).GetRoot().m_MaterialsInBranch.Add(material);
                        if (!m_Model.m_BoneTree.GetBoneByID(boneID).m_MaterialsInBranch.Contains(material))
                            m_Model.m_BoneTree.GetBoneByID(boneID).m_MaterialsInBranch.Add(material);

                        int inputCount = 0;
                        int vertexOffset = -1, normalOffset = -1, texCoordOffset = -1, colourOffset = -1;
                        string vertexSource = "", normalSource = "", texCoordSource = "", colourSource = "";
                        foreach (InputLocalOffset input in inputs)
                        {
                            if (input.semantic.Equals("VERTEX"))
                            {
                                vertexOffset = (int)input.offset;
                                vertexSource = geometryVertices[input.source.Replace("#", "")];
                            }
                            else if (input.semantic.Equals("NORMAL"))
                            {
                                normalOffset = (int)input.offset;
                                normalSource = input.source.Replace("#", "");
                            }
                            else if (input.semantic.Equals("TEXCOORD"))
                            {
                                texCoordOffset = (int)input.offset;
                                texCoordSource = input.source.Replace("#", "");
                            }
                            else if (input.semantic.Equals("COLOR"))
                            {
                                colourOffset = (int)input.offset;
                                colourSource = input.source.Replace("#", "");
                            }
                        }
                        if (vertexOffset != -1) inputCount++; if (normalOffset != -1) inputCount++;
                        if (texCoordOffset != -1) inputCount++; if (colourOffset != -1) inputCount++;

                        for (ulong pIndex = 0, vcountInd = 0; pIndex < (ulong)p.Length; vcountInd++)
                        {
                            ModelBase.FaceDef faceDef = new ModelBase.FaceDef(vcount[vcountInd]);

                            for (int i = 0; i < faceDef.m_NumVertices; i++)
                            {
                                ModelBase.VertexDef vert = new ModelBase.VertexDef();

                                int vertexIndex = p[pIndex + (ulong)vertexOffset];
                                float[] tmp = GetValueFromFloatArraySource(sources[vertexSource], vertexIndex);
                                vert.m_Position = new Vector3(tmp[0], tmp[1], tmp[2]);

                                if (normalOffset != -1)
                                {
                                    tmp = GetValueFromFloatArraySource(sources[normalSource], p[pIndex + (ulong)normalOffset]);
                                    vert.m_Normal = new Vector3(tmp[0], tmp[1], tmp[2]);
                                }
                                else
                                {
                                    vert.m_Normal = null;
                                }

                                if (texCoordOffset != -1 && m_Model.m_Materials[material].m_HasTextures)
                                {
                                    tmp = GetValueFromFloatArraySource(sources[texCoordSource], p[pIndex + (ulong)texCoordOffset]);
                                    vert.m_TextureCoordinate = new Vector2(tmp[0], tmp[1]);
                                }
                                else
                                {
                                    vert.m_TextureCoordinate = null;
                                }

                                if (colourOffset != -1)
                                {
                                    tmp = GetValueFromFloatArraySource(sources[colourSource], p[pIndex + (ulong)colourOffset]);
                                    vert.m_VertexColour = Color.FromArgb((int)(tmp[0] * 255f),
                                        (int)(tmp[1] * 255f), (int)(tmp[2] * 255f));
                                }
                                else
                                {
                                    vert.m_VertexColour = Color.White;
                                }

                                vert.m_VertexBoneID = (vertexBoneIDs != null) ? vertexBoneIDs[vertexIndex] : boneIndex;

                                faceDef.m_Vertices[i] = vert;

                                pIndex += (ulong)inputCount;
                            }

                            polyListDef.m_Faces.Add(faceDef);
                        }

                        geomDef.m_PolyLists.Add(boneID + "." + material, polyListDef);
                    }
                }
            }

            return geomDef;
        }

        private const float FRAMES_PER_SECOND = 30.0f;
        private const float INTERVAL = 1.0f / FRAMES_PER_SECOND;

        private void ReadAnimations()
        {
            if (this.library_animations == null)
                return;

            if (this.library_animations.animation != null && this.library_animations.animation.Length > 0)
            {
                Dictionary<string, List<Tuple<ModelBase.AnimationDef, TransformationType>>> boneAnimations = 
                    new Dictionary<string, List<Tuple<ModelBase.AnimationDef, TransformationType>>>();

                foreach (animation anim in this.library_animations.animation)
                {
                    string id = anim.id;

                    List<source> animSources = new List<source>();
                    foreach (var item in anim.Items.Where(item0 => item0.GetType().Equals(typeof(source))))
                        animSources.Add(item as source);
                    List<sampler> animSamplers = new List<sampler>();
                    foreach (var item in anim.Items.Where(item0 => item0.GetType().Equals(typeof(sampler))))
                        animSamplers.Add(item as sampler);
                    List<channel> animChannels = new List<channel>();
                    foreach (var item in anim.Items.Where(item0 => item0.GetType().Equals(typeof(channel))))
                        animChannels.Add(item as channel);

                    foreach (channel channel in animChannels)
                    {
                        string samplerID = channel.source.Replace("#", "");
                        sampler channelSampler = animSamplers.Where(samp => samp.id.Equals(samplerID)).ElementAt(0);

                        string inputSourceID =
                            channelSampler.input.Where(input => input.semantic.Equals("INPUT")).ElementAt(0).source.Replace("#", "");
                        string outputSourceID =
                            channelSampler.input.Where(input => input.semantic.Equals("OUTPUT")).ElementAt(0).source.Replace("#", "");

                        float[] inputTime = Array.ConvertAll<double, float>(
                            (animSources.Where(src => src.id.Equals(inputSourceID)).ElementAt(0).Item as float_array).Values,
                            Convert.ToSingle);

                        TransformationType animType = TransformationType.None;

                        string targetPath = channel.target;
                        string targetNodeID = targetPath.Substring(0, targetPath.IndexOf("/"));
                        string targetTransformationSID = targetPath.Substring(targetPath.IndexOf("/") + 1);

                        string targetParam = (targetTransformationSID.IndexOf('.') != -1) ?
                            targetTransformationSID.Substring(targetTransformationSID.IndexOf('.') + 1) : null;
                        targetTransformationSID = (targetParam != null) ? targetTransformationSID.Replace("." + targetParam, "") : 
                            targetTransformationSID;

                        if (!boneAnimations.ContainsKey(targetNodeID))
                            boneAnimations.Add(targetNodeID, new List<Tuple<ModelBase.AnimationDef, TransformationType>>());
                        List<Tuple<ModelBase.AnimationDef, TransformationType>> currentBoneAnimations = boneAnimations[targetNodeID];

                        node targetNode = FindNodeInLibraryVisualScenes(targetNodeID);
                        for (int i = 0; i < targetNode.Items.Length; i++)
                        {
                            var item = targetNode.Items[i];

                            if (item.GetType().Equals(typeof(matrix)) && (item as matrix).sid.Equals(targetTransformationSID))
                            {
                                animType = TransformationType.TransformationMatrix;
                                break;
                            }
                            else if (item.GetType().Equals(typeof(TargetableFloat3)) && (item as TargetableFloat3).sid.Equals(targetTransformationSID))
                            {
                                if (targetNode.ItemsElementName[i].Equals(ItemsChoiceType2.scale))
                                    animType = TransformationType.ScaleXYZ;
                                else if (targetNode.ItemsElementName[i].Equals(ItemsChoiceType2.translate))
                                    animType = TransformationType.TranslationXYZ;
                                break;
                            }
                            else if (item.GetType().Equals(typeof(rotate)) && (item as rotate).sid.Equals(targetTransformationSID))
                            {
                                animType = GetRotationType((item as rotate));
                                break;
                            }
                        }

                        if (animType.Equals(TransformationType.None)) continue;

                        float smallestDifference = GetSmallestDifference(inputTime);
                        float smallestInterval = smallestDifference;

                        float[][] outputs = (!animType.Equals(TransformationType.TransformationMatrix)) ?
                            GetIndividualParamValuesFromNonMatrixInputFloatArray(
                                animSources.Where(src => src.id.Equals(outputSourceID)).ElementAt(0)) :
                            GetIndividualValuesFromMatrixInput(
                                animSources.Where(src => src.id.Equals(outputSourceID)).ElementAt(0));
                        float[][] finalOutputs = new float[outputs.Length][];

                        if (smallestInterval < INTERVAL)
                        {
                            // If imported models uses a lower framerate, "convert" it to use 30fps by interpolating the 
                            // values at double the framerate and then choosing the closest values to the 30fps intervals
                            smallestInterval *= 2f;

                            for (int i = 0; i < outputs.Length; i++)
                            {
                                finalOutputs[i] =
                                    InterpolateFramesAndExtractOneOverFrameRateFPS(inputTime, smallestInterval, outputs[i]);
                            }
                        }
                        else
                        {
                            finalOutputs = outputs;
                        }

                        List<ModelBase.AnimationFrameDef> animFrames = new List<ModelBase.AnimationFrameDef>();
                        for (int i = 0; i < finalOutputs[0].Length; i++)
                        {
                            ModelBase.AnimationFrameDef frame = new ModelBase.AnimationFrameDef();
                            switch (animType)
                            {
                                case TransformationType.TransformationMatrix:
                                    {
                                        float[] vals = new float[] { 
                                            finalOutputs[0][i], finalOutputs[1][i], finalOutputs[2][i], finalOutputs[3][i], 
                                            finalOutputs[4][i], finalOutputs[5][i], finalOutputs[6][i], finalOutputs[7][i], 
                                            finalOutputs[8][i], finalOutputs[9][i], finalOutputs[10][i], finalOutputs[11][i], 
                                            finalOutputs[12][i], finalOutputs[13][i], finalOutputs[14][i], finalOutputs[15][i] };
                                        Matrix4 mat = Helper.FloatArrayToMatrix4(vals);
                                        Vector3 scale, rotation, translation;
                                        Helper.DecomposeSRTMatrix1(mat, out scale, out rotation, out translation);
                                        frame.SetScale(scale);
                                        frame.SetRotation(rotation);
                                        frame.SetTranslation(translation);
                                    }
                                    break;
                                case TransformationType.ScaleXYZ:
                                    {
                                        frame.SetScale(new Vector3(finalOutputs[0][i], finalOutputs[1][i], finalOutputs[2][i]));
                                    }
                                    break;
                                case TransformationType.RotationX:
                                    {
                                        Vector3 tmp = frame.GetRotation();
                                        frame.SetRotation(new Vector3(finalOutputs[0][i], tmp.Y, tmp.Z));
                                    }
                                    break;
                                case TransformationType.RotationY:
                                    {
                                        Vector3 tmp = frame.GetRotation();
                                        frame.SetRotation(new Vector3(tmp.X, finalOutputs[0][i], tmp.Z));
                                    }
                                    break;
                                case TransformationType.RotationZ:
                                    {
                                        Vector3 tmp = frame.GetRotation();
                                        frame.SetRotation(new Vector3(tmp.X, tmp.Y, finalOutputs[0][i]));
                                    }
                                    break;
                                case TransformationType.TranslationXYZ:
                                    {
                                        frame.SetTranslation(new Vector3(finalOutputs[0][i], finalOutputs[1][i], finalOutputs[2][i]));
                                    }
                                    break;
                            }
                            animFrames.Add(frame);
                        }
                        ModelBase.AnimationDef animation = new ModelBase.AnimationDef(id, targetNodeID, animFrames);
                        currentBoneAnimations.Add(new Tuple<ModelBase.AnimationDef, TransformationType>(animation, animType));
                    }

                }

                Dictionary<string, ModelBase.AnimationDef> finalBoneAnimations =
                    new Dictionary<string, ModelBase.AnimationDef>();

                // Merge all individual animations for a particular bone into one animation
                foreach (string boneID in boneAnimations.Keys)
                {
                    List<Tuple<ModelBase.AnimationDef, TransformationType>> currentBoneAnimations = boneAnimations[boneID];

                    if (currentBoneAnimations.Count <= 1)
                    {
                        if (currentBoneAnimations.Count == 1)
                            finalBoneAnimations.Add(boneID, currentBoneAnimations[0].Item1);
                    }
                    else
                    {
                        ModelBase.AnimationDef anim = new ModelBase.AnimationDef(boneID, boneID);
                        int numFrames = currentBoneAnimations[0].Item1.m_AnimationFrames.Count;
                        ModelBase.AnimationFrameDef[] frames = new ModelBase.AnimationFrameDef[numFrames];

                        for (int i = 0; i < numFrames; i++)
                        {
                            frames[i] = new ModelBase.AnimationFrameDef();

                            for (int j = 0; j < currentBoneAnimations.Count; j++)
                            {
                                switch (currentBoneAnimations[j].Item2)
                                {
                                    case TransformationType.TransformationMatrix:
                                        frames[i].SetScale(currentBoneAnimations[j].Item1.m_AnimationFrames[i].GetScale());
                                        frames[i].SetRotation(currentBoneAnimations[j].Item1.m_AnimationFrames[i].GetRotation());
                                        frames[i].SetTranslation(currentBoneAnimations[j].Item1.m_AnimationFrames[i].GetTranslation());
                                        break;
                                    case TransformationType.ScaleXYZ:
                                        frames[i].SetScale(currentBoneAnimations[j].Item1.m_AnimationFrames[i].GetScale());
                                        break;
                                    case TransformationType.ScaleX:
                                        {
                                            Vector3 tmp = frames[i].GetScale();
                                            frames[i].SetScale(new Vector3(
                                                currentBoneAnimations[j].Item1.m_AnimationFrames[i].GetScale().X, tmp.Y, tmp.Z));
                                        }
                                        break;
                                    case TransformationType.ScaleY:
                                        {
                                            Vector3 tmp = frames[i].GetScale();
                                            frames[i].SetScale(new Vector3(
                                                tmp.X, currentBoneAnimations[j].Item1.m_AnimationFrames[i].GetScale().Y, tmp.Z));
                                        }
                                        break;
                                    case TransformationType.ScaleZ:
                                        {
                                            Vector3 tmp = frames[i].GetScale();
                                            frames[i].SetScale(new Vector3(
                                                tmp.X, tmp.Y, currentBoneAnimations[j].Item1.m_AnimationFrames[i].GetScale().Z));
                                        }
                                        break;
                                    case TransformationType.RotationX:
                                        {
                                            Vector3 tmp = frames[i].GetRotation();
                                            frames[i].SetRotation(new Vector3(
                                                currentBoneAnimations[j].Item1.m_AnimationFrames[i].GetRotation().X * Helper.Deg2Rad, 
                                                tmp.Y, tmp.Z));
                                        }
                                        break;
                                    case TransformationType.RotationY:
                                        {
                                            Vector3 tmp = frames[i].GetRotation();
                                            frames[i].SetRotation(new Vector3(
                                                tmp.X, currentBoneAnimations[j].Item1.m_AnimationFrames[i].GetRotation().Y * Helper.Deg2Rad, 
                                                tmp.Z));
                                        }
                                        break;
                                    case TransformationType.RotationZ:
                                        {
                                            Vector3 tmp = frames[i].GetRotation();
                                            frames[i].SetRotation(new Vector3(
                                                tmp.X, tmp.Y, 
                                                currentBoneAnimations[j].Item1.m_AnimationFrames[i].GetRotation().Z * Helper.Deg2Rad));
                                        }
                                        break;
                                    case TransformationType.TranslationXYZ:
                                        frames[i].SetTranslation(currentBoneAnimations[j].Item1.m_AnimationFrames[i].GetTranslation());
                                        break;
                                    case TransformationType.TranslationX:
                                        {
                                            Vector3 tmp = frames[i].GetTranslation();
                                            frames[i].SetTranslation(new Vector3(
                                                currentBoneAnimations[j].Item1.m_AnimationFrames[i].GetTranslation().X, tmp.Y, tmp.Z));
                                        }
                                        break;
                                    case TransformationType.TranslationY:
                                        {
                                            Vector3 tmp = frames[i].GetTranslation();
                                            frames[i].SetTranslation(new Vector3(
                                                tmp.X, currentBoneAnimations[j].Item1.m_AnimationFrames[i].GetTranslation().Y, tmp.Z));
                                        }
                                        break;
                                    case TransformationType.TranslationZ:
                                        {
                                            Vector3 tmp = frames[i].GetTranslation();
                                            frames[i].SetTranslation(new Vector3(
                                                tmp.X, tmp.Y, currentBoneAnimations[j].Item1.m_AnimationFrames[i].GetTranslation().Z));
                                        }
                                        break;
                                }
                            }
                        }

                        anim.m_AnimationFrames = frames.ToList();
                        finalBoneAnimations.Add(anim.m_ID, anim);
                    }
                }

                m_Model.m_Animations = finalBoneAnimations;
            }
        }

        protected float[] InterpolateFramesAndExtractOneOverFrameRateFPS(float[]time, float smallestInterval, float[] outputValues)
        {
            int numFrames = (int)Math.Ceiling(time[time.Length - 1] * FRAMES_PER_SECOND);
            float[] alignedFrameValues = new float[numFrames];

            float lastFrameTime = time[time.Length - 1];
            List<float> interpolatedFrames = new List<float>();

            List<float> interpolatedTime = new List<float>();

            // Interpolate and then convert to (1 / FRAMES_PER_SECOND) frames using closest to keyframe time
            for (int kf = 0; kf < time.Length - 1; kf++)
            {
                float time_diff = time[kf + 1] - time[kf];
                if (time_diff < 0)
                    time_diff *= (-1);
                float value_diff = outputValues[kf + 1] - outputValues[kf];

                int numFramesInGap = (int)(1f / Math.Round((time_diff / smallestInterval), 2));
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
                        interp.Add(outputValues[kf] + (inf * ((value_diff > 0) ? -1 : 1) * (value_diff / numFramesInGap)));
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

        private static float[][] GetIndividualParamValuesFromNonMatrixInputFloatArray(source src)
        {
            float[][] outs = null;

            if (src.technique_common != null && src.technique_common.accessor != null)
            {
                accessor accs = src.technique_common.accessor;
                double[] vals = (src.Item as float_array).Values;
                int numParams = accs.param.Length;

                outs = new float[numParams][];

                for (int i = 0; i < numParams; i++)
                {
                    outs[i] = new float[accs.count];
                }
                int counter = 0;
                for (int i = 0; i < vals.Length; i += numParams, counter++)
                {
                    for (int j = 0; j < numParams; j++)
                    {
                        outs[j][counter] = (float)vals[i + j];
                    }
                }
            }

            return outs;
        }

        private static float[][] GetIndividualValuesFromMatrixInput(source src)
        {
            float[][] outs = null;

            if (src.technique_common != null && src.technique_common.accessor != null)
            {
                accessor accs = src.technique_common.accessor;
                double[] vals = (src.Item as float_array).Values;

                outs = new float[16][];

                for (int i = 0; i < 16; i++)
                {
                    outs[i] = new float[accs.count];
                }
                int counter = 0;
                for (int i = 0; i < vals.Length; i += 16, counter++)
                {
                    for (int j = 0; j < 16; j++)
                    {
                        outs[j][counter] = (float)vals[i + j];
                    }
                }
            }

            return outs;
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

        private node FindNodeInTree(node parent, string nodeID)
        {
            if (parent.id.Equals(nodeID))
            {
                return parent;
            }
            else if (parent.node1 != null && parent.node1.Length > 0)
            {
                foreach (node child in parent.node1)
                {
                    node result = FindNodeInTree(child, nodeID);
                    if (result != null)
                        return result;
                }
            }
            return null;
        }

        private node FindNodeInLibraryVisualScenes(string id)
        {
            foreach (visual_scene visualScene in this.library_visual_scenes.visual_scene)
            {
                foreach (node node0 in visualScene.node)
                {
                    node result = FindNodeInTree(node0, id);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }
            return null;
        }

        private static bool IsRootNodeOfChild(node parent, node child)
        {
            if (parent.node1 == null || parent.node1.Length == 0)
                return false;
            else
            {
                foreach (node n in parent.node1)
                {
                    if (child.id.Equals(n.id))
                        return true;
                    else if (IsRootNodeOfChild(child, n))
                        return true;
                }
            }
            return false;
        }

        private string FindIDFromSIDInSkeleton(string skeletonRoot, string sid)
        {
            string id = null;

            node skeletonRootNode = null;

            foreach (visual_scene visualScene in this.library_visual_scenes.visual_scene)
            {
                foreach (node node0 in visualScene.node)
                {
                    node result = FindNodeInTree(node0, skeletonRoot);
                    if (result != null)
                    {
                        skeletonRootNode = result;
                        break;
                    }
                }
            }

            var queue = new Queue<node>();
            queue.Enqueue(skeletonRootNode);

            while (queue.Count > 0)
            {
                var node = queue.Dequeue();

                if (node.sid != null)
                {
                    if (node.sid.Equals(sid))
                    {
                        id = node.id;
                        break;
                    }
                }
                else
                {
                    if (node.id.Equals(sid))
                    {
                        id = node.id;
                        break;
                    }
                }

                if (node.node1 != null)
                {
                    foreach (var child in node.node1)
                        queue.Enqueue(child);
                }
            }

            return id;
        }


        private float[] GetValueFromFloatArraySource(source src, int index)
        {
            float[] values = new float[0];

            float[] floatArray = new float[0];

            if (src.Item as float_array != null)
            {
                floatArray = Array.ConvertAll<double, float>((src.Item as float_array).Values, Convert.ToSingle);
            }
            if (src.technique_common != null && src.technique_common.accessor != null)
            {
                ulong count = src.technique_common.accessor.count;
                ulong stride = src.technique_common.accessor.stride;

                values = new float[stride];
                Array.Copy(floatArray, (int)((ulong)index * stride), values, 0, (int)stride);
            }

            return values;
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
