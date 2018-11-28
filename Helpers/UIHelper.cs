using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WebResourceManager.Helpers
{
    public static class UIHelper
    {
        public static void Invoke(Action action)
        {
            Application.Current.Dispatcher.BeginInvoke(action);
        }
    }
}
