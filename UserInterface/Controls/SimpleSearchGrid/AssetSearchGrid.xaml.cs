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

namespace UserInterface.Controls.SimpleSearchGrid
{
    /// <summary>
    /// Interaction logic for AssetSearchGrid2.xaml
    /// </summary>
    public partial class AssetSearchGrid : Window
    {
        private Data.Database.AssetsDataTable _assetDataTable = null;
        private Data.DatabaseTableAdapters.AssetsTableAdapter _assetTableAdapter = new Data.DatabaseTableAdapters.AssetsTableAdapter();

        public AssetSearchGrid()
        {
            InitializeComponent();
            uiDataGrid.AutoGenerateColumns = false;
            FilterGrid();
            uiCode.Focus();
        }

        private string _text = string.Empty;
        public string Text
        {
            get { return _text; }
            set { _text = value; uiCode.Text = value; }
        }
        public bool Accept { get; set; }

        private void uiCode_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterGrid();
        }

        private void FilterGrid()
        {
            if (_assetDataTable == null)
            {
                _assetDataTable = _assetTableAdapter.GetData();
            }

            if(uiShowActive.IsChecked.Value)
                uiDataGrid.ItemsSource = _assetDataTable.Where(x => x.Code.Contains(uiCode.Text));
            else
                uiDataGrid.ItemsSource = _assetDataTable.Where(x => x.Code.Contains(uiCode.Text) && x.Active == true);
        }

        private void uiDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (uiDataGrid.SelectedCells.Count == 0)
                return;

            uiCode.Text = ((Data.Database.AssetsRow)uiDataGrid.SelectedCells[0].Item).Code;
            Text = uiCode.Text;
            Accept = true;
            Close();
        }

        private void uiDataGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            e.Cancel = true;
        }

        private void uiOK_Click(object sender, RoutedEventArgs e)
        {
            if (uiDataGrid.SelectedCells.Count > 0)
            {
                uiCode.Text = ((Data.Database.AssetsRow)uiDataGrid.SelectedCells[0].Item).Code;
            }
            Text = uiCode.Text;
            Accept = true;
            Close();
        }

        private void uiCode_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                uiOK_Click(null, null);
            }
            else if (e.Key == Key.Down)
            {
                if (uiDataGrid.SelectedIndex == uiDataGrid.Items.Count)
                    return;
                uiDataGrid.SelectedIndex++;
            }
            else if (e.Key == Key.Up)
            {
                if (uiDataGrid.SelectedIndex < 1)
                    return;
                uiDataGrid.SelectedIndex--;
            }
            else if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        private void uiCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void uiShowActive_Click(object sender, RoutedEventArgs e)
        {
            FilterGrid();
        }
    }
}
