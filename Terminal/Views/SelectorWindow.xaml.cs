using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;

namespace Terminal.Views
{
    /// <summary>
    /// SelectorWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SelectorWindow : Window, INotifyPropertyChanged
    {
        private ObservableCollection<object> _fieldList;
        public ObservableCollection<object> List
        {
            get { return _fieldList; }
            private set
            {
                if (_fieldList != value)
                {
                    _fieldList = value;
                    RaisePropertyChanged(nameof(List));
                }
            }
        }
        private int _fieldIndex;
        public int Index
        {
            get { return _fieldIndex; }
            set
            {
                if (_fieldIndex != value)
                {
                    _fieldIndex = value;
                    RaisePropertyChanged(nameof(Index));
                }
            }
        }
        


        public SelectorWindow()
        {
            InitializeComponent();
            this.List = new ObservableCollection<object>();
            this.Index = 0;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void ListViewItem_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.DialogResult = true;

        }

        
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
            => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
