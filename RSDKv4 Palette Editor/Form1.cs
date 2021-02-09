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
        //Values for finding palette data from files
        private const int GAMECONFIG_COLOR_START = 103;
        private const int STAGETILES_COLOR_START = 397;
        private const int GC_COLOR_TABLE_LENGTH = 288;
        private const int COLOR_TABLE_LENGTH = 382;

        //Declare variables
        string dataPath = "";
        bool unsavedFlag = false;
        string lastSelected = "";

        public Form1()
        {
            InitializeComponent();
        }

        //Edit palette entry
        private void colorBox_Click(object sender, EventArgs e)
        {
            var colorBox = sender as PictureBox;

            colorDialog.Color = colorBox.BackColor;

            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                colorBox.BackColor = colorDialog.Color;
                unsavedFlag = true;
            }
        }

        //Save button
        private void button1_Click(object sender, EventArgs e)
        {
            SavePalette();
        }

        //Open button
        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Open GameConfig.bin...";
            ofd.Filter = "GameConfig.bin|GameConfig.bin";
            ofd.ShowDialog();
            if (ofd.FileName != "")
            {
                dataPath = ofd.FileName.Substring(0, ofd.FileName.Length - 19); //Get path of data folder
                ReadData();
            }
        }

        //Load palette from file
        private void LoadPalette(int offset, string path)
        {
            if (path != "")
            {
                FileStream fs = new FileStream(path, FileMode.Open);

                fs.Seek(offset, SeekOrigin.Begin);
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
                            }
                        }
                    }
                }

                fs.Dispose();

                //Enable the color boxes
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

        //Save palette to file
        private void SavePalette()
        {
            if (lastSelected.ToString() == "Global")
            {
                FileStream fs = new FileStream(dataPath + "Game\\GameConfig.bin", FileMode.Open);
                fs.Seek(GAMECONFIG_COLOR_START, SeekOrigin.Begin);

                for (int i = 0; i < GC_COLOR_TABLE_LENGTH / 3; i++)
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

                MessageBox.Show("Saved to " + dataPath + "Game\\GameConfig.bin", "Saved!", MessageBoxButtons.OK);
                fs.Dispose();
            } else
            {
                FileStream fs = new FileStream(dataPath + "Stages\\" + lastSelected + "\\16x16Tiles.gif", FileMode.Open);
                fs.Seek(STAGETILES_COLOR_START, SeekOrigin.Begin);

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

                MessageBox.Show("Saved to " + dataPath + "Stages\\" + listPalettes.SelectedItem.ToString() + "\\16x16Tiles.gif", "Saved!", MessageBoxButtons.OK);
                fs.Dispose();
            }

            unsavedFlag = false;

        }

        //Generates list of editable files
        private void ReadData()
        {
            listPalettes.Items.Clear();

            listPalettes.Items.Add("Global");

            if (File.Exists(dataPath + "Stages\\Continue\\16x16Tiles.gif")) listPalettes.Items.Add("Continue");
            if (File.Exists(dataPath + "Stages\\Credits\\16x16Tiles.gif")) listPalettes.Items.Add("Credits");
            if (File.Exists(dataPath + "Stages\\Ending\\16x16Tiles.gif")) listPalettes.Items.Add("Ending");
            if (File.Exists(dataPath + "Stages\\LSelect\\16x16Tiles.gif")) listPalettes.Items.Add("LSelect");
            if (File.Exists(dataPath + "Stages\\Special\\16x16Tiles.gif")) listPalettes.Items.Add("Special");
            if (File.Exists(dataPath + "Stages\\Title\\16x16Tiles.gif")) listPalettes.Items.Add("Title");

            for (int i = 1; i <= 12; i++)
            {
                if (File.Exists(dataPath + "Stages\\Zone"+i.ToString("D2")+"\\16x16Tiles.gif")) listPalettes.Items.Add("Zone" + i.ToString("D2"));
            }

            listPalettes.SelectedItem = "Global";

            btnSave.Enabled = true;

            Text = dataPath + " - RSDKv4 Palette Editor";
        }

        //Behavior for when a new palette is selected
        private void listPalettes_SelectedIndexChanged(object sender, EventArgs e)
        {

            if (listPalettes.SelectedItem.ToString() != lastSelected) //Do nothing if the same palette is selected
            {
                //Save prompt
                if (unsavedFlag)
                {
                    DialogResult dialogResult = MessageBox.Show("There are unsaved changes. Do you want to save this palette?", "Unsaved Changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                    if (dialogResult == DialogResult.Yes) SavePalette();
                    else if (dialogResult == DialogResult.Cancel) { listPalettes.SelectedItem = lastSelected; return; }
                }

                unsavedFlag = false;

                lastSelected = listPalettes.SelectedItem.ToString();

                switch (listPalettes.SelectedItem)
                {
                    case "Global":
                        LoadPalette(GAMECONFIG_COLOR_START, dataPath + "Game\\GameConfig.bin");

                        //Disable last two rows because they don't contain palette data in GameConfig
                        for (int i = 97; i <= 128; i++)
                        {
                            foreach (Control c in Controls)
                            {
                                PictureBox b = c as PictureBox;
                                if (b != null)
                                {
                                    if (b.Name == "pictureBox" + (i).ToString())
                                    {
                                        b.BackColor = Color.Empty;
                                        b.Enabled = false;
                                    }
                                }
                            }
                        }
                        break;
                    default:
                        LoadPalette(STAGETILES_COLOR_START, dataPath + "Stages\\" + listPalettes.SelectedItem + "\\16x16Tiles.gif");
                        break;
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //Prompt to save on close
            if (unsavedFlag)
            {
                DialogResult dialogResult = MessageBox.Show("There are unsaved changes. Do you want to save this palette?", "Unsaved Changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                if (dialogResult == DialogResult.Yes) SavePalette();
                else if (dialogResult == DialogResult.Cancel) { listPalettes.SelectedItem = lastSelected; e.Cancel = true; ; }
            }
        }
    }
}
