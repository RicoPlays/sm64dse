using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SM64DSe
{
    public class NARCFile : NitroFile
    {
        public NARCFile(NARC arc, ushort id) : base()
        {
            if (id >= 0xF000)
                throw new Exception("NARCFile: invalid file ID");

            m_Narc = arc;
            m_ID = id;
            m_Name = m_Narc.GetFileNameFromID(id);
            m_Data = m_Narc.ExtractFile(m_ID);

            if (Read32(0x0) == 0x37375A4C)
                LZ77.Decompress(ref m_Data, true);
        }

        public override void SaveChanges()
        {
            // TODO: LZ77 recompression!

            m_Narc.ReinsertFile(m_ID, m_Data);
        }


        public NARC m_Narc;
    }
}
