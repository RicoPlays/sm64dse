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
                cmbLanguages.Items.Add(langs[i]);
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
                        }
                    }

                    if (thechar != '\0')
                        thetext += thechar;
                    else if (cur == 0xFD)
                        thetext += "\r\n";
                    else if (cur == 0xFF)
                        break;
                    else
                        thetext += String.Format("[0x{0:X2}]", cur);
                }

                m_MsgData[i] = thetext;
                m_StringLengths[i] = length;

                string shortversion = thetext.Replace("\r\n", " ");
                shortversion = (thetext.Length > limit) ? thetext.Substring(0, limit - 3) + "..." : thetext;
                lbxMsgList.Items.Add(string.Format("[{0:X4}] {1}", i, shortversion));
            }
        }

        private List<byte> EncodeString(String msg)
        {
            String newMsg = msg;
            char[] newTextByte = msg.ToCharArray();
            List<byte> encodedString = new List<byte>();
            

            bool flagSpecialChar = false;// For inserting New Line and special DS-specific characters

            for (int i = 0; i < newTextByte.Length; i++)
            {
                byte byteToWrite = 0;

                if (!flagSpecialChar)
                {
                    // Upper
                    // nintendo encoding = ('A' + cur - 0x0A);
                    // ascii = A + ne - 0x0A
                    // ascii - A + 0x0A = ne
                    if (Char.IsNumber(newTextByte[i]))// Numeric
                        byteToWrite = (byte)(newTextByte[i] - '0');
                    else if (newTextByte[i] >= 0x41 && newTextByte[i] <= 0x5A)//Uppercase
                        byteToWrite = (byte)(newTextByte[i] - 'A' + 0x0A);
                    else if (newTextByte[i] >= 0x61 && newTextByte[i] <= 0x7A)// Lowercase
                        byteToWrite = (byte)(newTextByte[i] - 'a' + 0x2D);
                    else if (newTextByte[i] >= 0x80 && newTextByte[i] < (0xFF + 0x01))// Extended characters 128 to 255
                        byteToWrite = (byte)(newTextByte[i] - 0x30);// Character - offset of 0x30 to get Nintendo character
                    else if (newTextByte[i].Equals('\r'))// New Line is \r\n and also using \r for special DS-only characters like D-Pad icon
                    {
                        flagSpecialChar = true;
                        continue;// Now go check the next character
                    }

                    else// Punctuation and other characters
                    {
                        switch (newTextByte[i])
                        {
                            case '?': byteToWrite = (byte)(newTextByte[i] - '?' + 0x26); break;
                            case '!': byteToWrite = (byte)(newTextByte[i] - '!' + 0x27); break;
                            case '~': byteToWrite = (byte)(newTextByte[i] - '~' + 0x28); break;
                            case ',': byteToWrite = (byte)(newTextByte[i] - ',' + 0x29); break;
                            case '“': byteToWrite = (byte)(newTextByte[i] - '“' + 0x2A); break;
                            case '”': byteToWrite = (byte)(newTextByte[i] - '”' + 0x2B); break;

                            case '-': byteToWrite = (byte)(newTextByte[i] - '-' + 0x47); break;
                            case '.': byteToWrite = (byte)(newTextByte[i] - '.' + 0x48); break;
                            case '\'': byteToWrite = (byte)(newTextByte[i] - '\'' + 0x49); break;
                            case ':': byteToWrite = (byte)(newTextByte[i] - ':' + 0x4A); break;
                            case ';': byteToWrite = (byte)(newTextByte[i] - ';' + 0x4B); break;
                            case '&': byteToWrite = (byte)(newTextByte[i] - '&' + 0x4C); break;
                            case ' ': byteToWrite = (byte)(newTextByte[i] - ' ' + 0x4D); break;
                            case '/': byteToWrite = (byte)(newTextByte[i] - '/' + 0x4E); break;
                        }
                    }
                }
                else if (flagSpecialChar)
                {
                    if (newTextByte[i] == '\n')
                    {
                        byteToWrite = 0xFD;
                    }
                    // '\r' followed by a number refers to the DS-only characters
                    // Some of these characters are actually 2 characters long
                    else
                    {
                        int specialCharOffset = 0x42;
                        // Difficulty dealing with the characters past '?' - Rom crashes

                        byteToWrite = (byte)(newTextByte[i] - specialCharOffset);
                    }
                    flagSpecialChar = false;// Back to normal text
                }

                encodedString.Add(byteToWrite);
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
                updateEntries();
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
            ReadStrings("data/message/msg_data_" + langNames[cmbLanguages.SelectedIndex] + ".bin");//Reload texts after saving
            lbxMsgList.SelectedIndex = index;
        }

        private void updateEntries()
        {
            m_MsgData[selectedIndex] = txtEdit.Text;
            int lengthDif = txtEdit.Text.Length - m_StringLengths[selectedIndex];
            m_StringLengths[selectedIndex] += lengthDif;

            //Make or remove room for the new string if needed (don't need to for last entry)
            if (lengthDif > 0 && selectedIndex != m_MsgData.Length - 1)
            {
                uint curStringStart = m_StringHeaderData[selectedIndex] + m_DAT1Start;
                uint nextStringStart = m_StringHeaderData[selectedIndex + 1] + m_DAT1Start;
                byte[] followingData = file.ReadBlock(nextStringStart, (uint)(file.m_Data.Length - nextStringStart));
                for (int i = (int)curStringStart; i < (int)nextStringStart + lengthDif; i++)
                {
                    file.Write8((uint)i, 0);// Fill the gap with zeroes
                }
                file.WriteBlock((uint)(nextStringStart + lengthDif), followingData);
            }
            else if (lengthDif < 0 && selectedIndex != m_MsgData.Length - 1)
            {
                uint nextStringStart = m_StringHeaderData[selectedIndex + 1] + m_DAT1Start;
                byte[] followingData = file.ReadBlock(nextStringStart, (uint)(file.m_Data.Length - nextStringStart));
                file.WriteBlock((uint)(nextStringStart - lengthDif), followingData);
                int oldSize = file.m_Data.Length;
                Array.Resize(ref file.m_Data, oldSize - lengthDif);// Remove duplicate data at end of file
            }

            // Update pointers to string entry data
            if (lengthDif != 0)
            {
                for (int i = selectedIndex + 1; i < m_MsgData.Length; i++)
                {
                    if (lengthDif > 0)
                        m_StringHeaderData[i] += (uint)lengthDif;
                    else if (lengthDif < 0)
                        m_StringHeaderData[i] -= (uint)lengthDif;

                    file.Write32(m_StringHeaderAddr[i], m_StringHeaderData[i]);
                }
            }
            // Update total file size
            file.Write32(0x08, (uint)(int)(file.Read32(0x08) + lengthDif));
        }

        private void writeData()
        {
            // Encode and write all edited string entries
            foreach (int index in m_EditedEntries)
            {
                List<byte> entry = EncodeString(m_MsgData[index]);
                file.WriteBlock(m_StringHeaderData[index] + m_DAT1Start, entry.ToArray<byte>());
            }

            // Save changes
            file.SaveChanges();

            //Console.WriteLine("First header address:\t" + m_StringHeadersStart);
            //for (int i = 0; i < m_StringHeaderAddr.Length; i++ )
            //{
            //    Console.WriteLine("Header address " + i + ":\t" + m_StringHeaderAddr[i]);
            //    Console.WriteLine("Header data " + i + ":\t" + m_StringHeaderData[i]);
            //    //Console.WriteLine("Message " + i + ":\t" + m_MsgData[i]);
            //}
        }

        private void btnCoins_Click(object sender, EventArgs e)
        {
            txtEdit.Text = txtEdit.Text + '\r' + '0' + '\r' + '1';
        }

        private void btnStarFull_Click(object sender, EventArgs e)
        {
            txtEdit.Text = txtEdit.Text + '\r' + '2';
        }

        private void btnStarEmpty_Click(object sender, EventArgs e)
        {
            txtEdit.Text = txtEdit.Text + '\r' + '3';
        }

        private void btnDPad_Click(object sender, EventArgs e)
        {
            txtEdit.Text = txtEdit.Text + '\r' + '4' + '\r' + '5';
        }

        private void btnA_Click(object sender, EventArgs e)
        {
            txtEdit.Text = txtEdit.Text + '\r' + '6' + '\r' + '7';
        }

        private void btnB_Click(object sender, EventArgs e)
        {
            txtEdit.Text = txtEdit.Text + '\r' + '8' + '\r' + '9';
        }

        private void btnX_Click(object sender, EventArgs e)
        {
            txtEdit.Text = txtEdit.Text + '\r' + ':' + '\r' + ';';
        }

        private void btnY_Click(object sender, EventArgs e)
        {
            txtEdit.Text = txtEdit.Text + '\r' + '<' + '\r' + '=';
        }

        private void btnL_Click(object sender, EventArgs e)
        {
            txtEdit.Text = txtEdit.Text + '\r' + '>' + '\r' + '?' + '\r' + '@';
        }

        private void btnR_Click(object sender, EventArgs e)
        {
            txtEdit.Text = txtEdit.Text + '\r' + 'A' + '\r' + 'B' + '\r' + 'C';
        }

        private void cmbLanguages_SelectedIndexChanged(object sender, EventArgs e)
        {
            ReadStrings("data/message/msg_data_" + langNames[cmbLanguages.SelectedIndex] + ".bin");
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
                            "When reading, their codes are as follows:\n\n" +
                            "[0xEE][0xEF] \t Coins\n[0xF0] \t\t Star Full\n[0xF1] \t\t Star Empty\n" +
                            "[0xF2][0xF3] \t D-Pad\n[0xF4][0xF5] \t A\n[0xF6][0xF7] \t B\n" +
                            "[0xF8][0xF9] \t X\n[0xFA][0xFB] \t Y");
        }

    }
}
