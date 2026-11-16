namespace FittingModul
{
    public class ClubSpecification
    {
        // Skaftegenskaper
        public Shaftflex RecommendedFlex { get; set; }
        public int RecommendedShaftWeightGrams { get; set; } // T.ex. 65 för grafit, 105 för stål
        public double LengthAdjustmentInches { get; set; }   // T.ex. +0.5 eller -0.25 tum relativt standard

        // Huvudspecifikationer
        public double LieAdjustmentDegrees { get; set; }     // T.ex. +1.0 (Upright) eller -1.0 (Flat)
        public double RecommendedLoftDegrees { get; set; }   // T.ex. 10.5 grader
        public string HeadCategory { get; set; } = string.Empty; // "Game Improvement", "Players" etc.

        // Grepp
        public string GripSize { get; set; } = "Standard";   // "Undersize", "Standard", "Midsize"

        // Förklaringsmodell (Viktigt för transparens!)
        public List<string> FittingNotes { get; set; } = new();
    }
}
