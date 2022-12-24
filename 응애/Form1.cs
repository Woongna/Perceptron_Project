using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace 응애
{
    public partial class Form1 : Form
    {
        int flag = 3;     //학습데이터 개수
        Image img;
        Color color;
        const int CLUSTER = 4;  //클러 개수
        const int COORD = 5;    //속성 개수
        public static List<List<List<double>>> imgList = new List<List<List<double>>>();    //학습이미지
        public static List<List<double>> tempList = new List<List<double>>();               
        public static List<List<List<double>>> testList = new List<List<List<double>>>();   //테스트이미지
        public static List<List<double>> testtempList = new List<List<double>>();
        Perceptron per = new Perceptron();
        public static List<List<double>> tempdata;
        public static List<List<double>> trainedw = new List<List<double>>();
        public static List<double> trainedb = new List<double>();
        public Form1()
        {
            
            SinglePerceptron sp;
            InitializeComponent();
            this.Size = new Size(950, 610);
            //위치설정
            #region
            pictureBox1.Location = new Point(20, 20);
            pictureBox1.Size = new Size(256, 256);
            pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;

            pictureBox2.Location = new Point(296, 20);
            pictureBox2.Size = new Size(256, 256);
            pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            pictureBox2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;

            pictureBox3.Location = new Point(572, 20);
            pictureBox3.Size = new Size(256, 256);
            pictureBox3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            pictureBox3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;

            pictureBox4.Location = new Point(20, 296);
            pictureBox4.Size = new Size(256, 256);
            pictureBox4.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            pictureBox4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;

            button1.Location = new Point(300, 296);
            button1.Size = new Size(525, 256);
            #endregion
            
            for (int i = 0; i < flag; i++)
            {
                openFileDialog1.Title = "영상 파일 열기";
                openFileDialog1.Filter = "All Files(*.*) |*.*|Bitmap File(*.bmp) |*.bmp |Jpeg File(*.jpg) | *.jpg";
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    string strFilename = openFileDialog1.FileName;
                    img = Image.FromFile(strFilename);

                    Bitmap newbmp = new Bitmap(img);
                    int width = newbmp.Width;
                    int height = newbmp.Height;
                    imgList.Add(new List<List<double>>());

                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)     //각 학습데이터별 속성값 (x, y, r, g, b ) 
                        {
                            color = newbmp.GetPixel(x, y);
                            imgList[i].Add(new List<double>());
                            imgList[i][y * width + x].Add(x);
                            imgList[i][y * width + x].Add(y);
                            imgList[i][y * width + x].Add(color.R);
                            imgList[i][y * width + x].Add(color.G);
                            imgList[i][y * width + x].Add(color.B);

                            /* Console.WriteLine("=====================");
                             for (int j = 0; j < 5; j++)
                             {
                                 Console.WriteLine(imgList[i][y * width + x][j]);
                             }
                             Console.WriteLine("=====================");*/
                        }
                    }
                    //초기값뿌리기
                    FCM fcm = new FCM(width, height);
                    //fcm.getdata(i, width, height); 
                    fcm.startFCM(fcm.getdata(i, width, height));    //정규화된x데이터 배열
                    fcm.print();
                    //클러스터별 색칠하기
                    double[,] imgArray = BitmapToByteArray2D(newbmp);
                    imgArray = fcm.paintcluster();
                    switch (i) //학습영상개수
                    {
                        case 0:
                            pictureBox1.Image = byteArray2DToBitmap(imgArray, width, height);
                            break;
                        case 1:
                            pictureBox2.Image = byteArray2DToBitmap(imgArray, width, height);
                            break;
                        case 2:
                            pictureBox3.Image = byteArray2DToBitmap(imgArray, width, height);
                            break;
                    }
                    tempList = fcm.insertpattern();
                }
                //영상별 패턴값 저장
                tempdata = new List<List<double>>(per.getdata(CLUSTER, i, tempList));
                
            }
            sp = new SinglePerceptron(tempdata, flag, per.target);
            //퍼셉트론 실행
            sp.run();
            for (int wicount = 0; wicount < 20; wicount++)
            {
                trainedw.Add(new List<double>());
                for (int wocount = 0; wocount < 3; wocount++)
                {
                    trainedw[wicount].Add(sp.weight[wicount, wocount]);
                }
            }
            for (int wocount = 0; wocount < 3; wocount++)
            {
                trainedb.Add(sp.bias[wocount]);
            }
            //per.run();
        }
        public double[,] BitmapToByteArray2D(Bitmap bmp)
        {
            double[,] bmpArray = new double[bmp.Width * bmp.Height, 3];
            for (int x = 0; x < bmp.Width; x++)
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    Color color = bmp.GetPixel(x, y);
                    bmpArray[y * bmp.Width + x, 0] = color.R;
                    bmpArray[y * bmp.Width + x, 1] = color.G;
                    bmpArray[y * bmp.Width + x, 2] = color.B;
                }
            }
            return bmpArray;
        }
        public Bitmap byteArray2DToBitmap(double[,] byteArray, int width, int height)
        {
            Bitmap newbmp = new Bitmap(width, height);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Color newColor = Color.FromArgb((int)byteArray[y * width + x, 0], (int)byteArray[y * width + x, 1], (int)byteArray[y * width + x, 2]);
                    newbmp.SetPixel(x, y, newColor);
                }
            }
            return newbmp;
        }
        //테스트데이터 넣기
        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = "영상 파일 열기";
            openFileDialog1.Filter = "All Files(*.*) |*.*|Bitmap File(*.bmp) |*.bmp |Jpeg File(*.jpg) | *.jpg";
            if (openFileDialog2.ShowDialog() == DialogResult.OK)
            {
                string open = openFileDialog2.FileName;
                img = Image.FromFile(open);

                Bitmap newbmp = new Bitmap(img);
                int testwidth = newbmp.Width;
                int testheight = newbmp.Height;
                testList.Add(new List<List<double>>());//[width]

                for (int y = 0; y < testheight; y++)
                {
                    for (int x = 0; x < testwidth; x++)
                    {
                        color = newbmp.GetPixel(x, y);
                        testList[0].Add(new List<double>());
                        testList[0][y * testwidth + x].Add(x);
                        testList[0][y * testwidth + x].Add(y);
                        testList[0][y * testwidth + x].Add(color.R);
                        testList[0][y * testwidth + x].Add(color.G);
                        testList[0][y * testwidth + x].Add(color.B);
                    }
                }
                //초기값뿌리기
                FCM fcm = new FCM(testwidth, testheight);
                //fcm.getdata(i, width, height); 
                fcm.startFCM(fcm.getdata(0, testwidth, testheight, testList));  //정규화된 x데이터리스트
                fcm.print();
                //Console.WriteLine("\n======색칠하기======");

                double[,] imgArray = BitmapToByteArray2D(newbmp);
                imgArray = fcm.paintcluster();
                pictureBox4.Image = byteArray2DToBitmap(imgArray, testwidth, testheight);       //테스트데이터 fcm 결과이미지
                testtempList = fcm.insertpattern();
                Perceptron testper = new Perceptron();
                SinglePerceptron testsp = new SinglePerceptron(testper.getdata(CLUSTER, 0, 0, testtempList)[0]);
                testsp.Recognition(3, 20, trainedw, trainedb);
                
                //testper.getdata(CLUSTER, 0, 0, testtempList);
                //testper.run2();
               
            }
        }
    }
}
