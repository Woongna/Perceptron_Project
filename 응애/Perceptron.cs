using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 응애
{
    class Perceptron
    {
        const int MAX_PATTERN = 3;  //영상개수
        const int DIM = 20;         
        const int L_2_NODE = 20;    //input
        const int L_3_NODE = 3;     //output
        const int OUTPUT = L_3_NODE;
        const int INPUT = DIM;

        int N_PATTERN = MAX_PATTERN;

        public double[,] target = new double[3, 3]   { { 1.0, 0.0, 0.0},
                                                       { 0.0, 1.0, 0.0},
                                                       { 0.0, 0.0, 1.0} };

        double[,] weight = new double[OUTPUT, INPUT];
        double[,] dweight = new double[OUTPUT, INPUT];
        double[,] old_dweight = new double[OUTPUT, INPUT];
        double[] min = new double[INPUT];
        double[] max = new double[OUTPUT];
        double[] bias = new double[OUTPUT];
        double[] dbias = new double[OUTPUT];
        double[] old_dbias = new double[OUTPUT];
        double[] layer = new double[OUTPUT];
        int Max_Iteration = 1000;
        double Lrate1 = 0.001, Lrate2 = 0.001;
        double Momentum1 = 0.999, Momentum2 = 0.999;
        double Ecrit = 0.001;
        int nepoch;     //몇번도냐
        double pss, tss;
        StringBuilder sb = new StringBuilder();

        List<List<double>> pattern = new List<List<double>>(); // 학습 데이터 - 클러스터 중심 좌표, RGB값
        List<List<double>> testpattern = new List<List<double>>(); // 테스트 데이터
        public List<List<double>> getdata(int CLUSTER, int imagenum, List<List<double>> data) //영상번호, 총영상개수, 클러스터개수 * 속성, 속성값
        {
            Console.WriteLine("퍼셉트론 실행");
            for (int i = 0; i < CLUSTER; i++)
            {
                pattern.Add(new List<double>());
                pattern[imagenum].Add(data[i][0]); //[클러스터 개수, 중심벡터속성값]
                pattern[imagenum].Add(data[i][1]);
                pattern[imagenum].Add(data[i][2]);
                pattern[imagenum].Add(data[i][3]);
                pattern[imagenum].Add(data[i][4]);
            }
            return pattern;
        }
        ////////////////////////////////////
        ////test데이터 값받기
        public List<List<double>> getdata(int CLUSTER,int imagenum, int temp, List<List<double>> data) //영상번호, 총영상개수, 클러스터개수 * 속성, 속성값
        {
            testpattern.Clear();
            Console.WriteLine("퍼셉트론 실행");
                testpattern.Add(new List<double>());
            for (int j = 0; j < CLUSTER; j++)
            {
                testpattern[0].Add(data[j][0]); //[클러스터 개수, 중심벡터속성값]
                testpattern[0].Add(data[j][1]);
                testpattern[0].Add(data[j][2]);
                testpattern[0].Add(data[j][3]);
                testpattern[0].Add(data[j][4]);
            }
            /*Console.WriteLine(testpattern[0][15]);
            Console.WriteLine(testpattern[0][16]);
            Console.WriteLine(testpattern[0][17]);
            Console.WriteLine(testpattern[0][18]);
            Console.WriteLine(testpattern[0][19]);*/
            return testpattern;
        }
        ///////////////////////////////////
        public void Initialize_weight()     //weight초기화
        {
            int i, j;
            Random rr = new Random();
            for (i = 0; i < OUTPUT; i++)
            {
                for (j = 0; j < INPUT; j++)
                {
                    weight[i, j] = ((double)(rr.Next(10000)) / 10000.0 / 2) + 0.5;
                    //Console.Write(weight[i, j] + " / ");
                }
                bias[i] = ((double)(rr.Next(10000)) / 10000.0 / 2) + 0.5;
               //Console.WriteLine("// / " + bias[i]);

            }
        }
        public void activate(int index) 
        {
            int i, j;
            double wdiff, bdiff;
            for (i = 0; i < OUTPUT; i++)
            {
                max[i] = 0.0;
            }

            for (i = 0; i < OUTPUT; i++)
            {
                for (j = 0; j < INPUT; j++)
                {
                    if (pattern[index][j] > weight[i, j]) min[j] = weight[i, j];

                    else min[j] = pattern[index][j];
                    if (max[i] < min[j]) max[i] = min[j];
                }
                layer[i] = (max[i] > bias[i]) ? max[i] : bias[i]; //output node
            }

            for (i = 0; i < OUTPUT; i++)
            {
                for (j = 0; j < INPUT; j++)
                {
                    wdiff = (layer[i] == weight[i, j]) ? 1.0 : 0.0;
                    //Console.WriteLine(wdiff + " // ");
                    dweight[i, j] += wdiff * (target[index, i] - layer[i]);
                    //Console.WriteLine("asdfasdf" + (target[index, i] - layer[i]));
                }
                bdiff = (layer[i] == bias[i]) ? 1.0 : 0.0;
                dbias[i] += bdiff * (target[index, i] - layer[i]);
            }
        }

        public void compute_pss(int index)
        {
            int i;
            double error;
            pss = 0.0;
            for (i = 0; i < OUTPUT; i++)
            {
                error = layer[i] - target[index, i];
                pss += error * error;
                Lrate1 = pss;
                Lrate2 = pss;
                Momentum1 = 1 - pss;
                Momentum2 = 1 - pss;
            }
        }

        public void change_weight()
        {
            int i, j;
            for (i = 0; i < OUTPUT; i++)
            {
                for (j = 0; j < INPUT; j++)
                {
                    weight[i, j] += Lrate1 * dweight[i, j] + Momentum1 * old_dweight[i, j];
                    if (weight[i, j] < 0.0) weight[i, j] = 0.0;
                    if (weight[i, j] >= 1.0) weight[i, j] = 1.0;
                    old_dweight[i, j] = dweight[i, j];
                    dweight[i, j] = 0.0;

                    //Console.Write(" || 웨이트:" + weight[i,j]);

                }
                bias[i] += Lrate2 * dbias[i] + Momentum2 * old_dbias[i];
                if (bias[i] < 0.0) bias[i] = 0.0;
                if (bias[i] >= 1.0) bias[i] = 1.0;
                old_dbias[i] = dbias[i];
                dbias[i] = 0.0;

            }
        }

        public void Recognition()
        {
            int i, j;
            Console.WriteLine("Recognition result");
            for (i = 0; i < N_PATTERN; i++)
            {
                activate(i);
                for (j = 0; j < OUTPUT; j++)
                {
                    Console.WriteLine("Target[" + i + "][" + j + "]= " + target[i, j] + ", computed[" + j + "]= " + layer[j]);
                    if (((target[i, j] - layer[j]) > 0.5) || ((target[i, j] - layer[j]) < -0.5))
                        Console.WriteLine("학습이 제대로 되지않았습니다.");
                }
            }
        }
        
        public void run()
        {
            int i;
            Initialize_weight();
            Console.WriteLine("Learning Process");
            nepoch = 0;
            int tcount = 0;
            do
            {
                nepoch++;
                tss = 0.0;
                for (i = 0; i < N_PATTERN; i++)
                {
                    activate(i);
                    compute_pss(i);
                    tss += pss;
                }
                change_weight();
                //Console.WriteLine();
                //Console.WriteLine("[" + nepoch + "] : TSS = " + tss + "\n");
                if (nepoch > Max_Iteration)
                {
                    tcount++;
                    Console.WriteLine(tcount + " / " + tss);
                    Initialize_weight();
                    nepoch = 0;
                }
            } while (tss > Ecrit);
            Recognition();
        }

        public void Recognition2()
        {
            int i, j;
            Console.WriteLine("");
            Console.WriteLine("Recognition result");
            for (i = 0; i < N_PATTERN; i++)
            {
                activate2(0);
                for (j = 0; j < OUTPUT; j++)
                {
                    Console.WriteLine("Target [" + i + "]" + "[" + j + "]= " + target[i, j] + ", computed[" + j + "]= " + layer[j]);
                }
            }
        }
        public void activate2(int index)
        {
            int i, j;
            double wdiff, bdiff;
            for (i = 0; i < OUTPUT; i++)
            {
                max[i] = 0.0;
            }

            for (i = 0; i < OUTPUT; i++)
            {
                for (j = 0; j < INPUT; j++)
                {
                    if (testpattern[index][j] > weight[i, j]) min[j] = weight[i, j];

                    else min[j] = testpattern[index][j];
                    if (max[i] < min[j]) max[i] = min[j];
                }
                layer[i] = (max[i] > bias[i]) ? max[i] : bias[i]; //output node
            }

            for (i = 0; i < OUTPUT; i++)
            {
                for (j = 0; j < INPUT; j++)
                {
                    wdiff = (layer[i] == weight[i, j]) ? 1.0 : 0.0;
                    dweight[i, j] += wdiff * (target[index, i] - layer[i]);
                }
                bdiff = (layer[i] == bias[i]) ? 1.0 : 0.0;
                dbias[i] += bdiff * (target[index, i] - layer[i]);
            }
        }
        public void run2()
        {
            Console.WriteLine("Learning Process");
            
            tss = 0.0;
                
            activate2(0);
                
            change_weight();

            Recognition2();
        }
    }
}
