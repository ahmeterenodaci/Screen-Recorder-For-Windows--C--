using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using Accord.Video.FFMPEG;

namespace ScreenRecorder
{
    public partial class Form1 : Form
    {
        int frameRate;
        int fileCount;
        Timer recordScreenTimer;
        Bitmap target;
        Rectangle screenSize;
        Point mouseLocation;
        string tempPath;
        string savePath;

        public Form1()
        {
            InitializeComponent();
            InitializeFileName();
            InitializeParameters();
            InitializeTimer();
        }
        
        void InitializeFileName()
        {
            fileNameText.Text = "Record_" + Convert.ToString((int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds);
        }
        
        void InitializeParameters()
        {
            frameRate = 10;
            fileCount = 0;
            screenSize = Screen.PrimaryScreen.Bounds;
            target = new Bitmap(screenSize.Width, screenSize.Height);
        }
        
        void InitializeTimer()
        {
            recordScreenTimer = new Timer();
            recordScreenTimer.Tick += new EventHandler(TakeAScreenShot);
            recordScreenTimer.Interval = 1000 / frameRate;
        }
        
        void StartRecording()
        {
            recordScreenTimer.Start();
        }
        
        void StopRecording()
        {
            recordScreenTimer.Stop();
        }
        
        void SaveRecording()
        {
            string fileName = savePath + "\\" + fileNameText.Text + ".mp4";
            using (VideoFileWriter vFWriter = new VideoFileWriter())
            {
                vFWriter.Open(fileName, screenSize.Width, screenSize.Height, frameRate, VideoCodec.Default, 4500000);
                for (int i = 0; i < fileCount; i++)
                {
                    Bitmap imageFrame = Image.FromFile(tempPath + "/temp_" + i + ".png") as Bitmap;
                    vFWriter.WriteVideoFrame(imageFrame);
                    imageFrame.Dispose();
                }
                vFWriter.Close();
            }
            Directory.Delete(tempPath, true);
        }
        
        void TakeAScreenShot(object sender, EventArgs e)
        {
            using (Graphics g = Graphics.FromImage(target))
            {
                g.CopyFromScreen(0, 0, 0, 0, new Size(screenSize.Width, screenSize.Height));
            }
            target.Save(tempPath + "temp_" + fileCount++ + ".png", ImageFormat.Png);
        }
        
        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            mouseLocation = e.Location;
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int dx = e.Location.X - mouseLocation.X;
                int dy = e.Location.Y - mouseLocation.Y;
                this.Location = new Point(this.Location.X + dx, this.Location.Y + dy);
            }
        }

        private void exitButton_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void minimizeButton_Click(object sender, EventArgs e)
        {
            MinimizeApp();
        }

        private void browseFilesButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog folderBrowserDialog = new OpenFileDialog())
            {
                folderBrowserDialog.ValidateNames = false;
                folderBrowserDialog.CheckFileExists = false;
                folderBrowserDialog.CheckPathExists = true;
                folderBrowserDialog.FileName = "Choose a folder";
                
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    savePathText.Text = Path.GetDirectoryName(folderBrowserDialog.FileName);
                }
            }
        }

        private void recordButton_Click(object sender, EventArgs e)
        {
            ToggleButton();
            SetPath();
            SetClientSize();
            MinimizeApp();
            StartRecording();
        }
        
        private void stopButton_Click(object sender, EventArgs e)
        {
            ToggleButton();
            StopRecording();
            SaveRecording();
            InitializeFileName();
            SetClientSize();
            fileCount = 0;
        }
        
        private void MinimizeApp()
        {
            WindowState = FormWindowState.Minimized;
        }
        
        private void ToggleButton()
        {
            recordButton.Enabled = !recordButton.Enabled;
            stopButton.Enabled = !stopButton.Enabled;
        }
        
        private void SetPath()
        {
            savePath = savePathText.Text;
            tempPath = savePath + "\\Temp\\";
            Directory.CreateDirectory(tempPath);
        }
        
        private void SetClientSize()
        {
            if (this.ClientSize.Height > 120)
            {
                this.ClientSize = new Size(750, 120);
                return;
            }
            this.ClientSize = new Size(750, 220);
        }
       
    }
}
