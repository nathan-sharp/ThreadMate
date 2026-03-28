using Microsoft.Maui.Graphics;

namespace ThreadMate
{
    public partial class TappingInfoPage : ContentPage
    {
        private readonly ThreadProfileDrawable _internalDrawable = new(true);
        private readonly ThreadProfileDrawable _externalDrawable = new(false);

        private readonly List<ThreadFamily> _threadFamilies = ThreadStandards.StandardFamilies;
        private SelectedThreadResult? _selectedThreadFromMain;
        private bool _isApplyingSharedSelection;

        public TappingInfoPage()
        {
            InitializeComponent();

            InternalDiagramView.Drawable = _internalDrawable;
            ExternalDiagramView.Drawable = _externalDrawable;

            foreach (var family in _threadFamilies)
            {
                ThreadTypePicker.Items.Add(family.Name);
            }

            ThreadTypePicker.SelectedIndex = 0;
            PopulateSizes();

            SizeChanged += (_, _) => ApplyResponsiveLayout();
            ApplyResponsiveLayout();
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

            _internalDrawable.SetValues(major, internalPitchDiameter, internalMinor, pitch, isImperial, "Internal thread profile");
            _externalDrawable.SetValues(major, externalPitchDiameter, externalMinorDiameter, pitch, isImperial, "External thread profile");

            InternalDiagramView.Invalidate();
            ExternalDiagramView.Invalidate();
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

                Grid.SetRow(TappingHeaderCard, 0);
                Grid.SetColumn(TappingHeaderCard, 0);
                Grid.SetColumnSpan(TappingHeaderCard, 2);

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

                Grid.SetRow(TappingHeaderCard, 0);
                Grid.SetColumn(TappingHeaderCard, 0);
                Grid.SetColumnSpan(TappingHeaderCard, 1);

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

        private sealed class ThreadProfileDrawable(bool isInternal) : IDrawable
        {
            private double _major;
            private double _pitchDiameter;
            private double _minor;
            private double _pitch;
            private bool _showImperial;
            private string _title = string.Empty;

            public void SetValues(double major, double pitchDiameter, double minor, double pitch, bool showImperial, string title)
            {
                _major = major;
                _pitchDiameter = pitchDiameter;
                _minor = minor;
                _pitch = pitch;
                _showImperial = showImperial;
                _title = title;
            }

            public void Draw(ICanvas canvas, RectF dirtyRect)
            {
                var isDark = Application.Current?.RequestedTheme == AppTheme.Dark;
                var textColor = isDark ? Colors.White : Colors.Black;
                var secondaryText = isDark ? Colors.LightGray : Colors.DarkSlateGray;
                var majorColor = Color.FromArgb("#FF4444");
                var pitchColor = Color.FromArgb("#4488FF");
                var minorColor = Color.FromArgb("#44BB44");
                var hatchColor = new Color(textColor.Red, textColor.Green, textColor.Blue, 0.35f);

                canvas.Antialias = true;
                canvas.StrokeColor = textColor;
                canvas.FontColor = textColor;
                canvas.StrokeSize = 1.5f;

                var left = 20f;
                var top = 20f;
                var diagramWidth = dirtyRect.Width - 40f;
                var diagramHeight = 140f;

                // Title
                canvas.FontSize = 14;
                canvas.FontColor = textColor;
                canvas.DrawString(isInternal ? "NUT (INTERNAL THREAD)" : "BOLT (EXTERNAL THREAD)", left, top, diagramWidth, 16, HorizontalAlignment.Left, VerticalAlignment.Top);

                var diagramTop = top + 20f;
                var diagramLeft = left + 20f;
                var profileWidth = diagramWidth * 0.55f;
                var profileRight = diagramLeft + profileWidth;

                // Draw reference lines
                var profileCenterY = diagramTop + 45f;
                canvas.StrokeSize = 0.5f;
                canvas.StrokeColor = secondaryText;
                canvas.StrokeDashPattern = [4, 4];
                canvas.DrawLine(diagramLeft, profileCenterY, profileRight, profileCenterY);
                canvas.StrokeDashPattern = null;
                canvas.StrokeSize = 1.5f;
                canvas.StrokeColor = textColor;

                // Draw thread profile with consistent spacing
                const int threadCount = 4;
                var threadSpacing = profileWidth / threadCount;
                var profileHeight = 60f;
                var toothHeight = profileHeight / 2f;

                DrawThreadDiagram(canvas, diagramLeft, profileCenterY - toothHeight, profileWidth, profileHeight, threadCount, hatchColor, textColor, isDark, isInternal);

                // Draw dimension lines and labels
                var labelLeft = profileRight + 30f;
                var labelWidth = dirtyRect.Width - labelLeft - 20f;
                DrawDimensionLabels(canvas, labelLeft, diagramTop, labelWidth, majorColor, pitchColor, minorColor, secondaryText);
            }

            private void DrawThreadDiagram(ICanvas canvas, float x, float topY, float width, float height, int threadCount, Color hatchColor, Color lineColor, bool isDark, bool isInternal)
            {
                var threadSpacing = width / threadCount;
                var crestY = topY;
                var rootY = topY + height;
                var centerY = (crestY + rootY) / 2f;

                // Draw hatching first
                canvas.StrokeColor = hatchColor;
                canvas.StrokeSize = 1f;
                const float hatchSpacing = 5f;
                const float hatchSlant = 6f;

                for (var i = 0; i < threadCount; i++)
                {
                    var threadX = x + (i * threadSpacing);
                    var threadXEnd = threadX + threadSpacing;
                    var midX = threadX + (threadSpacing / 2f);

                    if (isInternal)
                    {
                        // Hatch in the grooves
                        for (var hx = threadX - 10f; hx < threadXEnd + 10f; hx += hatchSpacing)
                        {
                            canvas.DrawLine(hx, rootY, hx + hatchSlant, centerY);
                        }
                    }
                    else
                    {
                        // Hatch in the crests
                        for (var hx = threadX - 10f; hx < midX + 10f; hx += hatchSpacing)
                        {
                            canvas.DrawLine(hx, crestY, hx + hatchSlant, centerY);
                        }
                    }
                }

                // Draw profile outline
                canvas.StrokeColor = lineColor;
                canvas.StrokeSize = 1.5f;

                for (var i = 0; i < threadCount; i++)
                {
                    var threadX = x + (i * threadSpacing);
                    var threadXEnd = threadX + threadSpacing;
                    var midX = threadX + (threadSpacing / 2f);

                    if (isInternal)
                    {
                        // Internal: inverted peaks
                        var prevMidX = i > 0 ? threadX - (threadSpacing / 2f) : threadX;
                        if (i > 0)
                        {
                            canvas.DrawLine(prevMidX, centerY, threadX, crestY);
                        }
                        canvas.DrawLine(threadX, crestY, midX, rootY);
                        canvas.DrawLine(midX, rootY, threadXEnd, crestY);
                        if (i < threadCount - 1)
                        {
                            canvas.DrawLine(threadXEnd, crestY, threadXEnd + (threadSpacing / 2f), centerY);
                        }
                    }
                    else
                    {
                        // External: peaks
                        if (i == 0)
                        {
                            canvas.DrawLine(threadX, rootY, threadX, crestY);
                        }
                        canvas.DrawLine(threadX, crestY, midX, rootY);
                        canvas.DrawLine(midX, rootY, threadXEnd, crestY);
                        if (i == threadCount - 1)
                        {
                            canvas.DrawLine(threadXEnd, crestY, threadXEnd, rootY);
                        }
                    }
                }

                // Draw base lines
                canvas.StrokeColor = lineColor;
                canvas.StrokeSize = 1.5f;
                if (isInternal)
                {
                    canvas.DrawLine(x, crestY, x + width, crestY);
                    canvas.DrawLine(x - 5f, crestY, x - 5f, rootY);
                    canvas.DrawLine(x + width + 5f, crestY, x + width + 5f, rootY);
                }
                else
                {
                    canvas.DrawLine(x, rootY, x + width, rootY);
                    canvas.DrawLine(x - 5f, crestY, x - 5f, rootY);
                    canvas.DrawLine(x + width + 5f, crestY, x + width + 5f, rootY);
                }
            }

            private void DrawDimensionLabels(ICanvas canvas, float x, float y, float width, Color majorColor, Color pitchColor, Color minorColor, Color textColor)
            {
                canvas.FontSize = 11;
                var lineHeight = 35f;
                var currentY = y + 15f;

                // Major Diameter
                DrawDimensionLine(canvas, x, currentY, majorColor);
                canvas.FontColor = majorColor;
                canvas.DrawString($"Major Ø", x + 20f, currentY - 4f, width - 20f, 12, HorizontalAlignment.Left, VerticalAlignment.Top);
                canvas.DrawString($"{_major:F3} mm", x + 20f, currentY + 8f, width - 20f, 12, HorizontalAlignment.Left, VerticalAlignment.Top);
                currentY += lineHeight;

                // Pitch Diameter
                DrawDimensionLine(canvas, x, currentY, pitchColor);
                canvas.FontColor = pitchColor;
                canvas.DrawString($"Pitch Ø", x + 20f, currentY - 4f, width - 20f, 12, HorizontalAlignment.Left, VerticalAlignment.Top);
                canvas.DrawString($"{_pitchDiameter:F3} mm", x + 20f, currentY + 8f, width - 20f, 12, HorizontalAlignment.Left, VerticalAlignment.Top);
                currentY += lineHeight;

                // Minor Diameter
                DrawDimensionLine(canvas, x, currentY, minorColor);
                canvas.FontColor = minorColor;
                canvas.DrawString($"Minor Ø", x + 20f, currentY - 4f, width - 20f, 12, HorizontalAlignment.Left, VerticalAlignment.Top);
                canvas.DrawString($"{_minor:F3} mm", x + 20f, currentY + 8f, width - 20f, 12, HorizontalAlignment.Left, VerticalAlignment.Top);
            }

            private static void DrawDimensionLine(ICanvas canvas, float x, float y, Color color)
            {
                canvas.StrokeColor = color;
                canvas.StrokeSize = 1.5f;
                canvas.DrawLine(x, y, x + 12f, y);
                canvas.DrawLine(x + 6f, y - 3f, x + 6f, y + 3f);
            }
        }
    }
}
