using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SM64DSe.ImportExport.Writers.ExternalWriters
{
    public class DAEWriter : AbstractModelWriter
    {
        public DAEWriter(ModelBase model, string modelFileName) :
            base(model, modelFileName)
        {
            //
        }

        public override void WriteModel(bool save = true)
        {
            throw new NotImplementedException();
        }
    }
}
