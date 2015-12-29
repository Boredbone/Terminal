using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Reactive.Bindings.Extensions;
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

        private CompositeDisposable Disposables { get; }

        public MainWindow()
        {
            InitializeComponent();

            ((App)Application.Current).CoreData
                .WindowPlacement.Register(this, "TerminalHeavensRock");

            this.Disposables = new CompositeDisposable();

            this.ViewModel = new MainWindowViewModel().AddTo(this.Disposables);
            
            this.ViewModel.View = this;
            this.ViewModel.TextsList = this.textsList;
            this.DataContext = this.ViewModel;

            var textChanged =
                Observable.FromEvent<TextChangedEventHandler, TextChangedEventArgs>
                (h => (sender, e) => h(e),
                h => this.inputText.TextChanged += h,
                h => this.inputText.TextChanged -= h);


            this.ViewModel.TextHistoryIndex
                .Buffer(textChanged.Where(y => this.inputText.Text.Length > 0))
                .Where(y => y.Count > 0)
                .Subscribe(y => this.inputText.Select(this.inputText.Text.Length, 0))
                .AddTo(this.Disposables);
            
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Disposables.Dispose();
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
            //var core = ((App)Application.Current).CoreData;
            //foreach (var plugin in core.Plugins)
            //{
            //    core.LaunchPluginUI(plugin);
            //}
        }
    }
}
