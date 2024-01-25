using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace opencv_lite_example
{
    public unsafe partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        enum DEPTH
        {
            DEPTH_8U = 0,
            DEPTH_8S = 1,
            DEPTH_16S = 2,
            DEPTH_32S = 3,
            DEPTH_32F = 4,
            DEPTH_64F = 5,
        };


        [StructLayout(LayoutKind.Sequential)]
        public struct TMatrix
        {
            public int Width; // The width of a matrix
            public int Height; // The height of a matrix
            public int WidthStep; // The number of bytes occupied by a line element of a matrix
            public int Channel; // Number of matrix channels
            public int Depth; // The type of matrix element
            public byte* Data; // Data of a matrix
            public int Reserved; // Reserved use

            public TMatrix(int width, int height, int widthStep, int depth, int channel, byte* scan0)
            {
                Width = width;
                Height = height;
                WidthStep = widthStep;
                Depth = depth;
                Channel = channel;
                Data = scan0;
                Reserved = 0;
            }
        };


        [DllImport("opencv-lite.dll")]
        private static extern bool MatchTemplate(ref TMatrix src, ref TMatrix template, ref TMatrix* dest);

        [DllImport("opencv-lite.dll")]
        private static extern bool MinMaxLoc(ref TMatrix src, ref int minPosX, ref int minPosY, ref int maxPosX,
            ref int maxPosY);

        [DllImport("opencv-lite.dll")]
        private static extern bool FreeMatrix(TMatrix** dstImg);

        private void button1_Click(object sender, EventArgs e)
        {
            int minPosX = 0, minPosY = 0, maxPosX = 0, maxPosY = 0;

            var srcBmp = (Bitmap) pictureBox2.Image;
            var dstBmp = (Bitmap) pictureBox1.Image;


            var srcBmpData = srcBmp.LockBits(new Rectangle(0, 0, srcBmp.Width, srcBmp.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            var dstBmpData = dstBmp.LockBits(new Rectangle(0, 0, dstBmp.Width, dstBmp.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            var sw = new Stopwatch();
            sw.Start();

            var srcImg = new TMatrix(srcBmp.Width, srcBmp.Height, srcBmpData.Stride, (int) DEPTH.DEPTH_8U, 3,
                (byte*) srcBmpData.Scan0);
            var destImg = new TMatrix(dstBmp.Width, dstBmp.Height, dstBmpData.Stride, (int) DEPTH.DEPTH_8U, 3,
                (byte*) dstBmpData.Scan0);

            TMatrix* dest = null;
            MatchTemplate(ref destImg, ref srcImg, ref dest);
            MinMaxLoc(ref *dest, ref minPosX, ref minPosY, ref maxPosX, ref maxPosY);
            FreeMatrix(&dest);
            srcBmp.UnlockBits(srcBmpData);
            dstBmp.UnlockBits(dstBmpData);

            var P = new Pen(Color.Red);
            var G = Graphics.FromImage(dstBmp);
            G.DrawRectangle(P, new Rectangle(minPosX, minPosY, srcImg.Width, srcImg.Height));
            P.Dispose();
            G.Dispose();

            label1.Text = @"匹配用时:" + sw.ElapsedMilliseconds + @" ms";
            pictureBox1.Refresh();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                //获取嵌入dll文件的字节数组
                var dll = Properties.Resources.opencv_lite;
                //设置释放路径   导出路径
                var strPath = @"./opencv-lite.dll";

                var directoryPath = Path.GetDirectoryName(strPath);

                // 检查文件夹是否存在，如果不存在则创建它  
                if (!Directory.Exists(directoryPath))
                {
                    if (directoryPath != null) Directory.CreateDirectory(directoryPath);
                }

                //创建dll文件（覆盖模式）  
                using (var fs = new FileStream(strPath, FileMode.Create))
                {
                    fs.Write(dll, 0, dll.Length);
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(@"释放opencv-lite.dll失败!");
            }
        }
    }
}