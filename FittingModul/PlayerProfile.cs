namespace FittingModul
{
    public class PlayerProfile
    {




        public double HeightCm { get; set; }              // T.ex. 182.0
        public double WristToFloorCm { get; set; }       // T.ex. 88.5
        public double HandLengthCm { get; set; }         // För val av grepp

        // Spelarhistorik
        public double Handicap { get; set; }             // T.ex. 14.2

        // Svingegenskaper (från radar eller videoanalys)
        public double SwingSpeedMph { get; set; }        // T.ex. 96.0 med driver
        public double TempoRatio { get; set; }           // Backswing/downswing, t.ex. 3.0
        public BallFlightTendency TypicalMiss { get; set; }
        public TrajectoryNeed LaunchPreference { get; set; }






    }
}
