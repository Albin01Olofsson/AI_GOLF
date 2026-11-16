namespace FittingModul
{
    public class FittingEngine
    {
        public ClubSpecification DriverFit(PlayerProfile player)
        {
            var spec = new ClubSpecification();

            if (player.SwingSpeedMph < 75)
            {
                spec.RecommendedFlex = Shaftflex.Senior;
                spec.RecommendedShaftWeightGrams = 45;
                spec.FittingNotes.Add("Mjukare skaft (Senior) och låg vikt för att maximera klubbhastighet.");
            }

            else if (player.SwingSpeedMph < 90)
            {
                spec.RecommendedFlex = Shaftflex.Regular;
                spec.RecommendedShaftWeightGrams = 55;
                spec.FittingNotes.Add("Regular-flex rekommenderas för kontrollerad hastighet och bra känsla.");
            }

            else if (player.SwingSpeedMph < 105)
            {
                spec.RecommendedFlex = Shaftflex.Stiff;
                spec.RecommendedShaftWeightGrams = 60;
                spec.FittingNotes.Add("Stiff-flex rekommednderas för kontrollerad hastighet mellan 90 och 105 mph");
            }
            else
            {
                spec.RecommendedFlex = Shaftflex.ExtraStiff;
                spec.RecommendedShaftWeightGrams = 70;
                spec.FittingNotes.Add("Extra Stiff och tyngre skaft för att motverka spridning vid hög klubbhastighet.");
            }

            if (player.WristToFloorCm < 93)
            {
                spec.LengthAdjustmentInches = 0.5;
                spec.LieAdjustmentDegrees = 1.0; // Upright
                spec.FittingNotes.Add("Förlängt skaft (+0.5\") och +1° lie på grund av långt avstånd handled-golv.");
            }
            else if (player.WristToFloorCm < 82)
            {
                spec.LengthAdjustmentInches = -0.5;
                spec.LieAdjustmentDegrees = -1.0; // Flat
                spec.FittingNotes.Add("Förkortat skaft (-0.5\") och -1° flat lie för att undvika hälträffar.");
            }
            else
            {
                spec.LengthAdjustmentInches = 0.0;
                spec.LieAdjustmentDegrees = 0.0;
                spec.FittingNotes.Add("Standardlängd och standard lie matchar kroppsmåtten väl.");
            }
            // 3. Beslutsträd: Loftjustering baserat på miss och launch preference
            double baseLoft = 10.5;

            if (player.LaunchPreference == TrajectoryNeed.NeedsHigherLaunch || player.SwingSpeedMph < 85)
            {
                baseLoft = 12.0;
                spec.FittingNotes.Add("Högre loft (12°) valt för att hjälpa bollen upp i luften och öka carry.");
            }
            else if (player.LaunchPreference == TrajectoryNeed.NeedsLowerLaunch && player.SwingSpeedMph > 100)
            {
                baseLoft = 9.0;
                spec.FittingNotes.Add("Lägre loft (9°) valt för att optimera penetrerande bollbana.");
            }

            spec.RecommendedLoftDegrees = baseLoft;

            // 4. Huvudtyp baserat på handicap och sidospridning
            if (player.Handicap > 18 || player.TypicalMiss == BallFlightTendency.BigSlice)
            {
                spec.HeadCategory = "Max Game Improvement (Draw-bias)";
                spec.FittingNotes.Add("Förlåtande huvud med draw-viktning för att motverka högerskruv.");
            }
            else if (player.Handicap <= 6)
            {
                spec.HeadCategory = "Players / Low-Spin";
                spec.FittingNotes.Add("Kompakt huvud med låg spinn för maximal formbarhet och precision.");
            }
            else
            {
                spec.HeadCategory = "Game Improvement (Neutral)";
                spec.FittingNotes.Add("Allround-huvud med bra balans mellan förlåtande egenskaper och längd.");
            }

            // 5. Greppstorlek baserat på handlängd
            if (player.HandLengthCm > 20.0)
            {
                spec.GripSize = "Midsize";
            }
            else if (player.HandLengthCm < 17.5)
            {
                spec.GripSize = "Undersize";
            }
            else
            {
                spec.GripSize = "Standard";
            }

            return spec;
        }
    }
}
