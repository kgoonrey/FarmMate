using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for NumericControl.xaml
    /// </summary>
    public partial class NumericControl : UserControl
    {
        public enum FormatEnum
        {
            Decimal,
            Currency
        }

        private int _decimals = 0;
        public int Decimals
        {
            get { return _decimals; }
            set { _decimals = value; }
        }

        public static DependencyProperty TextProperty = DependencyProperty.Register("DataSource", typeof(string), typeof(NumericControl));
        public string DataSource
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static DependencyProperty FormatProperty = DependencyProperty.Register("Format", typeof(FormatEnum), typeof(NumericControl));
        public FormatEnum Format
        {
            get { return (FormatEnum)GetValue(FormatProperty); }
            set { SetValue(FormatProperty, value); }
        }

        public NumericControl()
        {
            InitializeComponent();
        }

        private void uiTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                Tools.MoveToNextUIElement(e);

            if (Tools.IsKeyAChar(e.Key))
            {
                e.Handled = true;
                return;
            }

            var containsDecimals = uiTextBox.Text.Contains(".");

            if (e.Key == Key.OemPeriod || e.Key == Key.Decimal)
            {
                if (containsDecimals)
                {
                    e.Handled = true;
                    return;
                }
            }
        }

        private void uiTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            uiTextBox.SelectAll();
        }

        private void uiTextBox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            uiTextBox.SelectAll();
        }

        //private void uc_Loaded(object sender, RoutedEventArgs e)
        //{
        //    var text = uiTextBox.Text;
        //    if (text == string.Empty)
        //        return;

        //    decimal amount;
        //    if (!decimal.TryParse(text.Replace("$", ""), out amount))
        //        amount = 0m;

        //    var format = "N" + Decimals;
        //    if (Format == FormatEnum.Currency)
        //        format = "C" + Decimals;

        //    uiTextBox.Text = Math.Round(amount, Decimals).ToString(format);
        //}
    }
}
