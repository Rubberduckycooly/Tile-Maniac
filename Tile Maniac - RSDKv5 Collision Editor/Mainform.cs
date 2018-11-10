using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Linq.Expressions;
using System.Diagnostics;

namespace TileManiac
{
    public partial class Mainform : Form
    {
        bool lockRadioButtons = false; //for locking radio button updates when switching single select options

        RSDKv5.TilesConfig.ColllisionMask TileClipboard;

        List<Bitmap> ColImges = new List<Bitmap>(); //List of images, saves memory
        List<Bitmap> ColActivatedImges = new List<Bitmap>(); //List of images, saves memory

        List<Bitmap> CollisionListImgA = new List<Bitmap>();
        List<Bitmap> CollisionListImgB = new List<Bitmap>();

        public int curColisionMask; //What Collision Mask are we editing?

        public string filepath; //Where is the file located?

        bool showPathB = false; //should we show Path A or Path B?

        bool MirrorPaths = false; //Do we want to activate "Mirror Paths" Mode?

        bool changingModes = false; //To prevent updating the radio buttons until after we change the viewer mode

        public RSDKv5.TilesConfig tcf; //The ColllisionMask Data

        List<Bitmap> Tiles = new List<Bitmap>(); //List of all the 16x16 Stage Tiles
        int gotoVal; //What collision mask we goto when "GO!" is pressed

        public Mainform()
        {
            InitializeComponent();

            ToolTip ToolTip = new ToolTip();

            ColImges.Add(new Bitmap(Properties.Resources._1));
            ColImges.Add(new Bitmap(Properties.Resources._2));
            ColImges.Add(new Bitmap(Properties.Resources._3));
            ColImges.Add(new Bitmap(Properties.Resources._4));
            ColImges.Add(new Bitmap(Properties.Resources._5));
            ColImges.Add(new Bitmap(Properties.Resources._6));
            ColImges.Add(new Bitmap(Properties.Resources._7));
            ColImges.Add(new Bitmap(Properties.Resources._8));
            ColImges.Add(new Bitmap(Properties.Resources._9));
            ColImges.Add(new Bitmap(Properties.Resources._10));
            ColImges.Add(new Bitmap(Properties.Resources._11));
            ColImges.Add(new Bitmap(Properties.Resources._12));
            ColImges.Add(new Bitmap(Properties.Resources._13));
            ColImges.Add(new Bitmap(Properties.Resources._14));
            ColImges.Add(new Bitmap(Properties.Resources._15));
            ColImges.Add(new Bitmap(Properties.Resources._16));
            ColImges.Add(new Bitmap(Properties.Resources._0));


            ColActivatedImges.Add(Properties.Resources.Red);
            ColActivatedImges.Add(Properties.Resources.Green);

            Viewer1.Image = ColImges[16];
            Viewer2.Image = ColImges[16];
            Viewer3.Image = ColImges[16];
            Viewer4.Image = ColImges[16];
            Viewer5.Image = ColImges[16];
            Viewer6.Image = ColImges[16];
            Viewer7.Image = ColImges[16];
            Viewer8.Image = ColImges[16];
            Viewer9.Image = ColImges[16];
            Viewer10.Image = ColImges[16];
            Viewer11.Image = ColImges[16];
            Viewer12.Image = ColImges[16];
            Viewer13.Image = ColImges[16];
            Viewer14.Image = ColImges[16];
            Viewer15.Image = ColImges[16];
            Viewer16.Image = ColImges[16];


            RGBox0.Image = ColActivatedImges[0];
            RGBox1.Image = ColActivatedImges[0];
            RGBox2.Image = ColActivatedImges[0];
            RGBox3.Image = ColActivatedImges[0];
            RGBox4.Image = ColActivatedImges[0];
            RGBox5.Image = ColActivatedImges[0];
            RGBox6.Image = ColActivatedImges[0];
            RGBox7.Image = ColActivatedImges[0];
            RGBox8.Image = ColActivatedImges[0];
            RGBox9.Image = ColActivatedImges[0];
            RGBoxA.Image = ColActivatedImges[0];
            RGBoxB.Image = ColActivatedImges[0];
            RGBoxC.Image = ColActivatedImges[0];
            RGBoxD.Image = ColActivatedImges[0];
            RGBoxE.Image = ColActivatedImges[0];
            RGBoxF.Image = ColActivatedImges[0];

            ToolTip.SetToolTip(SlopeNUD, "Controls the slope angle of the tile in degrees");
            ToolTip.SetToolTip(PhysicsNUD, "Controls the physics of the player interacting with the tile");
            ToolTip.SetToolTip(MomentumNUD, "Controls the momentum the player gets from the tile");
            ToolTip.SetToolTip(UnknownNUD, "Controls the Unknown Value of the tile");
            ToolTip.SetToolTip(SpecialNUD, "Controls the the tile's 'Special' Properties, like whether it's a conveyor belt or not");

            LoadSettings();
        }

        void LoadSettings()
        {
            if (Properties.Settings.Default.ListSetting == 0)
            {
                uncheckListViews();
                collisionViewRadioButton.Checked = true;
                lockRadioButtons = false;
            }
            else if (Properties.Settings.Default.ListSetting == 1)
            {
                uncheckListViews();
                tileViewRadioButton.Checked = true;
                lockRadioButtons = false;
            }

            if (Properties.Settings.Default.ViewerSetting == 0)
            {
                unCheckModes();
                radioButton1.Checked = true;
                TilePicBox.Visible = true;
                changingModes = false;
            }
            else if (Properties.Settings.Default.ViewerSetting == 1)
            {
                unCheckModes();
                radioButton2.Checked = true;
                CollisionPicBox.Visible = true;
                changingModes = false;
            }
            else if (Properties.Settings.Default.ViewerSetting == 2)
            {
                unCheckModes();
                radioButton3.Checked = true;
                overlayPicBox.Visible = true;
                changingModes = false;
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Title = "Open RSDKv5 Tileconfig";
            dlg.DefaultExt = ".bin";
            dlg.Filter = "RSDKv5 Tileconfig Files|Tileconfig*.bin|All Files|*";

            if (dlg.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
            {
                curColisionMask = 0; // Set the current collision mask to zero (avoids rare errors)
                filepath = dlg.FileName;
                tcf = new RSDKv5.TilesConfig(dlg.FileName);
                string tileBitmapPath = Path.Combine(Path.GetDirectoryName(filepath), "16x16tiles.gif"); // get the path to the stage's tileset
                LoadTileSet(new Bitmap(tileBitmapPath)); // load each 16x16 tile into the list

                CollisionList.Images.Clear();

                for (int i = 0; i < 1024; i++)
                {
                    if (Properties.Settings.Default.ListSetting == 0)
                    {
                        CollisionListImgA.Add(tcf.CollisionPath1[i].DrawCMask(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 255, 0)));
                        CollisionList.Images.Add(CollisionListImgA[i]);
                    }
                    else
                    {
                        CollisionListImgA.Add(Tiles[i]);
                        CollisionList.Images.Add(Tiles[i]);
                    }

                }

                for (int i = 0; i < 1024; i++)
                {
                    if (Properties.Settings.Default.ListSetting == 0)
                    {
                        CollisionListImgB.Add(tcf.CollisionPath2[i].DrawCMask(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 255, 0)));
                        CollisionList.Images.Add(CollisionListImgB[i]);
                    }
                    else
                    {
                        CollisionListImgB.Add(Tiles[i]);
                        CollisionList.Images.Add(Tiles[i]);
                    }
                }
                CollisionList.SelectedIndex = curColisionMask - 1;
                CollisionList.Refresh();

                RefreshUI(); //update the UI
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (filepath != null) //Did we open a file?
            {
                tcf.Write(filepath);
            }
            else //if not then use "Save As..."
            {
                saveAsToolStripMenuItem_Click(this, e);
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Title = "Save RSDKv5 Tileconfig As...";
            dlg.DefaultExt = ".bin";
            dlg.Filter = "RSDKv5 Tileconfig Files|Tileconfig*.bin";

            if (dlg.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
            {
                tcf.Write(dlg.FileName); //Write the data to a file
            }
        }

        private void openSingleCollisionMaskToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void exportCollisionMaskAsToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void splitFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            dlg.Description = "Select Folder to Export to...";

            if (dlg.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
            {
                for (int i = 0; i < 1024; i++)
                {
                    BinaryWriter Writer1 = new BinaryWriter(File.Create(dlg.SelectedPath + "//CollisionMaskPathA" + (i + 1) + ".rcm"));
                    BinaryWriter Writer2 = new BinaryWriter(File.Create(dlg.SelectedPath + "//CollisionMaskPathB" + (i + 1) + ".rcm"));
                    tcf.CollisionPath1[i].WriteUnc(Writer1);
                    tcf.CollisionPath2[i].WriteUnc(Writer2);
                    Writer1.Close();
                    Writer2.Close();
                }
                RefreshUI();
            }

        }

        public void LoadTileSet(Bitmap TileSet)
        {
            Tiles.Clear(); // Clear the previous images, since we load the entire file!
            int tsize = TileSet.Height; //Height of the image in pixels
            for (int i = 0; i < (tsize / 16); i++) //We divide by 16 to get the "height" in blocks
            {
                Rectangle CropArea = new Rectangle(0, (i * 16), 16, 16); //we then get tile at Y: i * 16, 
                                                                         //we have to multiply i by 16 to get the "true Tile value" (1* 16 = 16, 2 * 16 = 32, etc.)

                Bitmap CroppedImage = CropImage(TileSet, CropArea); // crop that image
                Tiles.Add(CroppedImage); // add it to the tile list
            }
        }

        public Bitmap CropImage(Bitmap source, Rectangle section)
        {
            // An empty bitmap which will hold the cropped image
            Bitmap bmp = new Bitmap(section.Width, section.Height);

            Graphics g = Graphics.FromImage(bmp);

            // Draw the given area (section) of the source image
            // at location 0,0 on the empty bitmap (bmp)
            g.DrawImage(source, 0, 0, section, GraphicsUnit.Pixel);

            return bmp;
        }

        void RefreshUI()
        {
            if (tcf != null)
            {
                CurMaskLabel.Text = "Collision Mask " + (curColisionMask + 1) + " of " + 1024; //what collision mask are we on?
                TilePicBox.Image = Tiles[curColisionMask]; //update the tile preview 
                Bitmap Overlaypic = new Bitmap(16, 16);
                if (!showPathB) //if we are showing Path A then refresh the values accordingly
                {
                    CollisionPicBox.Image = tcf.CollisionPath1[curColisionMask].DrawCMask(Color.FromArgb(0, 0, 0, 0), Color.FromArgb(255, 0, 255, 0));
                    Overlaypic = tcf.CollisionPath1[curColisionMask].DrawCMask(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 255, 0), Tiles[curColisionMask]);
                    degreeLabel.Text = "Degree of Slope (Experimental): " + ((int)((256 - tcf.CollisionPath1[curColisionMask].slopeAngle) * (360f / 0x100))).ToString();
                    SlopeNUD.Value = tcf.CollisionPath1[curColisionMask].slopeAngle;
                    //RawSlopeNUD.Value = tcf.CollisionPath1[curColisionMask].slopeAngle;
                    PhysicsNUD.Value = tcf.CollisionPath1[curColisionMask].physics;
                    MomentumNUD.Value = tcf.CollisionPath1[curColisionMask].momentum;
                    UnknownNUD.Value = tcf.CollisionPath1[curColisionMask].unknown;
                    SpecialNUD.Value = tcf.CollisionPath1[curColisionMask].special;
                    IsCeilingButton.Checked = tcf.CollisionPath1[curColisionMask].IsCeiling;

                    lb00.SelectedIndex = tcf.CollisionPath1[curColisionMask].Collision[0];
                    lb01.SelectedIndex = tcf.CollisionPath1[curColisionMask].Collision[1];
                    lb02.SelectedIndex = tcf.CollisionPath1[curColisionMask].Collision[2];
                    lb03.SelectedIndex = tcf.CollisionPath1[curColisionMask].Collision[3];
                    lb04.SelectedIndex = tcf.CollisionPath1[curColisionMask].Collision[4];
                    lb05.SelectedIndex = tcf.CollisionPath1[curColisionMask].Collision[5];
                    lb06.SelectedIndex = tcf.CollisionPath1[curColisionMask].Collision[6];
                    lb07.SelectedIndex = tcf.CollisionPath1[curColisionMask].Collision[7];
                    lb08.SelectedIndex = tcf.CollisionPath1[curColisionMask].Collision[8];
                    lb09.SelectedIndex = tcf.CollisionPath1[curColisionMask].Collision[9];
                    lb10.SelectedIndex = tcf.CollisionPath1[curColisionMask].Collision[10];
                    lb11.SelectedIndex = tcf.CollisionPath1[curColisionMask].Collision[11];
                    lb12.SelectedIndex = tcf.CollisionPath1[curColisionMask].Collision[12];
                    lb13.SelectedIndex = tcf.CollisionPath1[curColisionMask].Collision[13];
                    lb14.SelectedIndex = tcf.CollisionPath1[curColisionMask].Collision[14];
                    lb15.SelectedIndex = tcf.CollisionPath1[curColisionMask].Collision[15];

                    cb00.Checked = tcf.CollisionPath1[curColisionMask].HasCollision[0];
                    cb01.Checked = tcf.CollisionPath1[curColisionMask].HasCollision[1];
                    cb02.Checked = tcf.CollisionPath1[curColisionMask].HasCollision[2];
                    cb03.Checked = tcf.CollisionPath1[curColisionMask].HasCollision[3];
                    cb04.Checked = tcf.CollisionPath1[curColisionMask].HasCollision[4];
                    cb05.Checked = tcf.CollisionPath1[curColisionMask].HasCollision[5];
                    cb06.Checked = tcf.CollisionPath1[curColisionMask].HasCollision[6];
                    cb07.Checked = tcf.CollisionPath1[curColisionMask].HasCollision[7];
                    cb08.Checked = tcf.CollisionPath1[curColisionMask].HasCollision[8];
                    cb09.Checked = tcf.CollisionPath1[curColisionMask].HasCollision[9];
                    cb10.Checked = tcf.CollisionPath1[curColisionMask].HasCollision[10];
                    cb11.Checked = tcf.CollisionPath1[curColisionMask].HasCollision[11];
                    cb12.Checked = tcf.CollisionPath1[curColisionMask].HasCollision[12];
                    cb13.Checked = tcf.CollisionPath1[curColisionMask].HasCollision[13];
                    cb14.Checked = tcf.CollisionPath1[curColisionMask].HasCollision[14];
                    cb15.Checked = tcf.CollisionPath1[curColisionMask].HasCollision[15];

                    if (tcf.CollisionPath1[curColisionMask].HasCollision[0])
                    { Viewer1.Image = ColImges[lb00.SelectedIndex]; }
                    else { Viewer1.Image = ColImges[16]; }

                    if (tcf.CollisionPath1[curColisionMask].HasCollision[1])
                    { Viewer2.Image = ColImges[lb01.SelectedIndex]; }
                    else { Viewer2.Image = ColImges[16]; }

                    if (tcf.CollisionPath1[curColisionMask].HasCollision[2])
                    { Viewer3.Image = ColImges[lb02.SelectedIndex]; }
                    else { Viewer3.Image = ColImges[16]; }

                    if (tcf.CollisionPath1[curColisionMask].HasCollision[3])
                    { Viewer4.Image = ColImges[lb03.SelectedIndex]; }
                    else { Viewer4.Image = ColImges[16]; }

                    if (tcf.CollisionPath1[curColisionMask].HasCollision[4])
                    { Viewer5.Image = ColImges[lb04.SelectedIndex]; }
                    else { Viewer5.Image = ColImges[16]; }

                    if (tcf.CollisionPath1[curColisionMask].HasCollision[5])
                    { Viewer6.Image = ColImges[lb05.SelectedIndex]; }
                    else { Viewer6.Image = ColImges[16]; }

                    if (tcf.CollisionPath1[curColisionMask].HasCollision[6])
                    { Viewer7.Image = ColImges[lb06.SelectedIndex]; }
                    else { Viewer7.Image = ColImges[16]; }

                    if (tcf.CollisionPath1[curColisionMask].HasCollision[7])
                    { Viewer8.Image = ColImges[lb07.SelectedIndex]; }
                    else { Viewer8.Image = ColImges[16]; }

                    if (tcf.CollisionPath1[curColisionMask].HasCollision[8])
                    { Viewer9.Image = ColImges[lb08.SelectedIndex]; }
                    else { Viewer9.Image = ColImges[16]; }

                    if (tcf.CollisionPath1[curColisionMask].HasCollision[9])
                    { Viewer10.Image = ColImges[lb09.SelectedIndex]; }
                    else { Viewer10.Image = ColImges[16]; }

                    if (tcf.CollisionPath1[curColisionMask].HasCollision[10])
                    { Viewer11.Image = ColImges[lb10.SelectedIndex]; }
                    else { Viewer11.Image = ColImges[16]; }

                    if (tcf.CollisionPath1[curColisionMask].HasCollision[11])
                    { Viewer12.Image = ColImges[lb11.SelectedIndex]; }
                    else { Viewer12.Image = ColImges[16]; }

                    if (tcf.CollisionPath1[curColisionMask].HasCollision[12])
                    { Viewer13.Image = ColImges[lb12.SelectedIndex]; }
                    else { Viewer13.Image = ColImges[16]; }

                    if (tcf.CollisionPath1[curColisionMask].HasCollision[13])
                    { Viewer14.Image = ColImges[lb13.SelectedIndex]; }
                    else { Viewer14.Image = ColImges[16]; }

                    if (tcf.CollisionPath1[curColisionMask].HasCollision[14])
                    { Viewer15.Image = ColImges[lb14.SelectedIndex]; }
                    else { Viewer15.Image = ColImges[16]; }

                    if (tcf.CollisionPath1[curColisionMask].HasCollision[15])
                    { Viewer16.Image = ColImges[lb15.SelectedIndex]; }
                    else { Viewer16.Image = ColImges[16]; }




                    if (cb00.Checked) { RGBox0.Image = ColActivatedImges[1]; }
                    else { RGBox0.Image = ColActivatedImges[0]; }

                    if (cb01.Checked) { RGBox1.Image = ColActivatedImges[1]; }
                    else { RGBox1.Image = ColActivatedImges[0]; }

                    if (cb02.Checked) { RGBox2.Image = ColActivatedImges[1]; }
                    else { RGBox2.Image = ColActivatedImges[0]; }

                    if (cb03.Checked) { RGBox3.Image = ColActivatedImges[1]; }
                    else { RGBox3.Image = ColActivatedImges[0]; }

                    if (cb04.Checked) { RGBox4.Image = ColActivatedImges[1]; }
                    else { RGBox4.Image = ColActivatedImges[0]; }

                    if (cb05.Checked) { RGBox5.Image = ColActivatedImges[1]; }
                    else { RGBox5.Image = ColActivatedImges[0]; }

                    if (cb06.Checked) { RGBox6.Image = ColActivatedImges[1]; }
                    else { RGBox6.Image = ColActivatedImges[0]; }

                    if (cb07.Checked) { RGBox7.Image = ColActivatedImges[1]; }
                    else { RGBox7.Image = ColActivatedImges[0]; }

                    if (cb08.Checked) { RGBox8.Image = ColActivatedImges[1]; }
                    else { RGBox8.Image = ColActivatedImges[0]; }

                    if (cb09.Checked) { RGBox9.Image = ColActivatedImges[1]; }
                    else { RGBox9.Image = ColActivatedImges[0]; }

                    if (cb10.Checked) { RGBoxA.Image = ColActivatedImges[1]; }
                    else { RGBoxA.Image = ColActivatedImges[0]; }

                    if (cb11.Checked) { RGBoxB.Image = ColActivatedImges[1]; }
                    else { RGBoxB.Image = ColActivatedImges[0]; }

                    if (cb12.Checked) { RGBoxC.Image = ColActivatedImges[1]; }
                    else { RGBoxC.Image = ColActivatedImges[0]; }

                    if (cb13.Checked) { RGBoxD.Image = ColActivatedImges[1]; }
                    else { RGBoxD.Image = ColActivatedImges[0]; }

                    if (cb14.Checked) { RGBoxE.Image = ColActivatedImges[1]; }
                    else { RGBoxE.Image = ColActivatedImges[0]; }

                    if (cb15.Checked) { RGBoxF.Image = ColActivatedImges[1]; }
                    else { RGBoxF.Image = ColActivatedImges[0]; }

                }

                if (showPathB) //if we are showing Path B then refresh the values accordingly
                {
                    CollisionPicBox.Image = tcf.CollisionPath2[curColisionMask].DrawCMask(Color.FromArgb(0, 0, 0, 0), Color.FromArgb(0, 255, 0)); Overlaypic = tcf.CollisionPath2[curColisionMask].DrawCMask(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 255, 0), Tiles[curColisionMask]);
                    SlopeNUD.Value = tcf.CollisionPath2[curColisionMask].slopeAngle;
                    //RawSlopeNUD.Value = tcf.CollisionPath2[curColisionMask].slopeAngle;
                    degreeLabel.Text = "Degree of Slope (Experimental): " + ((int)((256 - tcf.CollisionPath2[curColisionMask].slopeAngle) * (360f / 0xFF))).ToString();
                    PhysicsNUD.Value = tcf.CollisionPath2[curColisionMask].physics;
                    MomentumNUD.Value = tcf.CollisionPath2[curColisionMask].momentum;
                    UnknownNUD.Value = tcf.CollisionPath2[curColisionMask].unknown;
                    SpecialNUD.Value = tcf.CollisionPath2[curColisionMask].special;
                    IsCeilingButton.Checked = tcf.CollisionPath2[curColisionMask].IsCeiling;

                    lb00.SelectedIndex = tcf.CollisionPath2[curColisionMask].Collision[0];
                    lb01.SelectedIndex = tcf.CollisionPath2[curColisionMask].Collision[1];
                    lb02.SelectedIndex = tcf.CollisionPath2[curColisionMask].Collision[2];
                    lb03.SelectedIndex = tcf.CollisionPath2[curColisionMask].Collision[3];
                    lb04.SelectedIndex = tcf.CollisionPath2[curColisionMask].Collision[4];
                    lb05.SelectedIndex = tcf.CollisionPath2[curColisionMask].Collision[5];
                    lb06.SelectedIndex = tcf.CollisionPath2[curColisionMask].Collision[6];
                    lb07.SelectedIndex = tcf.CollisionPath2[curColisionMask].Collision[7];
                    lb08.SelectedIndex = tcf.CollisionPath2[curColisionMask].Collision[8];
                    lb09.SelectedIndex = tcf.CollisionPath2[curColisionMask].Collision[9];
                    lb10.SelectedIndex = tcf.CollisionPath2[curColisionMask].Collision[10];
                    lb11.SelectedIndex = tcf.CollisionPath2[curColisionMask].Collision[11];
                    lb12.SelectedIndex = tcf.CollisionPath2[curColisionMask].Collision[12];
                    lb13.SelectedIndex = tcf.CollisionPath2[curColisionMask].Collision[13];
                    lb14.SelectedIndex = tcf.CollisionPath2[curColisionMask].Collision[14];
                    lb15.SelectedIndex = tcf.CollisionPath2[curColisionMask].Collision[15];

                    cb00.Checked = tcf.CollisionPath2[curColisionMask].HasCollision[0];
                    cb01.Checked = tcf.CollisionPath2[curColisionMask].HasCollision[1];
                    cb02.Checked = tcf.CollisionPath2[curColisionMask].HasCollision[2];
                    cb03.Checked = tcf.CollisionPath2[curColisionMask].HasCollision[3];
                    cb04.Checked = tcf.CollisionPath2[curColisionMask].HasCollision[4];
                    cb05.Checked = tcf.CollisionPath2[curColisionMask].HasCollision[5];
                    cb06.Checked = tcf.CollisionPath2[curColisionMask].HasCollision[6];
                    cb07.Checked = tcf.CollisionPath2[curColisionMask].HasCollision[7];
                    cb08.Checked = tcf.CollisionPath2[curColisionMask].HasCollision[8];
                    cb09.Checked = tcf.CollisionPath2[curColisionMask].HasCollision[9];
                    cb10.Checked = tcf.CollisionPath2[curColisionMask].HasCollision[10];
                    cb11.Checked = tcf.CollisionPath2[curColisionMask].HasCollision[11];
                    cb12.Checked = tcf.CollisionPath2[curColisionMask].HasCollision[12];
                    cb13.Checked = tcf.CollisionPath2[curColisionMask].HasCollision[13];
                    cb14.Checked = tcf.CollisionPath2[curColisionMask].HasCollision[14];
                    cb15.Checked = tcf.CollisionPath2[curColisionMask].HasCollision[15];


                    if (tcf.CollisionPath2[curColisionMask].HasCollision[0])
                    { Viewer1.Image = ColImges[lb00.SelectedIndex]; }
                    else { Viewer1.Image = ColImges[16]; }

                    if (tcf.CollisionPath2[curColisionMask].HasCollision[1])
                    { Viewer2.Image = ColImges[lb01.SelectedIndex]; }
                    else { Viewer2.Image = ColImges[16]; }

                    if (tcf.CollisionPath2[curColisionMask].HasCollision[2])
                    { Viewer3.Image = ColImges[lb02.SelectedIndex]; }
                    else { Viewer3.Image = ColImges[16]; }

                    if (tcf.CollisionPath2[curColisionMask].HasCollision[3])
                    { Viewer4.Image = ColImges[lb03.SelectedIndex]; }
                    else { Viewer4.Image = ColImges[16]; }

                    if (tcf.CollisionPath2[curColisionMask].HasCollision[4])
                    { Viewer5.Image = ColImges[lb04.SelectedIndex]; }
                    else { Viewer5.Image = ColImges[16]; }

                    if (tcf.CollisionPath2[curColisionMask].HasCollision[5])
                    { Viewer6.Image = ColImges[lb05.SelectedIndex]; }
                    else { Viewer6.Image = ColImges[16]; }

                    if (tcf.CollisionPath2[curColisionMask].HasCollision[6])
                    { Viewer7.Image = ColImges[lb06.SelectedIndex]; }
                    else { Viewer7.Image = ColImges[16]; }

                    if (tcf.CollisionPath2[curColisionMask].HasCollision[7])
                    { Viewer8.Image = ColImges[lb07.SelectedIndex]; }
                    else { Viewer8.Image = ColImges[16]; }

                    if (tcf.CollisionPath2[curColisionMask].HasCollision[8])
                    { Viewer9.Image = ColImges[lb08.SelectedIndex]; }
                    else { Viewer9.Image = ColImges[16]; }

                    if (tcf.CollisionPath2[curColisionMask].HasCollision[9])
                    { Viewer10.Image = ColImges[lb09.SelectedIndex]; }
                    else { Viewer10.Image = ColImges[16]; }

                    if (tcf.CollisionPath2[curColisionMask].HasCollision[10])
                    { Viewer11.Image = ColImges[lb10.SelectedIndex]; }
                    else { Viewer11.Image = ColImges[16]; }

                    if (tcf.CollisionPath2[curColisionMask].HasCollision[11])
                    { Viewer12.Image = ColImges[lb11.SelectedIndex]; }
                    else { Viewer12.Image = ColImges[16]; }

                    if (tcf.CollisionPath2[curColisionMask].HasCollision[12])
                    { Viewer13.Image = ColImges[lb12.SelectedIndex]; }
                    else { Viewer13.Image = ColImges[16]; }

                    if (tcf.CollisionPath2[curColisionMask].HasCollision[13])
                    { Viewer14.Image = ColImges[lb13.SelectedIndex]; }
                    else { Viewer14.Image = ColImges[16]; }

                    if (tcf.CollisionPath2[curColisionMask].HasCollision[14])
                    { Viewer15.Image = ColImges[lb14.SelectedIndex]; }
                    else { Viewer15.Image = ColImges[16]; }

                    if (tcf.CollisionPath2[curColisionMask].HasCollision[15])
                    { Viewer16.Image = ColImges[lb15.SelectedIndex]; }
                    else { Viewer16.Image = ColImges[16]; }




                    if (cb00.Checked) { RGBox0.Image = ColActivatedImges[1]; }
                    else { RGBox0.Image = ColActivatedImges[0]; }

                    if (cb01.Checked) { RGBox1.Image = ColActivatedImges[1]; }
                    else { RGBox1.Image = ColActivatedImges[0]; }

                    if (cb02.Checked) { RGBox2.Image = ColActivatedImges[1]; }
                    else { RGBox2.Image = ColActivatedImges[0]; }

                    if (cb03.Checked) { RGBox3.Image = ColActivatedImges[1]; }
                    else { RGBox3.Image = ColActivatedImges[0]; }

                    if (cb04.Checked) { RGBox4.Image = ColActivatedImges[1]; }
                    else { RGBox4.Image = ColActivatedImges[0]; }

                    if (cb05.Checked) { RGBox5.Image = ColActivatedImges[1]; }
                    else { RGBox5.Image = ColActivatedImges[0]; }

                    if (cb06.Checked) { RGBox6.Image = ColActivatedImges[1]; }
                    else { RGBox6.Image = ColActivatedImges[0]; }

                    if (cb07.Checked) { RGBox7.Image = ColActivatedImges[1]; }
                    else { RGBox7.Image = ColActivatedImges[0]; }

                    if (cb08.Checked) { RGBox8.Image = ColActivatedImges[1]; }
                    else { RGBox8.Image = ColActivatedImges[0]; }

                    if (cb09.Checked) { RGBox9.Image = ColActivatedImges[1]; }
                    else { RGBox9.Image = ColActivatedImges[0]; }

                    if (cb10.Checked) { RGBoxA.Image = ColActivatedImges[1]; }
                    else { RGBoxA.Image = ColActivatedImges[0]; }

                    if (cb11.Checked) { RGBoxB.Image = ColActivatedImges[1]; }
                    else { RGBoxB.Image = ColActivatedImges[0]; }

                    if (cb12.Checked) { RGBoxC.Image = ColActivatedImges[1]; }
                    else { RGBoxC.Image = ColActivatedImges[0]; }

                    if (cb13.Checked) { RGBoxD.Image = ColActivatedImges[1]; }
                    else { RGBoxD.Image = ColActivatedImges[0]; }

                    if (cb14.Checked) { RGBoxE.Image = ColActivatedImges[1]; }
                    else { RGBoxE.Image = ColActivatedImges[0]; }

                    if (cb15.Checked) { RGBoxF.Image = ColActivatedImges[1]; }
                    else { RGBoxF.Image = ColActivatedImges[0]; }
                }

                overlayPicBox.Image = Overlaypic;
                RefreshCollisionList();
            }
        }

        public void RefreshCollisionList()
        {
            CollisionList.Images.Clear();

            if (!showPathB)
            {
                for (int i = 0; i < 1024; i++)
                {
                    CollisionList.Images.Add(CollisionListImgA[i]);
                }
            }
            else if (showPathB)
            {
                for (int i = 0; i < 1024; i++)
                {
                    CollisionList.Images.Add(CollisionListImgB[i]);
                }
            }
            CollisionList.SelectedIndex = curColisionMask;
            CollisionList.Refresh();
        }

        private void showPathBToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Do we want to show Path B's Collision Masks instead of Path A's ones?
            if (!showPathB)
            {
                showPathB = showPathBToolStripMenuItem.Checked = true;
                VPLabel.Text = "Currently Viewing: Path B";
                RefreshUI();
            }
            else if (showPathB)
            {
                showPathB = showPathBToolStripMenuItem.Checked = false;
                VPLabel.Text = "Currently Viewing: Path A";
                RefreshUI();
            }
        }

        private void aboutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            About_Form frm = new About_Form();
            frm.ShowDialog(this); //Show the About window
        }

        private void NextButton_Click(object sender, EventArgs e)
        {
            if (tcf != null)
            {
                curColisionMask++;
                if (curColisionMask > 1023) //Don't go above 1024... 1023 + 1 (because it starts at zero) = 1024
                {
                    curColisionMask = 1023;
                }
                RefreshUI();
            }
        }

        private void PrevButton_Click(object sender, EventArgs e)
        {
            if (tcf != null)
            {
                curColisionMask--;
                if (curColisionMask < 0) //Don't go below zero
                {
                    curColisionMask = 0;
                }
                RefreshUI();
            }
        }

        private void GotoNUD_ValueChanged(object sender, EventArgs e)
        {
            if (tcf != null)
            {
                gotoVal = (int)GotoNUD.Value - 1; //Set the goto value, we then take -1 to get the "true value"
            }
        }

        private void GotoButton_Click(object sender, EventArgs e)
        {
            if (tcf != null)
            {
                curColisionMask = gotoVal; //Set the Collision Masl to the desired value
                RefreshUI(); //Show the user the new values
            }
        }

        private void SlopeNUD_ValueChanged(object sender, EventArgs e)
        {
            if (tcf != null)
            {
                if (!showPathB)
                {
                    if (SlopeNUD.Value <= 255)
                    {
                        //tcf.CollisionPath1[curColisionMask].slopeAngle = (byte)(256 - ((float)SlopeNUD.Value / (360f / 0x100))); //Set Slope angle for Path A
                        tcf.CollisionPath1[curColisionMask].slopeAngle = (byte)SlopeNUD.Value; //Set Slope angle for Path A
                    }
                    else
                    {
                        SlopeNUD.Value = 255;
                    }
 

                }
                if (showPathB)
                {
                    if (SlopeNUD.Value <= 255)
                    {
                        //tcf.CollisionPath2[curColisionMask].slopeAngle = (byte)(256 - ((float)SlopeNUD.Value / (360f / 0x100))); //Set Slope angle for Path B
                        tcf.CollisionPath2[curColisionMask].slopeAngle = (byte)SlopeNUD.Value; //Set Slope angle for Path B
                    }
                    else
                    {
                        SlopeNUD.Value = 255;
                    }
                }
                RefreshUI();
            }
        }


        private void RawSlopeNUD_ValueChanged(object sender, EventArgs e)
        {
            if (tcf != null)
            {
                if (!showPathB)
                {
                    if (RawSlopeNUD.Value <= 255)
                    {
                        tcf.CollisionPath1[curColisionMask].slopeAngle = (byte)(256 - ((float)RawSlopeNUD.Value / (360f / 0x100))); //Set Slope angle for Path A
                    }
                    else
                    {
                        RawSlopeNUD.Value = 255;
                    }

                }
                if (showPathB)
                {
                    if (RawSlopeNUD.Value <= 255)
                    {
                        tcf.CollisionPath2[curColisionMask].slopeAngle = (byte)(256 - ((float)RawSlopeNUD.Value / (360f / 0x100))); //Set Slope angle for Path B
                    }
                    else
                    {
                        RawSlopeNUD.Value = 255;
                    }
                }
                RefreshUI();
            }
        }

        private void PhysicsNUD_ValueChanged(object sender, EventArgs e)
        {
            if (tcf != null)
            {
                if (!showPathB)
                {
                    tcf.CollisionPath1[curColisionMask].physics = (byte)PhysicsNUD.Value; //Set the Physics for Path A
                }
                if (showPathB)
                {
                    tcf.CollisionPath2[curColisionMask].physics = (byte)PhysicsNUD.Value; //Set the Physics for Path B
                }
                RefreshUI();
            }
        }

        private void MomentumNUD_ValueChanged(object sender, EventArgs e)
        {
            if (tcf != null)
            {
                if (!showPathB)
                {
                    tcf.CollisionPath1[curColisionMask].momentum = (byte)MomentumNUD.Value; //Set the Momentum value for Path A
                }
                if (showPathB)
                {
                    tcf.CollisionPath2[curColisionMask].momentum = (byte)MomentumNUD.Value; //Set the Momentum value for Path B
                }
                RefreshUI();
            }
        }

        private void UnknownNUD_ValueChanged(object sender, EventArgs e)
        {
            if (tcf != null)
            {
                if (!showPathB)
                {
                    tcf.CollisionPath1[curColisionMask].unknown = (byte)UnknownNUD.Value; //Set the unknown value for Path A
                }
                if (showPathB)
                {
                    tcf.CollisionPath2[curColisionMask].unknown = (byte)UnknownNUD.Value; //Set the unknown value for Path B
                }
                RefreshUI();
            }
        }

        private void SpecialNUD_ValueChanged(object sender, EventArgs e)
        {
            if (tcf != null)
            {
                if (!showPathB)
                {
                    tcf.CollisionPath1[curColisionMask].special = (byte)SpecialNUD.Value; //Set the "Special" value for Path A
                }
                if (showPathB)
                {
                    tcf.CollisionPath2[curColisionMask].special = (byte)SpecialNUD.Value; //Set the "Special" value for Path B
                }
                RefreshUI();
            }
        }

        private void IsCeilingButton_CheckedChanged(object sender, EventArgs e)
        {
            if (tcf != null)
            {
                if (!showPathB)
                {
                    tcf.CollisionPath1[curColisionMask].IsCeiling = IsCeilingButton.Checked; //Set the "IsCeiling" Value for Path A
                }
                if (showPathB)
                {
                    tcf.CollisionPath2[curColisionMask].IsCeiling = IsCeilingButton.Checked; //Set the "IsCeiling" Value for Path B
                }
                RefreshUI();
            }
        }

        private void CollisionList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (CollisionList.SelectedIndex >= 0)
            {
                curColisionMask = CollisionList.SelectedIndex;
            }
            RefreshUI();
        }

        private void copyToOtherPathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!showPathB)
            {
                tcf.CollisionPath2[curColisionMask] = (RSDKv5.TilesConfig.ColllisionMask)tcf.CollisionPath1[curColisionMask].Clone();
                //tcf.CollisionPath2[curColisionMask] = tc;RSDKv5.TilesConfig.ColllisionMask tc
                CollisionListImgB[curColisionMask] = CollisionListImgA[curColisionMask];
                RefreshUI();
            }
            else if (showPathB)
            {
                tcf.CollisionPath1[curColisionMask] = (RSDKv5.TilesConfig.ColllisionMask)tcf.CollisionPath2[curColisionMask].Clone();
                CollisionListImgA[curColisionMask] = CollisionListImgB[curColisionMask];
                RefreshUI();
            }
        }

        private void mirrorPathsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (mirrorPathsToolStripMenuItem1.Checked)
            {
                mirrorPathsToolStripMenuItem1.Checked = MirrorPaths = false;
            }
            else if (!mirrorPathsToolStripMenuItem1.Checked)
            {
                mirrorPathsToolStripMenuItem1.Checked = MirrorPaths = true;
            }
        }

        #region Collision Mask Methods
        private void lb00_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tcf != null)
            {
                if (!showPathB)
                {
                    tcf.CollisionPath1[curColisionMask].Collision[0] = (byte)lb00.SelectedIndex;
                    CollisionListImgA[curColisionMask] = tcf.CollisionPath1[curColisionMask].DrawCMask(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 255, 0));
                    CollisionList.Refresh();
                }
                if (showPathB)
                {
                    tcf.CollisionPath2[curColisionMask].Collision[0] = (byte)lb00.SelectedIndex;
                    CollisionListImgB[curColisionMask] = tcf.CollisionPath2[curColisionMask].DrawCMask(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 255, 0));
                    CollisionList.Refresh();
                }
                RefreshUI();
            }
        }

        private void lb01_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tcf != null)
            {
                if (!showPathB)
                {
                    tcf.CollisionPath1[curColisionMask].Collision[1] = (byte)lb01.SelectedIndex;
                    CollisionListImgA[curColisionMask] = tcf.CollisionPath1[curColisionMask].DrawCMask(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 255, 0));
                    CollisionList.Refresh();
                }
                if (showPathB)
                {
                    tcf.CollisionPath2[curColisionMask].Collision[1] = (byte)lb01.SelectedIndex;
                    CollisionListImgB[curColisionMask] = tcf.CollisionPath2[curColisionMask].DrawCMask(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 255, 0));
                    CollisionList.Refresh();
                }
                RefreshUI();
            }
        }

        private void lb02_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tcf != null)
            {
                if (!showPathB)
                {
                    tcf.CollisionPath1[curColisionMask].Collision[2] = (byte)lb02.SelectedIndex;
                    CollisionListImgA[curColisionMask] = tcf.CollisionPath1[curColisionMask].DrawCMask(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 255, 0));
                    CollisionList.Refresh();
                }
                if (showPathB)
                {
                    tcf.CollisionPath2[curColisionMask].Collision[2] = (byte)lb02.SelectedIndex;
                    CollisionListImgB[curColisionMask] = tcf.CollisionPath2[curColisionMask].DrawCMask(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 255, 0));
                    CollisionList.Refresh();
                }
                RefreshUI();
            }
        }

        private void lb03_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tcf != null)
            {
                if (!showPathB)
                {
                    tcf.CollisionPath1[curColisionMask].Collision[3] = (byte)lb03.SelectedIndex;
                    CollisionListImgA[curColisionMask] = tcf.CollisionPath1[curColisionMask].DrawCMask(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 255, 0));
                    CollisionList.Refresh();
                }
                if (showPathB)
                {
                    tcf.CollisionPath2[curColisionMask].Collision[3] = (byte)lb03.SelectedIndex;
                    CollisionListImgB[curColisionMask] = tcf.CollisionPath2[curColisionMask].DrawCMask(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 255, 0));
                    CollisionList.Refresh();
                }
                RefreshUI();
            }
        }

        private void lb04_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tcf != null)
            {
                if (!showPathB)
                {
                    tcf.CollisionPath1[curColisionMask].Collision[4] = (byte)lb04.SelectedIndex;
                    CollisionListImgA[curColisionMask] = tcf.CollisionPath1[curColisionMask].DrawCMask(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 255, 0));
                    CollisionList.Refresh();
                }
                if (showPathB)
                {
                    tcf.CollisionPath2[curColisionMask].Collision[4] = (byte)lb04.SelectedIndex;
                    CollisionListImgB[curColisionMask] = tcf.CollisionPath2[curColisionMask].DrawCMask(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 255, 0));
                    CollisionList.Refresh();
                }
                RefreshUI();
            }
        }

        private void lb05_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tcf != null)
            {
                if (!showPathB)
                {
                    tcf.CollisionPath1[curColisionMask].Collision[5] = (byte)lb05.SelectedIndex;
                    CollisionListImgA[curColisionMask] = tcf.CollisionPath1[curColisionMask].DrawCMask(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 255, 0));
                    CollisionList.Refresh();
                }
                if (showPathB)
                {
                    tcf.CollisionPath2[curColisionMask].Collision[5] = (byte)lb05.SelectedIndex;
                    CollisionListImgB[curColisionMask] = tcf.CollisionPath2[curColisionMask].DrawCMask(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 255, 0));
                    CollisionList.Refresh();
                }
                RefreshUI();
            }
        }

        private void lb06_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tcf != null)
            {
                if (!showPathB)
                {
                    tcf.CollisionPath1[curColisionMask].Collision[6] = (byte)lb06.SelectedIndex;
                    CollisionListImgA[curColisionMask] = tcf.CollisionPath1[curColisionMask].DrawCMask(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 255, 0));
                    CollisionList.Refresh();
                }
                if (showPathB)
                {
                    tcf.CollisionPath2[curColisionMask].Collision[6] = (byte)lb06.SelectedIndex;
                    CollisionListImgB[curColisionMask] = tcf.CollisionPath2[curColisionMask].DrawCMask(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 255, 0));
                    CollisionList.Refresh();
                }
                RefreshUI();
            }
        }

        private void lb07_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tcf != null)
            {
                if (!showPathB)
                {
                    tcf.CollisionPath1[curColisionMask].Collision[7] = (byte)lb07.SelectedIndex;
                    CollisionListImgA[curColisionMask] = tcf.CollisionPath1[curColisionMask].DrawCMask(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 255, 0));
                    CollisionList.Refresh();
                }
                if (showPathB)
                {
                    tcf.CollisionPath2[curColisionMask].Collision[7] = (byte)lb07.SelectedIndex;
                    CollisionListImgB[curColisionMask] = tcf.CollisionPath2[curColisionMask].DrawCMask(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 255, 0));
                    CollisionList.Refresh();
                }
                RefreshUI();
            }
        }

        private void lb08_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tcf != null)
            {
                if (!showPathB)
                {
                    tcf.CollisionPath1[curColisionMask].Collision[8] = (byte)lb08.SelectedIndex;
                    CollisionListImgA[curColisionMask] = tcf.CollisionPath1[curColisionMask].DrawCMask(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 255, 0));
                    CollisionList.Refresh();
                }
                if (showPathB)
                {
                    tcf.CollisionPath2[curColisionMask].Collision[8] = (byte)lb08.SelectedIndex;
                    CollisionListImgB[curColisionMask] = tcf.CollisionPath2[curColisionMask].DrawCMask(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 255, 0));
                    CollisionList.Refresh();
                }
                RefreshUI();
            }
        }

        private void lb09_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tcf != null)
            {
                if (!showPathB)
                {
                    tcf.CollisionPath1[curColisionMask].Collision[9] = (byte)lb09.SelectedIndex;
                    CollisionListImgA[curColisionMask] = tcf.CollisionPath1[curColisionMask].DrawCMask(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 255, 0));
                    CollisionList.Refresh();
                }
                if (showPathB)
                {
                    tcf.CollisionPath2[curColisionMask].Collision[9] = (byte)lb09.SelectedIndex;
                    CollisionListImgB[curColisionMask] = tcf.CollisionPath2[curColisionMask].DrawCMask(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 255, 0));
                    CollisionList.Refresh();
                }
                RefreshUI();
            }
        }

        private void lb10_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tcf != null)
            {
                if (!showPathB)
                {
                    tcf.CollisionPath1[curColisionMask].Collision[10] = (byte)lb10.SelectedIndex;
                    CollisionListImgA[curColisionMask] = tcf.CollisionPath1[curColisionMask].DrawCMask(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 255, 0));
                    CollisionList.Refresh();
                }
                if (showPathB)
                {
                    tcf.CollisionPath2[curColisionMask].Collision[10] = (byte)lb10.SelectedIndex;
                    CollisionListImgB[curColisionMask] = tcf.CollisionPath2[curColisionMask].DrawCMask(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 255, 0));
                    CollisionList.Refresh();
                }
                RefreshUI();
            }
        }

        private void lb11_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tcf != null)
            {
                if (!showPathB)
                {
                    tcf.CollisionPath1[curColisionMask].Collision[11] = (byte)lb11.SelectedIndex;
                    CollisionListImgA[curColisionMask] = tcf.CollisionPath1[curColisionMask].DrawCMask(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 255, 0));
                    CollisionList.Refresh();
                }
                if (showPathB)
                {
                    tcf.CollisionPath2[curColisionMask].Collision[11] = (byte)lb11.SelectedIndex;
                    CollisionListImgB[curColisionMask] = tcf.CollisionPath2[curColisionMask].DrawCMask(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 255, 0));
                    CollisionList.Refresh();
                }
                RefreshUI();
            }
        }

        private void lb12_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tcf != null)
            {
                if (!showPathB)
                {
                    tcf.CollisionPath1[curColisionMask].Collision[12] = (byte)lb12.SelectedIndex;
                    CollisionListImgA[curColisionMask] = tcf.CollisionPath1[curColisionMask].DrawCMask(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 255, 0));
                    CollisionList.Refresh();
                }
                if (showPathB)
                {
                    tcf.CollisionPath2[curColisionMask].Collision[12] = (byte)lb12.SelectedIndex;
                    CollisionListImgB[curColisionMask] = tcf.CollisionPath2[curColisionMask].DrawCMask(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 255, 0));
                    CollisionList.Refresh();
                }
                RefreshUI();
            }
        }

        private void lb13_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tcf != null)
            {
                if (!showPathB)
                {
                    tcf.CollisionPath1[curColisionMask].Collision[13] = (byte)lb13.SelectedIndex;
                    CollisionListImgA[curColisionMask] = tcf.CollisionPath1[curColisionMask].DrawCMask(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 255, 0));
                    CollisionList.Refresh();
                }
                if (showPathB)
                {
                    tcf.CollisionPath2[curColisionMask].Collision[13] = (byte)lb13.SelectedIndex;
                    CollisionListImgB[curColisionMask] = tcf.CollisionPath2[curColisionMask].DrawCMask(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 255, 0));
                    CollisionList.Refresh();
                }
                RefreshUI();
            }
        }

        private void lb14_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tcf != null)
            {
                if (!showPathB)
                {
                    tcf.CollisionPath1[curColisionMask].Collision[14] = (byte)lb14.SelectedIndex;
                    CollisionListImgA[curColisionMask] = tcf.CollisionPath1[curColisionMask].DrawCMask(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 255, 0));
                    CollisionList.Refresh();
                }
                if (showPathB)
                {
                    tcf.CollisionPath2[curColisionMask].Collision[14] = (byte)lb14.SelectedIndex;
                    CollisionListImgB[curColisionMask] = tcf.CollisionPath2[curColisionMask].DrawCMask(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 255, 0));
                    CollisionList.Refresh();
                }
                RefreshUI();
            }
        }

        private void lb15_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tcf != null)
            {
                if (!showPathB)
                {
                    tcf.CollisionPath1[curColisionMask].Collision[15] = (byte)lb15.SelectedIndex;
                    CollisionListImgA[curColisionMask] = tcf.CollisionPath1[curColisionMask].DrawCMask(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 255, 0));
                    CollisionList.Refresh();
                }
                if (showPathB)
                {
                    tcf.CollisionPath2[curColisionMask].Collision[15] = (byte)lb15.SelectedIndex;
                    CollisionListImgB[curColisionMask] = tcf.CollisionPath2[curColisionMask].DrawCMask(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 255, 0));
                    CollisionList.Refresh();
                }
                RefreshUI();
            }
        }

        private void cb00_CheckedChanged(object sender, EventArgs e)
        {
            if (tcf != null)
            {
                if (!showPathB)
                {
                    tcf.CollisionPath1[curColisionMask].HasCollision[0] = cb00.Checked;
                    CollisionListImgA[curColisionMask] = tcf.CollisionPath1[curColisionMask].DrawCMask(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 255, 0));
                    CollisionList.Refresh();
                }
                if (showPathB)
                {
                    tcf.CollisionPath2[curColisionMask].HasCollision[0] = cb00.Checked;
                    CollisionListImgB[curColisionMask] = tcf.CollisionPath2[curColisionMask].DrawCMask(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 255, 0));
                    CollisionList.Refresh();
                }
                RefreshUI();
            }
        }

        private void cb01_CheckedChanged(object sender, EventArgs e)
        {
            if (tcf != null)
            {
                if (!showPathB)
                {
                    tcf.CollisionPath1[curColisionMask].HasCollision[1] = cb01.Checked;
                    CollisionListImgA[curColisionMask] = tcf.CollisionPath1[curColisionMask].DrawCMask(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 255, 0));
                    CollisionList.Refresh();
                }
                if (showPathB)
                {
                    tcf.CollisionPath2[curColisionMask].HasCollision[1] = cb01.Checked;
                    CollisionListImgB[curColisionMask] = tcf.CollisionPath2[curColisionMask].DrawCMask(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 255, 0));
                    CollisionList.Refresh();
                }
                RefreshUI();
            }
        }

        private void cb02_CheckedChanged(object sender, EventArgs e)
        {
            if (tcf != null)
            {
                if (!showPathB)
                {
                    tcf.CollisionPath1[curColisionMask].HasCollision[2] = cb02.Checked;
                    CollisionListImgA[curColisionMask] = tcf.CollisionPath1[curColisionMask].DrawCMask(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 255, 0));
                    CollisionList.Refresh();
                }
                if (showPathB)
                {
                    tcf.CollisionPath2[curColisionMask].HasCollision[2] = cb02.Checked;
                    CollisionListImgB[curColisionMask] = tcf.CollisionPath2[curColisionMask].DrawCMask(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 255, 0));
                    CollisionList.Refresh();
                }
                RefreshUI();
            }
        }

        private void cb03_CheckedChanged(object sender, EventArgs e)
        {
            if (tcf != null)
            {
                if (!showPathB)
                {
                    tcf.CollisionPath1[curColisionMask].HasCollision[3] = cb03.Checked;
                    CollisionListImgA[curColisionMask] = tcf.CollisionPath1[curColisionMask].DrawCMask(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 255, 0));
                    CollisionList.Refresh();
                }
                if (showPathB)
                {
                    tcf.CollisionPath2[curColisionMask].HasCollision[3] = cb03.Checked;
                    CollisionListImgB[curColisionMask] = tcf.CollisionPath2[curColisionMask].DrawCMask(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 255, 0));
                    CollisionList.Refresh();

                }
                RefreshUI();
            }
        }

        private void cb04_CheckedChanged(object sender, EventArgs e)
        {
            if (tcf != null)
            {
                if (!showPathB)
                {
                    tcf.CollisionPath1[curColisionMask].HasCollision[4] = cb04.Checked;
                    CollisionListImgA[curColisionMask] = tcf.CollisionPath1[curColisionMask].DrawCMask(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 255, 0));
                    CollisionList.Refresh();
                }
                if (showPathB)
                {
                    tcf.CollisionPath2[curColisionMask].HasCollision[4] = cb04.Checked;
                    CollisionListImgB[curColisionMask] = tcf.CollisionPath2[curColisionMask].DrawCMask(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 255, 0));
                    CollisionList.Refresh();
                }
                RefreshUI();
            }
        }

        private void cb05_CheckedChanged(object sender, EventArgs e)
        {
            if (tcf != null)
            {
                if (!showPathB)
                {
                    tcf.CollisionPath1[curColisionMask].HasCollision[5] = cb05.Checked;
                    CollisionListImgA[curColisionMask] = tcf.CollisionPath1[curColisionMask].DrawCMask(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 255, 0));
                    CollisionList.Refresh();
                }
                if (showPathB)
                {
                    tcf.CollisionPath2[curColisionMask].HasCollision[5] = cb05.Checked;
                    CollisionListImgB[curColisionMask] = tcf.CollisionPath2[curColisionMask].DrawCMask(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 255, 0));
                    CollisionList.Refresh();
                }
                RefreshUI();
            }
        }

        private void cb06_CheckedChanged(object sender, EventArgs e)
        {
            if (tcf != null)
            {
                if (!showPathB)
                {
                    tcf.CollisionPath1[curColisionMask].HasCollision[6] = cb06.Checked;
                    CollisionListImgA[curColisionMask] = tcf.CollisionPath1[curColisionMask].DrawCMask(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 255, 0));
                    CollisionList.Refresh();
                }
                if (showPathB)
                {
                    tcf.CollisionPath2[curColisionMask].HasCollision[6] = cb06.Checked;
                    CollisionListImgB[curColisionMask] = tcf.CollisionPath2[curColisionMask].DrawCMask(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 255, 0));
                    CollisionList.Refresh();
                }
                RefreshUI();
            }
        }

        private void cb07_CheckedChanged(object sender, EventArgs e)
        {
            if (tcf != null)
            {
                if (!showPathB)
                {
                    tcf.CollisionPath1[curColisionMask].HasCollision[7] = cb07.Checked;
                    CollisionListImgA[curColisionMask] = tcf.CollisionPath1[curColisionMask].DrawCMask(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 255, 0));
                    CollisionList.Refresh();
                }
                if (showPathB)
                {
                    tcf.CollisionPath2[curColisionMask].HasCollision[7] = cb07.Checked;
                    CollisionListImgB[curColisionMask] = tcf.CollisionPath2[curColisionMask].DrawCMask(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 255, 0));
                    CollisionList.Refresh();
                }
                RefreshUI();
            }
        }

        private void cb08_CheckedChanged(object sender, EventArgs e)
        {
            if (tcf != null)
            {
                if (!showPathB)
                {
                    tcf.CollisionPath1[curColisionMask].HasCollision[8] = cb08.Checked;
                    CollisionListImgA[curColisionMask] = tcf.CollisionPath1[curColisionMask].DrawCMask(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 255, 0));
                    CollisionList.Refresh();
                }
                if (showPathB)
                {
                    tcf.CollisionPath2[curColisionMask].HasCollision[8] = cb08.Checked;
                    CollisionListImgB[curColisionMask] = tcf.CollisionPath2[curColisionMask].DrawCMask(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 255, 0));
                    CollisionList.Refresh();
                }
                RefreshUI();
            }
        }

        private void cb09_CheckedChanged(object sender, EventArgs e)
        {
            if (tcf != null)
            {
                if (!showPathB)
                {
                    tcf.CollisionPath1[curColisionMask].HasCollision[9] = cb09.Checked;
                    CollisionListImgA[curColisionMask] = tcf.CollisionPath1[curColisionMask].DrawCMask(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 255, 0));
                    CollisionList.Refresh();
                }
                if (showPathB)
                {
                    tcf.CollisionPath2[curColisionMask].HasCollision[9] = cb09.Checked;
                    CollisionListImgB[curColisionMask] = tcf.CollisionPath2[curColisionMask].DrawCMask(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 255, 0));
                    CollisionList.Refresh();
                }
                RefreshUI();
            }
        }

        private void cb10_CheckedChanged(object sender, EventArgs e)
        {
            if (tcf != null)
            {
                if (!showPathB)
                {
                    tcf.CollisionPath1[curColisionMask].HasCollision[10] = cb10.Checked;
                    CollisionListImgA[curColisionMask] = tcf.CollisionPath1[curColisionMask].DrawCMask(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 255, 0));
                    CollisionList.Refresh();
                }
                if (showPathB)
                {
                    tcf.CollisionPath2[curColisionMask].HasCollision[10] = cb10.Checked;
                    CollisionListImgB[curColisionMask] = tcf.CollisionPath2[curColisionMask].DrawCMask(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 255, 0));
                    CollisionList.Refresh();
                }
                RefreshUI();
            }
        }

        private void cb11_CheckedChanged(object sender, EventArgs e)
        {
            if (tcf != null)
            {
                if (!showPathB)
                {
                    tcf.CollisionPath1[curColisionMask].HasCollision[11] = cb11.Checked;
                    CollisionListImgA[curColisionMask] = tcf.CollisionPath1[curColisionMask].DrawCMask(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 255, 0));
                    CollisionList.Refresh();
                }
                if (showPathB)
                {
                    tcf.CollisionPath2[curColisionMask].HasCollision[11] = cb11.Checked;
                    CollisionListImgB[curColisionMask] = tcf.CollisionPath2[curColisionMask].DrawCMask(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 255, 0));
                    CollisionList.Refresh();
                }
                RefreshUI();
            }
        }

        private void cb12_CheckedChanged(object sender, EventArgs e)
        {
            if (tcf != null)
            {
                if (!showPathB)
                {
                    tcf.CollisionPath1[curColisionMask].HasCollision[12] = cb12.Checked;
                    CollisionListImgA[curColisionMask] = tcf.CollisionPath1[curColisionMask].DrawCMask(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 255, 0));
                    CollisionList.Refresh();
                }
                if (showPathB)
                {
                    tcf.CollisionPath2[curColisionMask].HasCollision[12] = cb12.Checked;
                    CollisionListImgB[curColisionMask] = tcf.CollisionPath2[curColisionMask].DrawCMask(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 255, 0));
                    CollisionList.Refresh();
                }
                RefreshUI();
            }
        }

        private void cb13_CheckedChanged(object sender, EventArgs e)
        {
            if (tcf != null)
            {
                if (!showPathB)
                {
                    tcf.CollisionPath1[curColisionMask].HasCollision[13] = cb13.Checked;
                    CollisionListImgA[curColisionMask] = tcf.CollisionPath1[curColisionMask].DrawCMask(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 255, 0));
                    CollisionList.Refresh();
                }
                if (showPathB)
                {
                    tcf.CollisionPath2[curColisionMask].HasCollision[13] = cb13.Checked;
                    CollisionListImgB[curColisionMask] = tcf.CollisionPath2[curColisionMask].DrawCMask(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 255, 0));
                    CollisionList.Refresh();
                }
                RefreshUI();
            }
        }

        private void cb14_CheckedChanged(object sender, EventArgs e)
        {
            if (tcf != null)
            {
                if (!showPathB)
                {
                    tcf.CollisionPath1[curColisionMask].HasCollision[14] = cb14.Checked;
                    CollisionListImgA[curColisionMask] = tcf.CollisionPath1[curColisionMask].DrawCMask(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 255, 0));
                    CollisionList.Refresh();
                }
                if (showPathB)
                {
                    tcf.CollisionPath2[curColisionMask].HasCollision[14] = cb14.Checked;
                    CollisionListImgB[curColisionMask] = tcf.CollisionPath2[curColisionMask].DrawCMask(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 255, 0));
                    CollisionList.Refresh();
                }
                RefreshUI();
            }
        }

        private void cb15_CheckedChanged(object sender, EventArgs e)
        {
            if (tcf != null)
            {
                if (!showPathB)
                {
                    tcf.CollisionPath1[curColisionMask].HasCollision[15] = cb15.Checked;
                    CollisionListImgA[curColisionMask] = tcf.CollisionPath1[curColisionMask].DrawCMask(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 255, 0));
                    CollisionList.Refresh();
                }
                if (showPathB)
                {
                    tcf.CollisionPath2[curColisionMask].HasCollision[15] = cb15.Checked;
                    CollisionListImgB[curColisionMask] = tcf.CollisionPath2[curColisionMask].DrawCMask(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 255, 0));
                    CollisionList.Refresh();
                }
                RefreshUI();
            }
        }
        #endregion

        private void saveUncompressedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (filepath != null) //Did we open a file?
            {
                tcf.WriteUnc(filepath);
            }
            else //if not then use Save As instead
            {
                saveAsUncompressedToolStripMenuItem_Click(this, e);
            }
        }

        private void saveAsUncompressedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Title = "Save Uncompressed As...";
            dlg.DefaultExt = ".bin";
            dlg.Filter = "RSDKv5 Tileconfig Files (*.bin)|*.bin";

            if (dlg.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
            {
                tcf.WriteUnc(dlg.FileName); //Write Uncompressed
            }
        }
        private void toolsToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void InitializeComponents()
        {
            this.SuspendLayout();
            // 
            // Mainform
            // 
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Name = "Mainform";
            this.ResumeLayout(false);

        }

        public class PictureBoxNearestNeighbor : PictureBox
        {
            protected override void OnPaint(PaintEventArgs paintEventArgs)
            {
                paintEventArgs.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                base.OnPaint(paintEventArgs);
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (!changingModes)
            {
                unCheckModes();
                Properties.Settings.Default.ViewerSetting = 0;
                radioButton1.Checked = true;
                CollisionPicBox.Visible = true;
                Properties.Settings.Default.Save();
                changingModes = false;
                RefreshUI();

            }
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (!changingModes)
            {
                unCheckModes();
                radioButton2.Checked = true;
                Properties.Settings.Default.ViewerSetting = 1;
                Properties.Settings.Default.Save();
                TilePicBox.Visible = true;
                changingModes = false;
                RefreshUI();

            }
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            if (!changingModes)
            {
                unCheckModes();
                Properties.Settings.Default.ViewerSetting = 2;
                Properties.Settings.Default.Save();
                radioButton3.Checked = true;
                overlayPicBox.Visible = true;
                changingModes = false;
                RefreshUI();

            }
        }

        void unCheckModes()
        {
            changingModes = true;
            radioButton1.Checked = false;
            radioButton2.Checked = false;
            radioButton3.Checked = false;
            TilePicBox.Visible = false;
            CollisionPicBox.Visible = false;
            overlayPicBox.Visible = false;

        }

        public void lb00_scrolling(object sender, MouseEventArgs e)
        {
            var list = lb00;
            if (e.Delta <= -1)
            {
                if (list.SelectedIndex > 0)
                {
                    list.SelectedIndex--;
                }
            }
            else
            {
                if (list.SelectedIndex < 15)
                {
                    list.SelectedIndex++;
                }
            }
        }

        public void lb01_scrolling(object sender, MouseEventArgs e)
        {
            var list = lb01;
            if (e.Delta <= -1)
            {
                if (list.SelectedIndex > 0)
                {
                    list.SelectedIndex--;
                }
            }
            else
            {
                if (list.SelectedIndex < 15)
                {
                    list.SelectedIndex++;
                }
            }
        }

        public void lb02_scrolling(object sender, MouseEventArgs e)
        {
            var list = lb02;
            if (e.Delta <= -1)
            {
                if (list.SelectedIndex > 0)
                {
                    list.SelectedIndex--;
                }
            }
            else
            {
                if (list.SelectedIndex < 15)
                {
                    list.SelectedIndex++;
                }
            }
        }

        public void lb03_scrolling(object sender, MouseEventArgs e)
        {
            var list = lb03;
            if (e.Delta <= -1)
            {
                if (list.SelectedIndex > 0)
                {
                    list.SelectedIndex--;
                }
            }
            else
            {
                if (list.SelectedIndex < 15)
                {
                    list.SelectedIndex++;
                }
            }
        }

        public void lb04_scrolling(object sender, MouseEventArgs e)
        {
            var list = lb04;
            if (e.Delta <= -1)
            {
                if (list.SelectedIndex > 0)
                {
                    list.SelectedIndex--;
                }
            }
            else
            {
                if (list.SelectedIndex < 15)
                {
                    list.SelectedIndex++;
                }
            }
        }

        public void lb05_scrolling(object sender, MouseEventArgs e)
        {
            var list = lb05;
            if (e.Delta <= -1)
            {
                if (list.SelectedIndex > 0)
                {
                    list.SelectedIndex--;
                }
            }
            else
            {
                if (list.SelectedIndex < 15)
                {
                    list.SelectedIndex++;
                }
            }
        }

        public void lb06_scrolling(object sender, MouseEventArgs e)
        {
            var list = lb06;
            if (e.Delta <= -1)
            {
                if (list.SelectedIndex > 0)
                {
                    list.SelectedIndex--;
                }
            }
            else
            {
                if (list.SelectedIndex < 15)
                {
                    list.SelectedIndex++;
                }
            }
        }

        public void lb07_scrolling(object sender, MouseEventArgs e)
        {
            var list = lb07;
            if (e.Delta <= -1)
            {
                if (list.SelectedIndex > 0)
                {
                    list.SelectedIndex--;
                }
            }
            else
            {
                if (list.SelectedIndex < 15)
                {
                    list.SelectedIndex++;
                }
            }
        }

        public void lb08_scrolling(object sender, MouseEventArgs e)
        {
            var list = lb08;
            if (e.Delta <= -1)
            {
                if (list.SelectedIndex > 0)
                {
                    list.SelectedIndex--;
                }
            }
            else
            {
                if (list.SelectedIndex < 15)
                {
                    list.SelectedIndex++;
                }
            }
        }

        public void lb09_scrolling(object sender, MouseEventArgs e)
        {
            var list = lb09;
            if (e.Delta <= -1)
            {
                if (list.SelectedIndex > 0)
                {
                    list.SelectedIndex--;
                }
            }
            else
            {
                if (list.SelectedIndex < 15)
                {
                    list.SelectedIndex++;
                }
            }
        }

        public void lb10_scrolling(object sender, MouseEventArgs e)
        {
            var list = lb10;
            if (e.Delta <= -1)
            {
                if (list.SelectedIndex > 0)
                {
                    list.SelectedIndex--;
                }
            }
            else
            {
                if (list.SelectedIndex < 15)
                {
                    list.SelectedIndex++;
                }
            }
        }

        public void lb11_scrolling(object sender, MouseEventArgs e)
        {
            var list = lb11;
            if (e.Delta <= -1)
            {
                if (list.SelectedIndex > 0)
                {
                    list.SelectedIndex--;
                }
            }
            else
            {
                if (list.SelectedIndex < 15)
                {
                    list.SelectedIndex++;
                }
            }
        }

        public void lb12_scrolling(object sender, MouseEventArgs e)
        {
            var list = lb12;
            if (e.Delta <= -1)
            {
                if (list.SelectedIndex > 0)
                {
                    list.SelectedIndex--;
                }
            }
            else
            {
                if (list.SelectedIndex < 15)
                {
                    list.SelectedIndex++;
                }
            }
        }

        public void lb13_scrolling(object sender, MouseEventArgs e)
        {
            var list = lb13;
            if (e.Delta <= -1)
            {
                if (list.SelectedIndex > 0)
                {
                    list.SelectedIndex--;
                }
            }
            else
            {
                if (list.SelectedIndex < 15)
                {
                    list.SelectedIndex++;
                }
            }
        }

        public void lb14_scrolling(object sender, MouseEventArgs e)
        {
            var list = lb14;
            if (e.Delta <= -1)
            {
                if (list.SelectedIndex > 0)
                {
                    list.SelectedIndex--;
                }
            }
            else
            {
                if (list.SelectedIndex < 15)
                {
                    list.SelectedIndex++;
                }
            }
        }

        public void lb15_scrolling(object sender, MouseEventArgs e)
        {
            var list = lb15;
            if (e.Delta <= -1)
            {
                if (list.SelectedIndex > 0)
                {
                    list.SelectedIndex--;
                }
            }
            else
            {
                if (list.SelectedIndex < 15)
                {
                    list.SelectedIndex++;
                }
            }
        }

        private void tileViewRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (lockRadioButtons == false)
            {
                uncheckListViews();
                Properties.Settings.Default.ListSetting = 1;
                Properties.Settings.Default.Save();
                tileViewRadioButton.Checked = true;
                lockRadioButtons = false;
                refreshCollision();
            }
        }

        private void collisionViewRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (lockRadioButtons == false)
            {
                uncheckListViews();
                Properties.Settings.Default.ListSetting = 0;
                Properties.Settings.Default.Save();
                collisionViewRadioButton.Checked = true;
                lockRadioButtons = false;
                refreshCollision();
            }
        }

        void uncheckListViews()
        {
            lockRadioButtons = true;
            collisionViewRadioButton.Checked = false;
            tileViewRadioButton.Checked = false;
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!showPathB)
            {
                TileClipboard = tcf.CollisionPath1[curColisionMask];
                RefreshUI();
            }
            else if (showPathB)
            {
                TileClipboard = tcf.CollisionPath2[curColisionMask];
                RefreshUI();
            }
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!showPathB)
            {
                tcf.CollisionPath1[curColisionMask] = TileClipboard;
                RefreshUI();
            }
            else if (showPathB)
            {
                tcf.CollisionPath2[curColisionMask] = TileClipboard;
                RefreshUI();
            }
        }

        void refreshCollision()
        {

            if (filepath != null)
            {
                CollisionList.Images.Clear();
                CollisionListImgA.Clear();
                CollisionListImgB.Clear();

                for (int i = 0; i < 1024; i++)
                {
                    if (Properties.Settings.Default.ListSetting == 0)
                    {
                        CollisionListImgA.Add(tcf.CollisionPath1[i].DrawCMask(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 255, 0)));
                        CollisionList.Images.Add(CollisionListImgA[i]);
                    }
                    else
                    {
                        CollisionListImgA.Add(Tiles[i]);
                        CollisionList.Images.Add(Tiles[i]);
                    }

                }

                for (int i = 0; i < 1024; i++)
                {
                    if (Properties.Settings.Default.ListSetting == 0)
                    {
                        CollisionListImgB.Add(tcf.CollisionPath2[i].DrawCMask(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 255, 0)));
                        CollisionList.Images.Add(CollisionListImgB[i]);
                    }
                    else
                    {
                        CollisionListImgB.Add(Tiles[i]);
                        CollisionList.Images.Add(Tiles[i]);
                    }
                }
                CollisionList.Refresh();

                RefreshUI(); //update the UI

            }
        }

        private void openSingleCollisionMaskToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Title = "Import CollisionMask...";
            dlg.DefaultExt = ".rcm";
            dlg.Filter = "Singular RSDKv5 CollisionMask (*.rcm)|*.rcm";

            if (dlg.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
            {
                RSDKv5.Reader Reader1 = new RSDKv5.Reader(dlg.FileName);
                RSDKv5.Reader Reader2 = new RSDKv5.Reader(dlg.FileName);
                tcf.CollisionPath1[curColisionMask] = new RSDKv5.TilesConfig.ColllisionMask(Reader1);
                Reader1.Close();
                tcf.CollisionPath2[curColisionMask] = new RSDKv5.TilesConfig.ColllisionMask(Reader2);
                Reader2.Close();
            }
            RefreshUI();
            //RefreshCollisionList(true);
        }

        private void exportCurrentCollisionMaskAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Title = "Export As...";
            dlg.DefaultExt = ".rcm";
            dlg.Filter = "Singular RSDKv5 CollisionMask (*.rcm)|*.rcm";

            if (dlg.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
            {
                BinaryWriter Writer1 = new BinaryWriter(File.Create(dlg.FileName));
                BinaryWriter Writer2 = new BinaryWriter(File.Create(dlg.FileName));
                tcf.CollisionPath1[curColisionMask].WriteUnc(Writer1);
                tcf.CollisionPath2[curColisionMask].WriteUnc(Writer2);
                Writer1.Close();
                Writer2.Close();
                RefreshUI();
            }
        }

        private void importFromOlderRSDKVersionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Title = "Open Compressed";
            dlg.DefaultExt = ".bin";
            dlg.Filter = "RSDK ColllisionMask Files (CollisionMasks.bin)|CollisionMasks.bin";

            if (dlg.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
            {
                curColisionMask = 0; //Set the current collision mask to zero (avoids rare errors)
                filepath = dlg.FileName;
                tcf = new RSDKv5.TilesConfig();
                RSDKv4.CollisionMask tcfOLD = new RSDKv4.CollisionMask(dlg.FileName);
                string t = filepath.Replace("CollisionMasks.bin", "16x16tiles.gif"); //get the path to the stage's tileset
                LoadTileSet(new Bitmap(t)); //load each 16x16 tile into the list


                CollisionListImgA.Clear();
                CollisionListImgB.Clear();
                CollisionList.Images.Clear();

                for (int i = 0; i < 1024; i++)
                {
                    CollisionListImgA.Add(tcfOLD.CollisionPath1[i].DrawCMask(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 255, 0)));
                    CollisionListImgB.Add(tcfOLD.CollisionPath2[i].DrawCMask(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 255, 0)));

                    CollisionList.Images.Add(CollisionListImgA[i]);
                    CollisionList.Images.Add(CollisionListImgB[i]);

                    tcf.CollisionPath1[i].Collision = tcfOLD.CollisionPath1[i].Collision;
                    tcf.CollisionPath1[i].HasCollision = tcfOLD.CollisionPath1[i].HasCollision;
                    tcf.CollisionPath1[i].IsCeiling = tcfOLD.CollisionPath1[i].isCeiling;
                    tcf.CollisionPath1[i].momentum = tcfOLD.CollisionPath1[i].momentum;
                    tcf.CollisionPath1[i].physics = tcfOLD.CollisionPath1[i].physics;
                    tcf.CollisionPath1[i].slopeAngle = tcfOLD.CollisionPath1[i].slopeAngle;
                    tcf.CollisionPath1[i].special = 0;
                    tcf.CollisionPath1[i].unknown = tcfOLD.CollisionPath1[i].unknown;

                    tcf.CollisionPath2[i].Collision = tcfOLD.CollisionPath2[i].Collision;
                    tcf.CollisionPath2[i].HasCollision = tcfOLD.CollisionPath2[i].HasCollision;
                    tcf.CollisionPath2[i].IsCeiling = tcfOLD.CollisionPath2[i].isCeiling;
                    tcf.CollisionPath2[i].momentum = tcfOLD.CollisionPath2[i].momentum;
                    tcf.CollisionPath2[i].physics = tcfOLD.CollisionPath2[i].physics;
                    tcf.CollisionPath2[i].slopeAngle = tcfOLD.CollisionPath2[i].slopeAngle;
                    tcf.CollisionPath2[i].special = 0;
                    tcf.CollisionPath2[i].unknown = tcfOLD.CollisionPath2[i].unknown;
                }
                CollisionList.SelectedIndex = curColisionMask - 1;
                CollisionList.Refresh();

                RefreshUI(); //update the UI
            }
        }
    }
}
