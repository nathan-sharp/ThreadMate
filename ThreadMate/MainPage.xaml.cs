using System.Globalization;

namespace ThreadMate
{
    public partial class MainPage : ContentPage
    {
        private sealed record ThreadType(
            string Name,
            bool UsesTpi,
            string LengthUnit,
            string PitchLabel,
            string PitchPlaceholder,
            double PitchDiameterFactor,
            double MinorDiameterFactor,
            double ThreadHeightFactor,
            double StressAreaPitchFactor);

        private readonly List<ThreadType> _threadTypes =
        [
            new("ISO Metric (M)", false, "mm", "Pitch (mm)", "e.g. 1.5", 0.64952, 1.22687, 0.8660254, 0.9382),
            new("Unified UNC", true, "in", "Threads Per Inch (TPI)", "e.g. 13", 0.64952, 1.22687, 0.8660254, 0.9382),
            new("Unified UNF", true, "in", "Threads Per Inch (TPI)", "e.g. 20", 0.64952, 1.22687, 0.8660254, 0.9382),
            new("Unified UNEF", true, "in", "Threads Per Inch (TPI)", "e.g. 28", 0.64952, 1.22687, 0.8660254, 0.9382),
            new("Whitworth BSW", true, "in", "Threads Per Inch (TPI)", "e.g. 12", 0.640327, 1.280654, 0.960491, 1.0),
            new("Whitworth BSF", true, "in", "Threads Per Inch (TPI)", "e.g. 16", 0.640327, 1.280654, 0.960491, 1.0),
            new("BSPP (G)", true, "in", "Threads Per Inch (TPI)", "e.g. 19", 0.640327, 1.280654, 0.960491, 1.0),
            new("BSPT (R)", true, "in", "Threads Per Inch (TPI)", "e.g. 19", 0.640327, 1.280654, 0.960491, 1.0),
            new("NPT", true, "in", "Threads Per Inch (TPI)", "e.g. 18", 0.64952, 1.22687, 0.8660254, 0.9382),
            new("BA", false, "mm", "Pitch (mm)", "e.g. 0.9", 0.6, 1.2, 0.96, 1.0)
        ];

        public MainPage()
        {
            InitializeComponent();

            foreach (var threadType in _threadTypes)
            {
                ThreadTypePicker.Items.Add(threadType.Name);
            }

            ThreadTypePicker.SelectedIndex = 0;
            ApplyThreadTypeUi();

            SizeChanged += (_, _) => ApplyResponsiveLayout();
            ApplyResponsiveLayout();
        }

        private ThreadType SelectedThreadType => _threadTypes[Math.Max(ThreadTypePicker.SelectedIndex, 0)];

        private void OnCalculateClicked(object? sender, EventArgs e)
        {
            if (!TryParsePositive(MajorDiameterEntry.Text, out var majorDiameter) ||
                !TryParsePositive(PitchEntry.Text, out var pitchOrTpi) ||
                !TryParsePositive(LengthEntry.Text, out var length))
            {
                StatusLabel.Text = "Please enter valid positive numbers.";
                return;
            }

            var threadType = SelectedThreadType;
            var pitch = threadType.UsesTpi ? 1.0 / pitchOrTpi : pitchOrTpi;
            if (majorDiameter <= pitch)
            {
                StatusLabel.Text = threadType.UsesTpi
                    ? "Major diameter must be greater than pitch (1/TPI)."
                    : "Major diameter must be greater than pitch.";
                return;
            }

            var pitchDiameter = majorDiameter - (threadType.PitchDiameterFactor * pitch);
            var minorDiameter = majorDiameter - (threadType.MinorDiameterFactor * pitch);
            var threadHeight = threadType.ThreadHeightFactor * pitch;
            var threadsCount = length / pitch;
            var stressArea = (Math.PI / 4) * Math.Pow(majorDiameter - (threadType.StressAreaPitchFactor * pitch), 2);

            var lengthUnit = threadType.LengthUnit;
            var areaUnit = threadType.LengthUnit == "in" ? "in²" : "mm²";

            // Convert to mm for standard thread lookup
            var majorDiameterMm = threadType.LengthUnit == "in" ? majorDiameter * 25.4 : majorDiameter;
            var pitchMm = threadType.UsesTpi ? 25.4 / pitchOrTpi : pitchOrTpi;
            
            // Find the closest standard thread
            var closestThread = ThreadStandards.FindClosestThreadSize(threadType.Name, majorDiameterMm, pitchMm);
            var pitchFormatted = threadType.UsesTpi ? $"{pitchOrTpi:F2} TPI" : $"{pitchOrTpi:F3}mm";
            var threadDesignation = closestThread?.Label ?? $"{majorDiameter:F3}{lengthUnit} x {pitchFormatted}";

            var selectedMajorMm = closestThread?.MajorDiameterMm ?? majorDiameterMm;
            var selectedPitchMm = closestThread?.PitchMm ?? pitchMm;
            var familyName = ThreadStandards.MapFamilyName(threadType.Name);
            var family = ThreadStandards.GetFamilyByName(familyName);
            var isImperial = family?.IsImperial ?? threadType.UsesTpi;

            ThreadSelectionState.Update(new SelectedThreadResult(
                threadType.Name,
                familyName,
                isImperial,
                threadDesignation,
                selectedMajorMm,
                selectedPitchMm));

            StatusLabel.Text = $"Calculation complete for {threadType.Name}.";

            FinalThreadDesignationLabel.Text = $"Thread: {threadDesignation}";
            PitchDiameterLabel.Text = $"Pitch Diameter: {pitchDiameter:F3} {lengthUnit}";
            MinorDiameterLabel.Text = $"Minor Diameter: {minorDiameter:F3} {lengthUnit}";
            ThreadHeightLabel.Text = $"Thread Height: {threadHeight:F3} {lengthUnit}";
            ThreadsCountLabel.Text = $"Estimated Threads: {threadsCount:F2}";
            StressAreaLabel.Text = $"Tensile Stress Area: {stressArea:F4} {areaUnit}";
        }

        private void OnThreadTypeChanged(object? sender, EventArgs e)
        {
            ApplyThreadTypeUi();
        }

        private void ApplyThreadTypeUi()
        {
            var threadType = SelectedThreadType;
            CalculatorModeLabel.Text = $"Bolt Thread Calculator ({threadType.Name} External Thread)";

            MajorDiameterInputLabel.Text = $"Major Diameter ({threadType.LengthUnit})";
            MajorDiameterEntry.Placeholder = threadType.LengthUnit == "in" ? "e.g. 0.5" : "e.g. 10";

            PitchInputLabel.Text = threadType.PitchLabel;
            PitchEntry.Placeholder = threadType.PitchPlaceholder;

            LengthInputLabel.Text = $"Threaded Length ({threadType.LengthUnit})";
            LengthEntry.Placeholder = threadType.LengthUnit == "in" ? "e.g. 2" : "e.g. 30";

            StatusLabel.Text = "Enter values and tap Calculate.";
        }

        private void ApplyResponsiveLayout()
        {
            var isDesktop = Width >= 900;

            if (isDesktop)
            {
                MainLayoutGrid.ColumnDefinitions =
                [
                    new ColumnDefinition { Width = new GridLength(1.0, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(1.0, GridUnitType.Star) }
                ];
                MainLayoutGrid.RowDefinitions =
                [
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto }
                ];

                Grid.SetRow(HeaderCard, 0);
                Grid.SetColumn(HeaderCard, 0);
                Grid.SetColumnSpan(HeaderCard, 2);

                Grid.SetRow(InputCard, 1);
                Grid.SetColumn(InputCard, 0);
                Grid.SetColumnSpan(InputCard, 1);

                Grid.SetRow(ResultsCard, 1);
                Grid.SetColumn(ResultsCard, 1);
                Grid.SetColumnSpan(ResultsCard, 1);
            }
            else
            {
                MainLayoutGrid.ColumnDefinitions = [new ColumnDefinition { Width = GridLength.Star }];
                MainLayoutGrid.RowDefinitions =
                [
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto }
                ];

                Grid.SetRow(HeaderCard, 0);
                Grid.SetColumn(HeaderCard, 0);
                Grid.SetColumnSpan(HeaderCard, 1);

                Grid.SetRow(InputCard, 1);
                Grid.SetColumn(InputCard, 0);
                Grid.SetColumnSpan(InputCard, 1);

                Grid.SetRow(ResultsCard, 2);
                Grid.SetColumn(ResultsCard, 0);
                Grid.SetColumnSpan(ResultsCard, 1);
            }
        }

        private static bool TryParsePositive(string? value, out double result)
        {
            var isValid = double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result)
                || double.TryParse(value, NumberStyles.Float, CultureInfo.CurrentCulture, out result);
            return isValid && result > 0;
        }
    }
}
