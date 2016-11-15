using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Terminal.ViewModels;

namespace Terminal.Views
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel ViewModel { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            ((App)Application.Current).CoreData
                .WindowPlacement.Register(this, "TerminalHeavensRock");

            this.ViewModel = new MainWindowViewModel() { View = this };

            this.ViewModel.SubscribeTextChanged(this.inputText);

            this.DataContext = this.ViewModel;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
            => this.ViewModel?.Dispose();
        
    }
}
