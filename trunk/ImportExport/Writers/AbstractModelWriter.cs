using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Globalization;

namespace SM64DSe.ImportExport.Writers
{
    public abstract class AbstractModelWriter
    {
        public ModelBase m_Model;

        protected string m_ModelFileName;
        protected string m_ModelPath;

        public AbstractModelWriter(ModelBase model, string modelFileName)
        {
            m_Model = model;

            m_ModelFileName = modelFileName;
            m_ModelPath = Path.GetDirectoryName(m_ModelFileName);
        }

        public abstract void WriteModel(bool save = true);

        protected static void ExportTextureToPNG(string destDir, string destName, string bitmapLocation)
        {
            try
            {
                Bitmap bmp = new Bitmap(bitmapLocation);
                ExportTextureToPNG(destDir, destName, bmp);
            }
            catch (IOException)
            {
                Console.Write("Cannot read image: " + bitmapLocation);
            }
        }

        protected static void ExportTextureToPNG(string destDir, string destName, Bitmap lol)
        {
            //Export the current texture to .PNG
            try
            {
                lol.Save(destDir + "/" + destName + ".png", System.Drawing.Imaging.ImageFormat.Png);
            }
            catch (Exception e)
            {
                MessageBox.Show("An error occurred while trying to save texture " + destName + ".\n\n " +
                    e.Message + "\n" + e.Data + "\n" + e.StackTrace + "\n" + e.Source);
            }
        }

        protected static CultureInfo usa = Helper.USA;
    }
}
