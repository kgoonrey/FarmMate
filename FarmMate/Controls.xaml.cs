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

namespace FarmMateWPF
{
    /// <summary>
    /// Interaction logic for Controls.xaml
    /// </summary>
    public partial class Controls : Window
    {
        private Data.Database.AssetTypesDataTable _assetTypes = null;
        private Data.DatabaseTableAdapters.AssetTypesTableAdapter _assetTypesTableAdapter = new Data.DatabaseTableAdapters.AssetTypesTableAdapter();
        private Data.Database.ManufacturersDataTable _manufacturers = null;
        private Data.DatabaseTableAdapters.ManufacturersTableAdapter _manufacturersTableAdapter = new Data.DatabaseTableAdapters.ManufacturersTableAdapter();
        private int _selectedControl = -1;

        private enum ControlEnum
        {
            AssetType,
            Manufacturer
        }

        public Controls()
        {
            InitializeComponent();
            uiAssetType.IsSelected = true;
        }

        private void uiAssetType_Selected(object sender, RoutedEventArgs e)
        {
            Save(false);
            LoadAssetType();
            _selectedControl = (int)ControlEnum.AssetType;
        }

        private void uiManufacturer_Selected(object sender, RoutedEventArgs e)
        {
            Save(false);
            LoadManufacturer();
            _selectedControl = (int)ControlEnum.Manufacturer;
        }

        private void LoadAssetType()
        {
            uiContentPanel.Children.Clear();
            UserInterface.Controls.AssetTypeGrid uiDataGrid = new UserInterface.Controls.AssetTypeGrid();
            uiContentPanel.Children.Add(uiDataGrid);

            _assetTypes = _assetTypesTableAdapter.GetData();
            uiDataGrid.uiDataGrid.ItemsSource = _assetTypes;
        }

        private void LoadManufacturer()
        {
            uiContentPanel.Children.Clear();
            UserInterface.Controls.ManufacturerGrid uiDataGrid = new UserInterface.Controls.ManufacturerGrid();
            uiContentPanel.Children.Add(uiDataGrid);

            _manufacturers = _manufacturersTableAdapter.GetData();
            uiDataGrid.uiDataGrid.ItemsSource = _manufacturers;
        }

        private void uiBottom_SaveClick(object sender, RoutedEventArgs e)
        {
            Save(true);
        }

        private void Save(bool clicked)
        {
            if (!clicked)
            {
                if(uiContentPanel.Children.Count == 0)
                    return;

                if (_selectedControl == (int)ControlEnum.AssetType)
                {
                    if (!((UserInterface.Controls.AssetTypeGrid)uiContentPanel.Children[0]).IsDirty)
                        return;
                }
                else if (_selectedControl == (int)ControlEnum.Manufacturer)
                {
                    if (!((UserInterface.Controls.ManufacturerGrid)uiContentPanel.Children[0]).IsDirty)
                        return;
                }
            }
            if (MessageBox.Show(this, "Do you wish to save?", "Save?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                return;

            _assetTypesTableAdapter.Update(_assetTypes);
            _manufacturersTableAdapter.Update(_manufacturers);

            if (uiContentPanel.Children.Count == 0)
                return;

            if (_selectedControl == (int)ControlEnum.AssetType)
            {
                ((UserInterface.Controls.AssetTypeGrid)uiContentPanel.Children[0]).IsDirty = false;
            }
            else if (_selectedControl == (int)ControlEnum.Manufacturer)
            {
                ((UserInterface.Controls.ManufacturerGrid)uiContentPanel.Children[0]).IsDirty = false;
            }
        }
    }
}
