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
        NitroFile file;
        int currentIndex;
        uint inf1size;

        private void TextEditorForm_Load(object sender, EventArgs e)
        {
            file = Program.m_ROM.GetFileFromName("data/message/msg_data_eng.bin");

            inf1size = file.Read32(0x24);
            ushort numentries = file.Read16(0x28);
            m_MsgData = new string[numentries];

            for (int i = 0; i < m_MsgData.Length; i++)
            {
                uint straddr = file.Read32((uint)(0x30 + i * 8));
                straddr += 0x20 + inf1size + 0x8;

                string thetext = "";
                for (;;)
			    {
				    byte cur = file.Read8(straddr);
                    straddr++;
				    char thechar = '\0';
    				
				    if ((cur >= 0x00) && (cur <= 0x09))
					    thechar = (char)('0' + cur);
				    else if ((cur >= 0x0A) && (cur <= 0x23))
					    thechar = (char)('A' + cur - 0x0A);
				    else if ((cur >= 0x2D) && (cur <= 0x46))
					    thechar = (char)('a' + cur - 0x2D);
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

                int limit = 45;
                string shortversion = thetext.Replace("\r\n", " ");
                shortversion = (thetext.Length > limit) ? thetext.Substring(0, limit-3) + "..." : thetext;
                lbxMsgList.Items.Add(string.Format("[{0:X4}] {1}", i, shortversion));
            }
        }

        private void lbxMsgList_SelectedIndexChanged(object sender, EventArgs e)
        {
            tbxMsgPreview.Text = m_MsgData[lbxMsgList.SelectedIndex];
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            string newText = txtEdit.Text;
            char[] newTextByte = newText.ToCharArray();
            
            uint straddr = file.Read32((uint)(0x30 + currentIndex * 8));
            straddr += 0x20 + inf1size + 0x8;

            //increase straddr each time and write8 the current char
            for (int i = 0; i < newTextByte.Length; i++)
            {
                byte byteToWrite = 0;

                //Upper
                //nintendo encoding = ('A' + cur - 0x0A);
                //ascii = A + ne - 0x0A
                //ascii - A + 0x0A = ne
                if (Char.IsNumber(newTextByte[i]))//Numeric
                    byteToWrite = (byte)(newTextByte[i] - '0');
                else if (Char.IsUpper(newTextByte[i]))//Uppercase
                    byteToWrite = (byte)(newTextByte[i] - 'A' + 0x0A);
                else if (Char.IsLower(newTextByte[i]))//Lowercase
                    byteToWrite = (byte)(newTextByte[i] - 'a' + 0x2D);
                else if (newTextByte[i].Equals('\r'))//New Line character is \r\n however this gets split into 2 chars
                    continue;//next character will be \n - go check it
                else if (newTextByte[i].Equals('\n'))
                    byteToWrite = 0xFD;

                else//Punctuation and other characters
                {
                    switch (newTextByte[i])
                    {
                        case '?': byteToWrite = 0x26; break;
                        case '!': byteToWrite = 0x27; break;
                        case '~': byteToWrite = 0x28; break;
                        case ',': byteToWrite = 0x29; break;
                        case '“': byteToWrite = 0x2A; break;
                        case '”': byteToWrite = 0x2B; break;

                        case '-': byteToWrite = 0x47; break;
                        case '.': byteToWrite = 0x48; break;
                        case '\'': byteToWrite = 0x49; break;
                        case ':': byteToWrite = 0x4A; break;
                        case ';': byteToWrite = 0x4B; break;
                        case '&': byteToWrite = 0x4C; break;
                        case ' ': byteToWrite = 0x4D; break;
                        case '/': byteToWrite = 0x4E; break;
                    }
                }

                file.Write8(straddr, byteToWrite);//Write the current byte
                straddr++;
            }
            file.Write8(straddr, 0xFF);//End of message

            file.SaveChanges();
        }
    }
}
