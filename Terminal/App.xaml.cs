using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Terminal.Models;

namespace Terminal
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {

        public ApplicationCore CoreData { get; private set; }// = new ApplicationCore();

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            this.CoreData.Dispose();
            //ApplicationCore.Instance.Dispose();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            this.CoreData = new ApplicationCore();
        }
    }

}
