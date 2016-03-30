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

namespace Terminal.Views
{
    /// <summary>
    /// SettingWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingWindow : Window
    {
        //private MainWindowViewModel ViewModel { get; set; }

        public SettingWindow()
        {
            InitializeComponent();

            var core = ((App)Application.Current).CoreData;

            var items = core.Connection.GetSettings();
            this.dataGrid.ItemsSource = items;
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var core = ((App)Application.Current).CoreData;

            var textBox = sender as TextBox;
            if (textBox == null)
            {
                return;
            }
            var key = (textBox.DataContext as KeyValuePair<string, string>?)?.Key;
            var currentSetting = core.Connection.GetSetting(key);

            if (currentSetting == null || textBox.Text == currentSetting)
            {
                return;
            }

            core.Connection.SetSetting(key, textBox.Text);

            var items = core.Connection.GetSettings();
            this.dataGrid.ItemsSource = items;
        }
    }
}
