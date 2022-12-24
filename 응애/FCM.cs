using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace 응애
{
    class FCM
    {
        int width;
        int height;
        //int DATA = width*height;
        const int CLUSTER = 4;
        const int COORD = 5;
        const int w_para = 2;   //퍼지상수
        const double Error = 0.01; //임계값

        List<List<double>> U = new List<List<double>>();        //소속행렬 [CLUSTER, DATA]
        List<List<double>> U_old = new List<List<double>>();    //이전 소속행렬 [CLUSTER, DATA]
        List<List<double>> d = new List<List<double>>();        //데이터-중심벡터 거리배열 [CLUSTER, DATA]
        List<List<double>> temp_e = new List<List<double>>();   //(종료조건) 오차값계산배열 [CLUSTER, DATA]
        List<List<double>> v = new List<List<double>>();        //중심벡터 배열 [CLUSTER, COORD]
        List<List<double>> v2 = new List<List<double>>();       //중심벡터 배열 [CLUSTER, COORD]

        public List<List<double>> x = new List<List<double>>(); //데이터 배열 선언[DATA, COORD]
        List<List<double>> RAnd_U = new List<List<double>>();   //[DATA, CLUSTER]
        List<List<double>> Temp_U = new List<List<double>>();   //[DATA, CLUSTER]
        List<double> Init_Sum = new List<double>();             //[DATA]

        //초기화 부분
        public FCM(int w, int h) //생성자
        {
            width = w;
            height = h;

            for (int i = 0; i < CLUSTER; i++)
            {
                U.Add(new List<double>());
                U_old.Add(new List<double>());
                d.Add(new List<double>());
                temp_e.Add(new List<double>());
                v.Add(new List<double>());
                for (int j = 0; j < width * height; j++)
                {
                    U[i].Add(0);
                    U_old[i].Add(0);
                    d[i].Add(0);
                    temp_e[i].Add(0);
                }
                for (int j = 0; j < COORD; j++)
                {
                    v[i].Add(0);
                    v[i].Add(0);
                }
            }
            for (int i = 0; i < width * height; i++)
            {
                RAnd_U.Add(new List<double>());
                Temp_U.Add(new List<double>());
                Init_Sum.Add(0);
                for (int j = 0; j < CLUSTER; j++)
                {
                    RAnd_U[i].Add(0);
                    Temp_U[i].Add(0);
                }
            }
        }

        int i, j, k, iter;
        double num, den, t_num, t_den;
        double temp1_dist, temp2_dist; //거리계산용
        double max_error;   //최대 오차율
        double sum, NewValue;   //
        int count, sing;    //종료조건
        
        Random rr = new Random();
        ////////////////////////////

        
        //학습데이터 받아오기
        public List<List<double>> getdata(int flag, int width, int height)
        {
            double xmax, xmin;
            
            //데이터
            for (int i = 0; i < width * height; i++)
            {
                x.Add(new List<double>());
                for (int j = 0; j < COORD; j++)
                {
                    x[i].Add(Form1.imgList[flag][i][j]);

                    //Console.WriteLine(x[i][j]);
                }
            }
            //x[data,coord]
            //정규화
            //속성별 최대값, 최소값 계산
            for (i = 0; i < COORD; i++)
            {
                if (i == 0)
                {
                    xmin = 0;
                    xmax = width;
                    Console.Write("x최대값 : " + xmax);
                }
                else if (i == 1)
                {
                    xmin = 0;
                    xmax = height;
                    Console.Write("y최대값 : " + xmax);
                }
                else
                {
                    xmin = 9999999; xmax = 0;
                    for (int j = 0; j < width * height; j++)
                    {
                        xmin = Math.Min(x[j][i], xmin);
                        xmax = Math.Max(x[j][i], xmax);
                        //Console.WriteLine(i + "번째 속성" + j + "번째 데이터 // " + xmin + " ~ " + xmax);
                    }
                }
                
                //정규화부분
                for (int k = 0; k < width * height; k++) //0~1사이로 정규화
                {               
                    //시작점을 0으로~ 
                    if (xmax - xmin != 0)
                    {
                        x[k][i] = (x[k][i] - xmin) * 1 / (xmax - xmin);
                    }
                    else
                        x[k][i] = 0;
                }

            }
         
           /* for (int k = 0; k < width * height; k++) //0~1사이로 정규화
            {
                Console.Write(k + "번째 데이터 : ");
                for (i = 0; i < COORD; i++)
                {
                    Console.Write(x[k][i] + " / ");
                }
                Console.WriteLine();
            }*/

            return x;
        }

        ///////////////////////
        //테스트데이터 받아오기
        public List<List<double>> getdata(int flag, int width, int height, List<List<List<double>>> testimgList)
        {
            double xmax, xmin;

            //데이터
            for (int i = 0; i < width * height; i++)
            {
                x.Add(new List<double>());
                for (int j = 0; j < COORD; j++)
                {
                    x[i].Add(testimgList[flag][i][j]);

                    //Console.WriteLine(x[i][j]);
                }
            }
            //x[data,coord]
            //정규화
            //속성별 최대값, 최소값 계산
            for (i = 0; i < COORD; i++)
            {
                if (i == 0)
                {
                    xmin = 0;
                    xmax = width;
                    Console.Write("x 최대값 : " + xmax);
                }
                else if (i == 1)
                {
                    xmin = 0;
                    xmax = height;
                    Console.Write("y 최대값 : " + xmax);
                }
                else
                {
                    xmin = double.MaxValue; xmax = double.MinValue;
                    for (int j = 0; j < width * height; j++)
                    {
                        xmin = Math.Min(x[j][i], xmin);
                        xmax = Math.Max(x[j][i], xmax);
                        //Console.WriteLine(i + "번째 속성" + j + "번째 데이터 // " + xmin + " ~ " + xmax);
                    }
                }

                //정규화부분
                for (int k = 0; k < width * height; k++) //0~1사이로 정규화
                {
                    //시작점을 0으로~ 
                    if (xmax - xmin != 0)
                    {
                        x[k][i] = (x[k][i] - xmin) * 1 / (xmax - xmin);
                    }
                    else
                        x[k][i] = 0;
                }

            }
            return x;
        }
        //////////////////////

        //FCM실행
        public void startFCM(List<List<double>> changeX)
        {
            
            v_matrix(); //초기 중심벡터 뿌리기
            do
            {
                x = changeX;
                cal_distance(); //데이터-중심벡터 거리계산
                cal_update();   //소속행렬 계산
                cal_vector();   //중심벡터 계산
                cal_error();    //에러계산
            } while (count != 0);
            paintcluster();
        }
        // Initialize Make V-matrix 클러스터 중심벡터 초기값설정
        public void v_matrix()  //영상에 총 4개 중심벡터 균일하게 뚜따뚜따
        {
            v[0][0] = 1.0 / 3;
            v[0][1] = 1.0 / 3;
            v[0][2] = 1.0 / 5;
            v[0][3] = 1.0 / 5;
            v[0][4] = 1.0 / 5;
                       
            v[1][0] = 1.0 * 2 / 3;
            v[1][1] = 1.0 / 3;
            v[1][2] = 1.0 * 2 / 5;
            v[1][3] = 1.0 * 2 / 5;
            v[1][4] = 1.0 * 2 / 5;
                       
            v[2][0] = 1.0 / 3;
            v[2][1] = 1.0 * 2 / 3;
            v[2][2] = 1.0 * 3 / 5;
            v[2][3] = 1.0 * 3 / 5;
            v[2][4] = 1.0 * 3 / 5;
                       
            v[3][0] = 1.0 * 2 / 3;
            v[3][1] = 1.0 * 2 / 3;
            v[3][2] = 1.0 * 4 / 5;
            v[3][3] = 1.0 * 4 / 5;
            v[3][4] = 1.0 * 4 / 5;

            Console.WriteLine("초기 중심벡터");
            for (i = 0; i < CLUSTER; i++)
            {
                for (j = 0; j < COORD; j++)
                {
                    Console.Write(v[i][j]);
                }
                Console.Write("\n");
            }
            Console.Write("\n");
            
            iter = 0;
        }
        //Calculate distance between data and cluster center
        //데이터-중심벡터 거리 계산
        public void cal_distance()
        {
            for (i = 0; i < CLUSTER; i++)
            {
                for (j = 0; j < width * height; j++)
                {
                    temp2_dist = 0;
                    for (k = 0; k < COORD; k++)
                    {
                        temp1_dist = Math.Pow((x[j][k] - v[i][k]), 2);
                        temp2_dist += temp1_dist;
                    }
                    d[i][j] = Math.Sqrt(temp2_dist);
                    //Console.Write("d= " + d[i][j] + "\n");
                }
            }
            /*
            for (i = 0; i < CLUSTER; i++)
            {
                for (j = 0; j < width*height; j++)
                {
                    Console.Write("d= " + d[i][j] + "\n");
                }
            }
            Console.Write("\n");
            */
        }
        //소속행렬계산
        public void cal_update()    
        {
            for (k = 0; k < width * height; k++)
            {
                for (i = 0; i < CLUSTER; i++)
                {
                    if (d[i][k] != 0)
                    {
                        for (j = 0, sum = 0; j < COORD; j++)
                        {
                            for (int h = 0; h < CLUSTER; h++)
                            {
                                if (i == h) sum += 1.0;
                                else if (d[h][k] == 0)  //분모 0 예외처리
                                {
                                    U[i][k] = 0.0;
                                    break;
                                }
                                else sum += Math.Pow(d[i][k] / d[h][k], 2 / (w_para - 1));

                            }
                        }
                        NewValue = 1.0 / sum;
                        U[i][k] = NewValue;
                        //onsole.Write("U" + "[" + i + "]" + "[" + k + "] = " + NewValue);
                        //Console.Write("\n");
                    }
                    else    //클러스터 중심겹쳤을 경우
                    {
                        for (j = 0, sing = 1; j < i; j++) U[j][k] = 0.0;
                        for (j = i + 1, sing = 1; j < CLUSTER; j++)
                            if (d[j][k] == 0) sing++;
                        U[i][k] = (double)1.0 / (double)sing;
                        //Console.Write(" U[" + i + "][" + k + "] = " + U[i][k]);
                        //Console.WriteLine("클러스터중심"+v[i][j]);
                        for (j = i + 1; j < CLUSTER; j++)
                        {
                            if (d[j][k] == 0) U[i][k] = 1.0 / sing;
                            else U[i][k] = 0.0;
                        }
                        break;
                    }
                }
                //break;
            }
            //Console.Write("\n");
        }

        //Calculate the center v
        //중심벡터 계산
        public void cal_vector()
        {
            for (i = 0; i < CLUSTER; i++)
            {
                
                for (j = 0; j < COORD; j++)
                {
                    num = 0;
                    den = 0;
                    for (k = 0; k < width * height; k++)  //즁심벡터 업데이트
                    {
                        t_num = 0;
                        t_den = 0;
                        t_num = Math.Pow(U[i][k], w_para) * x[k][j];
                        t_den = Math.Pow(U[i][k], w_para);
                        //Console.Write("U[i][k] value = " + U[i][k]);
                        //Console.Write("\n t_num =" + t_num + " t_den =" + t_den);
                        num += t_num;
                        den += t_den;
                    }
                    v[i][j] = num / den;
                    //Console.Write("\n");
                }
                //Console.Write("\n");
            }
            /*Console.Write("\n");
            for (i = 0; i < CLUSTER; i++)
            {
                for (j = 0; j < COORD; j++)
                {
                    Console.Write("v[i][j] 's value = "+v[i][j] + " ");
                }
                Console.Write("\n");
            }
            Console.Write("\n");*/
        }

        public void cal_error() //오차율계산
        {
            max_error = 0.0;
            for (i = 0; i < CLUSTER; i++)
            {
                for (k = 0; k < width * height; k++)
                {
                    temp_e[i][k] = Math.Abs(U[i][k] - U_old[i][k]);
                    max_error = Math.Max(max_error, temp_e[i][k]);
                }
            }
            for (i = 0; i < CLUSTER; i++)
            {
                for (k = 0; k < width * height; k++)
                {
                    U_old[i][k] = U[i][k];
                }
            }
            //Console.Write("\n error = " + max_error);
            count = 0;
            if (max_error > Error) count = count + 1;
            iter++;
        }
        public void print()
        {
            Console.Write("\n\n*****final CLuster Center *****\n");
            for (i = 0; i < CLUSTER; i++)
            {
                for (j = 0; j < COORD; j++)
                {
                    Console.Write(" v[" + i + "][" + j + "] = " + v[i][j]);
                }
                Console.Write("\n");
            }
            Console.Write("\n");
            Console.Write("반복횟수 = " + iter + " 최대오차율[error] =" + max_error);
            
        }
        public double[,] paintcluster()
        {
            double[,] imgArray = new double[width*height, 3];

            for (int i = 0; i < width * height; i++)
            {
                double maxValue = 0;
                int maxclusternum = 0;
                for (int j = 0; j < CLUSTER; j++)   //imgArray[width*height, 3]
                {
                    if(U[j][i] > maxValue)
                    {
                        maxValue = U[j][i];
                        maxclusternum = j;
                    }
                }

                imgArray[i, 0] = v[maxclusternum][2] * 255;
                imgArray[i, 1] = v[maxclusternum][3] * 255;
                imgArray[i, 2] = v[maxclusternum][4] * 255;
                //Console.WriteLine("R : " + imgArray[i, 0] + " / G : " + imgArray[i, 1] + " / B : " + imgArray[i, 2]);
            }
            Console.WriteLine("======= x(width) : " + width + "========");
            for (int i = 0; i < CLUSTER; i++)
            {
                Console.WriteLine(i + "번 클러스터 x : " + v[i][0] + " y :" + v[i][1]);
                imgArray[(int)(width * v[i][1] * height + v[i][0] * width), 0] = 0;
                imgArray[(int)(width * v[i][1] * height + v[i][0] * width), 1] = 255;
                imgArray[(int)(width * v[i][1] * height + v[i][0] * width), 2] = 0;
            }

            /*for (int i = 0; i < width*height; i++)
            {
                for (int j = 0; j < CLUSTER; j++)
                {
                    imgArray[i, 0] = v[j][2];
                    imgArray[i, 1] = v[j][3];
                    imgArray[i, 2] = v[j][4];
                }
            }*/

            return imgArray;
        }

        public List<List<double>> insertpattern()
        {
            List<List<double>> pattern = new List<List<double>>();//[CLUSTER, CLUSTER*COORD]
            
            for (int i = 0; i < CLUSTER; i++)
            {
                pattern.Add(new List<double>());
                pattern[i].Add(v[i][0]);
                pattern[i].Add(v[i][1]);
                pattern[i].Add(v[i][2]);
                pattern[i].Add(v[i][3]);
                pattern[i].Add(v[i][4]);
                Console.WriteLine("=====패턴쌍=====");
                Console.Write(pattern[i][0] + " ");
                Console.Write(pattern[i][1] + " ");
                Console.Write(pattern[i][2] + " ");
                Console.Write(pattern[i][3] + " ");
                Console.WriteLine(pattern[i][4] + " ");
            }
            return pattern;
        }
    }
}

