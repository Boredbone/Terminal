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


            this.description.Text = @"This software is built using the open source software:"
                + "\n- Reactive Extensions\n- ReactiveProperty\n- .NET Compiler Platform (\"Roslyn\")";
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //現在のコードを実行しているAssemblyを取得
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();

                this.ReadAndSetText(assembly, "ReactiveExtensions.txt", this.rxText);
                this.ReadAndSetText(assembly, "ReactiveProperty.txt", this.rpText);
                this.ReadAndSetText(assembly, "roslyn.txt", this.roslynText);

            }
            catch
            {

            }
        }

        private void ReadAndSetText(System.Reflection.Assembly assembly, string name, TextBlock target)
        {
            try
            {
                //指定されたマニフェストリソースを読み込む
                using (var sr = new System.IO.StreamReader(
                    assembly.GetManifestResourceStream
                    ("Terminal.Assets.Licenses." + name),
                    System.Text.Encoding.GetEncoding("shift-jis")))
                {
                    //内容を読み込む
                    string s = sr.ReadToEnd();
                    target.Text = s;
                }
            }
            catch
            {

            }
        }
    }
}
