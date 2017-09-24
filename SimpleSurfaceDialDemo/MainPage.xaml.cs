using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SimpleSurfaceDialDemo
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        #region Fields

        private SolidColorBrush _backgroundBrush;
        private SolidColorBrush _lastBackgroundBrush;
        private SolidColorBrush _previewColorBrush;

        #endregion

        #region Properties

        public SolidColorBrush BackgroundBrush
        {
            get => _backgroundBrush;
            set
            {
                _lastBackgroundBrush = _backgroundBrush;
                _backgroundBrush = value;
                OnPropertyChanged();
            }
        }

        public SolidColorBrush PreviewColorBrush
        {
            get => _previewColorBrush;
            set
            {
                _previewColorBrush = value;
                OnPropertyChanged();
            }
        }

        #endregion

        public MainPage()
        {
            BackgroundBrush = new SolidColorBrush(Colors.White);
            PreviewColorBrush = new SolidColorBrush(Colors.Black);
            InitializeComponent();
        }


        public event PropertyChangedEventHandler PropertyChanged;

        public void ApplyPreviewColor()
        {
            BackgroundBrush = PreviewColorBrush;
        }

        public void UndoPreviewColor()
        {
            BackgroundBrush = _lastBackgroundBrush;
        }

        public void UpdatePreviewColor()
        {
            PreviewColorBrush = new SolidColorBrush(Color.FromArgb(255, (byte)RedSlider.Value, (byte)GreenSlider.Value, (byte)BlueSlider.Value));
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}