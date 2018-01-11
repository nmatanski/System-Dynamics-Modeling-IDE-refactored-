using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace SimpleDrawing.Diagrams
{
    [Serializable]
    public class Diagram : IDrawable, ITransformable, IEditable, IResizable, ICloneable
    {
        #region Fields

        private const string ObjectName = "Diagram ";

        private static int _objectCount;

        private Rectangle rectangle;

        private List<ChartObject> items;

        private Size minSize = new Size(180, 120);  // unused

        private int cells = 6;
        private double? max;
        private bool shouldRecalculate = true;

        #endregion

        #region Properties

        public string Name { get; private set; }

        #endregion

        #region Constructors

        public Diagram()
        {
            _objectCount++;
            Name = ObjectName + _objectCount;
        }

        public Diagram(Rectangle rect) : this()
        {
            rectangle = rect;
        }

        public Diagram(Diagram source)
        {
            rectangle = source.rectangle;
            Name = source.Name;
            items = source.items;
            cells = source.cells;
            max = source.max;
            shouldRecalculate = source.shouldRecalculate;
        }

        #endregion

        #region IEditable Members

        public object this[string key]
        {
            get
            {
                if (key == "NAME")
                    return Name;
                return null;
            }

            set
            {
                if (key == "NAME") Name = Convert.ToString(value);
            }
        }

        public List<PropertyDescriptor> GetEditableProperties()
        {
            return new List<PropertyDescriptor>
            {
                new PropertyDescriptor("Name:", "NAME", typeof(string))
            };
        }

        #endregion

        #region ITransformable Members

        public void Translate(int dX, int dY, int item)
        {
            if (item != 0) return;
            int newX = dX + rectangle.X;
            int newY = dY + rectangle.Y;
            rectangle = new Rectangle(newX, newY, rectangle.Width, rectangle.Height);
            shouldRecalculate = true;
        }

        public int Contains(int X, int Y)
        {
            int rectArea = 4; // px

            var t = new Rectangle(rectangle.Location, new Size(rectangle.Width, rectArea));
            var r = new Rectangle(new Point(rectangle.Right, rectangle.Top), new Size(rectArea, rectangle.Height));
            var d = new Rectangle(new Point(rectangle.Left, rectangle.Bottom - rectArea), new Size(rectangle.Width, rectArea));
            var l = new Rectangle(rectangle.Location, new Size(rectArea, rectangle.Height));

            var tl = new Rectangle(rectangle.Location, new Size(rectArea, rectArea));
            var tr = new Rectangle(new Point(rectangle.Right - rectArea, rectangle.Top), new Size(rectArea, rectArea));
            var br = new Rectangle(new Point(rectangle.Right - rectArea, rectangle.Bottom - rectArea), new Size(rectArea, rectArea));
            var bl = new Rectangle(new Point(rectangle.Left, rectangle.Bottom - rectArea), new Size(rectArea, rectArea));

            if (tl.Contains(X, Y))
            {
                return 1;
            }

            if (tr.Contains(X, Y))
            {
                return 2;
            }

            if (br.Contains(X, Y))
            {
                return 3;
            }

            if (bl.Contains(X, Y))
            {
                return 4;
            }

            if (t.Contains(X, Y))
            {
                return 5;
            }

            if (r.Contains(X, Y))
            {
                return 6;
            }

            if (d.Contains(X, Y))
            {
                return 7;
            }

            if (l.Contains(X, Y))
            {
                return 8;
            }

            return rectangle.Contains(X, Y) ? 0 : -1;
        }

        public Rectangle Bounds
        {
            get
            {
                return rectangle;
            }
        }

        #endregion

        #region IDrawable Members

        public void Draw(object sender, Graphics g)
        {/*
            int X ,Y;
            X = Y = this._margin;
            int Count = 0;
            int flows = 0;
            
            if (this._values != null)
            {
                flows = this._values[this._values.Keys.First()].Count;
                Count = this._values.Keys.Count;
            }

            using (Pen pen = new Pen(Color.Black, 2))
            {
                
                g.FillRectangle(Brushes.Green, _rect);
                g.DrawRectangle(pen, _rect);

                while (X < _rect.Width - _margin*2)
                {
                    g.FillRectangle(Brushes.Black, new Rectangle(_rect.Left + X, _rect.Bottom - _margin, 2, 2));
                    X += _stepX;
                }
                while (Y < _rect.Height - _margin*2)
                {
                    g.FillRectangle(Brushes.Black, new Rectangle(_rect.Left + _margin, (_rect.Bottom - _margin) - Y, 2, 2));
                    Y += _stepY;
                }
                
                int height = this._rect.Height - _margin *2;
                int width = this._rect.Width - _margin *2;
                double py = 0;

                if (this._values != null)
                {
                    foreach (string key in this._values.Keys)
                    {
                        Point[] pts = new Point[flows];
                        if (this._maxYVal == 0.0) break;
                        for (int f = 0; f < flows; f++)
                        {
                            py = (this._values[key][f] / this._maxYVal) * 100;
                            py = (((double)height) / 100) * py;
                            pts[f] = new Point(this._rect.Left + f * _stepX + _margin, this._rect.Bottom - _margin - (int)py);
                        }
                        g.DrawLines(pen, pts);
                    }
                }
            }*/
            DrawBackground(g);
        }

        #endregion

        #region Functions

        public void PopulateDiagram(List<Dictionary<string, List<double>>> source)
        {
            items = new List<ChartObject>();
            if (source == null || source.Count == 0) return;
            var rand = new Random();

            foreach (var dictionary in source)
            {
                foreach (var key in dictionary.Keys)
                {
                    var chartObject = new ChartObject(key, dictionary[key])
                    {
                        Color = Color.FromArgb(rand.Next(254), rand.Next(254), rand.Next(254))
                    };
                    items.Add(chartObject);
                }
            }

            double min = 0, max = 0;
            MinMaxValue(out min, out max);
            this.max = Math.Max(Math.Abs(min), Math.Abs(max));
        }

        private void MinMaxValue(out double min, out double max)
        {
            if (items == null)
            {
                min = max = 0;
            }
            else
            {
                min = max = 0;
                foreach (var chartObject in items)
                {
                    min = min < chartObject.Values.Min() ? min : chartObject.Values.Min();
                    max = max > chartObject.Values.Max() ? max : chartObject.Values.Max();
                }
            }
        }

        private static bool IsCorrectSize(Rectangle rect)
        {
            // return (rectangle.Width > _minSize.Width && rectangle.Height > _minSize.Height);
            return true;
        }

        private Point Mid(Rectangle rect)
        {
            return new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
        }

        public void SetRectangle(Rectangle rect)
        {
            rectangle = rect;
            shouldRecalculate = true;
        }

        #endregion

        #region PaintFunctions

        private void DrawBackground(Graphics g)
        {
            var rect = rectangle;
            double min = 0, max = 0;
            rect.Size -= new Size(120, 60);
            if (this.max == null)
            {
                MinMaxValue(out min, out max);
                this.max = Math.Max(Math.Abs(min), Math.Abs(max));
            }

            if (cells == 0) cells = 6;

            int iteration = 10;

            if (items != null)
                iteration = items.Count > 0 ? items[0].Values.Count : 0;

            rect.Location = new Point(rect.X + 100, rect.Y + 40);
            while (rect.Width % cells != 0)
            {
                rect.Width -= 1;
            }

            while (rect.Height % cells != 0)
            {
                rect.Height -= 1;
            }

            var m = Mid(rect);

            var tf = new StringFormat();
            tf.LineAlignment = StringAlignment.Near;
            tf.Alignment = StringAlignment.Center;

            // background
            g.FillRectangle(Brushes.WhiteSmoke, rectangle);
            using (var font = new Font("Tahoma", 8))
            {
                using (var pen = new Pen(Color.Black, 2))
                {
                    g.DrawRectangle(pen, rectangle);
                    if (Name != string.Empty)
                        g.DrawString(Name, font, Brushes.Black, new Point(Mid(rectangle).X, rectangle.Y), tf);

                    g.DrawLine(pen, rect.Location, new Point(rect.X, rect.Y + rect.Height));
                    g.DrawLine(pen, rect.X, m.Y, rect.X + rect.Width, m.Y);
                    using (var pdd = new Pen(Color.Thistle))
                    {
                        pdd.DashStyle = DashStyle.DashDot;

                        tf.LineAlignment = StringAlignment.Near;
                        tf.Alignment = StringAlignment.Center;

                        int i, x;
                        for (i = 1; i <= cells; i++)
                        {
                            x = rect.X + (rect.Width / cells) * i;
                            g.DrawLine(pdd, new Point(x, rect.Y), new Point(x, rect.Y + rect.Height));
                            if (iteration != 0)
                                g.DrawString(Math.Round(i * (iteration / (double)cells), 1).ToString(), font, Brushes.Black, x, rect.Y + rect.Height / 2, tf);
                        }

                        tf.LineAlignment = StringAlignment.Center;
                        tf.Alignment = StringAlignment.Far;

                        int y;
                        for (i = 0; i <= cells * 2; i++)
                        {
                            if (i == cells)
                            {
                                // 0 ?
                                g.DrawString("0", font, Brushes.Black, rect.X, m.Y, tf);
                            }
                            else
                            {
                                x = rect.X;
                                y = rect.Y + (rect.Height / (cells * 2)) * i;
                                g.DrawLine(pdd, new Point(x, y), new Point(x + rect.Width, y));
                                if (this.max != null & this.max.Value != 0)
                                {
                                    var bfr = this.max.Value - ((this.max.Value / cells) * i);
                                    g.DrawString(Math.Round(bfr).ToString(), font, Brushes.Black, new Point(x, y), tf);
                                }
                            }
                        }

                        if (items != null)
                        {
                            y = rect.Y;
                            var getMinMax = true;
                            tf.Alignment = StringAlignment.Near;

                            foreach (var item in items)
                            {
                                if (shouldRecalculate || item.Dots == null)
                                {
                                    if (getMinMax)
                                    {
                                        MinMaxValue(out min, out max);
                                        getMinMax = false;
                                    }

                                    item.Regenerate(rect, min, max);
                                }

                                using (var iPen = new Pen(item.Color))
                                {
                                    using (var iBrush = new SolidBrush(item.Color))
                                    {
                                        var points = item.Dots.ToArray();
                                        g.DrawLines(iPen, points);
                                        foreach (var pt in points)
                                        {
                                            g.FillEllipse(iBrush, pt.X - 2, pt.Y - 2, 3, 3);
                                        }

                                        g.DrawString(item.Name, font, iBrush, rectangle.X + 2, y, tf);
                                        y += 10;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region IResizable Members

        public bool Resize(int x, int y, int index)
        {
            var buffer = rectangle;

            switch (index)
            {
                case 1:
                    buffer.Offset(x, y);
                    buffer.Width += x < 0 ? Math.Abs(x) : -x;
                    buffer.Height += y < 0 ? Math.Abs(y) : -y;
                    break;
                case 2:
                    buffer.Offset(0, y);
                    buffer.Width += x;
                    buffer.Height += y < 0 ? Math.Abs(y) : -y;
                    break;
                case 3:
                    buffer.Width += x;
                    buffer.Height += y;
                    break;
                case 4:
                    buffer.Offset(x, 0);
                    buffer.Width += x < 0 ? Math.Abs(x) : -x;
                    buffer.Height += y;
                    break;
                case 5:
                    buffer.Offset(0, y);
                    buffer.Height += y;
                    break;
                case 6:
                    buffer.Width += x;
                    break;
                case 7:
                    buffer.Height += y;
                    break;
                case 8:
                    buffer.Offset(x, 0);
                    buffer.Width += x < 0 ? Math.Abs(x) : -x;
                    break;
            }

            shouldRecalculate = true;

            if (IsCorrectSize(buffer))
            {
                rectangle = buffer;
                return true;
            }

            return false;
        }

        public Cursor GetCursor(int index)
        {
            switch (index)
            {
                case 1:
                    return Cursors.SizeNWSE;
                case 5:
                    return Cursors.SizeNS;
                case 2:
                    return Cursors.SizeNESW;
                case 6:
                    return Cursors.SizeWE;
                case 3:
                    return Cursors.SizeNWSE;
                case 7:
                    return Cursors.SizeNS;
                case 4:
                    return Cursors.SizeNESW;
                case 8:
                    return Cursors.SizeWE;
                default:
                    return Cursors.Default;
            }
        }

        #endregion

        #region ICloneable Members

        public object Clone()
        {
            return new Diagram(this);
        }

        #endregion
    }
}
