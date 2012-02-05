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

namespace SM64DSe
{
    public partial class AnimationEditorForm : Form
    {
        private bool m_GLLoaded;
        private float m_AspectRatio;


        public AnimationEditorForm()
        {
            InitializeComponent();
        }


        private void glModelView_Load(object sender, EventArgs e)
        {
            m_GLLoaded = true;

            GL.ClearDepth(1f);
            GL.ClearColor(0f, 0f, 0.125f, 1f);

            Matrix4 cam = Matrix4.LookAt(0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref cam);
        }

        private void glModelView_Resize(object sender, EventArgs e)
        {
            if (!m_GLLoaded) return;
            glModelView.Context.MakeCurrent(glModelView.WindowInfo);

            GL.Viewport(glModelView.ClientRectangle);

            m_AspectRatio = (float)glModelView.Width / (float)glModelView.Height;
            GL.MatrixMode(MatrixMode.Projection);
            Matrix4 projmtx = Matrix4.CreatePerspectiveFieldOfView((float)(70f * Math.PI / 180f), m_AspectRatio, 0.01f, 1000f);
            GL.LoadMatrix(ref projmtx);

           // m_PixelFactorX = ((2f * (float)Math.Tan((35f * Math.PI) / 180f) * m_AspectRatio) / (float)(glModelView.Width));
           // m_PixelFactorY = ((2f * (float)Math.Tan((35f * Math.PI) / 180f)) / (float)(glModelView.Height));
        }

        private void glModelView_Paint(object sender, PaintEventArgs e)
        {
            if (!m_GLLoaded) return;
            glModelView.Context.MakeCurrent(glModelView.WindowInfo);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // todo paint the damn thing in here
            GL.Begin(BeginMode.Triangles);
            GL.Color3(Color.Red);
            GL.Vertex3(0f, 1f, 0f);
            GL.Color3(Color.Green);
            GL.Vertex3(-1f, -1f, 0f);
            GL.Color3(Color.Blue);
            GL.Vertex3(1f, -1f, 0f);
            GL.End();

            glModelView.SwapBuffers();
        }
    }
}
