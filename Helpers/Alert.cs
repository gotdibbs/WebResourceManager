using System.Windows;

namespace WebResourceManager.Helpers
{
    public static class Alert
    {
        public static void Show(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(message);
            });
        }
    }
}
