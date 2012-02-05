using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace SM64DSe
{
    static class ModelCache
    {
        public static void Init()
        {
            m_Models = new Dictionary<string, CachedModel>();
        }

        public static BMD GetModel(string name)
        {
            if (m_Models.ContainsKey(name))
            {
                CachedModel found = m_Models[name];
                found.m_References++;
                return found.m_Model;
            }

            NitroFile mdfile = Program.m_ROM.GetFileFromName(name);
            if (mdfile == null)
                return null;

            BMD model = new BMD(mdfile);
            model.PrepareToRender();

            CachedModel cmdl = new CachedModel();
            cmdl.m_Model = model;
            cmdl.m_DisplayLists = null;
            cmdl.m_References = 1;
            m_Models.Add(name, cmdl);

            return model;
        }

        public static int[] GetDisplayLists(BMD model)
        {
            if (!m_Models.ContainsKey(model.m_FileName))
                return null;

            CachedModel cmdl = m_Models[model.m_FileName];
            if (cmdl.m_DisplayLists != null)
                return cmdl.m_DisplayLists;

            int[] dl = new int[3];
            bool keep = false;

            dl[0] = GL.GenLists(1);
            GL.NewList(dl[0], ListMode.Compile);
            keep = cmdl.m_Model.Render(RenderMode.Opaque, 1f);
            GL.EndList();
            if (!keep) { GL.DeleteLists(dl[0], 1); dl[0] = 0; }

            dl[1] = GL.GenLists(1);
            GL.NewList(dl[1], ListMode.Compile);
            keep = cmdl.m_Model.Render(RenderMode.Translucent, 1f);
            GL.EndList();
            if (!keep) { GL.DeleteLists(dl[1], 1); dl[1] = 0; }

            dl[2] = GL.GenLists(1);
            GL.NewList(dl[2], ListMode.Compile);
            keep = cmdl.m_Model.Render(RenderMode.Picking, 1f);
            GL.EndList();
            if (!keep) { GL.DeleteLists(dl[2], 1); dl[2] = 0; }

            cmdl.m_DisplayLists = dl;
            return dl;
        }

        public static void RemoveModel(BMD model)
        {
            if (!m_Models.ContainsKey(model.m_FileName))
                return;

            CachedModel cmdl = m_Models[model.m_FileName];

            cmdl.m_References--;
            if (cmdl.m_References > 0)
                return;

            if (cmdl.m_DisplayLists != null)
            {
                GL.DeleteLists(cmdl.m_DisplayLists[0], 1);
                GL.DeleteLists(cmdl.m_DisplayLists[1], 1);
                GL.DeleteLists(cmdl.m_DisplayLists[2], 1);
                cmdl.m_DisplayLists = null;
            }

            cmdl.m_Model.Release();

            m_Models.Remove(model.m_FileName);
        }


        private class CachedModel
        {
            public BMD m_Model;
            public int[] m_DisplayLists;
            public int m_References;
        }


        private static Dictionary<string, CachedModel> m_Models;
    }
}
