using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlottingGraphsSystem
{
    public static class Plotter
    {
        public static string PlotIt(string expression, string path, Size size)
        {
            Func<double, double> func = Аnalyzer.GetFunction(expression);
            Image Plot = GetImage(func, size.Width, size.Height);

            string savingPath = "";
            try
            {
                savingPath = GenerateNextName(path);
            }
            catch
            {
                savingPath = "1.png";
            }
            Plot.Save(savingPath, ImageFormat.Png);

            return "..\\Pics\\" + savingPath.Split('\\').Last();
        }
        /// <summary>
        /// Возвращает набор точек графика
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        internal static float[,] GetPointSchema(string expression, int width, int height)
        {
            Func<double, double> func = Аnalyzer.GetFunction(expression);


            double freq = 10f;//количество точек в одной условной ед.
            double period = 1 / freq;//растояние между точками в условных единицах коорд. плоск.
            List<PointF> tempList = new List<PointF>();
            float[,] funcGraph = new float[(int)(height * freq), 2];
            int i = 0;

            for (double x = -(width / 2); x < width / 2 && i < funcGraph.GetLength(0); x += period, i++)
            {
                double y = func(x);
                if (y > -(height / 2) && y < height / 2)
                {
                    float xx = (float)(x + width / 2) == 0 ? 0.5f : (float)(x + width / 2);
                    float yy = (float)(height - (y + height / 2)) < 0.5 ? 0.5f : (float)(height - (y + height / 2));

                    tempList.Add(new PointF(xx, yy));
                }
            }
            funcGraph = new float[tempList.Count, 2];

            i = 0;
            foreach(PointF p in tempList)
            {
                funcGraph[i, 0] = p.X;
                funcGraph[i, 1] = p.Y;
                i++;
            }

            return funcGraph;
        }

        private static string GenerateNextName(string path)
        {
            string result = path + "\\Pics";

            string[] dirs = Directory.GetFiles(result);

            if (dirs.Length == 0) return result + "\\1.png";

            int[] picsName = new int[dirs.Length];

            for (int i = 0; i < dirs.Length; ++i)
            {
                dirs[i] = dirs[i].Split('\\').Last().Split('.')[0];
                int.TryParse(dirs[i], out picsName[i]);
            }

            return result + "\\" + (picsName.Max() + 1) + ".png";
        }


        private static Image GetImage(Func<double, double> function, int width, int height)
        {
            float freq = 0.5f;
            Bitmap graph = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(graph);
            g.Clear(Color.White);
            List<PointF> funcGraph = new List<PointF>();

            for (float x = -(width / 2); x < width / 2; x += freq)
            {
                float y = (float)function(x);

                if (y > -(height / 2) && y < height / 2)
                {
                    funcGraph.Add(new PointF(x + width / 2, height - (y + height / 2)));
                }
            }

            Pen p = new Pen(Color.Black) { Width = 1 };
            g.DrawLines(p, funcGraph.ToArray());
            g.Dispose();

            return graph;
        }

    }
}
