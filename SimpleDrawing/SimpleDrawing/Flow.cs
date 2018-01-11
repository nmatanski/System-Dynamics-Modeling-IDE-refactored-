using System;
using System.Collections.Generic;
using System.Drawing;

namespace SimpleDrawing
{
    [Serializable]
    public class Flow : IDrawable, ITransformable, IConnectable, IEditable, IRenderable
    {
        #region Fields

        private const int RectWidth = 16;
        private const int RectHeight = 16;

        private Rectangle middleRectangle;

        #endregion

        #region Properties

        public Rectangle? StartRect { get; set; }

        public Rectangle? EndRectangle { get; set; }

        public Stock Source { get; set; }

        public Stock Destination { get; set; }

        public string Name { get; private set; }

        public string Formula { get; private set; }

        public List<ITransformable> References { get; set; }

        #endregion

        #region Constructors

        public Flow(Rectangle? startRect, Rectangle? endRectangle)
        {
            StartRect = new Rectangle(startRect.Value.X - RectWidth / 2, startRect.Value.Y - RectHeight / 2, RectWidth, RectHeight);
            EndRectangle = new Rectangle(endRectangle.Value.X - RectWidth / 2, endRectangle.Value.Y - RectHeight / 2, RectWidth, RectHeight);
            int x = Math.Min(startRect.Value.X, endRectangle.Value.X) + (Math.Max(startRect.Value.X, endRectangle.Value.X) - Math.Min(startRect.Value.X, endRectangle.Value.X)) / 2;
            int y = Math.Min(startRect.Value.Y, endRectangle.Value.Y) + (Math.Max(startRect.Value.Y, endRectangle.Value.Y) - Math.Min(startRect.Value.Y, endRectangle.Value.Y)) / 2;
            middleRectangle = new Rectangle(x - RectWidth / 2, y - RectHeight / 2, RectWidth, RectHeight);

            Source = null;
            Destination = null;

            References = new List<ITransformable>();
        }

        public Flow(Rectangle? startRect, Stock destination)
        {
            StartRect = new Rectangle(startRect.Value.X - RectWidth / 2, startRect.Value.Y - RectHeight / 2, RectWidth, RectHeight);
            EndRectangle = null;
            int x = Math.Min(startRect.Value.X, destination.Bounds.X) + (Math.Max(startRect.Value.X, destination.Bounds.X) - Math.Min(startRect.Value.X, destination.Bounds.X)) / 2;
            int y = Math.Min(startRect.Value.Y, destination.Bounds.Y) + (Math.Max(startRect.Value.Y, destination.Bounds.Y) - Math.Min(startRect.Value.Y, destination.Bounds.Y)) / 2;
            middleRectangle = new Rectangle(x - RectWidth / 2, y - RectHeight / 2, RectWidth, RectHeight);

            Source = null;
            Destination = destination;

            References = new List<ITransformable>();
        }

        public Flow(Stock source, Rectangle? endRectangle)
        {
            StartRect = null;
            var point = new Point(endRectangle.Value.X + endRectangle.Value.Width / 2, endRectangle.Value.Y + endRectangle.Value.Height / 2);

            EndRectangle = new Rectangle(endRectangle.Value.X - RectWidth / 2, endRectangle.Value.Y - RectHeight / 2, RectWidth, RectHeight);

            var rect = source.Bounds;
            var sourcePoint = new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);

            int x = Math.Abs(sourcePoint.X + point.X) / 2 - RectWidth / 2;
            int y = Math.Abs(sourcePoint.Y + point.Y) / 2 - RectHeight / 2;

            middleRectangle = new Rectangle(x, y, RectWidth, RectHeight);

            Source = source;
            Destination = null;

            References = new List<ITransformable>();
        }

        public Flow(Stock source, Stock destination)
        {
            StartRect = null;
            EndRectangle = null;

            var endRect = destination.Bounds;
            var point = new Point(endRect.X + endRect.Width / 2, endRect.Y + endRect.Height / 2);
            var rect = source.Bounds;

            EndRectangle = new Rectangle(endRect.X - RectWidth / 2, endRect.Y - RectHeight / 2, RectWidth, RectHeight);

            var sourcePoint = new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);

            int x = (sourcePoint.X + point.X) / 2 - RectWidth / 2;
            int y = (sourcePoint.Y + point.Y) / 2 - RectHeight / 2;

            middleRectangle = new Rectangle(x, y, RectWidth, RectHeight);


            /*
            Point sPoint = new Point(source.GetBounds().x + source.GetBounds().Width / 2, source.GetBounds().y + source.GetBounds().Height);
            Point dPoint = new Point(destination.GetBounds().x + destination.GetBounds().Width / 2, destination.GetBounds().y + destination.GetBounds().Height);

            int x = Math.Min(source.GetBounds().x, destination.GetBounds().x) + (Math.Max(source.GetBounds().x, destination.GetBounds().x) - Math.Min(source.GetBounds().x, destination.GetBounds().x))/2;
            int y = Math.Min(source.GetBounds().y, destination.GetBounds().y) + (Math.Max(source.GetBounds().y, destination.GetBounds().y) - Math.Min(source.GetBounds().y, destination.GetBounds().y))/2;
            this._middleRectangle = new Rectangle(x - _rectWidth / 2, y - _rectHeight / 2, _rectWidth, _rectHeight);
            */
            Source = source;
            Destination = destination;

            References = new List<ITransformable>();
        }

        #endregion

        #region IDrawable

        public void Draw(object sender, Graphics g)
        {
            int rectanglesCount = 0;

            using (var pen = new Pen(Color.Black, 1))
            {
                var start = new Point(middleRectangle.X + middleRectangle.Width / 2, middleRectangle.Y + middleRectangle.Height / 2);

                foreach (var item in References)
                {
                    var target = item.Bounds;
                    g.DrawLine(pen, start, new Point(target.X + target.Width / 2, target.Y + target.Height / 2));
                }

                var rectangles = new Rectangle[6];
                if (StartRect != null)
                {
                    rectangles[rectanglesCount++] = StartRect.Value;
                    g.DrawRectangle(pen, StartRect.Value);
                }

                if (Name != null)
                {
                    var tf = new StringFormat
                    {
                        LineAlignment = StringAlignment.Far,
                        Alignment = StringAlignment.Center
                    };

                    using (var font = new Font("Tahoma", 10))
                    {
                        g.DrawString(Name, font, Brushes.Black, middleRectangle.X + middleRectangle.Width / 2, middleRectangle.Y, tf);
                    }
                }

                g.DrawRectangle(pen, middleRectangle);
                rectangles[rectanglesCount++] = middleRectangle;

                if (EndRectangle != null)
                {
                    rectangles[rectanglesCount++] = EndRectangle.Value;
                    g.DrawRectangle(pen, EndRectangle.Value);
                }

                Point end;
                if (StartRect != null)
                {
                    end = new Point(StartRect.Value.X + StartRect.Value.Width / 2, StartRect.Value.Y + StartRect.Value.Height / 2);
                    rectangles[rectanglesCount++] = StartRect.Value;
                }
                else end = new Point(Source.Bounds.X + Source.Bounds.Width / 2, Source.Bounds.Y + Source.Bounds.Height / 2);

                g.DrawLine(pen, start, end);

                if (EndRectangle != null)
                {
                    end = new Point(EndRectangle.Value.X + EndRectangle.Value.Width / 2, EndRectangle.Value.Y + EndRectangle.Value.Height / 2);
                    rectangles[rectanglesCount++] = EndRectangle.Value;
                }
                else end = new Point(Destination.Bounds.X + Destination.Bounds.Width / 2, Destination.Bounds.Y + Destination.Bounds.Height / 2);

                g.DrawLine(pen, start, end);

                for (int i = 0; i < rectanglesCount; i++)
                {
                    g.FillRectangle(Brushes.Azure, rectangles[i]);
                    g.DrawRectangle(pen, rectangles[i]);
                }
            }
        }

        #endregion

        #region ITransformable

        public void Translate(int dX, int dY, int item)
        {
            switch (item)
            {
                case 0:
                    {
                        int newX = dX + StartRect.Value.X;
                        int newY = dY + StartRect.Value.Y;
                        StartRect = new Rectangle(newX, newY, StartRect.Value.Width, StartRect.Value.Height);
                        break;
                    }

                case 1:
                    {
                        int newX = dX + middleRectangle.X;
                        int newY = dY + middleRectangle.Y;
                        middleRectangle = new Rectangle(newX, newY, middleRectangle.Width, middleRectangle.Height);
                        break;
                    }

                case 2:
                    {
                        int newX = dX + EndRectangle.Value.X;
                        int newY = dY + EndRectangle.Value.Y;
                        EndRectangle = new Rectangle(newX, newY, EndRectangle.Value.Width, EndRectangle.Value.Height);
                        break;
                    }
            }
        }

        public int Contains(int x, int y)
        {
            if ((StartRect != null) && StartRect.Value.Contains(x, y))
                return 0;
            if ((EndRectangle != null) && EndRectangle.Value.Contains(x, y))
                return 2;
            if (middleRectangle.Contains(x, y))
                return 1;
            return -1;
        }

        public Rectangle Bounds
        {
            get
            {
                return middleRectangle;
            }
        }

        #endregion

        #region IConnectable

        public void Connect(IConnectable target)
        {
            References.Add((ITransformable)target);
        }

        #endregion

        #region IEditable

        public object this[string key]
        {
            get
            {
                switch (key)
                {
                    case "NAME":
                        return Name;
                    case "FORMULA":
                        return Formula;
                }
                return null;
            }

            set
            {
                if (key == "NAME") Name = Convert.ToString(value);
                else if (key == "FORMULA")
                    Formula = Convert.ToString(value);
            }
        }

        public List<PropertyDescriptor> GetEditableProperties()
        {
            return new List<PropertyDescriptor>
            {
                new PropertyDescriptor(
                    "Name:",
                    "NAME",
                    typeof(string)),
                new PropertyDescriptor(
                    "Formula:",
                    "FORMULA",
                    typeof(string))
            };
        }

        #endregion

        #region Extended Functions

        public void SetDestinationStock(Stock stock, bool ReCalcRect)
        {
            if (stock == null) return;

            stock.InFlow = this;

            Destination = stock;
            EndRectangle = null;
            if (!ReCalcRect) return;

            var endRect = Destination.Bounds;
            var point = new Point(endRect.X + endRect.Width / 2, endRect.Y + endRect.Height / 2);

            var rect = Source != null ? Source.Bounds: StartRect.Value;

            var sourcePoint = new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);

            var x = (sourcePoint.X + point.X) / 2 - RectWidth / 2;
            var y = (sourcePoint.Y + point.Y) / 2 - RectHeight / 2;

            middleRectangle = new Rectangle(x, y, RectWidth, RectHeight);
        }

        public void SetSourceStock(Stock stock, bool recalculateRect)
        {
            if (stock == null) return;

            stock.OutFlow = this;

            Source = stock;
            StartRect = null;

            if (!recalculateRect) return;

            var rect = stock.Bounds;
            var point = new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);

            var rectangle = Destination != null ? Destination.Bounds: EndRectangle.Value;

            var sPoint = new Point(rectangle.X + rectangle.Width / 2, rectangle.Y + rectangle.Height / 2);

            var x = (sPoint.X + point.X) / 2 - RectWidth / 2;
            var y = (sPoint.Y + point.Y) / 2 - RectHeight / 2;

            middleRectangle = new Rectangle(x, y, RectWidth, RectHeight);
        }

        public void SetDestinationPoint(Point point, bool recalculateRect)
        {
            EndRectangle = new Rectangle(point.X - RectWidth / 2, point.Y - RectHeight / 2, RectWidth, RectHeight);

            if (Destination != null)
            {
                Destination.InFlow = null;
                Destination = null;
            }

            if (!recalculateRect) return;

            Point sourcePoint;
            if (Source != null)
            {
                var rect = Source.Bounds;
                sourcePoint = new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
            }
            else sourcePoint = new Point(StartRect.Value.X + StartRect.Value.Width / 2, StartRect.Value.Y + StartRect.Value.Height / 2);

            int x = Math.Abs(sourcePoint.X + point.X) / 2 - RectWidth / 2;
            int y = Math.Abs(sourcePoint.Y + point.Y) / 2 - RectHeight / 2;

            middleRectangle = new Rectangle(x, y, RectWidth, RectHeight);
        }

        public void SetSourcePoint(Point point, bool ReCalcRect)
        {
            StartRect = new Rectangle(point.X - RectWidth / 2, point.Y - RectHeight / 2, RectWidth, RectHeight);

            if (Source != null)
            {
                Source.OutFlow = null;
                Source = null;
            }

            if (!ReCalcRect) return;

            Point sourcePoint;
            if (Destination != null)
            {
                var rect = Destination.Bounds;
                sourcePoint = new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
            }
            else sourcePoint = new Point(EndRectangle.Value.X + EndRectangle.Value.Width / 2, EndRectangle.Value.Y + EndRectangle.Value.Height / 2);

            int x = Math.Abs(sourcePoint.X + point.X) / 2 - RectWidth / 2;
            int y = Math.Abs(sourcePoint.Y + point.Y) / 2 - RectHeight / 2;

            middleRectangle = new Rectangle(x, y, RectWidth, RectHeight);
        }

        #endregion

        #region IRenderable Members

        public string Render()
        {
            string result = Formula;

            foreach (IRenderable item in References)
            {
                if (item is Stock)
                    continue;
                result = result.Replace(item.Name, '(' + item.Render() + ')');
            }

            return result;
        }

        public string GetVariables()
        {
            string result = string.Empty;

            foreach (var item in References)
            {
                if (item is Stock || !(item is IRenderable))
                    continue;
                result = ((IRenderable)item).GetVariables();
            }

            return result;
        }

        public string GetHint()
        {
            return Name + " : " + Formula;
        }

        #endregion
    }
}
