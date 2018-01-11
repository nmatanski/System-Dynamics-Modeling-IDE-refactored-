using System;
using System.Collections.Generic;
using System.Drawing;

namespace SimpleDrawing
{
    [Serializable()]
    public class Coefficient : IDrawable, ITransformable, IConnectable, IEditable, IRenderable
    {
        #region Fields

        private static int _count;

        private Rectangle rectangle;

        #endregion

        #region Properties

        public string Name { get; private set; }

        public string Formula { get; private set; }

        public List<ITransformable> References { get; set; }   // IConnectable

        public object this[string key]
        {
            // IEditable
            get
            {
                switch (key)
                {
                    case "NAME":
                        return Name;
                    case "FORMULA":
                        return Formula;
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
                    case "FORMULA":
                        Formula = Convert.ToString(value);
                        break;
                }
            }
        }

        #endregion

        #region Constructors

        public Coefficient(Rectangle rect)
        {
            Name = "Coefficient" + ++_count;
            rectangle = rect;
            References = new List<ITransformable>();
        }

        #endregion

        #region IDrawable

        public void Draw(object sender, Graphics g)
        {
            g.FillEllipse(Brushes.Bisque, rectangle);
            g.DrawEllipse(Pens.Black, rectangle);
            if (Name == null) return;
            var tf = new StringFormat
            {
                LineAlignment = StringAlignment.Far,
                Alignment = StringAlignment.Center
            };

            using (var font = new Font("Tahoma", 10))
            {
                g.DrawString(Name, font, Brushes.Black, rectangle.X + rectangle.Width / 2, rectangle.Y, tf);
            }
        }

        #endregion

        #region ITransformable

        public void Translate(int dX, int dY, int item)
        {
            if (item != 0) return;
            int newX = dX + rectangle.X;
            int newY = dY + rectangle.Y;
            rectangle = new Rectangle(newX, newY, rectangle.Width, rectangle.Height);
        }

        public int Contains(int x, int y)
        {
            return rectangle.Contains(x, y) ? 0 : -1;
        }

        public Rectangle Bounds
        {
            get
            {
                return rectangle;
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

        public List<PropertyDescriptor> GetEditableProperties()
        {
            return new List<PropertyDescriptor>
            {
                new PropertyDescriptor("Name:", "NAME", typeof(string)),
                new PropertyDescriptor("Formula:", "FORMULA", typeof(string))
            };
        }

        #endregion

        #region IRenderable

        public string Render()
        {
            return Name;
        }

        public string GetVariables()
        {
            return Name + '=' + Formula + Environment.NewLine;
        }

        public string GetHint()
        {
            return Name + " : " + Formula;
        }

        #endregion
    }
}
