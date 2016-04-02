using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LungCare.SupportPlatform.Entities
{
    public class PositionChangedEventArgs : EventArgs
    {
        public double[] Position { get; set; }
    }
}
