using System;
using System.Windows.Forms;
using Emgu.CV.Structure;
using Emgu.CV; // It provides works capture,images functions.

namespace TestCam
{
    public partial class Form1 : Form
    {
        private Capture capture; //Points to the camera
        private Image<Bgr, Byte> IMG; //List of blue,green and red images. This is orginal image
        private Image<Gray, Byte> GrayImg;
        private Image<Gray, Byte> BWImg;
        public Form1()
        {
            InitializeComponent();
        }
        private void processFrame(object sender, EventArgs e)
        {
            if (capture == null) //There is no object of a camera. You are not connect to the camera.
            {
                try //Try to create to the object or try to connect to the camera.
                {
                    capture = new Capture(); 
                }
                catch (NullReferenceException excpt)
                {
                    MessageBox.Show(excpt.Message);
                }
            }

            IMG = capture.QueryFrame(); //get a picture from a camera            
            GrayImg = IMG.Convert<Gray, Byte>();
            BWImg = GrayImg.ThresholdBinaryInv(new Gray(50),new Gray(255));
            try
            {
                imageBox1.Image = IMG;
                imageBox2.Image = GrayImg;
                imageBox3.Image = BWImg;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            Application.Idle += processFrame; //It provides walking in the background the images
            button1.Enabled = false;
            button2.Enabled = true;
        }
        private void button2_Click(object sender, EventArgs e)
        {
            Application.Idle -= processFrame; //It stops
            button1.Enabled = true;
            button2.Enabled = false;
        }    
        private void button3_Click(object sender, EventArgs e)
        {
            IMG.Save("Image" +  ".jpg");
        }

        private void timer1_Tick(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
