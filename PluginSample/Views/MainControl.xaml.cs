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
using System.Windows.Navigation;
using System.Windows.Shapes;
using PluginSample.ViewModels;

namespace PluginSample.Views
{
    /// <summary>
    /// MainControl.xaml の相互作用ロジック
    /// </summary>
    public partial class MainControl : UserControl, IDisposable
    {
        private MainControlViewModel _viewModel;
        public MainControlViewModel ViewModel
        {
            get { return this._viewModel; }
            set
            {
                this._viewModel = value;
                this.DataContext = value;
            }
        }

        public MainControl()
        {
            InitializeComponent();
        }

        public void Dispose()
        {
            this.ViewModel?.Dispose();
        }
    }
}
