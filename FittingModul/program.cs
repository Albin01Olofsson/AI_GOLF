using FittingModul;

var engine = new FittingEngine();

// Skapa en testspelare
var player = new PlayerProfile
{
    HeightCm = 188,
    WristToFloorCm = 95,
    HandLengthCm = 21,
    Handicap = 12.4,
    SwingSpeedMph = 98,
    TempoRatio = 2.6, // Snabbt tempo
    TypicalMiss = BallFlightTendency.Slice,
    LaunchPreference = TrajectoryNeed.Optimal
};

// Kör fitting-motorn (notera om din metod heter DriverFit eller FitDriver)
var recommendation = engine.DriverFit(player);

Console.WriteLine("=== REKOMMENDERAD SPECIFIKATION ===");
Console.WriteLine($"Skaftflex:      {recommendation.RecommendedFlex}");
Console.WriteLine($"Skaftvikt:      {recommendation.RecommendedShaftWeightGrams}g");
Console.WriteLine($"Längdjustering: {recommendation.LengthAdjustmentInches:+#.0;-#.0;0}\"");
Console.WriteLine($"Lie-justering:  {recommendation.LieAdjustmentDegrees:+#.0;-#.0;0}°");
Console.WriteLine($"Loft:           {recommendation.RecommendedLoftDegrees}°");
Console.WriteLine($"Huvudmodell:    {recommendation.HeadCategory}");
Console.WriteLine($"Grepp:          {recommendation.GripSize}");

Console.WriteLine("\nMotiveringar:");
foreach (var note in recommendation.FittingNotes)
{
    Console.WriteLine($" - {note}");
}

Console.ReadLine();