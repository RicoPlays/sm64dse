using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SM64DSe
{
    static class Program
    {
        public static string AppTitle = "SM64DS Editor";
        // haxx: debugging shit is enabled if this contains 'private beta'
        public static string AppVersion = "v2.0 PRIVATE BETA";

        public static string ServerURL = "http://kuribo64.cjb.net/";

        public static string m_ROMPath;
        public static NitroROM m_ROM;

        public static List<LevelEditorForm> m_LevelEditors;

        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm(args));
        }
    }
}
