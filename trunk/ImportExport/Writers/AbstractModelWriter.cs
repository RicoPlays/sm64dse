using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SM64DSe.ImportExport.Writers
{
    public abstract class AbstractModelWriter
    {
        public ModelBase m_Model;

        protected string m_ModelFileName;
        protected string m_ModelPath;

        public AbstractModelWriter(ModelBase model, string modelFileName)
        {
            m_Model = model;

            m_ModelFileName = modelFileName;
            m_ModelPath = Path.GetDirectoryName(m_ModelFileName);
        }

        public abstract void WriteModel(bool save = true);
    }
}
