using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Runtime.Intrinsics.X86;
using System.Runtime.Intrinsics;
using System.Threading;

namespace IntrinsicsTests
{
    class Program
    {
        static float[,] array1;
        static float[,] array2;
        static float[,] finalResult;
        static int w = 10000;
        static int h = 10000;
        static int[,] testResults;
        static int resultsCount = 10;
        static Random rnd = new Random();
        static Stopwatch stopwatch = new Stopwatch();

        static int count = 1;
        static int runs = 20;

        static void Main(string[] args)
        {

            array1 = new float[h, w];
            array2 = new float[h, w];
            finalResult = new float[h, w];

            testResults = new int[resultsCount, 2];

            SelectTestResults();


            FillArray(array1);
            FillArray(array2);

            RunMultipleTests(runs, count,
                            (Normal, "Normal"),
                            (AVX, "AVX"),
                            (Parallel, "Parallel"),
                            (ParallelAVX, "Par + AVX"),
                            (Threads, "Threads"),
                            (ThreadsAVX, "Thr + AVX")
                            );


        }

        private static void RunMultipleTests(int countOfRuns, int iterationsPerRun, params (Func<int, long>, string)[] tests)
        {
            int countOfTests = tests.Length;
            long[,] times = new long[countOfRuns + 1, countOfTests];
            double[] meanTimes = new double[countOfTests];

            Console.WriteLine();
            Console.WriteLine($"Running tests: {countOfRuns} iterations, {iterationsPerRun} runs each, {countOfTests} tests\nArray size: {h} x {w}");
            Console.WriteLine();
            for (int i = 0; i < countOfRuns; i++)
            {
                for(int j = 0; j < countOfTests; j++)
                {
                    times[i, j] = tests[j].Item1.Invoke(iterationsPerRun);
                }
                Thread.Sleep(188);
            }

            for (int i = 0; i < countOfRuns; i++)
            {
                for (int j = 0; j < countOfTests; j++)
                {
                    meanTimes[j] += times[i, j] / (double)countOfRuns;
                }
            }

            Console.WriteLine($"{count} iterations, {iterationsPerRun} runs each, {countOfTests} tests\nArray size: {h} x {w}");
            Console.WriteLine($"Results:");
            Console.WriteLine();
            
            for (int i = 0; i < countOfTests; i++)
            {
                Console.Write("{0, 15:N0}", tests[i].Item2);
            }
            Console.WriteLine();
            for (int i = 0; i < countOfRuns; i++)
            {
                for (int j = 0; j < countOfTests; j++)
                {
                    Console.Write("{0, 15:N0}", times[i, j]);
                }
                Console.WriteLine();
            }
            Console.WriteLine();
            Console.WriteLine($"Mean:");
            Console.WriteLine();

            for (int i = 0; i < countOfTests; i++)
            {
                Console.Write("{0, 15:N0}", tests[i].Item2);
            }
            Console.WriteLine();
            for (int i = 0; i < countOfTests; i++)
            {
                Console.Write("{0, 15:N2}", meanTimes[i]);
            }

            Console.WriteLine();
            Console.WriteLine($"Speedup:");
            Console.WriteLine();

            for (int i = 0; i < countOfTests; i++)
            {
                Console.Write("{0, 15:N0}", tests[i].Item2);
            }
            Console.WriteLine();
            for (int i = 0; i < countOfTests; i++)
            {
                double tmp = meanTimes[0] / meanTimes[i];
                Console.Write("{0, 15:N2}", tmp);
            }
        }

        private static void SelectTestResults()
        {
            testResults[0, 0] = 0;
            testResults[0, 1] = 0;
            testResults[resultsCount - 1, 0] = h - 1;
            testResults[resultsCount - 1, 1] = w - 1;

            for (int i = 1; i < resultsCount - 1; i++)
            {
                testResults[i, 0] = rnd.Next(0, h);
                testResults[i, 1] = rnd.Next(0, w);
            }
        }

        private static void PrintTestResults()
        {
            Console.Write("Tests: ");
            for (int i = 0; i < resultsCount; i++)
            {
                Console.Write(finalResult[testResults[i, 0], testResults[i, 1]] + " ");
            }
        }

        private static long ThreadsAVX(int iterations)
        {
            Console.WriteLine("Starting Threads + AVX test...");
            stopwatch.Restart();
            for (int i = 0; i < iterations; i++) finalResult = SumThreadsAVX(array1, array2);
            stopwatch.Stop();
            PrintTestResults();
            Console.WriteLine();
            Console.WriteLine($"Threads + AVX: {stopwatch.ElapsedMilliseconds}ms");
            Console.WriteLine();
            return stopwatch.ElapsedTicks;
        }

        private static long Threads(int iterations)
        {
            Console.WriteLine("Starting Threads test...");
            stopwatch.Restart();
            for (int i = 0; i < iterations; i++) finalResult = SumThreads(array1, array2);
            stopwatch.Stop();
            PrintTestResults();
            Console.WriteLine();
            Console.WriteLine($"Threads: {stopwatch.ElapsedMilliseconds}ms");
            Console.WriteLine();
            return stopwatch.ElapsedTicks;
        }

        private static long ParallelAVX(int iterations)
        {
            Console.WriteLine("Starting Parallel + AVX test...");
            stopwatch.Restart();
            for (int i = 0; i < iterations; i++) finalResult = SumParallelAVX(array1, array2);
            stopwatch.Stop();
            PrintTestResults();
            Console.WriteLine();
            Console.WriteLine($"Parallel + AVX: {stopwatch.ElapsedMilliseconds}ms");
            Console.WriteLine();
            return stopwatch.ElapsedTicks;
        }

        private static long Parallel(int iterations)
        {
            Console.WriteLine("Starting AVX test...");
            stopwatch.Restart();
            for (int i = 0; i < iterations; i++) finalResult = SumAVX(array1, array2);
            stopwatch.Stop();
            PrintTestResults();
            Console.WriteLine();
            Console.WriteLine($"AVX: {stopwatch.ElapsedMilliseconds}ms");
            Console.WriteLine();
            return stopwatch.ElapsedTicks;
        }

        private static long AVX(int iterations)
        {
            Console.WriteLine("Starting Parallel test...");
            stopwatch.Restart();
            for (int i = 0; i < iterations; i++) finalResult = SumParallel(array1, array2);
            stopwatch.Stop();
            PrintTestResults();
            Console.WriteLine();
            Console.WriteLine($"Parallel: {stopwatch.ElapsedMilliseconds}ms");
            Console.WriteLine();
            return stopwatch.ElapsedTicks;
        }

        private static long Normal(int iterations)
        {
            Console.WriteLine("Starting Normal test...");
            stopwatch.Restart();
            for (int i = 0; i < iterations; i++) finalResult = SumNormal(array1, array2);
            stopwatch.Stop();
            PrintTestResults();
            Console.WriteLine();
            Console.WriteLine($"Normal: {stopwatch.ElapsedMilliseconds}ms");
            Console.WriteLine();
            return stopwatch.ElapsedTicks;
        }

        private static float[,] SumThreads(float[,] a, float[,] b)
        {
            int height = a.GetLength(0), width = a.GetLength(1);
            int length = height * width;
            float[,] result = new float[height, width];
            int threadCount = Environment.ProcessorCount;
            int singleHeight = (int)Math.Ceiling(height / (float)threadCount);

            Thread[] threads = new Thread[threadCount];
            for (int k = 0; k < threadCount; k++)
            {
                int index = k;
                threads[k] = new Thread(() =>
                {
                    int startHeight = index * singleHeight;
                    int endHeight = startHeight + singleHeight;

                    for (int i = startHeight; i < endHeight; i++)
                    {
                        for (int j = 0; j < width; j++)
                        {
                            result[i, j] = a[i, j] + b[i, j];
                            result[i, j] = a[i, j] * b[i, j];
                            result[i, j] = a[i, j] - b[i, j];
                            result[i, j] = a[i, j] / b[i, j];
                            result[i, j] = a[i, j] + b[i, j];
                            result[i, j] = a[i, j] * b[i, j];
                            result[i, j] = a[i, j] - b[i, j];
                            result[i, j] = a[i, j] / b[i, j];
                            result[i, j] = a[i, j] + b[i, j];
                            result[i, j] = a[i, j] * b[i, j];
                            result[i, j] = a[i, j] - b[i, j];
                            result[i, j] = a[i, j] / b[i, j];
                            result[i, j] = a[i, j] + b[i, j];
                            result[i, j] = a[i, j] * b[i, j];
                            result[i, j] = a[i, j] - b[i, j];
                            result[i, j] = a[i, j] / b[i, j];
                            result[i, j] = a[i, j] + b[i, j];
                            result[i, j] = a[i, j] * b[i, j];
                            result[i, j] = a[i, j] - b[i, j];
                            result[i, j] = a[i, j] / b[i, j];
                            result[i, j] = a[i, j] + b[i, j];
                            result[i, j] = a[i, j] * b[i, j];
                            result[i, j] = a[i, j] - b[i, j];
                            result[i, j] = a[i, j] / b[i, j];
                            result[i, j] = a[i, j] + b[i, j];
                            result[i, j] = a[i, j] * b[i, j];
                            result[i, j] = a[i, j] - b[i, j];
                            result[i, j] = a[i, j] / b[i, j];
                            result[i, j] = a[i, j] + b[i, j];
                            result[i, j] = a[i, j] * b[i, j];
                            result[i, j] = a[i, j] - b[i, j];
                            result[i, j] = a[i, j] / b[i, j];
                        }
                    }
                });
            }
            foreach (Thread t in threads)
            {
                t.Start();
            }
            foreach (Thread t in threads)
            {
                t.Join();
            }
            return result;
        }

        private static float[,] SumThreadsAVX(float[,] a, float[,] b)
        {
            int height = a.GetLength(0), width = a.GetLength(1);
            float[,] result = new float[height, width];
            int threadCount = Environment.ProcessorCount;
            int singleHeight = (int)Math.Ceiling(height / (float)threadCount);
            int singleLength = singleHeight * width;

            Thread[] threads = new Thread[threadCount];
            for (int k = 0; k < threadCount; k++)
            {
                int index = k;
                threads[index] = new Thread(() =>
                {
                    unsafe
                    {
                        fixed (float* pA = a, pB = b, pR = result)
                        {
                            int shift = index * singleHeight * width;
                            float* pAlocal = pA + shift;
                            float* pBlocal = pB + shift;
                            float* pRlocal = pR + shift;
                            for (int j = 0; j < singleLength; j += 8)
                            {
                                Avx2.Store(pRlocal + j, Avx2.Add(Avx2.LoadVector256(pAlocal + j), Avx2.LoadVector256(pBlocal + j)));
                                Avx2.Store(pRlocal + j, Avx2.Multiply(Avx2.LoadVector256(pAlocal + j), Avx2.LoadVector256(pBlocal + j)));
                                Avx2.Store(pRlocal + j, Avx2.Subtract(Avx2.LoadVector256(pAlocal + j), Avx2.LoadVector256(pBlocal + j)));
                                Avx2.Store(pRlocal + j, Avx2.Divide(Avx2.LoadVector256(pAlocal + j), Avx2.LoadVector256(pBlocal + j)));
                                Avx2.Store(pRlocal + j, Avx2.Add(Avx2.LoadVector256(pAlocal + j), Avx2.LoadVector256(pBlocal + j)));
                                Avx2.Store(pRlocal + j, Avx2.Multiply(Avx2.LoadVector256(pAlocal + j), Avx2.LoadVector256(pBlocal + j)));
                                Avx2.Store(pRlocal + j, Avx2.Subtract(Avx2.LoadVector256(pAlocal + j), Avx2.LoadVector256(pBlocal + j)));
                                Avx2.Store(pRlocal + j, Avx2.Divide(Avx2.LoadVector256(pAlocal + j), Avx2.LoadVector256(pBlocal + j)));
                                Avx2.Store(pRlocal + j, Avx2.Add(Avx2.LoadVector256(pAlocal + j), Avx2.LoadVector256(pBlocal + j)));
                                Avx2.Store(pRlocal + j, Avx2.Multiply(Avx2.LoadVector256(pAlocal + j), Avx2.LoadVector256(pBlocal + j)));
                                Avx2.Store(pRlocal + j, Avx2.Subtract(Avx2.LoadVector256(pAlocal + j), Avx2.LoadVector256(pBlocal + j)));
                                Avx2.Store(pRlocal + j, Avx2.Divide(Avx2.LoadVector256(pAlocal + j), Avx2.LoadVector256(pBlocal + j)));
                                Avx2.Store(pRlocal + j, Avx2.Add(Avx2.LoadVector256(pAlocal + j), Avx2.LoadVector256(pBlocal + j)));
                                Avx2.Store(pRlocal + j, Avx2.Multiply(Avx2.LoadVector256(pAlocal + j), Avx2.LoadVector256(pBlocal + j)));
                                Avx2.Store(pRlocal + j, Avx2.Subtract(Avx2.LoadVector256(pAlocal + j), Avx2.LoadVector256(pBlocal + j)));
                                Avx2.Store(pRlocal + j, Avx2.Divide(Avx2.LoadVector256(pAlocal + j), Avx2.LoadVector256(pBlocal + j)));
                                Avx2.Store(pRlocal + j, Avx2.Add(Avx2.LoadVector256(pAlocal + j), Avx2.LoadVector256(pBlocal + j)));
                                Avx2.Store(pRlocal + j, Avx2.Multiply(Avx2.LoadVector256(pAlocal + j), Avx2.LoadVector256(pBlocal + j)));
                                Avx2.Store(pRlocal + j, Avx2.Subtract(Avx2.LoadVector256(pAlocal + j), Avx2.LoadVector256(pBlocal + j)));
                                Avx2.Store(pRlocal + j, Avx2.Divide(Avx2.LoadVector256(pAlocal + j), Avx2.LoadVector256(pBlocal + j)));
                                Avx2.Store(pRlocal + j, Avx2.Add(Avx2.LoadVector256(pAlocal + j), Avx2.LoadVector256(pBlocal + j)));
                                Avx2.Store(pRlocal + j, Avx2.Multiply(Avx2.LoadVector256(pAlocal + j), Avx2.LoadVector256(pBlocal + j)));
                                Avx2.Store(pRlocal + j, Avx2.Subtract(Avx2.LoadVector256(pAlocal + j), Avx2.LoadVector256(pBlocal + j)));
                                Avx2.Store(pRlocal + j, Avx2.Divide(Avx2.LoadVector256(pAlocal + j), Avx2.LoadVector256(pBlocal + j)));
                                Avx2.Store(pRlocal + j, Avx2.Add(Avx2.LoadVector256(pAlocal + j), Avx2.LoadVector256(pBlocal + j)));
                                Avx2.Store(pRlocal + j, Avx2.Multiply(Avx2.LoadVector256(pAlocal + j), Avx2.LoadVector256(pBlocal + j)));
                                Avx2.Store(pRlocal + j, Avx2.Subtract(Avx2.LoadVector256(pAlocal + j), Avx2.LoadVector256(pBlocal + j)));
                                Avx2.Store(pRlocal + j, Avx2.Divide(Avx2.LoadVector256(pAlocal + j), Avx2.LoadVector256(pBlocal + j)));
                                Avx2.Store(pRlocal + j, Avx2.Add(Avx2.LoadVector256(pAlocal + j), Avx2.LoadVector256(pBlocal + j)));
                                Avx2.Store(pRlocal + j, Avx2.Multiply(Avx2.LoadVector256(pAlocal + j), Avx2.LoadVector256(pBlocal + j)));
                                Avx2.Store(pRlocal + j, Avx2.Subtract(Avx2.LoadVector256(pAlocal + j), Avx2.LoadVector256(pBlocal + j)));
                                Avx2.Store(pRlocal + j, Avx2.Divide(Avx2.LoadVector256(pAlocal + j), Avx2.LoadVector256(pBlocal + j)));
                            }
                        }
                    }
                });
            }
            foreach (Thread t in threads)
            {
                t.Start();
            }
            foreach (Thread t in threads)
            {
                t.Join();
            }
            return result;
        }

        private static float[,] SumAVX(float[,] a, float[,] b)
        {
            int height = a.GetLength(0), width = a.GetLength(1);
            int length = height * width;
            float[,] result = new float[height, width];
            Vector256<float> tmp = Vector256<float>.Zero;
            unsafe
            {
                fixed (float* pA = a, pB = b, pR = result)
                {
                    for (int i = 0; i < length; i += 8)
                    {
                        Avx2.Store(pR + i, Avx2.Add(Avx2.LoadVector256(pA + i), Avx2.LoadVector256(pB + i)));
                        Avx2.Store(pR + i, Avx2.Multiply(Avx2.LoadVector256(pA + i), Avx2.LoadVector256(pB + i)));
                        Avx2.Store(pR + i, Avx2.Subtract(Avx2.LoadVector256(pA + i), Avx2.LoadVector256(pB + i)));
                        Avx2.Store(pR + i, Avx2.Divide(Avx2.LoadVector256(pA + i), Avx2.LoadVector256(pB + i)));
                        Avx2.Store(pR + i, Avx2.Add(Avx2.LoadVector256(pA + i), Avx2.LoadVector256(pB + i)));
                        Avx2.Store(pR + i, Avx2.Multiply(Avx2.LoadVector256(pA + i), Avx2.LoadVector256(pB + i)));
                        Avx2.Store(pR + i, Avx2.Subtract(Avx2.LoadVector256(pA + i), Avx2.LoadVector256(pB + i)));
                        Avx2.Store(pR + i, Avx2.Divide(Avx2.LoadVector256(pA + i), Avx2.LoadVector256(pB + i)));
                        Avx2.Store(pR + i, Avx2.Add(Avx2.LoadVector256(pA + i), Avx2.LoadVector256(pB + i)));
                        Avx2.Store(pR + i, Avx2.Multiply(Avx2.LoadVector256(pA + i), Avx2.LoadVector256(pB + i)));
                        Avx2.Store(pR + i, Avx2.Subtract(Avx2.LoadVector256(pA + i), Avx2.LoadVector256(pB + i)));
                        Avx2.Store(pR + i, Avx2.Divide(Avx2.LoadVector256(pA + i), Avx2.LoadVector256(pB + i)));
                        Avx2.Store(pR + i, Avx2.Add(Avx2.LoadVector256(pA + i), Avx2.LoadVector256(pB + i)));
                        Avx2.Store(pR + i, Avx2.Multiply(Avx2.LoadVector256(pA + i), Avx2.LoadVector256(pB + i)));
                        Avx2.Store(pR + i, Avx2.Subtract(Avx2.LoadVector256(pA + i), Avx2.LoadVector256(pB + i)));
                        Avx2.Store(pR + i, Avx2.Divide(Avx2.LoadVector256(pA + i), Avx2.LoadVector256(pB + i)));
                        Avx2.Store(pR + i, Avx2.Add(Avx2.LoadVector256(pA + i), Avx2.LoadVector256(pB + i)));
                        Avx2.Store(pR + i, Avx2.Multiply(Avx2.LoadVector256(pA + i), Avx2.LoadVector256(pB + i)));
                        Avx2.Store(pR + i, Avx2.Subtract(Avx2.LoadVector256(pA + i), Avx2.LoadVector256(pB + i)));
                        Avx2.Store(pR + i, Avx2.Divide(Avx2.LoadVector256(pA + i), Avx2.LoadVector256(pB + i)));
                        Avx2.Store(pR + i, Avx2.Add(Avx2.LoadVector256(pA + i), Avx2.LoadVector256(pB + i)));
                        Avx2.Store(pR + i, Avx2.Multiply(Avx2.LoadVector256(pA + i), Avx2.LoadVector256(pB + i)));
                        Avx2.Store(pR + i, Avx2.Subtract(Avx2.LoadVector256(pA + i), Avx2.LoadVector256(pB + i)));
                        Avx2.Store(pR + i, Avx2.Divide(Avx2.LoadVector256(pA + i), Avx2.LoadVector256(pB + i)));
                        Avx2.Store(pR + i, Avx2.Add(Avx2.LoadVector256(pA + i), Avx2.LoadVector256(pB + i)));
                        Avx2.Store(pR + i, Avx2.Multiply(Avx2.LoadVector256(pA + i), Avx2.LoadVector256(pB + i)));
                        Avx2.Store(pR + i, Avx2.Subtract(Avx2.LoadVector256(pA + i), Avx2.LoadVector256(pB + i)));
                        Avx2.Store(pR + i, Avx2.Divide(Avx2.LoadVector256(pA + i), Avx2.LoadVector256(pB + i)));
                        Avx2.Store(pR + i, Avx2.Add(Avx2.LoadVector256(pA + i), Avx2.LoadVector256(pB + i)));
                        Avx2.Store(pR + i, Avx2.Multiply(Avx2.LoadVector256(pA + i), Avx2.LoadVector256(pB + i)));
                        Avx2.Store(pR + i, Avx2.Subtract(Avx2.LoadVector256(pA + i), Avx2.LoadVector256(pB + i)));
                        Avx2.Store(pR + i, Avx2.Divide(Avx2.LoadVector256(pA + i), Avx2.LoadVector256(pB + i)));
                    }
                }
            }
            return result;
        }

        private static float[,] SumParallelAVX(float[,] a, float[,] b)
        {
            int height = a.GetLength(0), width = a.GetLength(1);
            int length = height * width;
            float[,] result = new float[height, width];
            Vector256<float> tmp = Vector256<float>.Zero;
            unsafe
            {
                System.Threading.Tasks.Parallel.For(0, height, (i) =>
                {
                    fixed (float* pA = a, pB = b, pR = result)
                    {
                        for (int j = 0; j < width; j += 8)
                        {
                            Avx2.Store(pR + i * width + j, Avx2.Add(Avx2.LoadVector256(pA + i * width + j), Avx2.LoadVector256(pB + i * width + j)));
                            Avx2.Store(pR + i * width + j, Avx2.Multiply(Avx2.LoadVector256(pA + i * width + j), Avx2.LoadVector256(pB + i * width + j)));
                            Avx2.Store(pR + i * width + j, Avx2.Subtract(Avx2.LoadVector256(pA + i * width + j), Avx2.LoadVector256(pB + i * width + j)));
                            Avx2.Store(pR + i * width + j, Avx2.Divide(Avx2.LoadVector256(pA + i * width + j), Avx2.LoadVector256(pB + i * width + j)));
                            Avx2.Store(pR + i * width + j, Avx2.Add(Avx2.LoadVector256(pA + i * width + j), Avx2.LoadVector256(pB + i * width + j)));
                            Avx2.Store(pR + i * width + j, Avx2.Multiply(Avx2.LoadVector256(pA + i * width + j), Avx2.LoadVector256(pB + i * width + j)));
                            Avx2.Store(pR + i * width + j, Avx2.Subtract(Avx2.LoadVector256(pA + i * width + j), Avx2.LoadVector256(pB + i * width + j)));
                            Avx2.Store(pR + i * width + j, Avx2.Divide(Avx2.LoadVector256(pA + i * width + j), Avx2.LoadVector256(pB + i * width + j)));
                            Avx2.Store(pR + i * width + j, Avx2.Add(Avx2.LoadVector256(pA + i * width + j), Avx2.LoadVector256(pB + i * width + j)));
                            Avx2.Store(pR + i * width + j, Avx2.Multiply(Avx2.LoadVector256(pA + i * width + j), Avx2.LoadVector256(pB + i * width + j)));
                            Avx2.Store(pR + i * width + j, Avx2.Subtract(Avx2.LoadVector256(pA + i * width + j), Avx2.LoadVector256(pB + i * width + j)));
                            Avx2.Store(pR + i * width + j, Avx2.Divide(Avx2.LoadVector256(pA + i * width + j), Avx2.LoadVector256(pB + i * width + j)));
                            Avx2.Store(pR + i * width + j, Avx2.Add(Avx2.LoadVector256(pA + i * width + j), Avx2.LoadVector256(pB + i * width + j)));
                            Avx2.Store(pR + i * width + j, Avx2.Multiply(Avx2.LoadVector256(pA + i * width + j), Avx2.LoadVector256(pB + i * width + j)));
                            Avx2.Store(pR + i * width + j, Avx2.Subtract(Avx2.LoadVector256(pA + i * width + j), Avx2.LoadVector256(pB + i * width + j)));
                            Avx2.Store(pR + i * width + j, Avx2.Divide(Avx2.LoadVector256(pA + i * width + j), Avx2.LoadVector256(pB + i * width + j)));
                            Avx2.Store(pR + i * width + j, Avx2.Add(Avx2.LoadVector256(pA + i * width + j), Avx2.LoadVector256(pB + i * width + j)));
                            Avx2.Store(pR + i * width + j, Avx2.Multiply(Avx2.LoadVector256(pA + i * width + j), Avx2.LoadVector256(pB + i * width + j)));
                            Avx2.Store(pR + i * width + j, Avx2.Subtract(Avx2.LoadVector256(pA + i * width + j), Avx2.LoadVector256(pB + i * width + j)));
                            Avx2.Store(pR + i * width + j, Avx2.Divide(Avx2.LoadVector256(pA + i * width + j), Avx2.LoadVector256(pB + i * width + j)));
                            Avx2.Store(pR + i * width + j, Avx2.Add(Avx2.LoadVector256(pA + i * width + j), Avx2.LoadVector256(pB + i * width + j)));
                            Avx2.Store(pR + i * width + j, Avx2.Multiply(Avx2.LoadVector256(pA + i * width + j), Avx2.LoadVector256(pB + i * width + j)));
                            Avx2.Store(pR + i * width + j, Avx2.Subtract(Avx2.LoadVector256(pA + i * width + j), Avx2.LoadVector256(pB + i * width + j)));
                            Avx2.Store(pR + i * width + j, Avx2.Divide(Avx2.LoadVector256(pA + i * width + j), Avx2.LoadVector256(pB + i * width + j)));
                            Avx2.Store(pR + i * width + j, Avx2.Add(Avx2.LoadVector256(pA + i * width + j), Avx2.LoadVector256(pB + i * width + j)));
                            Avx2.Store(pR + i * width + j, Avx2.Multiply(Avx2.LoadVector256(pA + i * width + j), Avx2.LoadVector256(pB + i * width + j)));
                            Avx2.Store(pR + i * width + j, Avx2.Subtract(Avx2.LoadVector256(pA + i * width + j), Avx2.LoadVector256(pB + i * width + j)));
                            Avx2.Store(pR + i * width + j, Avx2.Divide(Avx2.LoadVector256(pA + i * width + j), Avx2.LoadVector256(pB + i * width + j)));
                            Avx2.Store(pR + i * width + j, Avx2.Add(Avx2.LoadVector256(pA + i * width + j), Avx2.LoadVector256(pB + i * width + j)));
                            Avx2.Store(pR + i * width + j, Avx2.Multiply(Avx2.LoadVector256(pA + i * width + j), Avx2.LoadVector256(pB + i * width + j)));
                            Avx2.Store(pR + i * width + j, Avx2.Subtract(Avx2.LoadVector256(pA + i * width + j), Avx2.LoadVector256(pB + i * width + j)));
                            Avx2.Store(pR + i * width + j, Avx2.Divide(Avx2.LoadVector256(pA + i * width + j), Avx2.LoadVector256(pB + i * width + j)));
                        }
                    }
                });
            }
            return result;
        }

        private static float[,] SumParallel(float[,] a, float[,] b)
        {
            int height = a.GetLength(0), width = a.GetLength(1);
            float[,] result = new float[height, width];
            System.Threading.Tasks.Parallel.For(0, height, (i) =>
        {
            for (int j = 0; j < width; j++)
            {
                result[i, j] = a[i, j] + b[i, j];
                result[i, j] = a[i, j] * b[i, j];
                result[i, j] = a[i, j] - b[i, j];
                result[i, j] = a[i, j] / b[i, j];
                result[i, j] = a[i, j] + b[i, j];
                result[i, j] = a[i, j] * b[i, j];
                result[i, j] = a[i, j] - b[i, j];
                result[i, j] = a[i, j] / b[i, j];
                result[i, j] = a[i, j] + b[i, j];
                result[i, j] = a[i, j] * b[i, j];
                result[i, j] = a[i, j] - b[i, j];
                result[i, j] = a[i, j] / b[i, j];
                result[i, j] = a[i, j] + b[i, j];
                result[i, j] = a[i, j] * b[i, j];
                result[i, j] = a[i, j] - b[i, j];
                result[i, j] = a[i, j] / b[i, j];
                result[i, j] = a[i, j] + b[i, j];
                result[i, j] = a[i, j] * b[i, j];
                result[i, j] = a[i, j] - b[i, j];
                result[i, j] = a[i, j] / b[i, j];
                result[i, j] = a[i, j] + b[i, j];
                result[i, j] = a[i, j] * b[i, j];
                result[i, j] = a[i, j] - b[i, j];
                result[i, j] = a[i, j] / b[i, j];
                result[i, j] = a[i, j] + b[i, j];
                result[i, j] = a[i, j] * b[i, j];
                result[i, j] = a[i, j] - b[i, j];
                result[i, j] = a[i, j] / b[i, j];
                result[i, j] = a[i, j] + b[i, j];
                result[i, j] = a[i, j] * b[i, j];
                result[i, j] = a[i, j] - b[i, j];
                result[i, j] = a[i, j] / b[i, j];
            }
        });
            return result;
        }

        private static float[,] SumNormal(float[,] a, float[,] b)
        {
            int height = a.GetLength(0), width = a.GetLength(1);
            float[,] result = new float[height, width];
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    result[i, j] = a[i, j] + b[i, j];
                    result[i, j] = a[i, j] * b[i, j];
                    result[i, j] = a[i, j] - b[i, j];
                    result[i, j] = a[i, j] / b[i, j];
                    result[i, j] = a[i, j] + b[i, j];
                    result[i, j] = a[i, j] * b[i, j];
                    result[i, j] = a[i, j] - b[i, j];
                    result[i, j] = a[i, j] / b[i, j];
                    result[i, j] = a[i, j] + b[i, j];
                    result[i, j] = a[i, j] * b[i, j];
                    result[i, j] = a[i, j] - b[i, j];
                    result[i, j] = a[i, j] / b[i, j];
                    result[i, j] = a[i, j] + b[i, j];
                    result[i, j] = a[i, j] * b[i, j];
                    result[i, j] = a[i, j] - b[i, j];
                    result[i, j] = a[i, j] / b[i, j];
                    result[i, j] = a[i, j] + b[i, j];
                    result[i, j] = a[i, j] * b[i, j];
                    result[i, j] = a[i, j] - b[i, j];
                    result[i, j] = a[i, j] / b[i, j];
                    result[i, j] = a[i, j] + b[i, j];
                    result[i, j] = a[i, j] * b[i, j];
                    result[i, j] = a[i, j] - b[i, j];
                    result[i, j] = a[i, j] / b[i, j];
                    result[i, j] = a[i, j] + b[i, j];
                    result[i, j] = a[i, j] * b[i, j];
                    result[i, j] = a[i, j] - b[i, j];
                    result[i, j] = a[i, j] / b[i, j];
                    result[i, j] = a[i, j] + b[i, j];
                    result[i, j] = a[i, j] * b[i, j];
                    result[i, j] = a[i, j] - b[i, j];
                    result[i, j] = a[i, j] / b[i, j];
                }
            }
            return result;
        }

        private static void FillArray(float[,] a)
        {
            int height = a.GetLength(0), width = a.GetLength(1);
            float[,] result = new float[height, width];

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    a[i, j] = (float)rnd.NextDouble() * 100;
                }
            }
        }
    }
}
