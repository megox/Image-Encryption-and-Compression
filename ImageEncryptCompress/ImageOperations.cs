using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Collections.Specialized;
using System.Diagnostics;
///Algorithms Project
///Intelligent Scissors
///
///
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Web;
using System.Drawing.Drawing2D;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;



namespace ImageEncryptCompress
{



    public class node
    {
        public int value { get; set; }
        public int freq { get; set; }
        public node left { get; set; }
        public node right { get; set; }

        public node(int symbol, int frequency)
        {
            value = symbol;
            freq = frequency;
            left = null;
            right = null;
        }
    }



    public class Heap
    {
        private int capacity = 10000;
        private node[] heap = new node[10000];
        private int count = 0;

        public int Count { get { return count; } }

        public void Insert(node node)
        {
            if (count == capacity)
            {
                ExtendCapacity();
            }

            heap[count] = node;
            count++;
            HeapifyUp(count - 1);
        }

        private void ExtendCapacity()
        {
            int newCapacity = capacity * 2;
            node[] newHeap = new node[newCapacity];
            Array.Copy(heap, newHeap, capacity);
            heap = newHeap;
            capacity = newCapacity;
        }



        public node Top()
        {
            return heap[0];
        }

        public void Pop()
        {
            if (count == 0)
            {
                throw new InvalidOperationException("Heap is empty");
            }

            heap[0] = heap[count - 1];
            count--;
            HeapifyDown(0);
        }

        private void HeapifyUp(int index)
        {
            while (index > 0)
            {
                int parentIndex = (index - 1) / 2;
                if (heap[parentIndex].freq > heap[index].freq)
                {
                    Swap(parentIndex, index);
                    index = parentIndex;
                }
                else
                {
                    break;
                }
            }
        }

        private void HeapifyDown(int index)
        {
            int smallest = index;
            int leftChild = 2 * index + 1;
            int rightChild = 2 * index + 2;

            if (leftChild < count && heap[leftChild].freq < heap[smallest].freq)
            {
                smallest = leftChild;
            }

            if (rightChild < count && heap[rightChild].freq < heap[smallest].freq)
            {
                smallest = rightChild;
            }

            if (smallest != index)
            {
                Swap(index, smallest);
                HeapifyDown(smallest);
            }
        }

        private void Swap(int index1, int index2)
        {
            node temp = heap[index1];
            heap[index1] = heap[index2];
            heap[index2] = temp;
        }
    }



















    /// <summary>
    /// Holds the pixel color in 3 byte values: red, green and blue
    /// </summary>
    public struct RGBPixel
    {
        public byte red, green, blue;
    }
    public struct RGBPixelD
    {
        public double red, green, blue;
    }



    /// <summary>
    /// Library of static functions that deal with images
    /// </summary>
    public class ImageOperations
    {

        /// <summary>
        /// Open an image and load it into 2D array of colors (size: Height x Width)
        /// </summary>
        /// <param name="ImagePath">Image file path</param>
        /// <returns>2D array of colors</returns>
        public static RGBPixel[,] OpenImage(string ImagePath)
        {
            Bitmap original_bm = new Bitmap(ImagePath);
            int Height = original_bm.Height;
            int Width = original_bm.Width;

            RGBPixel[,] Buffer = new RGBPixel[Height, Width];

            unsafe
            {
                BitmapData bmd = original_bm.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, original_bm.PixelFormat);
                int x, y;
                int nWidth = 0;
                bool Format32 = false;
                bool Format24 = false;
                bool Format8 = false;

                if (original_bm.PixelFormat == PixelFormat.Format24bppRgb)
                {
                    Format24 = true;
                    nWidth = Width * 3;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format32bppArgb || original_bm.PixelFormat == PixelFormat.Format32bppRgb || original_bm.PixelFormat == PixelFormat.Format32bppPArgb)
                {
                    Format32 = true;
                    nWidth = Width * 4;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format8bppIndexed)
                {
                    Format8 = true;
                    nWidth = Width;
                }
                int nOffset = bmd.Stride - nWidth;
                byte* p = (byte*)bmd.Scan0;
                for (y = 0; y < Height; y++)
                {
                    for (x = 0; x < Width; x++)
                    {
                        if (Format8)
                        {
                            Buffer[y, x].red = Buffer[y, x].green = Buffer[y, x].blue = p[0];
                            p++;
                        }
                        else
                        {
                            Buffer[y, x].red = p[2];
                            Buffer[y, x].green = p[1];
                            Buffer[y, x].blue = p[0];
                            if (Format24) p += 3;
                            else if (Format32) p += 4;
                        }
                    }
                    p += nOffset;
                }
                original_bm.UnlockBits(bmd);
            }

            return Buffer;
        }

        /// <summary>
        /// Get the height of the image 
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <returns>Image Height</returns>
        public static int GetHeight(RGBPixel[,] ImageMatrix)
        {
            return ImageMatrix.GetLength(0);
        }

        /// <summary>
        /// Get the width of the image 
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <returns>Image Width</returns>
        public static int GetWidth(RGBPixel[,] ImageMatrix)
        {
            return ImageMatrix.GetLength(1);
        }

        /// <summary>
        /// Display the given image on the given PictureBox object
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <param name="PicBox">PictureBox object to display the image on it</param>
        public static void DisplayImage(RGBPixel[,] ImageMatrix, PictureBox PicBox)
        {
            // Create Image:
            //==============
            int Height = ImageMatrix.GetLength(0);
            int Width = ImageMatrix.GetLength(1);

            Bitmap ImageBMP = new Bitmap(Width, Height, PixelFormat.Format24bppRgb);

            unsafe
            {
                BitmapData bmd = ImageBMP.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, ImageBMP.PixelFormat);
                int nWidth = 0;
                nWidth = Width * 3;
                int nOffset = bmd.Stride - nWidth;
                byte* p = (byte*)bmd.Scan0;
                for (int i = 0; i < Height; i++)
                {
                    for (int j = 0; j < Width; j++)
                    {
                        p[2] = ImageMatrix[i, j].red;
                        p[1] = ImageMatrix[i, j].green;
                        p[0] = ImageMatrix[i, j].blue;
                        p += 3;
                    }

                    p += nOffset;
                }
                ImageBMP.UnlockBits(bmd);
            }
            PicBox.Image = ImageBMP;
        }


        /// <summary>
        /// Apply Gaussian smoothing filter to enhance the edge detection 
        /// </summary>
        /// <param name="ImageMatrix">Colored image matrix</param>
        /// <param name="filterSize">Gaussian mask size</param>
        /// <param name="sigma">Gaussian sigma</param>
        /// <returns>smoothed color image</returns>
        public static RGBPixel[,] GaussianFilter1D(RGBPixel[,] ImageMatrix, int filterSize, double sigma)
        {
            int Height = GetHeight(ImageMatrix);
            int Width = GetWidth(ImageMatrix);

            RGBPixelD[,] VerFiltered = new RGBPixelD[Height, Width];
            RGBPixel[,] Filtered = new RGBPixel[Height, Width];


            // Create Filter in Spatial Domain:
            //=================================
            //make the filter ODD size
            if (filterSize % 2 == 0) filterSize++;

            double[] Filter = new double[filterSize];

            //Compute Filter in Spatial Domain :
            //==================================
            double Sum1 = 0;
            int HalfSize = filterSize / 2;
            for (int y = -HalfSize; y <= HalfSize; y++)
            {
                //Filter[y+HalfSize] = (1.0 / (Math.Sqrt(2 * 22.0/7.0) * Segma)) * Math.Exp(-(double)(y*y) / (double)(2 * Segma * Segma)) ;
                Filter[y + HalfSize] = Math.Exp(-(double)(y * y) / (double)(2 * sigma * sigma));
                Sum1 += Filter[y + HalfSize];
            }
            for (int y = -HalfSize; y <= HalfSize; y++)
            {
                Filter[y + HalfSize] /= Sum1;
            }

            //Filter Original Image Vertically:
            //=================================
            int ii, jj;
            RGBPixelD Sum;
            RGBPixel Item1;
            RGBPixelD Item2;

            for (int j = 0; j < Width; j++)
                for (int i = 0; i < Height; i++)
                {
                    Sum.red = 0;
                    Sum.green = 0;
                    Sum.blue = 0;
                    for (int y = -HalfSize; y <= HalfSize; y++)
                    {
                        ii = i + y;
                        if (ii >= 0 && ii < Height)
                        {
                            Item1 = ImageMatrix[ii, j];
                            Sum.red += Filter[y + HalfSize] * Item1.red;
                            Sum.green += Filter[y + HalfSize] * Item1.green;
                            Sum.blue += Filter[y + HalfSize] * Item1.blue;
                        }
                    }
                    VerFiltered[i, j] = Sum;
                }

            //Filter Resulting Image Horizontally:
            //===================================
            for (int i = 0; i < Height; i++)
                for (int j = 0; j < Width; j++)
                {
                    Sum.red = 0;
                    Sum.green = 0;
                    Sum.blue = 0;
                    for (int x = -HalfSize; x <= HalfSize; x++)
                    {
                        jj = j + x;
                        if (jj >= 0 && jj < Width)
                        {
                            Item2 = VerFiltered[i, jj];
                            Sum.red += Filter[x + HalfSize] * Item2.red;
                            Sum.green += Filter[x + HalfSize] * Item2.green;
                            Sum.blue += Filter[x + HalfSize] * Item2.blue;
                        }
                    }
                    Filtered[i, j].red = (byte)Sum.red;
                    Filtered[i, j].green = (byte)Sum.green;
                    Filtered[i, j].blue = (byte)Sum.blue;
                }

            return Filtered;
        }

        public static RGBPixel[,] Encryption(RGBPixel[,] ImageMatrix, string Is, int TapPosition)
        {
            
            int n = GetHeight(ImageMatrix);
            int m = GetWidth(ImageMatrix);
            StringBuilder InitialSeed = new StringBuilder(Is);
            StringBuilder temp = new StringBuilder();
            bool c = false;


            for (int i = 0; i < InitialSeed.Length; i++)
            {
                if (InitialSeed[i] == '1')
                {
                    c = true;
                }
                if (InitialSeed[i] != '0' && InitialSeed[i] != '1')
                {
                    int Ascci = (int)InitialSeed[i];

                    while (Ascci > 0)
                    {
                        if (Ascci % 2 == 0)
                        {
                            temp.Append('1');
                            c = true;
                        }
                        else
                        {
                            temp.Append('0');
                        }
                        Ascci /= 2;
                    }
                }
                else
                {
                    temp.Append(InitialSeed[i]);
                }
            }
            if (!c)
                return ImageMatrix;
            InitialSeed = temp;

            int tpindex = InitialSeed.Length - TapPosition - 1;
            int startindex = 0;
            int endindex = InitialSeed.Length;
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    int sumoxr = 0;

                    for (int l = 0; l < 8; l++)
                    {
                        char shiftedBit = InitialSeed[startindex];
                        startindex = (startindex + 1) % InitialSeed.Length;
                        int ClockOutPut = (byte)(shiftedBit - '0') ^ (byte)((InitialSeed[tpindex]) - '0');
                        tpindex = (tpindex + 1) % InitialSeed.Length;
                        sumoxr += ClockOutPut * (byte)Math.Pow(2, 7 - l);
                        endindex %= InitialSeed.Length;
                        if (ClockOutPut == 0)
                            InitialSeed[endindex] = '0';
                        else
                            InitialSeed[endindex] = '1';
                        endindex++;

                    }
                    ImageMatrix[i, j].red ^= (byte)sumoxr;

                    /////////////////////////////////////////////
                    sumoxr = 0;

                    for (int l = 0; l < 8; l++)
                    {
                        char shiftedBit = InitialSeed[startindex];
                        startindex = (startindex + 1) % InitialSeed.Length;
                        int ClockOutPut = (byte)(shiftedBit - '0') ^ (byte)((InitialSeed[tpindex]) - '0');
                        tpindex = (tpindex + 1) % InitialSeed.Length;
                        sumoxr += ClockOutPut * (byte)Math.Pow(2, 7 - l);
                        endindex %=  InitialSeed.Length;
                        if (ClockOutPut == 0)
                            InitialSeed[endindex] = '0';
                        else
                            InitialSeed[endindex] = '1';
                        endindex++;

                    }
                    ImageMatrix[i, j].green ^= (byte)sumoxr;

                    /////////////////////////////////////////////
                    sumoxr = 0;

                    for (int l = 0; l < 8; l++)
                    {
                        char shiftedBit = InitialSeed[startindex];
                        startindex = (startindex + 1) % InitialSeed.Length;
                        int ClockOutPut = (byte)(shiftedBit - '0') ^ (byte)((InitialSeed[tpindex]) - '0');
                        tpindex = (tpindex + 1) % InitialSeed.Length;
                        sumoxr += ClockOutPut * (byte)Math.Pow(2, 7 - l);
                        endindex %=  InitialSeed.Length;
                        if (ClockOutPut == 0)
                            InitialSeed[endindex] = '0';
                        else
                            InitialSeed[endindex] = '1';
                        endindex++;

                    }
                    ImageMatrix[i, j].blue ^= (byte)sumoxr;
                    /////////////////////////////////////////////  



                }
            }

            //  Console.WriteLine(tupleMap.Count);
            return ImageMatrix;
        }
        public static RGBPixel[,] Decryption(RGBPixel[,] ImageMatrix, string Is, int TapPosition)
        {
            int n = GetHeight(ImageMatrix);
            int m = GetWidth(ImageMatrix);
            StringBuilder InitialSeed = new StringBuilder(Is);
            StringBuilder temp = new StringBuilder();
            bool c = false;


            for (int i = 0; i < InitialSeed.Length; i++)
            {
                if (InitialSeed[i] == '1')
                {
                    c = true;
                }
                if (InitialSeed[i] != '0' && InitialSeed[i] != '1')
                {
                    int Ascci = (int)InitialSeed[i];

                    while (Ascci > 0)
                    {
                        if (Ascci % 2 == 0)
                        {
                            temp.Append('1');
                            c = true;
                        }
                        else
                        {
                            temp.Append('0');
                        }
                        Ascci /= 2;
                    }
                }
                else
                {
                    temp.Append(InitialSeed[i]);
                }
            }
            if (!c) {
            
                return (ImageMatrix);
            }
            InitialSeed = temp;

            int tpindex = InitialSeed.Length - TapPosition - 1;
            int startindex = 0;
            int endindex = InitialSeed.Length;
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    int sumoxr = 0;

                    for (int l = 0; l < 8; l++)
                    {
                        char shiftedBit = InitialSeed[startindex];
                        startindex = (startindex + 1) % InitialSeed.Length;
                        int ClockOutPut = (byte)(shiftedBit - '0') ^ (byte)((InitialSeed[tpindex]) - '0');
                        tpindex = (tpindex + 1) % InitialSeed.Length;
                        sumoxr += ClockOutPut * (byte)Math.Pow(2, 7 - l);
                        endindex %= InitialSeed.Length;
                        if (ClockOutPut == 0)
                            InitialSeed[endindex] = '0';
                        else
                            InitialSeed[endindex] = '1';
                        endindex++;

                    }
                    ImageMatrix[i, j].red ^= (byte)sumoxr;

                    /////////////////////////////////////////////
                    sumoxr = 0;

                    for (int l = 0; l < 8; l++)
                    {
                        char shiftedBit = InitialSeed[startindex];
                        startindex = (startindex + 1) % InitialSeed.Length;
                        int ClockOutPut = (byte)(shiftedBit - '0') ^ (byte)((InitialSeed[tpindex]) - '0');
                        tpindex = (tpindex + 1) % InitialSeed.Length;
                        sumoxr += ClockOutPut * (byte)Math.Pow(2, 7 - l);
                        endindex %= InitialSeed.Length;
                        if (ClockOutPut == 0)
                            InitialSeed[endindex] = '0';
                        else
                            InitialSeed[endindex] = '1';
                        endindex++;

                    }
                    ImageMatrix[i, j].green ^= (byte)sumoxr;

                    /////////////////////////////////////////////
                    sumoxr = 0;

                    for (int l = 0; l < 8; l++)
                    {
                        char shiftedBit = InitialSeed[startindex];
                        startindex = (startindex + 1) % InitialSeed.Length;
                        int ClockOutPut = (byte)(shiftedBit - '0') ^ (byte)((InitialSeed[tpindex]) - '0');
                        tpindex = (tpindex + 1) % InitialSeed.Length;
                        sumoxr += ClockOutPut * (byte)Math.Pow(2, 7 - l);
                        endindex %= InitialSeed.Length;
                        if (ClockOutPut == 0)
                            InitialSeed[endindex] = '0';
                        else
                            InitialSeed[endindex] = '1';
                        endindex++;

                    }
                    ImageMatrix[i, j].blue ^= (byte)sumoxr;
                    /////////////////////////////////////////////  



                }
            }

            return (ImageMatrix);
        }

    

        public static void DFS(node node, string value,string s ,ref Dictionary<int, string> R_huffman_output, ref Dictionary<int, string> G_huffman_output, ref Dictionary<int, string> B_huffman_output)
        {
            
            Stack<Tuple<node, string>> stack = new Stack<Tuple<node, string>>();
            stack.Push(Tuple.Create(node.right, "1"));
            stack.Push(Tuple.Create(node.left, "0"));

            while (stack.Count > 0)
            {
                Tuple<node, string> tmp = stack.Peek();
                stack.Pop();

                node = tmp.Item1;
                if (node.value != -1)
                {
                    if (s == "RED") R_huffman_output[node.value] = tmp.Item2;
                    else if (s == "GREEN") G_huffman_output[node.value] = tmp.Item2;
                    else B_huffman_output[node.value] = tmp.Item2;

                    continue;
                }

                if (node.right != null) stack.Push(Tuple.Create(node.right, tmp.Item2 + '1'));
                if (node.left != null) stack.Push(Tuple.Create(node.left, tmp.Item2 + '0'));
            }

        }

        public static void nodesBuild(Dictionary<int, int> freq, string color, ref Dictionary<int, string> R_huffman_output, ref Dictionary<int, string> G_huffman_output, ref Dictionary<int, string> B_huffman_output)
        {
            Heap heap = new Heap();
            foreach (int i in freq.Keys)
            {
                node nw_node = new node(i, freq[i]);
                heap.Insert(nw_node);
            }
            while (heap.Count > 1)
            {
                node min_1 = new node(0, 0);
                node min_2 = new node(0, 0);
                min_1 = heap.Top();
                heap.Pop();
                min_2 = heap.Top();
                heap.Pop();
                node new_node = new node(-1, min_1.freq + min_2.freq);
                new_node.left = min_1;
                new_node.right = min_2;
                heap.Insert(new_node);
            }
            if (color == "RED") DFS(heap.Top(), "", "RED", ref R_huffman_output, ref G_huffman_output, ref B_huffman_output);
            else if (color == "GREEN") DFS(heap.Top(), "", "GREEN", ref R_huffman_output, ref G_huffman_output, ref B_huffman_output);
            else DFS(heap.Top(), "", "BLUE", ref R_huffman_output, ref G_huffman_output, ref B_huffman_output);
        }
        public static void writeTreeOutput(BinaryWriter BW, Dictionary<int, int> freq, RGBPixel[,] ImageMatrix, string color, int n, int m, ref Dictionary<int, string> R_huffman_output, ref Dictionary<int, string> G_huffman_output, ref Dictionary<int, string> B_huffman_output)
        {

            string str = n.ToString();
            for (int i = 0; i < str.Length; i++)
            {
                int x = str[i];
                BW.Write((byte)x);
            }
            BW.Write('-');
            str = m.ToString();
            for (int i = 0; i < str.Length; i++)
            {
                int x = str[i];
                BW.Write((byte)x);
            }
            BW.Write('-');
            string distinctColors = freq.Count.ToString();
            for (int i = 0; i < distinctColors.Length; i++)
            {
                int x = distinctColors[i];
                BW.Write((byte)x);
            }
            BW.Write('-');
            foreach (int i in freq.Keys)
            {
                //convert number to binary and put it in string ;
                string binaryString = Convert.ToString(i, 2);
                byte colorValue = Convert.ToByte(binaryString, 2);
                int bitssz;
                BW.Write(colorValue);
                if (color == "RED") bitssz = R_huffman_output[i].Length;
                else if (color == "GREEN") bitssz = G_huffman_output[i].Length;
                else bitssz = B_huffman_output[i].Length;
                str = bitssz.ToString();
                for (int j = 0; j < str.Length; j++)
                {
                    int x = str[j];
                    BW.Write((byte)x);
                }
                BW.Write('-');
                byte compValue;
                if (color == "RED")
                {
                    string s = "";
                    for (int j = 0; j < R_huffman_output[i].Length; j++)
                    {
                        s += R_huffman_output[i][j];
                        if (s.Length == 8)
                        {
                            compValue = Convert.ToByte(s, 2);
                            BW.Write(compValue);
                            s = "";
                        }
                    }
                    if (s.Length != 0)
                    {
                        while (s.Length < 8) s += '0';
                        compValue = Convert.ToByte(s, 2);
                        BW.Write(compValue);
                    }
                }
                else if (color == "GREEN")
                {
                    string s = "";
                    for (int j = 0; j < G_huffman_output[i].Length; j++)
                    {
                        s += G_huffman_output[i][j];
                        if (s.Length == 8)
                        {
                            compValue = Convert.ToByte(s, 2);
                            BW.Write(compValue);
                            s = "";
                        }
                    }
                    if (s.Length != 0)
                    {
                        while (s.Length < 8) s += '0';
                        compValue = Convert.ToByte(s, 2);
                        BW.Write(compValue);
                    }
                }
                else
                {
                    string s = "";
                    for (int j = 0; j < B_huffman_output[i].Length; j++)
                    {
                        s += B_huffman_output[i][j];
                        if (s.Length == 8)
                        {
                            compValue = Convert.ToByte(s, 2);
                            BW.Write(compValue);
                            s = "";
                        }
                    }
                    if (s.Length != 0)
                    {
                        while (s.Length < 8) s += '0';
                        compValue = Convert.ToByte(s, 2);
                        BW.Write(compValue);
                    }
                }
            }
            int Bits = 0;
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    if (color == "RED") Bits += R_huffman_output[ImageMatrix[i, j].red].Length;
                    else if (color == "GREEN") Bits += G_huffman_output[ImageMatrix[i, j].green].Length;
                    else Bits += B_huffman_output[ImageMatrix[i, j].blue].Length;
                }
            }
            str = Bits.ToString();
            for (int i = 0; i < str.Length; i++)
            {
                int x = str[i];
                BW.Write((byte)x);
            }
            BW.Write('-');
            string temp = "";
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    if (color == "RED")
                    {
                        for (int k = 0; k < R_huffman_output[ImageMatrix[i, j].red].Length; k++)
                        {
                            temp += R_huffman_output[ImageMatrix[i, j].red][k];
                            if (temp.Length == 8)
                            {
                                byte byteValue = Convert.ToByte(temp, 2);
                                BW.Write(byteValue);
                                temp = "";
                            }

                        }
                    }
                    else if (color == "GREEN")
                    {
                        for (int k = 0; k < G_huffman_output[ImageMatrix[i, j].green].Length; k++)
                        {
                            temp += G_huffman_output[ImageMatrix[i, j].green][k];
                            if (temp.Length == 8)
                            {
                                byte byteValue = Convert.ToByte(temp, 2);

                                BW.Write(byteValue);
                                temp = "";
                            }

                        }
                    }
                    else
                    {
                        for (int k = 0; k < B_huffman_output[ImageMatrix[i, j].blue].Length; k++)
                        {
                            temp += B_huffman_output[ImageMatrix[i, j].blue][k];
                            if (temp.Length == 8)
                            {
                                byte byteValue = Convert.ToByte(temp, 2);

                                BW.Write(byteValue);
                                temp = "";
                            }

                        }
                    }
                }
            }
            if (temp.Length != 0)
            {
                while (temp.Length < 8) temp += '0';
                byte byteValue = Convert.ToByte(temp, 2);
                BW.Write(byteValue);
            }
        }
        //The COMPRESSION Process
        public static int compress(RGBPixel[,] ImageMatrix, string InitialSeed, int TapPosition)
        {
            int n = GetHeight(ImageMatrix);
            int m = GetWidth(ImageMatrix);
            Dictionary<int, int> freq_red = new Dictionary<int, int>();
            Dictionary<int, int> freq_green = new Dictionary<int, int>();
            Dictionary<int, int> freq_blue = new Dictionary<int, int>();
            Dictionary<int, string> R_huffman_output = new Dictionary<int, string>();
            Dictionary<int, string> G_huffman_output = new Dictionary<int, string>();
            Dictionary<int, string> B_huffman_output = new Dictionary<int, string>();

            //[1]Calculate Frequancy
            int count = 0, val;
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    //RED
                    if (!freq_red.ContainsKey(ImageMatrix[i, j].red)) freq_red.Add(ImageMatrix[i, j].red, 1);
                    else freq_red[ImageMatrix[i, j].red]++;

                    val = ImageMatrix[i, j].red;
                    while (val != 0)
                    {
                        count++;
                        val &= (val - 1);
                    }
                    //GREEN
                    if (!freq_green.ContainsKey(ImageMatrix[i, j].green)) freq_green.Add(ImageMatrix[i, j].green, 1);
                    else freq_green[ImageMatrix[i, j].green]++;
                    val = ImageMatrix[i, j].green;
                    while (val != 0)
                    {
                        count++;
                        val &= (val - 1);
                    }
                    //BLUE
                    if (!freq_blue.ContainsKey(ImageMatrix[i, j].blue)) freq_blue.Add(ImageMatrix[i, j].blue, 1);
                    else freq_blue[ImageMatrix[i, j].blue]++;
                    val = ImageMatrix[i, j].blue;
                    while (val != 0)
                    {
                        count++;
                        val &= (val - 1);
                    }
                }
            }
            //[2]Build the hoffman tree for RED , GREEN , BLUE channels. 

            nodesBuild(freq_red, "RED", ref R_huffman_output, ref G_huffman_output, ref B_huffman_output);
            nodesBuild(freq_green, "GREEN", ref R_huffman_output, ref G_huffman_output, ref B_huffman_output);
            nodesBuild(freq_blue, "BLUE", ref R_huffman_output, ref G_huffman_output, ref B_huffman_output);

            //[3]write the tree output in file
            Encoding encoding = Encoding.UTF8;
            using (BinaryWriter BW = new BinaryWriter(File.Open("C:\\fos_cygwin\\FOS_CODES\\FOS_PROJECT_2023_TEMPLATE\\Image-Encryption-and-Compression\\huff_out.bin", FileMode.Create), encoding))
            {
                string str = InitialSeed;
                for (int j = 0; j < str.Length; j++)
                {
                    int x = str[j];
                    BW.Write((byte)x);
                }
                BW.Write('-');
                str = TapPosition.ToString();
                for (int j = 0; j < str.Length; j++)
                {
                    int x = str[j];
                    BW.Write((byte)x);
                }
                BW.Write('-');
                writeTreeOutput(BW, freq_red, ImageMatrix, "RED", n, m, ref R_huffman_output, ref G_huffman_output, ref B_huffman_output);
                writeTreeOutput(BW, freq_green, ImageMatrix, "GREEN", n, m, ref R_huffman_output, ref G_huffman_output, ref B_huffman_output);
                writeTreeOutput(BW, freq_blue, ImageMatrix, "BLUE", n, m, ref R_huffman_output, ref G_huffman_output, ref B_huffman_output);
            }
            int count2 = 0;
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    val = ImageMatrix[i, j].red;
                    count2 += R_huffman_output[val].Length;
                    val = ImageMatrix[i, j].green;
                    count2 += G_huffman_output[val].Length;
                    val = ImageMatrix[i, j].blue;
                    count2 += B_huffman_output[val].Length;
                }
            }
            return count2;
        }
        //public static Dictionary<string, int> de_r_haffman = new Dictionary<string, int>();
        //public static Dictionary<string, int> de_g_haffman = new Dictionary<string, int>();
        //public static Dictionary<string, int> de_b_haffman = new Dictionary<string, int>();
        public static RGBPixel[,] readfile(BinaryReader br, string color)
        {
            Dictionary<string, int> de_r_haffman = new Dictionary<string, int>();
            Dictionary<string, int> de_g_haffman = new Dictionary<string, int>();
            Dictionary<string, int> de_b_haffman = new Dictionary<string, int>();
            string str = "";
            byte byteValue;
            byteValue = br.ReadByte();
            while ((char)byteValue != '-')
            {
                str += (char)byteValue;
                byteValue = br.ReadByte();
            }
            int n = int.Parse(str);
            str = "";
            byteValue = br.ReadByte();
            while ((char)byteValue != '-')
            {
                str += (char)byteValue;
                byteValue = br.ReadByte();
            }
            int m = int.Parse(str);
            str = "";
            byteValue = br.ReadByte();
            while ((char)byteValue != '-')
            {
                str += (char)byteValue;
                byteValue = br.ReadByte();
            }
            int distinctColors = int.Parse(str);
            int bitsz = 0, bytesz = 0;
            while (distinctColors > 0)
            {
                byteValue = br.ReadByte();
                int integer_value = byteValue;
                str = "";
                byteValue = br.ReadByte();
                while ((char)byteValue != '-')
                {
                    str += (char)byteValue;
                    byteValue = br.ReadByte();
                }
                bitsz = int.Parse(str);
                bytesz = (bitsz + 7) / 8;
                str = "";
                while (bytesz > 0)
                {
                    byteValue = br.ReadByte();
                    string s = Convert.ToString(byteValue, 2).PadLeft(8, '0');
                    for (int i = 0; i < s.Length; i++)
                    {
                        str += s[i];
                        bitsz--;
                        if (bitsz == 0) break;
                    }
                    bytesz--;
                }
                if (color == "RED") de_r_haffman.Add(str, integer_value);
                else if (color == "GREEN") de_g_haffman.Add(str, integer_value);
                else de_b_haffman.Add(str, integer_value);
                distinctColors--;
            }
            str = "";
            byteValue = br.ReadByte();
            while ((char)byteValue != '-')
            {
                str += (char)byteValue;
                byteValue = br.ReadByte();
            }
            bitsz = int.Parse(str);
            bytesz = (bitsz + 7) / 8;
            str = "";
            int x = 0, y = 0;
            RGBPixel[,] deImageMatrix = new RGBPixel[n, m];
            while (bytesz > 0)
            {
                byteValue = br.ReadByte();
                string s = Convert.ToString(byteValue, 2).PadLeft(8, '0');
                for (int i = 0; i < s.Length; i++)
                {
                    str += s[i];
                    if (color == "RED")
                    {
                        if (de_r_haffman.ContainsKey(str))
                        {
                            deImageMatrix[x, y].red = (byte)de_r_haffman[str];
                            y++;
                            if (y == m)
                            {
                                y = 0;
                                x++;
                            }
                            str = "";
                        }
                    }
                    else if (color == "GREEN")
                    {
                        if (de_g_haffman.ContainsKey(str))
                        {
                            deImageMatrix[x, y].green = (byte)de_g_haffman[str];
                            y++;
                            if (y == m)
                            {
                                y = 0;
                                x++;
                            }
                            str = "";
                        }
                    }
                    else
                    {
                        if (de_b_haffman.ContainsKey(str))
                        {
                            deImageMatrix[x, y].blue = (byte)de_b_haffman[str];
                            y++;
                            if (y == m)
                            {
                                y = 0;
                                x++;
                            }
                            str = "";
                        }
                    }
                }
                bytesz--;
            }

            return deImageMatrix;
        }
        public static (RGBPixel[,], string, int) decompress(string filepath)
        {

           

            using (BinaryReader br = new BinaryReader(File.Open(filepath, FileMode.Open)))
            {

                string str = "";
                byte byteValue = br.ReadByte();
                while ((char)byteValue != '-')
                {
                    str += (char)byteValue;
                    byteValue = br.ReadByte();
                }
                string InitialSeed = str;
                str = "";
                byteValue = br.ReadByte();
                while ((char)byteValue != '-')
                {
                    str += (char)byteValue;
                    byteValue = br.ReadByte();
                }
                int TapPosition = int.Parse(str); ;
                RGBPixel[,] r = readfile(br, "RED");

                int n = r.GetLength(0);
                int m = r.GetLength(1);

                RGBPixel[,] g = readfile(br, "GREEN");
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < m; j++)
                    {
                        r[i, j].green = g[i, j].green;
                    }
                }
                RGBPixel[,] b = readfile(br, "BLUE");
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < m; j++)
                    {
                        r[i, j].blue = b[i, j].blue;
                    }
                }


                return (r, InitialSeed, TapPosition);
                br.Close();
            }


           
        }

    }

}