using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Globalization;
using System.IO;
using SM64DSe.Exporters;
using System.Text.RegularExpressions;
using System.Xml;

namespace SM64DSe
{
    public partial class KCLEditorForm : Form
    {
        private bool m_GLLoaded;
        private float m_AspectRatio;

        public List<Vector3> points;
        public List<Vector3> vectors;
        public List<ColFace> planes;

        CultureInfo usa = new CultureInfo("en-US");

        List<Color> colours;

        NitroFile kclFile;

        Dictionary<string, int> matColTypes;

        public KCLEditorForm(NitroFile kclIn)
        {
            InitializeComponent();
            kclFile = kclIn;
            loadKCL(kclIn);
            colours = getColours();
            cmbPolygonMode.Items.Add("Wireframe");
            cmbPolygonMode.Items.Add("Fill");
            cmbPolygonMode.Items.Add("Fill Back");
            matColTypes = new Dictionary<string, int>();
        }

        public void loadKCL(NitroFile kcl)
        {
            points = new List<Vector3>();
            vectors = new List<Vector3>();
            planes = new List<ColFace>();

            uint pointStart = (uint)kcl.Read32(0);//Address of first point
            uint vectorStart = (uint)kcl.Read32(4);//Address of first normal
            uint planeStart = (uint)(kcl.Read32(8));//Address of first plane
            uint gridStart = (uint)kcl.Read32((uint)(0x0C));//Address of grid section
            int numPoints = (int)(vectorStart - pointStart) / 12;//Size of section / size of point header
            int numVectors = (int)((planeStart + 0x10 - vectorStart) / 6);
            int numPlanes = (int)(gridStart - planeStart) / 16;//Size of section / size of plane header

            uint offset = pointStart;
            for (int i = 0; i < numPoints; i++)
            {
                Vector3 curPoint = new Vector3();
                curPoint.X = (int)kcl.Read32((uint)offset) / 64000.0f;
                curPoint.Y = (int)kcl.Read32((uint)(offset + 4)) / 64000.0f;
                curPoint.Z = (int)kcl.Read32((uint)(offset + 8)) / 64000.0f;

                points.Add(curPoint);
                offset += 12;
            }

            offset = vectorStart;
            for (int i = 0; i < numVectors; i++)
            {
                Vector3 curVector = new Vector3();
                curVector.X = (short)kcl.Read16((uint)offset) / 1024.0f;
                curVector.Y = (short)kcl.Read16((uint)(offset + 2)) / 1024.0f;
                curVector.Z = (short)kcl.Read16((uint)(offset + 4)) / 1024.0f;

                vectors.Add(curVector);
                offset += 6;
            }

            offset = (uint)(planeStart + 0x10);
            for (int i = 0; i < numPlanes - 1; i++)
            {
                //Read length
                float planeLength = kcl.Read32(offset) / 65536000.0f;
                //Read ID of Point 1 and get it from list of points
                ushort p1IX = kcl.Read16((uint)(offset + 4));
                Vector3 p1 = points[(int)p1IX];
                //Read ID of Normal Vector and get it from list of vectors
                ushort normalIX = kcl.Read16((uint)(offset + 6));
                Vector3 normal = vectors[(int)normalIX];
                //Read ID of direction vectors and get it from list of vectors
                ushort d1IX = kcl.Read16((uint)(offset + 8));
                Vector3 d1 = vectors[(int)d1IX];
                ushort d2IX = kcl.Read16((uint)(offset + 10));
                Vector3 d2 = vectors[(int)d2IX];
                ushort d3IX = kcl.Read16((uint)(offset + 12));
                //Read collision type
                Vector3 d3 = vectors[(int)d3IX];
                int colType = (int)kcl.Read16((uint)(offset + 14));

                ColFace curFace = new ColFace(planeLength, p1, normal, d1, d2, d3, colType);
                planes.Add(curFace);

                offset += 16;
            }

            lbxPlanes.Items.Clear();

            for (int i = 0; i < planes.Count; i++)
            {
                lbxPlanes.Items.Add("Plane " + i.ToString("00000"));
            }

        }

        private void writeChanges()
        {
            uint planeStart = (kclFile.Read32(8));

            planeStart += (uint)(0x10);

            for (int i = 0; i < planes.Count; i++)
            {
                uint posColType = (uint)(planeStart + (i * 16) + 0x0E);//Get the address of this plane's Collision Type variable

                kclFile.Write16(posColType, (ushort)planes[i].type);//Write the new value to file
            }

            kclFile.SaveChanges();
        }

        private List<Color> getColours()
        {
            List<Color> theColours = new List<Color>();

            for (int i = 255; i > 0; i = i - 50)
            {
                for (int j = 0; j < 255; j = j + 40)
                {
                    for (int k = 255; k > 0; k = k - 40)
                    {
                        Color newColour = Color.FromArgb(180, k, i, j);
                        theColours.Add(newColour);
                    }
                }
            }
            return theColours;
        }


        private void glModelView_Load(object sender, EventArgs e)
        {
            m_GLLoaded = true;

            GL.Viewport(glModelView.ClientRectangle);

            float ratio = (float)glModelView.Width / (float)glModelView.Height;
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            Matrix4 projmtx = Matrix4.CreatePerspectiveFieldOfView((float)((70.0f * Math.PI) / 180.0f), ratio, 0.01f, 1000.0f);
            GL.MultMatrix(ref projmtx);

            m_PixelFactorX = ((2f * (float)Math.Tan((35f * Math.PI) / 180f) * ratio) / (float)(glModelView.Width));
            m_PixelFactorY = ((2f * (float)Math.Tan((35f * Math.PI) / 180f)) / (float)(glModelView.Height));

            GL.LineWidth(2.0f);

            m_CamRotation = new Vector2(0.0f, (float)Math.PI / 8.0f);
            m_CamTarget = new Vector3(0.0f, 0.0f, 0.0f);
            m_CamDistance = 1.0f;
            UpdateCamera();

            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

            GL.ClearColor(Color.FromArgb(0, 0, 32));
        }

        private void glModelView_Resize(object sender, EventArgs e)
        {
            if (!m_GLLoaded) return;
            glModelView.Context.MakeCurrent(glModelView.WindowInfo);

            GL.Viewport(glModelView.ClientRectangle);

            float ratio = (float)glModelView.Width / (float)glModelView.Height;
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            Matrix4 projmtx = Matrix4.CreatePerspectiveFieldOfView((float)((70.0f * Math.PI) / 180.0f), ratio, 0.01f, 1000.0f);
            GL.MultMatrix(ref projmtx);
        }

        private void glModelView_Paint(object sender, PaintEventArgs e)
        {
            if (!m_GLLoaded) return;
            glModelView.Context.MakeCurrent(glModelView.WindowInfo);

            GL.ClearColor(1.0f, 1.0f, 1.0f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref m_CamMatrix);

            GL.Flush();

            GL.ClearColor(0.0f, 0.0f, 0.125f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref m_CamMatrix);

            for (int i = 0; i < planes.Count; i++)
            {
                Color planeColour = colours[planes[i].type];

                GL.Begin(BeginMode.Triangles);
                GL.Color3(planeColour);
                GL.Vertex3(planes[i].point1 / 5);
                GL.Color3(planeColour);
                GL.Vertex3(planes[i].point2 / 5);
                GL.Color3(planeColour);
                GL.Vertex3(planes[i].point3 / 5);
            }

            foreach (int idx in lbxPlanes.SelectedIndices)
            {
                GL.Begin(BeginMode.Triangles);
                GL.Color3(Color.Orange);
                GL.Vertex3(planes[idx].point1 / 5);
                GL.Color3(Color.Orange);
                GL.Vertex3(planes[idx].point2 / 5);
                GL.Color3(Color.Orange);
                GL.Vertex3(planes[idx].point3 / 5);
            }

            GL.End();

            glModelView.SwapBuffers();
        }

        //Code for moving the camera, rotating etc.
        #region Moving Camera

        private void glModelView_MouseDown(object sender, MouseEventArgs e)
        {
            if (m_MouseDown != MouseButtons.None) return;
            m_MouseDown = e.Button;
            m_LastMouseClick = e.Location;
            m_LastMouseMove = e.Location;
        }

        private void glModelView_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != m_MouseDown) return;
            m_MouseDown = MouseButtons.None;
            m_UnderCursor = 0xFFFFFFFF;
        }

        private void glModelView_MouseMove(object sender, MouseEventArgs e)
        {
            float xdelta = (float)(e.X - m_LastMouseMove.X);
            float ydelta = (float)(e.Y - m_LastMouseMove.Y);

            m_MouseCoords = e.Location;
            m_LastMouseMove = e.Location;

            if (m_MouseDown != MouseButtons.None)
            {
                if (m_MouseDown == MouseButtons.Right)
                {
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

            glModelView.Refresh();
        }

        private void glModelView_MouseWheel(object sender, MouseEventArgs e)
        {
            float delta = -((e.Delta / 120.0f) * 0.1f);
            m_CamTarget.X += delta * (float)Math.Cos(m_CamRotation.X) * (float)Math.Cos(m_CamRotation.Y);
            m_CamTarget.Y += delta * (float)Math.Sin(m_CamRotation.Y);
            m_CamTarget.Z += delta * (float)Math.Sin(m_CamRotation.X) * (float)Math.Cos(m_CamRotation.Y);

            UpdateCamera();
            glModelView.Refresh();
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

            glModelView.Refresh();
        }

        // camera
        private Vector2 m_CamRotation;
        private Vector3 m_CamTarget;
        private float m_CamDistance;
        private Vector3 m_CamPosition;
        private bool m_UpsideDown;
        private Matrix4 m_CamMatrix;
        private float m_PixelFactorX, m_PixelFactorY;

        // mouse
        private MouseButtons m_MouseDown;
        private Point m_LastMouseClick, m_LastMouseMove;
        private Point m_MouseCoords;
        private uint m_UnderCursor;

        private uint[] m_PickingFrameBuffer;

        #endregion

        private void btnSave_Click(object sender, EventArgs e)
        {
            int lastChange;
            int.TryParse(txtColType.Text, out lastChange);
            planes[lbxPlanes.SelectedIndex].type = lastChange;//Make sure to get value of current plane
            writeChanges();

            loadKCL(kclFile);
            glModelView.Refresh();
        }

        void txtColType_TextChanged(object sender, System.EventArgs e)
        {
            int newColType;
            int.TryParse(txtColType.Text, out newColType);
            foreach (int idx in lbxPlanes.SelectedIndices)
            {
                planes[idx].type = newColType;
            }
        }

        private void lbxPlanes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbxPlanes.SelectedIndices.Count == 1)
            {
                int selPos = lbxPlanes.SelectedIndex;

                txtV1.Text = planes[selPos].point1.ToString();
                txtV2.Text = planes[selPos].point2.ToString();
                txtV3.Text = planes[selPos].point3.ToString();
                txtColType.Text = planes[selPos].type.ToString();
                txtNormal.Text = planes[selPos].normal.ToString();
                txtD1.Text = planes[selPos].dir1.ToString();
                txtD2.Text = planes[selPos].dir2.ToString();
                txtD3.Text = planes[selPos].dir3.ToString();
            }

            glModelView.Refresh();
        }

        private void cmbPolygonMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbPolygonMode.SelectedIndex == 0)
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            else if (cmbPolygonMode.SelectedIndex == 1)
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            else if (cmbPolygonMode.SelectedIndex == 2)
                GL.PolygonMode(MaterialFace.Front, PolygonMode.Line);

            glModelView.Refresh();
        }

        private void btnExportToOBJ_Click(object sender, EventArgs e)
        {
            KCL_Exporter.ExportKCLToOBJ(planes, colours);
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            using (var form = new ROMFileSelect("Please select a collision map (KCL) file to open."))
            {
                var result = form.ShowDialog();
                if (result == DialogResult.OK)
                {
                    loadKCL(Program.m_ROM.GetFileFromName(form.m_SelectedFile));
                }
            }
        }

        private void btnOpenOBJ_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Supported Models (*.obj, *.dae)|*.obj;*.dae";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                txtModelName.Text = ofd.FileName;
                String modelFormat = ofd.FileName.Substring(ofd.FileName.Length - 3, 3).ToLower();
                switch (modelFormat)
                {
                    case "obj":
                        getMatNames_OBJ(ofd.FileName);
                        break;
                    case "dae":
                        getMatNames_DAE(ofd.FileName);
                        break;
                    default:
                        getMatNames_OBJ(ofd.FileName);
                        break;
                }
                populateColTypes();
            }
        }

        private void getMatNames_OBJ(String name)
        {
            Stream fs = File.OpenRead(name);
            StreamReader sr = new StreamReader(fs);

            CultureInfo usahax = new CultureInfo("en-US");

            string curline;
            while ((curline = sr.ReadLine()) != null)
            {
                curline = curline.Trim();

                // skip empty lines and comments
                if (curline.Length < 1) continue;
                if (curline[0] == '#') continue;

                string[] parts = curline.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 1) continue;

                if (parts[0].Equals("usemtl"))
                {
                    if (parts.Length < 2) continue;
                    if (!matColTypes.ContainsKey(parts[1]))
                        matColTypes.Add(parts[1], 0);
                }
            }

            sr.Close();
        }

        private void getMatNames_DAE(String name)
        {
            using (XmlReader reader = XmlReader.Create(name))
            {
                reader.MoveToContent();

                while (reader.Read())
                {
                    if (reader.NodeType.Equals(XmlNodeType.Element))
                    {
                        if (reader.LocalName.Equals("effect"))
                        {
                            // Get the material name by removeing "-effect" from the end of the effect node's ID
                            String material_name = Regex.Replace(reader.GetAttribute("id"), @"-effect$", String.Empty);
                            if (!matColTypes.ContainsKey(material_name))
                                matColTypes.Add(material_name, 0);
                        }
                    }
                }
            }
        }

        private void populateColTypes()
        {
            gridColTypes.ColumnCount = 2;
            gridColTypes.Columns[0].HeaderText = "Material";
            gridColTypes.Columns[1].HeaderText = "Col. Type";

            int numMats = matColTypes.Count;
            gridColTypes.RowCount = numMats;
            for (int i = 0; i < numMats; i++)
            {
                gridColTypes.Rows[i].Cells[0].Value = matColTypes.Keys.ElementAt(i);
                gridColTypes.Rows[i].Cells[1].Value = matColTypes.Values.ElementAt(i);
            }
        }

        private void btnImportColMap_Click(object sender, EventArgs e)
        {
            float scale;
            if (!(float.TryParse(txtScale.Text, out scale) || float.TryParse(txtScale.Text, NumberStyles.Float, new CultureInfo("en-US"), out scale)))
            {
                MessageBox.Show("Please enter a valid float value for scale, eg. 1.23");
            }
            float faceSizeThreshold;
            if (!(float.TryParse(txtThreshold.Text, out faceSizeThreshold) || float.TryParse(txtThreshold.Text, NumberStyles.Float, new CultureInfo("en-US"), out faceSizeThreshold)))
            {
                MessageBox.Show("Please enter a valid float value, eg. 1.23");
            }

            try
            {
                KCL_Importer.ConvertToKCL(txtModelName.Text, ref kclFile, scale, faceSizeThreshold, matColTypes);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.Source + ex.StackTrace);
            }

            loadKCL(kclFile);
        }

        private void btnAssignTypes_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < gridColTypes.RowCount; i++)
            {
                matColTypes[gridColTypes.Rows[i].Cells[0].Value.ToString()] = int.Parse(gridColTypes.Rows[i].Cells[1].Value.ToString());
            }
        }

    }
}
