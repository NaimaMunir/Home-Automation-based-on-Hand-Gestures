//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.ColorBasics
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Navigation;
    using System.Windows.Shapes;
    using System.Drawing;
    using System.Drawing.Text;
    using System.Drawing.Drawing2D;
    using System.Globalization;
    using System.IO;
    using Microsoft.Kinect;
    using System.Windows.Threading;
    using System.IO.Ports;
    using System.Threading;
   

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor sensor;
        SerialPort _serialPort;
       // private WriteableBitmap colorBitmap3;
        
        /// <summary>
        /// Bitmap that will hold color information
        /// </summary>
        private WriteableBitmap colorBitmap;
        private DispatcherTimer timer = new DispatcherTimer();

        /// <summary>
        /// Intermediate storage for the color data received from the camera
        /// </summary>
        private byte[] colorPixels;
        public byte[] colorPixels2;
        public byte[] colorPixels3;

       
        

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        ///
        public double l1;
        public double l;
        public double x;
        public double g;
        public double ff = 0;

        public MainWindow()
        {
            InitializeComponent();

            
            //*************************************************************88//
            //***************************************************************//
            var image2 = new BitmapImage();
            using (var stream = new FileStream("s1.png", FileMode.Open))
            {
                image2.BeginInit();
                image2.CacheOption = BitmapCacheOption.OnLoad;
                image2.StreamSource = stream;
                image2.EndInit();
            }

            //image21.Source = image2;
            ImageSource ims2 = image2;
            BitmapImage bitmapImage2 = (BitmapImage)ims2;
            int nStride2 = (bitmapImage2.PixelWidth * bitmapImage2.Format.BitsPerPixel + 7) / 8;
            colorPixels2 = new byte[bitmapImage2.PixelHeight * nStride2];
            bitmapImage2.CopyPixels(colorPixels2, nStride2, 0);

            //&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&//

            var image3 = new BitmapImage();
            using (var stream = new FileStream("s3.png", FileMode.Open))
            {
                image3.BeginInit();
                image3.CacheOption = BitmapCacheOption.OnLoad;
                image3.StreamSource = stream;
                image3.EndInit();
            }

            //image21.Source = image2;
            ImageSource ims3 = image3;
            BitmapImage bitmapImage3 = (BitmapImage)ims3;
            int nStride3 = (bitmapImage3.PixelWidth * bitmapImage3.Format.BitsPerPixel + 7) / 8;
            colorPixels3 = new byte[bitmapImage3.PixelHeight * nStride3];
            bitmapImage3.CopyPixels(colorPixels3, nStride3, 0);


            //&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&//

            

            l1 = colorPixels2.Length;
            this.checkBox1.IsChecked = true;
               
        }
           
            
            
           

            
       

        /// <summary>
        /// Execute startup tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            // Look through all sensors and start the first connected one.
            // This requires that a Kinect is connected at the time of app startup.
            // To make your app robust against plug/unplug, 
            // it is recommended to use KinectSensorChooser provided in Microsoft.Kinect.Toolkit (See components in Toolkit Browser).
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    break;
                }
            }

            if (null != this.sensor)
            {
                // Turn on the color stream to receive color frames
                this.sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);

                // Allocate space to put the pixels we'll receive
                this.colorPixels = new byte[this.sensor.ColorStream.FramePixelDataLength];

                // This is the bitmap we'll display on-screen
                this.colorBitmap = new WriteableBitmap(this.sensor.ColorStream.FrameWidth, this.sensor.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);

                // Set the image we display to point to the bitmap where we'll put the image data
                this.VideoControl.Source = this.colorBitmap;

                // Add an event handler to be called whenever there is new color frame data
                this.sensor.ColorFrameReady += this.SensorColorFrameReady;

                // Start the sensor!
                try
                {
                    this.sensor.Start();
                   
                }
                catch (IOException)
                {
                    this.sensor = null;
                }
            }

            if (null == this.sensor)
            {
                this.statusBarText.Text = Properties.Resources.NoKinectReady;
            }
        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (null != this.sensor)
            {
                this.sensor.Stop();
            }
        }
        
       
        
        
        
        //*****************************GET_AVERAGE*********************************//

        public double get_avg(byte[] arr)
        {
            double sum = 0;
            for (int y = 0; y < arr.Length; y = y + 1)
            {
                sum = sum + (arr[y] * arr[y]);

            }

            return sum / arr.Length;

        }

        public double comb_avg(byte[] arr, byte[] arr1)
        {
            double sum = 0;

            for (int z = 0; z < arr.Length; z++)
            {
                sum = sum + (arr[z] * arr1[z]);
            }

            return sum / arr.Length;
        }

        public double norm_corr(byte[] arr, byte[] arr1)
        {
            double corr;
            double num = comb_avg(arr, arr1);
            double num2 = Math.Sqrt((get_avg(arr) * (get_avg(arr1))));
            return corr = num / num2;
        }

    
    //********************************************************************************888//
    /// <summary>
        /// Event handler for Kinect sensor's ColorFrameReady event
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void SensorColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {

            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame != null)
                {
                    // Copy the pixel data from the image to a temporary array

                    
                    colorFrame.CopyPixelDataTo(this.colorPixels);
                    l = colorPixels.Length;
                   

                       
                       // Write the pixel data into our bitmap
                    this.colorBitmap.WritePixels(
                        new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight),
                        this.colorPixels,
                        this.colorBitmap.PixelWidth * sizeof(int),
                        0);
                    
                    
                }



               
            }
            

        }
       

       





        private void SaveImage()
        {
            
            using (FileStream fileStream = new FileStream(string.Format("{0}.Jpg", Guid.NewGuid().ToString()), System.IO.FileMode.Create))
            {
                BitmapSource imageSource = (BitmapSource)VideoControl.Source;
                JpegBitmapEncoder jpegEncoder = new JpegBitmapEncoder();
                jpegEncoder.Frames.Add(BitmapFrame.Create(imageSource));
                jpegEncoder.Save(fileStream);
                fileStream.Close();
            }

           

        }

        public void StartTimer()
        {
            this.timer.Interval = new TimeSpan(0, 0, 2);
            this.timer.Start();
            this.timer.Tick += this.Timer_Tick;
        }
        public void Timer_Tick(object sender, object e)
        {

            if (this.sensor.IsRunning && this.sensor.ColorStream.IsEnabled)
            {

                if (ff < 2)
                {
                    this.SaveImage();
                   
                    ff++;
                }
                else
                {
                    x = norm_corr(colorPixels, colorPixels2); //fan
                    g = norm_corr(colorPixels, colorPixels3);//bulb

                    if ( g > 0.6 )
                    {

                        /***************/
                        _serialPort = new SerialPort("COM10", 9600, Parity.None, 8, StopBits.One)
                              {
                                  Handshake = Handshake.None,
                                  ReadTimeout = 500,
                                  WriteTimeout = 500
                              };

                        if (!_serialPort.IsOpen)                                                                      //checking serial port availibilty
                            _serialPort.Open();

                        while (true)                                                                                  //loop  
                        {
                            //Thread.Sleep(500);                                                                      //delay for 0.5 second
                            _serialPort.WriteLine("A");                                                             //write 'A' to serial port, send to arduino
                           //Thread.Sleep(1000);                                                                      //delay for 0.5 second
                            //_serialPort.WriteLine("E");                                                             //write 'B' to serial port, send to arduino
                        }
                        /**/
                    }
                   if (x < 0.6 )
                    {
                        /***************/
                        _serialPort = new SerialPort("COM10", 9600, Parity.None, 8, StopBits.One)
                        {
                            Handshake = Handshake.None,
                            ReadTimeout = 500,
                            WriteTimeout = 500
                        };

                        if (!_serialPort.IsOpen)                                                                      //checking serial port availibilty
                            _serialPort.Open();

                        while (true)                                                                                  //loop  
                        {
                           // Thread.Sleep(500);                                                                      //delay for 0.5 second
                            _serialPort.WriteLine("B");                                                             //write 'A' to serial port, send to arduino
                            //Thread.Sleep(1000);                                                                      //delay for 0.5 second
                            //_serialPort.WriteLine("E");                                                             //write 'B' to serial port, send to arduino
                        }
                        /**/

                    }

                   

                    sensor.Stop();
                }
            }
        }

        private void checkBox1_Checked(object sender, RoutedEventArgs e)
        {
            this.StartTimer();
           
        }
        
        private void textBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            
            textBox1.Text = x.ToString();//s1
            


        }

        private void textBox2_TextChanged(object sender, TextChangedEventArgs e)
        {
            textBox2.Text = g.ToString();//s3
        }

       
    }
}