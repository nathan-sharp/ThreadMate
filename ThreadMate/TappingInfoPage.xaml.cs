using ThreadMate.Services;

namespace ThreadMate
{
    public partial class TappingInfoPage : ContentPage
    {
        private readonly List<ThreadFamily> _threadFamilies = ThreadStandards.StandardFamilies;
        private SelectedThreadResult? _selectedThreadFromMain;
        private bool _isApplyingSharedSelection;

        public TappingInfoPage()
        {
            InitializeComponent();

            foreach (var family in _threadFamilies)
            {
                ThreadTypePicker.Items.Add(family.Name);
            }

            ThreadTypePicker.SelectedIndex = 0;
            PopulateSizes();

            SizeChanged += (_, _) => ApplyResponsiveLayout();
            ApplyResponsiveLayout();

            // Load banner ad
            AdMobService.LoadBannerAd("TappingInfoPageBanner", AdContainerGrid);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            ThreadSelectionState.SelectionChanged += OnSharedThreadSelectionChanged;
            ApplySharedSelection();
        }

        protected override void OnDisappearing()
        {
            ThreadSelectionState.SelectionChanged -= OnSharedThreadSelectionChanged;
            base.OnDisappearing();
        }

        private ThreadFamily SelectedFamily => _threadFamilies[Math.Max(ThreadTypePicker.SelectedIndex, 0)];

        private ThreadSize SelectedSize => SelectedFamily.Sizes[Math.Max(ThreadSizePicker.SelectedIndex, 0)];

        private void OnSharedThreadSelectionChanged(SelectedThreadResult result)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                _selectedThreadFromMain = result;
                ApplySharedSelection();
            });
        }

        private void ApplySharedSelection()
        {
            if (ThreadSelectionState.Current is null)
            {
                return;
            }

            _selectedThreadFromMain = ThreadSelectionState.Current;
            var result = _selectedThreadFromMain;
            if (result is null)
            {
                return;
            }

            _isApplyingSharedSelection = true;
            try
            {
                var familyIndex = _threadFamilies.FindIndex(f => f.Name == result.FamilyName);
                if (familyIndex >= 0 && ThreadTypePicker.SelectedIndex != familyIndex)
                {
                    ThreadTypePicker.SelectedIndex = familyIndex;
                    PopulateSizes();
                }
                else
                {
                    PopulateSizes();
                }

                var sizeIndex = SelectedFamily.Sizes
                    .Select((size, index) => new { size, index })
                    .FirstOrDefault(x => x.size.Label == result.Label)?.index ?? -1;

                if (sizeIndex >= 0)
                {
                    ThreadSizePicker.SelectedIndex = sizeIndex;
                }

                UpdateResults();
            }
            finally
            {
                _isApplyingSharedSelection = false;
            }
        }

        private void OnThreadTypeChanged(object? sender, EventArgs e)
        {
            if (!_isApplyingSharedSelection)
            {
                _selectedThreadFromMain = null;
            }

            PopulateSizes();
        }

        private void OnThreadSizeChanged(object? sender, EventArgs e)
        {
            if (!_isApplyingSharedSelection)
            {
                _selectedThreadFromMain = null;
            }

            UpdateResults();
        }

        private void PopulateSizes()
        {
            ThreadSizePicker.Items.Clear();
            foreach (var size in SelectedFamily.Sizes)
            {
                ThreadSizePicker.Items.Add(size.Label);
            }

            ThreadSizePicker.SelectedIndex = 0;
            UpdateResults();
        }

        private void UpdateResults()
        {
            if (ThreadSizePicker.SelectedIndex < 0)
            {
                return;
            }

            var family = SelectedFamily;
            var size = _selectedThreadFromMain is not null
                ? new ThreadSize(_selectedThreadFromMain.Label, _selectedThreadFromMain.MajorDiameterMm, _selectedThreadFromMain.PitchMm)
                : SelectedSize;

            var major = size.MajorDiameterMm;
            var pitch = size.PitchMm;
            var tpi = 25.4 / pitch;

            var tapDrill = major - pitch;
            var internalMinor = major - (1.08253 * pitch);
            var fullThreadDepth = 1.08253 * pitch;
            var threadPercent = Math.Clamp((major - tapDrill) / fullThreadDepth * 100.0, 0.0, 100.0);

            var isImperial = _selectedThreadFromMain?.IsImperial ?? family.IsImperial;
            var (clearanceClose, clearanceNormal, clearanceLoose) = GetClearanceHoles(major, isImperial);

            var internalPitchDiameter = major - (0.64952 * pitch);
            var externalPitchDiameter = major - (0.64952 * pitch);
            var externalMinorDiameter = major - (1.22687 * pitch);
            var externalThreadHeight = 0.8660254 * pitch;

            SelectedThreadSummaryLabel.Text = isImperial
                ? $"{size.Label}   ({tpi:F1} TPI)"
                : size.Label;

            FinalThreadDesignationLabel.Text = $"Thread: {size.Label}";

            TapDrillLabel.Text = $"Tap Drill: {FormatLength(tapDrill)}";
            ThreadEngagementLabel.Text = $"Estimated % Thread: {threadPercent:F0}% (with listed tap drill)";
            InternalMinorDiameterLabel.Text = $"Internal Minor Diameter (basic): {FormatLength(internalMinor)}";
            ClearanceCloseLabel.Text = $"Clearance Hole (Close): {FormatLength(clearanceClose)}";
            ClearanceNormalLabel.Text = $"Clearance Hole (Normal): {FormatLength(clearanceNormal)}";
            ClearanceLooseLabel.Text = $"Clearance Hole (Loose): {FormatLength(clearanceLoose)}";

            ExternalMajorLabel.Text = $"Major Diameter (nominal): {FormatLength(major)}";
            ExternalPitchLabel.Text = isImperial
                ? $"Pitch: {pitch:F3} mm ({tpi:F1} TPI)"
                : $"Pitch: {pitch:F3} mm";
            ExternalPitchDiameterLabel.Text = $"Pitch Diameter (basic): {FormatLength(externalPitchDiameter)}";
            ExternalMinorDiameterLabel.Text = $"Minor Diameter (basic): {FormatLength(externalMinorDiameter)}";
            ExternalThreadHeightLabel.Text = $"Thread Height (theoretical): {FormatLength(externalThreadHeight)}";

            // Generate SVG diagrams
            var internalSvg = ThreadDiagramSvgGenerator.GenerateInternalThreadDiagram(
                major, internalPitchDiameter, internalMinor);
            var textColor = Application.Current?.RequestedTheme == AppTheme.Dark ? "#FFFFFF" : "#000000";
            var internalHtml = $@"<html style='background:transparent;'><body style='margin:0;padding:0;background:transparent;color:{textColor};'>{internalSvg}</body></html>";
            InternalDiagramWebView.BackgroundColor = Colors.Transparent;
            InternalDiagramWebView.Source = new HtmlWebViewSource { Html = internalHtml };

            var externalSvg = ThreadDiagramSvgGenerator.GenerateExternalThreadDiagram(
                major, externalPitchDiameter, externalMinorDiameter);
            var externalHtml = $@"<html style='background:transparent;'><body style='margin:0;padding:0;background:transparent;color:{textColor};'>{externalSvg}</body></html>";
            ExternalDiagramWebView.BackgroundColor = Colors.Transparent;
            ExternalDiagramWebView.Source = new HtmlWebViewSource { Html = externalHtml };
        }

        private void ApplyResponsiveLayout()
        {
            var isDesktop = Width >= 1100;

            if (isDesktop)
            {
                TappingLayoutGrid.ColumnDefinitions =
                [
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Star }
                ];
                TappingLayoutGrid.RowDefinitions =
                [
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto }
                ];

                Grid.SetRow(AdContainerGrid, 0);
                Grid.SetColumn(AdContainerGrid, 0);
                Grid.SetColumnSpan(AdContainerGrid, 2);

                Grid.SetRow(SelectorCard, 1);
                Grid.SetColumn(SelectorCard, 0);
                Grid.SetColumnSpan(SelectorCard, 2);

                Grid.SetRow(InternalCard, 2);
                Grid.SetColumn(InternalCard, 0);
                Grid.SetColumnSpan(InternalCard, 1);

                Grid.SetRow(ExternalCard, 2);
                Grid.SetColumn(ExternalCard, 1);
                Grid.SetColumnSpan(ExternalCard, 1);
            }
            else
            {
                TappingLayoutGrid.ColumnDefinitions = [new ColumnDefinition { Width = GridLength.Star }];
                TappingLayoutGrid.RowDefinitions =
                [
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto }
                ];

                Grid.SetRow(AdContainerGrid, 0);
                Grid.SetColumn(AdContainerGrid, 0);
                Grid.SetColumnSpan(AdContainerGrid, 1);

                Grid.SetRow(SelectorCard, 1);
                Grid.SetColumn(SelectorCard, 0);
                Grid.SetColumnSpan(SelectorCard, 1);

                Grid.SetRow(InternalCard, 2);
                Grid.SetColumn(InternalCard, 0);
                Grid.SetColumnSpan(InternalCard, 1);

                Grid.SetRow(ExternalCard, 3);
                Grid.SetColumn(ExternalCard, 0);
                Grid.SetColumnSpan(ExternalCard, 1);
            }
        }

        private static (double close, double normal, double loose) GetClearanceHoles(double majorDiameterMm, bool isImperial)
        {
            if (isImperial)
            {
                var majorIn = majorDiameterMm / 25.4;
                var closeIn = majorIn + 0.003;
                var normalIn = majorIn + 0.008;
                var looseIn = majorIn + 0.015;
                return (closeIn * 25.4, normalIn * 25.4, looseIn * 25.4);
            }

            return (majorDiameterMm + 0.15, majorDiameterMm + 0.30, majorDiameterMm + 0.60);
        }

        private static string FormatLength(double millimeters)
        {
            var inches = millimeters / 25.4;
            return $"{millimeters:F3} mm ({inches:F4} in)";
        }
    }
}
