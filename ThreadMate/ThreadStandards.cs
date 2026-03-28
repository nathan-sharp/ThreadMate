namespace ThreadMate
{
    public sealed record ThreadSize(string Label, double MajorDiameterMm, double PitchMm);

    public sealed record ThreadFamily(string Name, bool IsImperial, IReadOnlyList<ThreadSize> Sizes);

    public static class ThreadStandards
    {
        public static readonly List<ThreadFamily> StandardFamilies =
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
                ]),
            new(
                "Unified UNEF",
                true,
                [
                    new("1/4-32 UNEF", 0.250 * 25.4, 25.4 / 32.0),
                    new("5/16-32 UNEF", 0.3125 * 25.4, 25.4 / 32.0),
                    new("3/8-32 UNEF", 0.375 * 25.4, 25.4 / 32.0),
                    new("1/2-28 UNEF", 0.500 * 25.4, 25.4 / 28.0)
                ]),
            new(
                "Whitworth BSW",
                true,
                [
                    new("1/8 BSW", 0.125 * 25.4, 25.4 / 40.0),
                    new("3/16 BSW", 0.1875 * 25.4, 25.4 / 24.0),
                    new("1/4 BSW", 0.250 * 25.4, 25.4 / 20.0),
                    new("5/16 BSW", 0.3125 * 25.4, 25.4 / 18.0),
                    new("3/8 BSW", 0.375 * 25.4, 25.4 / 16.0),
                    new("7/16 BSW", 0.4375 * 25.4, 25.4 / 14.0),
                    new("1/2 BSW", 0.500 * 25.4, 25.4 / 12.0)
                ]),
            new(
                "Whitworth BSF",
                true,
                [
                    new("1/4 BSF", 0.250 * 25.4, 25.4 / 26.0),
                    new("5/16 BSF", 0.3125 * 25.4, 25.4 / 22.0),
                    new("3/8 BSF", 0.375 * 25.4, 25.4 / 20.0),
                    new("7/16 BSF", 0.4375 * 25.4, 25.4 / 18.0),
                    new("1/2 BSF", 0.500 * 25.4, 25.4 / 16.0)
                ]),
            new(
                "BSPP (G)",
                true,
                [
                    new("1/8 BSPP", 0.125 * 25.4, 25.4 / 28.0),
                    new("1/4 BSPP", 0.250 * 25.4, 25.4 / 19.0),
                    new("3/8 BSPP", 0.375 * 25.4, 25.4 / 19.0),
                    new("1/2 BSPP", 0.500 * 25.4, 25.4 / 14.0)
                ]),
            new(
                "BSPT (R)",
                true,
                [
                    new("1/8 BSPT", 0.125 * 25.4, 25.4 / 28.0),
                    new("1/4 BSPT", 0.250 * 25.4, 25.4 / 19.0),
                    new("3/8 BSPT", 0.375 * 25.4, 25.4 / 19.0),
                    new("1/2 BSPT", 0.500 * 25.4, 25.4 / 14.0)
                ]),
            new(
                "NPT",
                true,
                [
                    new("1/8 NPT", 0.125 * 25.4, 25.4 / 27.0),
                    new("1/4 NPT", 0.250 * 25.4, 25.4 / 18.0),
                    new("3/8 NPT", 0.375 * 25.4, 25.4 / 18.0),
                    new("1/2 NPT", 0.500 * 25.4, 25.4 / 14.0)
                ]),
            new(
                "BA",
                false,
                [
                    new("BA 0", 6.0, 0.9),
                    new("BA 2", 5.0, 0.75),
                    new("BA 4", 3.625, 0.6),
                    new("BA 6", 2.642, 0.45)
                ])
        ];

        public static string MapFamilyName(string familyName) => familyName switch
        {
            "ISO Metric (M)" => "ISO Metric",
            "Unified UNC" => "Imperial UNC",
            "Unified UNF" => "Imperial UNF",
            "Unified UNEF" => "Unified UNEF",
            "Whitworth BSW" => "Whitworth BSW",
            "Whitworth BSF" => "Whitworth BSF",
            "BSPP (G)" => "BSPP (G)",
            "BSPT (R)" => "BSPT (R)",
            "NPT" => "NPT",
            "BA" => "BA",
            _ => familyName
        };

        public static ThreadFamily? GetFamilyByName(string familyName)
        {
            var mappedFamilyName = MapFamilyName(familyName);
            return StandardFamilies.FirstOrDefault(f => f.Name == mappedFamilyName);
        }

        /// <summary>
        /// Finds the closest standard thread size for the given family name, major diameter, and pitch.
        /// </summary>
        public static ThreadSize? FindClosestThreadSize(string familyName, double majorDiameterMm, double pitchMm)
        {
            var family = GetFamilyByName(familyName);
            if (family == null || family.Sizes.Count == 0)
                return null;

            const double diameterTolerancePercent = 0.5;
            const double pitchTolerancePercent = 1.0;

            var bestMatch = family.Sizes[0];
            var bestScore = double.MaxValue;

            foreach (var size in family.Sizes)
            {
                var diameterError = Math.Abs(size.MajorDiameterMm - majorDiameterMm);
                var pitchError = Math.Abs(size.PitchMm - pitchMm);

                // Calculate a weighted score based on percentage errors
                var diameterErrorPercent = (diameterError / size.MajorDiameterMm) * 100;
                var pitchErrorPercent = (pitchError / size.PitchMm) * 100;

                // Only consider sizes within tolerance
                if (diameterErrorPercent <= diameterTolerancePercent && pitchErrorPercent <= pitchTolerancePercent)
                {
                    var score = diameterErrorPercent + pitchErrorPercent;
                    if (score < bestScore)
                    {
                        bestScore = score;
                        bestMatch = size;
                    }
                }
            }

            // Return the best match only if it's within a reasonable tolerance
            return bestScore < double.MaxValue ? bestMatch : null;
        }
    }
}
