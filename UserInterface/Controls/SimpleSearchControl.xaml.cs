﻿using System;
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

namespace UserInterface.Controls
{
    /// <summary>
    /// Interaction logic for SimpleSearchControl.xaml
    /// </summary>
    public partial class SimpleSearchControl : UserControl
    {
        private Data.Database.AssetsDataTable _assetDataTable = null;
        private Data.DatabaseTableAdapters.AssetsTableAdapter _assetTableAdapter = new Data.DatabaseTableAdapters.AssetsTableAdapter();

        private bool HasFocus { get; set; }

        public enum SearchTypeEnum
        {
            Assets
        }

        public string Text
        {
            get { return uiCode.Text; }
            set { uiCode.Text = value; }
        }

        public SearchTypeEnum SearchType { get; set; }

        public new void Focus()
        {
            uiCode.Focus();
        }

        public SimpleSearchControl()
        {
            InitializeComponent();
            uiDataGrid.AutoGenerateColumns = false;
        }

        private void uiCode_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(uiCode.Text == string.Empty)
                uiDataGrid.Visibility = Visibility.Collapsed;
            else
                uiDataGrid.Visibility = Visibility.Visible;

            filterGrid();
        }

        private void LoseFocus()
        {
            uiDataGrid.Visibility = Visibility.Collapsed;
            HasFocus = false;
        }

        private void filterGrid()
        {
            if(SearchType == SearchTypeEnum.Assets)
            {
                if(_assetDataTable == null)
                {
                    _assetDataTable = _assetTableAdapter.GetData();
                }

                uiDataGrid.ItemsSource = _assetDataTable.Where(x => x.Code.Contains(uiCode.Text));
            }
        }

        private void uiDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (uiDataGrid.SelectedCells.Count == 0)
                return;

            uiCode.Text = ((Data.Database.AssetsRow)uiDataGrid.SelectedCells[0].Item).Code;

            Focus();
            LoseFocus();
            TraversalRequest request = new TraversalRequest(FocusNavigationDirection.Next);
            request.Wrapped = true;
            ((DataGrid)sender).MoveFocus(request);
        }

        private void uiDataGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            e.Cancel = true;
        }

        private void UserControl_GotFocus(object sender, RoutedEventArgs e)
        {
            HasFocus = true;
        }

        private void uiCode_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
            {
                LoseFocus();
            }
        }

        private void Control_LostFocus(object sender, RoutedEventArgs e)
        {
            if(HasFocus)
                e.Handled = true;
        }

        private void uiBrowse_Click(object sender, RoutedEventArgs e)
        {
            if (SearchType == SearchTypeEnum.Assets)
            {
                var asset = new SimpleSearchGrid.AssetSearchGrid();
                asset.Text = uiCode.Text;
                asset.ShowDialog();
                if (!asset.Accept)
                {
                    Focus();
                    return;
                }

                uiCode.Text = asset.Text;
                Focus();
                LoseFocus();
                TraversalRequest request = new TraversalRequest(FocusNavigationDirection.Next);
                request.Wrapped = true;
                ((Button)sender).MoveFocus(request);
            }
        }
    }
}