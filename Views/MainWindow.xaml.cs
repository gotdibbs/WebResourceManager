using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WebResourceManager.ViewModels;

namespace WebResourceManager.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            DataContext = new MainWindowViewModel();

            //var ibd = new IconBitmapDecoder(
            //    new Uri("pack://applicaiton:,,,/WebResourceManager;component/Resources/upload-button.png"),
            //    BitmapCreateOptions.None,
            //    BitmapCacheOption.Default);

            //Icon = BitmapFrame.Create(Application.GetResourceStream(new Uri("../Resources/upload-button.png", UriKind.RelativeOrAbsolute)).Stream);
        }
    }
}
