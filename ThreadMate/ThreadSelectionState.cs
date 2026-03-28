namespace ThreadMate
{
    public sealed record SelectedThreadResult(
        string SourceThreadType,
        string FamilyName,
        bool IsImperial,
        string Label,
        double MajorDiameterMm,
        double PitchMm);

    public static class ThreadSelectionState
    {
        public static SelectedThreadResult? Current { get; private set; }

        public static event Action<SelectedThreadResult>? SelectionChanged;

        public static void Update(SelectedThreadResult result)
        {
            Current = result;
            SelectionChanged?.Invoke(result);
        }
    }
}
