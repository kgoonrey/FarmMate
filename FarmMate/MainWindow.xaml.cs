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

namespace FarmMateWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void uiAssets_Click(object sender, RoutedEventArgs e)
        {
            var asset = new AssetMaintenance();
            asset.ShowDialog();
        }

        private void uiControls_Click(object sender, RoutedEventArgs e)
        {
            Controls control = new Controls();
            control.ShowDialog();
        }
    }
}
