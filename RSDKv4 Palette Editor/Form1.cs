using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace RSDKv4_Palette_Editor
{

    public partial class Form1 : Form
    {
        private const int COLOR_TABLE_START = 103;
        private const int COLOR_TABLE_LENGTH = 288;

        private Control[] colorBoxes = new Control[COLOR_TABLE_LENGTH];
        string path = "";

        public Form1()
        {
            InitializeComponent();
        }

        private void colorBox_Click(object sender, EventArgs e)
        {
            var colorBox = sender as PictureBox;

            colorDialog.Color = colorBox.BackColor;

            if (colorDialog.ShowDialog() == DialogResult.OK)
                colorBox.BackColor = colorDialog.Color;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            foreach (Control c in Controls)
            {
                PictureBox b = c as PictureBox;
                if (b != null)
                {
                    b.Enabled = false;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FileStream fs = new FileStream(path, FileMode.Open);
            fs.Seek(COLOR_TABLE_START, SeekOrigin.Begin);

            for (int i = 0; i < COLOR_TABLE_LENGTH / 3; i++)
            {
                foreach (Control c in Controls)
                {
                    PictureBox b = c as PictureBox;
                    if (b != null)
                    {
                        if (b.Name == "pictureBox" + (i + 1).ToString())
                        {
                            fs.WriteByte(b.BackColor.R);
                            fs.WriteByte(b.BackColor.G);
                            fs.WriteByte(b.BackColor.B);
                        }
                    }
                }
            }

            MessageBox.Show("Saved to " + path, "Saved!", MessageBoxButtons.OK);
            fs.Dispose();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "GameConfig.bin|GameConfig.bin";
            ofd.ShowDialog();
            path = ofd.FileName;

            Console.WriteLine(path);

            if (path != "")
            {
                FileStream fs = new FileStream(path, FileMode.Open);

                fs.Seek(COLOR_TABLE_START, SeekOrigin.Begin);
                for (int i = 0; i < COLOR_TABLE_LENGTH / 3; i++)
                {

                    foreach (Control c in Controls)
                    {
                        PictureBox b = c as PictureBox;
                        if (b != null)
                        {
                            if (b.Name == "pictureBox" + (i + 1).ToString())
                            {
                                b.BackColor = Color.FromArgb(Convert.ToInt32(fs.ReadByte()), Convert.ToInt32(fs.ReadByte()), Convert.ToInt32(fs.ReadByte()));
                                
                                //Console.WriteLine(string.Format("{0:X2}", redHexIn) + ", " + string.Format("{0:X2}", greenHexIn) + ", " + string.Format("{0:X2}", blueHexIn) + "\n");
                            }
                        }
                    }

                }
                fs.Dispose();

                foreach (Control c in Controls)
                {
                    PictureBox b = c as PictureBox;
                    if (b != null)
                    {
                        b.Enabled = true;
                    }
                }
            }



            
        }
    }
}
