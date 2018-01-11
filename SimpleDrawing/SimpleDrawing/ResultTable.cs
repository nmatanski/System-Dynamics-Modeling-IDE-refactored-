using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace SimpleDrawing
{
    [Serializable]
    public class ResultTable : IDrawable, ITransformable, IEditable, IDisposable, IResizable
    {
        #region Fields

        private const int PaddingTop = 24;
        private const int Padding = 8;
        private const string ObjectName = "Result Table ";

        private static int ObjectCount;

        private Rectangle rectangle;

        [NonSerialized]
        private DataGridView grid;

        private DataTable table;
        private int topAdd;
        private bool drag;

        private Size minSize = new Size(180, 120);

        #endregion

        #region Properties

        public string Name { get; private set; }

        public Rectangle Bounds
        {
            get
            {
                return rectangle;
            }
        }

        #endregion

        #region Constructors

        public ResultTable()
        {
            grid = new DataGridView();
            ObjectCount++;
            Name = ObjectName + ObjectCount;
        }

        public ResultTable(Control parent, Rectangle rectangle) : this()
        {
            grid.Parent = parent;
            this.rectangle = rectangle;

            RefreshGridBounds();
            grid.BringToFront();
        }

        #endregion

        #region IEditable Members

        public object this[string key]
        {
            get
            {
                return key == "NAME" ? Name : null;
            }

            set
            {
                if (key == "NAME") Name = Convert.ToString(value);
            }
        }

        public List<PropertyDescriptor> GetEditableProperties()
        {
            return new List<PropertyDescriptor> { new PropertyDescriptor("Name:", "NAME", typeof(string)) };
        }

        #endregion

        #region ITransformable Members

        public void Translate(int dX, int dY, int item)
        {
            if (item == 0)
            {
                int newX = dX + rectangle.X;
                int newY = dY + rectangle.Y;
                rectangle = new Rectangle(newX, newY, rectangle.Width, rectangle.Height);
                RefreshGridBounds();
            }
        }

        public int Contains(int x, int y)
        {
            const int Area = 4; // px  // const?

            var t = new Rectangle(rectangle.Location, new Size(rectangle.Width, Area));
            var r = new Rectangle(new Point(rectangle.Right, rectangle.Top), new Size(Area, rectangle.Height));
            var d = new Rectangle(new Point(rectangle.Left, rectangle.Bottom - Area), new Size(rectangle.Width, Area));
            var l = new Rectangle(rectangle.Location, new Size(Area, rectangle.Height));

            var tl = new Rectangle(rectangle.Location, new Size(Area, Area));
            var tr = new Rectangle(new Point(rectangle.Right - Area, rectangle.Top), new Size(Area, Area));
            var br = new Rectangle(new Point(rectangle.Right - Area, rectangle.Bottom - Area), new Size(Area, Area));
            var bl = new Rectangle(new Point(rectangle.Left, rectangle.Bottom - Area), new Size(Area, Area));

            if (tl.Contains(x, y))
            {
                return 1;
            }

            if (tr.Contains(x, y))
            {
                return 2;
            }

            if (br.Contains(x, y))
            {
                return 3;
            }

            if (bl.Contains(x, y))
            {
                return 4;
            }

            if (t.Contains(x, y))
            {
                return 5;
            }

            if (r.Contains(x, y))
            {
                return 6;
            }

            if (d.Contains(x, y))
            {
                return 7;
            }

            if (l.Contains(x, y))
            {
                return 8;
            }

            return rectangle.Contains(x, y) ? 0 : -1;
        }

        #endregion

        #region IDrawable Members

        public void Draw(object sender, Graphics g)
        {
            var tf = new StringFormat();
            tf.LineAlignment = StringAlignment.Near;
            tf.Alignment = StringAlignment.Center;
            using (var fnt = new Font("Tahoma", 10))
            {
                using (var pen = new Pen(Color.Black, 2))
                {
                    if (!drag)
                        g.FillRectangle(Brushes.Honeydew, rectangle);

                    g.DrawRectangle(pen, rectangle);

                    g.DrawString(Name, fnt, Brushes.Black, rectangle.X + rectangle.Width / 2, rectangle.Y, tf);
                }
            }

            if (grid == null)
            {
                var snd = sender as Control;
                if (snd == null) return;
                UpdateGrid(snd);
            }
        }

        #endregion

        #region Functions

        private void RefreshGridBounds()
        {
            grid.Width = rectangle.Width - Padding * 2;
            grid.Height = rectangle.Height - (Padding + PaddingTop);
            grid.Top = rectangle.Top + PaddingTop + topAdd;
            grid.Left = rectangle.Left + Padding;
        }

        private void UpdateGrid(Control parent)
        {
            grid = new DataGridView();
            grid.Parent = parent.Parent;
            grid.BringToFront();
            topAdd = parent.Top;
            RefreshGridBounds();

            if (table != null)
            {
                grid.DataSource = table;
            }
        }

        public void PopulateGrid(List<Dictionary<string, List<double>>> source)
        {
            table = new DataTable();

            var buff = new List<List<double>>();
            var columns = new List<string>();

            foreach (var item in source)
            {
                foreach (var vls in item)
                {
                    var column = new DataColumn
                    {
                        DataType = typeof(double),
                        ColumnName = vls.Key,
                        ReadOnly = true,
                        Unique = false
                    };
                    table.Columns.Add(column);
                    buff.Add(vls.Value);
                    columns.Add(vls.Key);
                }
            }

            if (buff.Count > 0)
            {
                for (int r = 0; r < buff[0].Count; r++)
                {
                    var row = table.NewRow();
                    for (int ii = 0; ii < columns.Count; ii++)
                    {
                        row[ii] = buff[ii][r];
                    }

                    table.Rows.Add(row);
                }
            }

            grid.DataSource = table;
        }

        public void BeginDrag()
        {
            grid.Hide();
            drag = true;
        }

        public void EndDrag()
        {
            grid.Show();
            drag = false;
        }

        private bool IsCorrectSize(Rectangle rect)
        {
            return rect.Width > minSize.Width && rect.Height > minSize.Height;
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            grid.Dispose();
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
                    buffer.Height -= y;
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

            if (IsCorrectSize(buffer))
            {
                rectangle = buffer;
                RefreshGridBounds();
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
    }
}
