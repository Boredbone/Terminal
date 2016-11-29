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
    /// LicenseWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class LicenseWindow : Window
    {
        public LicenseWindow()
        {
            InitializeComponent();

            var ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

            this.versionText.Text = ver.ToString();

            var buildDateTime = new DateTime(2000, 1, 1, 0, 0, 0);
            buildDateTime = buildDateTime.AddDays(ver.Build);
            buildDateTime = buildDateTime.AddSeconds(ver.Revision * 2);
            //this.buildDate.Text = buildDateTime.ToString();
            
        }
    }
}
