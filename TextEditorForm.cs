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
        int[] stringLengths;

        String[] langs = new String[0];
        String[] langNames = new String[0];

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
            stringLengths = new int[numentries];//This will hold the actual amount of bytes in each string

            lbxMsgList.Items.Clear();//Reset list of messages

            for (int i = 0; i < m_MsgData.Length; i++)
            {
                uint straddr = file.Read32((uint)(0x30 + i * 8));
                straddr += 0x20 + inf1size + 0x8;

                int length = 0;

                string thetext = "";
                for (; ; )
                {
                    byte cur = file.Read8(straddr);
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
                stringLengths[i] = length;

                int limit = 45;
                string shortversion = thetext.Replace("\r\n", " ");
                shortversion = (thetext.Length > limit) ? thetext.Substring(0, limit - 3) + "..." : thetext;
                lbxMsgList.Items.Add(string.Format("[{0:X4}] {1}", i, shortversion));
            }
        }

        private void WriteStrings()
        {
            string newText = txtEdit.Text;
            char[] newTextByte = newText.ToCharArray();

            uint straddr = file.Read32((uint)(0x30 + lbxMsgList.SelectedIndex * 8));//Read string offset from header
            straddr += 0x20 + inf1size + 0x8;//Address of string = offset + file header + INF1 section size + DAT1 header size

            ushort numentries = file.Read16(0x28);//Number of strings (2 bytes)
            uint headerAddress = (uint)(0x30 + lbxMsgList.SelectedIndex * 8);//Location of offset to string data (4 bytes)
            uint strOffset = file.Read32(headerAddress);
            uint oldSize = (uint)(stringLengths[lbxMsgList.SelectedIndex]);//Length of original text
            uint fileSizeAddress = 0x08;
            uint sizeOfFile = file.Read32(fileSizeAddress);//Total size of the file
            uint numChars = (uint)(file.Read32((uint)(0x30 + (numentries - 1) * 8)) + stringLengths[stringLengths.Length - 1]);//+1 for end message character
            int newStrSize;//Will hold the updated size of the string entry


            //Work out the difference in message size between new and old text
            newStrSize = (int)((newTextByte.Length + 1) - oldSize);//Plus one for the end of message character

            //Move all data after current entry foward by the difference in size
            if (lbxMsgList.SelectedIndex != numentries - 1)
            {
                uint nextStrStart = file.Read32((uint)(0x30 + (lbxMsgList.SelectedIndex + 1) * 8)) + 0x20 + inf1size + 0x8;
                uint firstStrAddr = (uint)(file.Read32((uint)(0x30 + (0) * 8)) + 0x20 + inf1size + 0x8);
                uint addrSizeDAT1 = (uint)(firstStrAddr - 0x04);//Read the size of the DAT1 header (starts 8 bytes before first string data)
                uint sizeDAT1 = file.Read32(addrSizeDAT1);
                file.Write32(addrSizeDAT1, (uint)(file.Read32(addrSizeDAT1) + newStrSize));//Write the new data section size
                //Number of characters after current entry by looping through each entry and getting length of message
                uint charsAfter = 0;
                for (int i = lbxMsgList.SelectedIndex + 1; i < stringLengths.Length; i++)
                {
                    charsAfter += (uint)(stringLengths[i]);
                }
                byte[] theNextChars = new byte[charsAfter];//Hold every char after the current entry.
                for (int i = 0; i < theNextChars.Length; i++)
                {
                    theNextChars[i] = (byte)file.Read8((uint)(nextStrStart + i));//Add the next character to array
                }
                nextStrStart = file.Read32((uint)(0x30 + (lbxMsgList.SelectedIndex + 1) * 8)) + 0x20 + inf1size + 0x8 + (uint)newStrSize;//Next string starts (its old position + difference in previous string length)
                for (int i = 0; i < theNextChars.Length; i++)
                {
                    file.Write8((uint)(nextStrStart + i), theNextChars[i]);//Write the character back to file with new position
                }
            }
            //If it's the last entry (no headers need updated or data shifted


            file.Write32(fileSizeAddress, (uint)(sizeOfFile + newStrSize));//Update new file size

            if (lbxMsgList.SelectedIndex != numentries - 1)
            {
                //Update all string entries after the current one and update their offsets
                for (int i = lbxMsgList.SelectedIndex + 1; i < numentries; i++)
                {
                    uint nextHeaderAddr = (uint)(0x30 + i * 8);//Point to next string header
                    uint theStrOff = file.Read32(nextHeaderAddr);
                    file.Write32(nextHeaderAddr, (uint)(theStrOff + newStrSize));
                }
            }


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

            ReadStrings("data/message/msg_data_" + langNames[cmbLanguages.SelectedIndex] + ".bin");//Reload texts after saving
        }

        private void lbxMsgList_SelectedIndexChanged(object sender, EventArgs e)
        {
            tbxMsgPreview.Text = m_MsgData[lbxMsgList.SelectedIndex];
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (lbxMsgList.SelectedIndex >= 0 && lbxMsgList.SelectedIndex <= file.Read16(0x28) && m_MsgData[lbxMsgList.SelectedIndex] != "")
                WriteStrings();
            else if (lbxMsgList.SelectedIndex < 0 || lbxMsgList.SelectedIndex >= file.Read16(0x28))
                MessageBox.Show("Please select a string entry to edit.");
            else if (m_MsgData[lbxMsgList.SelectedIndex] == "")
                MessageBox.Show("It is not currently possible to write over a blank entry.");
            
            
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
                            "When done, press 'Save'.\n\nPlease note that you cannot currently enter \n" +
                            "text into a blank entry.\n\n" +
                            "Use the buttons under the text editing box to insert the special characters.\n" +
                            "When reading, their codes are as follows:\n\n" +
                            "[0xEE][0xEF] \t Coins\n[0xF0] \t\t Star Full\n[0xF1] \t\t Star Empty\n" +
                            "[0xF2][0xF3] \t D-Pad\n[0xF4][0xF5] \t A\n[0xF6][0xF7] \t B\n" +
                            "[0xF8][0xF9] \t X\n[0xFA][0xFB] \t Y");
        }

    }
}
