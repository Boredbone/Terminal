using Boredbone.Utility.Tools;
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

            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var ver = assembly.GetName().Version;

            this.versionText.Text = ver.ToString(3);

            try
            {
                var buildDateTime = BuildTimeStamp.GetDateTimeUtcFrom(assembly.Location);
                this.versionDetail.Text = $"{ver} {buildDateTime}";
            }
            catch
            {
            }
        }
    }
}
