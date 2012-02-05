using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SM64DSe
{
    public class NitroFile : INitroROMBlock
    {
        public NitroFile() { }

        public NitroFile(NitroROM rom, ushort id)
        {
            if (id >= 0xF000)
                throw new Exception("NitroFile: invalid file ID");

            m_ROM = rom;
            m_ID = id;
            m_Name = m_ROM.GetFileNameFromID(id);
            m_Data = m_ROM.ExtractFile(m_ID);

            if (Read32(0x0) == 0x37375A4C)
                LZ77.Decompress(ref m_Data, true);
        }

        public void ForceDecompression()
        {
            LZ77.Decompress(ref m_Data, false);
        }

        public virtual void SaveChanges()
        {
            // TODO: LZ77 recompression!

            m_ROM.ReinsertFile(m_ID, m_Data);
        }


        public ushort m_ID;
        public string m_Name;
    }
}
