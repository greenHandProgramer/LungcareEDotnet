using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LungCare.Airway.Layout
{
    public partial class SideBySideLayout : UserControl
    {
        public void SetLeft(Control control)
        {
            control.Visible = true;
            //(control.Parent).Controls.Remove(control);
            panelLeft.Controls.Clear();
            panelLeft.Controls.Add(control);
            control.Dock = DockStyle.Fill;
        }

        public void SetRight(Control control)
        {
            control.Visible = true;
            panelRight.Controls.Clear();
            panelRight.Controls.Add(control);
            control.Dock = DockStyle.Fill;
        }

        public SideBySideLayout()
        {
            InitializeComponent();
        }
    }
}
