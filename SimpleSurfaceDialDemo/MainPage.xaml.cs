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
using Microsoft.Toolkit.Uwp;
using Microsoft.Toolkit.Uwp.Helpers;
using ColorHelper = Microsoft.Toolkit.Uwp.Helpers.ColorHelper;

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

            // And just leave the volume control and media controls
            dialConfig.SetDefaultMenuItems(new[]
            {
                RadialControllerSystemMenuItemKind.Volume,
                RadialControllerSystemMenuItemKind.NextPreviousTrack
            });

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

            // handling screen contact if supported - Surface Studio & Surface Pro 4
            _dial.ScreenContactStarted += DialOnScreenContactStarted;
            // raised if dial is moved while touching the screen
            _dial.ScreenContactContinued += DialOnScreenContactContinued;
            _dial.ScreenContactEnded += DialOnScreenContactEnded;
        }

        private void SetDialControlledSlider(Slider slider)
        {
            _dial.RotationChanged -= DialRotationToColor;
            _dial.RotationChanged += DialRotationToSlider;

            _dialControlledSlider = slider;
            _dialControlledSlider.Focus(FocusState.Programmatic);
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
            const double sat = 1.0;
            const double val = 1.0;
            var color = ColorHelper.FromHsv(360 - RadialDegrees, sat, val);
            RedSlider.Value = color.R;
            GreenSlider.Value = color.G;
            BlueSlider.Value = color.B;
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
            var color = Color.FromArgb(255, (byte) RedSlider.Value,
                (byte) GreenSlider.Value, (byte) BlueSlider.Value);
            PreviewColorBrush = new SolidColorBrush(color);
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region On Screen Contact
        private void DialOnScreenContactStarted(
            RadialController sender,
            RadialControllerScreenContactStartedEventArgs radialControllerScreenContactStartedEventArgs)
        {
            var contactCenter = radialControllerScreenContactStartedEventArgs.Contact.Position;
            var bounds = radialControllerScreenContactStartedEventArgs.Contact.Bounds;

            // position your UI or activate a region of controls as appropriate
        }

        private void DialOnScreenContactContinued(
            RadialController sender,
            RadialControllerScreenContactContinuedEventArgs radialControllerScreenContactContinuedEventArgs)
        {
            var contactCenter = radialControllerScreenContactContinuedEventArgs.Contact.Position;
            var bounds = radialControllerScreenContactContinuedEventArgs.Contact.Bounds;

            // update position your UI or activate a region of controls as appropriate
        }

        private void DialOnScreenContactEnded(
            RadialController sender,
            object o)
        {
            // reset your UI
        }

        #endregion

        private void UIElement_OnHolding(object sender, HoldingRoutedEventArgs e)
        {
            var dialConfig = RadialControllerConfiguration.GetForCurrentView();

            if (e.HoldingState == HoldingState.Started)
            {
                RadialDegrees = 360 - PreviewColorBrush.Color.ToHsl().H;

                // Allows the app to handle the push and hold activity
                dialConfig.IsMenuSuppressed = true;
                dialConfig.ActiveControllerWhenMenuIsSuppressed = _dial;

                _dial.RotationChanged -= DialRotationToSlider;
                _dial.RotationChanged += DialRotationToColor;

                // This places the on-screen radial menu centered on the touch location
                var position = e.GetPosition(this);
                OnScreenColorWheel.Margin = new Thickness(position.X - 240, position.Y - 240, 0, 0);
                OnScreenColorWheel.Visibility = Visibility.Visible;
            }
            else
            {
                dialConfig.IsMenuSuppressed = false;

                _dial.RotationChanged += DialRotationToSlider;
                _dial.RotationChanged -= DialRotationToColor;

                OnScreenColorWheel.Visibility = Visibility.Collapsed;
            }
        }
    }
}