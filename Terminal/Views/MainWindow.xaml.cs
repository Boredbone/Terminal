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
        
        private MainWindowViewModel ViewModel { get; set; }

        //private CompositeDisposable Disposables { get; }

        public MainWindow()
        {
            InitializeComponent();

            ((App)Application.Current).CoreData
                .WindowPlacement.Register(this, "TerminalHeavensRock");

            //this.Disposables = new CompositeDisposable();

            this.ViewModel = new MainWindowViewModel();//.AddTo(this.Disposables);
            
            this.ViewModel.View = this;
            this.ViewModel.TextsList = this.textsList;
            this.ViewModel.ListScroller = this.listScroller;
            this.ViewModel.SubscribeTextChanged(this.inputText);

            this.DataContext = this.ViewModel;

            //var textChanged =
            //    Observable.FromEvent<TextChangedEventHandler, TextChangedEventArgs>
            //    (h => (sender, e) => h(e),
            //    h => this.inputText.TextChanged += h,
            //    h => this.inputText.TextChanged -= h);
            //
            //
            //this.ViewModel.TextHistoryIndex
            //    .Buffer(textChanged.Where(y => this.inputText.Text.Length > 0))
            //    .Where(y => y.Count > 0)
            //    .Subscribe(y => this.inputText.Select(this.inputText.Text.Length, 0))
            //    .AddTo(this.Disposables);
            
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
            => this.ViewModel?.Dispose();
        

        /*
        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Return:
                    this.ViewModel.SendCommand.Execute();
                    break;
            }
        }
        
        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            return;
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
        */

        //private void textsList_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.Key == Key.C && (Key.LeftCtrl.IsDown() || Key.RightCtrl.IsDown()))
        //    {
        //        this.ViewModel.CopyCommand.Execute();
        //    }
        //}
    }

    //internal static class KeyboardExtensions
    //{
    //    public static bool IsDown(this Key key)
    //        => (Keyboard.GetKeyStates(key) & KeyStates.Down) == KeyStates.Down;
    //}
}
