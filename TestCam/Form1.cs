using System;
using System.Windows.Forms;
using Emgu.CV.Structure;
using System.IO.Ports; // To use connect ardunio 
using Emgu.CV; // It provides works capture,images functions.
namespace TestCam
{
    public partial class Form1 : Form
    {
        private Capture capture; //Points to the camera
        private Image<Bgr, Byte> IMG; //List of blue,green and red images. This is orginal image. 3 byte
        private Image<Gray, Byte> GrayImg;//1 byte
        private Image<Gray, Byte> BWImg;
        private double myScale=0; //camera size
        private int Xpx, Ypx,N;
        private double Xcm, Ycm;
        private double Px, Py,Pz;
        private double d1 = 4.0; //Robotic arm cm
        static SerialPort _serialPort;
        public byte[] Buff = new byte[2]; // Values exported #C to ardunio. It is two cause Th1 and Th2. 
        public Form1()
        {
            InitializeComponent();
            _serialPort = new SerialPort();
            _serialPort.BaudRate = 9600;
            _serialPort.PortName = "COM3";
            _serialPort.Open();
        }
        private void processFrame(object sender, EventArgs e)
        {
            if (capture == null) //There is no object of a camera. You are not connect to the camera yet.
            {
                try //Try to create to the object or try to connect to the camera.
                {
                    capture = new Capture(); //Created new object and connected to the camera. It is default bc I have 1 cam.
                }
                catch (NullReferenceException excpt)
                {
                    MessageBox.Show(excpt.Message);
                }
            }

            IMG = capture.QueryFrame(); //get a picture from a camera            
            GrayImg = IMG.Convert<Gray, Byte>();
            BWImg = GrayImg.ThresholdBinaryInv(new Gray(50),new Gray(255));
            Xpx = 0; //Specify center of the x object as zero.
            Ypx = 0; //Specify center of the y object as zero.
            N = 0; //Number of pixels
            for(int i= 0; i < BWImg.Width; i++)
            {
                for(int j= 0; j < BWImg.Height; j++)
                {
                    if (BWImg[j,i].Intensity > 128) //It is bigger than 128 because the threshold max value is 255, calcaleted half of this.
                    {
                        N++;
                        Xpx = Xpx + i;
                        Ypx = Ypx + j;
                    } 
                }
            }
            if (N > 0)
            {
                myScale = 150.0 / BWImg.Width; //The 150.0 can be change depent on your camara width.
                Xpx = Xpx / N; //Center point of the foreground in x object.
                Ypx = Ypx / N; //Center point of the foreground in y object.
                Xpx = Xpx - BWImg.Width / 2;
                Ypx=BWImg.Height / 2-Ypx;
                Xcm = Xpx * myScale; //Converts pixels to cm
                Ycm = Ypx * myScale;
                textBox1.Text = Xcm.ToString();
                textBox2.Text = Ycm.ToString();
                textBox3.Text = N.ToString();

                //Calculate Inverse Kinematic by formules
                Pz = Ycm + 25; //Distance between camera and keyboard.
                Py = -Xcm;
                Px = 100;
                double Th1=Math.Atan2(Py, Px);
                double Th2=Math.Atan(Math.Sin(Th1)*(Pz-d1)/Py);
                Th1=Th1*(180/Math.PI);
                Th2 = Th2 * (180 / Math.PI);
                Th1 = Th1 + 25;
                Th2 = Th2 + 107;
                Buff[0] = (byte)Th1;
                Buff[1] = (byte)Th2;
                _serialPort.Write(Buff, 0, 2);
            }
            else
            {
                textBox1.Text = Xcm.ToString();
                textBox2.Text = Ycm.ToString();
                textBox3.Text = N.ToString();
            }
            
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
            timer1.Enabled = true;
            button1.Enabled = false;
            button2.Enabled = true;
        }
        private void button2_Click(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            button1.Enabled = true;
            button2.Enabled = false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            IMG.Save("Image" +  ".jpg");
        }
        //Added timer because without timer system will crash. It makes computer slower than ardunio.
        private void timer1_Tick(object sender, EventArgs e)
        {
            processFrame(sender, e);
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
