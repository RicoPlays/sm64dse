using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SM64DSe.ImportExport.Writers.InternalWriters
{
    public class KCLWriter : AbstractModelWriter
    {
        public NitroFile m_ModelFile;

        public KCLWriter(ModelBase model, string modelFileName) :
            base(model, modelFileName)
        {
            m_ModelFile = Program.m_ROM.GetFileFromName(modelFileName);
        }

        public override void WriteModel(bool save = true)
        {
            throw new NotImplementedException();
        }
    }
}
