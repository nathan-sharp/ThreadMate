using Microsoft.Maui.Graphics;

namespace ThreadMate
{
    public partial class TappingInfoPage : ContentPage
    {
        private sealed record ThreadSize(string Label, double MajorDiameterMm, double PitchMm);

        private sealed record ThreadFamily(string Name, bool IsImperial, IReadOnlyList<ThreadSize> Sizes);

        private readonly ThreadProfileDrawable _internalDrawable = new(true);
        private readonly ThreadProfileDrawable _externalDrawable = new(false);

        private readonly List<ThreadFamily> _threadFamilies =
        [
            new(
                "ISO Metric",
                false,
                [
                    new("M3 x 0.5", 3.0, 0.5),
                    new("M4 x 0.7", 4.0, 0.7),
                    new("M5 x 0.8", 5.0, 0.8),
                    new("M6 x 1.0", 6.0, 1.0),
                    new("M8 x 1.25", 8.0, 1.25),
                    new("M10 x 1.5", 10.0, 1.5),
                    new("M12 x 1.75", 12.0, 1.75),
                    new("M16 x 2.0", 16.0, 2.0)
                ]),
            new(
                "Imperial UNC",
                true,
                [
                    new("#4-40 UNC", 0.112 * 25.4, 25.4 / 40.0),
                    new("#6-32 UNC", 0.138 * 25.4, 25.4 / 32.0),
                    new("#8-32 UNC", 0.164 * 25.4, 25.4 / 32.0),
                    new("#10-24 UNC", 0.190 * 25.4, 25.4 / 24.0),
                    new("1/4-20 UNC", 0.250 * 25.4, 25.4 / 20.0),
                    new("5/16-18 UNC", 0.3125 * 25.4, 25.4 / 18.0),
                    new("3/8-16 UNC", 0.375 * 25.4, 25.4 / 16.0),
                    new("1/2-13 UNC", 0.500 * 25.4, 25.4 / 13.0)
                ]),
            new(
                "Imperial UNF",
                true,
                [
                    new("#10-32 UNF", 0.190 * 25.4, 25.4 / 32.0),
                    new("1/4-28 UNF", 0.250 * 25.4, 25.4 / 28.0),
                    new("5/16-24 UNF", 0.3125 * 25.4, 25.4 / 24.0),
                    new("3/8-24 UNF", 0.375 * 25.4, 25.4 / 24.0),
                    new("7/16-20 UNF", 0.4375 * 25.4, 25.4 / 20.0),
                    new("1/2-20 UNF", 0.500 * 25.4, 25.4 / 20.0)
                ])
        ];

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

        private ThreadFamily SelectedFamily => _threadFamilies[Math.Max(ThreadTypePicker.SelectedIndex, 0)];

        private ThreadSize SelectedSize => SelectedFamily.Sizes[Math.Max(ThreadSizePicker.SelectedIndex, 0)];

        private void OnThreadTypeChanged(object? sender, EventArgs e)
        {
            PopulateSizes();
        }

        private void OnThreadSizeChanged(object? sender, EventArgs e)
        {
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
            var size = SelectedSize;

            var major = size.MajorDiameterMm;
            var pitch = size.PitchMm;
            var tpi = 25.4 / pitch;

            var tapDrill = major - pitch;
            var internalMinor = major - (1.08253 * pitch);
            var fullThreadDepth = 1.08253 * pitch;
            var threadPercent = Math.Clamp((major - tapDrill) / fullThreadDepth * 100.0, 0.0, 100.0);

            var (clearanceClose, clearanceNormal, clearanceLoose) = GetClearanceHoles(major, family.IsImperial);

            var internalPitchDiameter = major - (0.64952 * pitch);
            var externalPitchDiameter = major - (0.64952 * pitch);
            var externalMinorDiameter = major - (1.22687 * pitch);
            var externalThreadHeight = 0.8660254 * pitch;

            SelectedThreadSummaryLabel.Text = family.IsImperial
                ? $"{size.Label}   ({tpi:F1} TPI)"
                : size.Label;

            TapDrillLabel.Text = $"Tap Drill: {FormatLength(tapDrill)}";
            ThreadEngagementLabel.Text = $"Estimated % Thread: {threadPercent:F0}% (with listed tap drill)";
            InternalMinorDiameterLabel.Text = $"Internal Minor Diameter (basic): {FormatLength(internalMinor)}";
            ClearanceCloseLabel.Text = $"Clearance Hole (Close): {FormatLength(clearanceClose)}";
            ClearanceNormalLabel.Text = $"Clearance Hole (Normal): {FormatLength(clearanceNormal)}";
            ClearanceLooseLabel.Text = $"Clearance Hole (Loose): {FormatLength(clearanceLoose)}";

            ExternalMajorLabel.Text = $"Major Diameter (nominal): {FormatLength(major)}";
            ExternalPitchLabel.Text = family.IsImperial
                ? $"Pitch: {pitch:F3} mm ({tpi:F1} TPI)"
                : $"Pitch: {pitch:F3} mm";
            ExternalPitchDiameterLabel.Text = $"Pitch Diameter (basic): {FormatLength(externalPitchDiameter)}";
            ExternalMinorDiameterLabel.Text = $"Minor Diameter (basic): {FormatLength(externalMinorDiameter)}";
            ExternalThreadHeightLabel.Text = $"Thread Height (theoretical): {FormatLength(externalThreadHeight)}";

            _internalDrawable.SetValues(major, internalPitchDiameter, internalMinor, pitch, family.IsImperial, "Internal thread profile");
            _externalDrawable.SetValues(major, externalPitchDiameter, externalMinorDiameter, pitch, family.IsImperial, "External thread profile");

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
                var accent = Color.FromArgb("#FFD400");
                var hatchColor = new Color(textColor.Red, textColor.Green, textColor.Blue, 0.32f);

                var fullHeightH = 0.8660254 * _pitch;
                var pHalf = _pitch / 2.0;
                var pQuarter = _pitch / 4.0;
                var h8 = fullHeightH / 8.0;
                var h6 = fullHeightH / 6.0;
                var h4 = fullHeightH / 4.0;
                var h58 = (5.0 / 8.0) * fullHeightH;
                var h1724 = (17.0 / 24.0) * fullHeightH;
                var rootRadiusApprox = 0.1443 * _pitch;

                canvas.Antialias = true;
                canvas.StrokeColor = textColor;
                canvas.FontColor = textColor;
                canvas.StrokeSize = 1.4f;

                var compactLayout = dirtyRect.Width < 640;
                var left = 12f;
                var right = dirtyRect.Width - 12f;
                var top = 6f;
                var infoPanelWidth = compactLayout ? 0f : 180f;

                var xStart = left + 24f;
                var xEnd = right - infoPanelWidth - 18f;
                if (xEnd - xStart < 180f)
                {
                    xEnd = right - 18f;
                }

                var crestY = 74f;
                var pitchY = 112f;
                var rootY = 150f;
                var axisBottom = 200f;

                canvas.FontSize = 12;
                canvas.DrawString(isInternal ? "NUT (INTERNAL THREAD)" : "BOLT (EXTERNAL THREAD)", left, top, 280, 18, HorizontalAlignment.Left, VerticalAlignment.Top);
                canvas.FontSize = 10;
                canvas.DrawString(_title, left, top + 18, 280, 16, HorizontalAlignment.Left, VerticalAlignment.Top);

                var span = xEnd - xStart;
                const int toothCount = 3;
                var toothWidth = span / toothCount;
                var axisX = (xStart + xEnd) / 2f;

                if (isInternal)
                {
                    DrawHatchedBand(canvas, xStart, xEnd, 56f, rootY, hatchColor, textColor, 1.4f);

                    canvas.DrawLine(xStart, 56f, xEnd, 56f);
                    canvas.DrawLine(xStart, 56f, xStart, rootY);
                    canvas.DrawLine(xEnd, 56f, xEnd, rootY);
                    DrawThreadProfile(canvas, xStart, xEnd, rootY, crestY, toothWidth);
                }
                else
                {
                    DrawHatchedBand(canvas, xStart, xEnd, crestY, axisBottom - 6f, hatchColor, textColor, 1.4f);

                    DrawThreadProfile(canvas, xStart, xEnd, crestY, rootY, toothWidth);
                    canvas.DrawLine(xStart, axisBottom - 6f, xEnd, axisBottom - 6f);
                    canvas.DrawLine(xStart, axisBottom - 6f, xStart, crestY);
                    canvas.DrawLine(xEnd, axisBottom - 6f, xEnd, crestY);
                }

                canvas.StrokeDashPattern = [4, 4];
                canvas.StrokeColor = secondaryText;
                canvas.DrawLine(xStart, pitchY, xEnd, pitchY);
                canvas.DrawLine(axisX, 56f, axisX, axisBottom);
                canvas.StrokeDashPattern = null;
                canvas.StrokeColor = textColor;

                canvas.FontSize = 9;
                canvas.FontColor = secondaryText;
                canvas.DrawString("PITCH LINE", xEnd - 80, pitchY - 10, 80, 14, HorizontalAlignment.Left, VerticalAlignment.Top);
                canvas.DrawString(isInternal ? "AXIS OF NUT" : "AXIS OF BOLT", axisX - 34, axisBottom + 2, 90, 14, HorizontalAlignment.Left, VerticalAlignment.Top);

                canvas.FontColor = textColor;
                canvas.DrawString("30°", axisX - toothWidth * 0.32f, pitchY - 17, 28, 14, HorizontalAlignment.Left, VerticalAlignment.Top);
                canvas.DrawString("30°", axisX + toothWidth * 0.12f, pitchY - 17, 28, 14, HorizontalAlignment.Left, VerticalAlignment.Top);

                DrawVerticalDimension(canvas, xStart - 20f, crestY, rootY, "H");
                DrawHorizontalDimension(canvas, axisX - toothWidth * 0.5f, axisX + toothWidth * 0.5f, 176f, "P");
                DrawHorizontalDimension(canvas, axisX, axisX + toothWidth * 0.5f, 192f, "P/2");
                if (isInternal)
                {
                    DrawHorizontalDimension(canvas, axisX + toothWidth * 0.5f, axisX + toothWidth * 0.75f, 208f, "P/4");
                }

                var primaryLines = new List<string>
                {
                    _showImperial ? $"P = {_pitch:F3} mm ({25.4 / _pitch:F1} TPI)" : $"P = {_pitch:F3} mm",
                    $"H = {FormatMetric(fullHeightH)}",
                    $"P/2 = {FormatMetric(pHalf)}",
                    isInternal ? $"P/4 = {FormatMetric(pQuarter)}" : $"0.1443P = {FormatMetric(rootRadiusApprox)}",
                    isInternal ? $"5/8H = {FormatMetric(h58)}" : $"17/24H = {FormatMetric(h1724)}",
                    isInternal ? $"H/8 = {FormatMetric(h8)}" : $"H/6 = {FormatMetric(h6)}"
                };

                var diameterLines = isInternal
                    ? new List<string>
                    {
                        $"D  (major): {FormatMetric(_major)}",
                        $"D2 (pitch): {FormatMetric(_pitchDiameter)}",
                        $"D1 (minor): {FormatMetric(_minor)}",
                        $"H/4 = {FormatMetric(h4)}",
                        $"Angle = 60°"
                    }
                    : new List<string>
                    {
                        $"d  (major): {FormatMetric(_major)}",
                        $"d2 (pitch): {FormatMetric(_pitchDiameter)}",
                        $"d3 (minor): {FormatMetric(_minor)}",
                        $"Core depth ≈ {FormatMetric(Math.Abs(_major - _minor) / 2)}",
                        $"Angle = 60°"
                    };

                if (compactLayout)
                {
                    var baseY = 222f;
                    DrawInfoList(canvas, left, baseY, (dirtyRect.Width / 2f) - 14f, primaryLines, secondaryText);
                    DrawInfoList(canvas, (dirtyRect.Width / 2f) + 4f, baseY, (dirtyRect.Width / 2f) - 16f, diameterLines, accent);
                }
                else
                {
                    DrawInfoList(canvas, xEnd + 8f, 40f, infoPanelWidth - 8f, diameterLines, accent);
                    DrawInfoList(canvas, left, 224f, xEnd - left, primaryLines, secondaryText);
                }
            }

            private static void DrawHatchedBand(ICanvas canvas, float xStart, float xEnd, float yTop, float yBottom, Color hatchColor, Color lineColor, float lineStrokeSize)
            {
                if (yBottom <= yTop)
                {
                    return;
                }

                canvas.StrokeColor = hatchColor;
                canvas.StrokeSize = 1f;

                const float spacing = 8f;
                const float slant = 10f;

                for (var x = xStart - 30f; x < xEnd + 30f; x += spacing)
                {
                    canvas.DrawLine(x, yBottom, x + slant, yTop);
                }

                canvas.StrokeColor = lineColor;
                canvas.StrokeSize = lineStrokeSize;
            }

            private static void DrawInfoList(ICanvas canvas, float x, float y, float width, IReadOnlyList<string> lines, Color color)
            {
                canvas.FontColor = color;
                canvas.FontSize = 9;
                for (var i = 0; i < lines.Count; i++)
                {
                    canvas.DrawString(lines[i], x, y + (i * 14f), width, 14f, HorizontalAlignment.Left, VerticalAlignment.Top);
                }
            }

            private static string FormatMetric(double mm) => $"{mm:F3} mm";

            private static void DrawThreadProfile(ICanvas canvas, float xStart, float xEnd, float crestY, float rootY, float toothWidth)
            {
                var x = xStart;
                var isPeak = true;
                var currentY = crestY;

                while (x < xEnd)
                {
                    var x1 = x + (toothWidth / 2f);
                    var x2 = Math.Min(x + toothWidth, xEnd);
                    var midY = isPeak ? rootY : crestY;
                    var endY = isPeak ? crestY : rootY;

                    canvas.DrawLine(x, currentY, x1, midY);
                    canvas.DrawLine(x1, midY, x2, endY);

                    x = x2;
                    currentY = endY;
                    isPeak = !isPeak;
                }
            }

            private static void DrawVerticalDimension(ICanvas canvas, float x, float y1, float y2, string label)
            {
                canvas.DrawLine(x, y1, x, y2);
                canvas.DrawLine(x - 4, y1, x + 4, y1);
                canvas.DrawLine(x - 4, y2, x + 4, y2);
                canvas.FontSize = 9;
                canvas.DrawString(label, x + 4, (y1 + y2) / 2f - 6f, 20, 12, HorizontalAlignment.Left, VerticalAlignment.Top);
            }

            private static void DrawHorizontalDimension(ICanvas canvas, float x1, float x2, float y, string label)
            {
                canvas.DrawLine(x1, y, x2, y);
                canvas.DrawLine(x1, y - 3, x1, y + 3);
                canvas.DrawLine(x2, y - 3, x2, y + 3);
                canvas.FontSize = 9;
                canvas.DrawString(label, x1, y + 2, x2 - x1 + 20, 12, HorizontalAlignment.Left, VerticalAlignment.Top);
            }
        }
    }
}
