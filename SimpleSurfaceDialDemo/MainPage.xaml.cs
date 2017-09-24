using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Input;
using Windows.UI.Xaml;
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
        private RadialController _dial;
        private Slider _dialControlledSlider;

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
            Loaded += OnLoaded;
        }

        private void OnLoaded(Object sender, RoutedEventArgs e)
        {
            // Clear menu
            var dialConfig = RadialControllerConfiguration.GetForCurrentView();

            // And just leave the volume control
            dialConfig.SetDefaultMenuItems(new [] {RadialControllerSystemMenuItemKind.Volume});

            _dial = RadialController.CreateForCurrentView();

            _dial.RotationResolutionInDegrees = 1;
            _dial.UseAutomaticHapticFeedback = false;

            //var redMenuItem = RadialControllerMenuItem.CreateFromKnownIcon("Red", RadialControllerMenuKnownIcon.Ruler);
            //var redMenuItem = RadialControllerMenuItem.CreateFromFontGlyph("Red", "R", "Segoe UI");
            var redIcon = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/RedColor.png"));
            var redMenuItem = RadialControllerMenuItem.CreateFromIcon("Red", redIcon);
            redMenuItem.Invoked += (item, args) => SetDialControlledSlider(RedSlider);
            _dial.Menu.Items.Add(redMenuItem);

            var greenIcon = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/GreenColor.png"));
            var greenMenuItem = RadialControllerMenuItem.CreateFromIcon("Green", greenIcon);
            greenMenuItem.Invoked += (item, args) => SetDialControlledSlider(GreenSlider);
            _dial.Menu.Items.Add(greenMenuItem);

            var blueIcon = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/BlueColor.png"));
            var blueMenuItem = RadialControllerMenuItem.CreateFromIcon("Blue", blueIcon);
            blueMenuItem.Invoked += (item, args) => SetDialControlledSlider(BlueSlider);
            _dial.Menu.Items.Add(blueMenuItem);

            _dial.ButtonClicked += (controller, args) => ApplyPreviewColor();
            _dial.RotationChanged += (controller, args) =>
            {

                if (_dialControlledSlider.Value + args.RotationDeltaInDegrees >= _dialControlledSlider.Minimum &&
                    _dialControlledSlider.Value + args.RotationDeltaInDegrees <= _dialControlledSlider.Maximum)
                {
                    _dialControlledSlider.Value += args.RotationDeltaInDegrees;
                }
            };
        }

        private void SetDialControlledSlider(Slider slider)
        {
            _dialControlledSlider = slider;
            _dialControlledSlider.Focus(FocusState.Programmatic);
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