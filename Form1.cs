using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HSV_Assignment
    //Eric Beeler
{
    public partial class Form1 : Form
    {
        //initialize bars to corect values
        private int _HmaxBar = 33;
        private int _HminBar = 17;
        private int _SmaxBar = 255;
        private int _SminBar = 93;
        private int _VmaxBar = 255;
        private int _VminBar = 151;
        private int _BinaryTrackBar = 150;
        private Robot _robot = new Robot("Com8");//find correct port in order to communicate


        private VideoCapture _capture;
        private Thread _captureThread;



        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load_1(object sender, EventArgs e)
        {
            //setup values and webcam
            _capture = new VideoCapture(0);
            _captureThread = new Thread(DisplayWebcam);
            _captureThread.Start();
            BinaryTrackBar.Value = _BinaryTrackBar;
            HminBar.Value = _HminBar;
            HmaxBar.Value = _HmaxBar;
            SminBar.Value = _SminBar;
            SmaxBar.Value = _SmaxBar;
            VminBar.Value = _VminBar;
            HmaxBar.Value = _VmaxBar;
            //Setting up Min and Max Bar values

   
        }
       

        private void DisplayWebcam()
        {//create display functions
            while (_capture.IsOpened)
            {
                Mat frame = _capture.QueryFrame();
                // create new box size
                int newHeight = (frame.Size.Height * RawPictureBox.Size.Width) / frame.Size.Width;
                Size newSize = new Size(RawPictureBox.Size.Width, newHeight);
                CvInvoke.Resize(frame, frame, newSize);

                RawPictureBox.Image = frame.Bitmap;
                // convert to and display binary box
                Mat grayFrame = new Mat();
                Mat binaryFrame = new Mat();
                CvInvoke.CvtColor(frame, grayFrame, Emgu.CV.CvEnum.ColorConversion.Bgr2Gray);
                CvInvoke.Threshold(grayFrame, binaryFrame, _BinaryTrackBar, 255, Emgu.CV.CvEnum.ThresholdType.Binary);
               //display with threshold
                BinaryPictureBox.Image = binaryFrame.Bitmap;


                Mat hsvFrame = new Mat();
                CvInvoke.CvtColor(frame, hsvFrame, Emgu.CV.CvEnum.ColorConversion.Bgr2Hsv);
                //setup hsv and split into the 3 channels
                Mat[] hsvChannels = hsvFrame.Split();
                //complete h,s,and v channels and their respective boxes


                //hue
                Mat hueFilter = new Mat();
                CvInvoke.InRange(hsvChannels[0], new ScalarArray(_HminBar), new ScalarArray(_HmaxBar), hueFilter);
                Invoke(new Action(() => { HpictureBox.Image = hueFilter.Bitmap; }));

                //saturation
                Mat saturationFilter = new Mat();
                CvInvoke.InRange(hsvChannels[1], new ScalarArray(_SminBar), new ScalarArray(_SmaxBar), saturationFilter);
                Invoke(new Action(() => { SpictureBox.Image = saturationFilter.Bitmap; }));
                //value
                Mat valueFilter = new Mat();
                CvInvoke.InRange(hsvChannels[2], new ScalarArray(_VminBar), new ScalarArray(_VmaxBar), valueFilter);
                Invoke(new Action(() => { VpictureBox.Image = valueFilter.Bitmap; }));
                //assemble into combined image that can be changed
                Mat combinedImage = new Mat();
                CvInvoke.BitwiseAnd(hueFilter, saturationFilter, combinedImage);
                CvInvoke.BitwiseAnd(combinedImage, valueFilter, combinedImage);
                Invoke(new Action(() => { HSVPictureBox.Image = combinedImage.Bitmap; }));
                //Combined Image invoke/bitmap
                //Make yellow tracking code using HSV outline
                //with hue,sat, and val
                Mat YellowHueFilter = new Mat();
                CvInvoke.InRange(hsvChannels[0], new ScalarArray(16), new ScalarArray(32), YellowHueFilter);
                Invoke(new Action(() => { HpictureBox.Image = YellowHueFilter.Bitmap; }));

                Mat YellowSatFilter = new Mat();
                CvInvoke.InRange(hsvChannels[1], new ScalarArray(94), new ScalarArray(255), YellowSatFilter);
                Invoke(new Action(() => { SpictureBox.Image = YellowSatFilter.Bitmap; }));

                Mat YellowValFilter = new Mat();
                CvInvoke.InRange(hsvChannels[2], new ScalarArray(150), new ScalarArray(255), YellowValFilter);
                Invoke(new Action(() => { VpictureBox.Image = YellowValFilter.Bitmap; }));

                //Do the same exact thing for the red line code with hue, sat, and value

                Mat RedHueFilter = new Mat();
                CvInvoke.InRange(hsvChannels[0], new ScalarArray(130), new ScalarArray(177), YellowHueFilter);
                Invoke(new Action(() => { HpictureBox.Image = RedHueFilter.Bitmap; }));

                Mat RedSatFilter = new Mat();
                CvInvoke.InRange(hsvChannels[1], new ScalarArray(0), new ScalarArray(255), YellowSatFilter);
                Invoke(new Action(() => { SpictureBox.Image = RedSatFilter.Bitmap; }));

                Mat RedValFilter = new Mat();
                CvInvoke.InRange(hsvChannels[2], new ScalarArray(99), new ScalarArray(255), YellowValFilter);
                Invoke(new Action(() => { VpictureBox.Image = RedValFilter.Bitmap; }));

                //Merge both yellow and red into complete and combined images

                Mat YellowCombinedImage = new Mat();
                CvInvoke.BitwiseAnd(YellowHueFilter, YellowSatFilter, YellowCombinedImage);
                CvInvoke.BitwiseAnd(YellowCombinedImage, YellowValFilter, YellowCombinedImage);
                Invoke(new Action(() => { YellowBox.Image = YellowCombinedImage.Bitmap; }));
                //Copy the yellow code for the red variables
                Mat RedCombinedImage = new Mat();
                CvInvoke.BitwiseAnd(RedHueFilter, RedSatFilter, RedCombinedImage);
                CvInvoke.BitwiseAnd(RedCombinedImage, RedValFilter, RedCombinedImage);
                Invoke(new Action(() => { RedPictureBox.Image = RedCombinedImage.Bitmap; }));

                // Esablish code that will count pixels in all combined HSV picture boxes 
                int CP = 0, YP = 0, RP = 0;
                Image<Gray, byte> Cimage = combinedImage.ToImage<Gray, byte>();
                Image<Gray, byte> Yimage = YellowCombinedImage.ToImage<Gray, byte>();
                Image<Gray, byte> Rimage = RedCombinedImage.ToImage<Gray, byte>();

                for (int y = 0; y < combinedImage.Height; y++)
                {
                    for (int x = 0; x < combinedImage.Width; x++)
                    {
                        //Combined pixel count
                        if (Cimage.Data[y, x, 0 ] == 255)
                        {
                            CP++;
                        }
                        //Yellow pixel count
                        if (Yimage.Data[y, x, 0 ] == 255)
                        {
                            YP++;
                        }
                        //Red
                        //if (Rimage.Data[y, x, 0 ] == 255)
                        {
                            RP++;
                        }
                    }
                }
           
                //Split the yellow into 5 different pieces, in order to read for direction using a slice width varriable
                //each slice will be named by its position
                int YPFL = 0, YPL = 0, YPM = 0, YPR = 0, YPFR = 0;
                int SliceWidth = 0;
                for (int y = 0; y < binaryFrame.Height; y++)
                {
                    for (int x = 0; x < SliceWidth; x++)
                    {
                        if (Yimage.Data[y, x, 0] == 255)
                            YPFL++;
                    }
                    for (int x = SliceWidth; x < 2*SliceWidth; x++)
                    {
                        if (Yimage.Data[y, x, 0] == 255)
                            YPL++;
                    }
                    for (int x = 2*SliceWidth; x < 3*SliceWidth; x++)
                    {
                        if (Yimage.Data[y, x, 0] == 255)
                            YPM++;
                    }
                    for (int x = 3*SliceWidth; x < 4*SliceWidth; x++)
                    {
                        if (Yimage.Data[y, x, 0] == 255)
                            YPR++;
                    }
                    for (int x = 4*SliceWidth; x <= 5*SliceWidth; x++)
                    {
                        if (Yimage.Data[y, x, 0] == 255)
                            YPFR++;
                    }

                }
                    //set stop
                        if(RP > 4000)
                         {
                        _robot.Move(Robot.STOP);
                         }
                //set hard left and left
                else if (YPR < 2 * YPL && YPFR <= YPFL && YPM >= 0)
                    {
                        _robot.Move(Robot.HardLeft);
                    }
                else if (YPR < YPL && YPFR <= YPFL && YPM >= 0)
                    {
                        _robot.Move(Robot.liggity);

                    }
                //set mid
                else if (YPR == YPL && YPFR == YPFL && YPM >= 0)
                    {
                        _robot.Move(Robot.GO);
                    }
                //set right and hard right
                else if (YPR > YPL && YPFR >= YPFL && YPM >= 0)
                    {
                        _robot.Move(Robot.riggity);
                    }
                else if (YPR > 2 * YPL && YPFR >= YPFL && YPM >= 0)
                    {
                        _robot.Move(Robot.HardRight);
                    }
                else if(YPR == YPL && YPFR == YPFL && YPM == 0)
                    {
                        _robot.Move(Robot.Resume);
                    }

            }
        }

        private void Form1_FormClosing(object sender, FormClosedEventArgs e)
        {
            _captureThread.Abort();
        }

        //Set up scroll bars
        private void BinaryTrackBar_Scroll(object sender, EventArgs e)
        {
            _BinaryTrackBar = BinaryTrackBar.Value;
        }

        private void HminBar_Scroll(object sender, EventArgs e)
        {
            _HminBar = HminBar.Value;
        }

        private void HmaxBar_Scroll(object sender, EventArgs e)
        {
            _HmaxBar = HmaxBar.Value;
        }

        private void SminBar_Scroll(object sender, EventArgs e)
        {
            _SminBar = SminBar.Value;
        }

        private void SmaxBar_Scroll(object sender, EventArgs e)
        {
            _SmaxBar = SmaxBar.Value;

        }

        private void VminBar_Scroll(object sender, EventArgs e)
        {
            _VminBar = VminBar.Value;
        }

        private void VmaxBar_Scroll(object sender, EventArgs e)
        {
            _VmaxBar = VmaxBar.Value;
        }


    }       
}
