using System;
using System.Windows.Forms;

namespace SimpleDrawing
{
    public partial class frmRun : Form
    {
        private readonly DummyObj obj;

        public frmRun(DummyObj obj)
        {
            this.obj = obj;
            InitializeComponent();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            obj.Step = Convert.ToDouble(tbStep.Text);
            obj.Iteration = Convert.ToInt32(tbIteration.Text);
        }

    }
}
