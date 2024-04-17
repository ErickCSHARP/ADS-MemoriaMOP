using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Atv2_Memoria
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {

        public string TextAddress => _textAddress;
              

        public bool TargetOn
        {
            get => _targetOn;
            set
            {
                if (_targetOn != value)
                {
                    _targetOn = value;
                    notifier();
                }
            }
        }

        public string Text 
        { 
            get => _text; 
            set
            {
                if (_text != value)
                {
                    _text = value;
                    notifier();
                }
            }
        }

        public int BufferSize
        {
            get => _bufferSize;
            set
            {
                if (_bufferSize != value)
                {
                    _bufferSize = value;
                    notifier();
                }
            }
        }


        private string _text = string.Empty;
        private bool _targetOn;
        private string _textAddress;
        public int _bufferSize = 60;

        public void SetTextAddress(long address)
        {
            var bytes = BitConverter.GetBytes(address).Reverse().ToArray();

            var strhex = Convert.ToHexString(bytes);

            if (strhex != _textAddress)
            {
                _textAddress = "0x" + strhex;
                notifier(nameof(TextAddress));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void notifier([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
