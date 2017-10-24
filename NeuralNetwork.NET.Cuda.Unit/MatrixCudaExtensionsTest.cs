﻿using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeuralNetworkNET.Helpers;
using NeuralNetworkNET.Networks.Implementations;

namespace NeuralNetworkNET.Cuda.Unit
{
    /// <summary>
    /// Test class for the <see cref="MatrixCudaExtensionsTest"/> class
    /// </summary>
    [TestClass]
    [TestCategory(nameof(MatrixCudaExtensionsTest))]
    public class MatrixCudaExtensionsTest
    {
        [TestMethod]
        [SuppressMessage("ReSharper", "ReturnValueOfPureMethodIsNotUsed")]
        public void StopwatchTest()
        {
            // Helper
            void Benchmark(Action a1, Action a2, int iterations = 1)
            {
                while (iterations-- > 0)
                {
                    Stopwatch timer = new Stopwatch();
                    timer.Start();
                    a1();
                    timer.Stop();
                    var t1 = timer.ElapsedMilliseconds;
                    timer.Restart();
                    a2();
                    timer.Stop();
                    var t2 = timer.ElapsedMilliseconds;
                    Debug.WriteLine($"GPU: {t1}ms, CPU: {t2}ms");
                }
            }

            var network = NeuralNetwork.NewRandom(200, 100, 32, 10);
            var r = new Random();
            var input = r.NextMatrix(2000, 200);
            var y = r.NextMatrix(2000, 10);
            Benchmark(() =>
            {
                NetworkTrainerGpuPreferences.ProcessingMode = ProcessingMode.Gpu;
                network.ComputeGradient(input, y);
            },
            () =>
            {
                NetworkTrainerGpuPreferences.ProcessingMode = ProcessingMode.Cpu;
                network.ComputeGradient(input, y);
            }, 10);
        }

        [TestMethod]
        public void Transpose()
        {
            Random r = new Random();
            double[,]
                m1 = r.NextMatrix(7, 3),
                m2 = r.NextMatrix(25, 180),
                m3 = r.NextMatrix(1428, 3811);
            Assert.IsTrue(MatrixExtensions.Transpose(m1).ContentEquals(m1.Transpose()));
            Assert.IsTrue(MatrixExtensions.Transpose(m2).ContentEquals(m2.Transpose()));
            Assert.IsTrue(MatrixExtensions.Transpose(m3).ContentEquals(m3.Transpose()));
        }

        [TestMethod]
        public void Multiply()
        {
            Random r = new Random();
            double[,]
                m1 = r.NextMatrix(7, 3),
                m2 = r.NextMatrix(3, 4),
                check = MatrixExtensions.Multiply(m1, m2);
            double[,] test = m1.Multiply(m2);
            Assert.IsTrue(test.ContentEquals(check));
            m1 = r.NextMatrix(1500, 800);
            m2 = r.NextMatrix(800, 40);
            check = MatrixExtensions.Multiply(m1, m2);
            test = m1.Multiply(m2);
            Assert.IsTrue(test.ContentEquals(check));
        }

        [TestMethod]
        public void TransposeAndMultiply()
        {
            Random r = new Random();
            double[,]
                m1 = r.NextMatrix(5, 13),
                m2 = r.NextMatrix(5, 4),
                check = MatrixExtensions.Multiply(m1.Transpose(), m2);
            double[,] test = m1.TransposeAndMultiply(m2);
            Assert.IsTrue(test.ContentEquals(check));
            m1 = r.NextMatrix(800, 1500);
            m2 = r.NextMatrix(800, 40);
            check = MatrixExtensions.Multiply(m1.Transpose(), m2);
            test = m1.TransposeAndMultiply(m2);
            Assert.IsTrue(test.ContentEquals(check));
        }

        [TestMethod]
        public void MultiplyAndSigmoid()
        {
            Random r = new Random();
            double[,]
                m1 = r.NextMatrix(7, 3),
                m2 = r.NextMatrix(3, 4),
                check = MatrixExtensions.MultiplyAndSigmoid(m1, m2);
            double[,] test = m1.MultiplyAndSigmoid(m2);
            Assert.IsTrue(test.ContentEquals(check));
            m1 = r.NextMatrix(1500, 800);
            m2 = r.NextMatrix(800, 40);
            check = MatrixExtensions.MultiplyAndSigmoid(m1, m2);
            test = m1.MultiplyAndSigmoid(m2);
            Assert.IsTrue(test.ContentEquals(check));
        }

        [TestMethod]
        public void Sigmoid()
        {
            Random r = new Random();
            double[,]
                m = r.NextMatrix(7, 3),
                check = MatrixExtensions.Sigmoid(m);
            double[,] test = m.Sigmoid();
            Assert.IsTrue(test.ContentEquals(check));
            m = r.NextMatrix(1500, 800);
            check = MatrixExtensions.Sigmoid(m);
            test = m.Sigmoid();
            Assert.IsTrue(test.ContentEquals(check));
        }

        [TestMethod]
        public void HalfSquaredDifference()
        {
            Random r = new Random();
            double[,]
                m1 = r.NextMatrix(7, 3),
                m2 = r.NextMatrix(7, 3);
            double
                check = MatrixExtensions.HalfSquaredDifference(m1, m2),
                test = m1.HalfSquaredDifference(m2);
            Assert.IsTrue(Math.Abs(check - test) < 0.0000001);
            m1 = r.NextMatrix(1500, 800);
            m2 = r.NextMatrix(1500, 500);
            check = MatrixExtensions.HalfSquaredDifference(m1, m2);
            test = m1.HalfSquaredDifference(m2);
            Assert.IsTrue(Math.Abs(check - test) < 0.0000001);
        }

        [TestMethod]
        public void InPlaceSubtractAndHadamardProductWithSigmoidPrime()
        {
            Random r = new Random();
            double[,]
                m1 = r.NextMatrix(10, 10),
                m2 = r.NextMatrix(10, 10),
                m3 = r.NextMatrix(10, 10),
                backup = new double[10, 10];
            Buffer.BlockCopy(m1, 0, backup, 0, sizeof(double) * m1.Length);
            MatrixExtensions.InPlaceSubtractAndHadamardProductWithSigmoidPrime(backup, m2, m3);
            m1.InPlaceSubtractAndHadamardProductWithSigmoidPrime(m2, m3);
            Assert.IsTrue(m1.ContentEquals(backup));
            m1 = r.NextMatrix(200, 200);
            m2 = r.NextMatrix(200, 200);
            m3 = r.NextMatrix(200, 200);
            backup = new double[200, 200];
            Buffer.BlockCopy(m1, 0, backup, 0, sizeof(double) * m1.Length);
            MatrixExtensions.InPlaceSubtractAndHadamardProductWithSigmoidPrime(backup, m2, m3);
            m1.InPlaceSubtractAndHadamardProductWithSigmoidPrime(m2, m3);
            Assert.IsTrue(m1.ContentEquals(backup));
        }

        [TestMethod]
        public void InPlaceSigmoidPrimeAndHadamardProduct()
        {
            Random r = new Random();
            double[,]
                m1 = r.NextMatrix(10, 10),
                m2 = r.NextMatrix(10, 10),
                backup = new double[10, 10];
            Buffer.BlockCopy(m1, 0, backup, 0, sizeof(double) * m1.Length);
            MatrixExtensions.InPlaceSigmoidPrimeAndHadamardProduct(backup, m2);
            m1.InPlaceSigmoidPrimeAndHadamardProduct(m2);
            Assert.IsTrue(m1.ContentEquals(backup));
            m1 = r.NextMatrix(200, 200);
            m2 = r.NextMatrix(200, 200);
            backup = new double[200, 200];
            Buffer.BlockCopy(m1, 0, backup, 0, sizeof(double) * m1.Length);
            MatrixExtensions.InPlaceSigmoidPrimeAndHadamardProduct(backup, m2);
            m1.InPlaceSigmoidPrimeAndHadamardProduct(m2);
            Assert.IsTrue(m1.ContentEquals(backup));
        }
    }
}
