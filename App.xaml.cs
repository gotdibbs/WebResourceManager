using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Windows.Threading;

namespace WebResourceManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            ServicePointManager.DefaultConnectionLimit = 5;
            ServicePointManager.UseNagleAlgorithm = false;
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

            Dispatcher.UnhandledException += OnDispatcherUnhandledException;
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            var errorMessage = $"An unhandled exception occurred: {e.Exception.Message}\r\n\r\n{e.Exception.ToString()}";
            MessageBox.Show(errorMessage, "Web Resource Manager - Error", MessageBoxButton.OK, MessageBoxImage.Error);

            e.Handled = true;
        }
    }
}
