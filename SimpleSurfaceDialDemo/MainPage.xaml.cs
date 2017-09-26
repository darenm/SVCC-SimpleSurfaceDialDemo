using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
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


        public static readonly DependencyProperty RadialDegreesProperty = DependencyProperty.Register(
            "RadialDegrees", typeof(double), typeof(MainPage), new PropertyMetadata(default(double)));

        public double RadialDegrees
        {
            get { return (double) GetValue(RadialDegreesProperty); }
            set { SetValue(RadialDegreesProperty, value); }
        }

        #endregion
        public MainPage()
        {
            BackgroundBrush = new SolidColorBrush(Colors.White);
            PreviewColorBrush = new SolidColorBrush(Colors.Black);
            InitializeComponent();
            Loaded += OnLoaded;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Clear menu
            var dialConfig = RadialControllerConfiguration.GetForCurrentView();

            // And just leave the volume control
            dialConfig.SetDefaultMenuItems(new[] {RadialControllerSystemMenuItemKind.Volume});

            _dial = RadialController.CreateForCurrentView();

            _dial.RotationResolutionInDegrees = 1; // default is 5
            _dial.UseAutomaticHapticFeedback = false; // if less than 5, disable Haptic feedback

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
            _dial.RotationChanged += DialRotationToColor;
            _dial.ButtonClicked += (controller, args) => ApplyPreviewColor();
        }

        private void DialRotationToSlider(object sender, RadialControllerRotationChangedEventArgs args)
        {
            if (_dialControlledSlider.Value + args.RotationDeltaInDegrees >= _dialControlledSlider.Minimum &&
                _dialControlledSlider.Value + args.RotationDeltaInDegrees <= _dialControlledSlider.Maximum)
                _dialControlledSlider.Value += args.RotationDeltaInDegrees;
        }

        private void DialRotationToColor(object sender, RadialControllerRotationChangedEventArgs args)
        {
            RadialDegrees += args.RotationDeltaInDegrees;
            RadialDegrees = RadialDegrees < 0 ? RadialDegrees + 360 : RadialDegrees;
            RadialDegrees = RadialDegrees % 360;
            var hue = 1.0 - (RadialDegrees / 360.0);
            const double sat = 1.0;
            const double val = 1.0;
            HsvToRgb(hue, sat, val, out var r, out var g, out var b);
            var color = Color.FromArgb(255, (byte) (r * 255), (byte) (g * 255), (byte) (b * 255));
            PreviewColorBrush = new SolidColorBrush(color);
        }

        private void SetDialControlledSlider(Slider slider)
        {
            _dial.RotationChanged -= DialRotationToColor;
            _dial.RotationChanged += DialRotationToSlider;

            _dialControlledSlider = slider;
            _dialControlledSlider.Focus(FocusState.Programmatic);
        }

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
            PreviewColorBrush = new SolidColorBrush(Color.FromArgb(255, (byte) RedSlider.Value,
                (byte) GreenSlider.Value, (byte) BlueSlider.Value));
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void UIElement_OnHolding(object sender, HoldingRoutedEventArgs e)
        {
            var dialConfig = RadialControllerConfiguration.GetForCurrentView();

            if (e.HoldingState == HoldingState.Started)
            {
                dialConfig.IsMenuSuppressed = true;
                dialConfig.ActiveControllerWhenMenuIsSuppressed = _dial;
                _dial.RotationResolutionInDegrees = 5;
                _dial.RotationChanged -= DialRotationToSlider;
                _dial.RotationChanged += DialRotationToColor;

                var position = e.GetPosition(this);
                FakeMenu.Margin = new Thickness(position.X - 240, position.Y - 240, 0, 0);
                FakeMenu.Visibility = Visibility.Visible;
            }
            else
            {
                dialConfig.IsMenuSuppressed = false;
                _dial.RotationResolutionInDegrees = 1;
                _dial.RotationChanged += DialRotationToSlider;
                _dial.RotationChanged -= DialRotationToColor;
                FakeMenu.Visibility = Visibility.Collapsed;
            }
        }

        public static void HsvToRgb(double h, double s, double v, out double r, out double g, out double b)
        {
            Debug.WriteLine($"Hue: {h}");
            if (h == 1.0)
                h = 0.0;

            var step = 1.0 / 6.0;
            var vh = h / step;

            var i = (int) Math.Floor(vh);

            var f = vh - i;
            var p = v * (1.0 - s);
            var q = v * (1.0 - s * f);
            var t = v * (1.0 - s * (1.0 - f));

            switch (i)
            {
                case 0:
                {
                    r = v;
                    g = t;
                    b = p;
                    break;
                }
                case 1:
                {
                    r = q;
                    g = v;
                    b = p;
                    break;
                }
                case 2:
                {
                    r = p;
                    g = v;
                    b = t;
                    break;
                }
                case 3:
                {
                    r = p;
                    g = q;
                    b = v;
                    break;
                }
                case 4:
                {
                    r = t;
                    g = p;
                    b = v;
                    break;
                }
                case 5:
                {
                    r = v;
                    g = p;
                    b = q;
                    break;
                }
                default:
                {
                    // not possible - if we get here it is an internal error
                    throw new ArgumentException();
                }
            }
        }
    }
}