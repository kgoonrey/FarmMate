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
    /// Interaction logic for ManufacturerGrid.xaml
    /// </summary>
    public partial class ManufacturerGrid : UserControl
    {
        public bool IsDirty { get; set; }

        public ManufacturerGrid()
        {
            InitializeComponent();
        }

        private void uiDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            IsDirty = true;
        }

        private void uiDataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            IsDirty = true;
        }
    }
}
