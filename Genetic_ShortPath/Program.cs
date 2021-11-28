using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Genetic_ShortPath
{
    class Program
    {
        static int osobi, cities, geny, StartCity, EndCity;

        static void Main(string[] args)
        {
            var sw = new Stopwatch();
            cities = 300;
            Console.WriteLine("Кол-во городов: " + cities);
            Random rand = new Random();



            int[,] MatrDistance = new int[cities, cities];  //матрица растояний
            for (int i = 0; i < cities; i++)
                for (int j = i + 1; j < cities; j++)
                    MatrDistance[i, j] = rand.Next(5, 150);
            for (int i = 0; i < cities; i++)
                for (int j = 0; j < i; j++)
                    MatrDistance[i, j] = MatrDistance[j, i];

            /*    Console.WriteLine("Длина пути от одного города к другому: ");
              Console.Write("   | ");
               for (int i = 0; i < cities; i++)
               {
                   if (i < 9)
                       Console.Write("{0}   ", i + 1);
                   else
                       Console.Write("{0}  ", i + 1);
               }


               Console.WriteLine();
               Console.Write("---");
               for (int i = 0; i < cities; i++)
                   Console.Write("----");


               Console.WriteLine();
               for (int i = 0; i < cities; i++)
               {
                   if (i < 9)
                       Console.Write("{0}  | ", i + 1);
                   else
                       Console.Write("{0} | ", i + 1);
                   for (int j = 0; j < cities; j++)
                   {
                       if (MatrDistance[i, j] > 9)
                           Console.Write("{0}  ", MatrDistance[i, j]);
                       else
                           Console.Write("{0}   ", MatrDistance[i, j]);
                   }
                   Console.WriteLine();
               }*/

            StartCity = rand.Next(1, cities);
            EndCity = rand.Next(1, cities);
            if (StartCity == EndCity)
                StartCity = rand.Next(1, cities);
            Console.WriteLine("Город-отправитель: {0}, Город-получатель: {1}", StartCity, EndCity);

            osobi = 8;  
            Console.WriteLine("Кол-во особей в популяции: " + osobi);

            geny = 6;  
            Console.WriteLine("Кол-во генов у особи: " + geny);

            sw.Start();


            int[,] Individi = new int[osobi, geny];  //особи
            for (int i = 0; i < osobi; i++)
                for (int j = 0; j < geny; j++)
                    Individi[i, j] = rand.Next(0, cities);

            int[,] allFitnesses = new int[osobi, 2];  //пригодность каждой особи и её номер
            Console.WriteLine();
            Console.WriteLine("Поколение 1:");
            for (int i = 0; i < osobi; i++)
            {
                int fitness = 0;
                fitness += MatrDistance[StartCity - 1, Individi[i, 0]];
                Console.Write("Особь {0}, её гены: ", i + 1);
                for (int j = 0; j < geny; j++)
                {
                    if (Individi[i, j] < 9)
                        Console.Write("{0},  ", Individi[i, j] + 1);
                    else
                        Console.Write("{0}, ", Individi[i, j] + 1);
                    if (j > 0)
                        fitness += MatrDistance[Individi[i, j - 1], Individi[i, j]];
                }
                fitness += MatrDistance[Individi[i, geny - 1], EndCity - 1];
                Console.WriteLine(" Пригодность: " + fitness);
                allFitnesses[i, 0] = i;
                allFitnesses[i, 1] = fitness;
            }
            Console.WriteLine();

            int iterations = 0;
            do
            {
                iterations++;
                List<int[]> newIndividi = new List<int[]>(); //массив из потомства
                do
                {
                    int[] parent_1 = new int[geny];  //родители
                    int[] parent_2 = new int[geny];
                    //выбор родителей 
                    ViborParents(Individi, allFitnesses, ref parent_1, ref parent_2);

                    int[] child_1 = new int[geny];  //потомоки
                    int[] child_2 = new int[geny];
                    //скрещивание 
                    Crossing(parent_1, parent_2, ref child_1, ref child_2);

                    newIndividi.Add(child_1);
                    newIndividi.Add(child_2);
                } while (newIndividi.Count < osobi);  //формируем новую популяцию

                Mutation(ref newIndividi);  //мутация обменом соседних генов

                for (int i = 0; i < osobi; i++)  //объединение родителей и потомков
                {
                    newIndividi.Add(new int[geny]);
                    for (int j = 0; j < geny; j++)
                        newIndividi.Last()[j] = Individi[i, j];
                }

                Selection(MatrDistance, ref newIndividi);  //отбор особей в новое поколение методом усечения

                Console.WriteLine("Поколение {0}:", iterations + 1);
                OutputPopulation(MatrDistance, ref allFitnesses, newIndividi);

                int counter = 0;
                for (int i = 1; i < osobi; i++)  //подсчёт кол-ва особей с одинаковой пригодностью для остановки алгоритма
                    if (allFitnesses[i, 1] == allFitnesses[0, 1])
                        counter++;
                if (counter >= osobi * 0.75)
                    break;

                for (int i = 0; i < osobi; i++)
                    for (int j = 0; j < geny; j++)
                        Individi[i, j] = newIndividi[i][j];
            } while (iterations < 50);

            Console.WriteLine("Кол-во поколений: " + (iterations + 1));
            Console.Write("Найденный путь: " + StartCity);
            for (int i = 0; i < geny; i++)
                Console.Write(" -> " + (Individi[0, i] + 1));
            Console.WriteLine(" -> {0}, его длина: " + allFitnesses[0, 1], EndCity);
            Console.WriteLine("Длина прямого пути от {0} города к {1}: " + MatrDistance[StartCity - 1, EndCity - 1], StartCity, EndCity);
            


            sw.Stop();
            Console.WriteLine($"Времени потрачено - {sw.ElapsedMilliseconds} млс");
            Console.ReadLine();
        }

        static void Selection(int[,] MatrDistance, ref List<int[]> Individi)  //отбор усечением
        {
            Random rand = new Random();
            double T = 0.4;  //порог для отбора
            int fitness = 0;
            int[,] allFitnesses = new int[Individi.Count, 2];  //пригодность каждой особи и её номер
            for (int i = 0; i < Individi.Count; i++)  //вычисление пригодности каждой особи
            {
                fitness += MatrDistance[StartCity - 1, Individi[i][0]];
                for (int j = 1; j < geny; j++)
                    fitness += MatrDistance[Individi[i][j - 1], Individi[i][j]];
                fitness += MatrDistance[Individi[i][geny - 1], EndCity - 1];
                allFitnesses[i, 0] = i;
                allFitnesses[i, 1] = fitness;
                fitness = 0;
            }

            int[] temp = new int[Individi.Count];  //сортировка особей в порядке убывания их пригодности
            for (int i = 0; i < Individi.Count; i++)
                temp[i] = allFitnesses[i, 1];
            Array.Sort(temp);
            for (int i = 0; i < Individi.Count; i++)
                for (int j = 0; j < Individi.Count; j++)
                    if (temp[i] == allFitnesses[j, 1])
                    {
                        Swap(ref allFitnesses[i, 1], ref allFitnesses[j, 1]);
                        Swap(ref allFitnesses[i, 0], ref allFitnesses[j, 0]);
                        break;
                    }

            int countNewIndividi = (int)(Individi.Count * T);  //кол-во особей, прошедших через отбор
            List<int[]> newIndividi = new List<int[]>();
            int numberNewIndividi = 0;
            do  //отбор особей в новую популяцию
            {
                numberNewIndividi = rand.Next(0, countNewIndividi - 1);
                newIndividi.Add(new int[geny]);
                for (int i = 0; i < geny; i++)
                    newIndividi.Last()[i] = Individi[allFitnesses[numberNewIndividi, 0]][i];
            } while (newIndividi.Count < osobi);
            Individi.Clear();
            Individi = newIndividi;
        }

        static void ViborParents(int[,] Individi, int[,] allFitnesses, ref int[] parent_1, ref int[] parent_2)  //выбор родителей 
        {
            Random rand = new Random();
            int numberParent_1 = rand.Next(0, osobi - 1);  //выбор первого родителя          
            for (int i = 0; i < geny; i++)
                parent_1[i] = Individi[allFitnesses[numberParent_1, 0], i];

            double[] euclidDistance = new double[osobi];
            double mineuclidDistance = double.MaxValue;
            int numberParent_2 = 0;
            for (int i = 0; i < osobi; i++)  //вычисление Евклидова расстояния для выбора второго родителя
            {
                for (int j = 0; j < geny; j++)
                    euclidDistance[i] += Math.Pow(parent_1[j] - Individi[allFitnesses[i, 0], j], 2);
                euclidDistance[i] = Math.Sqrt(euclidDistance[i]);
                if (euclidDistance[i] < mineuclidDistance && euclidDistance[i] != 0)
                {
                    mineuclidDistance = euclidDistance[i];
                    numberParent_2 = i;
                }
            }
            for (int i = 0; i < geny; i++)
                parent_2[i] = Individi[allFitnesses[numberParent_2, 0], i];
        }

        static void Crossing(int[] parent_1, int[] parent_2, ref int[] child_1, ref int[] child_2)  //скрещивание методом дискретной рекомбинации
        {
            Random rand = new Random();
            int[,] mask_for_cross = new int[2, geny];  //маска для замены генов
            for (int i = 0; i < 2; i++)    //выбираем номера особи для замены генов
                for (int j = 0; j < geny; j++)
                    mask_for_cross[i, j] = rand.Next(0, 2);

            for (int i = 0; i < geny; i++)
            {
                if (mask_for_cross[0, i] == 1)  //замена генов для первого потомка
                    child_1[i] = parent_2[i];
                else
                    child_1[i] = parent_1[i];

                if (mask_for_cross[1, i] == 0)   //замена генов для второго потомка
                    child_2[i] = parent_1[i];
                else
                    child_2[i] = parent_2[i];
            }
        }

        static void Mutation(ref List<int[]> newIndividi)  //мутация методом обмена соседних генов
        {
            Random rand = new Random();
            double T = 0.3;  //вероятность мутации - порог
            double[] veroyatnost_mutation = new double[newIndividi.Count];
            for (int i = 0; i < newIndividi.Count; i++)
            {
                veroyatnost_mutation[i] = ((double)rand.Next(1, 100) / 100);  //случайно выбирается вероятность мутации для каждой особи
                if (veroyatnost_mutation[i] <= T)  //если вероятность мутации особи меньше порога
                {
                    int numOFgen = rand.Next(1, geny - 1);  //номер гена для мутации                    
                    if (newIndividi[i][numOFgen - 1] == newIndividi[i][numOFgen + 1])
                        numOFgen = rand.Next(0, geny - 1);
                    if (numOFgen == 0)
                        numOFgen++;
                    Swap(ref newIndividi[i][numOFgen - 1], ref newIndividi[i][numOFgen + 1]);
                }
            }
        }

        static void OutputPopulation(int[,] MatrDistance, ref int[,] allFitnesses, List<int[]> newIndividi)
        {
            int fitness = 0;
            for (int i = 0; i < osobi; i++)
            {
                fitness += MatrDistance[StartCity - 1, newIndividi[i][0]];
                Console.Write("Особь {0}, её гены: ", i + 1);
                for (int j = 0; j < geny; j++)
                {
                    if (newIndividi[i][j] < 9)
                        Console.Write("{0},  ", newIndividi[i][j] + 1);
                    else
                        Console.Write("{0}, ", newIndividi[i][j] + 1);
                    if (j > 0)
                        fitness += MatrDistance[newIndividi[i][j - 1], newIndividi[i][j]];
                }
                fitness += MatrDistance[newIndividi[i][geny - 1], EndCity - 1];
                Console.WriteLine(" пригодность: " + fitness);
                allFitnesses[i, 0] = i;
                allFitnesses[i, 1] = fitness;
                fitness = 0;
            }
            Console.WriteLine();
        }

        static void Swap<T>(ref T a, ref T b)
        {
            T temp = a;
            a = b;
            b = temp;
        }
    }
}
