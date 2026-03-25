using System.Globalization;

namespace ThreadMate
{
    public partial class TorqueInfoPage : ContentPage
    {
        private sealed record ThreadSize(string Label, double MajorDiameterMm, double PitchMm);

        private sealed record ThreadFamily(string Name, bool IsImperial, IReadOnlyList<ThreadSize> Sizes, IReadOnlyList<GradeOption> Grades);

        private sealed record GradeOption(string Name, double ProofStressMpa);

        private sealed record ConditionOption(string Name, double NutFactorK);

        private readonly List<ThreadFamily> _threadFamilies =
        [
            new(
                "ISO Metric",
                false,
                [
                    new("M5 x 0.8", 5.0, 0.8),
                    new("M6 x 1.0", 6.0, 1.0),
                    new("M8 x 1.25", 8.0, 1.25),
                    new("M10 x 1.5", 10.0, 1.5),
                    new("M12 x 1.75", 12.0, 1.75),
                    new("M16 x 2.0", 16.0, 2.0)
                ],
                [
                    new("Class 8.8", 600),
                    new("Class 10.9", 830),
                    new("Class 12.9", 970),
                    new("A2-70 Stainless", 450)
                ]),
            new(
                "Imperial UNC",
                true,
                [
                    new("#10-24 UNC", 0.190 * 25.4, 25.4 / 24.0),
                    new("1/4-20 UNC", 0.250 * 25.4, 25.4 / 20.0),
                    new("5/16-18 UNC", 0.3125 * 25.4, 25.4 / 18.0),
                    new("3/8-16 UNC", 0.375 * 25.4, 25.4 / 16.0),
                    new("1/2-13 UNC", 0.500 * 25.4, 25.4 / 13.0)
                ],
                [
                    new("Grade 5", 85 * 6.89476),
                    new("Grade 8", 120 * 6.89476),
                    new("18-8 Stainless", 65 * 6.89476)
                ]),
            new(
                "Imperial UNF",
                true,
                [
                    new("#10-32 UNF", 0.190 * 25.4, 25.4 / 32.0),
                    new("1/4-28 UNF", 0.250 * 25.4, 25.4 / 28.0),
                    new("5/16-24 UNF", 0.3125 * 25.4, 25.4 / 24.0),
                    new("3/8-24 UNF", 0.375 * 25.4, 25.4 / 24.0),
                    new("1/2-20 UNF", 0.500 * 25.4, 25.4 / 20.0)
                ],
                [
                    new("Grade 5", 85 * 6.89476),
                    new("Grade 8", 120 * 6.89476),
                    new("18-8 Stainless", 65 * 6.89476)
                ])
        ];

        private readonly List<ConditionOption> _conditions =
        [
            new("Dry", 0.20),
            new("Light Oil", 0.17),
            new("Lubricated", 0.15),
            new("Anti-Seize", 0.12)
        ];

        public TorqueInfoPage()
        {
            InitializeComponent();

            foreach (var family in _threadFamilies)
            {
                ThreadTypePicker.Items.Add(family.Name);
            }

            foreach (var condition in _conditions)
            {
                ConditionPicker.Items.Add(condition.Name);
            }

            ThreadTypePicker.SelectedIndex = 0;
            ConditionPicker.SelectedIndex = 0;
            PopulateDependentPickers();

            SizeChanged += (_, _) => ApplyResponsiveLayout();
            ApplyResponsiveLayout();
        }

        private ThreadFamily SelectedFamily => _threadFamilies[Math.Max(ThreadTypePicker.SelectedIndex, 0)];

        private ThreadSize SelectedSize => SelectedFamily.Sizes[Math.Max(ThreadSizePicker.SelectedIndex, 0)];

        private GradeOption SelectedGrade => SelectedFamily.Grades[Math.Max(GradePicker.SelectedIndex, 0)];

        private ConditionOption SelectedCondition => _conditions[Math.Max(ConditionPicker.SelectedIndex, 0)];

        private void OnThreadTypeChanged(object? sender, EventArgs e)
        {
            PopulateDependentPickers();
        }

        private void OnAnyInputChanged(object? sender, EventArgs e)
        {
            CalculateTorque();
        }

        private void OnPreloadTextChanged(object? sender, TextChangedEventArgs e)
        {
            CalculateTorque();
        }

        private void PopulateDependentPickers()
        {
            ThreadSizePicker.Items.Clear();
            GradePicker.Items.Clear();

            foreach (var size in SelectedFamily.Sizes)
            {
                ThreadSizePicker.Items.Add(size.Label);
            }

            foreach (var grade in SelectedFamily.Grades)
            {
                GradePicker.Items.Add(grade.Name);
            }

            ThreadSizePicker.SelectedIndex = 0;
            GradePicker.SelectedIndex = 0;
            CalculateTorque();
        }

        private void CalculateTorque()
        {
            if (ThreadSizePicker.SelectedIndex < 0 || GradePicker.SelectedIndex < 0 || ConditionPicker.SelectedIndex < 0)
            {
                return;
            }

            var preloadPercent = ParsePreloadPercent(PreloadEntry.Text);
            if (preloadPercent <= 0 || preloadPercent > 100)
            {
                StatusLabel.Text = "Enter a preload value from 1 to 100.";
                return;
            }

            var size = SelectedSize;
            var grade = SelectedGrade;
            var condition = SelectedCondition;
            var family = SelectedFamily;

            var stressAreaMm2 = (Math.PI / 4.0) * Math.Pow(size.MajorDiameterMm - (0.9382 * size.PitchMm), 2);
            var proofLoadN = stressAreaMm2 * grade.ProofStressMpa;
            var targetClampLoadN = proofLoadN * (preloadPercent / 100.0);
            var torqueNm = condition.NutFactorK * targetClampLoadN * (size.MajorDiameterMm / 1000.0);

            var torqueFtLb = torqueNm * 0.7375621493;
            var lowNm = torqueNm * 0.9;
            var highNm = torqueNm * 1.1;

            var majorIn = size.MajorDiameterMm / 25.4;
            var tpi = 25.4 / size.PitchMm;

            StatusLabel.Text = $"Calculated for {size.Label}, {grade.Name}, {condition.Name}, {preloadPercent:F0}% preload.";

            RecommendedTorqueNmLabel.Text = $"Torque: {torqueNm:F1} N·m";
            RecommendedTorqueFtLbLabel.Text = $"Torque: {torqueFtLb:F1} ft·lb";
            TorqueRangeLabel.Text = $"Suggested Working Range (±10%): {lowNm:F1} to {highNm:F1} N·m";

            NominalDiameterLabel.Text = $"Nominal Diameter: {size.MajorDiameterMm:F3} mm ({majorIn:F4} in)";
            PitchLabel.Text = family.IsImperial
                ? $"Pitch: {size.PitchMm:F3} mm ({tpi:F1} TPI)"
                : $"Pitch: {size.PitchMm:F3} mm";
            StressAreaLabel.Text = $"Tensile Stress Area: {stressAreaMm2:F2} mm²";
            ClampLoadLabel.Text = $"Estimated Clamp Load: {targetClampLoadN:F0} N";
        }

        private void ApplyResponsiveLayout()
        {
            var isDesktop = Width >= 1050;

            if (isDesktop)
            {
                TorqueLayoutGrid.ColumnDefinitions =
                [
                    new ColumnDefinition { Width = new GridLength(1.0, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(1.0, GridUnitType.Star) }
                ];
                TorqueLayoutGrid.RowDefinitions =
                [
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto }
                ];

                Grid.SetRow(TorqueHeaderCard, 0);
                Grid.SetColumn(TorqueHeaderCard, 0);
                Grid.SetColumnSpan(TorqueHeaderCard, 2);

                Grid.SetRow(TorqueInputCard, 1);
                Grid.SetColumn(TorqueInputCard, 0);
                Grid.SetRowSpan(TorqueInputCard, 2);

                Grid.SetRow(RecommendedCard, 1);
                Grid.SetColumn(RecommendedCard, 1);
                Grid.SetRowSpan(RecommendedCard, 1);

                Grid.SetRow(SupportingCard, 2);
                Grid.SetColumn(SupportingCard, 1);

                Grid.SetRow(NoteCard, 3);
                Grid.SetColumn(NoteCard, 0);
                Grid.SetColumnSpan(NoteCard, 2);
            }
            else
            {
                TorqueLayoutGrid.ColumnDefinitions = [new ColumnDefinition { Width = GridLength.Star }];
                TorqueLayoutGrid.RowDefinitions =
                [
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto }
                ];

                Grid.SetRow(TorqueHeaderCard, 0);
                Grid.SetColumn(TorqueHeaderCard, 0);
                Grid.SetColumnSpan(TorqueHeaderCard, 1);

                Grid.SetRow(TorqueInputCard, 1);
                Grid.SetColumn(TorqueInputCard, 0);
                Grid.SetRowSpan(TorqueInputCard, 1);

                Grid.SetRow(RecommendedCard, 2);
                Grid.SetColumn(RecommendedCard, 0);

                Grid.SetRow(SupportingCard, 3);
                Grid.SetColumn(SupportingCard, 0);

                Grid.SetRow(NoteCard, 4);
                Grid.SetColumn(NoteCard, 0);
                Grid.SetColumnSpan(NoteCard, 1);
            }
        }

        private static double ParsePreloadPercent(string? text)
        {
            if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value) ||
                double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out value))
            {
                return value;
            }

            return 0;
        }
    }
}
