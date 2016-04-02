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
    public partial class ThreePartView : UserControl
    {
        public void SetLeft(Control control)
        {
            control.Visible = true;
            //(control.Parent).Controls.Remove(control);
            panelLeft.Controls.Clear();
            panelLeft.Controls.Add(control);
            control.Dock = DockStyle.Fill;
        }

        public void Max(Control control)
        {
            Visible = false;
            if (panelLeft.Controls.Contains(control))
            {
                ShowLeftOnly();
            }
            else if (panelMiddle.Controls.Contains(control))
            {
                ShowMiddleOnly();
            }
            else if (panelRight.Controls.Contains(control))
            {
                ShowRightOnly();
            }
            Visible = true;
        }

        public void ShowAll()
        {
            Visible = false;
            splitContainer1.Panel1Collapsed = false;
            splitContainer1.Panel2Collapsed = false;
            splitContainer2.Panel1Collapsed = false;
            splitContainer2.Panel2Collapsed = false;
            Visible = true;
        }

        public void ShowLeftOnly()
        {
            splitContainer1.Panel2Collapsed = true;
        }

        public void ShowMiddleOnly()
        {
            splitContainer1.Panel1Collapsed = true;
            splitContainer2.Panel2Collapsed = true;
        }

        public void ShowRightOnly()
        {
            splitContainer1.Panel1Collapsed = true;
            splitContainer2.Panel1Collapsed = true;
        }

        public void SetMiddle(Control control)
        {
            control.Visible = true;
            panelMiddle.Controls.Clear();
            panelMiddle.Controls.Add(control);
            control.Dock = DockStyle.Fill;
        }

        public void SetRight(Control control)
        {
            control.Visible = true;
            panelRight.Controls.Clear();
            panelRight.Controls.Add(control);
            control.Dock = DockStyle.Fill;
        }

        public ThreePartView()
        {
            InitializeComponent();
        }
    }
}
