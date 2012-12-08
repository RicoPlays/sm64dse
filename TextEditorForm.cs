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

            uint straddr = file.Read32((uint)(0x30 + lbxMsgList.SelectedIndex * 8));//Header entry address
            straddr += 0x20 + inf1size + 0x8;

            ushort numentries = file.Read16(0x28);//Number of strings (2 bytes)
            uint headerAddress = straddr - 0x10;//Location of offset to string data (4 bytes)
            uint strOffset = file.Read32(headerAddress);
            uint sizeAddress = straddr - 0x04;//Location of size of string entry (4 bytes)
            uint oldSize = file.Read32(sizeAddress);
            uint fileSizeAddress = 0x08;
            uint sizeOfFile = file.Read32(fileSizeAddress);//Total size of the file
            uint newStrSize;//Will hold the updated size of the string entry

            /* NOT WORKING
            newStrSize = (uint)newTextByte.Length*8 - oldSize;//Times 8 to get bytes
            if (newStrSize < 0)
            {
                int temp = (int)newStrSize * (-1);
                newStrSize = (uint)temp;//If negative, make it positive
                file.Write32(sizeAddress, newStrSize);//Write the new size to file
            }
            else
                file.Write32(sizeAddress, newStrSize);

            file.Write32(fileSizeAddress, (sizeOfFile + newStrSize));//Update new file size

            //Update all string entries after the current one and update their offsets
            for (int i = 0; i < numentries; i++)
            {
                uint nextHeaderAddr = headerAddress + 8;//Point to next string header
                uint theStrOff = file.Read32(nextHeaderAddr);
                file.Write32(nextHeaderAddr, (theStrOff + newStrSize));
            }
            */

            bool flagSpecialChar = false;//For inserting New Line and special DS-specific characters

            //increase straddr each time and write8 the current char
            for (int i = 0; i < newTextByte.Length; i++)
            {
                byte byteToWrite = 0;

                if (!flagSpecialChar)
                {
                    //Upper
                    //nintendo encoding = ('A' + cur - 0x0A);
                    //ascii = A + ne - 0x0A
                    //ascii - A + 0x0A = ne
                    if (Char.IsNumber(newTextByte[i]))//Numeric
                        byteToWrite = (byte)(newTextByte[i] - '0');
                    else if (newTextByte[i] >= 0x41 && newTextByte[i] <= 0x5A)//Uppercase
                        byteToWrite = (byte)(newTextByte[i] - 'A' + 0x0A);
                    else if (newTextByte[i] >= 0x61 && newTextByte[i] <= 0x7A)//Lowercase
                        byteToWrite = (byte)(newTextByte[i] - 'a' + 0x2D);
                    else if (newTextByte[i] >= 0x80 && newTextByte[i] < (0xFF + 0x01))//Extended characters 128 to 255
                        byteToWrite = (byte)(newTextByte[i] - 0x30);//Character - offset of 0x30 to get Nintendo character
                    else if (newTextByte[i].Equals('\r'))//New Line is \r\n and also using \r for special DS-only characters like D-Pad icon
                    {
                        flagSpecialChar = true;
                        continue;//Now go check the next character
                    }

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
                }
                else if (flagSpecialChar)
                {
                    if (newTextByte[i] == '\n')
                    {
                        byteToWrite = 0xFD;
                    }
                    //'\r' followed by a number refers to the DS-only characters
                    //Some of these characters are actually 2 characters long
                    else
                    {
                        int specialCharOffset = 0x42;
                        //Difficulty dealing with the characters past '?' - Rom crashes
                        
                        byteToWrite = (byte)(newTextByte[i] - specialCharOffset);
                    }
                    flagSpecialChar = false;//Back to normal text
                }

                file.Write8(straddr, byteToWrite);//Write the current byte
                straddr++;
            }
            file.Write8(straddr, 0xFF);//End of message

            file.SaveChanges();
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

    }
}
