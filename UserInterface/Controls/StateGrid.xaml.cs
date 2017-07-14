using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace UserInterface.Controls
{
    /// <summary>
    /// Interaction logic for StateGrid.xaml
    /// </summary>
    public partial class StateGrid : UserControl
    {
        public bool IsDirty { get; set; }

        public StateGrid()
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

        private void uiDataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                if (e.Row.Item == uiDataGrid.Items[uiDataGrid.Items.Count - 2])
                {
                    var rowToSelect = uiDataGrid.Items[uiDataGrid.Items.Count - 1];
                    int rowIndex = uiDataGrid.Items.IndexOf(rowToSelect);
                    this.Dispatcher.BeginInvoke(new DispatcherOperationCallback((param) => {
                        var cell = DataGridHelper.GetCell(uiDataGrid, rowIndex, 0);
                        cell.Focus();
                        uiDataGrid.BeginEdit();
                        return null;
                    }), DispatcherPriority.Background, new object[] { null });
                }
            }
        }
    }
}
