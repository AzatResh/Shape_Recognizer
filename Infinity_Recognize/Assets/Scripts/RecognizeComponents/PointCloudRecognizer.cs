using System;
using System.Collections.Generic;

using UnityEngine;

namespace PDollarGestureRecognizer
{
    public class PointCloudRecognizer
    {

        public static Result Classify(Gesture candidate, Gesture[] trainingSet)
        {
            float minDistance = float.MaxValue;
            string gestureClass = "";
            foreach (Gesture template in trainingSet)
            {
                float dist = GreedyCloudMatch(candidate.Points, template.Points);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    gestureClass = template.Name;
                }
            }

			return gestureClass == "" ? new Result() {GestureClass = "No match", Score = 0.0f} : new Result() {GestureClass = gestureClass, Score = Mathf.Max((minDistance - 2.0f) / -2.0f, 0.0f)};
        }
        

        private static float GreedyCloudMatch(Point[] points1, Point[] points2)
        {
            int n = points1.Length; //количество точек
            float eps = 0.5f;       //от 0 до 1 для расст между точками
            int step = (int)Math.Floor(Math.Pow(n, 1.0f - eps));
            float minDistance = float.MaxValue;
            for (int i = 0; i < n; i += step)
            {
                float dist1 = CloudDistance(points1, points2, i);   // совпадение расстояния points1 c points2
                float dist2 = CloudDistance(points2, points1, i);   // совпадение расстояния points2 c points1
                minDistance = Math.Min(minDistance, Math.Min(dist1, dist2)); //находим меньшее расстояние из этих двух
            }
            return minDistance;
        }


        private static float CloudDistance(Point[] points1, Point[] points2, int startIndex)
        {
            int n = points1.Length;       // длина массива
            bool[] matched = new bool[n]; // проверка была ли отмечена ли точка в массиве
            Array.Clear(matched, 0, n);   // очистка массива (ни один элемент не отмечен)

            float sum = 0;  // сумма расстояний между двуся точками
            int i = startIndex;
            do
            {
                int index = -1; 
                float minDistance = float.MaxValue;
                for(int j = 0; j < n; j++)
                    if (!matched[j])
                    {
                        float dist = Geometry.SqrEuclideanDistance(points1[i], points2[j]);  // рассчет Distance через сумму квадратов для оптимизации
                        if (dist < minDistance)
                        {
                            minDistance = dist;
                            index = j;
                        }
                    }
                matched[index] = true; // индекс точки из второго массива точек сопоставляется с точкой i из первого массива точек
                float weight = 1.0f - ((i - startIndex + n) % n) / (1.0f * n);
                sum += weight * minDistance; // взвешивать каждое расстояние с помощью коэффициента доверия, который уменьшается от 1 до 0
                i = (i + 1) % n;
            } while (i != startIndex);
            return sum;
        }

    }
}