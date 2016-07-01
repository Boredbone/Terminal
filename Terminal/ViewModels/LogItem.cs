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

        private bool _fieldIsLast;
        public bool IsLast
        {
            get { return _fieldIsLast; }
            set
            {
                if (_fieldIsLast != value)
                {
                    _fieldIsLast = value;
                    RaisePropertyChanged(nameof(IsLast));
                }
            }
        }


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
            private set
            {
                if (_fieldColor != value)
                {
                    _fieldColor = value;
                    RaisePropertyChanged(nameof(Color));
                }
            }
        }

        public int BoldStart
        {
            get { return _fieldBoldStart; }
            set
            {
                if (_fieldBoldStart != value)
                {
                    _fieldBoldStart = value;
                    RaisePropertyChanged(nameof(BoldStart));
                }
            }
        }
        private int _fieldBoldStart;

        public int BoldCount
        {
            get { return _fieldBoldCount; }
            set
            {
                if (_fieldBoldCount != value)
                {
                    _fieldBoldCount = value;
                    RaisePropertyChanged(nameof(BoldCount));
                }
            }
        }
        private int _fieldBoldCount;



        public LogItem()
        {
            this.LogType = LogTypes.Normal;
            this.BoldStart = 0;
            this.BoldCount = 0;
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

        public void AddText(string text, bool isBold)
        {
            var oldLength = this.Text.Length;

            var added = this.Text + text;
            this.Text = added;

            if (isBold)
            {
                if (this.BoldStart <= 0 || this.BoldCount <= 0)
                {
                    this.BoldStart = oldLength;
                }
                this.BoldCount = this.Text.Length - this.BoldStart;
            }
        }
    }
}
