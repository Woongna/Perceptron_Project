using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 응애
{
    class SinglePerceptron
    {
        
            //init
            #region
            double[,] target;
            List<List<double>> pattern;
            List<double> testpattern;
        int N_PATERN;
            int PATERN_SIZE = 20;
        const int outnum = 3;
        public double[,] weight;
            public double[] bias = new double[outnum];
            const double learningRate = 0.1;    //학습률
            double totalError = 0.3;
            int nepochMax = 1000;
            
        #endregion
        public SinglePerceptron(List<double> data)
        {
            testpattern = new List<double>(data);
        }
            public SinglePerceptron(List<List<double>> data, int size, double[,] target)
            {
                //init
                #region
                this.target = target;

            N_PATERN = size;

                 pattern = new List<List<double>>(data);

                #endregion
                weight = new double[PATERN_SIZE, outnum];

                initialWeight();
                run();
            }

            public void initialWeight()
            {
                Random rr = new Random();
                for (int i = 0; i < PATERN_SIZE; i++)
                {
                    for (int j = 0; j < outnum; j++)
                    {
                        weight[i,j] = rr.NextDouble();
                        bias[j] = rr.NextDouble();
                    }
                }
            }

            public double[] Recognition(int outnum, int PATERN_SIZE, List<List<double>> weight, List<double> bias)
            {
                double[] y = new double[outnum];
                for (int k = 0; k < outnum; k++)
                {
                    for (int j = 0; j < PATERN_SIZE; j++)
                    {
                        y[k] += testpattern[j] * weight[j][k];
                    }
                    //y[k] -= bias[k];

                    Console.WriteLine("y : " + y[k]);
                }
                return y;
            }

            public void run()
            {
                int nepoch = 0;
                //N_PATERN은 패턴 갯수 PATERN_SIZE는 속성 갯수
                while (totalError > 0.1)
                {

                    totalError = 0;
                     double[,] sum = new double[N_PATERN, outnum];
                    for (int i = 0; i < N_PATERN; i++)
                    {
                        double error = 0;

                        for (int j = 0; j < PATERN_SIZE; j++)
                        {
                        for (int k = 0; k < outnum; k++)
                        {
                            sum[i,k] += pattern[i][j] * weight[j,k];
                            //sum[i,k] -= bias[k];

                            /*if (sum[i,k] > 0)
                            {
                                sum[i,k] = 1;
                            }
                            else
                            {
                                sum[i,k] = 0;
                            }*/

                            if (sum[i,k] == target[i,k])
                            {
                                continue;
                            }
                            else
                            {
                                error = target[i,k] - sum[i,k];
                                bias[k] = learningRate * error;
                                weight[j,k] += learningRate * error * pattern[i][j];
                            }
                        }
                    }
                    totalError += Math.Abs(error);
                }
                nepoch++;
                Console.WriteLine("오차값 : " + totalError);
                if (nepoch == 10000)
                {
                    break;
                }
            }
        }

    }
}
