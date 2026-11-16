namespace SwingCoach
{
    public class SwingAnalysisResult
    {
        public double BackswingTimeSeconds { get; set; }
        public double DownswingTimeSeconds { get; set; }
        public double TempoRatio { get; set; }
        public int TakeawayFrame { get; set; }
        public int TopFrame { get; set; }
        public int ImpactFrame { get; set; }
        public double AddressSpineAngle { get; set; }
        public double ImpactSpineAngle { get; set; }
        public double SpineAngleDifference => AddressSpineAngle - ImpactSpineAngle;
        public bool HasEarlyExtension => SpineAngleDifference > 6.0; // Tappat mer än 6 graders framåtlutning
        public string CoachFeedback { get; set; } = string.Empty;

    }

    public static class SwingKinematics
    {
        public static SwingAnalysisResult AnalyzeTempo(List<SwingFrameData> frames)
        {
            if (frames == null || frames.Count < 30)
            {
                throw new ArgumentException("För få bildrutor för att analysera svingen.");
            }

            // 1. Toppen av baksvingen: Högsta punkten i bilden (minsta Y)
            var topFrame = frames
                .OrderBy(f => (f.LeftWrist.Y + f.RightWrist.Y) / 2.0f)
                .First();

            int topIndex = frames.IndexOf(topFrame);

            // 2. Ta fram referenshöjd vid adressering (genomsnitt av tidiga bildrutor)
            int baselineCount = Math.Max(5, Math.Min(20, topIndex / 3));
            float addressWristY = frames.Take(baselineCount)
                                        .Average(f => (f.LeftWrist.Y + f.RightWrist.Y) / 2.0f);

            // 3. Takeaway: Identifiera var rörelsen startar från adresseringen
            // Vi letar efter den sista bildrutan före toppen där händerna fortfarande var i vila vid adresseringen
            int takeawayIndex = 0;
            const float startMoveThreshold = 0.015f; // Känsligare tröskel så starten inte missas

            for (int i = 0; i < topIndex; i++)
            {
                float currentY = (frames[i].LeftWrist.Y + frames[i].RightWrist.Y) / 2.0f;
                if (Math.Abs(currentY - addressWristY) > startMoveThreshold)
                {
                    // Ta bildrutan precis innan rörelsen tog fart
                    takeawayIndex = Math.Max(0, i - 1);
                    break;
                }
            }

            // 4. Impact: I en golfsving återvänder händerna till adressringens höjd vid träffen
            // Vi söker efter toppen framåt till den punkt där händernas Y-värde skär adressringens Y-värde
            int impactIndex = topIndex;
            for (int i = topIndex; i < frames.Count; i++)
            {
                float currentY = (frames[i].LeftWrist.Y + frames[i].RightWrist.Y) / 2.0f;
                if (currentY >= addressWristY)
                {
                    impactIndex = i;
                    break;
                }
            }

            // Fallback om bollen träffades något högre än adresseringen
            if (impactIndex == topIndex)
            {
                impactIndex = Math.Min(frames.Count - 1, topIndex + 10);
            }

            var takeawayFrame = frames[takeawayIndex];
            var impactFrame = frames[impactIndex];

            // 5. Tidsberäkningar
            double backswingDuration = topFrame.TimestampSeconds - takeawayFrame.TimestampSeconds;
            double downswingDuration = impactFrame.TimestampSeconds - topFrame.TimestampSeconds;

            double ratio = downswingDuration > 0 ? (backswingDuration / downswingDuration) : 0;

            string feedback;
            if (ratio < 2.4)
            {
                feedback = "Snabb eller ryckig baksving i relation till nedsvingen. Sträva efter en mjukare övergång i toppen.";
            }
            else if (ratio > 3.6)
            {
                feedback = "Långsam baksving i relation till nedsvingen. Ger bra kontroll, men kan begränsa svinghastigheten.";
            }
            else
            {
                feedback = "Mycket bra svingtempo! Ligger nära tourens standard (cirka 3:1).";
            }
            // --- Biomekanisk analys: Ryggradsvinkel (DTL) ---
            // I DTL-vy (bakifrån) använder vi höger sida för en högerspelare, 
            // med fallback till vänster sida om höger skulle sakna data.
            var addrShoulder = GetSmoothedLandmark(frames, takeawayIndex, f => f.RightShoulder ?? f.LeftShoulder);
            var addrHip = GetSmoothedLandmark(frames, takeawayIndex, f => f.RightHip ?? f.LeftHip);


            var impShoulder = GetSmoothedLandmark(frames, impactIndex, f => f.RightShoulder ?? f.LeftShoulder);
            var impHip = GetSmoothedLandmark(frames, impactIndex, f => f.RightHip ?? f.LeftHip);


            double addrAngle = CalculateSpineTilt(addrShoulder, addrHip);
            double impAngle = CalculateSpineTilt(impShoulder, impHip);



            return new SwingAnalysisResult
            {
                BackswingTimeSeconds = Math.Round(backswingDuration, 2),
                DownswingTimeSeconds = Math.Round(downswingDuration, 2),
                TempoRatio = Math.Round(ratio, 2),
                TakeawayFrame = takeawayFrame.FrameIndex,
                TopFrame = topFrame.FrameIndex,
                ImpactFrame = impactFrame.FrameIndex,
                CoachFeedback = feedback,
                AddressSpineAngle = addrAngle,
                ImpactSpineAngle = impAngle
            };



        }
        /// <summary>
        /// Beräknar vinkeln i grader vid punkt B mellan linjerna AB och BC (2D-vektorer).
        /// </summary>
        private static double CalculateAngle(LandmarkPoint a, LandmarkPoint b, LandmarkPoint c)
        {
            // Vektor BA (från B mot A) och Vektor BC (från B mot C)
            double v1x = a.X - b.X;
            double v1y = a.Y - b.Y;
            double v2x = c.X - b.X;
            double v2y = c.Y - b.Y;

            // Skalärprodukt (dot product)
            double dot = (v1x * v2x) + (v1y * v2y);

            // Vektorlängder (magnituder)
            double mag1 = Math.Sqrt(v1x * v1x + v1y * v1y);
            double mag2 = Math.Sqrt(v2x * v2x + v2y * v2y);

            if (mag1 == 0 || mag2 == 0) return 0.0;

            // Beräkna cosinus för vinkeln och begränsa till [-1, 1] för avrundningsfel
            double cosTheta = Math.Clamp(dot / (mag1 * mag2), -1.0, 1.0);

            // Räkna om radianer till grader
            return Math.Acos(cosTheta) * (180.0 / Math.PI);
        }
        /// <summary>
        /// Beräknar överkroppens framåtlutning i grader relativt vertikalen (lodlinjen).
        /// 0° = står spikrakt upp, 45° = lutar framåt 45 grader.
        /// </summary>
        private static double CalculateSpineTilt(LandmarkPoint shoulder, LandmarkPoint hip)
        {
            double dx = shoulder.X - hip.X;
            // Y är inverterat i bildkoordinater (ökar nedåt)
            double dy = hip.Y - shoulder.Y;

            // Vinkel mot vertikalen (dy)
            double radians = Math.Atan2(Math.Abs(dx), dy);
            return Math.Round(radians * (180.0 / Math.PI), 1);
        }
        /// <summary>
        /// Jämnar ut koordinaterna över 3 bildrutor runt centerIndex för att filtrera bort rörelseoskärpa och spikar.
        /// </summary>
        private static LandmarkPoint GetSmoothedLandmark(List<SwingFrameData> frames, int centerIndex, Func<SwingFrameData, LandmarkPoint?> selector)
        {
            var validPoints = new List<LandmarkPoint>();
            int start = Math.Max(0, centerIndex - 1);
            int end = Math.Min(frames.Count - 1, centerIndex + 1);

            for (int i = start; i <= end; i++)
            {
                var pt = selector(frames[i]);
                if (pt != null)
                {
                    validPoints.Add(pt);
                }
            }

            if (validPoints.Count == 0)
            {
                return new LandmarkPoint { X = 0, Y = 0, Z = 0 };
            }

            return new LandmarkPoint
            {
                X = validPoints.Average(p => p.X),
                Y = validPoints.Average(p => p.Y),
                Z = validPoints.Average(p => p.Z)
            };
        }
    }
}