using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SimpleDrawing
{
    public partial class frmEditIEditable : Form
    {
        private readonly List<Control> controls;
        private readonly IEditable item;
        private readonly frmMain main;

        public frmEditIEditable(IEditable item, frmMain main)
        {
            InitializeComponent();

            controls = new List<Control>();
            this.item = item;
            this.main = main;

            int height = 0;
            int panelHeight = 28;

            foreach (var editableProperty in this.item.GetEditableProperties())
            {
                var pnl = new Panel
                {
                    Parent = this,
                    Dock = DockStyle.Top,
                    Padding = new Padding(6),
                    Height = panelHeight
                };

                var lbl = new Label
                {
                    // unused
                    Parent = pnl,
                    AutoSize = false,
                    Location = new Point(6, 6),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Size = new Size(100, 21),
                    Text = editableProperty.Caption
                };

                var tb = new TextBox
                {
                    Parent = pnl,
                    Location = new Point(110, 6),
                    Width = 260,
                    Text = Convert.ToString(this.item[editableProperty.Key]),
                    Tag = editableProperty
                };
                tb.Validating += Tb_Validating; // CancelEventHandler
                controls.Add(tb);

                height += panelHeight;
            }

            height += panelHeight + 10;

            Width = 394;
            Height = height + pnlFooter.Height;
            if (item is IRenderable)
            {
                Text = "Edit: " + ((IRenderable)item).Name;
            }
            else Text = string.Empty;
        }

        private void Tb_Validating(object sender, CancelEventArgs e)
        {
            if (sender is TextBox)
            {
                var tb = (TextBox)sender;
                var editableProperty = (PropertyDescriptor)tb.Tag;

                if (editableProperty.Type.IsAssignableFrom(typeof(string)))
                {
                    e.Cancel = tb.Text == string.Empty;
                    if (e.Cancel) errorProvider.SetError(tb, editableProperty.Caption + " is required");
                }
                else if (editableProperty.Type.IsAssignableFrom(typeof(decimal)))
                {
                    decimal test;
                    e.Cancel = !decimal.TryParse(tb.Text, out test);
                    if (e.Cancel) errorProvider.SetError(tb, editableProperty.Caption + " is not Decimal");
                }
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            errorProvider.Clear();
            if (!ValidateChildren())
                return;

            foreach (var ctrl in controls)
            {
                if (ctrl is TextBox)
                {
                    var tb = (TextBox)ctrl;
                    var editableProperty = (PropertyDescriptor)tb.Tag;

                    item[editableProperty.Key] = tb.Text;
                }
            }

            DialogResult = DialogResult.OK;
            main.Refresh();
        }
    }
}
