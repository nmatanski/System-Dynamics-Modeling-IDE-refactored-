using System;
using System.Collections.Generic;
using System.Drawing;

namespace SimpleDrawing
{
    [Serializable()]
    public class Stock : IDrawable, ITransformable, IConnectable, IEditable, IRenderable
    {
        #region Fields

        private decimal initialValue;

        #endregion

        #region Properties

        public Flow InFlow { get; set; }

        public Flow OutFlow { get; set; }

        public string Name { get; private set; }

        public Rectangle Rectangle { get; set; }

        public List<ITransformable> References { get; set; }

        public Rectangle Bounds
        {
            get
            {
                return Rectangle;
            }
        }

        public object this[string key]
        {
            get
            {
                switch (key)
                {
                    case "NAME":
                        return Name;
                    case "INITIAL_VALUE":
                        return initialValue;
                    default:
                        return null;
                }
            }

            set
            {
                switch (key)
                {
                    case "NAME":
                        Name = Convert.ToString(value);
                        break;
                    case "INITIAL_VALUE":
                        initialValue = Convert.ToDecimal(value);
                        break;
                }
            }
        }

        #endregion

        #region Constructors

        public Stock(Rectangle rectangle)
        {
            Rectangle = rectangle;
            References = new List<ITransformable>();
        }

        #endregion

        #region IDrawable

        public void Draw(object sender, Graphics g)
        {
            using (var pen = new Pen(Color.Black, 2))
            {
                var start = new Point(Rectangle.X + Rectangle.Width / 2, Rectangle.Y + Rectangle.Height / 2);
                foreach (var item in References)
                {
                    var target = item.Bounds;
                    g.DrawLine(pen, start, new Point(target.X + target.Width / 2, target.Y + target.Height / 2));
                }

                g.FillRectangle(Brushes.Green, Rectangle);
                g.DrawRectangle(pen, Rectangle);
                if (Name != null)
                {
                    var tf = new StringFormat();
                    tf.LineAlignment = StringAlignment.Far;
                    tf.Alignment = StringAlignment.Center;

                    using (var font = new Font("Tahoma", 10))
                    {
                        g.DrawString(Name, font, Brushes.Black, Rectangle.X + Rectangle.Width / 2, Rectangle.Y, tf);
                    }
                }
            }
        }

        #endregion

        #region ITransformable

        public void Translate(int dX, int dY, int item)
        {
            if (item == 0)
            {
                int newX = dX + Rectangle.X;
                int newY = dY + Rectangle.Y;
                Rectangle = new Rectangle(newX, newY, Rectangle.Width, Rectangle.Height);
            }
        }

        public int Contains(int x, int y)
        {
            return Rectangle.Contains(x, y) ? 0 : -1;
        }

        #endregion

        #region IConnectable

        public void Connect(IConnectable target)
        {
            References.Add((ITransformable)target);
        }

        #endregion

        #region IEditable

        public List<PropertyDescriptor> GetEditableProperties()
        {
            var editableProperties = new List<PropertyDescriptor>
            {
                new PropertyDescriptor("Name:", "NAME", typeof(string)),
                new PropertyDescriptor("Initial Value:", "INITIAL_VALUE", typeof(decimal))
            };

            return editableProperties;
        }

        #endregion

        #region IRenderable

        public string Render()
        {
            string result = string.Empty;

            if (InFlow == null || OutFlow == null) return result;

            result = GetVariables();
            result += Name + "'=" + "(" + InFlow.Render() + ")" + "-(" + OutFlow.Render() + ")";

            return result;
        }

        public string GetVariables()
        {
            string result = string.Empty;
            result += Name + "=" + initialValue.ToString() + Environment.NewLine;
            result += InFlow.GetVariables() + OutFlow.GetVariables();
            return result;
        }

        public string GetHint()
        {
            return Name + " : " + initialValue;
        }

        #endregion
    }
}