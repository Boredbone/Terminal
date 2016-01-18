using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Text;

namespace Boredbone.XamlTools
{
    public class DisposableBase : IDisposable
    {
        public bool IsDisposed => this.disposables.IsDisposed;

        private CompositeDisposable disposables = new CompositeDisposable();
        protected CompositeDisposable Disposables => this.disposables;

        public virtual void Dispose() => this.disposables.Dispose();
    }
}
