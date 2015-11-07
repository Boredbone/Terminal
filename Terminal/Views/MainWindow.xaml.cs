using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Terminal.ViewModels;

namespace Terminal.Views
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {

        private MainWindowViewModel _viewModel;
        public MainWindowViewModel ViewModel
        {
            get { return this._viewModel; }
            private set { this._viewModel = value; }
        }

        public MainWindow()
        {
            InitializeComponent();
            
            this.ViewModel = new MainWindowViewModel();
            
            this.ViewModel.View = this;
            this.ViewModel.TextsList = this.textsList;
            this.DataContext = this.ViewModel;

            foreach (var plugin in ((App)Application.Current).CoreData.Plugins)
            {
                plugin.OpenWindowRequested = o =>
                {
                    var window = new ModuleWindow();
                    window.Content = o;
                    window.Title = plugin.Name;
                    //window.Owner = this;
                    window.Show();
                    window.Activate();
                };
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.ViewModel.Dispose();
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Return:
                    this.ViewModel.SendCommand.Execute();
                    break;
                case Key.Up:
                    this.ViewModel.DecrementHistoryIndex();
                    break;
                case Key.Down:
                    this.ViewModel.IncrementHistoryIndex();
                    break;
            }
        }
        
        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    this.ViewModel.DecrementHistoryIndex();
                    e.Handled = true;
                    break;
                case Key.Down:
                    this.ViewModel.IncrementHistoryIndex();
                    e.Handled = true;
                    break;
            }
        }


        private void textsList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.C &&
                ((Keyboard.GetKeyStates(Key.LeftCtrl) & KeyStates.Down) == KeyStates.Down ||
                (Keyboard.GetKeyStates(Key.RightCtrl) & KeyStates.Down) == KeyStates.Down))
            {
                this.ViewModel.CopyCommand.Execute();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (var plugin in ((App)Application.Current).CoreData.Plugins)
            {
                plugin.LaunchUI();
            }
        }
    }
}
