﻿using System;
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
    /// PluginWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class PluginWindow : Window
    {
        public string WindowId { get; set; }

        public PluginWindow()
        {
            InitializeComponent();
        }

        private void Window_Closed(object sender, EventArgs e)
            => (this.Content as IDisposable)?.Dispose();
    }
}
