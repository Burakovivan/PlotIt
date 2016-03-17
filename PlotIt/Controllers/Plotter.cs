using System;
using System.Collections.Generic;
using System.Drawing;

namespace PlottingGraphsSystem
{
    public static class Plotter
    {
       
        /// <summary>
        /// Возвращает набор точек графика
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        internal static float[,] GetPointSchema(string expression, float left, float right, float top, float bottom, float freq)
        {
            Func<double, double> func = Аnalyzer.GetFunction(expression);
            List<PointF> tempList = new List<PointF>();
            for (; left <= right; left += freq)
            {
                float y = (float)func(left);
               
                tempList.Add(new PointF(left, y));
            }

            float[,] funcGraph  = new float[tempList.Count,2];
            
            for(int i = 0; i < tempList.Count; ++i)
            {
                funcGraph[i, 0] = tempList[i].X;
                funcGraph[i, 1] = tempList[i].Y;
            }

            return funcGraph;
        }

    }
}
