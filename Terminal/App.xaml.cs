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
        public ApplicationCore CoreData { get; private set; }

        public App()
        {
            AppDomain.CurrentDomain.UnhandledException
                += CurrentDomain_UnhandledException;
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            this.CoreData = new ApplicationCore();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            this.CoreData.Save();
            this.CoreData.Dispose();
        }


        private void CurrentDomain_UnhandledException(
                        object sender,
                        UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            if (exception == null)
            {
                MessageBox.Show("Unknown Exception");
                return;
            }

            //var errorMember = exception.TargetSite.Name;
            //var errorMessage = exception.Message;
            var message = string.Format(exception.ToString());
            MessageBox.Show(message, "UnhandledException",
                            MessageBoxButton.OK, MessageBoxImage.Stop);
            Environment.Exit(0);
        }
    }
}
