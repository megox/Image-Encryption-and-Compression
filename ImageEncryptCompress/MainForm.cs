using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection.Emit;
using System.Text;
using System.Windows.Forms;

namespace ImageEncryptCompress
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            // Set the form to full screen
            this.WindowState = FormWindowState.Maximized;

            // Optionally, remove the window border
            this.FormBorderStyle = FormBorderStyle.None;
        }

        RGBPixel[,] ImageMatrix;
        RGBPixel[,] ImageMatrix2;
        RGBPixel[,] ImageMatrix3;
        RGBPixel[,] ImageMatrix_decompress;
        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //Open the browsed image and display it
                string OpenedFilePath = openFileDialog1.FileName;
                ImageMatrix = ImageOperations.OpenImage(OpenedFilePath);
                ImageOperations.DisplayImage(ImageMatrix, pictureBox1);
            }
            txtWidth.Text = ImageOperations.GetWidth(ImageMatrix).ToString();
            txtHeight.Text = ImageOperations.GetHeight(ImageMatrix).ToString();
        }



        public static Bitmap ConvertMatrixToBitmap(RGBPixel[,] rgbMatrix)
        {
            int width = rgbMatrix.GetLength(1);
            int height = rgbMatrix.GetLength(0);
            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color color = Color.FromArgb(rgbMatrix[y, x].red, rgbMatrix[y, x].green, rgbMatrix[y, x].blue);
                    bitmap.SetPixel(x, y, color);
                }
            }
            return bitmap;
        }

        // Method to save a Bitmap to a file
        public static void SaveBitmap(Bitmap bitmap)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.FileName = "out";
            saveFileDialog1.Filter = "bmp files (*.bmp)|*.bmp|All files (*.*)|*.*";
            saveFileDialog1.RestoreDirectory = true;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                bitmap.Save(saveFileDialog1.FileName, ImageFormat.Bmp);
            }
        }



        private void btnGaussSmooth_Click(object sender, EventArgs e)
        {
            /*double sigma = double.Parse(txtGaussSigma.Text);
            int maskSize = (int)nudMaskSize.Value;
            ImageMatrix = ImageOperations.GaussianFilter1D(ImageMatrix, maskSize, sigma);
            //  ImageMatrix2=*/

            string InitialSeed = txtInitialSeed.Text;
            int TapPosition = int.Parse(txtTapPosition.Text);
            
            Stopwatch stopwatch_EC = new Stopwatch();
            stopwatch_EC.Start();
            ImageMatrix2 = ImageOperations.Encryption(ImageMatrix, InitialSeed, TapPosition);
            ImageOperations.DisplayImage(ImageMatrix, pictureBox2);
            int x = ImageOperations.compress(ImageMatrix2, InitialSeed, TapPosition);
            stopwatch_EC .Stop();


            Stopwatch stopwatch_DD = new Stopwatch();
            stopwatch_DD.Start();
            string filepath = "C:\\fos_cygwin\\FOS_CODES\\FOS_PROJECT_2023_TEMPLATE\\Image-Encryption-and-Compression\\huff_out.bin";
            var items = ImageOperations.decompress(filepath);
            var items2 = ImageOperations.Decryption(items.Item1, items.Item2, items.Item3);
            stopwatch_DD.Stop();


            Bitmap bitmap = ConvertMatrixToBitmap(items2);
            SaveBitmap(bitmap);

            ImageOperations.DisplayImage(items2, pictureBox5);

            int size_after_compression = (x + 7) / 8;
            int size_before_compression = (ImageMatrix.GetLength(0) * ImageMatrix.GetLength(1) * 3);




            textBox1.Text = stopwatch_EC.Elapsed.TotalSeconds.ToString();
            textBox2.Text = (stopwatch_EC.Elapsed.TotalSeconds / 60).ToString();

            textBox7.Text = stopwatch_DD.Elapsed.TotalSeconds.ToString();
            textBox6.Text = (stopwatch_DD.Elapsed.TotalSeconds / 60).ToString();



            textBox3.Text = size_after_compression.ToString(); ;


            textBox4.Text = size_before_compression.ToString();

            textBox5.Text = ((  (double) size_after_compression * 100 / (double) size_before_compression)).ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }
    }
}