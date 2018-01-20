using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    /// Interaction logic for ComboBoxControl.xaml
    /// </summary>
    public partial class ComboBoxControl : UserControl
    {
        public string DisplayMemberPath { get; set; }
        public string SelectedValuePath { get; set; }

        private bool _addOnTheFly = false;
        public bool AddOnTheFly
        {
            get { return _addOnTheFly; }
            set
            {
                _addOnTheFly = value;
                uiComboBox.IsEditable = value;
            }
        }

        public static DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(object), typeof(ComboBoxControl));
        public object ItemsSource
        {
            get { return GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public static DependencyProperty SelectedValueProperty = DependencyProperty.Register("SelectedValue", typeof(string), typeof(ComboBoxControl));
        public string SelectedValue
        {
            get { return (string)GetValue(SelectedValueProperty); }
            set { SetValue(SelectedValueProperty, value); }
        }

        public static DependencyProperty TableProperty = DependencyProperty.Register("Table", typeof(Sources.TableEnum), typeof(ComboBoxControl));
        public Sources.TableEnum Table
        {
            get { return (Sources.TableEnum)GetValue(TableProperty); }
            set { SetValue(TableProperty, value); }
        }

        private bool _clicked = false;

        public ComboBoxControl()
        {
            InitializeComponent();
        }

        private void uiComboBox_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (_clicked || e.OriginalSource.GetType() != typeof(TextBox)) { _clicked = false; return; }

            if (!IsVisible || !AddOnTheFly || uiComboBox.Text == string.Empty || uiComboBox.SelectedItem != null)
                return;

            var newItem = uiComboBox.Text;
            if (Table == Sources.TableEnum.AssetTypes)
            {
                if (MessageBox.Show("Create a new asset type?", "Create New?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                {
                    uiComboBox.IsDropDownOpen = false;
                    e.Handled = true;
                    ((TextBox)e.OriginalSource).SelectAll();
                    return;
                }

                ((Data.Database.AssetTypesDataTable)uiComboBox.ItemsSource).AddAssetTypesRow(uiComboBox.Text, uiComboBox.Text, true);
                new Data.DatabaseTableAdapters.AssetTypesTableAdapter().Update(((Data.Database.AssetTypesDataTable)uiComboBox.ItemsSource));
                uiComboBox.SelectedValue = newItem;
            }
            else if (Table == Sources.TableEnum.Manufacturers)
            {
                if (MessageBox.Show("Create a new manufacturer?", "Create New?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                {
                    uiComboBox.IsDropDownOpen = false;
                    e.Handled = true;
                    ((TextBox)e.OriginalSource).SelectAll();
                    return;
                }

                ((Data.Database.ManufacturersDataTable)uiComboBox.ItemsSource).AddManufacturersRow(uiComboBox.Text, uiComboBox.Text, true);
                new Data.DatabaseTableAdapters.ManufacturersTableAdapter().Update(((Data.Database.ManufacturersDataTable)uiComboBox.ItemsSource));
                uiComboBox.SelectedValue = newItem;
            }
        }

        private void uiComboBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Tools.MoveToNextUIElement(e);
            }
        }

        private void uiComboBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource.GetType() != typeof(Rectangle))
                return;
            
            _clicked = true;           
        }
    }
}
