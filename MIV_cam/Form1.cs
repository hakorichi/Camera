using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Resources;
using System.IO;
using System.Media;

namespace MIV_cam
{
    public partial class MIV_cam : Form
    {

        int Cc; //индекс последнего видеоустройства(не canon)
        Boolean CanonBool;
        Boolean RecordBool;

        public MIV_cam()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
           CamFunction.Initialize();
           Canon.Initialize(pictureBox1.Image);

           List<String> s = CamFunction.CamList();
           Cc = s.Count  - 1;
           foreach (String cam in Canon.GetCamList()) s.Add(cam);
           comboBox1.DataSource = s;

           CanonBool = false;
           RecordBool = false;
           Application.Idle += video;
        }

        private void video(object sender, EventArgs e)
        {
            if (CanonBool)
                pictureBox1.Image = CamFunction.FilterBitmap(Canon.GetBitmap());
            else
                pictureBox1.Image = CamFunction.GetBitmap();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

            if (comboBox1.SelectedIndex > Cc)
            {
                CanonBool = true;
                Canon.CloseSession();
                Canon.OpenSession(comboBox1.SelectedIndex - Cc - 1);
            }
            else
            {
                CanonBool = true;
                CamFunction.ChendeCam(comboBox1.SelectedIndex, 0, 0);
            }
        }
        private void pictureBox2_Click(object sender, EventArgs e)
        {
            //if (!CanonBool) 
            //  Canon.TakePhoto();
            //else            
                CamFunction.SavePhoto(textBox1.Text);
        }
        private void pictureBox3_Click(object sender, EventArgs e)
        {
            if (CamFunction.EnabledVideo())
            {
                CamFunction.StopVideo();
                RecordBool = false;
                pictureBox3.Image = Properties.Resources.iconplay;
            }
            else
            {
                CamFunction.RecordVideo(textBox1.Text);
                RecordBool = true;
                pictureBox3.Image = Properties.Resources.iconpause;
            }
        }

        #region Filtres
        private void Filter1_Click(object sender, EventArgs e)
        {
            CamFunction.Filter = 0;
        }
        private void Filter2_Click(object sender, EventArgs e)
        {
            CamFunction.Filter = 1;
        }
        private void Filter3_Click(object sender, EventArgs e)
        {
            CamFunction.Filter = 2;
        }
        private void Filter4_Click(object sender, EventArgs e)
        {
            CamFunction.Filter = 3;
        }
        private void Filter5_Click(object sender, EventArgs e)
        {
            CamFunction.Filter = 4;
        }
        private void Filter6_Click(object sender, EventArgs e)
        {
            CamFunction.Filter = 5;
        }
        private void Filter7_Click(object sender, EventArgs e)
        {
            CamFunction.Filter = 6;
        }
        private void Filter8_Click(object sender, EventArgs e)
        {
            CamFunction.Filter = 7;
        }
        private void Filter9_Click(object sender, EventArgs e)
        {
            CamFunction.Filter = 8;
        }
        private void Filter10_Click(object sender, EventArgs e)
        {
            CamFunction.Filter = 9;
        }

        private void Filter11_Click(object sender, EventArgs e)
        {
            CamFunction.Mirror_double = !CamFunction.Mirror_double;
        }
        #endregion

        #region Settings
        private void trackBar1_MouseCaptureChanged(object sender, EventArgs e)
        {
            CamFunction.brightness = trackBar1.Value;
        }
        private void trackBar2_MouseCaptureChanged(object sender, EventArgs e)
        {
            CamFunction.contrast = trackBar2.Value;
        }
        private void trackBarB_MouseCaptureChanged(object sender, EventArgs e)
        {
            CamFunction.levelB = trackBarB.Value;
        }
        private void trackBarG_MouseCaptureChanged(object sender, EventArgs e)
        {
            CamFunction.levelG = trackBarG.Value;
        }
        private void trackBarR_MouseCaptureChanged(object sender, EventArgs e)
        {
            CamFunction.levelR = trackBarR.Value;
        }
        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            CamFunction.Mirror = checkBox2.Checked;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            CamFunction.Reset();
            trackBar1.Value = 100;
            trackBar2.Value = 100;
            trackBarB.Value = 100;
            trackBarG.Value = 100;
            trackBarR.Value = 100;
        }
        private void pictureBox4_Click(object sender, EventArgs e)
        {
            CamFunction.flipRotatin += 1;
            if (CamFunction.flipRotatin > 3) CamFunction.flipRotatin = 0;
        }
        #endregion

        #region ButtonAnimals
        private void pictureBox4_MouseEnter(object sender, EventArgs e)
        {
            pictureBox4.Image = Properties.Resources.iconrotateH;
        }
        private void pictureBox2_MouseEnter(object sender, EventArgs e)
        {
            pictureBox2.Image = Properties.Resources.iconcameraH;
        }
        private void pictureBox3_MouseEnter(object sender, EventArgs e)
        {
            if (RecordBool)
                pictureBox3.Image = Properties.Resources.iconpauseH;
            else
                pictureBox3.Image = Properties.Resources.iconplayH;
        }
        private void pictureBox4_MouseLeave(object sender, EventArgs e)
        {
            pictureBox4.Image = Properties.Resources.iconrotate;
        }
        private void pictureBox2_MouseLeave(object sender, EventArgs e)
        {
            pictureBox2.Image = Properties.Resources.iconcamera;
        }
        private void pictureBox3_MouseLeave(object sender, EventArgs e)
        {
            if (RecordBool)
                pictureBox3.Image = Properties.Resources.iconpause;
            else
                pictureBox3.Image = Properties.Resources.iconplay;
        }
        #endregion

        private void MIV_cam_FormClosing(object sender, FormClosingEventArgs e)
        {
            CamFunction.close();
            Canon.Close();
            this.Dispose();
            Application.Exit();
        }


    }
}
