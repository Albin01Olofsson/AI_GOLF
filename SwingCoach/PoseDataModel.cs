using System.Text.Json.Serialization;

namespace SwingCoach
{
    public class LandmarkPoint
    {
        [JsonPropertyName("x")]
        public float X { get; set; }

        [JsonPropertyName("y")]
        public float Y { get; set; }

        [JsonPropertyName("z")]
        public float Z { get; set; }
    }

    public class SwingFrameData
    {
        [JsonPropertyName("frameIndex")]
        public int FrameIndex { get; set; }

        [JsonPropertyName("timestampSeconds")]
        public double TimestampSeconds { get; set; }

        [JsonPropertyName("leftShoulder")]
        public LandmarkPoint LeftShoulder { get; set; } = new();

        [JsonPropertyName("rightShoulder")]
        public LandmarkPoint RightShoulder { get; set; } = new();

        [JsonPropertyName("leftElbow")]
        public LandmarkPoint LeftElbow { get; set; } = new();

        [JsonPropertyName("rightElbow")]
        public LandmarkPoint RightElbow { get; set; } = new();

        [JsonPropertyName("leftWrist")]
        public LandmarkPoint LeftWrist { get; set; } = new();

        [JsonPropertyName("rightWrist")]
        public LandmarkPoint RightWrist { get; set; } = new();

        [JsonPropertyName("leftHip")]
        public LandmarkPoint LeftHip { get; set; } = new();

        [JsonPropertyName("rightHip")]
        public LandmarkPoint RightHip { get; set; } = new();

        [JsonPropertyName("leftKnee")]
        public LandmarkPoint? LeftKnee { get; set; }

        [JsonPropertyName("rightKnee")]
        public LandmarkPoint? RightKnee { get; set; }
    }
}
