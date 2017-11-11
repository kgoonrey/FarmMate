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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace FarmMateWPF
{
    /// <summary>
    /// Interaction logic for AssetMaintenance.xaml
    /// </summary>
    public partial class AssetMaintenance : Window
    {
        private ViewModel _viewModel = null;
        private bool _closing = false;

        public AssetMaintenance()
        {
            InitializeComponent();
            _viewModel = new ViewModel();
            _viewModel.AssetTypeDataTable = _viewModel.AssetTypeAdapter.GetActiveData();
            _viewModel.ManufacturerDataTable = _viewModel.ManufacturerAdapter.GetActiveData();
            _viewModel.StatesDataTable = _viewModel.StatesAdapter.GetData();
            uiCode.Focus();
        }

        private void uiCode_LostFocus(object sender, RoutedEventArgs e)
        {
            if (_closing)
                return;

            _viewModel.SetAssetDataSource(uiCode.Text);
            if (_viewModel.AssetDataTable.Rows.Count > 0)
            {
                _viewModel.AssetRow = _viewModel.AssetDataTable[0];
            }
            else
            {
                if (MessageBox.Show(this, "Do you want to create a new Asset?", "Create a new Asset?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                {
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() => uiCode.Focus()));
                    return;
                }
                _viewModel.AssetRow = _viewModel.AssetDataTable.NewAssetsRow();
                _viewModel.AssetRow.Code = uiCode.Text;
                _viewModel.AssetDataTable.AddAssetsRow(_viewModel.AssetRow);
            }
            DataContext = _viewModel;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _closing = true;
        }

        private void uiBottom_SaveClick(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(this, "Do you wish to save?", "Save?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                return;

            _viewModel.Update();
            uiCode.UpdateDataSource();
        }
    }
}
