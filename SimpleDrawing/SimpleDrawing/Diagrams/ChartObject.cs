using System;
using System.Collections.Generic;
using System.Drawing;

namespace SimpleDrawing.Diagrams
{
    [Serializable]
    public class ChartObject
    {
        #region Properties

        public List<PointF> Dots { get; set; }

        public Color Color { get; set; }

        public string Name { get; set; }

        public List<double> Values { get; set; }

        #endregion

        #region Constructors

        public ChartObject(string name, List<double> items) : this()
        {
            Values = items;
            Name = name;
        }

        public ChartObject()
        {
        }

        #endregion

        #region Functions

        public void Regenerate(Rectangle rect, double min, double max)
        {
            Dots = new List<PointF>();
            if (Values == null) return;

            float dx = rect.Width / (Values.Count - 1);

            float x = 0;
            float my = rect.Height / 2;  // +rect.Top;
            if (min == 0) min = 0.1;
            if (max == 0) max = 0.1;
            foreach (var value in Values)
            {
                float y;
                if (value > 0)
                {
                    y = my - (float)value / (float)max * 100 * (float)(rect.Height / 2.0 / 100.0);
                }
                else
                {
                    y = my + (float)value / (float)min * 100 * (float)(rect.Height / 2.0 / 100.0);
                }

                Dots.Add(new PointF(dx * x++ + rect.X, y + rect.Y));
            }
        }

        #endregion
    }
}
