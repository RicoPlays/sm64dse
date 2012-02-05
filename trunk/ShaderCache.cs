using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace SM64DSe
{
    class ShaderCache
    {
        public ShaderCache()
        {
            m_Shaders = new Hashtable();
        }

        public int BuildShader(string name)
        {
            if (m_Shaders.Contains(name))
            {
                CachedShader found = (CachedShader)m_Shaders[name];
                found.m_References++;
                return found.m_ProgramID;
            }

            CachedShader shader = new CachedShader();

            try 
            { 
                string vshader = Properties.Resources.ResourceManager.GetString(name + "_vert");

                shader.m_VertShaderID = GL.CreateShader(ShaderType.VertexShader);
                GL.ShaderSource(shader.m_VertShaderID, vshader);
                GL.CompileShader(shader.m_VertShaderID);
            }
            catch 
            {
                shader.m_VertShaderID = 0;
            }

            try
            {
                string fshader = Properties.Resources.ResourceManager.GetString(name + "_frag");

                shader.m_FragShaderID = GL.CreateShader(ShaderType.FragmentShader);
                GL.ShaderSource(shader.m_FragShaderID, fshader);
                GL.CompileShader(shader.m_FragShaderID);
            }
            catch
            {
                shader.m_FragShaderID = 0;
            }

            if ((shader.m_VertShaderID == 0) && (shader.m_FragShaderID == 0))
                return 0;

            try
            {
                shader.m_ProgramID = GL.CreateProgram();
                if (shader.m_VertShaderID != 0)
                    GL.AttachShader(shader.m_ProgramID, shader.m_VertShaderID);
                if (shader.m_FragShaderID != 0)
                    GL.AttachShader(shader.m_ProgramID, shader.m_FragShaderID);

                GL.LinkProgram(shader.m_ProgramID);
            }
            catch
            {
                return 0;
            }

            m_Shaders.Add(name, shader);
            return shader.m_ProgramID;
        }

        public void UseShader(string name)
        {
            if (name == "none")
            {
                try
                {
                    GL.UseProgram(0);
                }
                catch { }

                return;
            }

            if (!m_Shaders.Contains(name))
                return;

            try
            {
                GL.UseProgram(((CachedShader)m_Shaders[name]).m_ProgramID);
            }
            catch { }
        }

        public void ReleaseShader(string name)
        {
            if (!m_Shaders.Contains(name))
                return;

            CachedShader shader = (CachedShader)m_Shaders[name];

            shader.m_References--;
            if (shader.m_References > 0)
                return;

            try
            {
                GL.DeleteProgram(shader.m_ProgramID);
                GL.DeleteShader(shader.m_VertShaderID);
                GL.DeleteShader(shader.m_FragShaderID);
            }
            catch { }

            m_Shaders.Remove(name);
        }


        private class CachedShader
        {
            public int m_VertShaderID, m_FragShaderID;
            public int m_ProgramID;
            public int m_References;
        }


        private Hashtable m_Shaders;
    }
}
