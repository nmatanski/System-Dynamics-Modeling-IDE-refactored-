using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using SimpleDrawing.Diagrams;

namespace SimpleDrawing
{
    public partial class frmMain : Form
    {
        #region Fields

        private readonly IConnectable start = null;

        private List<IDrawable> items = new List<IDrawable>();

        private int selectedIndex = -1;
        private int selectedItem = -1;
        private int cursorItem = -1;

        private int x;
        private int y;

        private Point? startPoint = null;

        private Point mousePoint;

        private Flow flow = null;

        private ITransformable reference;

        private IDrawable menuItem;

        private List<Dictionary<string, List<double>>> values;

        #endregion

        #region Constructors

        public frmMain()
        {
            InitializeComponent();
        }

        #endregion

        #region Simulation

        private void Run(double step, int time)
        {
            var equation = new StringBuilder(string.Empty);
            foreach (var item in items)
            {
                if (item is Stock)
                {
                    var stock = (Stock)item;
                    equation.Append(stock.Render() + '\n');
                }

                if (item is Flow)
                {
                    var flow = (Flow)item;
                    equation.Append(flow.Name + '=' + flow.Formula + '\n');
                }

                if (item is Coefficient)
                {
                    var coefficient = (Coefficient)item;

                    // equation.Append(coefficient.Render() + '\n');
                    equation.Append(coefficient.Name + '=' + coefficient.Formula + '\n');
                }
            }

            if (ValidateInputForEquationSolver(equation.ToString(), step.ToString(), time.ToString(), 1.ToString()))
            {
                values = Simulation.SolveEquation(equation.ToString(), step, time, 1);

                int cnt = 0;

                foreach (var item in values)
                {
                    foreach (var key in item.Keys)
                    {
                        if (cnt < item[key].Count) cnt = item[key].Count;
                    }
                }

                var toRemove = new Dictionary<Dictionary<string, List<double>>, List<string>>();

                foreach (var item in values)
                {
                    var buf = new List<string>();

                    foreach (var kvp in item)
                    {
                        if (item[kvp.Key].Count < cnt)
                        {
                            buf.Add(kvp.Key);
                        }
                    }

                    if (buf.Count > 0) toRemove.Add(item, buf);
                }

                foreach (var kvp in toRemove)
                {
                    foreach (var key in kvp.Value)
                        kvp.Key.Remove(key);
                }
            }
            else
            {
                MessageBox.Show(@"Incorrect input! Check the StartPoint, Step and Time input.");
            }
        }

        #endregion

        #region Event Handlers

        // toolStripMenu Toggle Buttons Behaviour implementation
        private void tsMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem is ToolStripButton)
            {
                foreach (var button in tsMenu.Items)
                    if (button is ToolStripButton)
                        ((ToolStripButton)button).CheckState = CheckState.Unchecked;
                ((ToolStripButton)e.ClickedItem).CheckState = CheckState.Checked;
            }
        }

        private void SetMenuSelectButton()
        {
            foreach (var button in tsMenu.Items)
                if (button is ToolStripButton)
                    ((ToolStripButton)button).CheckState = ((ToolStripButton)button).Text == "Select" ? CheckState.Checked : CheckState.Unchecked;
        }

        // items drawing
        private void pbDashboard_Paint(object sender, PaintEventArgs e)
        {
            foreach (var item in items)
                item.Draw(sender, e.Graphics);

            if (flow != null)
            {
                flow.Draw(sender, e.Graphics);
            }

            if (reference != null)
            {
                e.Graphics.DrawLine(Pens.Blue, Middle(reference.Bounds), mousePoint);
            }
        }

        private ITransformable getItem(Point p)
        {
            foreach (ITransformable item in items)
                if (item.Contains(p.X, p.Y) != -1)
                {
                    return item;
                }

            return null;
        }

        private void MenuDeleteInFlow(object sender, EventArgs e)
        {
            if (menuItem == null) return;
            var fl = menuItem as Flow;
            if (fl != null)
            {
                if (fl.Source != null)
                    fl.SetSourcePoint(Middle(fl.Source.Bounds), false);
            }
            else if (menuItem is Stock)
            {
                if (((Stock)menuItem).InFlow != null)
                    ((Stock)menuItem).InFlow.SetDestinationPoint(Middle(((Stock)menuItem).Bounds), false);
            }

            pbDashboard.Refresh();
        }

        private void MenuDeleteOutFlow(object sender, EventArgs e)
        {
            if (menuItem == null) return;
            var fl = menuItem as Flow;
            if (fl != null)
            {
                if (fl.Destination != null)
                    fl.SetDestinationPoint(Middle(fl.Destination.Bounds), false);
            }
            else if (menuItem is Stock)
            {
                if (((Stock)menuItem).OutFlow != null)
                    ((Stock)menuItem).OutFlow.SetSourcePoint(Middle(((Stock)menuItem).Bounds), false);
            }

            pbDashboard.Refresh();
        }

        private void MenuDeleteReference(object sender, EventArgs e)
        {
            if (menuItem == null) return;

            foreach (var item in items)
            {
                if (item is IConnectable)
                {
                    ((IConnectable)item).References.Remove((ITransformable)menuItem);
                }
            }

            pbDashboard.Refresh();
        }

        private void MenuDeleteObject(object sender, EventArgs e)
        {
            if (menuItem == null) return;

            MenuDeleteReference(sender, e);

            if (menuItem is Stock)
            {
                MenuDeleteOutFlow(sender, e);
                MenuDeleteInFlow(sender, e);
            }

            items.Remove(menuItem);
            pbDashboard.Refresh();
        }

        private void MenuEditObject(object sender, EventArgs e)
        {
            if (menuItem == null) return;

            if (menuItem is IEditable)
            {
                var editor = new frmEditIEditable((IEditable)menuItem, this);
                editor.ShowDialog();
            }

            pbDashboard.Refresh();
        }

        private void MenuDiagramForm(object sender, EventArgs e)
        {
            if (menuItem == null) return;

            if (menuItem is Diagram)
            {
                var dForm = new frmDiagram((Diagram)menuItem);
                dForm.ShowDialog();
            }

            pbDashboard.Refresh();
        }

        // items manupulation
        private void pbDashboard_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                pbMenu.Items.Clear();
                var pt = new Point(e.X, e.Y);
                menuItem = getItem(pt) as IDrawable;
                if (menuItem == null) return;

                if (menuItem is Stock)
                {
                    var stock = menuItem as Stock;
                    if (stock.InFlow != null)
                    {
                        pbMenu.Items.Add("Detach In Flow: " + stock.InFlow.Name);
                        pbMenu.Items[pbMenu.Items.Count - 1].Click += MenuDeleteInFlow;
                    }

                    if (stock.OutFlow != null)
                    {
                        pbMenu.Items.Add("Detach Out Flow: " + stock.OutFlow.Name);
                        pbMenu.Items[pbMenu.Items.Count - 1].Click += MenuDeleteOutFlow;
                    }

                    if (stock.References.Count > 0)
                    {
                        pbMenu.Items.Add("Detach References");
                        pbMenu.Items[pbMenu.Items.Count - 1].Click += MenuDeleteReference;
                    }

                    pbMenu.Items.Add("Edit Object");
                    pbMenu.Items[pbMenu.Items.Count - 1].Click += MenuEditObject;

                    if (pbMenu.Items.Count > 0) pbMenu.Items.Add("-");

                    pbMenu.Items.Add("Delete " + stock.Name);
                    pbMenu.Items[pbMenu.Items.Count - 1].Click += MenuDeleteObject;
                }
                else if (menuItem is Flow)
                {
                    var fl = menuItem as Flow;
                    if (fl.Source != null)
                    {
                        pbMenu.Items.Add("Detach Flow: " + fl.Source.Name);
                        pbMenu.Items[pbMenu.Items.Count - 1].Click += MenuDeleteInFlow;
                    }

                    if (fl.Destination != null)
                    {
                        pbMenu.Items.Add("Detach Flow: " + fl.Destination.Name);
                        pbMenu.Items[pbMenu.Items.Count - 1].Click += MenuDeleteOutFlow;
                    }

                    if (fl.References.Count > 0)
                    {
                        pbMenu.Items.Add("Detach References");
                        pbMenu.Items[pbMenu.Items.Count - 1].Click += MenuDeleteReference;
                    }

                    pbMenu.Items.Add("Edit Object");
                    pbMenu.Items[pbMenu.Items.Count - 1].Click += MenuEditObject;

                    if (pbMenu.Items.Count > 0) pbMenu.Items.Add("-");

                    pbMenu.Items.Add("Delete " + fl.Name);
                    pbMenu.Items[pbMenu.Items.Count - 1].Click += MenuDeleteObject;
                }
                else if (menuItem is Coefficient)
                {
                    var fl = menuItem as Coefficient;
                    if (fl.References.Count > 0)
                    {
                        pbMenu.Items.Add("Detach References");
                        pbMenu.Items[pbMenu.Items.Count - 1].Click += MenuDeleteReference;
                    }

                    pbMenu.Items.Add("Edit Object");
                    pbMenu.Items[pbMenu.Items.Count - 1].Click += MenuEditObject;

                    if (pbMenu.Items.Count > 0) pbMenu.Items.Add("-");

                    pbMenu.Items.Add("Delete " + fl.Name);
                    pbMenu.Items[pbMenu.Items.Count - 1].Click += MenuDeleteObject;
                }
                else if (menuItem is ResultTable)
                {
                    var fl = menuItem as ResultTable;
                    pbMenu.Items.Add("Edit Object");
                    pbMenu.Items[pbMenu.Items.Count - 1].Click += MenuEditObject;

                    if (pbMenu.Items.Count > 0) pbMenu.Items.Add("-");

                    pbMenu.Items.Add("Delete " + fl.Name);
                    pbMenu.Items[pbMenu.Items.Count - 1].Click += MenuDeleteObject;
                }
                else if (menuItem is Diagram)
                {
                    var fl = menuItem as Diagram;
                    pbMenu.Items.Add("Show in new form");
                    pbMenu.Items[pbMenu.Items.Count - 1].Click += MenuDiagramForm;

                    pbMenu.Items.Add("Edit Object");
                    pbMenu.Items[pbMenu.Items.Count - 1].Click += MenuEditObject;

                    if (pbMenu.Items.Count > 0) pbMenu.Items.Add("-");

                    pbMenu.Items.Add("Delete " + fl.Name);
                    pbMenu.Items[pbMenu.Items.Count - 1].Click += MenuDeleteObject;
                }

                pbMenu.Show(pbDashboard, pt);
            }

            if (e.Button != MouseButtons.Left)
                return;

            Cursor.Clip = new Rectangle(PointToScreen(pbDashboard.Location), pbDashboard.Size);

            pbDashboard.Capture = true;

            if (btnStock.CheckState == CheckState.Checked)
            {
                items.Add(new Stock(new Rectangle(e.X - 50, e.Y - 25, 100, 50)));
                pbDashboard.Refresh();
                SetMenuSelectButton();
            }
            else if (tsbDiagram.CheckState == CheckState.Checked)
            {
                items.Add(new Diagram(new Rectangle(e.X - 250, e.Y - 125, 500, 250)));
                pbDashboard.Refresh();
                SetMenuSelectButton();
            }
            else if (tsbResultTable.CheckState == CheckState.Checked)
            {
                items.Add(new ResultTable(pbDashboard, new Rectangle(e.X - 150, e.Y - 100, 300, 200)));
                pbDashboard.Refresh();
                SetMenuSelectButton();
            }
            else if (btnFlow.CheckState == CheckState.Checked)
            {
                if (startPoint == null)
                {
                    startPoint = new Point(e.X, e.Y);
                    var sItem = getItem(startPoint.Value);
                    var rect = new Rectangle(startPoint.Value.X, startPoint.Value.Y, 10, 10);

                    if (sItem is Stock && ((Stock)sItem).InFlow == null)
                    {
                        flow = new Flow((Stock)sItem, rect);
                        ((Stock)sItem).OutFlow = flow;
                    }
                    else
                    {
                        flow = new Flow(rect, new Rectangle(e.X, e.Y, 10, 10));
                    }
                }
                else
                {
                    var EndPoint = new Point(e.X, e.Y);
                    var sItem = getItem(startPoint.Value);  // unused
                    var eItem = getItem(EndPoint);   // unused

                    items.Add(flow);
                    pbDashboard.Refresh();

                    startPoint = null;
                    flow = null;
                    SetMenuSelectButton();
                }
            }
            else if (btnCoefficient.CheckState == CheckState.Checked)
            {
                items.Add(new Coefficient(new Rectangle(e.X - 25, e.Y - 25, 50, 50)));
                pbDashboard.Refresh();
                SetMenuSelectButton();
            }
            else if (btnRefference.CheckState == CheckState.Checked)
            {
                if (reference == null)
                {
                    reference = getItem(new Point(e.X, e.Y));
                    if (reference is Diagram || reference is ResultTable || !(reference is IConnectable)) reference = null;
                    if (reference == null) return;
                }
                else
                {
                    var cnt = getItem(new Point(e.X, e.Y)) as IConnectable;
                    if (cnt == null || cnt == reference)
                    {
                        reference = null;
                        SetMenuSelectButton();
                        return;
                    }

                    cnt.Connect((IConnectable)reference);
                    reference = null;
                    SetMenuSelectButton();
                }

                /*
                foreach (ITransformable item in this.items)
                    if (item.Contains(e.X, e.Y) != -1)
                    {
                        selectedItem = item.Contains(e.X, e.Y);
                        selectedIndex = this.items.IndexOf((IDrawable)item);
                        break;
                    }
                
                if (selectedIndex == -1)
                    this.start = null;
                else if (this.start == null)
                    this.start = (IConnectable)items[selectedIndex];
                else
                {
                    this.start.Connect((IConnectable)items[selectedIndex]);
                    this.start = null;
                }

                this.selectedIndex = -1;
                 */
                pbDashboard.Refresh();
            }
            else if (btnSelect.CheckState == CheckState.Checked)
            {
                foreach (ITransformable item in items)
                    if (item.Contains(e.X, e.Y) != -1)
                    {
                        selectedItem = item.Contains(e.X, e.Y);
                        cursorItem = selectedIndex;
                        selectedIndex = items.IndexOf((IDrawable)item);
                        if (item is ResultTable)
                        {
                            ((ResultTable)item).BeginDrag();
                        }

                        break;
                    }

                x = e.X;
                y = e.Y;
            }
        }

        private void pbDashboard_MouseMove(object sender, MouseEventArgs e)
        {
            mousePoint = new Point(e.X, e.Y);
            if (btnRefference.CheckState == CheckState.Checked)
            {
                // DestinationRefferencePt = new Point(e.X, e.Y);
                pbDashboard.Refresh();
            }

            if (selectedIndex != -1)
            {
                var dX = e.X - x;
                var dY = e.Y - y;
                x = e.X;
                y = e.Y;
                if (items[selectedIndex] is IResizable && selectedItem != 0)
                {
                    if (!((IResizable)items[selectedIndex]).Resize(dX, dY, selectedItem))
                    {
                        Cursor.Position = mousePoint;
                    }
                }

                if (items[selectedIndex] is Flow)
                {
                    var fl = (Flow)items[selectedIndex];

                    var itm = GetItemEx(e.X, e.Y, items[selectedIndex]);

                    switch (selectedItem)
                    {
                        case 2:
                            if (itm is Stock && fl.Source != itm && ((Stock)itm).InFlow == null)
                            {
                                if (fl.Source == null)
                                    fl.SetDestinationStock((Stock)itm, false);
                                else
                                    fl.SetDestinationPoint(mousePoint, false);
                            }
                            else
                            {
                                if (itm == null || fl.Destination != itm)
                                    fl.SetDestinationPoint(mousePoint, false);
                            }

                            pbDashboard.Refresh();

                            return;
                        case 0:
                            if (itm is Stock && fl.Destination != itm && ((Stock)itm).OutFlow == null)
                            {
                                if (fl.Destination == null)
                                    fl.SetSourceStock((Stock)itm, false);
                                else
                                    fl.SetSourcePoint(mousePoint, false);
                            }
                            else
                            {
                                if (itm == null || fl.Source != itm)
                                    fl.SetSourcePoint(mousePoint, false);
                            }

                            pbDashboard.Refresh();

                            return;
                    }

                }

                ((ITransformable)items[selectedIndex]).Translate(dX, dY, selectedItem);
                pbDashboard.Refresh();
            }
            else if (start != null)
            {
                pbDashboard.Refresh();
                var g = pbDashboard.CreateGraphics();
                var sRect = ((ITransformable)start).Bounds;
                var sPoint = new Point(sRect.X + sRect.Width / 2, sRect.Y + sRect.Height / 2);
                g.DrawLine(new Pen(Color.Red), sPoint, new Point(e.X, e.Y));
            }
            else if (startPoint != null)
            {

                var item = getItem(mousePoint);
                if (item is Stock && flow.Source != item && ((Stock)item).InFlow == null)
                {
                    flow.SetDestinationStock((Stock)item, true);
                    ((Stock)item).InFlow = flow;
                    pbDashboard.Refresh();
                }
                else
                {
                    if (item == null)
                    {
                        flow.SetDestinationPoint(mousePoint, true);
                        if (flow.Destination != null)
                        {
                            flow.Destination.OutFlow = null;
                            flow.Destination = null;
                        }
                    }
                    else
                    {
                        if (item != flow.Destination) flow.SetDestinationPoint(mousePoint, true);
                    }

                    pbDashboard.Refresh();
                }
            }

            if (cursorItem == -1 && btnSelect.CheckState == CheckState.Checked)
            {
                while (true)
                {
                    var item = getItem(new Point(e.X, e.Y));
                    if (!(item is IResizable)) break;

                    int cnt = item.Contains(e.X, e.Y);
                    if (cnt < 1) break;
                    pbDashboard.Cursor = ((IResizable)item).GetCursor(cnt);
                    return;
                }

                pbDashboard.Cursor = Cursors.Default;
                var it = getItem(new Point(e.X, e.Y)) as IRenderable;
                if (it != null)
                {
                    if (sbLabel1.Tag == it) return;
                    sbLabel1.Text = it.GetHint();
                    sbLabel1.Tag = it;
                }
                else
                {
                    if (sbLabel1.Tag == null) return;
                    sbLabel1.Tag = null;
                    sbLabel1.Text = string.Empty;
                }
            }
            else
            {
                pbDashboard.Cursor = Cursors.Default;
            }
        }

        private IRenderable GetItemEx(int x, int y, IDrawable drawable)
        {
            foreach (ITransformable item in items)
                if (item is IRenderable && item.Contains(x, y) != -1)
                {
                    if (item != drawable)
                        return (IRenderable)item;
                }

            return null;
        }

        private void pbDashboard_MouseUp(object sender, MouseEventArgs e)
        {
            if (selectedIndex != -1)
            {
                if (items[selectedIndex] is ResultTable)
                {
                    ((ResultTable)items[selectedIndex]).EndDrag();
                    pbDashboard.Refresh();
                }

                selectedIndex = -1;
            }

            pbDashboard.Capture = false;

            Cursor.Clip = Screen.PrimaryScreen.Bounds;
        }

        // Fast keys
        private void frmMain_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.S)
            {
                tsMenu_ItemClicked(tsMenu, new ToolStripItemClickedEventArgs(btnSelect));
            }
        }

        // Edit IEditable item
        private void pbDashboard_DoubleClick(object sender, EventArgs e)
        {
            if (btnSelect.Checked && selectedIndex > -1)
            {
                if (items[selectedIndex] is IEditable)
                {
                    var editor = new frmEditIEditable((IEditable)items[selectedIndex], this);
                    editor.ShowDialog();
                }
            }
        }

        #endregion

        private void BtnRun_Click(object sender, EventArgs e)
        {

        }

        private void UpdateTablesAndDiagrams()
        {
            if (values == null || values.Count == 0) return;

            foreach (var item in items)
            {
                if (item is ResultTable)
                {
                    ((ResultTable)item).PopulateGrid(values);
                }
                else if (item is Diagram)
                {
                    ((Diagram)item).PopulateDiagram(values);
                }
            }
        }

        private void CleanUp()
        {
            foreach (var item in items)
            {
                if (item is ResultTable)
                {
                    ((ResultTable)item).Dispose();
                }
            }

            items = new List<IDrawable>();
        }

        private Point Middle(Rectangle rect)
        {
            return new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
        }

        #region load/save

        private void ItemsSave(string filename)
        {
            using (var str = new FileStream(filename, FileMode.Create))
            {
                var serializer = new BinaryFormatter();
                serializer.Serialize(str, items);
            }
        }

        private void ItemsLoad(string filename)
        {
            CleanUp();
            using (var str = new FileStream(filename, FileMode.Open))
            {
                var deserializer = new BinaryFormatter();
                items = (List<IDrawable>)deserializer.Deserialize(str);
                str.Close();
            }
        }

        private void saveToolStripButton_Click(object sender, EventArgs e)
        {
            if (dlgSave.ShowDialog(this) == DialogResult.OK)
            {
                ItemsSave(dlgSave.FileName);
            }
        }

        private void openToolStripButton_Click(object sender, EventArgs e)
        {
            if (dlgOpen.ShowDialog(this) == DialogResult.OK)
            {
                ItemsLoad(dlgOpen.FileName);
                pbDashboard.Refresh();
            }
        }

        #endregion

        private void newToolStripButton_Click(object sender, EventArgs e)
        {
            CleanUp();
            pbDashboard.Refresh();
        }

        private void pbDashboard_Click(object sender, EventArgs e)
        {

        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void runToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var obj = new DummyObj();
            var fRun = new frmRun(obj);
            if (fRun.ShowDialog() == DialogResult.OK)
            {
                Run(obj.Step, obj.Iteration);
                UpdateTablesAndDiagrams();
                pbDashboard.Refresh();
            }
        }

        private bool ValidateInputForEquationSolver(string equation, string step, string time, string startPoint)
        {
            // for now the string input for the equation is not being checked
            // TODO: validate the equation!!!
            return IsPositiveNumber(step.ToString()) && IsPositiveNumber(time.ToString()) && IsPositiveNumber(startPoint.ToString());
        }

        private bool IsPositiveNumber(string strNumber)
        {
            var objNotPositivePattern = new Regex("[^0-9.]");
            var objPositivePattern = new Regex("^[.][0-9]+$|[0-9]*[.]*[0-9]+$");
            var objTwoDotPattern = new Regex("[0-9]*[.][0-9]*[.][0-9]*");
            return !objNotPositivePattern.IsMatch(strNumber) && objPositivePattern.IsMatch(strNumber) && !objTwoDotPattern.IsMatch(strNumber);
        }

    }
}
