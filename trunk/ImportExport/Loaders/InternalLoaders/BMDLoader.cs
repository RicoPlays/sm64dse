/* BMDLoader
 * 
 * Given a BMD object, produces a ModelBase object for use in the Writer classes.
 */ 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using SM64DSe.ImportExport.Writers.InternalWriters;
using System.Windows.Forms;
using OpenTK;

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
            foreach (BMD.ModelChunk mdchunk in m_BMD.m_ModelChunks)
            {
                ModelBase.BoneDef bone = new ModelBase.BoneDef(mdchunk.m_Name);
                bone.SetScale(mdchunk.m_20_12Scale);
                bone.SetRotation(mdchunk.m_4_12Rotation);
                bone.SetTranslation(mdchunk.m_20_12Translation);

                if (mdchunk.m_ParentOffset == 0)
                {
                    m_Model.m_BoneTree.AddRootBone(bone);
                }
                else
                {
                    List<ModelBase.BoneDef> listOfBones = m_Model.m_BoneTree.GetAsList();
                    listOfBones[listOfBones.Count + mdchunk.m_ParentOffset].AddChild(bone);
                }

                ModelBase.GeometryDef geomDef = new ModelBase.GeometryDef("geometry-0");
                bone.m_Geometries.Add(geomDef.m_ID, geomDef);

                foreach (BMD.MaterialGroup matgroup in mdchunk.m_MatGroups)
                {
                    ModelBase.PolyListDef polyListDef = new ModelBase.PolyListDef("polylist-" + matgroup.m_Name, matgroup.m_Name);
                    geomDef.m_PolyLists.Add(polyListDef.m_ID, polyListDef);

                    ModelBase.MaterialDef material = new ModelBase.MaterialDef(matgroup.m_Name, m_Model.m_Materials.Count);
                    material.m_DiffuseColour = matgroup.m_DiffuseColor;
                    material.m_AmbientColour = matgroup.m_AmbientColor;
                    material.m_SpecularColour = matgroup.m_SpecularColor;
                    material.m_EmissionColour = matgroup.m_EmissionColor;
                    material.m_HasTextures = (matgroup.m_Texture != null);
                    if (material.m_HasTextures)
                    {
                        if (!m_Model.m_ConvertedTexturesBitmap.ContainsKey(matgroup.m_Texture.m_TexName))
                            m_Model.m_ConvertedTexturesBitmap.Add(matgroup.m_Texture.m_TexName, ConvertBMDTextureToBitmap(matgroup.m_Texture));

                        material.m_DiffuseMapName = matgroup.m_Texture.m_TexName;
                        material.m_DiffuseMapInMemory = true;
                        material.m_DiffuseMapSize = new Vector2(matgroup.m_Texture.m_Width, matgroup.m_Texture.m_Height);
                    }
                    material.m_Opacity = matgroup.m_Alpha;
                    if ((matgroup.m_PolyAttribs & 0xC0) == 0xC0)
                        material.m_IsDoubleSided = true;

                    if (!m_Model.m_Materials.ContainsKey(material.m_ID))
                        m_Model.m_Materials.Add(material.m_ID, material);

                    bone.m_MaterialsInBranch.Add(matgroup.m_Name);

                    foreach (BMD.VertexList geometry in matgroup.m_Geometry)
                    {
                        uint polyType = geometry.m_PolyType;
                        List<BMD.Vertex> vtxList = geometry.m_VertexList;

                        switch (polyType)
                        {
                            case 0://Separate Triangles
                                {
                                    if (vtxList.Count <= 3)//Just 1 triangle
                                    {
                                        ModelBase.FaceDef face = new ModelBase.FaceDef(3);

                                        face.m_Vertices[0] = new ModelBase.VertexDef(vtxList[0].m_Position, vtxList[0].m_TexCoord,
                                            vtxList[0].m_Normal, vtxList[0].m_Color, (int)matgroup.m_BoneIDs[vtxList[0].m_MatrixID]);
                                        face.m_Vertices[1] = new ModelBase.VertexDef(vtxList[1].m_Position, vtxList[1].m_TexCoord,
                                            vtxList[1].m_Normal, vtxList[1].m_Color, (int)matgroup.m_BoneIDs[vtxList[1].m_MatrixID]);
                                        face.m_Vertices[2] = new ModelBase.VertexDef(vtxList[2].m_Position, vtxList[2].m_TexCoord,
                                            vtxList[2].m_Normal, vtxList[2].m_Color, (int)matgroup.m_BoneIDs[vtxList[2].m_MatrixID]);

                                        polyListDef.m_Faces.Add(face);
                                    }
                                    else if (vtxList.Count > 3 && (float)vtxList.Count % 3 == 0.0f)//Eg. 9 vertices in 3 triangles
                                    {
                                        int numFaces = vtxList.Count / 3;
                                        for (int a = 0, b = 0; a < numFaces; a++, b = b + 3)
                                        {
                                            ModelBase.FaceDef face = new ModelBase.FaceDef(3);

                                            face.m_Vertices[0] = new ModelBase.VertexDef(vtxList[b + 0].m_Position, vtxList[b + 0].m_TexCoord,
                                                vtxList[b + 0].m_Normal, vtxList[b + 0].m_Color, (int)matgroup.m_BoneIDs[vtxList[b + 0].m_MatrixID]);
                                            face.m_Vertices[1] = new ModelBase.VertexDef(vtxList[b + 1].m_Position, vtxList[b + 1].m_TexCoord,
                                                vtxList[b + 1].m_Normal, vtxList[b + 1].m_Color, (int)matgroup.m_BoneIDs[vtxList[b + 1].m_MatrixID]);
                                            face.m_Vertices[2] = new ModelBase.VertexDef(vtxList[b + 2].m_Position, vtxList[b + 2].m_TexCoord,
                                                vtxList[b + 2].m_Normal, vtxList[b + 2].m_Color, (int)matgroup.m_BoneIDs[vtxList[b + 2].m_MatrixID]);

                                            polyListDef.m_Faces.Add(face);
                                        }
                                    }
                                    break;
                                }
                            case 1://Separate Quadrilaterals
                                {
                                    if (vtxList.Count <= 4)//Just 1 quadrilateral
                                    {
                                        ModelBase.FaceDef face = new ModelBase.FaceDef(4);

                                        face.m_Vertices[0] = new ModelBase.VertexDef(vtxList[0].m_Position, vtxList[0].m_TexCoord,
                                            vtxList[0].m_Normal, vtxList[0].m_Color, (int)matgroup.m_BoneIDs[vtxList[0].m_MatrixID]);
                                        face.m_Vertices[1] = new ModelBase.VertexDef(vtxList[1].m_Position, vtxList[1].m_TexCoord,
                                            vtxList[1].m_Normal, vtxList[1].m_Color, (int)matgroup.m_BoneIDs[vtxList[1].m_MatrixID]);
                                        face.m_Vertices[2] = new ModelBase.VertexDef(vtxList[2].m_Position, vtxList[2].m_TexCoord,
                                            vtxList[2].m_Normal, vtxList[2].m_Color, (int)matgroup.m_BoneIDs[vtxList[2].m_MatrixID]);
                                        face.m_Vertices[3] = new ModelBase.VertexDef(vtxList[3].m_Position, vtxList[3].m_TexCoord,
                                            vtxList[3].m_Normal, vtxList[3].m_Color, (int)matgroup.m_BoneIDs[vtxList[3].m_MatrixID]);

                                        polyListDef.m_Faces.Add(face);
                                    }
                                    else if (vtxList.Count > 4 && (float)vtxList.Count % 4 == 0.0f)//Eg. 8 vertices in 2 quadrilaterals
                                    {
                                        int numFaces = vtxList.Count / 4;
                                        for (int a = 0, b = 0; a < numFaces; a++, b = b + 4)
                                        {
                                            ModelBase.FaceDef face = new ModelBase.FaceDef(4);

                                            face.m_Vertices[0] = new ModelBase.VertexDef(vtxList[b + 0].m_Position, vtxList[b + 0].m_TexCoord,
                                                vtxList[b + 0].m_Normal, vtxList[b + 0].m_Color, (int)matgroup.m_BoneIDs[vtxList[b + 0].m_MatrixID]);
                                            face.m_Vertices[1] = new ModelBase.VertexDef(vtxList[b + 1].m_Position, vtxList[b + 1].m_TexCoord,
                                                vtxList[b + 1].m_Normal, vtxList[b + 1].m_Color, (int)matgroup.m_BoneIDs[vtxList[b + 1].m_MatrixID]);
                                            face.m_Vertices[2] = new ModelBase.VertexDef(vtxList[b + 2].m_Position, vtxList[b + 2].m_TexCoord,
                                                vtxList[b + 2].m_Normal, vtxList[b + 2].m_Color, (int)matgroup.m_BoneIDs[vtxList[b + 2].m_MatrixID]);
                                            face.m_Vertices[3] = new ModelBase.VertexDef(vtxList[b + 3].m_Position, vtxList[b + 3].m_TexCoord,
                                                vtxList[b + 3].m_Normal, vtxList[b + 3].m_Color, (int)matgroup.m_BoneIDs[vtxList[b + 3].m_MatrixID]);

                                            polyListDef.m_Faces.Add(face);
                                        }
                                    }
                                    break;
                                }
                            case 2://Triangle Strips
                                {
                                    //3+(N-1) vertices per N triangles
                                    //(N-3)+1 Triangles per N Vertices
                                    int numFaces = vtxList.Count - 2;
                                    if (vtxList.Count < 3)//Should never be
                                        break;
                                    //Convert all faces with more than 3 vertices to ones with only 3
                                    for (int n = 0; n < numFaces; n++)
                                    {
                                        if (n % 2 == 0)
                                        {
                                            ModelBase.FaceDef face = new ModelBase.FaceDef(3);

                                            face.m_Vertices[0] = new ModelBase.VertexDef(vtxList[n + 0].m_Position, vtxList[n + 0].m_TexCoord,
                                                vtxList[n + 0].m_Normal, vtxList[n + 0].m_Color, (int)matgroup.m_BoneIDs[vtxList[n + 0].m_MatrixID]);
                                            face.m_Vertices[1] = new ModelBase.VertexDef(vtxList[n + 1].m_Position, vtxList[n + 1].m_TexCoord,
                                                vtxList[n + 1].m_Normal, vtxList[n + 1].m_Color, (int)matgroup.m_BoneIDs[vtxList[n + 1].m_MatrixID]);
                                            face.m_Vertices[2] = new ModelBase.VertexDef(vtxList[n + 2].m_Position, vtxList[n + 2].m_TexCoord,
                                                vtxList[n + 2].m_Normal, vtxList[n + 2].m_Color, (int)matgroup.m_BoneIDs[vtxList[n + 2].m_MatrixID]);

                                            polyListDef.m_Faces.Add(face);
                                        }
                                        else
                                        {
                                            ModelBase.FaceDef face = new ModelBase.FaceDef(3);

                                            face.m_Vertices[0] = new ModelBase.VertexDef(vtxList[n + 2].m_Position, vtxList[n + 2].m_TexCoord,
                                                vtxList[n + 2].m_Normal, vtxList[n + 2].m_Color, (int)matgroup.m_BoneIDs[vtxList[n + 2].m_MatrixID]);
                                            face.m_Vertices[1] = new ModelBase.VertexDef(vtxList[n + 1].m_Position, vtxList[n + 1].m_TexCoord,
                                                vtxList[n + 2].m_Normal, vtxList[n + 1].m_Color, (int)matgroup.m_BoneIDs[vtxList[n + 1].m_MatrixID]);
                                            face.m_Vertices[2] = new ModelBase.VertexDef(vtxList[n + 0].m_Position, vtxList[n + 0].m_TexCoord,
                                                vtxList[n + 0].m_Normal, vtxList[n + 0].m_Color, (int)matgroup.m_BoneIDs[vtxList[n + 0].m_MatrixID]);

                                            polyListDef.m_Faces.Add(face);
                                        }
                                        //Because of how normals are defined in triangle strips, every 2nd triangle is clockwise, whereas all others are anti-clockwise
                                    }
                                    break;
                                }
                            case 3://Quadrilateral Strips
                                {
                                    //4+(N-1)*2 vertices per N quads
                                    //((N-4)/2) + 1 Quads. per N Vertices
                                    int numFaces = ((vtxList.Count - 4) / 2) + 1;
                                    if (vtxList.Count < 4)//Should never be
                                        break;
                                    for (int n = 0, p = 0; n < numFaces; n++, p = p + 2)
                                    {
                                        ModelBase.FaceDef face = new ModelBase.FaceDef(4);

                                        face.m_Vertices[0] = new ModelBase.VertexDef(vtxList[p + 0].m_Position, vtxList[p + 0].m_TexCoord,
                                            vtxList[p + 0].m_Normal, vtxList[p + 0].m_Color, (int)matgroup.m_BoneIDs[vtxList[p + 0].m_MatrixID]);
                                        face.m_Vertices[1] = new ModelBase.VertexDef(vtxList[p + 1].m_Position, vtxList[p + 1].m_TexCoord,
                                            vtxList[p + 1].m_Normal, vtxList[p + 1].m_Color, (int)matgroup.m_BoneIDs[vtxList[p + 1].m_MatrixID]);
                                        face.m_Vertices[2] = new ModelBase.VertexDef(vtxList[p + 3].m_Position, vtxList[p + 3].m_TexCoord,
                                            vtxList[p + 2].m_Normal, vtxList[p + 3].m_Color, (int)matgroup.m_BoneIDs[vtxList[p + 3].m_MatrixID]);
                                        face.m_Vertices[3] = new ModelBase.VertexDef(vtxList[p + 2].m_Position, vtxList[p + 2].m_TexCoord,
                                            vtxList[p + 3].m_Normal, vtxList[p + 2].m_Color, (int)matgroup.m_BoneIDs[vtxList[p + 2].m_MatrixID]);

                                        polyListDef.m_Faces.Add(face);
                                    }
                                    break;
                                }
                            default: MessageBox.Show("Unknown polygon type."); break;
                        }//End polyType switch
                    }
                }

                bone.CalculateBranchTransformations();
            }

            m_Model.ApplyTransformations();

            return m_Model;
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
