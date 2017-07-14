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
        private Data.Database.AssetTypesDataTable _assetTypes = new Data.Database.AssetTypesDataTable();
        private Data.DatabaseTableAdapters.AssetTypesTableAdapter _assetTypesTableAdapter = new Data.DatabaseTableAdapters.AssetTypesTableAdapter();
        private Data.Database.ManufacturersDataTable _manufacturers = new Data.Database.ManufacturersDataTable();
        private Data.DatabaseTableAdapters.ManufacturersTableAdapter _manufacturersTableAdapter = new Data.DatabaseTableAdapters.ManufacturersTableAdapter();
        private Data.Database.StatesDataTable _statesDataTable = new Data.Database.StatesDataTable();
        private Data.DatabaseTableAdapters.StatesTableAdapter _statesTableAdapter = new Data.DatabaseTableAdapters.StatesTableAdapter();
        private Data.Database.ExpiryTypesDataTable _expiryTypesDataTable = new Data.Database.ExpiryTypesDataTable();
        private Data.DatabaseTableAdapters.ExpiryTypesTableAdapter _expiryTypesTableAdapter = new Data.DatabaseTableAdapters.ExpiryTypesTableAdapter();
        private Data.Database.ExpiryStatusDataTable _expiryStatusDataTable = new Data.Database.ExpiryStatusDataTable();
        private Data.DatabaseTableAdapters.ExpiryStatusTableAdapter _expiryStatusTableAdapter = new Data.DatabaseTableAdapters.ExpiryStatusTableAdapter();
        private int _selectedControl = -1;

        private enum ControlEnum
        {
            AssetType,
            Manufacturer,
            States,
            ExpiryTypes,
            ExpiryStatus
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


        private void uiStates_Selected(object sender, RoutedEventArgs e)
        {
            Save(false);
            LoadStates();
            _selectedControl = (int)ControlEnum.States;
        }

        private void uiExpiryTypes_Selected(object sender, RoutedEventArgs e)
        {
            Save(false);
            LoadExpiryTypes();
            _selectedControl = (int)ControlEnum.ExpiryTypes;
        }

        private void uiExpiryStatus_Selected(object sender, RoutedEventArgs e)
        {
            Save(false);
            LoadExpiryStatus();
            _selectedControl = (int)ControlEnum.ExpiryStatus;
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

        private void LoadStates()
        {
            uiContentPanel.Children.Clear();
            UserInterface.Controls.StateGrid uiDataGrid = new UserInterface.Controls.StateGrid();
            uiContentPanel.Children.Add(uiDataGrid);

            _statesDataTable = _statesTableAdapter.GetData();
            uiDataGrid.uiDataGrid.ItemsSource = _statesDataTable;
        }

        private void LoadExpiryTypes()
        {
            uiContentPanel.Children.Clear();
            UserInterface.Controls.ExpiryTypeGrid uiDataGrid = new UserInterface.Controls.ExpiryTypeGrid();
            uiContentPanel.Children.Add(uiDataGrid);

            _expiryTypesDataTable = _expiryTypesTableAdapter.GetData();
            uiDataGrid.uiDataGrid.ItemsSource = _expiryTypesDataTable;
        }

        private void LoadExpiryStatus()
        {
            uiContentPanel.Children.Clear();
            UserInterface.Controls.ExpiryStatusGrid uiDataGrid = new UserInterface.Controls.ExpiryStatusGrid();
            uiContentPanel.Children.Add(uiDataGrid);

            _expiryStatusDataTable = _expiryStatusTableAdapter.GetData();
            uiDataGrid.uiDataGrid.ItemsSource = _expiryStatusDataTable;
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
                else if (_selectedControl == (int)ControlEnum.States)
                {
                    if (!((UserInterface.Controls.StateGrid)uiContentPanel.Children[0]).IsDirty)
                        return;
                }
                else if (_selectedControl == (int)ControlEnum.ExpiryTypes)
                {
                    if (!((UserInterface.Controls.ExpiryTypeGrid)uiContentPanel.Children[0]).IsDirty)
                        return;
                }
                else if (_selectedControl == (int)ControlEnum.ExpiryStatus)
                {
                    if (!((UserInterface.Controls.ExpiryStatusGrid)uiContentPanel.Children[0]).IsDirty)
                        return;
                }
            }
            if (MessageBox.Show(this, "Do you wish to save?", "Save?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                return;

            _assetTypesTableAdapter.Update(_assetTypes);
            _manufacturersTableAdapter.Update(_manufacturers);
            _statesTableAdapter.Update(_statesDataTable);
            _expiryTypesTableAdapter.Update(_expiryTypesDataTable);
            _expiryStatusTableAdapter.Update(_expiryStatusDataTable);

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
            else if (_selectedControl == (int)ControlEnum.States)
            {
                ((UserInterface.Controls.StateGrid)uiContentPanel.Children[0]).IsDirty = false;
            }
            else if (_selectedControl == (int)ControlEnum.ExpiryTypes)
            {
                ((UserInterface.Controls.ExpiryTypeGrid)uiContentPanel.Children[0]).IsDirty = false;
            }
            else if (_selectedControl == (int)ControlEnum.ExpiryStatus)
            {
                ((UserInterface.Controls.ExpiryStatusGrid)uiContentPanel.Children[0]).IsDirty = false;
            }
        }
    }
}
