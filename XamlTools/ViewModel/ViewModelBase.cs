using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;

namespace Boredbone.XamlTools.ViewModel
{
    public class ViewModelBase : INotifyPropertyChanged, IDisposable
    {
        public bool IsDisposed => this.disposables.IsDisposed;

        private CompositeDisposable disposables = new CompositeDisposable();
        protected CompositeDisposable Disposables => this.disposables;

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void Dispose()
            => this.disposables.Dispose();

        protected void RaisePropertyChanged(string propertyName)
            => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
