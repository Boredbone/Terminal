using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Boredbone.Utility.Extensions;
using Boredbone.XamlTools;
using Boredbone.XamlTools.Extensions;
using PluginSample.Models;
using PluginSample.Views;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace PluginSample.ViewModels
{
    public class MainControlViewModel : DisposableBase
    {
        private CoreModel Core { get; }

        [Required(ErrorMessage = "Required")]
        public ReactiveProperty<string> Word { get; }
        public ReadOnlyReactiveProperty<string> WordErrorMessage { get; }
        
        [Range(1, int.MaxValue, ErrorMessage = "must be positive")]
        public ReactiveProperty<int> Loop { get; }
        public ReadOnlyReactiveProperty<string> LoopErrorMessage { get; }


        public ReactiveCommand StartCommand { get; }
        public ReactiveCommand DialogCommand { get; }




        public MainControlViewModel(CoreModel core)
        {
            this.Core = core;

            this.Word = this.Core.Plugin
                .ToReactivePropertyAsSynchronized(y => y.Word, ignoreValidationErrorValue: true)
                .SetValidateAttribute(() => this.Word)
                .AddTo(this.Disposables);

            this.WordErrorMessage = this.Word
                .ObserveErrorChanged
                .Select(x => x?.Cast<string>()?.Join())
                .ToReadOnlyReactiveProperty()
                .AddTo(this.Disposables);


            this.Loop = this.Core.Plugin
                .ToReactivePropertyAsSynchronized(y => y.Loop, ignoreValidationErrorValue: true)
                .SetValidateAttribute(() => this.Loop)
                .AddTo(this.Disposables);

            this.LoopErrorMessage = this.Loop
                .ObserveErrorChanged
                .Select(x => x?.Cast<string>()?.Join())
                .ToReadOnlyReactiveProperty()
                .AddTo(this.Disposables);


            this.StartCommand = new[]
                {
                    this.Word.ObserveHasErrors,
                    this.Loop.ObserveHasErrors
                }
                .CombineLatestValuesAreAllFalse()
                .ToReactiveCommand()
                .WithSubscribe(_ =>
                {
                    this.Core.Plugin.Run();
                }, this.Disposables);

            this.DialogCommand = new ReactiveCommand()
                .WithSubscribe(control =>
                {
                    var window = (Window)this.Core
                    .OpenWindow(new Terminal.Macro.Api.OpenWindowRequestArgs()
                    {
                        Content = new SubControl(),
                        Height = 200,
                        Width = 200,
                        Title = "SubControl",
                        IsHidden = true,
                    });

                    var parent = Window.GetWindow((DependencyObject)control);
                    if (parent == null)
                    {
                        window.Show();
                        window.Activate();
                    }
                    else
                    {
                        window.Owner = parent;
                        window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                        window.ShowDialog();
                    }


                }, this.Disposables);
        }
    }
}
