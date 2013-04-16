/*
    Copyright 2012 Kuribo64

    This file is part of SM64DSe.

    SM64DSe is free software: you can redistribute it and/or modify it under
    the terms of the GNU General Public License as published by the Free
    Software Foundation, either version 3 of the License, or (at your option)
    any later version.

    SM64DSe is distributed in the hope that it will be useful, but WITHOUT ANY 
    WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS 
    FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

    You should have received a copy of the GNU General Public License along 
    with SM64DSe. If not, see http://www.gnu.org/licenses/.
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace SM64DSe
{
    public partial class TextEditorForm : Form
    {
        public TextEditorForm()
        {
            InitializeComponent();
        }

        string[] m_MsgData;
        int[] m_StringLengths;
        NitroFile file;
        uint inf1size;
        uint m_FileSize;
        uint m_DAT1Start;// Address at which the string data is held
        uint[] m_StringHeaderAddr;// The addresses of the string headers
        uint[] m_StringHeaderData;// The offsets of the strings (relative to start of DAT1 section)
        List<int> m_EditedEntries = new List<int>();// Holds indices of edited entries, needed because of how old and new strings are stored differently

        String[] langs = new String[0];
        String[] langNames = new String[0];

        int limit = 45;// Length of preview text to be shown
        int selectedIndex;
        int langIndex = -1;

        private void TextEditorForm_Load(object sender, EventArgs e)
        {
            NitroROM.Version theVersion = Program.m_ROM.m_Version;

            if (theVersion == NitroROM.Version.EUR)
            {
                lblVer.Text = "EUR";
                langs = new String[] { "English", "Français", "Deutsch", "Italiano", "Español" };
                langNames = new String[] { "eng", "frn", "gmn", "itl", "spn" };
            }
            else if (theVersion == NitroROM.Version.JAP)
            {
                lblVer.Text = "JAP";
                langs = new String[] { "Japanese (Unsupported)", "English" };
                langNames = new String[] { "jpn", "nes" };
            }
            else if (theVersion == NitroROM.Version.USA_v1)
            {
                lblVer.Text = "USAv1";
                langs = new String[] { "English", "Japanese (Unsupported)" };
                langNames = new String[] { "nes", "jpn" };
            }
            else if (theVersion == NitroROM.Version.USA_v2)
            {
                lblVer.Text = "USAv2";
                langs = new String[] { "English", "Japanese (Unsupported)" };
                langNames = new String[] { "nes", "jpn" };
            }

            for (int i = 0; i < langs.Length; i++)
            {
                btnLanguages.DropDownItems.Add(langs[i]).Tag = i;
            }

        }

        public void ReadStrings(String fileName)
        {
            file = Program.m_ROM.GetFileFromName(fileName);

            inf1size = file.Read32(0x24);
            ushort numentries = file.Read16(0x28);

            m_MsgData = new string[numentries];
            m_StringLengths = new int[numentries];
            m_FileSize = file.Read32(0x08);
            m_StringHeaderAddr = new uint[numentries];
            m_StringHeaderData = new uint[numentries];
            m_DAT1Start = 0x20 + inf1size + 0x08;

            for (int i = 0; i < numentries; i++)
            {
                m_StringHeaderAddr[i] = (uint)(0x20 + 0x10 + (i * 8));
                m_StringHeaderData[i] = file.Read32(m_StringHeaderAddr[i]);
            }

            lbxMsgList.Items.Clear();//Reset list of messages

            for (int i = 0; i < m_MsgData.Length; i++)
            {
                uint straddr = file.Read32((uint)(0x30 + i * 8));
                straddr += 0x20 + inf1size + 0x8;

                int length = 0;

                string thetext = "";
                for (; ; )
                {
                    byte cur;
                    try
                    {
                        cur = file.Read8(straddr);
                    }
                    catch
                    {
                        break;
                    }
                    straddr++;
                    length++;
                    char thechar = '\0';

                    if ((cur >= 0x00) && (cur <= 0x09))
                        thechar = (char)('0' + cur);
                    else if ((cur >= 0x0A) && (cur <= 0x23))
                        thechar = (char)('A' + cur - 0x0A);
                    else if ((cur >= 0x2D) && (cur <= 0x46))
                        thechar = (char)('a' + cur - 0x2D);
                    else if ((cur >= 0x50) && (cur <= 0xCF))//Extended ASCII Characters
                        thechar = (char)(0x30 + cur);
                    else
                    {
                        switch (cur)
                        {
                            case 0x26: thechar = '?'; break;
                            case 0x27: thechar = '!'; break;
                            case 0x28: thechar = '~'; break;
                            case 0x29: thechar = ','; break;
                            case 0x2A: thechar = '“'; break;
                            case 0x2B: thechar = '”'; break;
                            case 0x47: thechar = '-'; break;
                            case 0x48: thechar = '.'; break;
                            case 0x49: thechar = '\''; break;
                            case 0x4A: thechar = ':'; break;
                            case 0x4B: thechar = ';'; break;
                            case 0x4C: thechar = '&'; break;
                            case 0x4D: thechar = ' '; break;
                            case 0x4E: thechar = '/'; break;
                            case 0xEE: thetext += "[\\r]C"; straddr++; length++; break;
                            case 0xF0: thetext += "[\\r]S"; break;
                            case 0xF1: thetext += "[\\r]s"; break;
                            case 0xF2: thetext += "[\\r]D"; straddr++; length++; break;
                            case 0xF4: thetext += "[\\r]A"; straddr++; length++; break;
                            case 0xF6: thetext += "[\\r]B"; straddr++; length++; break;
                            case 0xF8: thetext += "[\\r]X"; straddr++; length++; break;
                            case 0xFA: thetext += "[\\r]Y"; straddr++; length++; break;
                        }
                    }

                    if (thechar != '\0')
                        thetext += thechar;
                    else if (cur == 0xFD)
                        thetext += "\r\n";
                    else if (cur == 0xFF)
                        break;
                    else if (cur == 0xFE)// Special Character
                    {
                        int len = file.Read8(straddr);
                        thetext += "[\\r]";
                        thetext += String.Format("{0:X2}", cur);
                        for (int spec = 0; spec < len - 1; spec++)
                        {
                            thetext += String.Format("{0:X2}", file.Read8((uint)(straddr + spec)));
                        }
                        length += (len - 1);// Already increased by 1 at start
                        straddr += (uint)(len - 1);
                    }
                }

                m_MsgData[i] = thetext;
                m_StringLengths[i] = length;

                string shortversion = thetext.Replace("\r\n", " ");
                shortversion = (thetext.Length > limit) ? thetext.Substring(0, limit - 3) + "..." : thetext;
                lbxMsgList.Items.Add(string.Format("[{0:X4}] {1}", i, shortversion));

                btnImport.Enabled = true; btnExport.Enabled = true;
            }
        }

        private List<byte> EncodeString(String msg)
        {
            String newMsg = msg.Replace("[\\r]", "\r");
            char[] newTextByte = newMsg.ToCharArray();
            List<byte> encodedString = new List<byte>();

            int i = 0;
            while (i < newTextByte.Length)
            {
                // Upper
                // nintendo encoding = ('A' + cur - 0x0A);
                // ascii = A + ne - 0x0A
                // ascii - A + 0x0A = ne
                if (Char.IsNumber(newTextByte[i]))// Numeric
                    encodedString.Add((byte)(newTextByte[i] - '0'));
                else if (newTextByte[i] >= 0x41 && newTextByte[i] <= 0x5A)//Uppercase
                    encodedString.Add((byte)(newTextByte[i] - 'A' + 0x0A));
                else if (newTextByte[i] >= 0x61 && newTextByte[i] <= 0x7A)// Lowercase
                    encodedString.Add((byte)(newTextByte[i] - 'a' + 0x2D));
                else if (newTextByte[i] >= 0x80 && newTextByte[i] < (0xFF + 0x01))// Extended characters 128 to 255
                    encodedString.Add((byte)(newTextByte[i] - 0x30));// Character - offset of 0x30 to get Nintendo character
                else if (newTextByte[i].Equals('\r'))// New Line is \r\n
                {
                    i++;// Point after r
                    if (newTextByte[i].Equals('\n'))
                    {
                        encodedString.Add((byte)0xFD);
                        i++;
                        continue;
                    }
                    // 0xFE denotes special character
                    else if (newTextByte[i].Equals('F') && newTextByte[i + 1].Equals('E'))
                    {
                        //FE 05 03 00 06 - [R) glyph
                        //FE 07 01 00 00 00 XX - number of stars till you get XX
                        String byte2 = "" + newTextByte[i + 2] + newTextByte[i + 3];
                        int len = int.Parse(byte2, System.Globalization.NumberStyles.HexNumber);
                        for (int j = 0; j < (len * 2); j += 2)
                        {
                            String temp = "" + newTextByte[i + j] + newTextByte[i + j + 1];
                            encodedString.Add((byte)int.Parse(temp, System.Globalization.NumberStyles.HexNumber));
                        }
                        i += (len * 2);

                        continue;
                    }
                    else
                    {
                        // 0x42
                        int special = 0x42;
                        switch (newTextByte[i])
                        {
                            case 'C':
                                encodedString.Add((byte)(48 - special));
                                encodedString.Add((byte)(49 - special));
                                break;
                            case 'S':
                                encodedString.Add((byte)(50 - special));
                                break;
                            case 's':
                                encodedString.Add((byte)(51 - special));
                                break;
                            case 'D':
                                encodedString.Add((byte)(52 - special));
                                encodedString.Add((byte)(53 - special));
                                break;
                            case 'A':
                                encodedString.Add((byte)(54 - special));
                                encodedString.Add((byte)(55 - special));
                                break;
                            case 'B':
                                encodedString.Add((byte)(56 - special));
                                encodedString.Add((byte)(57 - special));
                                break;
                            case 'X':
                                encodedString.Add((byte)(58 - special));
                                encodedString.Add((byte)(59 - special));
                                break;
                            case 'Y':
                                encodedString.Add((byte)(60 - special));
                                encodedString.Add((byte)(61 - special));
                                break;
                        }
                        i++;
                        continue;
                    }
                }

                else// Punctuation and other characters
                {
                    switch (newTextByte[i])
                    {
                        case '?': encodedString.Add((byte)(newTextByte[i] - '?' + 0x26)); break;
                        case '!': encodedString.Add((byte)(newTextByte[i] - '!' + 0x27)); break;
                        case '~': encodedString.Add((byte)(newTextByte[i] - '~' + 0x28)); break;
                        case ',': encodedString.Add((byte)(newTextByte[i] - ',' + 0x29)); break;
                        case '“': encodedString.Add((byte)(newTextByte[i] - '“' + 0x2A)); break;
                        case '”': encodedString.Add((byte)(newTextByte[i] - '”' + 0x2B)); break;

                        case '-': encodedString.Add((byte)(newTextByte[i] - '-' + 0x47)); break;
                        case '.': encodedString.Add((byte)(newTextByte[i] - '.' + 0x48)); break;
                        case '\'': encodedString.Add((byte)(newTextByte[i] - '\'' + 0x49)); break;
                        case ':': encodedString.Add((byte)(newTextByte[i] - ':' + 0x4A)); break;
                        case ';': encodedString.Add((byte)(newTextByte[i] - ';' + 0x4B)); break;
                        case '&': encodedString.Add((byte)(newTextByte[i] - '&' + 0x4C)); break;
                        case ' ': encodedString.Add((byte)(newTextByte[i] - ' ' + 0x4D)); break;
                        case '/': encodedString.Add((byte)(newTextByte[i] - '/' + 0x4E)); break;
                    }
                }
                i++;
            }

            encodedString.Add(0xFF);// End of message

            return encodedString;
        }

        private void lbxMsgList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbxMsgList.SelectedIndex != -1)
            {
                selectedIndex = lbxMsgList.SelectedIndex;
                tbxMsgPreview.Text = m_MsgData[selectedIndex];
            }
        }

        private void btnUpdateString_Click(object sender, EventArgs e)
        {
            if (lbxMsgList.SelectedIndex != -1)
            {
                updateEntries(txtEdit.Text, selectedIndex);
                m_EditedEntries.Add(selectedIndex);
                string shortversion = m_MsgData[selectedIndex].Replace("\r\n", " ");
                shortversion = (m_MsgData[selectedIndex].Length > limit) ? m_MsgData[selectedIndex].Substring(0, limit - 3) + "..." : m_MsgData[selectedIndex];
                lbxMsgList.Items[selectedIndex] = string.Format("[{0:X4}] {1}", selectedIndex, shortversion);
            }
        }

        private void btnSaveAll_Click(object sender, EventArgs e)
        {
            writeData();

            int index = lbxMsgList.SelectedIndex;
            ReadStrings("data/message/msg_data_" + langNames[langIndex] + ".bin");//Reload texts after saving
            lbxMsgList.SelectedIndex = index;
        }

        private void updateEntries(String msg, int index)
        {
            m_MsgData[index] = msg;
            int lengthDif = EncodeString(msg).Count - m_StringLengths[index];
            m_StringLengths[index] += lengthDif;

            //Make or remove room for the new string if needed (don't need to for last entry)
            if (lengthDif > 0 && index != m_MsgData.Length - 1)
            {
                uint curStringStart = m_StringHeaderData[index] + m_DAT1Start;
                uint nextStringStart = m_StringHeaderData[index + 1] + m_DAT1Start;
                byte[] followingData = file.ReadBlock(nextStringStart, (uint)(file.m_Data.Length - nextStringStart));
                for (int i = (int)curStringStart; i < (int)nextStringStart + lengthDif; i++)
                {
                    file.Write8((uint)i, 0);// Fill the gap with zeroes
                }
                file.WriteBlock((uint)(nextStringStart + lengthDif), followingData);
            }
            else if (lengthDif < 0 && index != m_MsgData.Length - 1)
            {
                // lengthDif is negative, -- +
                uint nextStringStart = m_StringHeaderData[index + 1] + m_DAT1Start;
                byte[] followingData = file.ReadBlock(nextStringStart, (uint)(file.m_Data.Length - nextStringStart));
                file.WriteBlock((uint)(nextStringStart + lengthDif), followingData);
                int oldSize = file.m_Data.Length;
                Array.Resize(ref file.m_Data, oldSize + lengthDif);// Remove duplicate data at end of file
            }

            // Update pointers to string entry data
            if (lengthDif != 0)
            {
                for (int i = index + 1; i < m_MsgData.Length; i++)
                {
                    if (lengthDif > 0)
                        m_StringHeaderData[i] += (uint)lengthDif;
                    else if (lengthDif < 0)
                        m_StringHeaderData[i] = (uint)(m_StringHeaderData[i] + lengthDif);

                    file.Write32(m_StringHeaderAddr[i], m_StringHeaderData[i]);
                }
            }
            // Update total file size
            file.Write32(0x08, (uint)(int)(file.Read32(0x08) + lengthDif));
            // Update DAT1 size
            file.Write32(m_DAT1Start - 0x04, (uint)(int)(file.Read32(m_DAT1Start - 0x04) + lengthDif));
        }

        private void writeData()
        {
            // Encode and write all edited string entries
            foreach (int index in m_EditedEntries)
            {
                List<byte> entry = EncodeString(m_MsgData[index]);
                file.WriteBlock(m_StringHeaderData[index] + m_DAT1Start, entry.ToArray<byte>());
            }

            // Compress file
            //file.Compress();

            // Save changes
            file.SaveChanges();
        }

        private void btnCoins_Click(object sender, EventArgs e)
        {
            txtEdit.Text += "[\\r]C";
        }

        private void btnStarFull_Click(object sender, EventArgs e)
        {
            txtEdit.Text += "[\\r]S";
        }

        private void btnStarEmpty_Click(object sender, EventArgs e)
        {
            txtEdit.Text += "[\\r]s";
        }

        private void btnDPad_Click(object sender, EventArgs e)
        {
            // FE05030000 doesn't always appear, depends on message
            txtEdit.Text += "[\\r]D";
        }

        private void btnA_Click(object sender, EventArgs e)
        {
            txtEdit.Text += "[\\r]A";
        }

        private void btnB_Click(object sender, EventArgs e)
        {
            txtEdit.Text += "[\\r]B";
        }

        private void btnX_Click(object sender, EventArgs e)
        {
            txtEdit.Text += "[\\r]X";
        }

        private void btnY_Click(object sender, EventArgs e)
        {
            txtEdit.Text += "[\\r]Y";
        }

        private void btnL_Click(object sender, EventArgs e)
        {
            txtEdit.Text += "[\\r]FE05030005";
        }

        private void btnR_Click(object sender, EventArgs e)
        {
            txtEdit.Text += "[\\r]FE05030006";
        }

        void btnLanguages_DropDownItemClicked(object sender, System.Windows.Forms.ToolStripItemClickedEventArgs e)
        {
            langIndex = int.Parse(e.ClickedItem.Tag.ToString());
            ReadStrings("data/message/msg_data_" + langNames[int.Parse(e.ClickedItem.Tag.ToString())] + ".bin");
        }
        private void btnHelp_Click(object sender, EventArgs e)
        {
            MessageBox.Show("To begin editing, select a language using the drop-down menu. This will display \n" +
                            "all of the languages available for your ROM Version.\n\n" +
                            "Next, click on the string you want to edit on the left-hand side.\n" +
                            "The full text will then be displayed in the upper-right box.\n\n" +
                            "Type your new text in the text box on the right-hand side.\n" +
                            "When done editing an entry, click 'Update String'.\n\nWhen you have finished, click " +
                            "on 'Save Changes'\n\n" +
                            "Use the buttons under the text editing box to insert the special characters.\n" + 
                            "[\\r] is the special character used by the text editor to indicate special characters.\n" + 
                            "When reading, their codes are as follows:\n\n" +
                            "[0xEE][0xEF] \t Coins\n[0xF0] \t\t Star Full\n[0xF1] \t\t Star Empty\n" +
                            "[0xF2][0xF3] \t D-Pad\n[0xF4][0xF5] \t A\n[0xF6][0xF7] \t B\n" +
                            "[0xF8][0xF9] \t X\n[0xFA][0xFB] \t Y");
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            ImportXML();
        }

        private void ImportXML()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "XML Document (.xml)|*.xml";//Filter by .xml
            DialogResult dlgeResult = ofd.ShowDialog();
            if (dlgeResult == DialogResult.Cancel)
                return;

            lbxMsgList.Items.Clear();

            using (XmlReader reader = XmlReader.Create(ofd.FileName))
            {
                reader.MoveToContent();

                int i = 0;
                while (reader.Read())
                {
                    if (reader.NodeType.Equals(XmlNodeType.Element))
                    {
                        switch (reader.LocalName)
                        {
                            case "Text":
                                if (i < m_MsgData.Length)
                                {
                                    String temp = reader.ReadElementContentAsString();
                                    temp = temp.Replace("\n", "\r\n");
                                    temp = temp.Replace("[\\r]", "\r");
                                    m_MsgData[i] = temp;
                                    string shortversion = m_MsgData[i].Replace("\r\n", " ");
                                    shortversion = (m_MsgData[i].Length > limit) ? m_MsgData[i].Substring(0, limit - 3) + "..." : m_MsgData[i];
                                    lbxMsgList.Items.Add(string.Format("[{0:X4}] {1}", i, shortversion));
                                }
                                i++;
                                break;
                        }
                    }
                }
            }

            for (int i = 0; i < m_MsgData.Length; i++)
            {
                updateEntries(m_MsgData[i], i);
                List<byte> entry = EncodeString(m_MsgData[i]);
                file.WriteBlock(m_StringHeaderData[i] + m_DAT1Start, entry.ToArray<byte>());
            }

            file.SaveChanges();

            ReadStrings("data/message/msg_data_" + langNames[langIndex] + ".bin");//Reload texts after saving
        }

        private void ExportXML()
        {
            SaveFileDialog saveXML = new SaveFileDialog();
            saveXML.FileName = "SM64DS Texts";//Default name
            saveXML.DefaultExt = ".xml";//Default file extension
            saveXML.Filter = "XML Document (.xml)|*.xml";//Filter by .xml
            if (saveXML.ShowDialog() == DialogResult.Cancel)
                return;

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "  ";
            settings.NewLineChars = "\r\n";
            settings.NewLineHandling = NewLineHandling.Replace;
            using (XmlWriter writer = XmlWriter.Create(saveXML.FileName, settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("SM64DS_Texts");

                for (int i = 0; i < m_MsgData.Length; i++)
                {
                    writer.WriteElementString("Text", m_MsgData[i]);
                }
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            ExportXML();
        }

    }
}
