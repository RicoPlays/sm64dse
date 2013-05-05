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
    public partial class ROMFileSelect : Form
    {
        public String m_SelectedFile = "";

        public ROMFileSelect()
        {
            InitializeComponent();

            LoadFileList();
        }

        public ROMFileSelect(String title)
        {
            InitializeComponent();
            this.Text = title;

            LoadFileList();
        }

        private void LoadFileList()
        {
            NitroROM.FileEntry[] files = Program.m_ROM.GetFileEntries();
            tvFiles.Nodes.Add("root", "ROM File System");

            TreeNode node = tvFiles.Nodes["root"];

            LoadFiles(node, files, new NARC.FileEntry[] { });
        }

        private void LoadFiles(TreeNode node, NitroROM.FileEntry[] files, NARC.FileEntry[] filesNARC)
        {
            TreeNode parent = node;
            String[] names = new String[0];
            if (files.Length == 0)
            {
                names = new String[filesNARC.Length];
                for (int i = 0; i < filesNARC.Length; i++)
                    names[i] = filesNARC[i].FullName;
            }
            else if (filesNARC.Length == 0)
            {
                names = new String[files.Length];
                for (int i = 0; i < files.Length; i++)
                    names[i] = files[i].FullName;
            }

            for (int i = 0; i < names.Length; i++)
            {
                String[] parts = names[i].Split('/');

                if (parts.Length == 1)
                {
                    /*if (parts[0].Equals(""))
                        tvFiles.Nodes["root"].Nodes.Add("Overlay");
                    else */if (!parts[0].Equals(""))
                        tvFiles.Nodes["root"].Nodes.Add(parts[0]).Tag = names[i];
                }
                else
                {
                    node = parent;

                    for (int j = 0; j < parts.Length; j++)
                    {
                        if (!node.Nodes.ContainsKey(parts[j]))
                            node.Nodes.Add(parts[j], parts[j]).Tag = names[i];
                        node = node.Nodes[parts[j]];

                        if (parts[j].EndsWith(".narc"))
                        {
                            LoadFiles(node, new NitroROM.FileEntry[]{}, new NARC(Program.m_ROM, Program.m_ROM.GetFileIDFromName(files[i].FullName)).GetFileEntries());
                        }
                    }
                }
            }
        }

        private void tvFiles_AfterSelect(object sender, TreeViewEventArgs e)
        {
            m_SelectedFile = e.Node.Tag.ToString();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
