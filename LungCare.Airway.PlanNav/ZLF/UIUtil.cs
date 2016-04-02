using System;
using System.Windows.Threading;

namespace LungCare.SupportPlatform.UI
{
    public class UIUtil
    {
        public static void Invoke(DispatcherObject userControl, Action action)
        {
            userControl.Dispatcher.Invoke(new Action(action));
        }
    }
}
