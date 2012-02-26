using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;


namespace SM64DSe
{
    public partial class LevelEditorForm : Form
    {
        private static Color k_SelectionColor = Color.FromArgb(255, 255, 128);
        private static Color k_HoverColor = Color.FromArgb(255, 255, 192);

        public Dictionary<ushort, bool> m_ObjAvailable;
        private void GetObjectsAvailable()
        {
            m_ObjAvailable.Clear();

            for (int i = 0; i < 326; i++)
            {
                ObjectDatabase.ObjectInfo objinfo = ObjectDatabase.m_ObjectInfo[i];

                bool available = true;
                if (objinfo.m_BankRequirement == 1)
                {
                    if (m_LevelSettings.ObjectBanks[objinfo.m_NumBank] != objinfo.m_BankSetting)
                        available = false;
                }
                else if (objinfo.m_BankRequirement == 2)
                    available = false;

                m_ObjAvailable.Add((ushort)i, available);
            }

            m_ObjAvailable.Add(511, true);
        }

        private void ClampRotation(ref float val, float twopi)
        {
	        if (val > twopi)
	        {
		        while (val > twopi)
			        val -= twopi;
	        }
	        else if (val < -twopi)
	        {
		        while (val < -twopi)
			        val += twopi;
	        }
        }

        private bool IsSimpleObject(ushort id)
        {
            switch (id)
            {
                case 37: // COIN
                case 38: // RED_COIN
                case 39: // BLUE_COIN
                case 41: // TREE
                case 53: // SBIRD
                case 54: // FISH
                case 55: // BUTTERFLY
                case 60: // STAR_CAMERA
                case 63: // STARBASE
                case 269: // BLK_OKINOKO_TAG
                case 270: // BLK_SKINOKO_TAG
                case 271: // BLK_GNSHELL_TAG
                case 272: // BLK_SLVSTAR_TAG
                case 323: // SET_SE
                case 324: // MUGEN_BGM
                case 511: // minimap change
                    return true;
                default: return false;
            }
        }

        private int m_EntranceID = 0;
        private void ReadObjectTable(uint offset, int area)
        {
            AddPointer(offset + 0x4);
            uint subtbl_num = m_Overlay.Read32(offset);
            uint subtbl_offset = m_Overlay.ReadPointer(offset + 0x4);
            for (uint st = 0; st < subtbl_num; st++)
            {
                uint curoffset = subtbl_offset + (st * 8);
                AddPointer(curoffset + 0x4);

                byte flags = m_Overlay.Read8(curoffset);
                byte entries_num = m_Overlay.Read8(curoffset + 0x1);
                uint entries_offset = m_Overlay.ReadPointer(curoffset + 0x4);

                byte type = (byte)(flags & 0x1F);
                byte layer = (byte)(flags >> 5);

                switch (type)
                {
                    case 0:
                        for (byte e = 0; e < entries_num; e++)
                        {
                            LevelObject obj = new StandardObject(m_Overlay, (uint)(entries_offset + (e * 16)), m_LevelObjects.Count, layer, area);
                            m_LevelObjects.Add(obj.m_UniqueID, obj);
                        }
                        break;

                    case 1:
                        for (byte e = 0; e < entries_num; e++)
                        {
                            LevelObject obj = new EntranceObject(m_Overlay, (uint)(entries_offset + (e * 16)), m_LevelObjects.Count, layer, m_EntranceID++);
                            m_LevelObjects.Add(obj.m_UniqueID, obj);
                        }
                        break;

                    /*case 2:
                        for (byte e = 0; e < entries_num; e++)
                        {
                            LevelObject obj = new PathPointObject(m_Overlay, (uint)(entries_offset + (e * 6)), m_LevelObjects.Count);
                            m_LevelObjects.Add(obj.m_UniqueID, obj);
                        }
                        break;

                    case 3:
                        for (byte e = 0; e < entries_num; e++)
                        {
                            LevelObject obj = new PathObject(m_Overlay, (uint)(entries_offset + (e * 6)), m_LevelObjects.Count);
                            m_LevelObjects.Add(obj.m_UniqueID, obj);
                        }
                        break;*/

                    case 4:
                        for (byte e = 0; e < entries_num; e++)
                        {
                            LevelObject obj = new ViewObject(m_Overlay, (uint)(entries_offset + (e * 14)), m_LevelObjects.Count);
                            m_LevelObjects.Add(obj.m_UniqueID, obj);
                        }
                        break;

                    case 5:
                        for (byte e = 0; e < entries_num; e++)
                        {
                            LevelObject obj = new SimpleObject(m_Overlay, (uint)(entries_offset + (e * 8)), m_LevelObjects.Count, layer, area);
                            m_LevelObjects.Add(obj.m_UniqueID, obj);
                        }
                        break;

                    case 6:
                        for (byte e = 0; e < entries_num; e++)
                        {
                            LevelObject obj = new TpSrcObject(m_Overlay, (uint)(entries_offset + (e * 8)), m_LevelObjects.Count, layer);
                            m_LevelObjects.Add(obj.m_UniqueID, obj);
                        }
                        break;

                    case 7:
                        for (byte e = 0; e < entries_num; e++)
                        {
                            LevelObject obj = new TpDstObject(m_Overlay, (uint)(entries_offset + (e * 8)), m_LevelObjects.Count, layer);
                            m_LevelObjects.Add(obj.m_UniqueID, obj);
                        }
                        break;

                    case 8:
                        // fog todo
                        //MessageBox.Show(string.Format("{0:X8}{1:X8}", m_Overlay.Read32(entries_offset), m_Overlay.Read32(entries_offset + 4)));
                        break;

                    case 9:
                        for (byte e = 0; e < entries_num; e++)
                        {
                            LevelObject obj = new DoorObject(m_Overlay, (uint)(entries_offset + (e * 12)), m_LevelObjects.Count, layer);
                            m_LevelObjects.Add(obj.m_UniqueID, obj);
                        }
                        break;

                    case 10:
                        for (byte e = 0; e < entries_num; e++)
                        {
                            LevelObject obj = new ExitObject(m_Overlay, (uint)(entries_offset + (e * 14)), m_LevelObjects.Count, layer);
                            m_LevelObjects.Add(obj.m_UniqueID, obj);
                        }
                        break;

                    case 11:
                        m_MinimapFileIDsOffset = entries_offset;
                        m_MinimapFileIDs = new ushort[entries_num];
                        for (byte e = 0; e < entries_num; e++)
                            m_MinimapFileIDs[e] = m_Overlay.Read16((uint)(entries_offset + (e * 2)));
                        break;

                    case 12:
                        // per-area minimap scale factors todo
                        {
                           /* string lol = "OBJECT 12:  ";
                            for (byte e = 0; e < entries_num; e++)
                            {
                                lol += string.Format("{0:X4} | ", m_Overlay.Read16((uint)(entries_offset + (e * 2))));
                                m_Overlay.Write16((uint)(entries_offset + (e * 2)), 0x0898);
                            }
                            MessageBox.Show(lol.Substring(0, lol.Length - 3));*/
                        }
                        break;

                    case 14:
                        // ??? todo
                        {
                         /*   string lol = "OBJECT 14:  ";
                            for (byte e = 0; e < entries_num; e++)
                                lol += string.Format("{0:X8} | ", m_Overlay.Read32((uint)(entries_offset + (e * 4))));
                            MessageBox.Show(lol.Substring(0, lol.Length - 3));*/
                            //m_Overlay.Write32(entries_offset, 0x03030304);
                            //m_Overlay.Write32(entries_offset, 0xFFFFFFFF);
                        }
                        break;
                }
            }
        }

        private void ReadTextureAnimations(uint offset, int area)
        {
            AddPointer(offset + 0x4);
            AddPointer(offset + 0x8);
            AddPointer(offset + 0xC);
            AddPointer(offset + 0x14);

            uint numanim = m_Overlay.Read32(offset + 0x10);
            uint animaddr = m_Overlay.ReadPointer(offset + 0x14);
            for (uint i = 0; i < numanim; i++)
            {
                AddPointer(animaddr + 0x4);

                m_TexAnims[area].Add(new LevelTexAnim(m_Overlay, offset, animaddr, m_TexAnims[area].Count, area));

                animaddr += 0x1C;
            }
        }

        private void LoadLevelData()
        {
            m_PointerList = new List<PointerReference>();
            AddPointer(0x60);
            AddPointer(0x64);
            AddPointer(0x70);

            m_LevelSettings = new LevelSettings(m_Overlay);

            // read object lists

            m_NumAreas = m_Overlay.Read8(0x74);
            uint objlistptr = m_Overlay.ReadPointer(0x70);

            m_LevelObjects = new Dictionary<uint, LevelObject>();
            m_TexAnims = new List<LevelTexAnim>[m_NumAreas];
            for (int a = 0; a < m_NumAreas; a++)
            {
                m_TexAnims[a] = new List<LevelTexAnim>();
            }

            ReadObjectTable(m_Overlay.ReadPointer(0x64), 0);
            for (byte a = 0; a < m_NumAreas; a++)
            {
                // read object tables
                uint addr = (uint)(objlistptr + (a * 12));
                if (m_Overlay.Read32(addr) != 0)
                {
                    AddPointer(addr);
                    ReadObjectTable(m_Overlay.ReadPointer(addr), a);
                }

                // read texture animation
                addr += 4;
                if (m_Overlay.Read32(addr) != 0)
                {
                    AddPointer(addr);
                    ReadTextureAnimations(m_Overlay.ReadPointer(addr), a);
                }
            }

            m_LevelModel = null;
            m_LevelCollMap = new KCL(m_ROM.GetFileFromInternalID(m_LevelSettings.KCLFileID));
            //MessageBox.Show(KCL.OctreeNode.maxkids.ToString());

            m_SkyboxModel = null;

            //m_LevelModified = false;
            m_ObjAvailable = new Dictionary<ushort, bool>();
            GetObjectsAvailable();
        }


        public LevelEditorForm(NitroROM rom, int levelid)
        {
            InitializeComponent();

            // remove debug controls if needed
            if (!Program.AppVersion.ToLowerInvariant().Contains("private beta"))
            {
                btnDumpOverlay.Visible = false;
                btnEditPaths.Visible = false;
            }

            this.Text = string.Format("[{0}] {1} - {2} {3}", levelid, Strings.LevelNames[levelid], Program.AppTitle, Program.AppVersion);

            m_MouseDown = MouseButtons.None;

            m_ROM = rom;
            LevelID = levelid;

            m_Overlay = new NitroOverlay(m_ROM, m_ROM.GetLevelOverlayID(LevelID));

            // dump overlay
            //System.IO.File.WriteAllBytes(string.Format("level{0}_overlay.bin", LevelID), m_Overlay.m_Data);

            m_GLLoaded = false;

            btnStar1.Checked = true;
            btnStarAll.Checked = true;
            m_ShowCommonLayer = true;
            m_AuxLayerNum = 1;
            btnEditObjects.Checked = true;
            m_EditMode = 1;

            m_Hovered = 0xFFFFFFFF; 
            m_LastHovered = 0xFFFFFFFF;
            m_HoveredObject = null;
            m_Selected = 0xFFFFFFFF;
            m_LastSelected = 0xFFFFFFFF;
            m_SelectedObject = null;
            m_LastClicked = 0xFFFFFFFF;
            m_ObjectBeingPlaced = 0xFFFF;
            m_ShiftPressed = false;

            slStatusLabel.Text = "Ready";
        }


        public void UpdateSkybox(int id)
        {
            if (id == -1)
                id = m_LevelSettings.Background;

            if (m_SkyboxModel != null)
                ModelCache.RemoveModel(m_SkyboxModel);
            if (m_SkyboxDL != 0)
                GL.DeleteLists(m_SkyboxDL, 1);

            if (id > 0)
            {
                string filename = String.Format("data/vrbox/vr{0:D2}.bmd", id);
                m_SkyboxModel = ModelCache.GetModel(filename);

                m_SkyboxDL = GL.GenLists(1);
                m_SkyboxModel.PrepareToRender();
                GL.NewList(m_SkyboxDL, ListMode.Compile);
                m_SkyboxModel.Render(0.01f);
                GL.EndList();
            }
            else
            {
                m_SkyboxModel = null;
                m_SkyboxDL = 0;
            }

            glLevelView.Refresh();
        }

        public void UpdateLevelModel()
        {
            if (m_LevelModel != null)
                m_LevelModel.Release();

            m_LevelModel = new BMD(m_ROM.GetFileFromInternalID(m_LevelSettings.BMDFileID));
            m_LevelModel.PrepareToRender();

            if (m_LevelModelDLs == null)
                m_LevelModelDLs = new int[m_LevelModel.m_ModelChunks.Length, 3];

            for (int c = 0; c < m_LevelModel.m_ModelChunks.Length; c++)
            {
                m_LevelModelDLs[c, 0] = GL.GenLists(1);
                GL.NewList(m_LevelModelDLs[c, 0], ListMode.Compile);
                m_LevelModel.m_ModelChunks[c].Render(RenderMode.Opaque, 1.0f);
                GL.EndList();

                m_LevelModelDLs[c, 1] = GL.GenLists(1);
                GL.NewList(m_LevelModelDLs[c, 1], ListMode.Compile);
                m_LevelModel.m_ModelChunks[c].Render(RenderMode.Translucent, 1.0f);
                GL.EndList();

                m_LevelModelDLs[c, 2] = GL.GenLists(1);
                GL.NewList(m_LevelModelDLs[c, 2], ListMode.Compile);
                m_LevelModel.m_ModelChunks[c].Render(RenderMode.Picking, 1.0f);
                GL.EndList();
            }

            glLevelView.Refresh();
        }

        private void UpdateCamera()
        {
            Vector3 up;

			if (Math.Cos(m_CamRotation.Y) < 0)
			{
				m_UpsideDown = true;
				up = new Vector3(0.0f, -1.0f, 0.0f);
			}
			else
			{
				m_UpsideDown = false;
				up = new Vector3(0.0f, 1.0f, 0.0f);
			}
				
			m_CamPosition.X = m_CamDistance * (float)Math.Cos(m_CamRotation.X) * (float)Math.Cos(m_CamRotation.Y);
			m_CamPosition.Y = m_CamDistance * (float)Math.Sin(m_CamRotation.Y);
			m_CamPosition.Z = m_CamDistance * (float)Math.Sin(m_CamRotation.X) * (float)Math.Cos(m_CamRotation.Y);

			Vector3 skybox_target;
			skybox_target.X = -(float)Math.Cos(m_CamRotation.X) * (float)Math.Cos(m_CamRotation.Y);
			skybox_target.Y = -(float)Math.Sin(m_CamRotation.Y);
			skybox_target.Z = -(float)Math.Sin(m_CamRotation.X) * (float)Math.Cos(m_CamRotation.Y);

			Vector3.Add(ref m_CamPosition, ref m_CamTarget, out m_CamPosition);
            
			m_CamMatrix = Matrix4.LookAt(m_CamPosition, m_CamTarget, up);
			m_SkyboxMatrix = Matrix4.LookAt(Vector3.Zero, skybox_target, up);
        }

        private void RenderObjectLists(RenderMode mode, int layer)
        {
            int t = 0;
            switch (mode)
            {
                case RenderMode.Opaque: t = 0; break;
                case RenderMode.Translucent: t = 1; break;
                case RenderMode.Picking: t = 2; break;
            }
            
            if (m_ObjectDLs[layer, t] == 0)
                m_ObjectDLs[layer, t] = GL.GenLists(1);

            GL.NewList(m_ObjectDLs[layer, t], ListMode.Compile);

            if (mode == RenderMode.Picking)
            {
                IEnumerable<LevelObject> objects = m_LevelObjects.Values.Where(obj => (obj.m_UniqueID >> 28) == m_EditMode && obj.m_Layer == layer);
                foreach (LevelObject obj in objects)
                {
                    GL.Color4(Color.FromArgb((int)obj.m_UniqueID));
                    obj.Render(mode);
                }
            }
            else
            {
                IEnumerable<LevelObject> objects = m_LevelObjects.Values.Where(obj => obj.m_Layer == layer);
                foreach (LevelObject obj in objects)
                    obj.Render(mode);
            }

            GL.EndList();
        }

        private void RenderObjectHilite(LevelObject obj, Color color, int dlist)
        {
            GL.NewList(dlist, ListMode.Compile);
            GL.PushAttrib(AttribMask.AllAttribBits);

            GL.Disable(EnableCap.Lighting);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            GL.ColorMask(false, false, false, false);
            GL.Enable(EnableCap.StencilTest);
            GL.StencilMask(0x3);
            GL.StencilFunc(StencilFunction.Always, 0x1, 0x3);
            GL.StencilOp(StencilOp.Replace, StencilOp.Replace, StencilOp.Replace);
            obj.Render(RenderMode.Picking);

            GL.ColorMask(true, true, true, true);
            GL.Enable(EnableCap.PolygonOffsetFill);
            GL.PolygonOffset(-1.0f, -1.0f);
            GL.StencilFunc(StencilFunction.Equal, 0x1, 0x3);
            GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Incr);
            GL.Color4(Color.FromArgb(100, color));
            obj.Render(RenderMode.Picking);

            GL.Disable(EnableCap.PolygonOffsetFill);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            GL.LineWidth(3.0f);
            GL.StencilFunc(StencilFunction.Equal, 0x0, 0x3);
            GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Replace);
            GL.DepthFunc(DepthFunction.Always);
            GL.Color4(color);
            obj.Render(RenderMode.Picking);

            // would be faster, but doesn't quite work right
            // (highlights overlapping glitch)
            /*GL.Enable(EnableCap.StencilTest);
            GL.Enable(EnableCap.PolygonOffsetFill);
            GL.PolygonOffset(-1f, 1f);
            GL.StencilFunc(StencilFunction.Always, 0x1, 0x1);
            GL.StencilOp(StencilOp.Replace, StencilOp.Replace, StencilOp.Replace);
            GL.Color4(Color.FromArgb(100, color));
            obj.Render(RenderMode.Picking);

            GL.Disable(EnableCap.PolygonOffsetFill);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            GL.LineWidth(3.0f);
            GL.StencilFunc(StencilFunction.Equal, 0x0, 0x1);
            GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Keep);
            GL.DepthFunc(DepthFunction.Always);
            GL.Color4(color);
            obj.Render(RenderMode.Picking);*/

            GL.PopAttrib();
            GL.EndList();
        }

        private void RefreshObjects(int layer)
        {
            RenderObjectLists(RenderMode.Opaque, layer);
            RenderObjectLists(RenderMode.Translucent, layer);
            RenderObjectLists(RenderMode.Picking, layer);
            if (m_SelectedObject != null)
            {
                RenderObjectHilite(m_SelectedObject, k_SelectionColor, m_SelectHiliteDL);
                RenderObjectHilite(m_SelectedObject, k_HoverColor, m_HoverHiliteDL);
            }
            glLevelView.Refresh();
        }


        private void PopulateObjectList()
        {
            tvObjectList.Nodes.Clear();
            foreach (ToolStripItem ctl in tsEditActions.Items)
                ctl.Visible = false;

            switch (m_EditMode)
            {
                case 0:
                    {
                        btnImportModel.Visible = true;// Properties.Settings.Default.lolhax;
                        //btnExportModel.Visible = true;
                        //btnAddTexAnim.Visible = true;
                        //btnRemoveSel.Visible = true;

                        /*TreeNode node0 = tvObjectList.Nodes.Add("Texture animations");
                        for (int a = 0; a < m_TexAnims.Length; a++)
                            foreach (LevelTexAnim anim in m_TexAnims[a])
                                node0.Nodes.Add(anim.m_UniqueID.ToString("X8"), anim.GetDescription()).Tag = anim.m_UniqueID;*/
                        tvObjectList.Nodes.Add("lol", "(nothing available for now)");
                    }
                    break;

                case 1:
                    {
                        btnAddObject.Visible = true;
                        btnRemoveSel.Visible = true;

                        TreeNode node0 = tvObjectList.Nodes.Add("parent0", "Objects");

                        IEnumerable<LevelObject> objects = m_LevelObjects.Values.Where(obj => 
                            ((m_ShowCommonLayer && obj.m_Layer == 0) || (m_AuxLayerNum != 0 && obj.m_Layer == m_AuxLayerNum)) &&
                            (obj.m_UniqueID >> 28) == 1);
                        foreach (LevelObject obj in objects)
                            node0.Nodes.Add(obj.m_UniqueID.ToString("X8"), obj.GetDescription()).Tag = obj.m_UniqueID;
                    }
                    break;

                case 2:
                    {
                        btnAddWarp.Visible = true;
                        btnRemoveSel.Visible = true;

                        TreeNode node0 = tvObjectList.Nodes.Add("parent0", "Entrances");
                        TreeNode node1 = tvObjectList.Nodes.Add("parent1", "Exits");
                        TreeNode node2 = tvObjectList.Nodes.Add("parent2", "Doors");
                        TreeNode node3 = tvObjectList.Nodes.Add("parent3", "Teleport sources");
                        TreeNode node4 = tvObjectList.Nodes.Add("parent4", "Teleport destinations");

                        IEnumerable<LevelObject> objects = m_LevelObjects.Values.Where(obj =>
                            ((m_ShowCommonLayer && obj.m_Layer == 0) || (m_AuxLayerNum != 0 && obj.m_Layer == m_AuxLayerNum)) &&
                            (obj.m_UniqueID >> 28) == 2);
                        foreach (LevelObject obj in objects)
                        {
                            switch (obj.m_Type)
                            {
                                case 1: node0.Nodes.Add(obj.m_UniqueID.ToString("X8"), obj.GetDescription()).Tag = obj.m_UniqueID; break;
                                case 10: node1.Nodes.Add(obj.m_UniqueID.ToString("X8"), obj.GetDescription()).Tag = obj.m_UniqueID; break;
                                case 9: node2.Nodes.Add(obj.m_UniqueID.ToString("X8"), obj.GetDescription()).Tag = obj.m_UniqueID; break;
                                case 6: node3.Nodes.Add(obj.m_UniqueID.ToString("X8"), obj.GetDescription()).Tag = obj.m_UniqueID; break;
                                case 7: node4.Nodes.Add(obj.m_UniqueID.ToString("X8"), obj.GetDescription()).Tag = obj.m_UniqueID; break;
                            }
                        }
                    }
                    break;

                /*case 3:
                    {
                        TreeNode node0 = tvObjectList.Nodes.Add("parent0", "Paths");
                        foreach (PathObject obj in m_PathObjects)
                            node0.Nodes.Add(obj.m_Num.ToString("X8"), obj.GetDescription()).Tag = 0;
                    }
                    break;*/

                case 4:
                    {
                        btnAddView.Visible = true;
                        btnRemoveSel.Visible = true;

                        if (!m_ShowCommonLayer) break;
                        TreeNode node0 = tvObjectList.Nodes.Add("parent0", "Views");

                        IEnumerable<LevelObject> objects = m_LevelObjects.Values.Where(obj => (obj.m_UniqueID >> 28) == 4);
                        foreach (LevelObject obj in objects)
                        {
                            node0.Nodes.Add(obj.m_UniqueID.ToString("X8"), obj.GetDescription()).Tag = obj.m_UniqueID;
                        }
                    }
                    break;

                case 5:
                    tvObjectList.Nodes.Add("lol", "(nothing available for now)");
                    break;
            }

            tvObjectList.ExpandAll();
        }

        private void UpdateSelection()
        {
            PropertyTable ptable = (PropertyTable)pgObjectProperties.SelectedObject;
            ptable["X position"] = m_SelectedObject.Position.X;
            ptable["Y position"] = m_SelectedObject.Position.Y;
            ptable["Z position"] = m_SelectedObject.Position.Z;
            if (m_SelectedObject.SupportsRotation())
                ptable["Y rotation"] = m_SelectedObject.YRotation;
            pgObjectProperties.Refresh();

            RefreshObjects(m_SelectedObject.m_Layer);
        }


        private NitroROM m_ROM;
        public int LevelID;
        private NitroOverlay m_Overlay;


        private struct PointerReference
        {
            public PointerReference(uint _ref, uint _ptr) { m_ReferenceAddr = _ref; m_PointerAddr = _ptr; }
            public uint m_ReferenceAddr; // where the pointer is stored
            public uint m_PointerAddr; // where the pointer points
        }
        private List<PointerReference> m_PointerList;

        private void AddPointer(uint _ref)
        {
            uint _ptr = m_Overlay.ReadPointer(_ref);
            m_PointerList.Add(new PointerReference(_ref, _ptr));
        }

        private void RemovePointer(uint _ref)
        {
            for (int i = 0; i < m_PointerList.Count;)
            {
                if (m_PointerList[i].m_ReferenceAddr == _ref)
                    m_PointerList.RemoveAt(i);
                else
                    i++;
            }
        }

        private void UpdateObjectOffsets(uint start, uint delta)
        {
            foreach (LevelObject obj in m_LevelObjects.Values)
                if (obj.m_Offset >= start) obj.m_Offset += delta;

            for (int a = 0; a < m_TexAnims.Length; a++)
            {
                foreach (LevelTexAnim anim in m_TexAnims[a])
                {
                    if (anim.m_Offset >= start) anim.m_Offset += delta;
                    if (anim.m_ScaleTblOffset >= start) anim.m_ScaleTblOffset += delta;
                    if (anim.m_RotTblOffset >= start) anim.m_RotTblOffset += delta;
                    if (anim.m_TransTblOffset >= start) anim.m_TransTblOffset += delta;
                    if (anim.m_MatNameOffset >= start) anim.m_MatNameOffset += delta;
                }
            }
        }

        private void AddSpace(uint offset, uint amount)
        {
            if ((m_Overlay.GetSize() + amount) > NitroROM.LEVEL_OVERLAY_SIZE)
                throw new Exception("This level has reached the level size limit. Cannot add more data.");

            // move the data
            byte[] block = m_Overlay.ReadBlock(offset, (uint)(m_Overlay.GetSize() - offset));
            m_Overlay.WriteBlock(offset + amount, block);

            // write zeroes in the newly created space
            for (int i = 0; i < amount; i++)
                m_Overlay.Write8((uint)(offset + i), 0);

            // update the pointers
            for (int i = 0; i < m_PointerList.Count; i++)
            {
                PointerReference ptrref = m_PointerList[i];
                if (ptrref.m_ReferenceAddr >= offset)
                    ptrref.m_ReferenceAddr += amount;
                if (ptrref.m_PointerAddr >= offset)
                {
                    ptrref.m_PointerAddr += amount;
                    m_Overlay.WritePointer(ptrref.m_ReferenceAddr, ptrref.m_PointerAddr);
                }
                m_PointerList[i] = ptrref;
            }

            // update the objects 'n' all
            UpdateObjectOffsets(offset, amount);
        }

        private void RemoveSpace(uint offset, uint amount)
        {
            // move the data
            byte[] block = m_Overlay.ReadBlock(offset + amount, (uint)(m_Overlay.GetSize() - offset - amount));
            m_Overlay.WriteBlock(offset, block);
            m_Overlay.SetSize(m_Overlay.GetSize() - amount);

            // update the pointers
            for (int i = 0; i < m_PointerList.Count; i++)
            {
                PointerReference ptrref = m_PointerList[i];
                if (ptrref.m_ReferenceAddr >= (offset + amount))
                    ptrref.m_ReferenceAddr -= amount;
                if (ptrref.m_PointerAddr >= (offset + amount))
                {
                    ptrref.m_PointerAddr -= amount;
                    m_Overlay.WritePointer(ptrref.m_ReferenceAddr, ptrref.m_PointerAddr);
                }
                m_PointerList[i] = ptrref;
            }

            // update the objects 'n' all
            UpdateObjectOffsets(offset + amount, (uint)-amount);
        }

        private uint AddObjectSlot(int type, int layer, int area)
        {
            int[] sizes = {16, 16, 6, 6, 14, 8, 8, 8, 8, 12, 14, 2, 2, 0, 4};
            int size = sizes[type];

            uint tableptr;
            if (type == 0 || type == 5)
            {
                uint areaptr = (uint)(m_Overlay.ReadPointer(0x70) + (area * 12));
                tableptr = m_Overlay.ReadPointer(areaptr);

                if (tableptr == 0xFFFFFFFF)
                {
                    tableptr = m_Overlay.GetSize();
                    m_Overlay.WritePointer(areaptr, tableptr);
                    AddPointer(areaptr);
                    m_Overlay.Write32(tableptr, 1);
                    m_Overlay.WritePointer(tableptr + 4, tableptr + 8);
                    AddPointer(tableptr + 4);
                    m_Overlay.Write8(tableptr + 8, (byte)(type | (layer << 5)));
                    m_Overlay.Write8(tableptr + 9, 1);
                    m_Overlay.Write16(tableptr + 10, 0);
                    m_Overlay.WritePointer(tableptr + 12, tableptr + 16);
                    AddPointer(tableptr + 12);
                    return tableptr + 16;
                }
            }
            else
                tableptr = m_Overlay.ReadPointer(0x64);

            uint numentries = m_Overlay.Read32(tableptr);
            for (uint i = 0; i < numentries; i++)
            {
                uint curptr = (uint)(m_Overlay.ReadPointer(tableptr + 4) + (i * 8));

                byte type_layer = m_Overlay.Read8(curptr);
                if ((type_layer & 0x1F) != type) continue;
                if ((type_layer >> 5) != layer) continue;

                byte numobjs = m_Overlay.Read8(curptr + 1);
                if (numobjs == 255) continue;

                uint endptr = (uint)(m_Overlay.ReadPointer(curptr + 4) + (numobjs * size));
                AddSpace(endptr, (uint)size);
                m_Overlay.Write8(curptr + 1, (byte)(numobjs + 1));
                return endptr;
            }

            uint tableendptr = (uint)(m_Overlay.ReadPointer(tableptr + 4) + (numentries * 8));
            AddSpace(tableendptr, 8);
            m_Overlay.Write32(tableptr, numentries + 1);

            uint objaddr = m_Overlay.GetSize();
            m_Overlay.Write8(tableendptr, (byte)(type | (layer << 5)));
            m_Overlay.Write8(tableendptr + 1, 1);
            m_Overlay.Write16(tableendptr + 2, 0);
            m_Overlay.WritePointer(tableendptr + 4, objaddr);
            AddPointer(tableendptr + 4);
            
            return objaddr;
        }

        private void RemoveObjectSlot(LevelObject obj)
        {
            int type = obj.m_Type;
            int[] sizes = { 16, 16, 6, 6, 14, 8, 8, 8, 8, 12, 14, 2, 2, 0, 4 };
            int size = sizes[type];

            uint tableptr;
            if (type == 0 || type == 5)
            {
                uint areaptr = (uint)(m_Overlay.ReadPointer(0x70) + (obj.m_Area * 12));
                tableptr = m_Overlay.ReadPointer(areaptr);
            }
            else
                tableptr = m_Overlay.ReadPointer(0x64);

            uint numentries = m_Overlay.Read32(tableptr);
            for (uint i = 0; i < numentries; i++)
            {
                uint curptr = (uint)(m_Overlay.ReadPointer(tableptr + 4) + (i * 8));

                int tbltype = m_Overlay.Read8(curptr) & 0x1F;
                if (tbltype != type) continue;

                int numobjs = m_Overlay.Read8(curptr + 1);
                uint tblstart = m_Overlay.ReadPointer(curptr + 4);
                uint tblend = (uint)(tblstart + (numobjs * sizes[tbltype]));

                if (obj.m_Offset < tblstart || obj.m_Offset >= tblend)
                    continue;

                RemoveSpace(obj.m_Offset, (uint)size);
                if (numobjs > 1)
                {
                    m_Overlay.Write8(curptr + 1, (byte)(numobjs - 1));
                    return;
                }

                RemovePointer(curptr + 4);
                RemoveSpace(curptr, 8);
                if (numentries > 1 || (type != 0 && type != 5))
                {
                    m_Overlay.Write32(tableptr, (uint)(numentries - 1));
                    return;
                }

                RemovePointer(tableptr + 4);
                RemoveSpace(tableptr, 8);
                uint areaptr = (uint)(m_Overlay.ReadPointer(0x70) + (obj.m_Area * 12));
                RemovePointer(areaptr);
                m_Overlay.WritePointer(areaptr, 0xFFFFFFFF);

                return;
            }
        }

        private LevelObject AddObject(int type, ushort id, int layer, int area)
        {
            int[] sizes = { 16, 16, 6, 6, 14, 8, 8, 8, 8, 12, 14, 2, 2, 0, 4 };
            int size = sizes[type];

            uint offset = AddObjectSlot(type, layer, area);
            for (int i = 0; i < size; i++)
                m_Overlay.Write8((uint)(offset + i), 0x00);

            // write the object ID before creating the object so that it is created
            // with the right renderer and settings
            if (type == 0 || type == 5)
                m_Overlay.Write16(offset, id);

            uint uniqueid = m_LevelObjects.Keys.DefaultIfEmpty((uint)m_LevelObjects.Count).First(uid => m_LevelObjects.Keys.Count(uid2 => (uid2 & 0x0FFFFFFF) == ((uid & 0x0FFFFFFF) + 1)) == 0);
            uniqueid = (uniqueid & 0x0FFFFFFF) + 1;

            LevelObject obj = null;
            string parentnode = "parent0";
            switch (type)
            {
                case 0: obj = new StandardObject(m_Overlay, offset, (int)uniqueid, layer, area); break;
                case 1:
                    {
                        int maxid = m_LevelObjects.Values.Where(obj2 => obj2.m_Type == 1).Max(obj2 => ((EntranceObject)obj2).m_EntranceID);
                        obj = new EntranceObject(m_Overlay, offset, (int)uniqueid, layer, maxid + 1);
                    }
                    break;
                case 2: obj = new PathPointObject(m_Overlay, offset, (int)uniqueid); break;
                case 3: break;
                case 4: obj = new ViewObject(m_Overlay, offset, (int)uniqueid); break;
                case 5: obj = new SimpleObject(m_Overlay, offset, (int)uniqueid, layer, area); break;
                case 6: parentnode = "parent3"; obj = new TpSrcObject(m_Overlay, offset, (int)uniqueid, layer); break;
                case 7: parentnode = "parent4"; obj = new TpDstObject(m_Overlay, offset, (int)uniqueid, layer); break;
                case 8: /* fog */ break;
                case 9: parentnode = "parent2"; obj = new DoorObject(m_Overlay, offset, (int)uniqueid, layer); break;
                case 10: parentnode = "parent1"; obj = new ExitObject(m_Overlay, offset, (int)uniqueid, layer); break;
                case 11: /* minimap */ break;
                case 12: /* unk */ break;
                case 14: /* unk */ break;
            }

            if (obj != null)
            {
                m_LevelObjects.Add(obj.m_UniqueID, obj);
                tvObjectList.Nodes[parentnode].Nodes.Add(obj.m_UniqueID.ToString("X8"), obj.GetDescription()).Tag = obj.m_UniqueID;
            }

            return obj;
        }

        private void RemoveObject(LevelObject obj)
        {
            RemoveObjectSlot(obj);
            obj.Release();
            m_LevelObjects.Remove(obj.m_UniqueID);
            tvObjectList.Nodes.Find(obj.m_UniqueID.ToString("X8"), true)[0].Parent.Nodes.RemoveByKey(obj.m_UniqueID.ToString("X8"));

            if (obj.m_Type == 1)
            {
                IEnumerable<LevelObject> toupdate = m_LevelObjects.Values.Where(obj2 => obj2.m_Type == 1 && ((EntranceObject)obj2).m_EntranceID > ((EntranceObject)obj).m_EntranceID);
                foreach (LevelObject entrance in toupdate)
                {
                    ((EntranceObject)entrance).m_EntranceID--;
                    tvObjectList.Nodes.Find(entrance.m_UniqueID.ToString("X8"), true)[0].Text = entrance.GetDescription();
                }
            }
        }

        private void RelocateObject(LevelObject obj, int layer, int area)
        {
            RemoveObjectSlot(obj);
            obj.m_Offset = AddObjectSlot(obj.m_Type, layer, area);
        }


        //private bool m_LevelModified;

        // level data
        public LevelSettings m_LevelSettings;
        public int m_NumAreas;
        public Dictionary<uint, LevelObject> m_LevelObjects;
        public List<LevelTexAnim>[] m_TexAnims;

        public uint m_MinimapFileIDsOffset;
        public ushort[] m_MinimapFileIDs;

        private bool m_GLLoaded;

        private Vector2 m_CamRotation;
        private Vector3 m_CamTarget;
        private float m_CamDistance;
        private float m_AspectRatio;
        private float m_PixelFactorX, m_PixelFactorY;
        private Vector3 m_CamPosition;
        private bool m_UpsideDown;
        private Matrix4 m_CamMatrix, m_SkyboxMatrix;

        private uint[] m_PickingFrameBuffer;

        private bool m_ShowCommonLayer;
        private int m_AuxLayerNum;
        private int m_EditMode;

        private MouseButtons m_MouseDown;
        private Point m_LastMouseClick, m_LastMouseMove;
        private Point m_MouseCoords;

        private uint m_Hovered, m_LastHovered;
        private uint m_Selected, m_LastSelected;
        private uint m_LastClicked;
        private LevelObject m_HoveredObject;
        private LevelObject m_SelectedObject;
        private uint m_ObjectBeingPlaced;
        private bool m_ShiftPressed;

        private BMD m_SkyboxModel;
        private BMD m_LevelModel;
        private KCL m_LevelCollMap;

        private int m_SkyboxDL;
        private int[,] m_LevelModelDLs;
        private int[,] m_ObjectDLs;
        private int m_SelectHiliteDL;
        private int m_HoverHiliteDL;


        private void glLevelView_Load(object sender, EventArgs e)
        {
            // initialize OpenGL
            glLevelView.Context.MakeCurrent(glLevelView.WindowInfo);

            m_PickingFrameBuffer = new uint[9];
            
            GL.Viewport(glLevelView.ClientRectangle);

            m_AspectRatio = (float)glLevelView.Width / (float)glLevelView.Height;
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            Matrix4 projmtx = Matrix4.CreatePerspectiveFieldOfView((float)((70f * Math.PI) / 180f), m_AspectRatio, 0.01f, 1000f);
            GL.MultMatrix(ref projmtx);

            GL.Enable(EnableCap.DepthTest);
            GL.ClearDepth(1f);

            // lighting!
            //GL.Light(LightName.Light0, LightParameter.Position, new Vector4(0.0f, -0.646484375f, -0.646484375f, 0.0f));
            GL.Light(LightName.Light0, LightParameter.Position, new Vector4(1.0f, 1.0f, 1.0f, 0.0f));
            GL.Light(LightName.Light0, LightParameter.Ambient, Color.SkyBlue);
            GL.Light(LightName.Light0, LightParameter.Diffuse, Color.SkyBlue);
            GL.Light(LightName.Light0, LightParameter.Specular, Color.SkyBlue);
            /*GL.Light(LightName.Light1, LightParameter.Position, new Vector4(-0.666015625f, -0.35546875f, -0.1103515625f, 0.0f));
            GL.Light(LightName.Light1, LightParameter.Ambient, Color.Red);
            GL.Light(LightName.Light1, LightParameter.Diffuse, Color.Red);
            GL.Light(LightName.Light1, LightParameter.Specular, Color.Red);*/

            GL.Enable(EnableCap.Normalize);

            m_CamRotation = new Vector2(0.0f, (float)Math.PI / 8.0f);
			// m_CamRotation = new Vector2(0.0f, 0.0f);
			m_CamTarget = new Vector3(0.0f, 0.0f, 0.0f);
		    m_CamDistance = 1.0f;//6.5f;
			UpdateCamera();

			m_PixelFactorX = ((2.0f * (float)Math.Tan((35.0f*Math.PI)/180.0f) * m_AspectRatio) / (float)(glLevelView.Width));
			m_PixelFactorY = ((2.0f * (float)Math.Tan((35.0f*Math.PI)/180.0f)) / (float)(glLevelView.Height));

			GL.Enable(EnableCap.Blend);
			GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

			GL.Enable(EnableCap.AlphaTest);
			GL.AlphaFunc(AlphaFunction.Greater, 0.0f);

            GL.Enable(EnableCap.Texture2D);

            //GL.Enable(EnableCap.CullFace);
            //GL.CullFace(CullFaceMode.Back);


            LoadLevelData();
            PopulateObjectList();

            // prerender the skybox
            UpdateSkybox(-1);

            // prerender the level model
            m_LevelModel = null; m_LevelModelDLs = null;
            UpdateLevelModel();

            // prerender the objects
            m_ObjectDLs = new int[8,3];
            for (int l = 0; l < 8; l++)
            {
                RenderObjectLists(RenderMode.Opaque, l);
                RenderObjectLists(RenderMode.Translucent, l);
                RenderObjectLists(RenderMode.Picking, l);
            }

            m_SelectHiliteDL = GL.GenLists(1);
            m_HoverHiliteDL = GL.GenLists(1);

            m_GLLoaded = true;
        }

        private void glLevelView_Resize(object sender, EventArgs e)
        {
            if (!m_GLLoaded) return;
            glLevelView.Context.MakeCurrent(glLevelView.WindowInfo);

            GL.Viewport(glLevelView.ClientRectangle);

            m_AspectRatio = (float)glLevelView.Width / (float)glLevelView.Height;
            GL.MatrixMode(MatrixMode.Projection);
            Matrix4 projmtx = Matrix4.CreatePerspectiveFieldOfView((float)(70f * Math.PI / 180f), m_AspectRatio, 0.01f, 1000f);
            //Matrix4 projmtx = Matrix4.CreateOrthographic(2f, 2f, 0.01f, 1000f);
            GL.LoadMatrix(ref projmtx);

            m_PixelFactorX = ((2f * (float)Math.Tan((35f * Math.PI) / 180f) * m_AspectRatio) / (float)(glLevelView.Width));
            m_PixelFactorY = ((2f * (float)Math.Tan((35f * Math.PI) / 180f)) / (float)(glLevelView.Height));
        }

        int lol = 0;

        private void glLevelView_Paint(object sender, PaintEventArgs e)
        {
            if (!m_GLLoaded) return;
            glLevelView.Context.MakeCurrent(glLevelView.WindowInfo);

            // Pass 1 - picking mode rendering (render stuff with fake colors that identify objects)

            GL.ClearColor(1.0f, 1.0f, 1.0f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref m_CamMatrix);

            GL.Disable(EnableCap.AlphaTest);
            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.Dither);
            GL.Disable(EnableCap.LineSmooth);
            GL.Disable(EnableCap.PolygonSmooth);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.Disable(EnableCap.Lighting);

            for (int a = 0; a < m_LevelModelDLs.GetLength(0); a++)
            {
                GL.Color4(Color.FromArgb(a));
                GL.CallList(m_LevelModelDLs[a, 2]);
            }

            if (m_ShowCommonLayer) GL.CallList(m_ObjectDLs[0, 2]);
            if (m_AuxLayerNum > 0) GL.CallList(m_ObjectDLs[m_AuxLayerNum, 2]);

            GL.Flush();
            GL.ReadPixels(m_MouseCoords.X - 1, glLevelView.Height - m_MouseCoords.Y + 1, 3, 3, PixelFormat.Bgra, PixelType.UnsignedByte, m_PickingFrameBuffer);

            //float lol = 0f;
            //GL.ReadPixels(m_MouseCoords.X, glLevelView.Height - m_MouseCoords.Y, 1, 1, PixelFormat.DepthComponent, PixelType.Float, ref lol);
            //lol = (float)-Math.Log(1f - lol);
            //slStatusLabel.Text = lol.ToString();

            // Pass 2 - real rendering

            GL.DepthMask(true);
            GL.ClearColor(0.0f, 0.0f, 0.125f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            GL.Enable(EnableCap.AlphaTest);
            GL.Enable(EnableCap.Blend);
            GL.Enable(EnableCap.Dither);
            GL.Enable(EnableCap.LineSmooth);
            GL.Enable(EnableCap.PolygonSmooth);

            GL.LoadMatrix(ref m_SkyboxMatrix);
            GL.CallList(m_SkyboxDL);

            GL.LoadMatrix(ref m_CamMatrix);

            //GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

            // opaque polygons
            for (int a = 0; a < m_LevelModelDLs.GetLength(0); a++)
                GL.CallList(m_LevelModelDLs[a, 0]);

            if (m_ShowCommonLayer) GL.CallList(m_ObjectDLs[0, 0]);
            if (m_AuxLayerNum > 0) GL.CallList(m_ObjectDLs[m_AuxLayerNum, 0]);

            // translucent polygons
            for (int a = 0; a < m_LevelModelDLs.GetLength(0); a++)
                GL.CallList(m_LevelModelDLs[a, 1]);

            if (m_ShowCommonLayer) GL.CallList(m_ObjectDLs[0, 1]);
            if (m_AuxLayerNum > 0) GL.CallList(m_ObjectDLs[m_AuxLayerNum, 1]);

            // highlight outlines
            if (m_SelectedObject != null && m_SelectedObject != m_HoveredObject) GL.CallList(m_SelectHiliteDL);
            if (m_HoveredObject != null) GL.CallList(m_HoverHiliteDL);

            //GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

            // axes (temp)
            /*GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.LineWidth(2.0f);
            GL.Begin(BeginMode.Lines);
            GL.Color3(1f, 0f, 0f);
            GL.Vertex3(0f, 0f, 0f);
            GL.Vertex3(500f, 0f, 0f);
            GL.Color3(0f, 1f, 0f);
            GL.Vertex3(0f, 0f, 0f);
            GL.Vertex3(0f, 500f, 0f);
            GL.Color3(0f, 0f, 1f);
            GL.Vertex3(0f, 0f, 0f);
            GL.Vertex3(0f, 0f, 500f);
            GL.End();*/

#if false
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.LineWidth(1f);
           // GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

            // enable this to view the level's collision map (slow)
            int n = 0;
            foreach (KCL.Plane plane in m_LevelCollMap.m_Planes)
	        {
                Color[] colors = { Color.Red, Color.Green, Color.Blue, Color.Cyan, Color.Magenta, Color.Yellow, Color.White, Color.OrangeRed };
                GL.Color3(colors[n & 7]); 
                //GL.Color3(colors[(n >> 12) & 7]);
                n++;
                //if (n != 37) continue;

                GL.Begin(BeginMode.Triangles);
		        Vector3 lol1 = Vector3.Cross(plane.m_Dir1, plane.m_Normal);
                float lol1len = plane.m_Length / (float)Math.Cos(Math.Acos(Math.Min(1f,Vector3.Dot(lol1, plane.m_Dir3))));
                Vector3 pta = Vector3.Add(plane.m_Position, Vector3.Multiply(lol1, lol1len));

		        Vector3 lol2 = Vector3.Cross(plane.m_Normal, plane.m_Dir2);
		        float lol2len = plane.m_Length / (float)Math.Cos(Math.Acos(Math.Min(1f,Vector3.Dot(lol2, plane.m_Dir3))));
                Vector3 ptb = Vector3.Add(plane.m_Position, Vector3.Multiply(lol2, lol2len));
		        //GL.Vertex3(plane.m_Position);

                //if (pta.Length > 100f || ptb.Length > 100f || plane.m_Position.Length > 100f)
                //    MessageBox.Show(string.Format("degenerated plane {0}: {1} {2} {3}\n\n{4}\n{5}\n{6}\n{7}\n{8}\n{9}", 
                //        n, pta, plane.m_Position, ptb, plane.m_Position, plane.m_Normal, plane.m_Dir1, plane.m_Dir2, plane.m_Dir3, plane.m_Length));

               /* if (n == 32)
                    MessageBox.Show(String.Format("1: {0} {1}\n2: {2} {3}\n\n{4}", 
                        lol1, lol1len, lol2, lol2len,
                        Vector3.Dot(lol2, plane.m_Dir3)));*/

                GL.Vertex3(pta);
                GL.Vertex3(plane.m_Position);
		        GL.Vertex3(ptb);

		        GL.End();
            }

            // enable this to view the octree cubes (slow as well)
            //foreach (KCL.OctreeNode node in KCL.OctreeNode.m_List)
            /*KCL.OctreeNode node = KCL.OctreeNode.m_List[lol];
            {
                Vector3 s0 = node.m_Pos;
                Vector3 s1 = node.m_Pos + node.m_Size;

                //if (node.m_LOL)
                //    GL.Color3(Color.LimeGreen);
               // else
              //  if (node.m_NumPlanes > 8)
                {
                    if (node.m_NumPlanes > 22)
                        GL.Color3(Color.Red);
                    else
                        GL.Color3(Color.Blue);
                }
               // else
                //    continue;
                    //GL.Color3(Color.Green);

                GL.Begin(BeginMode.LineStrip);
                GL.Vertex3(s1.X, s1.Y, s1.Z);
                GL.Vertex3(s0.X, s1.Y, s1.Z);
                GL.Vertex3(s0.X, s1.Y, s0.Z);
                GL.Vertex3(s1.X, s1.Y, s0.Z);
                GL.Vertex3(s1.X, s1.Y, s1.Z);
                GL.Vertex3(s1.X, s0.Y, s1.Z);
                GL.Vertex3(s0.X, s0.Y, s1.Z);
                GL.Vertex3(s0.X, s0.Y, s0.Z);
                GL.Vertex3(s1.X, s0.Y, s0.Z);
                GL.Vertex3(s1.X, s0.Y, s1.Z);
                GL.End();

                GL.Begin(BeginMode.Lines);
                GL.Vertex3(s0.X, s1.Y, s1.Z);
                GL.Vertex3(s0.X, s0.Y, s1.Z);
                GL.Vertex3(s0.X, s1.Y, s0.Z);
                GL.Vertex3(s0.X, s0.Y, s0.Z);
                GL.Vertex3(s1.X, s1.Y, s0.Z);
                GL.Vertex3(s1.X, s0.Y, s0.Z);
                GL.End();

                foreach (int plol in node.m_PlaneList)
                {
                    KCL.Plane plane = m_LevelCollMap.m_Planes[plol];

                    GL.Color3(Color.LimeGreen);
                    GL.Begin(BeginMode.Triangles);
                    Vector3 lol1 = Vector3.Cross(plane.m_Dir1, plane.m_Normal);
                    float lol1len = plane.m_Length / (float)Math.Cos(Math.Acos(Math.Min(1f, Vector3.Dot(lol1, plane.m_Dir3))));
                    GL.Vertex3(Vector3.Add(plane.m_Position, Vector3.Multiply(lol1, lol1len)));

                    GL.Vertex3(plane.m_Position);

                    Vector3 lol2 = Vector3.Cross(plane.m_Normal, plane.m_Dir2);
                    float lol2len = plane.m_Length / (float)Math.Cos(Math.Acos(Math.Min(1f, Vector3.Dot(lol2, plane.m_Dir3))));
                    //GL.Vertex3(plane.m_Position);
                    GL.Vertex3(Vector3.Add(plane.m_Position, Vector3.Multiply(lol2, lol2len)));

                    GL.End();
                }
            }*/
#endif

            glLevelView.SwapBuffers();
        }

        private void glLevelView_MouseDown(object sender, MouseEventArgs e)
        {
            if (m_MouseDown != MouseButtons.None) return;

            if (m_ObjectBeingPlaced != 0xFFFF)
            {
                int type = (int)(m_ObjectBeingPlaced >> 16);
                ushort id = (ushort)(m_ObjectBeingPlaced & 0xFFFF);
                if (type == 0 && IsSimpleObject(id))
                    type = 5;

                LevelObject obj = AddObject(type, id, 0, 0);
                obj.Position = Get3DCoords(e.Location, 2f);
                obj.GenerateProperties();
                pgObjectProperties.SelectedObject = obj.m_Properties;
                
                m_Selected = obj.m_UniqueID;
                m_SelectedObject = obj;
                m_LastSelected = obj.m_UniqueID;
                m_Hovered = obj.m_UniqueID;
                m_HoveredObject = obj;
                m_LastHovered = obj.m_UniqueID;
                m_LastClicked = obj.m_UniqueID;

                RefreshObjects(m_SelectedObject.m_Layer);

                if (!m_ShiftPressed)
                {
                    m_ObjectBeingPlaced = 0xFFFF;
                    slStatusLabel.Text = "Object added.";
                }
            }
            else if ((m_PickingFrameBuffer[4] == m_PickingFrameBuffer[1]) &&
                (m_PickingFrameBuffer[4] == m_PickingFrameBuffer[3]) &&
                (m_PickingFrameBuffer[4] == m_PickingFrameBuffer[5]) &&
                (m_PickingFrameBuffer[4] == m_PickingFrameBuffer[7]))
            {
                if (btnRemoveSel.Checked)
                {
                    uint sel = m_PickingFrameBuffer[4];
                    uint type = (sel >> 28);
                    if (type == m_EditMode)
                    {
                        LevelObject obj = m_LevelObjects[sel];
                        RemoveObject(obj);
                        RefreshObjects(obj.m_Layer);

                        if (!m_ShiftPressed)
                        {
                            btnRemoveSel.Checked = false;
                            slStatusLabel.Text = "Object removed.";
                        }
                    }
                }
                else
                    m_LastClicked = m_PickingFrameBuffer[4];
            }

            m_MouseDown = e.Button;
            m_LastMouseClick = e.Location;
            m_LastMouseMove = e.Location;
        }

        private void glLevelView_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != m_MouseDown) return;

            if ((Math.Abs(e.X - m_LastMouseClick.X) < 3) && (Math.Abs(e.Y - m_LastMouseClick.Y) < 3) &&
                (m_PickingFrameBuffer[4] == m_PickingFrameBuffer[1]) &&
                (m_PickingFrameBuffer[4] == m_PickingFrameBuffer[3]) &&
                (m_PickingFrameBuffer[4] == m_PickingFrameBuffer[5]) &&
                (m_PickingFrameBuffer[4] == m_PickingFrameBuffer[7]))
            {
                uint sel = m_PickingFrameBuffer[4];
                uint type = (sel >> 28);
                
                if (type == m_EditMode && type != 0 && type != 5)
                {
                    m_Selected = sel;

                    if (m_LastSelected != m_Selected)
                    {
                        LevelObject obj = m_LevelObjects[sel];
                        RenderObjectHilite(obj, k_SelectionColor, m_SelectHiliteDL);
                        m_LastSelected = m_Selected;
                        m_SelectedObject = obj;

                        pgObjectProperties.SelectedObject = obj.m_Properties;
                        tvObjectList.SelectedNode = tvObjectList.Nodes.Find(obj.m_UniqueID.ToString("X8"), true)[0];
                    }
                }
                else
                {
                    m_Selected = 0xFFFFFFFF;
                    m_LastSelected = 0xFFFFFFFF;
                    m_SelectedObject = null;

                    pgObjectProperties.SelectedObject = null;
                    tvObjectList.SelectedNode = null;
                }
            }

            m_MouseDown = MouseButtons.None;
        }

        private void glLevelView_MouseMove(object sender, MouseEventArgs e)
        {
            float xdelta = (float)(e.X - m_LastMouseMove.X);
			float ydelta = (float)(e.Y - m_LastMouseMove.Y);
			 
			m_MouseCoords = e.Location;
			m_LastMouseMove = e.Location;

            //if (m_SelectedObject != null)
            //    m_SelectedObject.m_TestMatrix.M11 *= 1.001f;

            if (m_MouseDown != MouseButtons.None)
            {
                if (m_LastSelected == 0xFFFFFFFF || m_LastSelected != m_LastClicked)
                {
                    if (m_MouseDown == MouseButtons.Right)
                    {
                        /*if (btnReverseRot.Checked)
                        {
                            xdelta = -xdelta;
                            ydelta = -ydelta;
                        }*/

                        if (m_UpsideDown)
                            xdelta = -xdelta;

                        m_CamRotation.X -= xdelta * 0.002f;
                        m_CamRotation.Y -= ydelta * 0.002f;

                        ClampRotation(ref m_CamRotation.X, (float)Math.PI * 2.0f);
                        ClampRotation(ref m_CamRotation.Y, (float)Math.PI * 2.0f);
                    }
                    else if (m_MouseDown == MouseButtons.Left)
                    {
                        xdelta *= 0.005f;
                        ydelta *= 0.005f;

                        m_CamTarget.X -= xdelta * (float)Math.Sin(m_CamRotation.X);
                        m_CamTarget.X -= ydelta * (float)Math.Cos(m_CamRotation.X) * (float)Math.Sin(m_CamRotation.Y);
                        m_CamTarget.Y += ydelta * (float)Math.Cos(m_CamRotation.Y);
                        m_CamTarget.Z += xdelta * (float)Math.Cos(m_CamRotation.X);
                        m_CamTarget.Z -= ydelta * (float)Math.Sin(m_CamRotation.X) * (float)Math.Sin(m_CamRotation.Y);
                    }

                    UpdateCamera();
                }
                else
                {
                    if (m_MouseDown == MouseButtons.Right)
                    {
                        if (m_UpsideDown)
                            xdelta = -xdelta;

                        // TODO take obj/camera rotation into account?
                        if (m_SelectedObject.SupportsRotation())
                        {
                            m_SelectedObject.YRotation += xdelta * 0.5f;

                            if (m_SelectedObject.YRotation >= 180f)
                            {
                                m_SelectedObject.YRotation = (float)(-360f + m_SelectedObject.YRotation);
                            }
                            else if (m_SelectedObject.YRotation < -180f)
                            {
                                m_SelectedObject.YRotation = (float)(360f + m_SelectedObject.YRotation);
                            }
                        }
                    }
                    else if (m_MouseDown == MouseButtons.Left)
                    {
                        Vector3 between;
                        Vector3.Subtract(ref m_CamPosition, ref m_SelectedObject.Position, out between);

                        float objz = (((between.X * (float)Math.Cos(m_CamRotation.X)) + (between.Z * (float)Math.Sin(m_CamRotation.X))) * (float)Math.Cos(m_CamRotation.Y)) + (between.Y * (float)Math.Sin(m_CamRotation.Y));

                        xdelta *= m_PixelFactorX * objz;
                        ydelta *= -m_PixelFactorY * objz;

                        float _xdelta = (xdelta * (float)Math.Sin(m_CamRotation.X)) - (ydelta * (float)Math.Sin(m_CamRotation.Y) * (float)Math.Cos(m_CamRotation.X));
                        float _ydelta = ydelta * (float)Math.Cos(m_CamRotation.Y);
                        float _zdelta = (xdelta * (float)Math.Cos(m_CamRotation.X)) + (ydelta * (float)Math.Sin(m_CamRotation.Y) * (float)Math.Sin(m_CamRotation.X));

                        m_SelectedObject.Position.X += _xdelta;
                        m_SelectedObject.Position.Y += _ydelta;
                        m_SelectedObject.Position.Z -= _zdelta;
                    }

                    UpdateSelection();
                }
            }
            //else
            {
                if ((m_PickingFrameBuffer[4] == m_PickingFrameBuffer[1]) &&
                    (m_PickingFrameBuffer[4] == m_PickingFrameBuffer[3]) &&
                    (m_PickingFrameBuffer[4] == m_PickingFrameBuffer[5]) &&
                    (m_PickingFrameBuffer[4] == m_PickingFrameBuffer[7]))
                {
                    uint sel = m_PickingFrameBuffer[4];
                    uint type = (sel >> 28);
                    if (type == m_EditMode)
                    {
                        m_Hovered = sel;
                        if ((type == 0) || (type == 5))
                        {
                            m_LastHovered = 0xFFFFFFFF;
                            m_HoveredObject = null;
                        }
                        else
                        if (m_LastHovered != m_Hovered)
                        {
                            LevelObject obj = m_LevelObjects[sel];
                            RenderObjectHilite(obj, k_HoverColor, m_HoverHiliteDL);
                            m_LastHovered = m_Hovered;
                            m_HoveredObject = obj;
                        }
                    }
                    else
                    {
                        m_Hovered = 0xFFFFFFFF;
                        m_LastHovered = 0xFFFFFFFF;
                        m_HoveredObject = null;
                    }
                }
            }

            glLevelView.Refresh();
        }

        private void glLevelView_MouseWheel(object sender, MouseEventArgs e)
        {
            if ((m_MouseDown == MouseButtons.Left) && ((m_Selected >> 28) != 0xF) && (m_LastClicked == m_Selected))
            {
                float delta = -(e.Delta/120f);
				delta = ((delta < 0f) ? -1f:1f) * (float)Math.Pow(delta, 2f) * 0.05f;

				m_SelectedObject.Position.X += delta * (float)Math.Cos(m_CamRotation.X) * (float)Math.Cos(m_CamRotation.Y);
				m_SelectedObject.Position.Y += delta * (float)Math.Sin(m_CamRotation.Y);
				m_SelectedObject.Position.Z += delta * (float)Math.Sin(m_CamRotation.X) * (float)Math.Cos(m_CamRotation.Y);

				float xdist = delta * (m_MouseCoords.X - (glLevelView.Width / 2f)) * m_PixelFactorX;
				float ydist = delta * (m_MouseCoords.Y - (glLevelView.Height / 2f)) * m_PixelFactorY;

				m_SelectedObject.Position.X -= (xdist * (float)Math.Sin(m_CamRotation.X)) + (ydist * (float)Math.Sin(m_CamRotation.Y) * (float)Math.Cos(m_CamRotation.X));
				m_SelectedObject.Position.Y += ydist * (float)Math.Cos(m_CamRotation.Y);
				m_SelectedObject.Position.Z += (xdist * (float)Math.Cos(m_CamRotation.X)) - (ydist * (float)Math.Sin(m_CamRotation.Y) * (float)Math.Sin(m_CamRotation.X));

                UpdateSelection();
            }
            else
            {
                float delta = -((e.Delta / 120.0f) * 0.1f);
			    m_CamTarget.X += delta * (float)Math.Cos(m_CamRotation.X) * (float)Math.Cos(m_CamRotation.Y);
		        m_CamTarget.Y += delta * (float)Math.Sin(m_CamRotation.Y);
			    m_CamTarget.Z += delta * (float)Math.Sin(m_CamRotation.X) * (float)Math.Cos(m_CamRotation.Y);

			    UpdateCamera();
            }

            glLevelView.Refresh();
        }

        private Vector3 Get3DCoords(Point coords2d, float depth)
        {
            Vector3 ret = m_CamPosition;

            ret.X -= (depth * (float)Math.Cos(m_CamRotation.X) * (float)Math.Cos(m_CamRotation.Y));
            ret.Y -= (depth * (float)Math.Sin(m_CamRotation.Y));
            ret.Z -= (depth * (float)Math.Sin(m_CamRotation.X) * (float)Math.Cos(m_CamRotation.Y));

            float x = (coords2d.X - (glLevelView.Width / 2f)) * m_PixelFactorX * depth;
            float y = -(coords2d.Y - (glLevelView.Height / 2f)) * m_PixelFactorY * depth;

            ret.X += (x * (float)Math.Sin(m_CamRotation.X)) - (y * (float)Math.Sin(m_CamRotation.Y) * (float)Math.Cos(m_CamRotation.X));
            ret.Y += y * (float)Math.Cos(m_CamRotation.Y);
            ret.Z -= (x * (float)Math.Cos(m_CamRotation.X)) + (y * (float)Math.Sin(m_CamRotation.Y) * (float)Math.Sin(m_CamRotation.X));

            return ret;
        }


        private void ReleaseObjectTable(List<LevelObject> list)
        {
            foreach (LevelObject obj in list)
                obj.Release();
        }

        private void LevelEditorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // save confirm goes here

            foreach (LevelObject obj in m_LevelObjects.Values)
                obj.Release();

            m_LevelModel.Release();
            if (m_SkyboxModel != null)
                ModelCache.RemoveModel(m_SkyboxModel);

            Program.m_LevelEditors.Remove(this);
        }


        private void btnStarX_Click(object sender, EventArgs e)
        {
            ToolStripButton btn = (ToolStripButton)sender;

            if (btn.Checked)
            {
                btn.Checked = false;
                m_AuxLayerNum = 0;
            }
            else
            {
                btnStar1.Checked = false;
                btnStar2.Checked = false;
                btnStar3.Checked = false;
                btnStar4.Checked = false;
                btnStar5.Checked = false;
                btnStar6.Checked = false;
                btnStar7.Checked = false;
                btn.Checked = true;
                m_AuxLayerNum = int.Parse((string)btn.Tag);
            }

            PopulateObjectList();
            glLevelView.Refresh();
        }

        private void btnStarAll_Click(object sender, EventArgs e)
        {
            m_ShowCommonLayer = btnStarAll.Checked;
            PopulateObjectList();
            glLevelView.Refresh();
        }

        private void btnEditXXX_Click(object sender, EventArgs e)
        {
            ToolStripButton btn = (ToolStripButton)sender;
            if (btn.Checked) return;

            btnEdit3DModel.Checked = false;
            btnEditObjects.Checked = false;
            btnEditWarps.Checked = false;
            btnEditPaths.Checked = false;
            btnEditViews.Checked = false;
            btnEditMisc.Checked = false;
            btn.Checked = true;

            m_EditMode = int.Parse((string)btn.Tag);

            for (int l = 0; l < 8; l++)
                RenderObjectLists(RenderMode.Picking, l);
            PopulateObjectList();

            glLevelView.Refresh();
        }

        private int m_StatusMarqueeOffset = 0;
        private void StatusMarqueeUpdate(object sender, ElapsedEventArgs e)
        {
            slStatusLabel.Invalidate();
            ssStatusBar.Refresh();
        }
        private void slStatusLabel_Paint(object sender, PaintEventArgs e)
        {
            string text = slStatusLabel.Text;
            int txtwidth = (int)e.Graphics.MeasureString(text, slStatusLabel.Font).Width - 12;
            if (txtwidth > e.ClipRectangle.Width)
            {
                text += " -- ";
                txtwidth = (int)e.Graphics.MeasureString(text, slStatusLabel.Font).Width - 12;

                System.Timers.Timer tmr = new System.Timers.Timer(30);
                tmr.AutoReset = false;
                tmr.Elapsed += new ElapsedEventHandler(StatusMarqueeUpdate);
                tmr.Start();

                m_StatusMarqueeOffset -= 2;
                if (-m_StatusMarqueeOffset >= txtwidth)
                    m_StatusMarqueeOffset = 0;
            }
            else
                m_StatusMarqueeOffset = 0;

            e.Graphics.FillRectangle(new SolidBrush(slStatusLabel.BackColor), e.ClipRectangle);
            e.Graphics.DrawString(text, slStatusLabel.Font, new SolidBrush(slStatusLabel.ForeColor), new PointF(m_StatusMarqueeOffset, 0));
            if (m_StatusMarqueeOffset < 0)
                e.Graphics.DrawString(text, slStatusLabel.Font, new SolidBrush(slStatusLabel.ForeColor), new PointF(m_StatusMarqueeOffset + txtwidth, 0));
        }
        private void slStatusLabel_TextChanged(object sender, EventArgs e)
        {
            m_StatusMarqueeOffset = 0;
        }

        private void btnImportModel_Click(object sender, EventArgs e)
        {
            ModelImporter form = new ModelImporter();
            if (form != null && !form.m_EarlyClosure)
                form.Show(this);
        }

        private void tvObjectList_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            Color fgcolor, bgcolor;
            
			Font font = e.Node.NodeFont;
			if (font == null) font = tvObjectList.Font;
            
            bool red = false;
            if (e.Node.Tag is uint && m_LevelObjects.ContainsKey((uint)e.Node.Tag))
            {
                uint uniqueid = (uint)e.Node.Tag;
                if ((uniqueid >> 28) == 1)
                    red = !m_ObjAvailable[m_LevelObjects[uniqueid].ID];
            }

			if ((e.State & TreeNodeStates.Selected) != 0)
			{
				fgcolor = red ? Color.LightPink : SystemColors.HighlightText;
				bgcolor = SystemColors.Highlight;
			}
			else
			{
				fgcolor = red ? Color.Red : SystemColors.ControlText;
				bgcolor = SystemColors.ControlLightLight;
			}

            // apparently we can't rely on e.Bounds, we have to calculate the size of the string ourselves
            Rectangle txtbounds = e.Bounds;
            SizeF txtsize = e.Graphics.MeasureString(e.Node.Text, font);
            txtbounds.Width = (int)txtsize.Width;
            
            e.Graphics.FillRectangle(new SolidBrush(bgcolor), txtbounds);
            e.Graphics.DrawString(e.Node.Text, font, new SolidBrush(fgcolor), (float)e.Bounds.X, (float)e.Bounds.Y + 1f);
        }

        private void btnLevelSettings_Click(object sender, EventArgs e)
        {
            new LevelSettingsForm(m_LevelSettings).ShowDialog(this);
            GetObjectsAvailable();
            tvObjectList.Refresh();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            bool bankwarning = false;
            IEnumerable<LevelObject> objs = m_LevelObjects.Values.Where(obj => (obj.m_UniqueID >> 28) == 1);
            foreach (LevelObject obj in objs)
                if (!m_ObjAvailable[obj.ID])
                {
                    bankwarning = true;
                    break;
                }

            if (bankwarning)
            {
                DialogResult res = MessageBox.Show("This level contains objects which aren't available with the current object bank settings, and would crash the game.\n\nSave anyway?",
                    Program.AppTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                if (res == DialogResult.No)
                    return;
            }

            m_LevelSettings.SaveChanges();

            foreach (LevelObject obj in m_LevelObjects.Values)
                obj.SaveChanges();

            uint curoffset = m_MinimapFileIDsOffset;
            foreach (ushort id in m_MinimapFileIDs)
            {
                m_Overlay.Write16(curoffset, id);
                curoffset += 2;
            }

            m_Overlay.SaveChanges();
            slStatusLabel.Text = "Changes saved.";
        }

        private void tvObjectList_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag == null) return;
            
            uint objid = (uint)e.Node.Tag;
            m_Selected = m_LastSelected = objid;
            m_SelectedObject = m_LevelObjects[objid];
            pgObjectProperties.SelectedObject = m_SelectedObject.m_Properties;
            RenderObjectHilite(m_SelectedObject, k_SelectionColor, m_SelectHiliteDL);
            glLevelView.Refresh();
        }

        private void pgObjectProperties_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            if (m_SelectedObject == null) // should never happen but we never know
            {
                MessageBox.Show("No object was selected. This shouldn't have happened. Tell Mega-Mario about it.", "Bug!");
                return;
            }

            if (e.ChangedItem.Label == "Object ID")
            {
                if (IsSimpleObject((ushort)e.ChangedItem.Value) ^ IsSimpleObject(m_SelectedObject.ID))
                {
                    LevelObject oldobj = m_SelectedObject;
                    RemoveObject(oldobj);

                    ushort newid = (ushort)e.ChangedItem.Value;
                    int type = IsSimpleObject(newid) ? 5 : 0;
                    LevelObject obj = AddObject(type, newid, oldobj.m_Layer, oldobj.m_Area);
                    obj.Position = oldobj.Position;
                    obj.Parameters[0] = (ushort)(oldobj.Parameters[0] & ((type == 5) ? 0x007F : 0xFFFF));
                    obj.GenerateProperties();
                    pgObjectProperties.SelectedObject = obj.m_Properties;

                    m_Selected = obj.m_UniqueID;
                    m_SelectedObject = obj;
                    m_LastSelected = obj.m_UniqueID;
                    m_Hovered = obj.m_UniqueID;
                    m_HoveredObject = obj;
                    m_LastHovered = obj.m_UniqueID;
                    m_LastClicked = obj.m_UniqueID;

                    RefreshObjects(obj.m_Layer);
                    return;
                }
            }

            if (e.ChangedItem.Label == "Star")
            {
                int newstar;
                if (e.ChangedItem.Value is int) newstar = (int)e.ChangedItem.Value;
                else if ((string)e.ChangedItem.Value == "All") newstar = 0;
                else newstar = int.Parse((string)e.ChangedItem.Value);

                RelocateObject(m_SelectedObject, newstar, m_SelectedObject.m_Area);
                return;
            }

            if (e.ChangedItem.Label == "Area")
            {
                RelocateObject(m_SelectedObject, m_SelectedObject.m_Layer, (int)e.ChangedItem.Value);
                return;
            }

            int actmask = m_SelectedObject.SetProperty(e.ChangedItem.Label, e.ChangedItem.Value);
            if ((actmask & 4) != 0)
                tvObjectList.Nodes.Find(m_SelectedObject.m_UniqueID.ToString("X8"), true)[0].Text = m_SelectedObject.GetDescription();
            if ((actmask & 2) != 0)
                pgObjectProperties.Refresh();
            if ((actmask & 1) != 0)
                RefreshObjects(m_SelectedObject.m_Layer);
        }

        private void btnAddObject_Click(object sender, EventArgs e)
        {
            btnRemoveSel.Checked = false;

            ObjectListForm objlist = new ObjectListForm(0);
            if (objlist.ShowDialog(this) != DialogResult.OK) return;
            if (objlist.ObjectID > 0x145 && objlist.ObjectID != 0x1FF) return;

            m_ObjectBeingPlaced = objlist.ObjectID;
            slStatusLabel.Text = string.Format("Click anywhere in the level to place your new object ({0} - {1}). Hold Shift while clicking to place multiple objects. Hit Escape to abort.",
                objlist.ObjectID, ObjectDatabase.m_ObjectInfo[objlist.ObjectID].m_Name);
        }

        private void btnAddWhatever_Click(object sender, EventArgs e)
        {
            btnRemoveSel.Checked = false;

            uint type = uint.Parse((string)((ToolStripItem)sender).Tag);
            m_ObjectBeingPlaced = type << 16;

            string obj = "OSHIT BUG";
            switch (type)
            {
                case 1: obj = "entrance"; break;
                case 2: obj = "path node"; break;
                case 4: obj = "view"; break;
                case 6: obj = "teleport source"; break;
                case 7: obj = "teleport destination"; break;
                case 9: obj = "door"; break;
                case 10: obj = "exit"; break;
            }

            slStatusLabel.Text = "Click anywhere in the level to place your new "+obj+". Hold Shift while clicking to place multiple "+obj+"s. Hit Escape to abort.";
        }

        private void btnRemoveSel_Click(object sender, EventArgs e)
        {
            if (m_SelectedObject == null)
            {
                if (btnRemoveSel.Checked)
                {
                    btnRemoveSel.Checked = false;
                    slStatusLabel.Text = "Ready";
                    return;
                }

                slStatusLabel.Text = "Click the object you want to remove. Hold Shift while clicking to remove multiple objects. Hit Escape to abort.";
                btnRemoveSel.Checked = true;

                return;
            }

            LevelObject obj = m_SelectedObject;
            RemoveObject(obj);
            RefreshObjects(obj.m_Layer);
            slStatusLabel.Text = "Object removed.";
        }

        private void glLevelView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ShiftKey || e.KeyCode == Keys.RShiftKey)
                m_ShiftPressed = true;
            else if (e.KeyCode == Keys.Escape)
            {
                m_ObjectBeingPlaced = 0xFFFF;
                btnRemoveSel.Checked = false;
                slStatusLabel.Text = "Ready";
            }
            else if (e.KeyCode == Keys.Delete)
            {
                if (m_SelectedObject != null)
                    btnRemoveSel.PerformClick(); // quick cheat
            }

            if (e.KeyCode == Keys.Q)
                btnLOL.PerformClick();
        }

        private void glLevelView_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ShiftKey || e.KeyCode == Keys.RShiftKey)
                m_ShiftPressed = false;
        }

        private void btnStarAll_DoubleClick(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.lolhax)
            {
                btnStarAll.Checked = !btnStarAll.Checked;
                m_ShowCommonLayer = btnStarAll.Checked;
                glLevelView.Refresh();
                return;
            }

            new TempHaxForm().ShowDialog();
            btnImportModel.Visible = (m_EditMode == 0) && Properties.Settings.Default.lolhax;
        }

        private void btnDumpOverlay_Click(object sender, EventArgs e)
        {
            string filename = "level" + LevelID.ToString() + "_overlay.bin";
            System.IO.File.WriteAllBytes(filename, m_Overlay.m_Data);
            slStatusLabel.Text = "Level overlay dumped to " + filename;
        }

        private void btnLOL_Click(object sender, EventArgs e)
        {
            lol++;
            if (lol >= KCL.OctreeNode.m_List.Count) lol = 0;
            glLevelView.Refresh();
        }

        private void btnEditMinimap_Click(object sender, EventArgs e)
        {
            new MinimapEditor().Show(this);
        }
    }
}
