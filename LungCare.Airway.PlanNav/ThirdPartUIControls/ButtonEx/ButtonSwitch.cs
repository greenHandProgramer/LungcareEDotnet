using System.Drawing;
using System;

namespace CSharpWin
{
    public class ButtonSwitch : CSharpWin.ButtonEx
    {
        public string Text1 { get; set; }
        public string Text2 { get; set; }

        private int status = 0;

        public int Status
        {
            get { return status; }
        }

        public ButtonSwitch()
            : base()
        {
            Click += new System.EventHandler(ButtonSwitch_Click);
        }

        public event EventHandler Status1Event;
        public event EventHandler Status2Event;

        internal void Switch()
        {
            ButtonSwitch_Click(this, new EventArgs());
        }

       internal void ButtonSwitch_Click(object sender, System.EventArgs e)
        {
            if (status == 0)
            {
                status = 1;

                if (!string.IsNullOrEmpty(Text1))
                {
                    Text = Text2;
                }
                else
                {
                    BaseColor = Color.Gray;
                }

                if (Status1Event != null)
                {
                    Status1Event(sender, e);
                }
            }
            else
            {
                status = 0;

                if (!string.IsNullOrEmpty(Text2))
                {
                    Text = Text1;
                }
                else
                {
                    BaseColor = Color.FromArgb(51, 161, 224);
                }

                if (Status2Event != null)
                {
                    Status2Event(sender, e);
                }
            }

            Refresh();
        }
    }
}
