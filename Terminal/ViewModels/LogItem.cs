using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Boredbone.XamlTools.ViewModel;

namespace Terminal.ViewModels
{

    /// <summary>
    /// コンソールアイテムのViewModel
    /// </summary>
    public class LogItem : ViewModelBase
    {
        private string _fieldText;
        public string Text
        {
            get { return _fieldText; }
            set
            {
                if (_fieldText != value)
                {
                    _fieldText = value;
                    RaisePropertyChanged(nameof(Text));
                }
            }
        }

        public int Index { get; set; }

        private LogTypes _fieldLogType;
        public LogTypes LogType
        {
            get { return _fieldLogType; }
            set
            {
                if (_fieldLogType != value)
                {
                    _fieldLogType = value;
                    RaisePropertyChanged(nameof(LogType));
                    this.SetColor();
                }
            }
        }

        private Brush _fieldColor;
        public Brush Color
        {
            get { return _fieldColor; }
            set
            {
                if (_fieldColor != value)
                {
                    _fieldColor = value;
                    RaisePropertyChanged(nameof(Color));
                }
            }
        }

        public LogItem()
        {
            this.LogType = LogTypes.Normal;
            this.SetColor();
        }

        private void SetColor()
        {
            switch (this.LogType)
            {
                case LogTypes.Normal:
                    this.Color = new SolidColorBrush(Colors.Black);
                    break;
                case LogTypes.Error:
                    this.Color = new SolidColorBrush(Colors.Red);
                    break;
                case LogTypes.Echo:
                    this.Color = new SolidColorBrush(Colors.Blue);
                    break;
                case LogTypes.Notice:
                    this.Color = new SolidColorBrush(Colors.Green);
                    break;
                case LogTypes.DisabledMessage:
                    this.Color = new SolidColorBrush(Colors.Gray);
                    break;
                case LogTypes.MacroMessage:
                    this.Color = new SolidColorBrush(Colors.Blue);
                    break;
                default:
                    break;
            }
        }
    }
}
