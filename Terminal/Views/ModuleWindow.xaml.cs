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
    /// ModuleWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ModuleWindow : Window
    {
        public ModuleWindow()
        {
            InitializeComponent();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            var dc = this.Content as IDisposable;
            if (dc != null)
            {
                dc.Dispose();
            }
        }
    }
}
