using System;
using System.Drawing;
using System.Windows.Forms;
using SimpleDrawing.Diagrams;

namespace SimpleDrawing
{
    public partial class frmDiagram : Form
    {
        private readonly Diagram item;

        public frmDiagram(ICloneable source)
        {
            item = (Diagram)source.Clone();
            InitializeComponent();
            item.SetRectangle(new Rectangle(0, 0, pbGraphic.Width, pbGraphic.Height));
        }

        private void pbGraphic_Paint(object sender, PaintEventArgs e)
        {
            item.Draw(sender, e.Graphics);
        }

        private void frmDiagram_Resize(object sender, EventArgs e)
        {
            item.SetRectangle(new Rectangle(0, 0, pbGraphic.Width, pbGraphic.Height));
            pbGraphic.Refresh();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pbSaveDlg.ShowDialog(this) == DialogResult.OK)
            {
                var img = new Bitmap(pbGraphic.Width, pbGraphic.Height, pbGraphic.CreateGraphics());
                using (var g = Graphics.FromImage(img))
                {
                    item.Draw(sender, g);
                }

                img.Save(pbSaveDlg.FileName, System.Drawing.Imaging.ImageFormat.Jpeg);
            }
        }
    }
}
