using System.Windows.Forms;

namespace LungCare.Airway.Layout
{
    public partial class SideBySideByUpDownSubViewLayout : UserControl
    {
        public void SetLeft(Control control)
        {
            control.Visible = true;
            //(control.Parent).Controls.Remove(control);
            panelLeft.Controls.Clear();
            panelLeft.Controls.Add(control);
            control.Dock = DockStyle.Fill;
        }

        public void SetMiddle(Control control)
        {
            control.Visible = true;
            panelMiddle.Controls.Clear();
            panelMiddle.Controls.Add(control);
            control.Dock = DockStyle.Fill;
        }

        public void SetRightUp(Control control)
        {
            control.Visible = true;
            panelRightUp.Controls.Clear();
            panelRightUp.Controls.Add(control);
            control.Dock = DockStyle.Fill;
        }

        public void SetRightDown(Control control)
        {
            control.Visible = true;
            panelRightDown.Controls.Clear();
            panelRightDown.Controls.Add(control);
            control.Dock = DockStyle.Fill;
        }

        public void Max(Control control)
        {
            //Visible = false;
            if (panelLeft.Controls.Contains(control))
            {
                ShowLeftOnly();
            }
            else if (panelMiddle.Controls.Contains(control))
            {
                ShowMiddleOnly();
            }
            else if (panelRightUp.Controls.Contains(control))
            {
                ShowRightUpOnly();
            }
            else if (panelRightDown.Controls.Contains(control))
            {
                ShowRightDownOnly();
            }
            //Visible = true;
        }

        public void ShowAll()
        {
            Visible = false;
            splitContainer1.Panel1Collapsed = false;
            splitContainer1.Panel2Collapsed = false;
            splitContainer2.Panel1Collapsed = false;
            splitContainer2.Panel2Collapsed = false;
            splitContainer3.Panel1Collapsed = false;
            splitContainer3.Panel2Collapsed = false;
            Visible = true;
        }

        public void ShowLeftOnly()
        {
            splitContainer1.Panel2Collapsed = true;
        }

        public void ShowMiddleOnly()
        {
            //splitContainer1.Panel1Collapsed = true;
            splitContainer2.Panel2Collapsed = true;
        }

        public void ShowRightUpOnly()
        {
            //splitContainer1.Panel1Collapsed = true;
            splitContainer2.Panel1Collapsed = true;
            splitContainer3.Panel2Collapsed = true;
        }

        public void ShowRightDownOnly()
        {
            //splitContainer1.Panel1Collapsed = true;
            splitContainer2.Panel1Collapsed = true;
            splitContainer3.Panel1Collapsed = true;
        }

        public SideBySideByUpDownSubViewLayout()
        {
            InitializeComponent();
        }
    }
}
