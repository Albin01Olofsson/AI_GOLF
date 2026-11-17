namespace SwingCoach
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Golf Vision AI Coach ===");

            // 1. Ange sökvägen till din videofil
            Console.Write("Ange hela sökvägen till svingvideon (.mp4): ");
            string? videoPath = Console.ReadLine()?.Trim('"', ' ');

            if (string.IsNullOrWhiteSpace(videoPath) || !File.Exists(videoPath))
            {
                Console.WriteLine("Kunde inte hitta filen. Kontrollera sökvägen och försök igen.");
                return;
            }

            Console.WriteLine("\nSkickar video till lokal Python Vision-tjänst...");

            try
            {
                // 2. Skicka videon till Python-mikrotjänsten via HTTP
                var apiClient = new PoseApiClient();
                var frames = await apiClient.AnalyzeVideoAsync(videoPath);

                Console.WriteLine($"Klar! Tog emot {frames.Count} analyserade bildrutor från MediaPipe.");

                // 3. Skicka koordinaterna till kinematikmotorn
                var analysis = SwingKinematics.AnalyzeTempo(frames);

                // 4. Presentera resultatet
                Console.WriteLine("\n--- RESULTAT ---");
                Console.WriteLine($"Baksving:  {analysis.BackswingTimeSeconds} sekunder");
                Console.WriteLine($"Nedsving:  {analysis.DownswingTimeSeconds} sekunder");
                Console.WriteLine($"Temporatio: {analysis.TempoRatio}:1 (Mål: ca 3.0:1)");
                Console.WriteLine($"\nCoach-råd: {analysis.CoachFeedback}");
                Console.WriteLine($"Bildrutor (Frames): Start: {analysis.TakeawayFrame} -> Topp: {analysis.TopFrame} -> Träff: {analysis.ImpactFrame}");
                Console.WriteLine("\n--- Biomekanik (Down-the-Line) ---");
                Console.WriteLine($"Ryggradsvinkel adressering : {analysis.AddressSpineAngle}°");
                Console.WriteLine($"Ryggradsvinkel impact      : {analysis.ImpactSpineAngle}°");
                Console.WriteLine($"Förändring                : {analysis.SpineAngleDifference:+#.0;-#.0;0.0}°");

                if (analysis.HasEarlyExtension)
                {
                    Console.WriteLine("VARNING: Early Extension upptäckt! Du rätar upp kroppen mot bollen vid träff.");
                }
                else
                {
                    Console.WriteLine("Bra jobbat! Du behåller din framåtlutning genom träffögonblicket.");
                }

                if (analysis.HasOverTheTop)
                {
                    Console.WriteLine("VARNING: Over-the-Top upptäckt! Klubban kommer utifrån och in.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nEtt fel uppstod: {ex.Message}");
                Console.WriteLine("Kontrollera att terminalen med Uvicorn körs i bakgrunden!");
            }

            Console.WriteLine("\nTryck på valfri tangent för att avsluta...");
            Console.ReadKey();
        }
    }
}
