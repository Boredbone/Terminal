using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Boredbone.XamlTools
{
    public class NotificationBase : DisposableBase, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
            => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
