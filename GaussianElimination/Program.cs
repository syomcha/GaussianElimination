using System;
using System.Collections.Generic;

namespace GaussianElimination
{
    class Program
    {
        private static void Main()
        {
            var experiments = new List<Tuple<double[,], double[]>>
            {
                // 0 - Бесконечное множество решений
                Tuple.Create(
                    new double[,]
                    {
                        {2, 3, -1},
                        {4, 6, -2},
                        {3, -1, 2}
                    },
                    new double[] {3, 6, -1}),

                // нет решений
                Tuple.Create(
                    new double[,]
                    {
                        {7, -2, -1},
                        {6, -4, -5},
                        {1, 2, 4}
                    },
                    new double[] {2, 3, 5}),

                // 1 - x0 = 3, x1 = -1
                Tuple.Create(
                    new double[,]
                    {
                        {3, 2},
                        {1, -1}
                    },
                    new double[] {7, 4}),

                // 2 - x0 = 4, x1 = 3, x2 = 5
                Tuple.Create(
                    new double[,]
                    {
                        {1, 2, 0},
                        {3, 2, 1},
                        {0, 1, 2}
                    },
                    new double[] {10, 23, 13}),

                // x0 = 2.49, x1=-1.06, x2=0.54, x3=-0.96
                Tuple.Create(
                    new double[,]
                    {
                        {5, 4, -6, 1},
                        {7, -1, 4, 8},
                        {0, 5, 3, -9},
                        {4, 0, 7, 6}
                    },
                    new double[] {4, 13, 5, 8})
            };

            for (var i = 0; i < experiments.Count; i++)
            {
                var a = experiments[i].Item1;
                var b = experiments[i].Item2;

                PrintColorMessage($"Эксперимент #{i}", ConsoleColor.Cyan);
                Console.WriteLine("Исходная система линейных уравнений:");
                PrintSystem(a, b);

                try
                {
                    var x = SolveMatrixByGaussMethod(a, b);
                    PrintColorMessage("Ответ:", ConsoleColor.Green);
                    PrintMatrix("X", x);
                    Console.WriteLine();
                }
                catch (Exception e)
                {
                    PrintColorMessage(e.Message, ConsoleColor.Red);
                }
            }
        }

        public static double[] SolveMatrixByGaussMethod(double[,] a, double[] b)
        {
            if (a.GetLength(0) != a.GetLength(1))
                throw new Exception("Матрица А не является квадратной");

            if (a.GetLength(0) != b.Length)
                throw new Exception("Матрицы A и B имеют разные размеры");

            Console.WriteLine("\nПриведение матрицы к треугольному виду...");
            ReduceMatrixToTriangularForm(a, b);

            Console.WriteLine("\nПриведенная матрица:");
            PrintSystem(a, b);

            var x = new double[b.Length]; // Матрица Х, явлеющаяся решением системы линейных уравнений
            for (var i = b.Length - 1; i >= 0; i--)
            {
                double sum = 0;
                for (var j = i + 1; j < a.GetLength(1); j++)
                    sum += x[j] * a[i, j];
                x[i] = b[i] - sum;
            }

            return x;
        }

        public static void ReduceMatrixToTriangularForm(double[,] a, double[] b)
        {
            var column = 0;
            // Обходить матрицу будем по главной диагонали, поэтому номер столбца = номер строки во всех выражениях
            while (column < a.GetLength(1))
            {
                for (var row = 0; row < a.GetLength(0); row++)
                {
                    var isZeroRow = true;
                    for (var col = 0; col < a.GetLength(1); col++)
                        if (a[row, col] != 0)
                            isZeroRow = false;

                    if (isZeroRow && b[row] != 0)
                        throw new ArgumentException("Система не имеет решений");
                    if (isZeroRow && b[row] == 0)
                        throw new Exception("Система имеет бесконечное множество решений");
                }


                // Поиск максимального элемента в столбце, если такой элемент будет найден, то он премещается на самую верхнуюю возможную строку
                Console.WriteLine("\nИщем максимальный по модулю элемент в столбце {0}:", column + 1);
                FindMaxInColumn(a, b, column);
                PrintSystem(a, b);

                Console.WriteLine("\nПоделим строку {0} на {1:f2}:", column + 1, a[column, column]);
                DivideRowByNumber(a, b, column, a[column, column]);
                PrintSystem(a, b);

                // Обработка строк, которые лежат ниже элемента (используем номер стлобца, так как номер столбца = номеру строки)
                for (var i = column + 1; i < a.GetLength(0); i++)
                {
                    Console.WriteLine("\n{0}-ю строку складываем с {1} строкой, умноженной на {2:f2}:", i + 1,
                        column + 1, -a[i, column]);
                    SumRows(a, b, i, column, -a[i, column]);
                    PrintSystem(a, b);
                }

                column += 1;
            }
        }

        public static void FindMaxInColumn(double[,] a, double[] b, int column)
        {
            var currentRow = int.MinValue;
            for (var i = column; i < a.GetLength(0); i++)
                if (currentRow == int.MinValue || Math.Abs(a[i, column]) > Math.Abs(a[currentRow, column]))
                    currentRow = i;
            if (currentRow != column)
                SwapRows(a, b, currentRow, column);
        }

        // Элементарные преобразования строк
        public static void SumRows(double[,] a, double[] b, int row1, int row2, double k)
        {
            for (var i = 0; i < a.GetLength(1); i++)
                a[row1, i] += a[row2, i] * k;
            b[row1] += b[row2] * k;
        }

        public static void DivideRowByNumber(double[,] a, double[] b, int row, double k)
        {
            for (var j = 0; j < a.GetLength(1); j++)
                a[row, j] /= k;
            b[row] /= k;
        }

        public static void SwapRows(double[,] a, double[] b, int row1, int row2)
        {
            double temp; // Переменная для хранения значения элемента матрицы
            // Обмен строк в матрице А
            for (var j = 0; j < a.GetLength(1); j++)
            {
                temp = a[row1, j];
                a[row1, j] = a[row2, j];
                a[row2, j] = temp;
            }

            // Обмен строк в матрице B
            temp = b[row1];
            b[row1] = b[row2];
            b[row2] = temp;
        }

        // Вывод сообщений
        public static void PrintSystem(double[,] a, double[] b)
        {
            for (var i = 0; i < a.GetLength(0); i++)
            {
                var rowsA = new string[a.GetLength(0)];
                for (var j = 0; j < a.GetLength(1); j++)
                    rowsA[j] = $"({a[i, j],5:f2}) * X{j}";
                var fullRow = string.Join(" + ", rowsA);
                Console.WriteLine("{0} = {1,5:f2}", fullRow, b[i]);
            }
        }

        public static void PrintMatrix(string name, double[] matrix)
        {
            for (var i = 0; i < matrix.Length; i++)
                Console.WriteLine("{0}{1} = {2,5:F4}", name, i, matrix[i]);
        }

        public static void PrintColorMessage(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(new string('=', message.Length + 6));
            Console.WriteLine($"   {message}");
            Console.WriteLine(new string('=', message.Length + 6));
            Console.ResetColor();
        }
    }
}
