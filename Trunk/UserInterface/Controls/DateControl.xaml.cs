using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace UserInterface.Controls
{
    /// <summary>
    /// Interaction logic for DateControl.xaml
    /// </summary>
    public partial class DateControl : UserControl
    {
        public static DependencyProperty DataSourceProperty = DependencyProperty.Register("DataSource", typeof(object), typeof(DateControl));
        public object DataSource
        {
            get { return GetValue(DataSourceProperty); }
            set { SetValue(DataSourceProperty, value); }
        }

        public DateControl()
        {
            InitializeComponent();
        }

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            (e.Parameter as Calendar).SelectedDate = DateTime.Today;
            uiDate.IsDropDownOpen = false;
        }

        private void PART_TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
                Tools.MoveToNextUIElement(e);
        }

        private void PART_TextBox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var a = "";
        }
    }

    public class MyCommands
    {
        public static RoutedCommand SelectToday = new RoutedCommand("Today", typeof(MyCommands));
    }
}
